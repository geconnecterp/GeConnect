
namespace gc.infraestructura.Dtos.Almacen
{
    public class ProductoParaOcDto : Dto
    {
		public string P_Id { get; set; }=string.Empty;
		public string P_Desc { get; set; } = string.Empty;
		public string P_Id_Prov { get; set; } = string.Empty;
		public string Cta_Id { get; set; } = string.Empty;
		public string Cta_Denominacion { get; set; } = string.Empty;
		public string Pg_Id { get; set; } = string.Empty;
		public string Pg_Desc { get; set; } = string.Empty;
		public int? P_Orden_Pg { get; set; }
		public string Rub_Id { get; set; } = string.Empty;
		public string Rub_Desc { get; set; } = string.Empty;
		public string Up_Id { get; set; } = string.Empty;
		public int P_Unidad_Pres { get; set; }
		public int P_Unidad_Palet { get; set; }
		public int Bultos { get; set; }
		public decimal Cantidad { get; set; } = 0.000M;
		public decimal P_Plista { get; set; } = 0.0000M;
		public decimal P_Dto1 { get; set; } = 0.00M;
		public decimal P_Dto2 { get; set; } = 0.00M;
		public decimal P_Dto3 { get; set; } = 0.00M;
		public decimal P_Dto4 { get; set; } = 0.00M;
		public decimal P_Dto_Pa { get; set; } = 0.00M;
		public string P_Boni { get; set; } = string.Empty;
		public decimal P_Porc_Flete { get; set; } = 0.00M;
		public char Iva_Situacion { get; set; }
		public decimal Iva_Alicuota { get; set; } = 0.00M;
		public decimal In_Alicuota { get; set; } = 0.00M;
		public decimal P_Pcosto { get; set; } = 0.0000M;
		public int Bonificados { get; set; }
		public decimal Pedido_Mas_Boni { get; set; } = 0.0000M;
		public decimal Cantidad_Total { get; set; } = 0.000M;
		public decimal P_Pcosto_Total { get; set; } = 0.0000M;
		public decimal Paletizado { get; set; } = 0.00M;
		public decimal Stk { get; set; } = 0.000M;
		public decimal Stk_Suc { get; set; } = 0.000M;

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
			P_Dto1 = 0;
			P_Dto2 = 0;
			P_Dto3 = 0;
			P_Dto4 = 0;
			P_Dto_Pa = 0;
			P_Boni = string.Empty;
			P_Porc_Flete = item.P_Porc_Flete;
			Iva_Situacion = item.Iva_Situacion;
			Iva_Alicuota = item.Iva_Alicuota;
			In_Alicuota = item.In_Alicuota;
			P_Pcosto = item.costo;
			Bonificados = item.oc_pendiente;
			Pedido_Mas_Boni = item.pedido;
			Cantidad_Total = item.cantidad;
			P_Pcosto_Total = item.costo_total;
			Paletizado = item.paletizado;
			Stk = item.stk;
			Stk_Suc = item.stk_suc;
		}
	}

}
