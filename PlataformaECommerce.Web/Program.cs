using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using PlataformaECommerce.Application.DependencyInjection;
using PlataformaECommerce.Infrastructure.DependencyInjection;
using PlataformaECommerce.Web.Authorization;
using PlataformaECommerce.Web.Middlewares;
using PlataformaECommerce.Web.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", AuthorizationPolicies.AdminOnly);
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
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication()
    .AddCookie(AuthorizationPolicies.AdminCookieScheme, options =>
    {
        options.Cookie.Name = "PlataformaECommerce.Admin";
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
    {
        policy.AuthenticationSchemes.Add(AuthorizationPolicies.AdminCookieScheme);
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Administrador");
    });
});

var app = builder.Build();

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

app.UseRouting();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
app.MapControllers();

app.Run();
