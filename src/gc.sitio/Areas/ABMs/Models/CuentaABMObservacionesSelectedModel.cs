using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ABMs.Models
{
	public class CuentaABMObservacionesSelectedModel
	{
		public SelectList ComboTipoObs { get; set; }
		public ObservacionesModel Observacion { get; set; }
		public CuentaABMObservacionesSelectedModel()
		{ 
			Observacion = new ObservacionesModel();
		}
	}
}
