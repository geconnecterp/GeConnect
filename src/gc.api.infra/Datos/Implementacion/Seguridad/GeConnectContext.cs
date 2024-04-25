namespace gc.api.infra.Datos
{
    using System;
    using System.Linq;
    using System.Reflection;
    using gc.api.core.Entidades;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;


    public partial class GeConnectContext : DbContext
    {
        public GeConnectContext()
        {
        }

        public GeConnectContext(DbContextOptions<GeConnectContext> options)
            : base(options)
        {
        }

       
        public virtual DbSet<Usuario> Usuarios { get; set; }
        public virtual DbSet<Administracion> Administraciones { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //especifico que todos los tipos decimales tengan una misma presicion
            foreach (var propiedad in modelBuilder.Model.GetEntityTypes().SelectMany(t=>t.GetProperties())
                .Where(p=>p.ClrType==typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                propiedad.SetColumnType("decimal(18,2)");
            }

            //rastrea en el assembly los archivos de configuración
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
               
    }
}
