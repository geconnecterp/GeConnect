using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Dtos.Productos.Precio;

namespace gc.api.core.Contratos.Servicios.Ofertas
{
    public interface IApiPrecioListaServicio
    {
        List<PrecioListaDto> ObtenerListaPrecios();
        List<PrecioListaDetalleDto> ObtenerDetallePrecios(QueryFilters filters);
        List<ListaPrecioRubCtaDto> ObtenerListaPreciosRubCta(string lp_id);
        List<RespuestaDto> RegistrarModificacionesEnListaDePrecios(RegistrarModificacionesEnListaDePreciosRequest request);
	}
}
