namespace gc.infraestructura.Dtos.Consultas
{
    public abstract class ConsultasDto : Dto { }
    public class ConsCtaCteDto: ConsultasDto
    {
        public int Total_registros { get; set; }
        public int Total_paginas { get; set; }
        public string Cta_id { get; set; } = string.Empty;
        public string Dia_movi { get; set; } = string.Empty;
        public string Tco_id { get; set; } = string.Empty;
        public string Tco_desc { get; set; } = string.Empty;
        public string Cm_compte { get; set; } = string.Empty;
        public string Cc_fecha { get; set; } = string.Empty;
        public string Cc_concepto { get; set; } = string.Empty;
        public string Cc_debe { get; set; } = string.Empty;
        public string Cc_haber { get; set; } = string.Empty;
        public string Cc_saldo { get; set; } = string.Empty;
        public string Usu_id { get; set; } = string.Empty;
        public string Usu_apellidoynombre { get; set; } = string.Empty;
    }

    public class ConsVtoDto : ConsultasDto
    {
        public string Cta_id { get; set; } = string.Empty;
        public string Dia_movi { get; set; } = string.Empty;
        public string Tco_id { get; set; } = string.Empty;
        public string Tco_desc { get; set; } = string.Empty;
        public string Cm_compte { get; set; } = string.Empty;
        public string Cm_compte_cuota { get; set; } = string.Empty;
        public DateTime Cv_fecha_carga { get; set; }
        public string Cv_concepto { get; set; } = string.Empty;
        public DateTime Cv_fecha_vto { get; set; }
        public decimal Cv_importe { get; set; }
        public decimal Cv_importe_ori { get; set; }
        public string Cv_estado { get; set; } = string.Empty;
        public string Ccb_id { get; set; } = string.Empty;
        public string Ve_id { get; set; } = string.Empty;
        public string Ve_nombre { get; set; } = string.Empty;
    }

    public class ConsCompTotDto : ConsultasDto
    {
        public string Cta_id { get; set; } = string.Empty;
        public string Periodo { get; set; } = string.Empty;
        public decimal Cm_neto { get; set; }
        public decimal Cm_iva { get; set; }
        public decimal Cm_total { get; set; }
        public decimal Nc_cm_neto { get; set; }
        public decimal Nc_cm_iva { get; set; }
        public decimal Nc_cm_total { get; set; }
        public decimal Dif_m_ant { get; set; }

    }

    public class ConsCompDetDto : ConsultasDto
    {
        public string Tco_id { get; set; } = string.Empty;
        public string Cm_compte { get; set; } = string.Empty;
        public string Cm_repetido { get; set; } = string.Empty;
        public string Cta_id { get; set; } = string.Empty;
        public string Dia_movi { get; set; } = string.Empty;
        public string Cm_nombre { get; set; } = string.Empty;
        public string Cm_domicilio { get; set; } = string.Empty;
        public string Cm_cuit { get; set; } = string.Empty;
        public string Cm_fecha { get; set; } = string.Empty;
        public string Cm_libro_iva { get; set; } = string.Empty;
        public decimal Cm_gravado { get; set; }
        public decimal Cm_no_gravado { get; set; }
        public decimal Cm_exento { get; set; }
        public decimal Cm_neto { get; set; }
        public decimal Cm_iva { get; set; }
        public decimal Cm_ii { get; set; }
        public decimal Cm_percepciones { get; set; }
        public decimal Cm_total { get; set; }
        public string Usu_id { get; set; } = string.Empty;
        public string Cm_compte_obs { get; set; } = string.Empty;
        public string Afip_id { get; set; } = string.Empty;
        public string Doc_compte { get; set; } = string.Empty;
        public string Tco_id_ori { get; set; } = string.Empty;
        public string Cm_compte_ori { get; set; } = string.Empty;
        public string Op_compte { get; set; } = string.Empty;
        public string Rp_compte { get; set; } = string.Empty;

    }

    public class ConsPagosDto : ConsultasDto
    {
        public string Op_compte { get; set; } = string.Empty;
        public string Opt_id { get; set; }
        public string Opt_desc { get; set; }
        public string Usu_id { get; set; } = string.Empty;
        public string Usu_apellidoynombre { get; set; } = string.Empty;
        public string Dia_movi { get; set; } = string.Empty;
        public decimal Op_importe { get; set; }
        public DateTime Op_concepto { get; set; }
        public string Cta_id { get; set; } = string.Empty;
        public string Cta_denominacion { get; set; } = string.Empty;
        public char Op_anulada { get; set; }
        public char Op_impreso { get; set; }
        public string Ctaf_id { get; set; } = string.Empty;
        public string Ctaf_denominacion { get; set; } = string.Empty;

    }

}
