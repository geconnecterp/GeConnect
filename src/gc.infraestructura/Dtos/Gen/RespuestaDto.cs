
using Newtonsoft.Json;

namespace gc.infraestructura.Dtos.Gen
{
    public class RespuestaDto
    {
        //tiene el valor 0 para indicar que se ha ejecutado el proceso, 1 para indicar que hay un warning y -1 para indicar que se ha producido un error
        public short resultado { get; set; }
        //si resultado es 1 o -1, resultado_id puede contener un código de error o warning específico para identificar el tipo de error o warning ocurrido
        public string resultado_id { get; set; } = string.Empty;
        //si resultado es 1 o -1, resultado_msj puede contener un mensaje descriptivo del error o warning ocurrido
        public string resultado_msj { get; set; } = string.Empty;
        //si resultado es <> 0, resultado_setfocus puede contener el nombre del campo al que se le debe hacer foco para corregir el error o warning ocurrido
        public string resultado_setfocus { get; set; } = string.Empty;        
        public Guid IdFile { get; set; }
        public DateTime hoy { get; set; }

    }

    public class RespuestaReportDto : RespuestaDto
    {
        [JsonProperty("base64")]
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
        public bool perfil_solicitado { get; set; }
        public bool perfil_guardado { get; set; }
        public string perfil_msj { get; set; } = string.Empty;
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
