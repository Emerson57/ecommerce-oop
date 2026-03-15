namespace PlataformaECommerce.Infrastructure.Audit.Services
{
    /// Define el contrato que deben implementar todos los observadores
    /// que desean recibir notificaciones de eventos del sistema.
    public interface IObservador
    {
        void Actualizar(string evento, string mensaje);
    }
}