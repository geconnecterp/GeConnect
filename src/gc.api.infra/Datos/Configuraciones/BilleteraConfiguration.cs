using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using gc.api.core.Entidades;

namespace gc.api.infra.Datos.Configuraciones
{
    public class BilleteraConfiguration : IEntityTypeConfiguration<Billetera>
    {
        public void Configure(EntityTypeBuilder<Billetera> builder)
        {
            builder.ToTable("billeteras");

            builder.HasKey(e => e.Bill_id);


            builder.Property(e => e.Bill_desc)
                 .IsRequired()
                 .HasMaxLength(20)
                 .IsUnicode(false);

        }
    }
}
