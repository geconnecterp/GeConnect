
namespace gc.infraestructura.Dtos.Financieros
{
	public class ChequeModificadosListaDto : Dto
	{
		public string ctaf_id { get; set; } = string.Empty;
		public int che_emision { get; set; }
		public string che_nro { get; set; } = string.Empty;
		public DateTime che_fecha { get; set; }
		public string che_anombre { get; set; } = string.Empty;
		public decimal che_importe { get; set; } = 0.000M;
		public string usu_id { get; set; } = string.Empty;
		public DateTime che_fecha_emi { get; set; }
		public string cta_id { get; set; } = string.Empty;
		public string mod_usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
		public DateTime? mod_fecha { get; set; }

	}
}
