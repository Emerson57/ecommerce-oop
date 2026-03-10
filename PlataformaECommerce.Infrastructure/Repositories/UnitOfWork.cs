using PlataformaECommerce.Application.Interfaces.Repositories;
using PlataformaECommerce.Infrastructure.Persistence.Context;

namespace PlataformaECommerce.Infrastructure.Repositories
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        #region Campos privados

        private readonly ECommerceDbContext _context;

        #endregion

        #region Constructor

        /// Inicializa una nueva instancia de la unidad de trabajo.
        public UnitOfWork(ECommerceDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #endregion

        #region Métodos públicos

        /// Guarda en la base de datos todos los cambios pendientes del contexto actual.
        public async Task<int> GuardarCambiosAsync()
        {
            return await _context.SaveChangesAsync();
        }

        #endregion
    }
}