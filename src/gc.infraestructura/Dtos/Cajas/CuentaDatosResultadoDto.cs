using Microsoft.AspNetCore.Mvc;

namespace gc.infraestructura.Dtos.Cajas
{
    public class CuentaDatosResultadoDto
    {
        public string cta_id { get; set; } = string.Empty;
        public string cta_denominacion { get; set; } = string.Empty;
        public string cta_domicilio { get; set; } = string.Empty;
        public string cta_celu { get; set; } = string.Empty;
        public string cta_email { get; set; } = string.Empty;

        public string tdoc_id { get; set; } = string.Empty;
        public string tdoc_desc { get; set; } = string.Empty;
        public string cta_documento { get; set; } = string.Empty;
        public string cta_sexo { get; set; } = string.Empty;

        public string afip_id { get; set; } = string.Empty;
        public string afip_desc { get; set; } = string.Empty;

        public string nj_id { get; set; } = string.Empty;
        public string cta_ib_nro { get; set; } = string.Empty;
        public string ib_id { get; set; } = string.Empty;

        public string pib_cert { get; set; } = string.Empty;
        public DateTime? pib_cert_vto { get; set; }

        public string piva_cert { get; set; } = string.Empty;
        public DateTime? piva_cert_vto { get; set; }

        public string ve_id { get; set; } = string.Empty;
        public string ve_nombre { get; set; } = string.Empty;

        public string zn_id { get; set; } = string.Empty;
        public string zn_desc { get; set; } = string.Empty;

        public string rp_id { get; set; } = string.Empty;
        public string rp_nombre { get; set; } = string.Empty;

        public string lp_id { get; set; } = string.Empty;

        public string ctc_id { get; set; } = string.Empty;
        public string ctc_desc { get; set; } = string.Empty;

        public string ctn_id { get; set; } = string.Empty;
        public string ctn_desc { get; set; } = string.Empty;

        public string cta_emp { get; set; } = string.Empty;
        public string cta_emp_legajo { get; set; } = string.Empty;

        public string valida { get; set; } = string.Empty;
        public string valida_desc { get; set; } = string.Empty;

        public decimal ctac_tope_credito { get; set; }
        public decimal ctac_dto_operacion { get; set; }

        public string fp { get; set; } = string.Empty;
        public string tco_letra { get; set; } = string.Empty;
        public string? Origen { get; set; }
    }

    //seria la representacion del json fp que viene en el resultado del sp, para facilitar su uso en el front.
    public class FormaPagoItem
    {
        public string fp_id { get; set; } = string.Empty;
        public string fp_desc { get; set; } = string.Empty;
        public int fp_dias { get; set; }
        public string obs { get; set; } = string.Empty;
        public string bco_cbu { get; set; } = string.Empty;
        public string bco_cta_nro { get; set; } = string.Empty;
        public string valores_a_nombre { get; set; } = string.Empty;
    }

}
