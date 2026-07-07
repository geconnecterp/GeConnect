using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Cajas.Request
{
    public class NCProductoBuscarRequestDto
    {
        public string tco_id { get; set; }=string.Empty;   
        public string cm_compte { get; set; }= string.Empty;    
        public string cm_repetido { get; set; }= string.Empty;
        public string adm_id { get; set; }= string.Empty;
        public string valor { get; set; }= string.Empty;
        public decimal cantidad { get; set; } = 1;
        public string json_p { get; set; }= string.Empty;
    }
}
