using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Gen;

namespace gc.api.core.Contratos.Servicios.Asientos
{
    public interface IAsientoServicio:IServicio<EntidadBase>
    {
        List<EjercicioDto> ObtenerEjercicios();
        List<TipoAsientoDto> ObtenerTiposAsiento();
        List<UsuAsientoDto> ObtenerUsuariosDeEjercicio(int nro_eje);

        List<AsientoAjusteDto> ObtenerAsientosAjuste(int eje_nro);
        List<AsientoAjusteCcbDto> ObtenerAsientosAjusteCcb(int eje_nro, string ccb_id,bool todas);
        RespuestaDto ConfirmarAsientoAjuste(AjusteConfirmarDto confirmar);

        List<AsientoResultadoDto> ObtenerAsientosResultadoPG(int eje_nro);
        RespuestaDto ConfirmarAsientoResultadoPG(AjusteConfirmarDto confirmar);
    }
}
