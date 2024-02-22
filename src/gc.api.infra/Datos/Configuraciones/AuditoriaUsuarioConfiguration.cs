using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using gc.api.core.Entidades;

namespace gc.api.infra.Datos.Configuraciones
{
    public class AuditoriaUsuarioConfiguration : IEntityTypeConfiguration<AuditoriaUsuario>
    {
        public void Configure(EntityTypeBuilder<AuditoriaUsuario> builder)
        {
            builder.ToTable("AuditoriaUsuario");

            builder.HasKey(e => e.Id);


            builder.Property(e => e.FechaAuditoria)
                .IsRequired()
                .HasColumnType("datetime");


            builder.Property(e => e.IP)
                 .IsRequired()
                 .HasMaxLength(15)
                 .IsUnicode(false);


            builder.Property(e => e.MetodoAccedido)
                 .IsRequired()
                 .HasMaxLength(100)
                 .IsUnicode(false);


            builder.HasOne(s => s.Usuario)
                .WithMany(r => r.AuditoriaUsuarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AuditoriaUsuario_Usuario");

        }
    }
}
