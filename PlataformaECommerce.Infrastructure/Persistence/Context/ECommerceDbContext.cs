using Microsoft.EntityFrameworkCore;
using PlataformaECommerce.Infrastructure.Persistence.Entities;

namespace PlataformaECommerce.Infrastructure.Persistence.Context
{
    public sealed class ECommerceDbContext : DbContext
    {
        #region Constructor

        /// Inicializa una nueva instancia del contexto de base de datos.
        public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options)
            : base(options)
        {
        }

        #endregion

        #region DbSets

        /// Representa la tabla de productos en la base de datos.
        public DbSet<ProductEntity> Products { get; set; } = null!;

        #endregion

        #region Configuración del modelo

        /// Aplica la configuración del modelo utilizando Fluent API.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ECommerceDbContext).Assembly);
        }

        #endregion
    }
}