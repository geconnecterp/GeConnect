using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Productos.PromoCombo;

namespace gc.api.core.Contratos.Servicios.Ofertas
{
    public interface IApiPromoComboServicio
    {
        List<ComboTipoDto> ObtenerComboTipo();
        List<ComboEstadoDto> ObtenerComboEstado();
        List<ComboListaDto> ObtenerDetalleDeCombos(QueryFilters req);
        ComboDatosDto ObtenerComboPorId(string id);
        List<ComboCanalDto> ObtenerCanalesDeCombo(string id);
    }
}
