using gc.infraestructura.Dtos.Almacen.Rpr;

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
        public DateTime Cc_fecha { get; set; } 
        public string Cc_concepto { get; set; } = string.Empty;
        public decimal Cc_debe { get; set; } 
        public decimal Cc_haber { get; set; }
        public decimal Cc_saldo { get; set; } 
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
        public DateTime Cm_fecha { get; set; } 
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
        public DateTime Cm_fecha_carga { get; set; } 

    }

    public class ConsOrdPagosDto : ConsultasDto
    {
        public string Op_compte { get; set; } = string.Empty;
        public string Opt_id { get; set; } = string.Empty;
        public string Opt_desc { get; set; } = string.Empty;    
        public string Usu_id { get; set; } = string.Empty;
        public string Usu_apellidoynombre { get; set; } = string.Empty;
        public string Dia_movi { get; set; } = string.Empty;
        public decimal Op_importe { get; set; }
        public DateTime Op_fecha { get; set; }
        public DateTime Op_concepto { get; set; }
        public string Cta_id { get; set; } = string.Empty;
        public string Cta_denominacion { get; set; } = string.Empty;
        public char Op_anulada { get; set; }
        public char Op_impreso { get; set; }
        public string Ctaf_id { get; set; } = string.Empty;
        public string Ctaf_denominacion { get; set; } = string.Empty;

    }

    public class ConsOrdPagosDetDto : ConsultasDto
    {
        public string Op_compte { get; set; } = string.Empty;
        public string Cm_compte_cuota{ get; set; } = string.Empty;
        public string Tco_id { get; set; } = string.Empty;
        public string Tco_desc { get; set; } = string.Empty;
        public decimal Cc_importe { get; set; } 
        public decimal Cc_importe_ori { get; set; } 
        public DateTime Cc_fecha_vto { get; set; } 
        public DateTime Cc_fecha_carga { get; set; }
        public string Ctag_id { get; set; } = string.Empty;
        public string Ctag_motivo { get; set; } = string.Empty;
        public string Dia_movi { get; set; } = string.Empty;
        public string Cc_concepto { get; set; } = string.Empty;
        public string Grupo { get; set; } = string.Empty;
        public string Grupo_des { get; set; } = string.Empty;
        public string Concepto { get; set; } = string.Empty;
    }

    public class ConsOrdPagoDetExtendDto : ConsOrdPagosDetDto
    {
        public string Opt_id { get; set; } = string.Empty;
		public string Opt_desc { get; set; } = string.Empty;
		public string Usu_id { get; set; } = string.Empty;
		public string Usu_apellidoynombre { get; set; } = string.Empty;
        public decimal Op_importe { get; set; } = 0.00M;
		public DateTime Op_fecha { get; set; }
        public string Op_concepto { get; set; } = string.Empty;
        public string Cta_id { get; set; } = string.Empty;
        public string Cta_denominacion { get; set; } = string.Empty;
        public string Op_anulada { get; set; } = string.Empty;
		public string Op_impreso { get; set; } = string.Empty;
        public string Cm_compte { get; set; } = string.Empty;
	}

	public class ConsRecepcionProveedorDto : ConsultasDto
    {
        public string Rp_compte { get; set; } = string.Empty;
        public string Cta_id { get; set; } = string.Empty;
        public string Cta_denominacion { get; set; } = string.Empty;
        public DateTime Rp_fecha { get; set; }
        public string Usu_id { get; set; } = string.Empty;
        public string Usu_apellidoynombre { get; set; } = string.Empty;
        public string Adm_id { get; set; }= string.Empty;
        public string Adm_nombre { get; set; }=string.Empty;
        public char Rpe_id { get; set; }
        public string Rpe_desc { get; set; } = string.Empty;
        public string Tco_id_rp { get; set; } = string.Empty;
        public string Cm_compte_rp { get; set; } = string.Empty;
        public DateTime Cm_fecha_rp { get; set; }
        public decimal Cm_importe_rp { get; set; }
        public string Tco_id { get; set; } = string.Empty;
        public string Cm_compte { get; set; } = string.Empty;
        public string Dia_movi { get; set; } = string.Empty;
        public string Oc_compte { get; set; } = string.Empty;
        public bool Controlada{ get; set; }
        public bool Factura{ get; set; }
        public bool Valorizada{ get; set; }
        public bool Modificada{ get; set; }
        public bool Colector { get; set; }

    }

    public class ConsRecepcionProveedorDetalleDto
    {
        public string Rp_compte { get; set; } = string.Empty;
        public string Cta_id { get; set; } = string.Empty;
        public string Cta_denominacion { get; set; } = string.Empty;
        public DateTime Rp_fecha { get; set; }
        public string Usu_id { get; set; } = string.Empty;
        public string Usu_apellidoynombre { get; set; } = string.Empty;
        public string Adm_id { get; set; } = string.Empty;
        public string Adm_nombre { get; set; } = string.Empty;
        public string Depo_id { get; set; } = string.Empty;
        public string Depo_nombre { get; set; } = string.Empty;
        public char Rpe_id { get; set; }
        public string Tco_id_rp { get; set; } = string.Empty;
        public string Cm_compte_rp { get; set; } = string.Empty;
        public DateTime Cm_fecha_rp { get; set; }
        public decimal Cm_importe_rp { get; set; }
        public string Oc_compte { get; set; } = string.Empty;
        public string Cm_compte { get; set; }=string.Empty;
        public string Dia_movi { get; set; } = string.Empty;
        public int Rpd_item { get; set; }
        public string P_id { get; set; }= string.Empty;
        public string P_desc { get; set; } = string.Empty;
        public int Rpd_unidad_pres { get; set; }
        //public int Rpd_unidad_x_bulto { get; set; }
        public int Rpd_bulto_compte { get; set; }
        public decimal Rpd_unidad_suelta_compte { get; set; }
        public decimal Rpd_cantidad_compte { get; set; }
        public int Rpd_bulto_recibidos { get; set; }
        public decimal Rpd_unidad_suelta { get; set; }
        public decimal Rpd_Cantidad { get; set; }
        public decimal Rpd_total { get; set; }
        public char Up_tipo { get; set; }

    }
}
