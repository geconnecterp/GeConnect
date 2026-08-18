namespace gc.sitio.Areas.Financieros.Models.CargarExtractoBancario
{
	public class JsonExtractoEliminadoModel
	{
		public string ctaf_id { get; set; } = string.Empty;
		public DateTime ext_fecha { get; set; }
		public DateTime ext_fecha_ori { get; set; }
		public decimal ext_debe { get; set; } = 0.00M;
		public decimal ext_haber { get; set; } = 0.00M;
	}
}
