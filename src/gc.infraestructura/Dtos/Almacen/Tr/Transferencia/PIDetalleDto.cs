namespace gc.infraestructura.Dtos.Almacen.Tr.Transferencia
{
	public class PIDetalleDto : Dto
	{
		public string pi_compte { get; set; } = string.Empty;
		public DateTime pi_fecha { get; set; }
		public string? pi_nota { get; set; }
		public string pie_id { get; set; } = string.Empty;
		public string pie_desc { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string usu_apillidoynombre { get; set; } = string.Empty;
		public string pit_id { get; set; } = string.Empty;
		public string pit_desc { get; set; } = string.Empty;
		public string adm_id_gen { get; set; } = string.Empty;
		public string adm_id_gen_desc { get; set; } = string.Empty;
		public string adm_id_des { get; set; } = string.Empty;
		public string adm_id_nom { get; set; } = string.Empty;
		public int pid_item { get; set; }
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public string? p_id_prov { get; set; }
		public string p_id_barrado { get; set; } = string.Empty;
		public string rub_id { get; set; } = string.Empty;
		public string rub_id_desc { get; set; } = string.Empty;
		public decimal pid_cantidad { get; set; } = 0.000M;
		public decimal pid_enviado { get; set; } = 0.000M;
		public decimal pid_stk { get; set; } = 0.000M;
	}
}
