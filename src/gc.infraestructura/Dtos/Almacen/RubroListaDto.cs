using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Almacen
{
    public class RubroListaDto
    {
        public RubroListaDto()
        {
            Rub_Desc=string.Empty;
            Rub_Id =string.Empty;
        }
        public string Rub_Id { get; set; }
        public string Rub_Desc { get; set; }
    }
}
