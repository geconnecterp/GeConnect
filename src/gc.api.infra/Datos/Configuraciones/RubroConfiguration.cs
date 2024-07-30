using gc.api.core.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace gc.api.infra.Datos.Configuraciones
{
    public class RubroConfiguration : IEntityTypeConfiguration<Rubro>
    {
        public void Configure(EntityTypeBuilder<Rubro> builder)
        {
            builder.ToTable("rubros");

            builder.HasKey(e => e.Rub_Id);


            builder.Property(e => e.Rub_Desc)
                 .IsRequired()
                 .HasMaxLength(100)
                 .IsUnicode(false);


            builder.Property(e => e.Rubg_Id)
                 .IsRequired()
                 .HasMaxLength(4)
                 .IsUnicode(false);

        }
    }
}
