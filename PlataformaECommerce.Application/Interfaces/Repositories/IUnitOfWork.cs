namespace PlataformaECommerce.Application.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        /// Guarda los cambios pendientes en el origen de datos.
        Task<int> GuardarCambiosAsync();
    }
}