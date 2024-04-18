using gc.api.core.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace gc.api.infra.Datos.Configuraciones
{
    public class UsuariosConfiguration : IEntityTypeConfiguration<Usuarios>
    {
        public void Configure(EntityTypeBuilder<Usuarios> builder)
        {
            builder.ToTable("usuarios");

            builder.HasKey(e => e.usu_id);


            builder.Property(e => e.usu_password)
                 .IsRequired()
                 .HasMaxLength(300)
                 .IsUnicode(false);


            builder.Property(e => e.usu_apellidoynombre)
                 .IsRequired()
                 .HasMaxLength(50)
                 .IsUnicode(false);


            builder.Property(e => e.usu_bloqueado_fecha)
                .HasColumnType("datetime");


            builder.Property(e => e.usu_alta)
                .HasColumnType("datetime");


            builder.Property(e => e.usu_fecha_expira_inicio)
                .HasColumnType("datetime");


            builder.Property(e => e.tdo_codigo)
                 .HasMaxLength(2)
                 .IsUnicode(false);


            builder.Property(e => e.usu_ducumento)
                 .HasMaxLength(11)
                 .IsUnicode(false);


            builder.Property(e => e.usu_email)
                 .HasMaxLength(80)
                 .IsUnicode(false);


            builder.Property(e => e.usu_celu)
                 .HasMaxLength(80)
                 .IsUnicode(false);


            builder.Property(e => e.usu_pin)
                 .HasMaxLength(30)
                 .IsUnicode(false);


            builder.Property(e => e.cta_id)
                 .HasMaxLength(8)
                 .IsUnicode(false);

        }

    }
}
