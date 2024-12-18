using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
	public class TipoContactoServicio : Servicio<TipoContacto>, ITipoContactoServicio
	{
		public TipoContactoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<TipoContactoDto> GetTipoContactoLista(string tipo = "P")
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TIPO_CONTACTO_LISTA;
			var ps = new List<SqlParameter>()
			{
				new("@tipo",tipo)
			};
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(x => new TipoContactoDto()
				{
					#region Campos
					tc_id = x.tc_id,
					tc_cliente = x.tc_cliente,
					tc_desc = x.tc_desc,
					tc_proveedor = x.tc_proveedor,
					#endregion
				}).ToList();
		}

	}
}
