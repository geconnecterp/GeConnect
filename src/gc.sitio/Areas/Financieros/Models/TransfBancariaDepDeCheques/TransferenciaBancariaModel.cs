using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;

namespace gc.sitio.Areas.Financieros.Models
{
	public class TransferenciaBancariaModel
	{
		public string concepto { get; set; } = string.Empty;
		public DateTime fecha { get; set; } = DateTime.Today;
		public GridCoreSmart<ValoresDesdeObligYCredDto> GrillaOrigen { get; set; }
		public GridCoreSmart<ValoresDesdeObligYCredDto> GrillaDestino { get; set; }
		public string parametro_valores_origen { get; set; } = string.Empty;
		public string parametro_valores_destino { get; set; } = string.Empty;
		public string parametro_confirmacion { get; set; } = string.Empty;
		public decimal total_origen { get; set; } = 0.00M;
		public decimal total_destino { get; set; } = 0.00M;
	}
}
