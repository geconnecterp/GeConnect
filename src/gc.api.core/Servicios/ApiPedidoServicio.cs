using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Productos.Pedidos;
using gc.infraestructura.Dtos.Productos.Presupuestos;
using Microsoft.Data.SqlClient;

namespace gc.api.core.Servicios
{
	public class ApiPedidoServicio : Servicio<EntidadBase>, IApiPedidoServicio
	{
		public ApiPedidoServicio(IUnitOfWork uow) : base(uow)
		{
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
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_PC_LISTA;

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
	}
}
