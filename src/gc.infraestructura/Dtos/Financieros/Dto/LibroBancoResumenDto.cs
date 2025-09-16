
namespace gc.infraestructura.Dtos.Financieros
{
	public class LibroBancoResumenDto : Dto
	{
		public string descripcion { get; set; } = string.Empty;
		public string saldo { get; set; } = string.Empty;
		public bool es_fuente_negrita { get; set; }
		public string background { get; set; } = string.Empty;
		public bool es_header_1 { get; set; } = false;
		public bool es_header_2 { get; set; } = false;
	}
}
