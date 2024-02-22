using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using gc.api.core.Entidades;

namespace gc.api.infra.Datos.Configuraciones
{
    public class AccesoConfiguration : IEntityTypeConfiguration<Acceso>
    {
        public void Configure(EntityTypeBuilder<Acceso> builder)
        {
            builder.ToTable("Acceso");

            builder.HasKey(e => e.Id);


            builder.Property(e => e.Fecha)
                .IsRequired()
                .HasColumnType("datetime");


            builder.Property(e => e.IP)
                 .IsRequired()
                 .HasMaxLength(15)
                 .IsUnicode(false);


            builder.HasOne(s => s.Usuario)
                .WithMany(r => r.Accesos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Accesos_Usuario");

        }
    }
}
