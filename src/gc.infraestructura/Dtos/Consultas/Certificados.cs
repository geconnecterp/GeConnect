
namespace gc.infraestructura.Dtos.Consultas
{
	public abstract class CertificadosDto : Dto { }

	public class  CertRetenGananDto : CertificadosDto
	{
		public string cgan_nro { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cgan_cuit { get; set; } = string.Empty;
		public string cgan_raz_soc { get; set; } = string.Empty;
		public string cgan_domicilio { get; set; } = string.Empty;
		public DateTime cgan_fecha { get; set; }
		public decimal cgan_base { get; set; } = 0.00M;
		public decimal cgan_reten { get; set; } = 0.00M;
		public string cgan_estado { get; set; } = string.Empty;
		public string op_compte { get; set; } = string.Empty;
		public string cgan_actu { get; set; } = string.Empty;
		public string emp_razon_social { get; set; } = string.Empty;
		public string emp_cuit { get; set; } = string.Empty;
		public string emp_ib_nro { get; set; } = string.Empty;
		public string emp_domicilio { get; set; } = string.Empty;
		public string rgan_desc { get; set; } = string.Empty;
		public decimal rgan_porc { get; set; } = 0.00M;
		public string rgan_impreso { get; set; } = string.Empty;
	}

	public class CertRetenIBDto : CertificadosDto
	{
		public string cib_nro { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cib_cuit { get; set; } = string.Empty;
		public string cib_raz_soc { get; set; } = string.Empty;
		public string cib_domicilio { get; set; } = string.Empty;
		public DateTime cib_fecha { get; set; }
		public decimal cib_base { get; set; } = 0.00M;
		public decimal cib_reten { get; set; } = 0.00M;
		public string cib_estado { get; set; } = string.Empty;
		public string op_compte { get; set; } = string.Empty;
		public string cib_actu { get; set; } = string.Empty;
		public decimal cib_reten_lh { get; set; } = 0.00M;
		public string emp_razon_social { get; set; } = string.Empty;
		public string emp_cuit { get; set; } = string.Empty;
		public string emp_ib_nro { get; set; } = string.Empty;
		public string emp_domicilio { get; set; } = string.Empty;
		public string cib_nro_ins { get; set; } = string.Empty;
		public decimal cib_ali { get; set; } = 0.00M;
		public decimal cib_ali_lh { get; set; } = 0.00M;
		public string rib_desc { get; set; } = string.Empty;
		public decimal rib_porc { get; set; } = 0.00M;
		public string rib_impreso { get; set; } = string.Empty;
	}

	public class CertRetenIVADto : CertificadosDto
	{
		public string civa_nro { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string civa_cuit { get; set; } = string.Empty;
		public string civa_raz_soc { get; set; } = string.Empty;
		public string civa_domicilio { get; set; } = string.Empty;
		public DateTime civa_fecha { get; set; }
		public decimal civa_base { get; set; } = 0.00M;
		public decimal civa_reten { get; set; } = 0.00M;
		public string civa_estado { get; set; } = string.Empty;
		public string op_compte { get; set; } = string.Empty;
		public string civa_actu { get; set; } = string.Empty;
		public string civa_impreso { get; set; } = string.Empty;
		public string emp_razon_social { get; set; } = string.Empty;
		public string emp_cuit { get; set; } = string.Empty;
		public string emp_ib_nro { get; set; } = string.Empty;
		public string emp_domicilio { get; set; } = string.Empty;
	}
}
