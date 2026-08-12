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
}
