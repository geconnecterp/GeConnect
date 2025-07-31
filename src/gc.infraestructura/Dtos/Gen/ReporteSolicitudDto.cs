using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Enumeraciones;

namespace gc.infraestructura.Dtos.Gen
{
    public class ReporteSolicitudDto
    {
        public InfoReporte Reporte { get; set; }
        public Dictionary<string, string> Parametros { get; set; } = [];
		public List<IdCollection> Ids { get; set; } = [];
		public string Titulo { get; set; } = string.Empty;
        public string SubTitulo { get; set; } = string.Empty;
        public string Observacion { get; set; } = string.Empty;
        public string? LogoPath { get; set; } = string.Empty;
        public string Administracion { get; set; } = string.Empty;
        public string? Formato { get; set; } = string.Empty;
        public CuentaDto? Cuenta { get; set; } = new();
    }

    public class IdCollection
    {
        public string Id1 { get; set; } = string.Empty;
		public string Id2 { get; set; } = string.Empty;
		public string Id3 { get; set; } = string.Empty;
		public string Id4 { get; set; } = string.Empty;
		public string Id5 { get; set; } = string.Empty;
	}
}
