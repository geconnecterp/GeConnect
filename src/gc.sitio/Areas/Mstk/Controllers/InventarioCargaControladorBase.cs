using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Inventario;
using gc.infraestructura.Dtos.Mstk;
using gc.infraestructura.Dtos.Users;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Mstk.Controllers
{
	public class InventarioCargaControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public InventarioCargaControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}

		public List<RubroEnInventarioDto> ListaRubroEnInventario
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaRubroEnInventario");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<RubroEnInventarioDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaRubroEnInventario", json);
			}
		}

		public List<UsuarioEnInventarioDto> ListaUsuarioEnInventario
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaUsuarioEnInventario");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<UsuarioEnInventarioDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaUsuarioEnInventario", json);
			}
		}

		public List<RubroListaDto> ListaRubros
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaRubros");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<RubroListaDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaRubros", json);
			}
		}

		public List<UserDto> ListaUsuarios
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaUsuarios");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<UserDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaUsuarios", json);
			}
		}
		//
	}
}
