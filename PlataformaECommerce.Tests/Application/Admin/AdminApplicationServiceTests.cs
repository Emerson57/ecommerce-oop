using PlataformaECommerce.Application.Features.Admin.Commands;
using PlataformaECommerce.Application.Common.Results;
using PlataformaECommerce.Application.Features.Admin.DTOs;
using PlataformaECommerce.Application.Features.Admin.Queries;
using PlataformaECommerce.Application.Features.Admin.Services;
using PlataformaECommerce.Application.Features.Admin.Validators;
using PlataformaECommerce.Application.Interfaces.Persistence;
using PlataformaECommerce.Application.Interfaces.Repositories.Audit;
using PlataformaECommerce.Application.Interfaces.Repositories.Cart;
using PlataformaECommerce.Application.Interfaces.Repositories.Orders;
using PlataformaECommerce.Application.Interfaces.Repositories.Products;
using PlataformaECommerce.Application.Interfaces.Repositories.Users;
using PlataformaECommerce.Application.Interfaces.Services.Audit;
using PlataformaECommerce.Application.Interfaces.Services.Auth;
using PlataformaECommerce.Application.Interfaces.Services.Common;
using PlataformaECommerce.Domain.Entities.Cart;
using PlataformaECommerce.Domain.Entities.Orders;
using PlataformaECommerce.Domain.Entities.Products;
using PlataformaECommerce.Domain.Entities.Users;
using PlataformaECommerce.Domain.Enums;
using PlataformaECommerce.Domain.ValueObjects;

namespace PlataformaECommerce.Tests.Application.Admin;

[TestFixture]
public class AdminApplicationServiceTests
{
    [Test]
    public async Task RegisterAdminAsync_OperacionExitosa_RegistraEventoDeAuditoria()
    {
        FakeUserRepository userRepository = new();
        Administrador superUserActor = CreateSuperUserActor();
        await userRepository.AddAsync(superUserActor);
        FakeAuditTrailService auditTrailService = new();
        AdminApplicationService service = new(
            new FakeProductRepository(),
            new FakeOrderRepository(),
            userRepository,
            new FakeCartRepository(),
            new FakeAuditRepository(),
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            auditTrailService,
            new FakeCurrentUserService(userId: superUserActor.Id, role: RolUsuario.SuperUsuario.ToString()),
            new RegisterAdminCommandValidator());

        await service.RegisterAdminAsync(new RegisterAdminCommand
        {
            Name = "Admin Demo",
            Email = "admin@plataforma.com",
            Password = "Password#2026",
            ConfirmPassword = "Password#2026",
            Area = "Operaciones",
            IsActive = true,
            IsEmailConfirmed = true
        });

        Assert.That(auditTrailService.RegisteredEvents.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task RegisterAdminAsync_OperacionExitosa_AuditaMetadataSinDatosSensiblesYConRolCreadorCorrecto()
    {
        FakeUserRepository userRepository = new();
        Administrador superUserActor = CreateSuperUserActor();
        await userRepository.AddAsync(superUserActor);
        FakeAuditTrailService auditTrailService = new();
        AdminApplicationService service = new(
            new FakeProductRepository(),
            new FakeOrderRepository(),
            userRepository,
            new FakeCartRepository(),
            new FakeAuditRepository(),
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            auditTrailService,
            new FakeCurrentUserService(userId: superUserActor.Id, role: null),
            new RegisterAdminCommandValidator());

        await service.RegisterAdminAsync(new RegisterAdminCommand
        {
            Name = "Admin Demo",
            Email = "admin.audit@plataforma.com",
            Password = "Password#2026",
            ConfirmPassword = "Password#2026",
            Area = "Operaciones",
            Source = "AdminPortal",
            Reason = "Alta operativa"
        });

        Assert.That(auditTrailService.LastMetadata, Is.Not.Null);
        Assert.That(auditTrailService.LastMetadata!["createdByRole"], Is.EqualTo(RolUsuario.SuperUsuario.ToString()));
        Assert.That(auditTrailService.LastMetadata["source"], Is.EqualTo("AdminPortal"));
        Assert.That(auditTrailService.LastMetadata.ContainsKey("password"), Is.False);
        Assert.That(auditTrailService.LastMetadata.ContainsKey("passwordHash"), Is.False);
    }

    [Test]
    public async Task RegisterAdminAsync_AdministradorSinPrivilegiosElevados_RetornaErrorDeAutorizacion()
    {
        FakeUserRepository userRepository = new();
        Administrador administratorActor = new("Admin Actor", new Email("admin.actor@plataforma.com"), "hash-admin-actor-2026", "Operaciones");
        administratorActor.ConfirmarCorreoElectronico();
        await userRepository.AddAsync(administratorActor);

        AdminApplicationService service = new(
            new FakeProductRepository(),
            new FakeOrderRepository(),
            userRepository,
            new FakeCartRepository(),
            new FakeAuditRepository(),
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            new FakeAuditTrailService(),
            new FakeCurrentUserService(userId: administratorActor.Id, role: RolUsuario.Administrador.ToString()),
            new RegisterAdminCommandValidator());

        Result<AdminDto> result = await service.RegisterAdminAsync(new RegisterAdminCommand
        {
            Name = "Admin Secundario",
            Email = "admin-secundario@plataforma.com",
            Password = "Password#2026",
            ConfirmPassword = "Password#2026",
            Area = "Operaciones"
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo("Admin.SuperUserRequired"));
    }

    [Test]
    public async Task RegisterAdminAsync_EmailDuplicado_RetornaConflicto()
    {
        FakeUserRepository userRepository = new();
        Administrador superUserActor = CreateSuperUserActor();
        await userRepository.AddAsync(superUserActor);
        await userRepository.AddAsync(new Administrador(
            "Admin Existente",
            new Email("admin.existente@plataforma.com"),
            "hash-admin-existente-2026",
            "Operaciones"));

        AdminApplicationService service = new(
            new FakeProductRepository(),
            new FakeOrderRepository(),
            userRepository,
            new FakeCartRepository(),
            new FakeAuditRepository(),
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            new FakeAuditTrailService(),
            new FakeCurrentUserService(userId: superUserActor.Id, role: RolUsuario.SuperUsuario.ToString()),
            new RegisterAdminCommandValidator());

        Result<AdminDto> result = await service.RegisterAdminAsync(new RegisterAdminCommand
        {
            Name = "Admin Duplicado",
            Email = "admin.existente@plataforma.com",
            Password = "Password#2026",
            ConfirmPassword = "Password#2026",
            Area = "Operaciones"
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo("Admin.EmailAlreadyExists"));
    }

    [Test]
    public async Task RegisterAdminAsync_BootstrapInicial_CreaSuperUsuario()
    {
        FakeUserRepository userRepository = new();
        AdminApplicationService service = new(
            new FakeProductRepository(),
            new FakeOrderRepository(),
            userRepository,
            new FakeCartRepository(),
            new FakeAuditRepository(),
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            new FakeAuditTrailService(),
            new FakeCurrentUserService(isAuthenticated: false, role: null),
            new RegisterAdminCommandValidator());

        Result<AdminDto> result = await service.RegisterAdminAsync(new RegisterAdminCommand
        {
            Name = "Root Platform",
            Email = "root@plataforma.com",
            Password = "Password#2026",
            ConfirmPassword = "Password#2026",
            Area = "Plataforma",
            Role = RolUsuario.SuperUsuario,
            IsActive = true,
            IsEmailConfirmed = true,
            IsBootstrap = true
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Role, Is.EqualTo(RolUsuario.SuperUsuario));
    }

    [Test]
    public async Task RegisterAdminAsync_ErrorDePersistencia_RetornaFalloControlado()
    {
        FakeUserRepository userRepository = new();
        Administrador superUserActor = CreateSuperUserActor();
        await userRepository.AddAsync(superUserActor);
        userRepository.ThrowOnAdd = true;

        AdminApplicationService service = new(
            new FakeProductRepository(),
            new FakeOrderRepository(),
            userRepository,
            new FakeCartRepository(),
            new FakeAuditRepository(),
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            new FakeAuditTrailService(),
            new FakeCurrentUserService(userId: superUserActor.Id, role: RolUsuario.SuperUsuario.ToString()),
            new RegisterAdminCommandValidator());

        Result<AdminDto> result = await service.RegisterAdminAsync(new RegisterAdminCommand
        {
            Name = "Admin Persistencia",
            Email = "admin.persistencia@plataforma.com",
            Password = "Password#2026",
            ConfirmPassword = "Password#2026",
            Area = "Operaciones"
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo("Admin.Persistence"));
    }

    [Test]
    public async Task RegisterAdminAsync_BootstrapConAdministradorExistenteYPersistenciaSinSuperUsuario_CreaSuperUsuario()
    {
        FakeUserRepository userRepository = new();
        Administrador administrator = new("Admin Demo", new Email("admin.existente@plataforma.com"), "hash-admin-seguro-2026", "Operaciones");
        administrator.ConfirmarCorreoElectronico();
        await userRepository.AddAsync(administrator);

        AdminApplicationService service = new(
            new FakeProductRepository(),
            new FakeOrderRepository(),
            userRepository,
            new FakeCartRepository(),
            new FakeAuditRepository(),
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            new FakeAuditTrailService(),
            new FakeCurrentUserService(isAuthenticated: false, role: null),
            new RegisterAdminCommandValidator());

        Result<AdminDto> result = await service.RegisterAdminAsync(new RegisterAdminCommand
        {
            Name = "Root Platform",
            Email = "root-bootstrap@plataforma.com",
            Password = "Password#2026",
            ConfirmPassword = "Password#2026",
            Area = "Plataforma",
            Role = RolUsuario.SuperUsuario,
            IsActive = true,
            IsEmailConfirmed = true,
            IsBootstrap = true
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Role, Is.EqualTo(RolUsuario.SuperUsuario));
    }

    [Test]
    public async Task RegisterAdminAsync_SolicitudNoBootstrapConRolSuperUsuario_RetornaErrorDeValidacion()
    {
        FakeUserRepository userRepository = new();
        Administrador superUserActor = CreateSuperUserActor();
        await userRepository.AddAsync(superUserActor);

        AdminApplicationService service = new(
            new FakeProductRepository(),
            new FakeOrderRepository(),
            userRepository,
            new FakeCartRepository(),
            new FakeAuditRepository(),
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            new FakeAuditTrailService(),
            new FakeCurrentUserService(userId: superUserActor.Id, role: RolUsuario.SuperUsuario.ToString()),
            new RegisterAdminCommandValidator());

        Result<AdminDto> result = await service.RegisterAdminAsync(new RegisterAdminCommand
        {
            Name = "Admin Inválido",
            Email = "admin-invalido@plataforma.com",
            Password = "Password#2026",
            ConfirmPassword = "Password#2026",
            Area = "Operaciones",
            Role = RolUsuario.SuperUsuario
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo("Admin.Validation"));
    }

    [Test]
    public async Task GetAdminRegistrationDefinitionAsync_SuperUsuarioAutenticado_RetornaDefinicionEsperada()
    {
        FakeUserRepository userRepository = new();
        Administrador superUserActor = CreateSuperUserActor();
        await userRepository.AddAsync(superUserActor);

        AdminApplicationService service = new(
            new FakeProductRepository(),
            new FakeOrderRepository(),
            userRepository,
            new FakeCartRepository(),
            new FakeAuditRepository(),
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            new FakeAuditTrailService(),
            new FakeCurrentUserService(userId: superUserActor.Id, role: RolUsuario.SuperUsuario.ToString()),
            new RegisterAdminCommandValidator());

        Result<AdminRegistrationDefinitionDto> result = await service.GetAdminRegistrationDefinitionAsync(new GetAdminRegistrationDefinitionQuery());

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.AllowedRole, Is.EqualTo(RolUsuario.Administrador));
        Assert.That(result.Value.AllowsSuperUserCreation, Is.False);
    }

    [Test]
    public async Task GetAdminRegistrationDefinitionAsync_AdministradorSinPrivilegiosElevados_RetornaErrorDeAutorizacion()
    {
        FakeUserRepository userRepository = new();
        Administrador administratorActor = new("Admin Actor", new Email("admin.actor@plataforma.com"), "hash-admin-actor-2026", "Operaciones");
        administratorActor.ConfirmarCorreoElectronico();
        await userRepository.AddAsync(administratorActor);

        AdminApplicationService service = new(
            new FakeProductRepository(),
            new FakeOrderRepository(),
            userRepository,
            new FakeCartRepository(),
            new FakeAuditRepository(),
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            new FakeAuditTrailService(),
            new FakeCurrentUserService(userId: administratorActor.Id, role: RolUsuario.Administrador.ToString()),
            new RegisterAdminCommandValidator());

        Result<AdminRegistrationDefinitionDto> result = await service.GetAdminRegistrationDefinitionAsync(new GetAdminRegistrationDefinitionQuery());

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo("Admin.SuperUserRequiredForAdminCreationDefinition"));
    }

    [Test]
    public async Task GetDashboardAsync_ConsultaValida_RetornaResumenAdministrativo()
    {
        FakeUserRepository userRepository = new();
        AdminApplicationService service = new(
            new FakeProductRepository(),
            new FakeOrderRepository(),
            userRepository,
            new FakeCartRepository(),
            new FakeAuditRepository(),
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            new FakeAuditTrailService(),
            new FakeCurrentUserService(),
            new RegisterAdminCommandValidator());

        var result = await service.GetDashboardAsync(new GetAdminDashboardQuery(), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.WindowInDays, Is.EqualTo(30));
    }

    [Test]
    public async Task GetUsersAsync_SuperUsuarioAutenticado_RetornaResumenConSeparacionDeRoles()
    {
        Cliente customer = new("Cliente Demo", new Email("cliente@plataforma.com"), "hash-cliente-seguro-2026");
        customer.ConfirmarCorreoElectronico();

        Administrador administrator = new("Admin Demo", new Email("admin@plataforma.com"), "hash-admin-seguro-2026", "Operaciones");
        administrator.ConfirmarCorreoElectronico();

        Administrador superUser = new("Root Demo", new Email("root@plataforma.com"), "hash-root-seguro-2026", "Plataforma", RolUsuario.SuperUsuario);
        superUser.ConfirmarCorreoElectronico();
        superUser.RegistrarAcceso();

        FakeUserRepository userRepository = new();
        await userRepository.AddAsync(customer);
        await userRepository.AddAsync(administrator);
        await userRepository.AddAsync(superUser);

        AdminApplicationService service = new(
            new FakeProductRepository(),
            new FakeOrderRepository(),
            userRepository,
            new FakeCartRepository(),
            new FakeAuditRepository(),
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            new FakeAuditTrailService(),
            new FakeCurrentUserService(userId: superUser.Id, role: RolUsuario.SuperUsuario.ToString()),
            new RegisterAdminCommandValidator());

        Result<AdminUsersBackofficeDto> result = await service.GetUsersAsync(new GetAdminUsersQuery(), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.TotalUsers, Is.EqualTo(3));
        Assert.That(result.Value.TotalAdministrators, Is.EqualTo(2));
        Assert.That(result.Value.TotalCustomers, Is.EqualTo(1));
        Assert.That(result.Value.TotalSuperUsers, Is.EqualTo(1));
    }

    [Test]
    public async Task ResetUserPasswordAsync_SuperUsuarioAutenticado_RestableceCredencialDeCualquierUsuarioYAuditaLaOperacion()
    {
        FakeUserRepository userRepository = new();
        Administrador superUserActor = CreateSuperUserActor();
        Cliente customer = new("Cliente Reset", new Email("cliente.reset@plataforma.com"), "hash-cliente-original-2026");
        customer.ConfirmarCorreoElectronico();
        await userRepository.AddAsync(superUserActor);
        await userRepository.AddAsync(customer);
        FakeAuditTrailService auditTrailService = new();

        AdminApplicationService service = new(
            new FakeProductRepository(),
            new FakeOrderRepository(),
            userRepository,
            new FakeCartRepository(),
            new FakeAuditRepository(),
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            auditTrailService,
            new FakeCurrentUserService(userId: superUserActor.Id, role: RolUsuario.SuperUsuario.ToString()),
            new RegisterAdminCommandValidator());

        Result<AdminBackofficeUserDto> result = await service.ResetUserPasswordAsync(new ResetUserPasswordCommand
        {
            TargetUserId = customer.Id,
            NewPassword = "Password#2027",
            ConfirmPassword = "Password#2027",
            Source = "AdminPortal",
            Reason = "Soporte operativo"
        });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(customer.ContrasenaHash, Is.EqualTo("hash-Password#2027-seguro-2026"));
        Assert.That(result.Value.IsAdministrative, Is.False);
        Assert.That(auditTrailService.RegisteredEvents, Does.Contain("admin.user-password-reset"));
        Assert.That(auditTrailService.LastMetadata, Is.Not.Null);
        Assert.That(auditTrailService.LastMetadata!["targetEmail"], Is.EqualTo("cliente.reset@plataforma.com"));
        Assert.That(auditTrailService.LastMetadata.ContainsKey("password"), Is.False);
        Assert.That(auditTrailService.LastMetadata.ContainsKey("passwordHash"), Is.False);
    }

    [Test]
    public async Task ResetUserPasswordAsync_AdministradorSinPrivilegiosElevados_RetornaErrorDeAutorizacion()
    {
        FakeUserRepository userRepository = new();
        Administrador administratorActor = new("Admin Actor", new Email("admin.actor.reset@plataforma.com"), "hash-admin-actor-reset-2026", "Operaciones");
        administratorActor.ConfirmarCorreoElectronico();
        Cliente customer = new("Cliente Demo", new Email("cliente.reset-unauthorized@plataforma.com"), "hash-cliente-reset-unauthorized-2026");
        customer.ConfirmarCorreoElectronico();
        await userRepository.AddAsync(administratorActor);
        await userRepository.AddAsync(customer);

        AdminApplicationService service = new(
            new FakeProductRepository(),
            new FakeOrderRepository(),
            userRepository,
            new FakeCartRepository(),
            new FakeAuditRepository(),
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            new FakeAuditTrailService(),
            new FakeCurrentUserService(userId: administratorActor.Id, role: RolUsuario.Administrador.ToString()),
            new RegisterAdminCommandValidator());

        Result<AdminBackofficeUserDto> result = await service.ResetUserPasswordAsync(new ResetUserPasswordCommand
        {
            TargetUserId = customer.Id,
            NewPassword = "Password#2027",
            ConfirmPassword = "Password#2027"
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo("Admin.SuperUserRequiredForUserPasswordReset"));
    }

    [Test]
    public async Task RegisterAdminAsync_ClaimsDeSuperUsuarioSinActorPersistido_RetornaErrorDeAutorizacion()
    {
        AdminApplicationService service = new(
            new FakeProductRepository(),
            new FakeOrderRepository(),
            new FakeUserRepository(),
            new FakeCartRepository(),
            new FakeAuditRepository(),
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            new FakeAuditTrailService(),
            new FakeCurrentUserService(role: RolUsuario.SuperUsuario.ToString()),
            new RegisterAdminCommandValidator());

        Result<AdminDto> result = await service.RegisterAdminAsync(new RegisterAdminCommand
        {
            Name = "Admin Manual",
            Email = "admin.manual@plataforma.com",
            Password = "Password#2026",
            ConfirmPassword = "Password#2026",
            Area = "Operaciones"
        });

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo("Admin.SuperUserRequired"));
    }

    [Test]
    public async Task GetUsersAsync_SoloAdministrativos_ReutilizaElCasoDeUsoSinExponerClientes()
    {
        Cliente customer = new("Cliente Demo", new Email("cliente@plataforma.com"), "hash-cliente-seguro-2026");
        customer.ConfirmarCorreoElectronico();

        Administrador administrator = new("Admin Demo", new Email("admin@plataforma.com"), "hash-admin-seguro-2026", "Operaciones");
        administrator.ConfirmarCorreoElectronico();

        Administrador superUser = new("Root Demo", new Email("root@plataforma.com"), "hash-root-seguro-2026", "Plataforma", RolUsuario.SuperUsuario);
        superUser.ConfirmarCorreoElectronico();

        FakeUserRepository userRepository = new();
        await userRepository.AddAsync(customer);
        await userRepository.AddAsync(administrator);
        await userRepository.AddAsync(superUser);

        AdminApplicationService service = new(
            new FakeProductRepository(),
            new FakeOrderRepository(),
            userRepository,
            new FakeCartRepository(),
            new FakeAuditRepository(),
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            new FakeAuditTrailService(),
            new FakeCurrentUserService(userId: superUser.Id, role: RolUsuario.SuperUsuario.ToString()),
            new RegisterAdminCommandValidator());

        Result<AdminUsersBackofficeDto> result = await service.GetUsersAsync(new GetAdminUsersQuery
        {
            OnlyAdministrativeUsers = true
        }, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.TotalUsers, Is.EqualTo(2));
        Assert.That(result.Value.TotalCustomers, Is.EqualTo(0));
        Assert.That(result.Value.Users.All(user => user.IsAdministrative), Is.True);
        Assert.That(userRepository.GetAdministratorsAsyncCallCount, Is.EqualTo(1));
        Assert.That(userRepository.GetAllAsyncCallCount, Is.EqualTo(0));
    }

    [Test]
    public async Task GetUsersAsync_AdministradorSinPrivilegiosElevados_RetornaErrorDeAutorizacion()
    {
        FakeUserRepository userRepository = new();
        Administrador administratorActor = new("Admin Actor", new Email("admin.actor.users@plataforma.com"), "hash-admin-actor-users-2026", "Operaciones");
        administratorActor.ConfirmarCorreoElectronico();
        await userRepository.AddAsync(administratorActor);

        AdminApplicationService service = new(
            new FakeProductRepository(),
            new FakeOrderRepository(),
            userRepository,
            new FakeCartRepository(),
            new FakeAuditRepository(),
            new FakeUnitOfWork(),
            new FakePasswordHasher(),
            new FakeAuditTrailService(),
            new FakeCurrentUserService(userId: administratorActor.Id, role: RolUsuario.Administrador.ToString()),
            new RegisterAdminCommandValidator());

        Result<AdminUsersBackofficeDto> result = await service.GetUsersAsync(new GetAdminUsersQuery(), CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo("Admin.SuperUserRequiredForUsersBackoffice"));
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        public Task<IReadOnlyCollection<Producto>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Producto>>(Array.Empty<Producto>());
        public Task<Producto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Producto?>(null);
        public Task<Producto?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default) => Task.FromResult<Producto?>(null);
        public Task<IReadOnlyCollection<Producto>> GetActiveProductsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Producto>>(Array.Empty<Producto>());
        public Task<IReadOnlyCollection<Producto>> GetFeaturedProductsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Producto>>(Array.Empty<Producto>());
        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AddAsync(Producto producto, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(Producto producto, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeOrderRepository : IOrderRepository
    {
        public Task<IReadOnlyCollection<Pedido>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Pedido>>(Array.Empty<Pedido>());
        public Task<Pedido?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Pedido?>(null);
        public Task<IReadOnlyCollection<Pedido>> GetByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Pedido>>(Array.Empty<Pedido>());
        public Task<IReadOnlyCollection<Pedido>> GetByStatusAsync(EstadoPedido estado, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Pedido>>(Array.Empty<Pedido>());
        public Task<IReadOnlyCollection<Pedido>> GetByCustomerIdAndStatusAsync(Guid clienteId, EstadoPedido estado, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Pedido>>(Array.Empty<Pedido>());
        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> ExistsByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AddAsync(Pedido pedido, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(Pedido pedido, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeCartRepository : ICartRepository
    {
        public Task<IReadOnlyCollection<CarritoCompra>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<CarritoCompra>>(Array.Empty<CarritoCompra>());
        public Task<CarritoCompra?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<CarritoCompra?>(null);
        public Task<CarritoCompra?> GetByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default) => Task.FromResult<CarritoCompra?>(null);
        public Task<IReadOnlyCollection<CarritoCompra>> GetAllByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<CarritoCompra>>(Array.Empty<CarritoCompra>());
        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> ExistsByCustomerIdAsync(Guid clienteId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AddAsync(CarritoCompra carrito, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(CarritoCompra carrito, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeAuditRepository : IAuditRepository
    {
        public Task RegisterEventAsync(AuditEntry entry, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IReadOnlyCollection<AuditEntry>> GetHistoryAsync(Guid aggregateId, string aggregateType, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<AuditEntry>>(Array.Empty<AuditEntry>());
        public Task<AuditSearchResult> SearchAsync(AuditSearchFilter filter, CancellationToken cancellationToken = default) => Task.FromResult(new AuditSearchResult { Items = Array.Empty<AuditEntry>(), TotalCount = 0, PageNumber = 1, PageSize = filter.PageSize <= 0 ? 25 : filter.PageSize });
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly List<Usuario> _users = new();

        public int GetAllAsyncCallCount { get; private set; }
        public int GetAdministratorsAsyncCallCount { get; private set; }
        public bool ThrowOnAdd { get; set; }

        public Task<IReadOnlyCollection<Usuario>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            GetAllAsyncCallCount++;
            return Task.FromResult<IReadOnlyCollection<Usuario>>(_users.ToArray());
        }

        public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.FirstOrDefault(user => user.Id == id));

        public Task<Usuario?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.FirstOrDefault(user => user.CorreoElectronico.Equals(email)));

        public Task<IReadOnlyCollection<Usuario>> GetByRoleAsync(RolUsuario rol, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Usuario>>(_users.Where(user => user.Rol == rol).ToArray());

        public Task<IReadOnlyCollection<Cliente>> GetCustomersAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Cliente>>(_users.OfType<Cliente>().ToArray());

        public Task<IReadOnlyCollection<Administrador>> GetAdministratorsAsync(CancellationToken cancellationToken = default)
        {
            GetAdministratorsAsyncCallCount++;
            return Task.FromResult<IReadOnlyCollection<Administrador>>(_users.OfType<Administrador>().ToArray());
        }

        public Task<Cliente?> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.OfType<Cliente>().FirstOrDefault(user => user.Id == id));

        public Task<Administrador?> GetAdministratorByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.OfType<Administrador>().FirstOrDefault(user => user.Id == id));

        public Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.Any(user => user.Id == id));

        public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.Any(user => user.CorreoElectronico.Equals(email)));

        public Task<bool> ExistsByRoleAsync(RolUsuario rol, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.Any(user => user.Rol == rol));

        public Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default)
        {
            if (ThrowOnAdd)
            {
                throw new InvalidOperationException("Fallo de persistencia simulado.");
            }

            _users.Add(usuario);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _users.RemoveAll(user => user.Id == id);
            return Task.CompletedTask;
        }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password) => $"hash-{password}-seguro-2026";
        public bool VerifyPassword(string password, string passwordHash) => passwordHash == HashPassword(password);
    }

    private sealed class FakeAuditTrailService : IAuditTrailService
    {
        public List<string> RegisteredEvents { get; } = new();
        public IReadOnlyDictionary<string, string>? LastMetadata { get; private set; }

        public Task RegisterAsync(Guid aggregateId, string aggregateType, string module, string action, string detail, IReadOnlyDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
        {
            RegisteredEvents.Add(action);
            LastMetadata = metadata;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
        public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CommitTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private static Administrador CreateSuperUserActor()
    {
        Administrador superUser = new("Root Actor", new Email($"root-{Guid.NewGuid():N}@plataforma.com"), "hash-root-actor-2026", "Plataforma", RolUsuario.SuperUsuario);
        superUser.ConfirmarCorreoElectronico();
        return superUser;
    }

    private sealed class FakeCurrentUserService : ICurrentUserService
    {
        private readonly Guid? _userId;
        private readonly string? _role;

        public FakeCurrentUserService(Guid? userId = null, bool isAuthenticated = true, string? role = "Administrador")
        {
            _userId = userId ?? Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            IsAuthenticated = isAuthenticated;
            _role = role;
        }

        public Guid? UserId => _userId;
        public string? UserName => "Admin Demo";
        public string? Email => "admin@plataforma.com";
        public string? Role => _role;
        public bool IsAuthenticated { get; }
        public bool IsInRole(string role) => string.Equals(role, Role, StringComparison.OrdinalIgnoreCase);
        public string? GetClaimValue(string claimType) => null;
        public IReadOnlyCollection<string> GetClaimValues(string claimType) => Array.Empty<string>();
    }
}
