namespace gc.infraestructura.Dtos.Almacen.Tr.Transferencia
{
	public class TRProductoParaAgregar : Dto
	{
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public decimal stk_adm { get; set; } = 0;
        public decimal stk { get; set; } = 0;
		public string box_id { get; set; } = string.Empty;
		public string depo_id { get; set; } = string.Empty;
		public string depo_nombre { get; set; } = string.Empty;
		public string fv { get; set; } = string.Empty;
		public decimal unidad_palet { get; set; } = 0;
    }
}
