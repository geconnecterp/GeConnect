
namespace gc.infraestructura.Dtos.Gen
{
    public class RespuestaDto
    {
        public short resultado { get; set; }
        public string resultado_id { get; set; } = string.Empty;
        public string resultado_msj { get; set; } = string.Empty;
        public string resultado_setfocus { get; set; } = string.Empty;
        public Guid IdFile { get; set; }
        public DateTime hoy { get; set; }

    }

    public class RespuestaReportDto : RespuestaDto
    {
        public string Base64 { get; set; } = string.Empty;
    }

    public class TIRespuestaDto : RespuestaDto
    {
        public string Ti { get; set; } = string.Empty;
        public string Tit_id { get; set; } = string.Empty;
    }

    public class RespuestaRelaDto : RespuestaDto
    {
        public string rela { get; set; } = string.Empty;
    }

    public class RespuestaCPDto : RespuestaDto
    {
        public string p_id { get; set; } = string.Empty;
        public string p_id_prov { get; set; } = string.Empty;
        public string p_plista { get; set; } = string.Empty;
        public string p_dto1 { get; set; } = string.Empty;
        public string p_dto2 { get; set; } = string.Empty;
        public string p_dto3 { get; set; } = string.Empty;
        public string p_dto4 { get; set; } = string.Empty;
        public string p_dto_pa { get; set; } = string.Empty;
        public string p_porc_flete { get; set; } = string.Empty;
        public string p_boni { get; set; } = string.Empty;
        public string p_pcosto { get; set; } = string.Empty;
        public string iva_alicuota { get; set; } = string.Empty;
        public string in_alicuota { get; set; } = string.Empty;
        public string p_ean { get; set; } = string.Empty;
        public string p_ean_otro { get; set; } = string.Empty;
        public string p_dun { get; set; } = string.Empty;

        public string p_marca { get; set; } = string.Empty;
        public string p_desc { get; set; } = string.Empty;
        public string p_capacidad { get; set; } = string.Empty;

        public string p_unidad_pres { get; set; } = string.Empty;
        public string p_unidadxbulto { get; set; } = string.Empty;
        public string p_bultoxpiso { get; set; } = string.Empty;
        public string p_pisoxpallet { get; set; } = string.Empty;

        public string cta_id_geco { get; set; } = string.Empty;
        public short registro_estado { get; set; }
        public string registro_msj { get; set; } = string.Empty;
        public Guid idfile { get; set; } 
    }
}
