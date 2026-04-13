using PlataformaECommerce.Web.Extensions.Startup;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureWebApplicationHost(args);

WebApplication app = builder.Build();

await app.BootstrapWebApplicationAsync();

app.Run();
