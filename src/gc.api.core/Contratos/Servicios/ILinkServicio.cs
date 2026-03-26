using gc.infraestructura.Dtos.Gen;

namespace gc.api.core.Contratos.Servicios
{
    public interface ILinkServicio
    {
        ReporteLinkResponseDto CrearLink(ReporteSolicitudDto solicitud, string usu_id = null, string? clienteId = null);
        ReporteSolicitudDto ObtenerSolicitud(string codigo);
    }
}
