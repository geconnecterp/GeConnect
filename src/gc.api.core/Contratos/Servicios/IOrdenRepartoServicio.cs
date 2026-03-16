using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Almacen.Tr;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenReparto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.core.Contratos.Servicios
{
    public interface IOrdenRepartoServicio : IServicio<EntidadBase>
    {
        List<OrdenRepartoListDto> ObtenerOrdenesReparto(ORRequestDto request);
        List<ORListaDto> ObtenerListaORbyRubro(string or_compte, string adm, string usu);
        List<ORListaDto> ObtenerListaORbyBox(string or_compte, string adm, string usu);
        List<ORProductoDto> ObtenerListaORProductos(ORProdRequestDto request);
        RespuestaDto ValidaProductoCarritoOR(ORCargaCarritoRequest request);
        RespuestaDto ResguardarProductoCarrito(ORCargaCarritoRequest request);
        List<ORProductoDto> ObtenerListaProductosOrCtl(ORProdRequestDto request);
    }
}
