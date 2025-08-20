using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Productos.Models
{
    public class ResultadoImportacionViewModel
    {
        public List<RespuestaCPDto> Resultados { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int RegistrosExitosos { get; set; }
        public int RegistrosConError { get; set; }
        public string ArchivoOriginal { get; set; } = string.Empty;
        public DateTime FechaProceso { get; set; }
        public string ProveedorId { get; set; } = string.Empty;

        public decimal PorcentajeExito => TotalRegistros > 0 ?
            Math.Round((decimal)RegistrosExitosos / TotalRegistros * 100, 1) : 0;

        public bool TieneErrores => RegistrosConError > 0;
        public bool EsProcesadoCompleto => TotalRegistros > 0 && RegistrosConError == 0;
    }
}
