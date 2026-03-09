using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;
using gc.infraestructura.Dtos.Productos.Pedidos;
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

		public List<PedidoEnOrdenDeRepartoDto> ObtenerPedidosEnOrdenDeReparto(string orCompte)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_OR_PEDIDOS_EN_OR;

			var ps = new List<SqlParameter>
			{
				new("@or_compte", orCompte)
			};

			var detalle = _repository.EjecutarLstSpExt<PedidoEnOrdenDeRepartoDto>(sp, ps, true);

			return detalle;
		}

		public RespuestaDto ConfirmarOrdenDeReparto(ConfirmaOrdenDeRepartoRequest req)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_OR_CONFIRMA;

			var ps = new List<SqlParameter>() {
				new("@abm", req.abm),
				new("@or_compte", req.or_compte),
				new("@or_obs", req.or_obs),
				new("@rp_id", req.rp_id),
				new("@json", req.json),
				new("@usu_id", req.usu_id),
				new("@adm_id", req.adm_id),
				};


			var respuesta = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			if (respuesta.Count == 0)
			{
				return new RespuestaDto() { resultado = -1, resultado_msj = "No se Recepcionó respuesta del proceso." };
			}
			return respuesta[0];
		}
	}
}
