
using Newtonsoft.Json;

namespace gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra
{
	public class CompteValorizaDetalleRprListaDto : Dto, ICloneable
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
		public char nc_genera { get; set; }
		public bool valorizacion_mostrar_dc { get; set; } = false;
		public bool valorizacion_mostrar_dp { get; set; } = false;
		public string valorizacion_class_dc { get; set; } = string.Empty;
		public string valorizacion_class_dp { get; set; } = string.Empty;
		public string valorizacion_value_dc { get; set; } = string.Empty;
		public string valorizacion_value_dp { get; set; } = string.Empty;

		public static decimal CalcularPCosto(decimal p_plista, decimal p_d1, decimal p_d2, decimal p_d3, decimal p_d4, decimal p_dpa, string p_boni, decimal flete = 0)
		{
			var arr = p_boni.Split('/');
			var boni = 1.0M;
			if (arr.Length == 2)
			{
				if (decimal.TryParse(arr[1], out decimal val1) && decimal.TryParse(arr[0], out decimal val0))
					boni = val1 / val0;
			}
			return p_plista * ((100 - p_d1) / 100) * ((100 - p_d2) / 100) * ((100 - p_d3) / 100) * ((100 - p_d4) / 100) * ((100 - p_dpa) / 100) * boni * ((100 + flete) / 100);
		}

		public object Clone()
		{
			return (CompteValorizaDetalleRprListaDto)MemberwiseClone();
		}

		private bool _nc_genera_bool;

		public bool nc_genera_bool
		{
			get { return nc_genera == 'N' ? false : true; }
			set { _nc_genera_bool = value; }
		}
	}
}
