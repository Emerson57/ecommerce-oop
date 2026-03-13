using System;

namespace PlataformaECommerce.Infrastructure.Observers
{
    /// Observador que simula la actualización de la interfaz
    /// de usuario cuando ocurre un evento del sistema.
    public class ObservadorUI : IObservador
    {
        public void Actualizar(string evento, string mensaje)
        {
            Console.WriteLine($"[UI] Evento recibido: {evento} -> {mensaje}");
        }
    }
}