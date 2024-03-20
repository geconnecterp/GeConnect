using System;
using System.Collections.Generic;
using System.Text;

namespace gc.infraestructura.Dtos
{
    public partial class RolDto : Dto
    {
        public RolDto()
        {
            Nombre = string.Empty;
        }
        public Guid Id { get; set; }
        public string Nombre { get; set; }

    }
}
