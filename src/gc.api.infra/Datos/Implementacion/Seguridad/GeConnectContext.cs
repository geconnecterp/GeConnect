﻿namespace gc.api.infra.Datos
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
        public virtual DbSet<BilleteraOrden> BilleteraOrdenes { get; set; }
        public virtual DbSet<Billetera> Billeteras { get; set; }
        public virtual DbSet<BOrdenEstado> BilleteraConfiguraciones { get; set; }
        public virtual DbSet<BilleteraConfiguracion> BOrdenEstados { get; set; }
        public virtual DbSet<Caja> Cajas { get; set; }
        public virtual DbSet<Proveedor> Proveedores { get; set; }
        public virtual DbSet<Producto> Productos { get; set; }
        public virtual DbSet<Cuenta> Cuentas { get; set; }
        public virtual DbSet<Rubro> Rubros { get; set; }
        public virtual DbSet<Deposito> Depositos { get; set; }
        public virtual DbSet<ProductoDeposito> ProductoDepositos { get; set; }
        //public virtual DbSet<UsuarioAdministracion> UsuarioAdministracioens { get; set; }




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
