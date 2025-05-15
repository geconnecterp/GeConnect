using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Asientos;

namespace gc.api.core.Contratos.Servicios.Asientos
{
    public interface IAsientoServicio:IServicio<EntidadBase>
    {
        List<EjercicioDto> ObtenerEjercicios();
        List<TipoAsientoDto> ObtenerTiposAsiento();
        List<UsuAsientoDto> ObtenerUsuariosDeEjercicio(int nro_eje);
    }
}
