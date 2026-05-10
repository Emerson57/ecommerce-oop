namespace PlataformaECommerce.Web.Configuration;

internal static class SecretConfigurationAliases
{
    internal const string DatabasePrimaryConnectionString = "Secrets:Database:PrimaryConnectionString";
    // Alias: Secrets:Security:JwtSigningKey maps to configuration key 'Jwt:SigningKey'
    internal const string JwtSigningKey = "Secrets:Security:JwtSigningKey";
    internal const string WompiPublicKey = "Secrets:Payments:WompiPublicKey";
    internal const string WompiIntegritySecret = "Secrets:Payments:WompiIntegritySecret";
    internal const string SmtpUserName = "Secrets:Notifications:SmtpUserName";
    internal const string SmtpPassword = "Secrets:Notifications:SmtpPassword";
    internal const string BootstrapSuperUserPassword = "Secrets:Bootstrap:SuperUserPassword";
    internal const string AllowedHosts = "Secrets:Hosting:AllowedHosts";
    internal const string AdminBootstrapEnabled = "AdminBootstrap:Enabled";
    internal const string AdminBootstrapEmail = "AdminBootstrap:Email";
    internal const string AdminBootstrapPassword = "AdminBootstrap:Password";

    internal static IReadOnlyList<(string SourcePath, string DestinationPath)> Mappings { get; } =
    [
        (DatabasePrimaryConnectionString, "ConnectionStrings:DefaultConnection"),
        (JwtSigningKey, "Jwt:SigningKey"),
        (WompiPublicKey, "Payments:Wompi:PublicKey"),
        (WompiIntegritySecret, "Payments:Wompi:IntegritySecret"),
        (SmtpUserName, "Notifications:Smtp:UserName"),
        (SmtpPassword, "Notifications:Smtp:Password"),
        (BootstrapSuperUserPassword, "Bootstrap:SuperUser:Password"),
        (AllowedHosts, "AllowedHosts"),
        (AdminBootstrapEnabled, "Bootstrap:SuperUser:Enabled"),
        (AdminBootstrapEmail, "Bootstrap:SuperUser:Email"),
        (AdminBootstrapPassword, "Bootstrap:SuperUser:Password")
    ];
}
