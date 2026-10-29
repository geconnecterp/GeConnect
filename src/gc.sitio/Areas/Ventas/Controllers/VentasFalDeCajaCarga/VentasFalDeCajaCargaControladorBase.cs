using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Cajas;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Ventas.Controllers.VentasFalDeCajaCarga
{
	public class VentasFalDeCajaCargaControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;

		public VentasFalDeCajaCargaControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}

		public List<FaltanteDeCajaDto> FaltanteDeCajaLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("FaltanteDeCajaLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<FaltanteDeCajaDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("FaltanteDeCajaLista", json);
			}
		}
	}
}
