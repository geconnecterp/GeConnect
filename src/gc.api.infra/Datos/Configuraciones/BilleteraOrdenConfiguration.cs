using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using gc.api.core.Entidades;

namespace gc.api.infra.Datos.Configuraciones
{
    public class BilleteraOrdenConfiguration : IEntityTypeConfiguration<BilleteraOrden>
    {
        public void Configure(EntityTypeBuilder<BilleteraOrden> builder)
        {
            builder.ToTable("billeteras_ordenes");

            builder.HasKey(e => e.Bo_Id);


            builder.Property(e => e.Rb_Compte)
                 .IsRequired()
                 .HasMaxLength(20)
                 .IsUnicode(false);


            builder.Property(e => e.Adm_Id)
                 .IsRequired()
                 .HasMaxLength(4)
                 .IsUnicode(false);


            builder.Property(e => e.Caja_Id)
                 .IsRequired()
                 .HasMaxLength(4)
                 .IsUnicode(false);


            builder.Property(e => e.Bill_Id)
                 .HasMaxLength(2)
                 .IsUnicode(false);


            builder.Property(e => e.Boe_Id)
                 .IsRequired()
                 .HasMaxLength(2)
                 .IsUnicode(false);


            builder.Property(e => e.Cuit)
                 .IsRequired()
                 .HasMaxLength(15)
                 .IsUnicode(false);


            builder.Property(e => e.Tco_Id)
                 .HasMaxLength(3)
                 .IsUnicode(false);


            builder.Property(e => e.Cm_Compte)
                 .HasMaxLength(15)
                 .IsUnicode(false);


            builder.Property(e => e.Bo_Importe)
                .IsRequired()
                .HasPrecision(10, 2);


            builder.Property(e => e.Bo_Carga)
                .IsRequired()
                .HasColumnType("datetime");


            builder.Property(e => e.Bo_Clave)
                 .HasMaxLength(500)
                 .IsUnicode(false);


            builder.Property(e => e.Bo_Id_Ext)
                 .HasMaxLength(20)
                 .IsUnicode(false);


            builder.Property(e => e.Bo_Notificado)
                .HasColumnType("datetime");


            builder.Property(e => e.Bo_Notificado_Desc)
                 .HasMaxLength(100)
                 .IsUnicode(false);


            builder.Property(e => e.Ip)
                 .HasMaxLength(15)
                 .IsUnicode(false);

        }
    }
}
