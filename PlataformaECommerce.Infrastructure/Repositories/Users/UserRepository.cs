using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Domain.Common;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;
using PlataformaECommerce.Infrastructure.Persistence.Context;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Repositories.Users;

/// <summary>
/// Implementa el repositorio de usuarios sobre Entity Framework Core.
/// </summary>
/// <remarks>
/// Esta implementación traduce entre el agregado <see cref="Usuario"/> y su proyección
/// persistente <see cref="UserEntity"/>, manteniendo una separación clara entre la lógica
/// de dominio y los detalles de almacenamiento requeridos por la infraestructura.
/// </remarks>
public sealed class UserRepository : IUserRepository
{
    private const BindingFlags ReflectionFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly string[] AdministrativeRoles = [RolUsuario.Administrador.ToString(), RolUsuario.SuperUsuario.ToString()];
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ECommerceDbContext _context;

    /// <summary>
    /// Inicializa una nueva instancia del repositorio de usuarios.
    /// </summary>
    /// <param name="context">Contexto EF Core asociado.</param>
    public UserRepository(ECommerceDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Usuario>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<UserEntity> entities = await _context.Users
            .AsNoTracking()
            .OrderBy(user => user.Nombre)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToArray();
    }

    /// <inheritdoc />
    public async Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        UserEntity? entity = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

        return entity is null ? null : MapToDomain(entity);
    }

    /// <inheritdoc />
    public async Task<Usuario?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);

        UserEntity? entity = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.CorreoElectronico == email.Value, cancellationToken);

        return entity is null ? null : MapToDomain(entity);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Usuario>> GetByRoleAsync(RolUsuario rol, CancellationToken cancellationToken = default)
    {
        string role = rol.ToString();

        List<UserEntity> entities = await _context.Users
            .AsNoTracking()
            .Where(user => user.Rol == role)
            .OrderBy(user => user.Nombre)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToArray();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Cliente>> GetCustomersAsync(CancellationToken cancellationToken = default)
    {
        List<UserEntity> entities = await _context.Users
            .AsNoTracking()
            .Where(user => user.Rol == RolUsuario.Cliente.ToString())
            .OrderBy(user => user.Nombre)
            .ToListAsync(cancellationToken);

        return entities.Select(entity => (Cliente)MapToDomain(entity)).ToArray();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Administrador>> GetAdministratorsAsync(CancellationToken cancellationToken = default)
    {
        List<UserEntity> entities = await _context.Users
            .AsNoTracking()
            .Where(user => AdministrativeRoles.Contains(user.Rol))
            .OrderBy(user => user.Nombre)
            .ToListAsync(cancellationToken);

        return entities.Select(entity => (Administrador)MapToDomain(entity)).ToArray();
    }

    /// <inheritdoc />
    public async Task<Cliente?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        UserEntity? entity = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == id && user.Rol == RolUsuario.Cliente.ToString(), cancellationToken);

        return entity is null ? null : (Cliente)MapToDomain(entity);
    }

    /// <inheritdoc />
    public async Task<Administrador?> GetAdministratorByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        UserEntity? entity = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == id && AdministrativeRoles.Contains(user.Rol), cancellationToken);

        return entity is null ? null : (Administrador)MapToDomain(entity);
    }

    /// <inheritdoc />
    public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return id == Guid.Empty
            ? Task.FromResult(false)
            : _context.Users.AnyAsync(user => user.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);

        return _context.Users.AnyAsync(user => user.CorreoElectronico == email.Value, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> ExistsByRoleAsync(RolUsuario rol, CancellationToken cancellationToken = default)
    {
        string role = rol.ToString();
        return _context.Users.AnyAsync(user => user.Rol == role, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        UserEntity entity = MapToEntity(usuario);
        await _context.Users.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        UserEntity? entity = await _context.Users
            .FirstOrDefaultAsync(current => current.Id == usuario.Id, cancellationToken);

        if (entity is null)
        {
            throw new InvalidOperationException($"No se encontró el usuario con identificador '{usuario.Id}' para actualizar.");
        }

        UpdateEntityFromDomain(entity, usuario);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return;
        }

        UserEntity? entity = await _context.Users
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

        if (entity is null)
        {
            return;
        }

        _context.Users.Remove(entity);
    }

    private static Usuario MapToDomain(UserEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (!Enum.TryParse(entity.Rol, ignoreCase: true, out RolUsuario role))
        {
            throw new InvalidOperationException($"El rol persistido '{entity.Rol}' no está soportado por el dominio.");
        }

        return role switch
        {
            RolUsuario.Cliente => MapToCustomer(entity),
            RolUsuario.Administrador => MapToAdministrator(entity, role),
            RolUsuario.SuperUsuario => MapToAdministrator(entity, role),
            _ => throw new InvalidOperationException($"El rol persistido '{entity.Rol}' no está soportado por la infraestructura.")
        };
    }

    private static Cliente MapToCustomer(UserEntity entity)
    {
        Cliente customer = new(entity.Nombre, new Email(entity.CorreoElectronico), entity.ContrasenaHash);
        ApplyBasePersistenceState(customer, entity);
        ApplyCustomerPersistenceState(customer, entity);
        return customer;
    }

    private static Administrador MapToAdministrator(UserEntity entity, RolUsuario role)
    {
        if (string.IsNullOrWhiteSpace(entity.Area))
        {
            throw new InvalidOperationException($"La cuenta administrativa '{entity.CorreoElectronico}' no tiene un área persistida válida.");
        }

        Administrador admin = new(entity.Nombre, new Email(entity.CorreoElectronico), entity.ContrasenaHash, entity.Area, role);
        ApplyBasePersistenceState(admin, entity);
        return admin;
    }

    private static UserEntity MapToEntity(Usuario usuario)
    {
        UserEntity entity = new();
        UpdateEntityFromDomain(entity, usuario);
        return entity;
    }

    private static void UpdateEntityFromDomain(UserEntity entity, Usuario usuario)
    {
        entity.Id = usuario.Id;
        entity.Nombre = usuario.Nombre;
        entity.CorreoElectronico = usuario.CorreoElectronico.Value;
        entity.ContrasenaHash = usuario.ContrasenaHash;
        entity.Rol = usuario.Rol.ToString();
        entity.Activo = usuario.Activo;
        entity.CorreoConfirmado = usuario.CorreoConfirmado;
        entity.FechaCreacionUtc = usuario.FechaCreacionUtc;
        entity.FechaActualizacionUtc = usuario.FechaActualizacionUtc;
        entity.FechaUltimoAccesoUtc = usuario.FechaUltimoAccesoUtc;
        entity.Area = null;
        entity.HistorialComprasSerializado = null;
        entity.PreferenciasSerializadas = null;

        if (usuario is Cliente customer)
        {
            entity.HistorialComprasSerializado = SerializePurchaseHistory(customer.HistorialCompras);
            entity.PreferenciasSerializadas = SerializePreferences(customer.Preferencias);
            return;
        }

        if (usuario is Administrador admin)
        {
            entity.Area = admin.Area;
        }
    }

    private static void ApplyBasePersistenceState(Usuario usuario, UserEntity entity)
    {
        SetPropertyValue(typeof(AggregateRoot), usuario, nameof(AggregateRoot.Id), entity.Id);
        SetPropertyValue(typeof(AggregateRoot), usuario, nameof(AggregateRoot.FechaCreacionUtc), entity.FechaCreacionUtc);
        SetPropertyValue(typeof(AggregateRoot), usuario, nameof(AggregateRoot.FechaActualizacionUtc), entity.FechaActualizacionUtc);
        SetPropertyValue(typeof(Usuario), usuario, nameof(Usuario.Activo), entity.Activo);
        SetPropertyValue(typeof(Usuario), usuario, nameof(Usuario.CorreoConfirmado), entity.CorreoConfirmado);
        SetPropertyValue(typeof(Usuario), usuario, nameof(Usuario.FechaUltimoAccesoUtc), entity.FechaUltimoAccesoUtc);
    }

    private static void ApplyCustomerPersistenceState(Cliente customer, UserEntity entity)
    {
        List<Guid> purchaseHistory = GetFieldValue<List<Guid>>(customer, "_historialCompras");
        purchaseHistory.Clear();
        purchaseHistory.AddRange(DeserializePurchaseHistory(entity.HistorialComprasSerializado));

        HashSet<string> preferences = GetFieldValue<HashSet<string>>(customer, "_preferencias");
        preferences.Clear();

        foreach (string preference in DeserializePreferences(entity.PreferenciasSerializadas))
        {
            preferences.Add(preference);
        }
    }

    private static TField GetFieldValue<TField>(object instance, string fieldName)
        where TField : class
    {
        FieldInfo? field = instance.GetType().GetField(fieldName, ReflectionFlags);
        object? value = field?.GetValue(instance);

        return value as TField
            ?? throw new InvalidOperationException($"No se pudo acceder al campo '{fieldName}' durante la rehidratación del agregado.");
    }

    private static void SetPropertyValue(Type declaringType, object instance, string propertyName, object? value)
    {
        PropertyInfo? property = declaringType.GetProperty(propertyName, ReflectionFlags);

        if (property is null)
        {
            throw new InvalidOperationException($"No se pudo acceder a la propiedad '{propertyName}' durante la rehidratación del agregado.");
        }

        property.SetValue(instance, value);
    }

    private static IReadOnlyCollection<Guid> DeserializePurchaseHistory(string? serializedHistory)
    {
        if (string.IsNullOrWhiteSpace(serializedHistory))
        {
            return Array.Empty<Guid>();
        }

        Guid[]? values = JsonSerializer.Deserialize<Guid[]>(serializedHistory, JsonOptions);
        return values is null || values.Length == 0
            ? Array.Empty<Guid>()
            : values;
    }

    private static string? SerializePurchaseHistory(IEnumerable<Guid> purchaseHistory)
    {
        Guid[] values = purchaseHistory
            .Where(purchaseId => purchaseId != Guid.Empty)
            .Distinct()
            .ToArray();

        return values.Length == 0
            ? null
            : JsonSerializer.Serialize(values, JsonOptions);
    }

    private static IReadOnlyCollection<string> DeserializePreferences(string? serializedPreferences)
    {
        if (string.IsNullOrWhiteSpace(serializedPreferences))
        {
            return Array.Empty<string>();
        }

        string[]? values = JsonSerializer.Deserialize<string[]>(serializedPreferences, JsonOptions);
        if (values is null || values.Length == 0)
        {
            return Array.Empty<string>();
        }

        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string? SerializePreferences(IEnumerable<string> preferences)
    {
        string[] values = preferences
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value)
            .ToArray();

        return values.Length == 0
            ? null
            : JsonSerializer.Serialize(values, JsonOptions);
    }
}
