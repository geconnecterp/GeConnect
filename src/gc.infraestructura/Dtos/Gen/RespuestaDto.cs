
namespace gc.infraestructura.Dtos.Gen
{
    public class RespuestaDto
    {
        public short resultado { get; set; }
        public string resultado_id { get; set; } = string.Empty;
        public string resultado_msj { get; set; } = string.Empty;
        public string resultado_setfocus { get; set; } = string.Empty;

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
}
