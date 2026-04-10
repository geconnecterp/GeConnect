using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Almacen.Tr
{
	public class PedidoInternoRequest
	{
		public DateTime fecha_d { get; set; }
		public DateTime fecha_h { get; set; }
		public bool adm { get; set; }
		public string adm_list { get; set; }
		public bool estado { get; set; }
		public string estado_list { get; set; }
		public int Registros { get; set; }
		public int Pagina { get; set; }
	}
}
