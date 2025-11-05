
namespace gc.infraestructura.EntidadesComunes.Options
{
	public class ImportacionLiquidacionDeEmpleado
	{
		public ImportacionLiquidacionDeEmpleado()
		{
			Formatos = new();
		}
		public List<FormatoExtractoConfig> Formatos { get; set; } = new();
	}
}
