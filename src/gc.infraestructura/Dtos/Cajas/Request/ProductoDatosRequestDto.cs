namespace gc.infraestructura.Dtos.Cajas.Request
{
    public class ProductoDatosRequestDto
    {
        /// <summary>
        /// P = Producto
        /// F = Prefacturado
        /// C = Presupuesto
        /// </summary>
        public string tipo_valor { get; set; } = string.Empty;
        public string valor { get; set; } = string.Empty;
        public string lp_id { get; set; } = string.Empty;
        public string adm_id { get; set; } = string.Empty;
        public decimal cantidad { get; set; } = 1m;
        public bool bulto { get; set; } = true;
        public string ctc_id { get; set; } = "MI";
        public string cta_id { get; set; } = string.Empty;
        public decimal ctac_dto { get; set; } = 0m;
    }

    public class ProductoDatosResponseDto
    {

        public string p_id { get; set; } = string.Empty;
        public string p_id_barrado { get; set; } = string.Empty;
        public bool sin_scan_con_barrado { get; set; }

        public string p_desc { get; set; } = string.Empty;

        public decimal p_pcosto { get; set; }
        public decimal p_pcosto_repo { get; set; }

        public decimal in_alicuota { get; set; }
        public decimal p_in { get; set; }
        public string? iva_situacion { get; set; }
        public decimal iva_alicuota { get; set; }
        public decimal p_iva { get; set; }
        public decimal p_pneto { get; set; }

        public bool po { get; set; }
        public decimal po_limite { get; set; }
        public decimal p_pvta { get; set; }

        public decimal cantidad_tot { get; set; }
        public bool bultos { get; set; }

        public decimal lp_prevision_tot { get; set; }
        public decimal lp_prevision_pin { get; set; }

        public decimal cm_gravado { get; set; }
        public decimal cm_no_gravado { get; set; }
        public decimal cm_exento { get; set; }
        public decimal cm_iva { get; set; }
        public decimal cm_ii { get; set; }
        public decimal cm_dto { get; set; }
        public decimal cm_dto_porc { get; set; }

        public string p_activo { get; set; } = string.Empty;
        public string rub_id { get; set; } = string.Empty;
        public string rub_desc { get; set; } = string.Empty;
        public string cta_id { get; set; } = string.Empty;
        public string pg_id { get; set; } = string.Empty;
        public string up_id { get; set; } = string.Empty;
        public string up_tipo { get; set; } = string.Empty;
        public string up_desc { get; set; } = string.Empty;

        public int p_unidad_pres { get; set; }
        public decimal? p_peso { get; set; }

        public string? pre_id { get; set; }
        public string? cpf_nro { get; set; }

        public int respuesta { get; set; }
        public string respuesta_msj { get; set; } = string.Empty;

    }
}
