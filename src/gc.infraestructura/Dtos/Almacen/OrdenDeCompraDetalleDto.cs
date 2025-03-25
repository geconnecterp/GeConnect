
namespace gc.infraestructura.Dtos.Almacen
{
    public class OrdenDeCompraDetalleDto : Dto
    {
		public string oc_compte { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public DateTime oc_fecha { get; set; }
		public int oc_entrega_dias { get; set; }
		public DateTime oc_entrega_fecha { get; set; }
		public string usu_id { get; set; } = string.Empty;
		public char oc_pago_ant { get; set; }
		public DateTime? oc_pago_ant_vto { get; set; }
		public decimal oc_dto { get; set; } = 0.00M;
		public decimal oc_dto_porc { get; set; } = 0.00M;
		public decimal oc_flete_porc { get; set; } = 0.00M;
		public decimal oc_flete_importe { get; set; } = 0.00M;
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public string oc_observaciones { get; set; } = string.Empty;
		public decimal oc_gravado { get; set; } = 0.00M;
		public decimal oc_no_gravado { get; set; } = 0.00M;
		public decimal oc_exento { get; set; } = 0.00M;
		public decimal oc_percepciones { get; set; } = 0.00M;
		public char oce_id { get; set; }
		public string oce_desc { get; set; } = string.Empty;
		public int ocd_item { get; set; }
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public int ocd_unidad_x_bulto { get; set; }
		public int ocd_cantidad { get; set; }
		public decimal ocd_plista { get; set; } = 0.00M;
		public decimal ocd_dto1 { get; set; } = 0.00M;
		public decimal ocd_dto2 { get; set; } = 0.00M;
		public decimal ocd_dto3 { get; set; } = 0.00M;
		public decimal ocd_dto4 { get; set; } = 0.00M;
		public string ocd_boni { get; set; } = string.Empty;
		public int ocd_bonificacion { get; set; }
		public decimal ocd_pcosto { get; set; } = 0.00M;
		public decimal ocd_pcosto_tot { get; set; } = 0.00M;
		public decimal in_alicuota { get; set; } = 0.00M;
		public decimal iva_alicuota { get; set; } = 0.00M;
		public decimal ocd_iva { get; set; } = 0.00M;
		public decimal ocd_in { get; set; } = 0.00M;
		public string cta_denominacion { get; set; } = string.Empty;
		public int ocd_unidad_pres { get; set; }
		public int ocd_bultos { get; set; }
		public decimal ocd_dto_pa { get; set; } = 0.00M;
		public decimal oc_iva { get; set; } = 0.00M;
		public decimal oc_in { get; set; } = 0.00M;
		public decimal oc_flete_iva { get; set; } = 0.00M;
		public string p_id_prov { get; set; } = string.Empty;
		public string up_id { get; set; } = string.Empty;
		public string up_desc { get; set; } = string.Empty;
	}
}
