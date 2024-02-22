using System;
using System.Collections.Generic;
using System.Text;

namespace gc.infraestructura.DTOs
{
    public partial class RolDto
    {
        public RolDto()
        {
            Nombre = string.Empty;
        }
        public Guid Id { get; set; }
        public string Nombre { get; set; }

    }
}
