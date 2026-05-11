using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace PlataformaECommerce.Web.Configuration;

/// <summary>
/// Falla el arranque en Production si faltan secretos críticos o si quedan valores típicos de prueba en pagos.
/// No registra ni incluye en mensajes el valor de los secretos.
/// </summary>
public static class ProductionSecretsConfigurationGuard
{
    /// <summary>
    /// Valida presencia mínima de secretos y coherencia básica para entorno real.
    /// </summary>
    public static void Validate(IConfiguration configuration, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        if (!environment.IsProduction())
        {
            return;
        }

        List<string> errors = [];

        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            errors.Add(
                "La cadena de conexión 'ConnectionStrings:DefaultConnection' es obligatoria y no puede estar vacía. "
                + "Defínala con variable de entorno 'ConnectionStrings__DefaultConnection' o alias 'Secrets__Database__PrimaryConnectionString'.");
        }

        string? jwtSigningKey = configuration["Jwt:SigningKey"];
        if (string.IsNullOrWhiteSpace(jwtSigningKey) || jwtSigningKey.Trim().Length < 32)
        {
            errors.Add(
                "La clave 'Jwt:SigningKey' es obligatoria en Production y debe tener al menos 32 caracteres. "
                + "Use 'Jwt__SigningKey' o 'Secrets__Security__JwtSigningKey'.");
        }

        bool wompiEnabled = configuration.GetValue("Payments:Wompi:Enabled", false);
        if (wompiEnabled)
        {
            string? publicKey = configuration["Payments:Wompi:PublicKey"];
            string? integritySecret = configuration["Payments:Wompi:IntegritySecret"];
            if (string.IsNullOrWhiteSpace(publicKey) || string.IsNullOrWhiteSpace(integritySecret))
            {
                errors.Add(
                    "Con 'Payments:Wompi:Enabled' en true, deben definirse 'Payments:Wompi:PublicKey' y 'Payments:Wompi:IntegritySecret' (p. ej. variables 'Payments__Wompi__PublicKey' y 'Payments__Wompi__IntegritySecret').");
            }
            else if (publicKey.Trim().StartsWith("pub_test_", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(
                    "En Production no se permiten llaves públicas Wompi de entorno de pruebas (prefijo pub_test_). Use llaves de comercio y rote cualquier valor de prueba que haya llegado a un entorno real.");
            }

            string? checkout = configuration["Payments:Wompi:CheckoutBaseUrl"];
            string? transactions = configuration["Payments:Wompi:TransactionsApiBaseUrl"];
            if (string.IsNullOrWhiteSpace(checkout) || string.IsNullOrWhiteSpace(transactions))
            {
                errors.Add("Con Wompi habilitado, 'Payments:Wompi:CheckoutBaseUrl' y 'Payments:Wompi:TransactionsApiBaseUrl' son obligatorias.");
            }
        }

        bool smtpEnabled = configuration.GetValue("Notifications:Smtp:Enabled", false);
        if (smtpEnabled)
        {
            if (string.IsNullOrWhiteSpace(configuration["Notifications:Smtp:Host"]))
            {
                errors.Add("Con SMTP habilitado, 'Notifications:Smtp:Host' es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(configuration["Notifications:Smtp:Password"]))
            {
                errors.Add("Con SMTP habilitado, 'Notifications:Smtp:Password' es obligatorio (p. ej. 'Notifications__Smtp__Password' o alias 'Secrets__Notifications__SmtpPassword').");
            }

            if (string.IsNullOrWhiteSpace(configuration["Notifications:Smtp:FromAddress"]))
            {
                errors.Add("Con SMTP habilitado, 'Notifications:Smtp:FromAddress' es obligatorio.");
            }
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "La configuración de secretos para Production es inválida o incompleta: "
                + string.Join(" ", errors)
                + " Consulte docs/production-secrets.md y docs/operations/configuration-secrets.md.");
        }
    }
}
