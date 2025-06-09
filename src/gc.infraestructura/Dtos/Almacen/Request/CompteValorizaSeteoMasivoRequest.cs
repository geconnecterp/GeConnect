
namespace gc.infraestructura.Dtos.Almacen.Request
{
	public class CompteValorizaSeteoMasivoRequest
	{
		public decimal plista { get; set; } = 0.00M;
		public decimal dto1 { get; set; } = 0.00M;
		public decimal dto2 { get; set; } = 0.00M;
		public decimal dto3 { get; set; } = 0.00M;
		public decimal dto4 { get; set; } = 0.00M;
		public decimal dtodpa { get; set; } = 0.00M;
		public string boni { get; set; } = string.Empty;
		public string[] idsProductos { get; set; } = [];
		public int seccion { get; set; } = 0; //1- Precio // 2- Factura
		public bool aplica_oc { get; set; }
		public bool aplica_fac { get; set; }
		public bool plista_bool { get; set; }
		public bool dto1_bool { get; set; }
		public bool dto2_bool { get; set; }
		public bool dto3_bool { get; set; }
		public bool dto4_bool { get; set; }
		public bool dtoPa_bool { get; set; }
		public bool boni_bool { get; set; }
	}
}
