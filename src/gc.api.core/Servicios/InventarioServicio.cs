using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Inventario;
using gc.infraestructura.Dtos.Users;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.core.Servicios
{
	public class InventarioServicio : Servicio<Inventario>, IInventarioServicio
	{
		public InventarioServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<InventarioDto> GetInventarioLista(GetInventarioListaRequest request)
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
			var listaTemp = _repository.EjecutarLstSpExt<InventarioDto>(sp, ps, true);
			return listaTemp;
		}
	}
}
