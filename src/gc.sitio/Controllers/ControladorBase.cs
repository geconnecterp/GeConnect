using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.AjusteDeStock;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Almacen.Tr.Transferencia;
using gc.infraestructura.Dtos.CuentaComercial;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes;
using gc.infraestructura.EntidadesComunes.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Dynamic.Core;
using X.PagedList;

namespace gc.sitio.Controllers
{
	public class ControladorBase : Controller
	{
		private readonly AppSettings _options;
		protected readonly IHttpContextAccessor _context;

		public List<Orden> _orden;

		public ControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto)
		{
			_options = options.Value;
			_context = contexto;
		}

		public string NombreSitio
		{
			get { return _options.Nombre; }
		}
        public string Etiqueta
        {
            get { return _context.HttpContext.Session.GetString("Etiqueta"); }

            set { HttpContext.Session.SetString("Etiqueta", value); }
        }
        public string Token
		{
			get { return HttpContext.Session.GetString("JwtToken"); }

			set { HttpContext.Session.SetString("JwtToken", value); }
		}

		public string TokenCookie
		{
			get
			{
				//var nombre = User.Claims.First(c => c.Type.Contains("name")).Value;
				return _context.HttpContext.Request.Cookies[Etiqueta];
			}

		}
        public string AdministracionId
        {
            get
            {
                var adm = User.Claims.First(c => c.Type.Contains("AdmId")).Value;
                if (string.IsNullOrEmpty(adm))
                {
                    return string.Empty;
                }
                var parts = adm.Split('#');

                return parts[0];
            }
        }

        public string AdministracionName
        {
            get
            {
                var adm = User.Claims.First(c => c.Type.Contains("AdmId")).Value;
                if (string.IsNullOrEmpty(adm))
                {
                    return string.Empty;
                }

                var parts = adm.Split('#');

                return parts[1];
            }
        }

        public (bool, DateTime?) EstaAutenticado
		{
			get
			{
				DateTime? expira;
				var handler = new JwtSecurityTokenHandler(); //Libreria System.IdentityModel.Token.Jwt (6.7.1)
				try
				{
					var tokenS = handler.ReadToken(TokenCookie) as JwtSecurityToken;
					var venc = tokenS.Claims.First(c => c.Type.Contains("expires")).Value;
					expira = venc.ToDateTimeFromTicks();
					if (!expira.HasValue || expira.Value < DateTime.Now)
					{

						return (false, null);
					}
				}
				catch { return (false, null); }
				return (true, expira);
			}
		}

		public bool TieneRoles
		{
			get
			{
				var handler = new JwtSecurityTokenHandler(); //Libreria System.IdentityModel.Token.Jwt (6.7.1)
				var tokenS = handler.ReadToken(Token) as JwtSecurityToken;

				var rolesUser = tokenS.Claims.First(c => c.Type.Contains("role")).Value;
				if (string.IsNullOrEmpty(rolesUser)) { return false; }
				return true;
			}
		}

		public string RolUsuario
		{
			get
			{
				var handler = new JwtSecurityTokenHandler(); //Libreria System.IdentityModel.Token.Jwt (6.7.1)
				var tokenS = handler.ReadToken(TokenCookie) as JwtSecurityToken;
				var rolesUser = tokenS.Claims.First(c => c.Type.Contains("role")).Value;

				#region codigo despreciable para saber el rol
				//if (User.Identity.IsAuthenticated)
				//{
				//    if (User.IsInRole(nameof(RolesUsuario.ADMINISTRACION)))
				//    {
				//        return nameof(RolesUsuario.ADMINISTRACION);
				//    }
				//    else if (User.IsInRole(nameof(RolesUsuario.ADMINISTRADOR)))
				//    {
				//        return nameof(RolesUsuario.ADMINISTRADOR);
				//    }
				//    else if (User.IsInRole(nameof(RolesUsuario.CAJERO)))
				//    {
				//        return nameof(RolesUsuario.CAJERO);
				//    }
				//    else if (User.IsInRole(nameof(RolesUsuario.CONSULTA)))
				//    {
				//        return nameof(RolesUsuario.CONSULTA);
				//    }
				//    else if (User.IsInRole(nameof(RolesUsuario.LABORATORISTA)))
				//    {
				//        return nameof(RolesUsuario.LABORATORISTA);
				//    }
				//    else if (User.IsInRole(nameof(RolesUsuario.VENDEDOR)))
				//    {
				//        return nameof(RolesUsuario.VENDEDOR);
				//    }
				//}
				#endregion
				if (string.IsNullOrEmpty(rolesUser)) { return string.Empty; }
				return rolesUser;

			}
		}

		public Guid IdUsuario
		{
			get
			{
				var handler = new JwtSecurityTokenHandler(); //Libreria System.IdentityModel.Token.Jwt (6.7.1)
				var tokenS = handler.ReadToken(TokenCookie) as JwtSecurityToken;
				var id = tokenS.Claims.First(c => c.Type.Contains("id")).Value;
				if (string.IsNullOrEmpty(id)) { return default; }
				return id.ToGuid();
			}
		}

		public string UserName
		{
			get
			{
				var handler = new JwtSecurityTokenHandler(); //Libreria System.IdentityModel.Token.Jwt (6.7.1)
				var tokenS = handler.ReadToken(TokenCookie) as JwtSecurityToken;
				var usuario = tokenS.Claims.First(c => c.Type.Contains("user")).Value;
				if (string.IsNullOrEmpty(usuario)) { return string.Empty; }
				return usuario;
			}
		}

		public IQueryable<T> OrdenarEntidad<T>(IQueryable<T> lista, string sortdir, string sort) where T : Dto
		{
			IQueryable<T> query = null;
			query = lista.AsQueryable().OrderBy($"{sort} {sortdir}");

			return query;
		}

		public List<UsuarioMenu> PermisosMenuPorUsuario
		{
			get
			{
				string json = _context.HttpContext.Session.GetString("PermisosMenuPorUsuario");
				if (string.IsNullOrEmpty(json))
				{
					return new();
				}
				return JsonConvert.DeserializeObject<List<UsuarioMenu>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("PermisosMenuPorUsuario", json);
			}
		}

		#region Variables globales
		protected bool ElementoEditado
		{
			get
			{
				string json = _context.HttpContext.Session.GetString("ElementoEditado");
				if (string.IsNullOrEmpty(json))
				{
					return new();
				}
				return JsonConvert.DeserializeObject<bool>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("ElementoEditado", json);
			}
		}
		protected ProductoBusquedaDto ProductoBase
		{
			get
			{
				string json = _context.HttpContext.Session.GetString("ProductoBase");
				if (string.IsNullOrEmpty(json))
				{
					return new();
				}
				return JsonConvert.DeserializeObject<ProductoBusquedaDto>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("ProductoBase", json);
			}
		}

		public List<ProductoBusquedaDto> ProductosSeleccionados
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("ProductosSeleccionados");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<ProductoBusquedaDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("ProductosSeleccionados", json);
			}
		}
		#endregion

		#region COMPRAS
		public List<TipoComprobanteDto> TiposComprobantePorCuenta
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("TiposComprobantePorCuenta");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<TipoComprobanteDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("TiposComprobantePorCuenta", json);
			}
		}
		public CuentaDto CuentaComercialSeleccionada
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("CuentaComercialSeleccionada");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return null;
				}
				return JsonConvert.DeserializeObject<CuentaDto>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("CuentaComercialSeleccionada", json);
			}
		}

		public AutoComptesPendientesDto RPRAutorizacionSeleccionada
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("RPRAutorizacionSeleccionada");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return null;
				}
				return JsonConvert.DeserializeObject<AutoComptesPendientesDto>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("RPRAutorizacionSeleccionada", json);
			}
		}

		public JsonDeRPDto JsonDeRPVerCompte
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("JsonDeRPVerCompte");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return null;
				}
				return JsonConvert.DeserializeObject<JsonDeRPDto>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("JsonDeRPVerCompte", json);
			}
		}

		public JsonDeRPDto JsonDeRP
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("JsonDeRP");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return null;
				}
				return JsonConvert.DeserializeObject<JsonDeRPDto>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("JsonDeRP", json);
			}
		}

		public RPRDetalleComprobanteDeRP RPRComprobanteDeRPSeleccionado
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("RPRComprobanteDeRPSeleccionado");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return null;
				}
				return JsonConvert.DeserializeObject<RPRDetalleComprobanteDeRP>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("RPRComprobanteDeRPSeleccionado", json);
			}
		}

		protected List<RPRComptesDeRPDto> RPRComptesDeRPRegs
		{
			get
			{
				string json = _context.HttpContext.Session.GetString("RPRComptesDeRPRegs");
				if (string.IsNullOrEmpty(json))
				{
					return new();
				}
				return JsonConvert.DeserializeObject<List<RPRComptesDeRPDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("RPRComptesDeRPRegs", json);
			}
		}

		protected List<RPRItemVerCompteDto> RPRItemVerCompteLista
		{
			get
			{
				string json = _context.HttpContext.Session.GetString("RPRItemVerCompteLista");
				if (string.IsNullOrEmpty(json))
				{
					return new();
				}
				return JsonConvert.DeserializeObject<List<RPRItemVerCompteDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("RPRItemVerCompteLista", json);
			}
		}

		protected List<RPRVerConteoDto> RPRItemVerConteoLista
		{
			get
			{
				string json = _context.HttpContext.Session.GetString("RPRItemVerConteoLista");
				if (string.IsNullOrEmpty(json))
				{
					return new();
				}
				return JsonConvert.DeserializeObject<List<RPRVerConteoDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("RPRItemVerConteoLista", json);
			}
		}

		protected List<ProductoBusquedaDto> RPRDetalleDeProductosEnRP
		{
			get
			{
				string json = _context.HttpContext.Session.GetString("RPRDetalleDeProductosEnRP");
				if (string.IsNullOrEmpty(json))
				{
					return new();
				}
				return JsonConvert.DeserializeObject<List<ProductoBusquedaDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("RPRDetalleDeProductosEnRP", json);
			}
		}

		protected List<AutoComptesPendientesDto> RPRAutorizacionesPendientesEnRP
		{
			get
			{
				string json = _context.HttpContext.Session.GetString("RPRAutorizacionesPendientesEnRP");
				if (string.IsNullOrEmpty(json))
				{
					return new();
				}
				return JsonConvert.DeserializeObject<List<AutoComptesPendientesDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("RPRAutorizacionesPendientesEnRP", json);
			}
		}
		#endregion

		#region TRANSFERENCIAS
		protected string TRDepositosSeleccionados
		{
			get
			{
				string json = _context.HttpContext.Session.GetString("TRDepositosSeleccionados");
				if (string.IsNullOrEmpty(json))
				{
					return string.Empty;
				}
				return JsonConvert.DeserializeObject<string>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("TRDepositosSeleccionados", json);
			}
		}
		protected List<TRAutPIDto> TRAutPedidosSucursalLista //ListaPedidosSucursal
		{
			get
			{
				string json = _context.HttpContext.Session.GetString("TRAutPedidosSucursalLista");
				if (string.IsNullOrEmpty(json))
				{
					return new();
				}
				return JsonConvert.DeserializeObject<List<TRAutPIDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("TRAutPedidosSucursalLista", json);
			}
		}
		protected List<TRAutPIDto> TRAutPedidosIncluidosILista
		{
			get
			{
				string json = _context.HttpContext.Session.GetString("TRAutPedidosIncluidosILista");
				if (string.IsNullOrEmpty(json))
				{
					return new();
				}
				return JsonConvert.DeserializeObject<List<TRAutPIDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("TRAutPedidosIncluidosILista", json);
			}
		}
		protected List<TRAutSucursalesDto> TRSucursalesLista
		{
			get
			{
				string json = _context.HttpContext.Session.GetString("TRSucursalesLista");
				if (string.IsNullOrEmpty(json))
				{
					return new();
				}
				return JsonConvert.DeserializeObject<List<TRAutSucursalesDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("TRSucursalesLista", json);
			}
		}
		protected List<TRAutAnalizaDto> TRAutAnaliza
		{
			get
			{
				string json = _context.HttpContext.Session.GetString("TRAutAnaliza");
				if (string.IsNullOrEmpty(json))
				{
					return new();
				}
				return JsonConvert.DeserializeObject<List<TRAutAnalizaDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("TRAutAnaliza", json);
			}
		}
		protected List<TRNuevaAutSucursalDto> TRNuevaAutSucursalLista
		{
			get
			{
				string json = _context.HttpContext.Session.GetString("TRNuevaAutSucursalLista");
				if (string.IsNullOrEmpty(json))
				{
					return new();
				}
				return JsonConvert.DeserializeObject<List<TRNuevaAutSucursalDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("TRNuevaAutSucursalLista", json);
			}
		}
		protected List<TRNuevaAutDetalleDto> TRNuevaAutDetallelLista
		{
			get
			{
				string json = _context.HttpContext.Session.GetString("TRNuevaAutDetallelLista");
				if (string.IsNullOrEmpty(json))
				{
					return new();
				}
				return JsonConvert.DeserializeObject<List<TRNuevaAutDetalleDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("TRNuevaAutDetallelLista", json);
			}
		}
		#endregion

		#region AJUSTES DE STOCK
		protected List<AjustePrevioCargadoDto> AjustePrevioCargadoLista 
		{
			get
			{
				string json = _context.HttpContext.Session.GetString("AjustePrevioCargadoLista");
				if (string.IsNullOrEmpty(json))
				{
					return new();
				}
				return JsonConvert.DeserializeObject<List<AjustePrevioCargadoDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("AjustePrevioCargadoLista", json);
			}
		}
		protected List<ProductoAAjustarDto> AjusteProductosLista
		{
			get
			{
				string json = _context.HttpContext.Session.GetString("AjusteProductosLista");
				if (string.IsNullOrEmpty(json))
				{
					return new();
				}
				return JsonConvert.DeserializeObject<List<ProductoAAjustarDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("AjusteProductosLista", json);
			}
		}
		protected List<DepositoDto> DepositoLista
		{
			get
			{
				string json = _context.HttpContext.Session.GetString("DepositoLista");
				if (string.IsNullOrEmpty(json))
				{
					return new();
				}
				return JsonConvert.DeserializeObject<List<DepositoDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("DepositoLista", json);
			}
		}
		#endregion

		#region Metodos generales
		public PartialViewResult ObtenerMensajeDeError(string mensaje)
		{
			RespuestaGenerica<EntidadBase> response = new()
			{
				Ok = false,
				EsError = true,
				EsWarn = false,
				Mensaje = mensaje
			};
			return PartialView("_gridMensaje", response);
		}
		public static decimal ConvertToDecimal(string value, int precision)
		{
			if (value.Contains('.'))
			{
				var splited = value.Split('.');
				value = splited[0];
				value += ".";
				value = value.PadRight(precision, '0');
			}
			else
				value += ".000";
			if (!decimal.TryParse(value, out decimal converted))
			{
				return 0.000M;
			}
			return converted;
		}
		public GridCore<T> ObtenerGridCore<T>(List<T> lista) where T : Dto
		{
			var listaDetalle = new StaticPagedList<T>(lista, 1, 999, lista.Count);
			return new GridCore<T>() { ListaDatos = listaDetalle, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Item", SortDir = "ASC" };
		}
		#endregion
	}
}
