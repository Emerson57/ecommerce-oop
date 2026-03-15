using System;

namespace PlataformaECommerce.Infrastructure.Audit.Services
{
    /// Observador que registra eventos importantes
    /// para auditoría o monitoreo del sistema.
    public class ObservadorLogs : IObservador
    {
        public void Actualizar(string evento, string mensaje)
        {
            Console.WriteLine($"[LOG] {DateTime.Now:HH:mm:ss} | {evento} -> {mensaje}");
        }
    }
}