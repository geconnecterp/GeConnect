using System;
using System.Collections.Generic;
using System.Text;

namespace gc.infraestructura.Dtos
{
    public class RegistroUserDto : Dto
    {
        public string?User { get; set; }
        public string?Password { get; set; }
        public string?Correo { get; set; }
        public string?Role { get; set; }
    }
}
