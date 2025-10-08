using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Productos.Ofertas;
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
        List<ComboProductoDto> ObtenerProductosDeCombo(string id);
        List<ComboSustitutoDto> ObtenerProductosSustitutosDeCombo(string id, string p_id);
    }
}
