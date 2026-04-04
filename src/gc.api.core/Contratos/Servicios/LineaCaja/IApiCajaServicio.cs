using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Gen;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace gc.api.core.Contratos.Servicios.LineaCaja
{
    public interface IApiCajaServicio
    {
        RespuestaDto ValidaIntegridadUsuarioCaja(CajaValidaReqDto req);
        RespuestaDto AperturaCaja(CajaValidaReqDto reqDto);
    }
}
