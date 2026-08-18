using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Dtos.Productos.Precio;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IPrecioListaServicio
    {
        Task<RespuestaGenerica<PrecioListaDetalleDto>> ObtenerDetallePrecios(QueryFilters filters, string tokenCookie);
        Task<RespuestaGenerica<PrecioListaDto>> ObtenerListaPrecios(string token);
        Task<RespuestaGenerica<ListaPrecioRubCtaDto>> ObtenerListaPreciosRubCta(string lp_id, string token);
        RespuestaGenerica<RespuestaDto> RegistrarModificacionesEnListaDePrecios(RegistrarModificacionesEnListaDePreciosRequest request, string token);
	}
}
