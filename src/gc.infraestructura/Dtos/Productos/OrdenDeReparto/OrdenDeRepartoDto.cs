using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Productos.OrdenDeReparto
{
	public class OrdenDeRepartoListaDto : OrdenDeRepartoDto
	{
		public int Total_registros { get; set; } = 0;
		public int Total_paginas { get; set; } = 0;
	}
	public class OrdenDeRepartoDto : Dto
	{
		public string or_compte { get; set; } = string.Empty;
		public string or_obs { get; set; } = string.Empty;
		public DateTime or_fecha { get; set; }
		public DateTime or_fecha_fac { get; set; }
		public char ore_id { get; set; }
		public string ore_desc { get; set; } = string.Empty;
		public string rp_id { get; set; } = string.Empty;
		public string rp_nombre { get; set; } = string.Empty;
	}
}
