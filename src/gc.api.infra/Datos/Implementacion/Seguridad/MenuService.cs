using Azure.Core;
using gc.api.core.Constantes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.api.core.Servicios;
using gc.api.infra.Datos.Contratos.Security;
using gc.infraestructura.Constantes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.infra.Datos.Implementacion.Security
{
	public class MenuService : Servicio<UsuarioMenu>, IMenuService
	{
		private readonly PaginationOptions paginationOptions;
		public MenuService(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
			paginationOptions = options.Value;
		}

		public List<UsuarioMenu> GetMenuList(string usuId)
		{
			var sp = ConstantesGC.StoredProcedures.MNU_GET_MENU_LIST;
			var ps = new List<SqlParameter>()
			{
					new("@usu_id",usuId),
			};
			var listaTemp = _repository.EjecutarLstSpExt<UsuarioMenu>(sp, ps, true);
			return listaTemp;
		}
	}
}
