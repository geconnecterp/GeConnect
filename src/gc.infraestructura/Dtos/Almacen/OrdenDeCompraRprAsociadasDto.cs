
namespace gc.infraestructura.Dtos.Almacen
{
    public class OrdenDeCompraRprAsociadasDto : Dto
    {
		public string oc_compte { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string p_id_prov { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public DateTime oc_fecha { get; set; }
		public int oc_entrega_dias { get; set; }
		public DateTime oc_entrega_fecha { get; set; }
		public string usu_id { get; set; } = string.Empty;
		public char oc_pago_ant { get; set; }
		public DateTime? oc_pago_ant_vto { get; set; }
		public char? oce_id { get; set; }
		public string? oce_desc { get; set; }
		public string adm_id { get; set; } = string.Empty;
		public int ocd_item { get; set; }
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public int ocd_bultos { get; set; }
		public int ocd_unidad_pres { get; set; }
		public int ocd_cantidad { get; set; }
		public decimal ocd_plista { get; set; } = 0.00M;
		public decimal ocd_dto1 { get; set; } = 0.00M;
		public decimal ocd_dto2 { get; set; } = 0.00M;
		public decimal ocd_dto3 { get; set; } = 0.00M;
		public decimal ocd_dto4 { get; set; } = 0.00M;
		public decimal ocd_dto_pa { get; set; } = 0.00M;
		public string? ocd_boni { get; set; }
		public int ocd_bonificacion { get; set; }
		public decimal ocd_pcosto { get; set; } = 0.00M;
		public decimal ocd_pcosto_tot { get; set; } = 0.00M;
		public string? rp_compte { get; set; } = string.Empty;
		public decimal? rpd_cantidad { get; set; } = 0.00M;
		public decimal? rpd_cantidad_compte { get; set; } = 0.00M;
		public string? up_tipo { get; set; } = string.Empty;
		private decimal _dif_oc_rpr;

		public decimal dif_oc_rpr
		{
			get 
			{
				if (rpd_cantidad == null)
					return ocd_cantidad;
				return ocd_cantidad-rpd_cantidad.Value; 
			}
			set { _dif_oc_rpr = value; }
		}

	}
}
