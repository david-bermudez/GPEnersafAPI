using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using GpEnerSaf.Models.BD;

namespace GpEnerSaf.Data
{
    public partial class DbGpContext : Microsoft.EntityFrameworkCore.DbContext
    {
        private readonly IHttpContextAccessor httpAccessor;

        public DbGpContext(IHttpContextAccessor httpAccessor, DbContextOptions<DbGpContext> options):base(options)
        {
            this.httpAccessor = httpAccessor;
        }

        public DbGpContext(IHttpContextAccessor httpAccessor)
        {
            this.httpAccessor = httpAccessor;
        }

        public DbGpContext (DbContextOptions<DbGpContext> options):base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<GPLiquidacion>()
                .HasKey(a => new { a.Fechafacturacion, a.Factura_id});
            builder.Entity<GPLiquidacionLog>()
                .HasKey(a => new { a.Id});
            builder.Entity<GPConfiguracion>()
                .HasKey(a => new { a.Codigo, a.Nombre, a.Tipo, a.Concepto, a.Codctaco});
            builder.Entity<GPMunicipio>()
                .HasKey(a => new { a.Municipio });
            builder.Entity<GPSaldo>()
                .HasKey(a => new { a.CodigoIngreso, a.Fechafacturacion, a.Version, a.Factura_id, a.Concepto });
            builder.Entity<GPLiquidacionConcepto>()
                .HasKey(a => new { a.Fechafacturacion, a.Factura_id, a.Concepto });
            builder.Entity<GPUsuario>()
                .HasKey(a => new { a.Usuario });
        }

        //Entities
        public DbSet<GpEnerSaf.Models.BD.GPLiquidacion> GPLiquidacionEntity
        {
          get;
          set;
        }

        public DbSet<GpEnerSaf.Models.BD.GPLiquidacionLog> GPLiquidacionLogEntity
        {
            get;
            set;
        }

        public DbSet<GpEnerSaf.Models.BD.GPConfiguracion> GPConfiguracionEntity
        {
            get;
            set;
        }

        public DbSet<GpEnerSaf.Models.BD.GPMunicipio> GPMunicipioEntity
        {
            get;
            set;
        }

        public DbSet<GpEnerSaf.Models.BD.GPSaldo> GPSaldoEntity
        {
            get;
            set;
        }

        public DbSet<GpEnerSaf.Models.BD.GPLiquidacionConcepto> GPLiquidacionConceptoEntity
        {
            get;
            set;
        }

        public DbSet<GpEnerSaf.Models.BD.GPUsuario> GPUsuarioEntity
        {
            get;
            set;
        }
    }
}
