using System;

namespace PlataformaECommerce.Infrastructure.Observers
{
    /// Observador responsable de reaccionar a eventos relacionados
    /// con inventario dentro del sistema.
    public class ObservadorInventario : IObservador
    {
        public void Actualizar(string evento, string mensaje)
        {
            Console.WriteLine($"[INVENTARIO] Evento: {evento} -> {mensaje}");
        }
    }
}