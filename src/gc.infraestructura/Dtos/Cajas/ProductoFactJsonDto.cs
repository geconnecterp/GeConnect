namespace gc.infraestructura.Dtos.Cajas
{
    public class ProductoFactJsonDto
    {
        public string p_id { get; set; } = string.Empty;
        public string p_id_barrado { get; set; } = string.Empty;
        public string p_desc { get; set; } = string.Empty;

        public decimal p_pcosto { get; set; }
        public decimal p_pcosto_repo { get; set; }
        public decimal in_alicuota { get; set; }
        public decimal p_in { get; set; }

        public string iva_situacion { get; set; } = string.Empty;
        public decimal iva_alicuota { get; set; }
        public decimal p_iva { get; set; }

        public bool po { get; set; }
        public decimal po_limite { get; set; }

        public decimal p_pneto { get; set; }
        public decimal p_margen_imp { get; set; }
        public decimal p_margen_vig { get; set; }
        public decimal p_pvta { get; set; }

        public decimal cantidad_tot { get; set; }
        public decimal p_pvta_tot { get; set; }

        public decimal cm_gravado { get; set; }
        public decimal cm_no_gravado { get; set; }
        public decimal cm_exento { get; set; }
        public decimal cm_iva { get; set; }
        public decimal cm_ii { get; set; }

        public decimal? cm_dto { get; set; }
        public decimal? cm_dto_porc { get; set; }
        /// <summary>
        /// es la del proveedor
        /// </summary>
        public string cta_id { get; set; } = string.Empty;
            
        public string? pre_id { get; set; }
        public string? cpf_nro { get; set; }

        public string cmb_p_id { get; set; } = string.Empty;
        public string cmb { get; set; } = string.Empty;

        public string? cmb_id { get; set; }

        public decimal cmb_dto { get; set; }
        public decimal cmb_cant { get; set; }

        public string? cmb_desc { get; set; }

        public int item { get; set; }                  // int
    }
}
