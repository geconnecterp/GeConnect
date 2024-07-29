using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using gc.api.core.Entidades;

namespace gc.api.infra.Datos.Configuraciones
{
    public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
    {
        public void Configure(EntityTypeBuilder<Producto> builder)
        {
            builder.ToTable("productos");

            builder.HasKey(e => e.P_Id);


            builder.Property(e => e.P_M_Marca)
                 .HasMaxLength(30)
                 .IsUnicode(false);


            builder.Property(e => e.P_M_Desc)
                 .HasMaxLength(30)
                 .IsUnicode(false);


            builder.Property(e => e.P_M_Capacidad)
                 .HasMaxLength(15)
                 .IsUnicode(false);


            builder.Property(e => e.P_Id_Prov)
                 .HasMaxLength(15)
                 .IsUnicode(false);


            builder.Property(e => e.P_Desc)
                 .IsRequired()
                 .HasMaxLength(80)
                 .IsUnicode(false);


            builder.Property(e => e.P_Desc_Ticket)
                 .IsRequired()
                 .HasMaxLength(50)
                 .IsUnicode(false);


            builder.Property(e => e.P_Peso)
                .HasPrecision(10, 2);


            builder.Property(e => e.Up_Id)
                 .HasMaxLength(2)
                 .IsUnicode(false);


            builder.Property(e => e.Rub_Id)
                 .IsRequired()
                 .HasMaxLength(4)
                 .IsUnicode(false);


            builder.Property(e => e.Cta_Id)
                 .IsRequired()
                 .HasMaxLength(8)
                 .IsUnicode(false);


            builder.Property(e => e.Pg_Id)
                 .IsRequired()
                 .HasMaxLength(5)
                 .IsUnicode(false);


            builder.Property(e => e.P_Plista)
                .IsRequired()
                .HasPrecision(10, 2);


            builder.Property(e => e.P_Dto1)
                .IsRequired()
                .HasPrecision(10, 2);


            builder.Property(e => e.P_Dto2)
                .IsRequired()
                .HasPrecision(10, 2);


            builder.Property(e => e.P_Dto3)
                .IsRequired()
                .HasPrecision(10, 2);


            builder.Property(e => e.P_Dto4)
                .IsRequired()
                .HasPrecision(10, 2);


            builder.Property(e => e.P_Dto5)
                .IsRequired()
                .HasPrecision(10, 2);


            builder.Property(e => e.P_Dto6)
                .IsRequired()
                .HasPrecision(10, 2);


            builder.Property(e => e.P_Dto_Pa)
                .IsRequired()
                .HasPrecision(10, 2);


            builder.Property(e => e.P_Boni)
                 .HasMaxLength(7)
                 .IsUnicode(false);


            builder.Property(e => e.P_Porc_Flete)
                .IsRequired()
                .HasPrecision(10, 2);


            builder.Property(e => e.In_Alicuota)
                .IsRequired()
                .HasPrecision(10, 2);


            builder.Property(e => e.Iva_Alicuota)
                .IsRequired()
                .HasPrecision(10, 2);


            builder.Property(e => e.P_Pcosto)
                .IsRequired()
                .HasPrecision(10, 2);


            builder.Property(e => e.P_Pcosto_Repo)
                .IsRequired()
                .HasPrecision(10, 2);


            builder.Property(e => e.P_Alta)
                .HasColumnType("datetime");


            builder.Property(e => e.Usu_Id_Alta)
                 .HasMaxLength(10)
                 .IsUnicode(false);


            builder.Property(e => e.P_Modi)
                .HasColumnType("datetime");


            builder.Property(e => e.Usu_Id_Modi)
                 .HasMaxLength(10)
                 .IsUnicode(false);


            builder.Property(e => e.P_Obs)
                 .HasMaxLength(100)
                 .IsUnicode(false);


            builder.Property(e => e.P_Balanza_Id)
                 .HasMaxLength(4)
                 .IsUnicode(false);


            builder.Property(e => e.Lp_Id_Default)
                 .HasMaxLength(3)
                 .IsUnicode(false);

        }
    }
}
