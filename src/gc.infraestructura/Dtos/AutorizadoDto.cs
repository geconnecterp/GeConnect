using System;
using System.Collections.Generic;
using System.Text;

namespace gc.infraestructura.DTOs
{
    public partial class AutorizadoDto
    {
        public AutorizadoDto()
        {
            UsuarioContrasena = string.Empty;
            RolNombre = string.Empty;
        }
        public Guid UsuarioId { get; set; }
        public Guid RoleId { get; set; }

        public string UsuarioContrasena { get; set; }
        public string RolNombre { get; set; }
    }
}
