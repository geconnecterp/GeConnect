using System;
using System.Collections.Generic;
using System.Text;

namespace gc.infraestructura.DTOs
{
    public partial class AuditoriaUsuarioDto
    {
        public AuditoriaUsuarioDto()
        {
            UsuarioContrasena = string.Empty;
            IP = string.Empty;
            MetodoAccedido = string.Empty;
        }
        public int Id { get; set; }
        public Guid UsuarioId { get; set; }
        public DateTime FechaAuditoria { get; set; }
        public string IP { get; set; }
        public string MetodoAccedido { get; set; }

        public string UsuarioContrasena { get; set; }
    }
}
