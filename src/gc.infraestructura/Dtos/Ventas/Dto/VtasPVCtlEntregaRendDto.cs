
namespace gc.infraestructura.Dtos.Ventas
{
	public class VtasPVCtlEntregaRendDto : Dto
	{
		public string ent_compte { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public string caja_nro_proceso { get; set; } = string.Empty;
		public int caja_nro_cierre { get; set; }
		public int caja_nro_rend { get; set; }
		public int rend_item { get; set; }
		public string caja_id { get; set; } = string.Empty;
		public char rend_tipo { get; set; }
		public DateTime rend_fecha { get; set; }
		public string ins_id { get; set; } = string.Empty;
		public decimal rend_importe_ok { get; set; }
		public decimal rend_importe_arq { get; set; }
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
		public char rend_estado { get; set; }
		public char rend_entrega { get; set; }
		public bool rend_estado_bool => rend_estado == 'S';
		public bool rend_entrega_bool => rend_entrega == 'S';
		public char ent_estado { get; set; }
		public bool ent_estado_bool => ent_estado == 'P';
		public bool editado { get; set; } = false;
	}
}
