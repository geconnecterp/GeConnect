using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Productos.Models
{
    public class ResultadoImportacionViewModel
    {
        public List<RespuestaCPDto> Resultados { get; set; } = new();
        public RespuestaCPDto FirstReg { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int RegistrosExitosos { get; set; }
        public int RegistrosConError { get; set; }
        public string ArchivoOriginal { get; set; } = string.Empty;
        public DateTime FechaProceso { get; set; }
        public string ProveedorId { get; set; } = string.Empty;
        public bool PerfilSolicitado { get; set; }
        public bool PerfilGuardado { get; set; }
        public string MensajePerfil { get; set; } = string.Empty;

        public decimal PorcentajeExito => TotalRegistros > 0 ?
            Math.Round((decimal)RegistrosExitosos / TotalRegistros * 100, 1) : 0;

        public bool TieneErrores => RegistrosConError > 0;
        public bool EsProcesadoCompleto => TotalRegistros > 0 && RegistrosConError == 0;
        public int RegistrosConfirmables => Resultados.Count(r => r.registro_estado == 0);
        public bool TieneArchivoTemporal => FirstReg.idfile != Guid.Empty;
        public bool PuedeConfirmar => RegistrosConfirmables > 0 &&
                                       TieneArchivoTemporal &&
                                       FirstReg.resultado == 0;
        public string MotivoNoConfirmacion => RegistrosConfirmables == 0
            ? "No existen registros válidos para confirmar. Corrija el archivo o el mapeo y vuelva a procesarlo."
            : !TieneArchivoTemporal
                ? "No se generó la carga temporal necesaria para confirmar la importación."
                : FirstReg.resultado != 0
                    ? string.IsNullOrWhiteSpace(FirstReg.resultado_msj)
                        ? "El proceso preliminar informó una condición que impide confirmar la importación."
                        : FirstReg.resultado_msj
                    : string.Empty;
        public string Mensaje_proc => FirstReg.registro_msj;
    }
}
