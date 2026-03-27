using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using PlataformaECommerce.Application.DependencyInjection;
using PlataformaECommerce.Infrastructure.DependencyInjection;
using PlataformaECommerce.Infrastructure.Persistence.Context;
using PlataformaECommerce.Web.Authorization;
using PlataformaECommerce.Web.Configuration;
using PlataformaECommerce.Web.Initialization;
using PlataformaECommerce.Web.Middlewares;
using PlataformaECommerce.Web.OpenApi;
using PlataformaECommerce.Web.Services.Products;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", AuthorizationPolicies.AdminOnly);
    options.Conventions.AuthorizeFolder("/Admin/Users", AuthorizationPolicies.SuperUserOnly);
});

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errores = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var respuesta = new
            {
                mensaje = "La solicitud contiene errores de validación.",
                errores
            };

            return new BadRequestObjectResult(respuesta);
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(SwaggerGroups.Public, new OpenApiInfo
    {
        Title = "PlataformaECommerce API Pública",
        Version = "v1",
        Description = "Endpoints públicos de consulta del catálogo de productos."
    });

    options.SwaggerDoc(SwaggerGroups.Admin, new OpenApiInfo
    {
        Title = "PlataformaECommerce API Administrativa",
        Version = "v1",
        Description = "Endpoints administrativos protegidos para gestión integral del catálogo."
    });

    options.DocInclusionPredicate((documentName, apiDescription) =>
    {
        string? groupName = apiDescription.GroupName;
        return string.Equals(groupName, documentName, StringComparison.OrdinalIgnoreCase);
    });
});

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services
    .AddOptions<BootstrapSuperUserOptions>()
    .Bind(builder.Configuration.GetSection(BootstrapSuperUserOptions.SectionName))
    .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.Name), "El bootstrap del super usuario requiere un nombre válido.")
    .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.Email), "El bootstrap del super usuario requiere un correo electrónico válido.")
    .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.Password), "El bootstrap del super usuario requiere una contraseña válida.")
    .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.Area), "El bootstrap del super usuario requiere un área válida.")
    .ValidateOnStart();

builder.Services
    .AddOptions<AdminUsersBackofficeOptions>()
    .Bind(builder.Configuration.GetSection(AdminUsersBackofficeOptions.SectionName));

builder.Services
    .AddOptions<ProductImagesOptions>()
    .Bind(builder.Configuration.GetSection(ProductImagesOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.UploadsDirectory), "La configuración de imágenes de productos requiere un directorio de almacenamiento válido.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.RequestPath) && options.RequestPath.StartsWith('/'), "La configuración de imágenes de productos requiere una ruta pública válida que comience con '/'.")
    .Validate(options => options.MaxFileSizeInBytes > 0, "La configuración de imágenes de productos requiere un tamaño máximo de archivo mayor que cero.")
    .Validate(options => options.AllowedExtensions.Count > 0, "La configuración de imágenes de productos requiere al menos una extensión permitida.")
    .ValidateOnStart();

builder.Services.AddScoped<AdminCookieSecurityService>();
builder.Services.AddScoped<AdminCookieAuthenticationEvents>();
builder.Services.AddScoped<CustomerCookieSecurityService>();
builder.Services.AddScoped<CustomerCookieAuthenticationEvents>();
builder.Services.AddScoped<SuperUserBootstrapService>();
builder.Services.AddScoped<IProductImageStorageService, ProductImageStorageService>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = AuthorizationPolicies.AppCookieScheme;
        options.DefaultAuthenticateScheme = AuthorizationPolicies.AppCookieScheme;
        options.DefaultChallengeScheme = AuthorizationPolicies.AppCookieScheme;
        options.DefaultSignOutScheme = AuthorizationPolicies.AppCookieScheme;
    })
    .AddPolicyScheme(AuthorizationPolicies.AppCookieScheme, "Application cookie selector", options =>
    {
        options.ForwardDefaultSelector = AuthorizationPolicies.ResolveApplicationCookieScheme;
    })
    .AddCookie(AuthorizationPolicies.AdminCookieScheme, AuthorizationPolicies.ConfigureAdminCookie)
    .AddCookie(AuthorizationPolicies.CustomerCookieScheme, AuthorizationPolicies.ConfigureCustomerCookie);

builder.Services.AddAuthorization(AuthorizationPolicies.ConfigureBackofficePolicies);

CultureInfo[] supportedCultures =
[
    CultureInfo.GetCultureInfo("es-CO"),
    CultureInfo.GetCultureInfo("es"),
    CultureInfo.GetCultureInfo("en-US")
];

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("es-CO");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.ApplyCurrentCultureToResponseHeaders = true;
    options.FallBackToParentCultures = true;
    options.FallBackToParentUICultures = true;
});

var app = builder.Build();

await using (AsyncServiceScope scope = app.Services.CreateAsyncScope())
{
    ECommerceDbContext dbContext = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
    await dbContext.Database.MigrateAsync();

    SuperUserBootstrapService bootstrapService = scope.ServiceProvider.GetRequiredService<SuperUserBootstrapService>();
    await bootstrapService.BootstrapAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint($"/swagger/{SwaggerGroups.Public}/swagger.json", "API Pública v1");
        options.SwaggerEndpoint($"/swagger/{SwaggerGroups.Admin}/swagger.json", "API Administrativa v1");
        options.DocumentTitle = "PlataformaECommerce Swagger";
    });
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

RequestLocalizationOptions requestLocalizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(requestLocalizationOptions);

ProductImagesOptions productImagesOptions = app.Services.GetRequiredService<IOptions<ProductImagesOptions>>().Value;
string webRootPath = string.IsNullOrWhiteSpace(app.Environment.WebRootPath)
    ? Path.Combine(app.Environment.ContentRootPath, "wwwroot")
    : app.Environment.WebRootPath;
string productImagesPhysicalPath = Path.Combine(webRootPath, productImagesOptions.UploadsDirectory.Replace('/', Path.DirectorySeparatorChar));
Directory.CreateDirectory(productImagesPhysicalPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(productImagesPhysicalPath),
    RequestPath = productImagesOptions.RequestPath
});

app.UseRouting();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
app.MapControllers();

app.Run();
