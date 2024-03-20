using System;
using System.Collections.Generic;
using System.Text;

namespace gc.infraestructura.Dtos
{
    public partial class UsuarioDto:Dto
    {
        public UsuarioDto()
        {
            Contrasena = string.Empty;
            Correo = string.Empty;
            UserName = string.Empty;
        }
        public Guid Id { get; set; }
        public string Contrasena { get; set; }
        public string Correo { get; set; }
        public bool Bloqueado { get; set; }
        public int Intentos { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime? FechaBloqueo { get; set; }
        public string UserName { get; set; }
        public bool EstaLogueado { get; set; }

    }
}
