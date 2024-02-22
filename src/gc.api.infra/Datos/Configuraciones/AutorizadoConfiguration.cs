using gc.api.core.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace gc.api.infra.Datos.Configuraciones
{
    public class AutorizadoConfiguration : IEntityTypeConfiguration<Autorizado>
    {
        public void Configure(EntityTypeBuilder<Autorizado> builder)
        {
            builder.ToTable("Autorizado");

            builder.HasKey(e => new { e.RoleId, e.UsuarioId });  //soluciona el error RoleId1


            builder.HasOne(d => d.Usuario)
              .WithMany(p => p.Autorizados)
              .HasForeignKey(d => d.UsuarioId)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK_Autorizado_Usuario");

            builder.HasOne(d => d.Role)
               .WithMany(p => p.Autorizados)
               .HasForeignKey(d => d.RoleId)
               .OnDelete(DeleteBehavior.ClientSetNull)
               .HasConstraintName("FK_Autorizado_Rol");

        }
    }
}
