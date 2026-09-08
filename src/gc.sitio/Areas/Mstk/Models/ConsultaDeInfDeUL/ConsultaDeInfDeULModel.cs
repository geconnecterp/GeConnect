namespace gc.sitio.Areas.Mstk.Models
{
	public class ConsultaDeInfDeULModel
	{
		public DateTime FechaDesde { get; set; }
		public DateTime FechaHasta { get; set; }
		public bool UL_Por_Fecha { get; set; } = true;
		public bool UL_Sin_Almacen { get; set; }
		public string TipoUL { get; set; } = "FECHA";
	}
}
