using System.Collections.Generic;

namespace PlataformaECommerce.Infrastructure.Audit.Services
{
    /// Implementación concreta del sujeto observado.
    /// Gestiona la lista de observadores y envía notificaciones
    /// cuando ocurre un evento relevante del sistema.
    public class NotificadorEventos : ISujeto
    {
        private readonly List<IObservador> _observadores = new();

        public void RegistrarObservador(IObservador observador)
        {
            if (observador == null)
                return;

            if (!_observadores.Contains(observador))
                _observadores.Add(observador);
        }

        public void RemoverObservador(IObservador observador)
        {
            if (observador == null)
                return;

            _observadores.Remove(observador);
        }

        public void NotificarObservadores(string evento, string mensaje)
        {
            foreach (var observador in _observadores)
            {
                observador.Actualizar(evento, mensaje);
            }
        }
    }
}