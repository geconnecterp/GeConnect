using gc.api.core.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace gc.api.infra.Datos.Configuraciones
{
    public class CajaConfiguration : IEntityTypeConfiguration<Caja>
    {
        public void Configure(EntityTypeBuilder<Caja> builder)
        {
            builder.ToTable("cajas");

            builder.HasKey(e => e.Caja_Id);


            builder.Property(e => e.Adm_Id)
                 .IsRequired()
                 .HasMaxLength(4)
                 .IsUnicode(false);


            builder.Property(e => e.Depo_Id)
                 .IsRequired()
                 .HasMaxLength(2)
                 .IsUnicode(false);


            builder.Property(e => e.Dia_Movi)
                 .HasMaxLength(15)
                 .IsUnicode(false);


            builder.Property(e => e.Usu_Id)
                 .HasMaxLength(10)
                 .IsUnicode(false);


            builder.Property(e => e.Caja_Nombre)
                 .IsRequired()
                 .HasMaxLength(30)
                 .IsUnicode(false);


            builder.Property(e => e.Caja_Modalidad)
                 .IsRequired()
                 .HasMaxLength(2)
                 .IsUnicode(false);


            builder.Property(e => e.Caja_Apertura)
                .HasColumnType("datetime");


            builder.Property(e => e.Caja_Cierre)
                .HasColumnType("datetime");


            builder.Property(e => e.Caja_Maquina)
                 .HasMaxLength(30)
                 .IsUnicode(false);


            builder.Property(e => e.Caja_Nro_Proceso)
                 .HasMaxLength(15)
                 .IsUnicode(false);


            builder.Property(e => e.Caja_Nro_Operacion)
                .IsRequired()
                .HasColumnType("int");


            builder.Property(e => e.Caja_Mepa_Categoria)
                 .HasMaxLength(6)
                 .IsUnicode(false);


            builder.Property(e => e.Caja_Mepa_Id)
                 .HasMaxLength(15)
                 .IsUnicode(false);
        }
    }
}
