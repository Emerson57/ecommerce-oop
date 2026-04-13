using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PlataformaECommerce.Maintenance;

internal static class Program
{
    /// <summary>
    /// Ejecuta el proceso explícito de mantenimiento fuera del host web.
    /// </summary>
    public static async Task<int> Main(string[] args)
    {
        using CancellationTokenSource cancellationTokenSource = new();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        try
        {
            MaintenanceCommandRequest commandRequest = MaintenanceCommandRequest.Parse(args);
            if (commandRequest.ShowHelp)
            {
                LegacyTenantMaintenanceCommands.WriteHelp(Console.Out);
                return 0;
            }

            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
            builder.ConfigureMaintenanceHost(args);

            using IHost host = builder.Build();
            MaintenanceCommandDispatcher dispatcher = host.Services.GetRequiredService<MaintenanceCommandDispatcher>();
            await dispatcher.DispatchAsync(commandRequest, cancellationTokenSource.Token).ConfigureAwait(false);
            return 0;
        }
        catch (OperationCanceledException)
        {
            return 130;
        }
    }
}
