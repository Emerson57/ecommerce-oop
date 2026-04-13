using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlataformaECommerce.Infrastructure.Persistence.Context;

namespace PlataformaECommerce.Maintenance;

internal sealed class SqlServerMaintenanceCommandLock
{
    private const int LockTimeoutMilliseconds = 30_000;
    private readonly ECommerceDbContext _dbContext;
    private readonly ILogger<SqlServerMaintenanceCommandLock> _logger;

    public SqlServerMaintenanceCommandLock(
        ECommerceDbContext dbContext,
        ILogger<SqlServerMaintenanceCommandLock> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IAsyncDisposable> AcquireAsync(string resource, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(resource))
        {
            throw new ArgumentException("El recurso de lock es obligatorio.", nameof(resource));
        }

        string connectionString = _dbContext.Database.GetConnectionString()
            ?? throw new InvalidOperationException("No se pudo resolver la cadena de conexión SQL Server para adquirir el lock de mantenimiento.");

        SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await using SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = """
                DECLARE @result int;
                EXEC @result = sp_getapplock
                    @Resource = @resource,
                    @LockMode = 'Exclusive',
                    @LockOwner = 'Session',
                    @LockTimeout = @lockTimeout;
                SELECT @result;
                """;
            command.Parameters.Add(new SqlParameter("@resource", SqlDbType.NVarChar, 255) { Value = resource.Trim() });
            command.Parameters.Add(new SqlParameter("@lockTimeout", SqlDbType.Int) { Value = LockTimeoutMilliseconds });

            object? result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            int resultCode = Convert.ToInt32(result, CultureInfo.InvariantCulture);
            if (resultCode < 0)
            {
                throw new InvalidOperationException($"No fue posible adquirir el lock exclusivo de mantenimiento '{resource}'. Código SQL: {resultCode}.");
            }

            _logger.LogInformation("Lock exclusivo de mantenimiento adquirido para el recurso '{Resource}'.", resource);
            return new SqlServerMaintenanceCommandLockHandle(connection, resource.Trim(), _logger);
        }
        catch
        {
            await connection.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    private sealed class SqlServerMaintenanceCommandLockHandle : IAsyncDisposable
    {
        private readonly SqlConnection _connection;
        private readonly string _resource;
        private readonly ILogger _logger;
        private bool _disposed;

        public SqlServerMaintenanceCommandLockHandle(SqlConnection connection, string resource, ILogger logger)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _resource = resource ?? throw new ArgumentNullException(nameof(resource));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                await using SqlCommand command = _connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = "EXEC sp_releaseapplock @Resource = @resource, @LockOwner = 'Session';";
                command.Parameters.Add(new SqlParameter("@resource", SqlDbType.NVarChar, 255) { Value = _resource });
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                _logger.LogInformation("Lock exclusivo de mantenimiento liberado para el recurso '{Resource}'.", _resource);
            }
            finally
            {
                await _connection.DisposeAsync().ConfigureAwait(false);
                _disposed = true;
            }
        }
    }
}
