
using gc.infraestructura.Dtos.Almacen;

namespace gc.infraestructura.Dtos
{
	public class InvRepoStkVsConteoDto : Dto, IProductoConUnidad
	{
		public string inv_nro { get; set; } = string.Empty;
		public string p_id { get; set; } = string.Empty;
		public string p_des { get; set; } = string.Empty;
		public string box_desc { get; set; } = string.Empty;
		public string up_id { get; set; } = string.Empty;
		public string up_desc { get; set; } = string.Empty;
		public string up_tipo { get; set; } = string.Empty;
		public decimal ps_stk { get; set; } = 0.000M;
		public decimal conteo1 { get; set; } = 0.000M;
		public decimal conteo2 { get; set; } = 0.000M;
		public decimal ps_conteo { get; set; } = 0.000M;
		public char ps_ajuste { get; set; }
		public char invt_id { get; set; }
		public string invt_desc { get; set; } = string.Empty;
		public char inve_id { get; set; }
		public string inve_desc { get; set; } = string.Empty;
		public bool PermiteDecimales => up_tipo == "P";
	}
}
