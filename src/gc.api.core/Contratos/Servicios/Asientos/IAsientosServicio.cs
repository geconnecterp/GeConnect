using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Asientos;

namespace gc.api.core.Contratos.Servicios.Asientos
{
    public interface IAsientoServicio:IServicio<EntidadBase>
    {
        List<EjercicioDto> ObtenerEjercicios();
        List<TipoAsientoDto> ObtenerTiposAsiento();
        List<UsuAsientoDto> ObtenerUsuariosDeEjercicio(int nro_eje);

        List<AsientoAjusteDto> ObtenerAsientosAjuste(int eje_nro);
        List<AsientoAjusteCcbDto> ObtenerAsientosAjusteCcb(int eje_nro, string ccb_id,bool todas);
    }
}
