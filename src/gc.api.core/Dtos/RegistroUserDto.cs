using System;
using System.Collections.Generic;
using System.Text;

namespace gc.api.core.DTOs
{
    public class RegistroUserDto
    {
        public string?User { get; set; }
        public string?Password { get; set; }
        public string?Correo { get; set; }
        public string?Role { get; set; }
    }
}
