using gc.api.core.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace gc.api.infra.Datos.Configuraciones
{
    public class BilleteraConfiguracionConfiguration : IEntityTypeConfiguration<BilleteraConfiguracion>
    {
        public void Configure(EntityTypeBuilder<BilleteraConfiguracion> builder)
        {
            builder.ToTable("billetera_configuracion");

            builder.HasKey(e => e.Bc_Id);


            builder.Property(e => e.Bc_Url_Base_Notificacion)
                 .IsRequired()
                 .HasMaxLength(50)
                 .IsUnicode(false);


            builder.Property(e => e.Bc_Url_Base_Servicio)
                 .HasMaxLength(50)
                 .IsUnicode(false);


            builder.Property(e => e.Bc_Ruta_Publickey)
                 .HasMaxLength(50)
                 .IsUnicode(false);


            builder.Property(e => e.Bc_Ruta_Privatekey)
                 .HasMaxLength(50)
                 .IsUnicode(false);

        }
    }
}
