using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.core.Servicios
{
	public class OrdenDePagoServicio : Servicio<OrdenDePago>, IOrdenDePagoServicio
	{
		public OrdenDePagoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}
		public List<OPValidacionPrevDto> GetOPValidacionesPrev(string cta_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_OP_VALIDACIONES_PREV;
			var ps = new List<SqlParameter>()
			{
				new("@cta_id", cta_id)
			};
			var listaTemp = _repository.EjecutarLstSpExt<OPValidacionPrevDto>(sp, ps, true);
			return listaTemp;
		}

		public List<OPDebitoYCreditoDelProveedorDto> GetOPDebitoYCreditoDelProveedor(string cta_id, char tipo, bool excluye_notas = false)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_OP_VTO;
			var ps = new List<SqlParameter>()
			{
				new("@cta_id", cta_id),
				new("@tipo", tipo),
				new("@excluye_notas", excluye_notas)
			};
			var listaTemp = _repository.EjecutarLstSpExt<OPDebitoYCreditoDelProveedorDto>(sp, ps, true);
			return listaTemp;
		}
	}
}
