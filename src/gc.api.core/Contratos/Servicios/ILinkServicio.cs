using gc.infraestructura.Dtos.Gen;

namespace gc.api.core.Contratos.Servicios
{
    public interface ILinkServicio
    {
        ReporteLinkResponseDto CrearLink(ReporteSolicitudDto solicitud, string usu_id = null, string? clienteId = null);
        ReporteLinkAccesoResponseDto ObtenerSolicitud(string codigo, ReporteLinkAccesoContextoDto contexto);
        ReporteLinkOperacionResponseDto ConfirmarDescarga(ReporteLinkDescargaDto descarga);
        void RegistrarFallo(ReporteLinkDescargaDto descarga);
    }
}
