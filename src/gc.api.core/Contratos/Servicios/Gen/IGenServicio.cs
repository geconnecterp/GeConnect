using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.core.Contratos.Servicios.Gen
{
    public interface IGenServicio
    {
        Task<RespuestaGenericaBase<string>> InvokeApiGET(ApiInvokeRequest request);
        Task<RespuestaGenericaBase<string>> InvokeApiPOST(ApiInvokeRequest request);
    }
}
