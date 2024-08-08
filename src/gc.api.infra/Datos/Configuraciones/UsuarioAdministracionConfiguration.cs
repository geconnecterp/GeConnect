using gc.api.core.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace gc.api.infra.Datos.Configuraciones
{
    public class UsuarioAdministracionConfiguration : IEntityTypeConfiguration<UsuarioAdministracion>
    {
        public void Configure(EntityTypeBuilder<UsuarioAdministracion> builder)
        {
            builder.ToTable("usuarios_administraciones");

            builder.HasKey(e => new { e.Usu_Id, e.Adm_Id });

            builder.HasOne(d => d.Usuario)
              .WithMany(p => p.UsuarioAdministraciones)
              .HasForeignKey(d => d.Usu_Id)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("fk_usuadm_ref_usu");

        }
    }
}
