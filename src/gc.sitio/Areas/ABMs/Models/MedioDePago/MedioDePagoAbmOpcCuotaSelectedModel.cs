namespace gc.sitio.Areas.ABMs.Models
{
	public class MedioDePagoAbmOpcCuotaSelectedModel
	{
		public OpcionCuotaModel OpcionCuota { get; set; }
		public MedioDePagoAbmOpcCuotaSelectedModel()
		{
			OpcionCuota = new OpcionCuotaModel();
		}
	}
}
