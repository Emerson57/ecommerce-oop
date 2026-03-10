namespace PlataformaECommerce.Application.Interfaces.Repositories
{
    public interface IProductoAuditRepository
    {
        /// Registra un evento de auditoría asociado a un producto.
        Task RegistrarEventoAsync(int productoId, string accion, string detalle, string usuarioResponsable);

        /// Obtiene el historial de auditoría de un producto.
        Task<IReadOnlyList<string>> ObtenerHistorialAsync(int productoId);
    }
}