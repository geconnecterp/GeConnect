using gc.api.core.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace geco_0000.Infraestructura.Datos.Configuraciones
{
    public class AdministracionConfiguration : IEntityTypeConfiguration<Administracion>
    {
        public void Configure(EntityTypeBuilder<Administracion> builder)
        {
            builder.ToTable("administraciones","dbo");

            builder.HasKey(e => e.Adm_id);


            builder.Property(e => e.Adm_nombre)
                 .IsRequired()
                 .HasMaxLength(40)
                 .IsUnicode(false);


            builder.Property(e => e.Adm_direccion)
                 .IsRequired()
                 .HasMaxLength(150)
                 .IsUnicode(false);


            builder.Property(e => e.Usu_id_encargado)
                 .IsRequired()
                 .HasMaxLength(10)
                 .IsUnicode(false);


            builder.Property(e => e.Adm_nro_lote)
                .IsRequired()
                .HasColumnType("int");


            builder.Property(e => e.Adm_nro_lote_central)
                .IsRequired()
                .HasColumnType("int");


            builder.Property(e => e.Cx_profile)
                 .HasMaxLength(100)
                 .IsUnicode(false);


            builder.Property(e => e.Cx_base)
                 .HasMaxLength(60)
                 .IsUnicode(false);


            builder.Property(e => e.Cx_login)
                 .HasMaxLength(60)
                 .IsUnicode(false);


            builder.Property(e => e.Cx_pass)
                 .HasMaxLength(60)
                 .IsUnicode(false);


            builder.Property(e => e.Lote_f)
                .HasColumnType("datetime");


            builder.Property(e => e.Lote_central_f)
                .HasColumnType("datetime");


            builder.Property(e => e.Adm_oc_limite)
                .IsRequired()
                .HasPrecision(10,2);

            builder.Property(e => e.Adm_MePa_Id)
                .HasMaxLength(15)
                .IsUnicode(false);

        }
    }
}
