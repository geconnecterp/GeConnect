using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ABMs.Models
{
	public class CuentaABMObservacionesModel
	{
		public SelectList ComboTipoObs { get; set; }
		public GridCore<CuentaObsDto> CuentaObservaciones { get; set; }
        public ObservacionesModel Observacion { get; set; }

		public CuentaABMObservacionesModel()
		{
			Observacion = new ObservacionesModel();
		}
	}
}
