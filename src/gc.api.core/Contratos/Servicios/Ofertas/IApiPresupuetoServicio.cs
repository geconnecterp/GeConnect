using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Presupuestos;

namespace gc.api.core.Contratos.Servicios.Ofertas
{
    public interface IApiPresupuetoServicio
    {
        List<PresupuestoListDto> ObtenerListaPresupuestos(PresupuestoRequest req);
        List<PresupuestoDto> ObtenerPresupuesto(string pre_id);
        List<PresupuestoProductoDto> ObtenerDetallePresupuesto(string pre_id);
        List<PresupuestoProductoDto> ObtenerDetallePresupuestoActualizado(string pre_id);        
        List<PresupE> ObtenerEstadosPresupuesto();
        List<PresupT> ObtenerTiposPresupuesto();
        RespuestaDto ConfirmarPresupuesto(AbmPlusGenDto req);
    }
}
