using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using gc.api.core.Entidades;

namespace gc.api.infra.Datos.Configuraciones
{
    public class ProductoDepositoConfiguration : IEntityTypeConfiguration<ProductoDeposito>
    {
        public void Configure(EntityTypeBuilder<ProductoDeposito> builder)
        {
            builder.ToTable("productos_depositos");

            builder.HasKey(e => e.P_Id);


            builder.HasKey(e => e.Box_Id);


            builder.Property(e => e.Depo_Id)
                 .IsRequired()
                 .HasMaxLength(2)
                 .IsUnicode(false);


            builder.Property(e => e.Ps_Stk)
                .IsRequired()
                .HasPrecision(10,2);


            builder.Property(e => e.Ps_Stk_B)
                .IsRequired()
                .HasPrecision(10,2);


            builder.Property(e => e.Ps_Fv)
                 .HasMaxLength(10)
                 .IsUnicode(false);


            builder.Property(e => e.Ps_Transito)
                .IsRequired()
                .HasPrecision(10,2);

        }
    }
}
