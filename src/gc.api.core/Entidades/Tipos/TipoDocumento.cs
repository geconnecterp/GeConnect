using System;
using System.Collections.Generic;
using System.Text;

namespace gc.api.core.Entidades
{
    public partial class TipoDocumento : EntidadBase
    {
        public TipoDocumento()
        {
            Tdoc_Id = string.Empty;
            Tdoc_Desc = string.Empty;
        }

        public string Tdoc_Id { get; set; }
        public string Tdoc_Desc { get; set; }


        //public virtual ICollection<cuentas> Cuentass { get; set; }

    }
}
