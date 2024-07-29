using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using gc.api.core.Entidades;

namespace gc.api.infra.Datos.Configuraciones
{
    public class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
    {
        public void Configure(EntityTypeBuilder<Proveedor> builder)
        {
            builder.ToTable("proveedores");

            builder.HasKey(e => e.Cta_Id);


            builder.Property(e => e.Ctap_Ean)
                 .HasMaxLength(20)
                 .IsUnicode(false);


            builder.Property(e => e.Ctap_Id_Externo)
                 .HasMaxLength(20)
                 .IsUnicode(false);


            builder.Property(e => e.Ctap_Viajante)
                 .HasMaxLength(80)
                 .IsUnicode(false);


            builder.Property(e => e.Ctap_Viajante_Ce)
                 .HasMaxLength(40)
                 .IsUnicode(false);


            builder.Property(e => e.Ctap_Viajante_Email)
                 .HasMaxLength(100)
                 .IsUnicode(false);


            builder.Property(e => e.Ctap_Valores_A_Nombre)
                 .HasMaxLength(80)
                 .IsUnicode(false);


            builder.Property(e => e.Rgan_Id)
                 .HasMaxLength(2)
                 .IsUnicode(false);


            builder.Property(e => e.Rgan_Cert_Vto)
                .HasColumnType("datetime");


            builder.Property(e => e.Rgan_Porc)
                .IsRequired()
                .HasPrecision(10, 2);


            builder.Property(e => e.Rib_Cert_Vto)
                .HasColumnType("datetime");


            builder.Property(e => e.Rib_Porc)
                .IsRequired()
                .HasPrecision(10, 2);


            builder.Property(e => e.Ctap_Ret_Iva_Porc)
                .HasPrecision(10, 2);


            builder.Property(e => e.Ctap_Per_Iva_Ali)
                .IsRequired()
                .HasPrecision(10, 2);


            builder.Property(e => e.Ctap_Per_Ib_Ali)
                .IsRequired()
                .HasPrecision(10, 2);


            builder.Property(e => e.Ctap_D1)
                .HasPrecision(10, 2);


            builder.Property(e => e.Ctap_D2)
                .HasPrecision(10, 2);


            builder.Property(e => e.Ctap_D3)
                .HasPrecision(10, 2);


            builder.Property(e => e.Ctap_D4)
                .HasPrecision(10, 2);


            builder.Property(e => e.Ctap_D5)
                .HasPrecision(10, 2);


            builder.Property(e => e.Ctap_D6)
                .HasPrecision(10, 2);


            builder.Property(e => e.Ope_Iva)
                 .IsRequired()
                 .HasMaxLength(2)
                 .IsUnicode(false);


            builder.Property(e => e.Ctag_Id)
                 .HasMaxLength(8)
                 .IsUnicode(false);


            builder.Property(e => e.Ctap_Obs_Op)
                 .HasMaxLength(255)
                 .IsUnicode(false);


            builder.Property(e => e.Ctap_Obs_Precios)
                 .HasMaxLength(255)
                 .IsUnicode(false);


            builder.Property(e => e.Id_Old)
                 .HasMaxLength(10)
                 .IsUnicode(false);

        }
    }
}
