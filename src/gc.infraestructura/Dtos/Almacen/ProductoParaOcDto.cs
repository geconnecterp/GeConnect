
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace gc.infraestructura.Dtos.Almacen
{
	public class ProductoParaOcDto : Dto, IProductoConUnidad
	{
		[JsonProperty(PropertyName = "p_id")]
		public string p_id { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "p_desc")]
		public string p_desc { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "p_id_barrado")]
		public string p_id_barrado { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "p_id_prov")]
		public string p_id_prov { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "cta_id")]
		public string cta_id { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "cta_denominacion")]
		public string cta_denominacion { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "pg_id")]
		public string pg_id { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "pg_desc")]
		public string pg_desc { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "p_orden_pg")]
		public int? p_orden_pg { get; set; }
		[JsonProperty(PropertyName = "rub_id")]
		public string rub_id { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "rub_desc")]
		public string rub_desc { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "up_id")]
		public string up_id { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "p_unidad_pres")]
		public int p_unidad_pres { get; set; }
		[JsonProperty(PropertyName = "p_unidad_palet")]
		public int p_unidad_palet { get; set; }
		[JsonProperty(PropertyName = "bultos")]
		public int bultos { get; set; }
		[JsonProperty(PropertyName = "cantidad")]
		public decimal cantidad { get; set; } = 0.00M;
		[JsonProperty(PropertyName = "p_plista")]
		public decimal p_plista { get; set; } = 0.00M;
		[JsonProperty(PropertyName = "p_dto1")]
		public decimal p_dto1 { get; set; } = 0.0M;
		[JsonProperty(PropertyName = "p_dto2")]
		public decimal p_dto2 { get; set; } = 0.0M;
		[JsonProperty(PropertyName = "p_dto3")]
		public decimal p_dto3 { get; set; } = 0.0M;
		[JsonProperty(PropertyName = "p_dto4")]
		public decimal p_dto4 { get; set; } = 0.0M;
		[JsonProperty(PropertyName = "p_dto_pa")]
		public decimal p_dto_pa { get; set; } = 0.0M;
		[JsonProperty(PropertyName = "p_boni")]
		public string p_boni { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "p_porc_flete")]
		public decimal p_porc_flete { get; set; } = 0.00M;
		[JsonProperty(PropertyName = "iva_situacion")]
		public char iva_situacion { get; set; }
		[JsonProperty(PropertyName = "iva_alicuota")]
		public decimal iva_alicuota { get; set; } = 0.00M;
		[JsonProperty(PropertyName = "in_alicuota")]
		public decimal in_alicuota { get; set; } = 0.00M;
		[JsonProperty(PropertyName = "p_pcosto")]
		public decimal p_pcosto { get; set; } = 0.00M;
		[JsonProperty(PropertyName = "bonificados")]
		public int bonificados { get; set; }
		[JsonProperty(PropertyName = "pedido_mas_boni")]
		public decimal pedido_mas_boni { get; set; } = 0.00M;
		[JsonProperty(PropertyName = "cantidad_total")]
		public decimal cantidad_total { get; set; } = 0.00M;
		[JsonProperty(PropertyName = "p_pcosto_total")]
		public decimal p_pcosto_total { get; set; } = 0.00M;
		[JsonProperty(PropertyName = "paletizado")]
		public decimal paletizado { get; set; } = 0.00M;
		[JsonProperty(PropertyName = "stk")]
		public decimal stk { get; set; } = 0.00M;
		[JsonProperty(PropertyName = "stk_suc")]
		public decimal stk_suc { get; set; } = 0.00M;
		[JsonProperty(PropertyName = "up_tipo")]
		public string up_tipo { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "up_desc")]
		public string up_desc { get; set; } = string.Empty;
		public bool PermiteDecimales => up_tipo == "P";
		public ProductoParaOcDto() { }

		public ProductoParaOcDto(ProductoNCPIDto item)
		{
			p_id = item.p_id;
			p_desc = item.p_desc;
			p_id_prov = item.p_id_prov;
			cta_id = item.cta_id;
			cta_denominacion = item.cta_denominacion;
			pg_id = item.pg_id;
			pg_desc = item.pg_desc;
			p_orden_pg = item.p_orden_pg;
			rub_id = item.rub_id;
			rub_desc = item.rub_desc;
			up_id = item.up_id;
			up_desc = item.up_desc;
			up_tipo = item.up_tipo;
			p_unidad_pres = item.p_unidad_pres;
			p_unidad_palet = item.p_unidad_palet;
			bultos = item.Bultos;
			cantidad = item.cantidad;
			p_plista = item.P_Plista;
			p_dto1 = item.P_Dto1;
			p_dto2 = item.P_Dto2;
			p_dto3 = item.P_Dto3;
			p_dto4 = item.P_Dto4;
			p_dto_pa = item.P_Dto_Pa;
			p_boni = item.P_Boni;
			p_porc_flete = item.P_Porc_Flete;
			iva_situacion = item.Iva_Situacion;
			iva_alicuota = item.Iva_Alicuota;
			in_alicuota = item.In_Alicuota;
			p_pcosto = CalcularPCosto(p_plista, p_dto1, p_dto2, p_dto3, p_dto4, p_dto_pa, p_boni, 0);
			bonificados = item.oc_pendiente;
			pedido_mas_boni = item.pedido;
			cantidad_total = item.cantidad;
			p_pcosto_total = item.costo_total;
			paletizado = item.paletizado;
			stk = item.stk;
			stk_suc = item.stk_suc;
		}

		public static decimal CalcularPCosto(decimal p_plista, decimal p_d1, decimal p_d2, decimal p_d3, decimal p_d4, decimal p_dpa, string p_boni, decimal flete)
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
	}
}