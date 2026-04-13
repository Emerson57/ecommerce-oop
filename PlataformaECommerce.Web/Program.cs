using PlataformaECommerce.Web.Extensions.Startup;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureApplicationConfiguration(args);
builder.ConfigureApplicationLogging();
builder.Services.AddApplicationCompositionServices(builder.Configuration, builder.Environment);

var app = builder.Build();

await app.RunApplicationStartupInitializationAsync();
app.UseApplicationPipeline();
app.MapHttpEndpoints();

app.Run();
