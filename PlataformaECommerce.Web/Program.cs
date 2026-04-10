using PlataformaECommerce.Application.DependencyInjection;
using PlataformaECommerce.Infrastructure.DependencyInjection;
using PlataformaECommerce.Web.Extensions.Startup;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddModularConfigurationFiles(builder.Environment);
builder.Configuration.AddLocalDevelopmentConfiguration(builder.Environment);
builder.Host.AddConfiguredSerilog(builder.Configuration);

builder.Services
    .AddConfiguredProblemDetails()
    .AddConfiguredAntiforgery(builder.Configuration, builder.Environment)
    .AddConfiguredBranding(builder.Configuration)
    .AddConfiguredBackoffice(builder.Configuration)
    .AddConfiguredPresentation()
    .AddConfiguredOpenApi()
    .AddConfiguredObservability(builder.Configuration)
    .AddConfiguredForwardedHeaders(builder.Configuration, builder.Environment)
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
