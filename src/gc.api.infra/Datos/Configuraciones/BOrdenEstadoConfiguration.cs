using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using gc.api.core.Entidades;

namespace gc.api.infra.Datos.Configuraciones
{
    public class BOrdenEstadoConfiguration : IEntityTypeConfiguration<BOrdenEstado>
    {
        public void Configure(EntityTypeBuilder<BOrdenEstado> builder)
        {
            builder.ToTable("billeteras_ordenes_e");

            builder.HasKey(e => e.Boe_id);


            builder.Property(e => e.Boe_desc)
                 .IsRequired()
                 .HasMaxLength(20)
                 .IsUnicode(false);

        }
    }
}
