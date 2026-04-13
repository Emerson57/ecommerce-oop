namespace PlataformaECommerce.Web.Configuration;

internal static class SecretConfigurationAliases
{
    internal const string DatabasePrimaryConnectionString = "Secrets:Database:PrimaryConnectionString";
    internal const string JwtSigningKey = "Secrets:Security:JwtSigningKey";
    internal const string MongoDbConnectionString = "Secrets:Observability:MongoDbConnectionString";
    internal const string WompiPublicKey = "Secrets:Payments:WompiPublicKey";
    internal const string WompiIntegritySecret = "Secrets:Payments:WompiIntegritySecret";
    internal const string SmtpUserName = "Secrets:Notifications:SmtpUserName";
    internal const string SmtpPassword = "Secrets:Notifications:SmtpPassword";
    internal const string BootstrapSuperUserPassword = "Secrets:Bootstrap:SuperUserPassword";

    internal static IReadOnlyList<(string SourcePath, string DestinationPath)> Mappings { get; } =
    [
        (DatabasePrimaryConnectionString, "ConnectionStrings:DefaultConnection"),
        (JwtSigningKey, "Jwt:SigningKey"),
        (MongoDbConnectionString, "MongoDb:ConnectionString"),
        (WompiPublicKey, "Payments:Wompi:PublicKey"),
        (WompiIntegritySecret, "Payments:Wompi:IntegritySecret"),
        (SmtpUserName, "Notifications:Smtp:UserName"),
        (SmtpPassword, "Notifications:Smtp:Password"),
        (BootstrapSuperUserPassword, "Bootstrap:SuperUser:Password")
    ];
}
