namespace PlataformaECommerce.Infrastructure.Observers
{
    /// Define el contrato para los objetos que generan eventos
    /// y notifican a los observadores suscritos.
    public interface ISujeto
    {
        void RegistrarObservador(IObservador observador);

        void RemoverObservador(IObservador observador);

        void NotificarObservadores(string evento, string mensaje);
    }
}