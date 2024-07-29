using gc.api.core.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace gc.api.infra.Datos.Configuraciones
{
    public class CuentaConfiguration : IEntityTypeConfiguration<Cuenta>
    {
        public void Configure(EntityTypeBuilder<Cuenta> builder)
        {
            builder.ToTable("cuentas");

            builder.HasKey(e => e.Cta_Id);


            builder.Property(e => e.Cta_Denominacion)
                 .IsRequired()
                 .HasMaxLength(150)
                 .IsUnicode(false);


            builder.Property(e => e.Tdoc_Id)
                 .HasMaxLength(2)
                 .IsUnicode(false);


            builder.Property(e => e.Cta_Documento)
                 .HasMaxLength(25)
                 .IsUnicode(false);


            builder.Property(e => e.Cta_Domicilio)
                 .HasMaxLength(150)
                 .IsUnicode(false);


            builder.Property(e => e.Cta_Localidad)
                 .HasMaxLength(150)
                 .IsUnicode(false);


            builder.Property(e => e.Cta_Cpostal)
                 .HasMaxLength(10)
                 .IsUnicode(false);


            builder.Property(e => e.Dep_Id)
                 .HasMaxLength(3)
                 .IsUnicode(false);


            builder.Property(e => e.Cta_Te)
                 .HasMaxLength(70)
                 .IsUnicode(false);


            builder.Property(e => e.Cta_Celu)
                 .HasMaxLength(20)
                 .IsUnicode(false);


            builder.Property(e => e.Cta_Email)
                 .HasMaxLength(100)
                 .IsUnicode(false);


            builder.Property(e => e.Cta_Www)
                 .HasMaxLength(255)
                 .IsUnicode(false);


            builder.Property(e => e.Afip_Id)
                 .HasMaxLength(2)
                 .IsUnicode(false);


            builder.Property(e => e.Nj_Id)
                 .HasMaxLength(3)
                 .IsUnicode(false);


            builder.Property(e => e.Cta_Ib_Nro)
                 .HasMaxLength(15)
                 .IsUnicode(false);


            builder.Property(e => e.Cta_Bco_Cuenta_Nro)
                 .HasMaxLength(100)
                 .IsUnicode(false);


            builder.Property(e => e.Cta_Bco_Cuenta_Cbu)
                 .HasMaxLength(40)
                 .IsUnicode(false);


            builder.Property(e => e.Cta_Alta)
                .HasColumnType("datetime");


            builder.Property(e => e.Cta_Obs)
                 .HasMaxLength(255)
                 .IsUnicode(false);


            builder.Property(e => e.Cta_Cuit_Vto)
                .HasColumnType("datetime");


            builder.Property(e => e.Cta_Emp_Legajo)
                 .HasMaxLength(10)
                 .IsUnicode(false);


            builder.Property(e => e.Cta_Emp_Ctaf)
                 .HasMaxLength(8)
                 .IsUnicode(false);


            builder.Property(e => e.Cta_Actu_Fecha)
                .HasColumnType("datetime");

        }
    }
}
