using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Etiqueta;

namespace gc.api.core.Contratos.Servicios.Ofertas
{
    public interface IApiEtiquetaServicio
    {
        List<CargaPreviaDto> ObtenerCargaPreviaUsuario(string adm_id);
        List<IEDetalleDto> ObtenerDetalleEtiquetas(QueryFilters filters);
        List<EtiquetaDto> ObtenerDatosParaEtiqueta(string json, int etiq, string adm, string usu);
        RespuestaDto ConfirmarImpresionEtiqueta(string json, string adm, string usu);
        RespuestaDto ConfirmarCargaPrevia(AbmGenDto req);
    }
}
