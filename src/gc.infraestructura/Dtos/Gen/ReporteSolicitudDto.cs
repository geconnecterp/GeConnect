using gc.infraestructura.Enumeraciones;

namespace gc.infraestructura.Dtos.Gen
{
    public class ReporteSolicitudDto
    {
        public InfoReporte Reporte { get; set; }
        public Dictionary<string, object> Parametros { get; set; } = [];
        public string Titulo { get; set; } = string.Empty;
        public string Observacion { get; set; }= string.Empty;
        public string LogoPath { get; set; } = string.Empty;
        public string Administracion { get; set; } = string.Empty;

    }
}
