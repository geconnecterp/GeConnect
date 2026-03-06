using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;
using gc.infraestructura.Dtos.Productos.Presupuestos;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.core.Servicios
{
	public class ApiOrdenDeRepartoServicio : Servicio<EntidadBase>, IApiOrdenDeRepartoServicio
	{
		public ApiOrdenDeRepartoServicio(IUnitOfWork uow) : base(uow)
		{
		}

		public List<OrdenDeRepartoEstadoDto> GetOrdenDeRepartoEstados()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_OR_ESTADOS;
			var ps = new List<SqlParameter>();
			var estados = _repository.EjecutarLstSpExt<OrdenDeRepartoEstadoDto>(sp, ps, true);
			return estados;
		}

		public List<OrdenDeRepartoListaDto> ObtenerListaOrdenDeReparto(OrdenDeRepartoRequest req)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_OR_LISTA;

			var ps = new List<SqlParameter>();
			if (req.Desde != default && req.Hasta != default)
			{
				ps.Add(new SqlParameter("@f", true));
				ps.Add(new SqlParameter("@desde", req.Desde));
				ps.Add(new SqlParameter("@hasta", req.Hasta));
			}
			else
			{
				ps.Add(new SqlParameter("@f", false));
			}

			if (string.IsNullOrEmpty(req.ore_list))
			{
				ps.Add(new SqlParameter("@e", false));
			}
			else
			{
				ps.Add(new SqlParameter("@e", true));
				ps.Add(new SqlParameter("@ore_list", req.ore_list));
			}

			if (string.IsNullOrEmpty(req.rp_list))
			{
				ps.Add(new SqlParameter("@r", false));
			}
			else
			{
				ps.Add(new SqlParameter("@r", true));
				ps.Add(new SqlParameter("@rp_list", req.rp_list));
			}

			ps.Add(new SqlParameter("@registros", req.Registros));
			ps.Add(new SqlParameter("@pagina", req.Pagina));

			var ordenes = _repository.EjecutarLstSpExt<OrdenDeRepartoListaDto>(sp, ps, true);

			return ordenes;
		}
	}
}
