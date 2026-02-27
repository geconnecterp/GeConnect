using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Pedidos;
using gc.infraestructura.Dtos.Productos.Presupuestos;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Distribuidora.Controllers
{
	public class PedidoDeClienteControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public PedidoDeClienteControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}

		internal RespuestaGenerica<EntidadBase> CrearRespuestaError(string mensaje)
		{
			return new RespuestaGenerica<EntidadBase>
			{
				Mensaje = mensaje,
				Ok = false,
				EsWarn = false,
				EsError = true
			};
		}

		internal RespuestaGenerica<EntidadBase> CrearRespuestaWarning(string mensaje)
		{
			return new RespuestaGenerica<EntidadBase>
			{
				Mensaje = mensaje,
				Ok = false,
				EsWarn = true,
				EsError = false
			};
		}

		public List<PedidoProductoDto> ProductosActualesEnPedido
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ProductosActualesEnPedido") ?? string.Empty;
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<PedidoProductoDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ProductosActualesEnPedido", json);
			}
		}
	}
}
