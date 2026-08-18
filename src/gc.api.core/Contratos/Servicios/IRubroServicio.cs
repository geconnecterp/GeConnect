using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Almacen;

namespace gc.api.core.Contratos.Servicios
{
    public interface IRubroServicio : IServicio<Rubro>
    {
        List<RubroListaDto> GetRubroLista(string cta_id = "%");
        List<RubroItemListaDto> GetRubroUno(string rub_id);

	}
}
