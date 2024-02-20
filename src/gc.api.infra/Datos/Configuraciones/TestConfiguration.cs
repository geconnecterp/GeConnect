using gc.api.core.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.infra.Datos.Configuraciones
{
    public class TestConfiguration : IEntityTypeConfiguration<Test>
    {
        public void Configure(EntityTypeBuilder<Test> builder)
        {
            builder.ToTable("Test");

            builder.HasKey(t => t.Id);

            builder.Property(e=>e.DatoStr).IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        }
    }
}
