using gc.api.core.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace gc.api.infra.Datos.Configuraciones
{
    public class UsuariosConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("usuarios");

            builder.HasKey(e => e.Usu_id);


            builder.Property(e => e.Usu_password)
                 .IsRequired()
                 .HasMaxLength(300)
                 .IsUnicode(false);


            builder.Property(e => e.Usu_apellidoynombre)
                 .IsRequired()
                 .HasMaxLength(50)
                 .IsUnicode(false);


            builder.Property(e => e.Usu_bloqueado_fecha)
                .HasColumnType("datetime");


            builder.Property(e => e.Usu_alta)
                .HasColumnType("datetime");


            builder.Property(e => e.Usu_fecha_expira_inicio)
                .HasColumnType("datetime");


            builder.Property(e => e.Tdo_codigo)
                 .HasMaxLength(2)
                 .IsUnicode(false);


            builder.Property(e => e.Usu_documento)
                 .HasMaxLength(11)
                 .IsUnicode(false);


            builder.Property(e => e.Usu_email)
                 .HasMaxLength(80)
                 .IsUnicode(false);


            builder.Property(e => e.Usu_celu)
                 .HasMaxLength(80)
                 .IsUnicode(false);


            builder.Property(e => e.Usu_pin)
                 .HasMaxLength(30)
                 .IsUnicode(false);


            builder.Property(e => e.Cta_id)
                 .HasMaxLength(8)
                 .IsUnicode(false);

        }

    }
}
