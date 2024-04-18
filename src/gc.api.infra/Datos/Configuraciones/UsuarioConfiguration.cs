using gc.api.core.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace gc.api.infra.Datos.Configuraciones
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuarios>
    {
        public void Configure(EntityTypeBuilder<Usuarios> builder)
        {
            builder.ToTable("Usuario");

            builder.HasKey(e => e.Id);


            builder.Property(e => e.Contrasena)
                 .IsRequired()
                 .HasMaxLength(200)
                 .IsUnicode(false);


            builder.Property(e => e.Correo)
                 .IsRequired()
                 .HasMaxLength(50)
                 .IsUnicode(false);


            builder.Property(e => e.Intentos)
                .IsRequired()
                .HasColumnType("int");


            builder.Property(e => e.FechaAlta)
                .IsRequired()
                .HasColumnType("datetime");


            builder.Property(e => e.FechaBloqueo)
                .HasColumnType("datetime");


            builder.Property(e => e.UserName)
                 .IsRequired()
                 .HasMaxLength(50)
                 .IsUnicode(false);

        }
    }
}
