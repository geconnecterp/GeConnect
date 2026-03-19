using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Almacen.Tr;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenReparto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IORServicio
    {
        Task<RespuestaGenerica<OrdenRepartoListDto>> ObtenerOrdenesReparto(ORRequestDto request, string token);
        Task<RespuestaGenerica<ResponseBaseDto>> ValidarUsuario(string id, string usuId, string token);
        Task<RespuestaGenerica<ORListaDto>> ObtenerListaORbyRubro(string or_compte, string adm, string usu, string token);
        Task<RespuestaGenerica<ORListaDto>> ObtenerListaORbyBox(string or_compte, string adm, string usu, string token);
        Task<RespuestaGenerica<ORProductoDto>> ObtenerORProductos(ORProdRequestDto request, string token);
        Task<RespuestaGenerica<RespuestaDto>> ValidaProductoCarritoOR(ORCargaCarritoRequest request, string tokenCookie);
        Task<RespuestaGenerica<RespuestaDto>> ResguardarProductoCarrito(ORCargaCarritoRequest request, string tokenCookie);
        Task<RespuestaGenerica<OrCtlProductoDto>> ObtenerListaProductosOrCtl(string or_compte, string usu_id, string token);
        Task<RespuestaGenerica<RespuestaDto>> CargaProductoORCtl(string json, string token);
    }
}
