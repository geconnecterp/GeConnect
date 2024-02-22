using gc.api.core.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeConnectDB.Infraestructura.Datos.Configuraciones
{
    public class TipoDocumentoConfiguration : IEntityTypeConfiguration<TipoDocumento>
    {
        public void Configure(EntityTypeBuilder<TipoDocumento> builder)
        {
            builder.ToTable("TipoDocumento");

            builder.HasKey(e => e.Id);


            builder.Property(e => e.Descripcion)
                 .IsRequired()
                 .HasMaxLength(10)
                 .IsUnicode(false);


            builder.Property(e => e.Hasar)
                 .HasMaxLength(2)
                 .IsUnicode(false);


            builder.Property(e => e.Epson)
                 .HasMaxLength(4)
                 .IsUnicode(false);

        }
    }
}
