using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace PlataformaECommerce.Tests.Web.Integration;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost,1433;Database=PlataformaECommerceTests;User Id=sa;Password=StrongPassword#2026;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True;",
                ["MongoDb:Enabled"] = "false",
                ["Jwt:Issuer"] = "PlataformaECommerce.Tests",
                ["Jwt:Audience"] = "PlataformaECommerce.Tests.Clients",
                ["Jwt:SigningKey"] = "IntegrationTestsSigningKey_With32Chars!",
                ["DataProtection:ApplicationName"] = "PlataformaECommerce.Tests"
            });
        });
    }
}
