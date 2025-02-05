using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.ABMs.Models
{
	public class MedioDePagoAbmOpcCuotaModel
	{
		public OpcionCuotaModel OpcionCuota { get; set; }
		public GridCore<OpcionCuotaDto> ListaOpcionesCuota { get; set; }
		public MedioDePagoAbmOpcCuotaModel()
		{
			OpcionCuota = new OpcionCuotaModel();
		}
	}
}
