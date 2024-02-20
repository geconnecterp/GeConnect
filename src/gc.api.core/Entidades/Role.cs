using System;
using System.Collections.Generic;

namespace gc.api.core.Entidades
{
    public class Role:EntidadBase
    {
        public Role()
        {
            Autorizados = new HashSet<Autorizado>();
        }

        public Guid Id { get; set; }
        public string?Nombre { get; set; }

        public virtual ICollection<Autorizado> Autorizados{ get; set; }

    }
}