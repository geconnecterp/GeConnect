using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using gc.api.core.Entidades;

namespace gc.api.infra.Datos.Configuraciones
{
    public class TipoDocumentoConfiguration : IEntityTypeConfiguration<TipoDocumento>
    {
        public void Configure(EntityTypeBuilder<TipoDocumento> builder)
        {
            builder.ToTable("tipos_documentos");

            builder.HasKey(e => e.Tdoc_Id);


            builder.Property(e => e.Tdoc_Desc)
                 .IsRequired()
                 .HasMaxLength(15)
                 .IsUnicode(false);

        }
    }
}
