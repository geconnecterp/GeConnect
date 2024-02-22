using gc.api.core.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace gc.api.infra.Datos.Configuraciones
{
    public class RolConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Rol");

            builder.HasKey(e => e.Id);


            builder.Property(e => e.Nombre)
                 .IsRequired()
                 .HasMaxLength(20)
                 .IsUnicode(false);

        }
    }
}
