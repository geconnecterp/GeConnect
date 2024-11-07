using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Almacen.Tr.Remito;
using gc.infraestructura.Dtos.Almacen.Tr.Request;
using gc.infraestructura.Dtos.Gen;

namespace gc.api.core.Contratos.Servicios
{
    public interface IRemitoServicio : IServicio<Remito>
    {
        List<RemitoGenDto> ObtenerRemitosPendientes(string admId, string reeId = "%");
        List<RespuestaDto> SeteaEstado(RSetearEstadoRequest request);
        List<RemitoVerConteoDto> VerConteos(string remCompte);
        List<RespuestaDto> ConfirmaRecepcion(RConfirmaRecepcionRequest reques);
        RespuestaDto VerificaProductoEnRemito(string remCompte, string pId);
        RespuestaDto RTRCargarConteos(CargarJsonGenRequest request);
        List<RTRxULDto> RTRCargarConteosXUL(string reCompte);

	}
}
