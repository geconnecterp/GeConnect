using System;

namespace gc.api.core.Entidades
{
    public partial class Acceso : EntidadBase
    {
        public Acceso()
        {
            Usuario = new Usuarios();
        }

        public long Id { get; set; }
        public Guid UsuarioId { get; set; }
        public DateTime Fecha { get; set; }
        public string? IP { get; set; }
        public char TipoAcceso { get; set; }

        public virtual Usuarios Usuario { get; set; }


    }
}
