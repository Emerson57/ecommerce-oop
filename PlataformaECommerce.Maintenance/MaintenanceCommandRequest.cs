namespace PlataformaECommerce.Maintenance;

internal sealed record MaintenanceCommandRequest(string CommandName, string? TenantOverride, bool ShowHelp)
{
    public static MaintenanceCommandRequest Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Any(LegacyTenantMaintenanceCommands.IsHelpToken))
        {
            return new MaintenanceCommandRequest(LegacyTenantMaintenanceCommands.Help, null, ShowHelp: true);
        }

        string? commandName = args.FirstOrDefault(argument => !argument.StartsWith("-", StringComparison.Ordinal));
        if (string.IsNullOrWhiteSpace(commandName))
        {
            return new MaintenanceCommandRequest(LegacyTenantMaintenanceCommands.Help, null, ShowHelp: true);
        }

        string normalizedCommand = commandName.Trim().ToLowerInvariant();
        if (!LegacyTenantMaintenanceCommands.IsSupported(normalizedCommand)
            && !SaaSBootstrapMaintenanceCommands.IsSupported(normalizedCommand))
        {
            throw new InvalidOperationException($"El comando de mantenimiento '{commandName}' no existe. Usa 'help' para listar comandos válidos.");
        }

        return new MaintenanceCommandRequest(normalizedCommand, ResolveTenantOverride(args), ShowHelp: false);
    }

    private static string? ResolveTenantOverride(string[] args)
    {
        const string tenantArgumentPrefix = "--tenant=";
        string? tenantArgument = args.FirstOrDefault(argument => argument.StartsWith(tenantArgumentPrefix, StringComparison.OrdinalIgnoreCase));
        if (tenantArgument is null)
        {
            return null;
        }

        string tenantId = tenantArgument[tenantArgumentPrefix.Length..].Trim();
        return string.IsNullOrWhiteSpace(tenantId) ? null : tenantId;
    }
}
