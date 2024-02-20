using System;
using System.Collections.Generic;
using System.Text;

namespace gc.api.core.Entidades
{
    public partial class AuditoriaUsuario : EntidadBase
    {
        public AuditoriaUsuario()
        {
            Usuario = new Usuario();
        }

        public int Id { get; set; }
        public Guid UsuarioId { get; set; }
        public DateTime FechaAuditoria { get; set; }
        public string? IP { get; set; }
        public string? MetodoAccedido { get; set; }

        public virtual Usuario Usuario { get; set; }


    }
}
