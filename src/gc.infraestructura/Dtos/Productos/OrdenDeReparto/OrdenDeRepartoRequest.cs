using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Productos.OrdenDeReparto
{
	public class OrdenDeRepartoRequest
	{
		public int Registros { get; set; }
		public int Pagina { get; set; }

		public DateTime Desde { get; set; }
		public DateTime Hasta { get; set; }
		public string? ore_list { get; set; }
		public string? rp_list { get; set; }
		public bool id { get; set; } = false;
		public string or_compte { get; set; } = string.Empty;
	}
}
