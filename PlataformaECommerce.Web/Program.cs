using PlataformaECommerce.Application.DependencyInjection;
using PlataformaECommerce.Infrastructure.DependencyInjection;
using PlataformaECommerce.Web.Extensions.Startup;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddLocalDevelopmentConfiguration(builder.Environment);
builder.Host.AddConfiguredSerilog(builder.Configuration);

builder.Services
    .AddConfiguredProblemDetails()
    .AddConfiguredPresentation()
    .AddConfiguredOpenApi()
    .AddConfiguredSecurity(builder.Configuration, builder.Environment)
    .AddConfiguredRateLimiting(builder.Configuration)
    .AddConfiguredHealthChecks(builder.Configuration)
    .AddConfiguredInitialization(builder.Configuration)
    .AddApplicationServices()
    .AddInfrastructure(builder.Configuration, builder.Environment);

var app = builder.Build();

await app.RunCriticalInitializationAsync();

app.UseConfiguredPipeline();
app.MapConfiguredEndpoints();

app.Run();
