using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Inventario;
using gc.infraestructura.Dtos.Inventario.Dto;
using gc.infraestructura.Dtos.Inventario.Request;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IInventarioServicio : IServicio<InventarioDto>
    {
        List<InventarioListaDto> GetInventarioLista(GetInventarioListaRequest request, string token);
        List<RubroEnInventarioDto> GetRubrosEnInventario(string inv_nro, string token, string usu_id = "%");
        List<UsuarioEnInventarioDto> GetUsuariosEnInventario(string inv_nro, string token);
        RespuestaGenerica<RespuestaDto> ConfirmarInventario(ConfirmarInventarioRequest request, string token);
        Task<RespuestaGenerica<InventarioBoxDto>> GetInventarioBox(InventarioRequestDto req, string token);
        Task<RespuestaGenerica<InventarioPlanillaDto>> GetInventarioPlanilla(InventarioRequestDto req, string token);
		List<InventarioListaDto> GetInventarioDatos(GetInventarioDatosRequest request, string token);
		RespuestaGenerica<RespuestaDto> RegistrarControlDeStock(RegistrarStockDeControlRequest request, string token);
		List<ProductosEnValorizacionDto> GetProductosEnValorizacion(ProductosEnValorizacionRequest request, string token);
		List<ConteoEnValorizacionDto> GetConteosEnValorizacion(ConteosEnValorizacionRequest request, string token);	
        Task<RespuestaGenerica<RespuestaDto>> ValidaConteo(InventarioRequestDto request, string token);
        Task<RespuestaGenerica<InventarioConteoDto>> GetConteno(InventarioRequestDto req, string token);
        Task<RespuestaGenerica<RespuestaDto>> ConfirmarConteo(InventarioRequestDto req, string token);
    }
}
