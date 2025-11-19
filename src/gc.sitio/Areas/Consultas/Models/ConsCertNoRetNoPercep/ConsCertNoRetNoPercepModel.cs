using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Consultas.Models
{
	public class ConsCertNoRetNoPercepModel
	{
		public SelectList ListaTipoImpuesto { get; set; }
		public string selectedValue { get; set; } = string.Empty;
		public bool CertNoRetencion { get; set; }
		public bool CertNoPercepcion { get; set; }
		public bool NoVencidos { get; set; }
		public bool Vencidos { get; set; }
	}
}
