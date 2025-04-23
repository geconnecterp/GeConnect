
namespace gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra
{
	public class CompteValorizaDetalleRprListaDto : Dto
	{
		public string rp_compte { get; set; } = string.Empty;
		public int rpd_item { get; set; }
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public decimal in_alicuota { get; set; } = 0.00M;
		public decimal iva_alicuota { get; set; } = 0.00M;
		public char iva_situacion { get; set; }
		public string up_id { get; set; } = string.Empty;
		public char up_tipo { get; set; }
		public string? oc_compte { get; set; }
		public decimal ocd_plista { get; set; } = 0.00M;
		public decimal ocd_dto1 { get; set; } = 0.00M;
		public decimal ocd_dto2 { get; set; } = 0.00M;
		public decimal ocd_dto3 { get; set; } = 0.00M;
		public decimal ocd_dto4 { get; set; } = 0.00M;
		public decimal ocd_dto_pa { get; set; } = 0.00M;
		public string? ocd_boni { get; set; } = string.Empty;
		public int ocd_bonificacion { get; set; }
		public decimal ocd_pcosto { get; set; } = 0.00M;
		public int rpd_unidad_pres { get; set; }
		public int rpd_bulto_recibidos { get; set; }
		public decimal rpd_unidad_suelta { get; set; } = 0.000M;
		public decimal rpd_cantidad { get; set; } = 0.000M;
		public decimal rpd_plista { get; set; } = 0.00M;
		public decimal rpd_dto1 { get; set; } = 0.00M;
		public decimal rpd_dto2 { get; set; } = 0.00M;
		public decimal rpd_dto3 { get; set; } = 0.00M;
		public decimal rpd_dto4 { get; set; } = 0.00M;
		public decimal rpd_dto_pa { get; set; } = 0.00M;
		public string? rpd_boni { get; set; } = string.Empty;
		public int rpd_bonificacion { get; set; }
		public decimal rpd_pcosto { get; set; } = 0.00M;
		public int rpd_bulto_compte { get; set; }
		public decimal rpd_cantidad_compte { get; set; } = 0.000M;
		public decimal nc_cantidad_difp { get; set; } = 0.000M;
		public decimal nc_pcosto_difp { get; set; } = 0.000M;
		public decimal nc_cantidad_difc { get; set; } = 0.000M;
		public decimal nc_pcosto_difc { get; set; } = 0.000M;
	}
}
