
namespace gc.sitio.Areas.Financieros.Models
{
	public class ProyeccionDeGastoSeleccionadaModel
	{
		public int itemsProyeccion { get; set; }
		public DateTime FechaProyeccion { get; set; }
		public string ConceptoProyeccion { get; set; } = string.Empty;
		public decimal ImporteProyeccion { get; set; } = 0.00M;
	}
}
