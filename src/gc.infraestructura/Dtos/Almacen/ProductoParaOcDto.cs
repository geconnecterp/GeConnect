
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
		public int P_Orden_Pg { get; set; }
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
		public decimal Cantidad_Total { get; set; } = 0.000M;
		public decimal P_Pcosto_Total { get; set; } = 0.0000M;
		public decimal Paletizado { get; set; } = 0.00M;
	}
}
