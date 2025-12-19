using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Inventario;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
	public class InventarioServicio : Servicio<Inventario>, IInventarioServicio
	{
		public InventarioServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<InventarioListaDto> GetInventarioLista(GetInventarioListaRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_INV_LISTA;
			var ps = new List<SqlParameter>()
			{
				new("@desde",request.desde),
				new("@hasta",request.hasta),
				new("@adm_id",request.adm_id),
				new("@usu_id",request.usu_id),
				new("@inve_id",request.inve_id),
			};
			var listaTemp = _repository.EjecutarLstSpExt<InventarioListaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<RubroEnInventarioDto> GetRubrosEnInventario(string inv_nro, string usu_id = "%")
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_INV_RUBROS;
			var ps = new List<SqlParameter>()
			{
				new("@inv_nro",inv_nro),
				new("@usu_id",usu_id),
			};
			var listaTemp = _repository.EjecutarLstSpExt<RubroEnInventarioDto>(sp, ps, true);
			return listaTemp;
		}

		public List<UsuarioEnInventarioDto> GetUSuariosEnInventario(string inv_nro)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_INV_USUARIOS;
			var ps = new List<SqlParameter>()
			{
				new("@inv_nro",inv_nro),
			};
			var listaTemp = _repository.EjecutarLstSpExt<UsuarioEnInventarioDto>(sp, ps, true);
			return listaTemp;
		}
	}
}
