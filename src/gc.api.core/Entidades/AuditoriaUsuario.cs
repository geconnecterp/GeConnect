using System;
using System.Collections.Generic;
using System.Text;

namespace gc.api.core.Entidades
{
    public partial class AuditoriaUsuario : EntidadBase
    {
        public AuditoriaUsuario()
        {
            Usuario = new Usuarios();
        }

        public int Id { get; set; }
        public Guid UsuarioId { get; set; }
        public DateTime FechaAuditoria { get; set; }
        public string? IP { get; set; }
        public string? MetodoAccedido { get; set; }

        public virtual Usuarios Usuario { get; set; }


    }
}
