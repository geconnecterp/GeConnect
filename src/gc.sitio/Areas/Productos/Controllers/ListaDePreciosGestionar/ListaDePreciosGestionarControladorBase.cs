using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Productos.Controllers.ListaDePreciosGestionar
{
	
	public class ListaDePreciosGestionarControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public ListaDePreciosGestionarControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}

		public List<ListaPrecioDto> ListaPrecio
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaPrecio");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<ListaPrecioDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaPrecio", json);
			}
		}

		public List<ListaPrecioRubCtaDto> ListaPrecioRubCta
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaPrecioRubCta");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<ListaPrecioRubCtaDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaPrecioRubCta", json);
			}
		}
		//
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
	}
}
