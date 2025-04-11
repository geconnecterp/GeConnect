
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace gc.infraestructura.Dtos.Almacen
{
	public class ProductoParaOcDto : Dto
	{
		[JsonProperty(PropertyName = "p_id")]
		public string P_Id { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "p_desc")]
		public string P_Desc { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "p_id_prov")]
		public string P_Id_Prov { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "cta_id")]
		public string Cta_Id { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "cta_denominacion")]
		public string Cta_Denominacion { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "pg_id")]
		public string Pg_Id { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "pg_desc")]
		public string Pg_Desc { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "p_orden_pg")]
		public int? P_Orden_Pg { get; set; }
		[JsonProperty(PropertyName = "rub_id")]
		public string Rub_Id { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "rub_desc")]
		public string Rub_Desc { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "up_id")]
		public string Up_Id { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "p_unidad_pres")]
		public int P_Unidad_Pres { get; set; }
		[JsonProperty(PropertyName = "p_unidad_palet")]
		public int P_Unidad_Palet { get; set; }
		[JsonProperty(PropertyName = "bultos")]
		public int Bultos { get; set; }
		[JsonProperty(PropertyName = "cantidad")]
		public decimal Cantidad { get; set; } = 0.00M;
		[JsonProperty(PropertyName = "p_plista")]
		public decimal P_Plista { get; set; } = 0.00M;
		[JsonProperty(PropertyName = "p_dto1")]
		public decimal P_Dto1 { get; set; } = 0.0M;
		[JsonProperty(PropertyName = "p_dto2")]
		public decimal P_Dto2 { get; set; } = 0.0M;
		[JsonProperty(PropertyName = "p_dto3")]
		public decimal P_Dto3 { get; set; } = 0.0M;
		[JsonProperty(PropertyName = "p_dto4")]
		public decimal P_Dto4 { get; set; } = 0.0M;
		[JsonProperty(PropertyName = "p_dto_pa")]
		public decimal P_Dto_Pa { get; set; } = 0.0M;
		[JsonProperty(PropertyName = "p_boni")]
		public string P_Boni { get; set; } = string.Empty;
		[JsonProperty(PropertyName = "p_porc_flete")]
		public decimal P_Porc_Flete { get; set; } = 0.00M;
		[JsonProperty(PropertyName = "iva_situacion")]
		public char Iva_Situacion { get; set; }
		[JsonProperty(PropertyName = "iva_alicuota")]
		public decimal Iva_Alicuota { get; set; } = 0.00M;
		[JsonProperty(PropertyName = "in_alicuota")]
		public decimal In_Alicuota { get; set; } = 0.00M;
		[JsonProperty(PropertyName = "p_pcosto")]
		public decimal P_Pcosto { get; set; } = 0.00M;
		[JsonProperty(PropertyName = "bonificados")]
		public int Bonificados { get; set; }
		[JsonProperty(PropertyName = "pedido_mas_boni")]
		public decimal Pedido_Mas_Boni { get; set; } = 0.00M;
		[JsonProperty(PropertyName = "cantidad_total")]
		public decimal Cantidad_Total { get; set; } = 0.00M;
		[JsonProperty(PropertyName = "p_pcosto_total")]
		public decimal P_Pcosto_Total { get; set; } = 0.00M;
		[JsonProperty(PropertyName = "paletizado")]
		public decimal Paletizado { get; set; } = 0.00M;
		[JsonProperty(PropertyName = "stk")]
		public decimal Stk { get; set; } = 0.00M;
		[JsonProperty(PropertyName = "stk_suc")]
		public decimal Stk_Suc { get; set; } = 0.00M;

		public ProductoParaOcDto() { }

		public ProductoParaOcDto(ProductoNCPIDto item)
		{
			P_Id = item.p_id;
			P_Desc = item.p_desc;
			P_Id_Prov = item.p_id_prov;
			Cta_Id = item.cta_id;
			Cta_Denominacion = item.cta_denominacion;
			Pg_Id = item.pg_id;
			Pg_Desc = item.pg_desc;
			P_Orden_Pg = item.p_orden_pg;
			Rub_Id = item.rub_id;
			Rub_Desc = item.rub_desc;
			Up_Id = item.up_id;
			P_Unidad_Pres = item.p_unidad_pres;
			P_Unidad_Palet = item.p_unidad_palet;
			Bultos = item.Bultos;
			Cantidad = item.cantidad;
			P_Plista = item.P_Plista;
			P_Dto1 = item.P_Dto1;
			P_Dto2 = item.P_Dto2;
			P_Dto3 = item.P_Dto3;
			P_Dto4 = item.P_Dto4;
			P_Dto_Pa = item.P_Dto_Pa;
			P_Boni = item.P_Boni;
			P_Porc_Flete = item.P_Porc_Flete;
			Iva_Situacion = item.Iva_Situacion;
			Iva_Alicuota = item.Iva_Alicuota;
			In_Alicuota = item.In_Alicuota;
			P_Pcosto = CalcularPCosto(P_Plista, P_Dto1, P_Dto2, P_Dto3, P_Dto4, P_Dto_Pa, P_Boni, 0);
			Bonificados = item.oc_pendiente;
			Pedido_Mas_Boni = item.pedido;
			Cantidad_Total = item.cantidad;
			P_Pcosto_Total = item.costo_total;
			Paletizado = item.paletizado;
			Stk = item.stk;
			Stk_Suc = item.stk_suc;
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