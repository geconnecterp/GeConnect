namespace gc.infraestructura.Dtos.Gen
{
    public class LinkRequestDto
    {
        public ReporteSolicitudDto Solicitud { get; set; } = new();
        public string Usu_id { get; set; } = string.Empty;
        public string ClienteId { get; set; } = string.Empty;
    }
}
