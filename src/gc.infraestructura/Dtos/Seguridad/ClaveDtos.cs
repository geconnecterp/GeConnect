using gc.infraestructura.Dtos.Gen;

namespace gc.infraestructura.Dtos.Seguridad
{
    public class PoliticaClaveDto : Dto
    {
        public bool ValidarLongitud { get; set; }
        public short LongitudMinima { get; set; }
        public short LongitudMaxima { get; set; }
        public bool ValidarComplejidad { get; set; }
        public bool RequiereMayuscula { get; set; }
        public bool RequiereMinuscula { get; set; }
        public bool RequiereNumero { get; set; }
        public bool RequiereSimbolo { get; set; }
        public bool ImpedirClaveActual { get; set; }
        public short DiasVigencia { get; set; }
        public short ClaveTemporalVigenciaHoras { get; set; }
        public string? DerechoBlanquearClave { get; set; }
        public string? DerechoDesbloquearUsuario { get; set; }
    }

    public class CambioClaveRequestDto : Dto
    {
        public string ClaveActual { get; set; } = string.Empty;
        public string ClaveNueva { get; set; } = string.Empty;
    }

    public class CambioClaveResultadoDto : RespuestaDto
    {
        public Guid OperacionId { get; set; }
    }

    public class CambioClaveForzadaRequestDto : Dto
    {
        public string ClaveNueva { get; set; } = string.Empty;
    }

    public class OperacionUsuarioSeguridadRequestDto : Dto
    {
        public string UsuarioObjetivo { get; set; } = string.Empty;
    }

    public class EstadoSeguridadUsuarioDto : Dto
    {
        public bool CambioClaveObligatorio { get; set; }
        public string? CambioClaveMotivo { get; set; }
        public DateTime? CambioClaveFecha { get; set; }
        public DateTime? CambioClaveVencimiento { get; set; }
        public Guid? CambioClaveOperacionId { get; set; }
        public int VersionCredencial { get; set; }
        public bool ClaveTemporalVencida { get; set; }
    }

    public class OperacionesSeguridadUsuarioDto : Dto
    {
        public bool PuedeBlanquearClave { get; set; }
        public bool PuedeDesbloquearUsuario { get; set; }
    }
}
