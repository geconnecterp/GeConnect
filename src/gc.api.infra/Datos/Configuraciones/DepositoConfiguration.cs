using gc.api.core.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace gc.api.infra.Datos.Configuraciones
{
    public class DepositoConfiguration : IEntityTypeConfiguration<Deposito>
    {
        public void Configure(EntityTypeBuilder<Deposito> builder)
        {
            builder.ToTable("depositos");

            builder.HasKey(e => e.Depo_Id);


            builder.Property(e => e.Depo_Nombre)
                 .IsRequired()
                 .HasMaxLength(40)
                 .IsUnicode(false);


            builder.Property(e => e.Adm_Id)
                 .IsRequired()
                 .HasMaxLength(4)
                 .IsUnicode(false);

        }
    }
}
