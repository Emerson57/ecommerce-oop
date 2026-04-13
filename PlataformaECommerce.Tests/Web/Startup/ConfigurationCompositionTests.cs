using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using PlataformaECommerce.Web.Extensions.Startup;

namespace PlataformaECommerce.Tests.Web.Startup;

[TestFixture]
public class ConfigurationCompositionTests
{
    [Test]
    public void ConfigureApplicationConfiguration_Development_EnvironmentVariablesOverrideLocalJson()
    {
        string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDirectory);

        const string environmentVariableName = "Jwt__SigningKey";
        string environmentVariableValue = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + new string('x', 32);
        string? originalEnvironmentVariableValue = Environment.GetEnvironmentVariable(environmentVariableName);

        try
        {
            File.WriteAllText(
                Path.Combine(tempDirectory, "appsettings.Development.local.json"),
                """
                {
                  "Jwt": {
                    "SigningKey": "local-signing-key-that-must-be-overridden-123456"
                  }
                }
                """);

            Environment.SetEnvironmentVariable(environmentVariableName, environmentVariableValue);

            WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = Array.Empty<string>(),
                ApplicationName = typeof(Program).Assembly.FullName,
                ContentRootPath = tempDirectory,
                EnvironmentName = Environments.Development
            });

            builder.ConfigureApplicationConfiguration(Array.Empty<string>());

            Assert.That(builder.Configuration["Jwt:SigningKey"], Is.EqualTo(environmentVariableValue));
        }
        finally
        {
            Environment.SetEnvironmentVariable(environmentVariableName, originalEnvironmentVariableValue);

            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }
}
