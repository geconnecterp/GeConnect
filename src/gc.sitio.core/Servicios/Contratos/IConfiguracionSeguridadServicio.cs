using gc.infraestructura.Dtos.Seguridad;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IConfiguracionSeguridadServicio
    {
        Task<PoliticaClaveDto> ObtenerPoliticaClave(string token);
        Task<CambioClaveResultadoDto> CambiarClave(CambioClaveRequestDto request, string token, string? ip);
        Task<CambioClaveResultadoDto> CambiarClaveForzada(CambioClaveForzadaRequestDto request, string token, string? ip);
    }
}
