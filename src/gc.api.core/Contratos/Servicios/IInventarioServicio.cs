using gc.api.core.Entidades;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Inventario;
using gc.infraestructura.Dtos.Inventario.Dto;
using gc.infraestructura.Dtos.Inventario.Request;

namespace gc.api.core.Contratos.Servicios
{
	public interface IInventarioServicio : IServicio<Inventario>
	{
		List<InventarioListaDto> GetInventarioLista(GetInventarioListaRequest request);
		List<RubroEnInventarioDto> GetRubrosEnInventario(string inv_nro, string usu_id = "%");
		List<UsuarioEnInventarioDto> GetUSuariosEnInventario(string inv_nro);
		List<RespuestaDto> ConfirmarInventario(ConfirmarInventarioRequest request);
		List<InventarioListaDto> GetInventarioDatos(GetInventarioDatosRequest request);
		List<RespuestaDto> RegistrarControlDeStock(RegistrarStockDeControlRequest request);
		List<ProductosEnValorizacionDto> GetProductosEnValorizacion(ProductosEnValorizacionRequest request);
		List<ConteoEnValorizacionDto> GetConteoEnValorizacion(ConteosEnValorizacionRequest request);	
        List<InventarioBoxDto> GetInventarioBox(string inv_nro, string usu_id);
        List<InventarioPlanillaDto> GetInventarioPlanilla(string inv_nro, string usu_id);
		RespuestaDto ValidarConteo(InventarioRequestDto request);
        List<InventarioConteoDto> GetInventarioConteo(InventarioRequestDto req);
		RespuestaDto InventarioConfirmarConteo(InventarioRequestDto request);
		List<RespuestaDto> RegistrarValorizacion(RegistrarValorizacionRequest request);
	}
}
