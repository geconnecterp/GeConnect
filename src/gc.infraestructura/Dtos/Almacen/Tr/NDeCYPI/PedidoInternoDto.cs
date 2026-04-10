using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Almacen.Tr
{
	public class PedidoInternoListaDto : PedidoInternoDto
	{
		public int Total_registros { get; set; } = 0;
		public int Total_paginas { get; set; } = 0;
	}

	public class PedidoInternoDto : Dto
	{
		public string pi_compte { get; set; } = string.Empty;
		public DateTime pi_fecha { get; set; }
		public string adm_id_gen { get; set; } = string.Empty;
		public string adm_id_gen_nombre { get; set; } = string.Empty;
		public string adm_id_des { get; set; } = string.Empty;
		public string adm_id_des_nombre { get; set; } = string.Empty;
		public string pie_id { get; set; } = string.Empty;
		public string pie_desc { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
	}
}
