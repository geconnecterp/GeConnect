using gc.infraestructura.Dtos.Almacen;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IRubroServicio : IServicio<RubroDto>
    {
        List<RubroListaDto> ObtenerListaRubros(string tokenCookie);
    }
}
