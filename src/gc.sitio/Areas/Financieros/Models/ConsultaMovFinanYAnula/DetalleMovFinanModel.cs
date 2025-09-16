using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Financieros.Models
{
	public class DetalleMovFinanModel
	{
		public GridCoreSmart<FinancieroTraRepoDDto> GrillaOrigen { get; set; }
		public decimal TotalOrigen { get; set; } = 0.00M;
		public bool MostrarSeccionGrillaOrigen { get; set; } = false;
		public GridCoreSmart<FinancieroTraRepoDDto> GrillaDestino { get; set; }
		public decimal TotalDestino { get; set; } = 0.00M;
		public bool MostrarSeccionGrillaDestino { get; set; } = false;
		public GridCoreSmart<FinancieroTraRepoCtagDto> GrillaCtag { get; set; }
		public decimal TotalCtag { get; set; } = 0.00M;
		public bool MostrarSeccionGrillaCtag { get; set; } = false;
	}
}
