namespace gc.infraestructura.Dtos.Almacen.Tr.Transferencia
{
	public class PIDetalleDto : Dto, IProductoConUnidad
	{
		public string pi_compte { get; set; } = string.Empty;
		public DateTime pi_fecha { get; set; }
		public string? pi_nota { get; set; }
		public string pie_id { get; set; } = string.Empty;
		public string pie_desc { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
		public string pit_id { get; set; } = string.Empty;
		public string pit_desc { get; set; } = string.Empty;
		public string adm_id_gen { get; set; } = string.Empty;
		public string adm_id_gen_nombre { get; set; } = string.Empty; //Origen
		public string adm_id_des { get; set; } = string.Empty;
		public string adm_id_des_nombre { get; set; } = string.Empty; //Destino
		public int pid_item { get; set; }
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public string? p_id_prov { get; set; }
		public string p_id_barrado { get; set; } = string.Empty;
		public string rub_id { get; set; } = string.Empty;
		public string rub_desc { get; set; } = string.Empty;
		public decimal pid_cantidad { get; set; } = 0.000M;
		public decimal pid_enviado { get; set; } = 0.000M;
		public decimal pid_stk { get; set; } = 0.000M;
		public string up_id { get; set; } = string.Empty;
		public string up_desc { get; set; } = string.Empty;
		public string up_tipo { get; set; } = string.Empty;
		public bool PermiteDecimales => up_tipo == "P";
		public int unidad_pres { get; set; }
		public decimal stk_dest_salon { get; set; }
		public decimal stk_dest { get; set; }
	}
}
