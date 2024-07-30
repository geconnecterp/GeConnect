using System;
using System.Collections.Generic;
using System.Text;

namespace gc.api.core.Entidades
{
    public partial class Rubro : EntidadBase
    {
        public Rubro()
        {
            Rub_Id = string.Empty;
            Rub_Desc = string.Empty;
            Rubg_Id = string.Empty;

            Productos = new HashSet<Producto>();
        }

        public string Rub_Id { get; set; }
        public string Rub_Desc { get; set; }
        public string Rubg_Id { get; set; }
        public char Rub_Feteado { get; set; }
        public char Rub_Ctlstk { get; set; }
        public char Rub_Actu { get; set; }


        public virtual ICollection<Producto> Productos { get; set; }

    }
}
