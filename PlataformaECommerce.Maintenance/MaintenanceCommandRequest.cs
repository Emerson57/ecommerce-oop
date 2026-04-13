namespace PlataformaECommerce.Maintenance;

internal sealed record MaintenanceCommandRequest(string CommandName, string? TenantOverride, bool ShowHelp)
{
    public static MaintenanceCommandRequest Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Any(LegacyTenantMaintenanceCommands.IsHelpToken))
        {
            return new MaintenanceCommandRequest(LegacyTenantMaintenanceCommands.Help, null, showHelp: true);
        }

        string? commandName = args.FirstOrDefault(argument => !argument.StartsWith('-', StringComparison.Ordinal));
        if (string.IsNullOrWhiteSpace(commandName))
        {
            return new MaintenanceCommandRequest(
                LegacyTenantMaintenanceCommands.NormalizeLegacyTenantData,
                ResolveTenantOverride(args),
                showHelp: false);
        }

        string normalizedCommand = commandName.Trim().ToLowerInvariant();
        if (!LegacyTenantMaintenanceCommands.IsSupported(normalizedCommand))
        {
            throw new InvalidOperationException($"El comando de mantenimiento '{commandName}' no existe. Usa 'help' para listar comandos válidos.");
        }

        return new MaintenanceCommandRequest(normalizedCommand, ResolveTenantOverride(args), showHelp: false);
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
