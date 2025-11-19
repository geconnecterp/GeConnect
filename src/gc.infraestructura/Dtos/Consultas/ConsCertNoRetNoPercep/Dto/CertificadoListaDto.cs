
namespace gc.infraestructura.Dtos.Consultas.ConsCertNoRetNoPercep
{
	public class CertificadoListaDto : Dto
	{
		public int total_registros { get; set; }
		public int total_paginas { get; set; }
		public string grupo { get; set; } = string.Empty;
		public string grupo_des { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string tdoc_id { get; set; } = string.Empty;
		public string cta_documento { get; set; } = string.Empty;
		public string cta_domicilio { get; set; } = string.Empty;
		public string cta_celu { get; set; } = string.Empty;
		public string cta_email { get; set; } = string.Empty;
		public string cta_te { get; set; } = string.Empty;
		public DateTime? cert_vto { get; set; }
		public DateTime? hoy { get; set; }
	}
}
