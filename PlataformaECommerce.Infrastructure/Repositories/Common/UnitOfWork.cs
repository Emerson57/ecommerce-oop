using Microsoft.EntityFrameworkCore.Storage;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Infrastructure.Persistence.Context;

namespace PlataformaECommerce.Infrastructure.Repositories.Common;

/// <summary>
/// Implementa la unidad de trabajo sobre <see cref="ECommerceDbContext"/>.
/// </summary>
/// <remarks>
/// Esta implementación coordina persistencia y control transaccional explícito
/// para los repositorios respaldados por Entity Framework Core.
/// </remarks>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ECommerceDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    /// <summary>
    /// Inicializa una nueva instancia de la unidad de trabajo.
    /// </summary>
    /// <param name="context">Contexto EF Core asociado.</param>
    public UnitOfWork(ECommerceDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
        {
            return;
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            return;
        }

        try
        {
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    /// <inheritdoc />
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            return;
        }

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_currentTransaction is not null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        await _context.DisposeAsync();
    }
}