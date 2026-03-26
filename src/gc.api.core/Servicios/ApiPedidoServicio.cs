using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Pedidos;
using Microsoft.Data.SqlClient;

namespace gc.api.core.Servicios
{
	public class ApiPedidoServicio : Servicio<EntidadBase>, IApiPedidoServicio
	{
		public ApiPedidoServicio(IUnitOfWork uow) : base(uow)
		{
		}

		public RespuestaDto ConfirmarPedido(ConfirmarPedidoRequest req)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_PC_CONFIRMA;

			var ps = new List<SqlParameter>() {
				new("@abm", req.abm),
				new("@pc_compte", req.pc_compte),
				new("@pc_obs", req.pc_obs),
				new("@cta_id", req.cta_id),
				new("@pc_cf", req.pc_cf),
				new("@json", req.json_prod),
				new("@usu_id", req.usu_id),
				new("@adm_id", req.adm_id),
				new("@pc_fecha", req.pc_fecha),
				new("@pc_entrega", req.pc_entrega),
				};


			var respuesta = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			if (respuesta.Count == 0)
			{
				return new RespuestaDto() { resultado = -1, resultado_msj = "No se Recepcionó respuesta del proceso." };
			}
			return respuesta[0];
		}

		public List<PedidoProductoDto> ObtenerDetalleDePedido(string pc_compte)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_PC_PRODUCTOS;

			var ps = new List<SqlParameter>
			{
				new("@pc_compte", pc_compte)
			};

			var detalle = _repository.EjecutarLstSpExt<PedidoProductoDto>(sp, ps, true);

			return detalle;
		}

		public List<PedidoListDto> ObtenerListaPedidos(PedidoRequest req)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_PC_LISTA_2;

			var ps = new List<SqlParameter>();
			if (string.IsNullOrEmpty(req.cli_list))
			{
				ps.Add(new SqlParameter("@c", false));
			}
			else
			{
				ps.Add(new SqlParameter("@c", true));
				ps.Add(new SqlParameter("@cta_list", req.cli_list));
			}

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

			if (string.IsNullOrEmpty(req.pce_list))
			{
				ps.Add(new SqlParameter("@e", false));
			}
			else
			{
				ps.Add(new SqlParameter("@e", true));
				ps.Add(new SqlParameter("@pce_list", req.pce_list));
			}

			if (string.IsNullOrEmpty(req.ve_list))
			{
				ps.Add(new SqlParameter("@v", false));
			}
			else
			{
				ps.Add(new SqlParameter("@v", true));
				ps.Add(new SqlParameter("@ve_list", req.ve_list));
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

			foreach (var p in ps)
			{
				Console.WriteLine($"{p.ParameterName} = {p.Value}");
			}

			var pedidos = _repository.EjecutarLstSpExt<PedidoListDto>(sp, ps, true);

			return pedidos;
		}

		public List<PedidoDto> ObtenerPedido(string pc_compte)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_PC_DATOS;

			var ps = new List<SqlParameter>();

			ps.Add(new SqlParameter("@pc_compte", pc_compte));

			var presup = _repository.EjecutarLstSpExt<PedidoDto>(sp, ps, true);

			return presup;
		}

		public RespuestaDto PasarPedidoACF(PasarPedidoACFRequest req)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_PC_CF;

			var ps = new List<SqlParameter>() {
				new("@pc_compte", req.pc_compte),
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

		public RespuestaDto DividePedidoDeCliente(DividePedidoDeClienteRequest req)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_PC_DIVIDE;

			var ps = new List<SqlParameter>() {
				new("@pc_compte", req.pc_compte),
				new("@divide", req.divide),
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
