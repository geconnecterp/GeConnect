namespace gc.infraestructura.Dtos.Cajas.Response
{
    public class NCProductoBuscarResponseDto
    {
        public string p_id { get; set; } = string.Empty;
        public string p_id_barrado { get; set; } = string.Empty;
        public string p_desc { get; set; } = string.Empty;

        public decimal? p_pcosto { get; set; }
        public decimal? p_pcosto_repo { get; set; }

        public decimal? in_alicuota { get; set; }
        public decimal? p_in { get; set; }

        public string iva_situacion { get; set; } = string.Empty;
        public decimal? iva_alicuota { get; set; }
        public decimal? p_iva { get; set; }

        public decimal? p_pneto { get; set; }

        public int po { get; set; }

        public decimal? p_pvta { get; set; }
        public decimal? cantidad_tot { get; set; }

        public int bultos { get; set; }

        public string cmd_cmb { get; set; } = string.Empty;
        public string cmd_cmb_id { get; set; } = string.Empty;
        public decimal? cmd_cmb_dto { get; set; }
        public decimal? cmd_cmb_cant { get; set; }
        public string cmd_cmb_desc { get; set; } = string.Empty;

        public string p_activo { get; set; } = string.Empty;
        public string rub_id { get; set; } = string.Empty;
        public string rub_desc { get; set; } = string.Empty;

        public string cta_id { get; set; } = string.Empty;
        public string pg_id { get; set; } = string.Empty;
        public string up_id { get; set; } = string.Empty;

        public string up_tipo { get; set; } = string.Empty;
        public string up_desc { get; set; } = string.Empty;

        public int? p_unidad_pres { get; set; }
        public decimal? p_peso { get; set; }

        public short? respuesta { get; set; }
        public string respuesta_msj { get; set; } = string.Empty;
    }
}
