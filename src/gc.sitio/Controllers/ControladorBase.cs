using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.AjusteDeStock;
using gc.infraestructura.Dtos.Almacen.DevolucionAProveedor;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Almacen.Tr.Transferencia;
using gc.infraestructura.Dtos.CuentaComercial;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
		private readonly ILogger _logger;

		public ControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger)
		{
			_options = options.Value;
			_context = contexto;
			_logger = logger;
		}

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

				if (tokenS == null)
					return false;
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
				if (tokenS == null)
					return string.Empty;
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
				if (tokenS == null)
					return Guid.Empty;
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
				if (tokenS == null)
					return string.Empty;
				var usuario = tokenS.Claims.First(c => c.Type.Contains("user")).Value;
				if (string.IsNullOrEmpty(usuario)) { return string.Empty; }
				return usuario;
			}
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

		protected AutoComptesPendientesDto RPRAutorizacionPendienteSeleccionadoEnLista
		{
			get
			{
				string json = _context.HttpContext.Session.GetString("RPRAutorizacionPendienteSeleccionadoEnLista");
				if (string.IsNullOrEmpty(json))
				{
					return new();
				}
				return JsonConvert.DeserializeObject<AutoComptesPendientesDto>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("RPRAutorizacionPendienteSeleccionadoEnLista", json);
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

		#region DEVOLUCION A PROVEEDOR
		protected List<ProductoADevolverDto> DevolucionProductosLista
		{
			get
			{
				string json = _context.HttpContext.Session.GetString("DevolucionProductosLista");
				if (string.IsNullOrEmpty(json))
				{
					return new();
				}
				return JsonConvert.DeserializeObject<List<ProductoADevolverDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("DevolucionProductosLista", json);
			}
		}
		protected List<DevolucionPrevioCargadoDto> DevolucionPrevioCargadoLista
		{
			get
			{
				string json = _context.HttpContext.Session.GetString("DevolucionPrevioCargadoLista");
				if (string.IsNullOrEmpty(json))
				{
					return new();
				}
				return JsonConvert.DeserializeObject<List<DevolucionPrevioCargadoDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("DevolucionPrevioCargadoLista", json);
			}
		}
		#endregion

		#region PROVEEDOR
		public List<ProveedorListaDto> ProveedoresLista
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("ProveedoresLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<ProveedorListaDto>();
				}
				return JsonConvert.DeserializeObject<List<ProveedorListaDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("ProveedoresLista", json);
			}
		}
		#endregion

		#region RUBRO
		public List<RubroListaDto> RubroLista
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("RubroLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<RubroListaDto>();
				}
				return JsonConvert.DeserializeObject<List<RubroListaDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("RubroLista", json);
			}
		}
		#endregion

		#region TIPO DE NEGOCIO
		public List<TipoNegocioDto> TipoNegocioLista
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("TipoNegocioLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<TipoNegocioDto>();
				}
				return JsonConvert.DeserializeObject<List<TipoNegocioDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("TipoNegocioLista", json);
			}
		}
		#endregion

		#region ZONAS
		public List<ZonaDto> ZonasLista
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("ZonasLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<ZonaDto>();
				}
				return JsonConvert.DeserializeObject<List<ZonaDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("ZonasLista", json);
			}
		}
		#endregion

		#region DOCUMENTOS TIPO
		public List<TipoDocumentoDto> TipoDocumentoLista
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("TipoDocumentoLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<TipoDocumentoDto>();
				}
				return JsonConvert.DeserializeObject<List<TipoDocumentoDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("TipoDocumentoLista", json);
			}
		}
		#endregion

		#region CONDICIONES AFIP
		public List<CondicionAfipDto> CondicionesAfipLista
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("CondicionesAfipLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<CondicionAfipDto>();
				}
				return JsonConvert.DeserializeObject<List<CondicionAfipDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("CondicionesAfipLista", json);
			}
		}
		#endregion

		#region CONDICIONES AFIP
		public List<CondicionIBDto> CondicionIBLista
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("CondicionIBLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<CondicionIBDto>();
				}
				return JsonConvert.DeserializeObject<List<CondicionIBDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("CondicionIBLista", json);
			}
		}
		#endregion

		#region NATURALEZA JURIDICA
		public List<NaturalezaJuridicaDto> NaturalezaJuridicaLista
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("NaturalezaJuridicaLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<NaturalezaJuridicaDto>();
				}
				return JsonConvert.DeserializeObject<List<NaturalezaJuridicaDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("NaturalezaJuridicaLista", json);
			}
		}
		#endregion

		#region DEPARTAMENTOS
		public List<DepartamentoDto> DepartamentoLista
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("DepartamentoLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<DepartamentoDto>();
				}
				return JsonConvert.DeserializeObject<List<DepartamentoDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("DepartamentoLista", json);
			}
		}
		#endregion

		#region FORMA DE PAGO
		public List<FormaDePagoDto> FormaDePagoLista
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("FormaDePagoLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<FormaDePagoDto>();
				}
				return JsonConvert.DeserializeObject<List<FormaDePagoDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("FormaDePagoLista", json);
			}
		}
		#endregion

		#region TIPO DE PAGO
		public List<TipoContactoDto> TipoContactoLista
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("TipoContactoLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<TipoContactoDto>();
				}
				return JsonConvert.DeserializeObject<List<TipoContactoDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("TipoContactoLista", json);
			}
		}
		#endregion

		#region PROVINCIA
		public List<ProvinciaDto> ProvinciaLista
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("ProvinciaLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<ProvinciaDto>();
				}
				return JsonConvert.DeserializeObject<List<ProvinciaDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("ProvinciaLista", json);
			}
		}
		#endregion

		#region TIPO CANAL
		public List<TipoCanalDto> TipoCanalLista
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("TipoCanalLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<TipoCanalDto>();
				}
				return JsonConvert.DeserializeObject<List<TipoCanalDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("TipoCanalLista", json);
			}
		}
		#endregion

		#region TIPO CUENTA BANCO
		public List<TipoCuentaBcoDto> TipoCuentaBcoLista
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("TipoCuentaBcoLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<TipoCuentaBcoDto>();
				}
				return JsonConvert.DeserializeObject<List<TipoCuentaBcoDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("TipoCuentaBcoLista", json);
			}
		}
		#endregion

		#region LISTA DE PRECIOS
		public List<ListaPrecioDto> ListaDePreciosLista
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("ListaDePreciosLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<ListaPrecioDto>();
				}
				return JsonConvert.DeserializeObject<List<ListaPrecioDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("ListaDePreciosLista", json);
			}
		}
		#endregion

		#region VENDEDORES
		public List<VendedorDto> VendedoresLista
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("VendedoresLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<VendedorDto>();
				}
				return JsonConvert.DeserializeObject<List<VendedorDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("VendedoresLista", json);
			}
		}
		#endregion

		#region REPARTIDORES
		public List<RepartidorDto> RepartidoresLista
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("RepartidoresLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<RepartidorDto>();
				}
				return JsonConvert.DeserializeObject<List<RepartidorDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("RepartidoresLista", json);
			}
		}
		#endregion

		#region DIA DE LA SEMANA
		public List<DiaDeLaSemanaDto> DiasDeLaSemanaLista
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("DiasDeLaSemanaLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<DiaDeLaSemanaDto>();
				}
				return JsonConvert.DeserializeObject<List<DiaDeLaSemanaDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("DiasDeLaSemanaLista", json);
			}
		}
		#endregion

		#region FINANCIEROS
		public List<FinancieroDto> FinancierosLista
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("FinancierosLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<FinancieroDto>();
				}
				return JsonConvert.DeserializeObject<List<FinancieroDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("FinancierosLista", json);
			}
		}
		#endregion

		#region TIPO OBSERVACIONES
		public List<TipoObsDto> TipoObservacionesLista
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("TipoObservacionesLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<TipoObsDto>();
				}
				return JsonConvert.DeserializeObject<List<TipoObsDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("TipoObservacionesLista", json);
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

		public IQueryable<T> OrdenarEntidad<T>(IQueryable<T> lista, string sortdir, string sort) where T : Dto
		{
			IQueryable<T> query = null;
			query = lista.AsQueryable().OrderBy($"{sort} {sortdir}");

			return query;
		}

		public List<T> OrdenarEntidad<T>(List<T> lista, string sortdir, string sort) where T : Dto
		{
			IQueryable<T> result;
			result = lista.AsQueryable().OrderBy($"{sort} {sortdir}");
			return result.ToList();
		}

		protected GridCore<T> GenerarGrilla<T>(List<T>? lista, string nnCol, int cantReg, int pagina, int totalReg, int totalPag = 0, string sortDir = "ASC")
		{
			var l = new StaticPagedList<T>(lista, pagina, cantReg, lista != null ? lista.Count : 0);

			return new GridCore<T>() { ListaDatos = l, CantidadReg = cantReg, PaginaActual = pagina, CantidadPaginas = totalPag, Sort = nnCol, SortDir = sortDir };
		}
		protected GridCore<T> GenerarGrilla<T>(List<T>? lista, string sort)
		{
			return GenerarGrilla(lista, sort, _options.NroRegistrosPagina, 1, 99999);
		}

		#region Metodos unicos para realizar busquedas con autocomplete
		protected void ObtenerRubros(IRubroServicio _rubSv)
		{
			RubroLista = _rubSv.ObtenerListaRubros(TokenCookie);
		}

		protected void ObtenerProveedores(ICuentaServicio _ctaSv)
		{
			//se guardan los proveedores en session. Para ser utilizados posteriormente

			ProveedoresLista = _ctaSv.ObtenerListaProveedores(TokenCookie);
		}

		protected void ObtenerTiposNegocio(ITipoNegocioServicio _tipoNegSv)
		{
			//se guardan los tipos de negocio en session. Para ser utilizados posteriormente

			TipoNegocioLista = _tipoNegSv.ObtenerTiposDeNegocio(TokenCookie);
		}

		protected void ObtenerZonas(IZonaServicio _zonaSv)
		{
			//se guardan las zonas en session. Para ser utilizados posteriormente

			ZonasLista = _zonaSv.GetZonaLista(TokenCookie);
		}

		protected void ObtenerCondicionesAfip(ICondicionAfipServicio _condAfip)
		{
			CondicionesAfipLista = _condAfip.GetCondicionesAfipLista(TokenCookie);
		}

		protected void ObtenerNaturalezaJuridica(INaturalezaJuridicaServicio _natJur)
		{
			NaturalezaJuridicaLista = _natJur.GetNaturalezaJuridicaLista(TokenCookie);
		}

		protected void ObtenerCondicionesIB(ICondicionIBServicio _condIB)
		{
			CondicionIBLista = _condIB.GetCondicionIBLista(TokenCookie);
		}

		protected void ObtenerProvincias(IProvinciaServicio _provin)
		{
			ProvinciaLista = _provin.GetProvinciaLista(TokenCookie);
		}

		protected void ObtenerFormasDePago(IFormaDePagoServicio _formPago)
		{
			FormaDePagoLista = _formPago.GetFormaDePagoLista(TokenCookie, "C");
		}

		protected void ObtenerTiposDeCanal(ITipoCanalServicio _tipoCanal)
		{
			TipoCanalLista = _tipoCanal.GetTipoCanalLista(TokenCookie);
		}

		protected void ObtenerTiposDeCuentaBco(ITipoCuentaBcoServicio _tipoCueBco)
		{
			TipoCuentaBcoLista = _tipoCueBco.GetTipoCuentaBcoLista(TokenCookie);
		}

		protected void ObtenerTiposDocumento(ITipoDocumentoServicio _tipoDoc)
		{
			TipoDocumentoLista = _tipoDoc.GetTipoDocumentoLista(TokenCookie);
		}
		protected void ObtenerDepartamentos(IDepartamentoServicio _depto, string prov_id)
		{
			DepartamentoLista = _depto.GetDepartamentoPorProvinciaLista(prov_id, TokenCookie);
		}
		protected void ObtenerListaDePrecios(IListaDePrecioServicio _listaPrec)
		{
			ListaDePreciosLista = _listaPrec.GetListaPrecio(TokenCookie);
		}
		protected void ObtenerListaDeVendedores(IVendedorServicio _vendedor)
		{
			VendedoresLista = _vendedor.GetVendedorLista(TokenCookie);
		}
		protected void ObtenerListaDeRepartidores(IRepartidorServicio _repartidor)
		{
			RepartidoresLista = _repartidor.GetRepartidorLista(TokenCookie);
		}
		protected void ObteneFinancieros(IFinancieroServicio _finan, string ctf_id)
		{
			FinancierosLista = _finan.GetFinancierosPorTipoCfLista(ctf_id, TokenCookie);
		}
		protected void ObtenerTipoContacto(ITipoContactoServicio _tipoCon, string tipo)
		{
			TipoContactoLista = _tipoCon.GetTipoContactoLista(TokenCookie, tipo);
		}
		protected void ObtenerTipoObservaciones(ITipoObsServicio _tipoObs, string tipo)
		{
			TipoObservacionesLista = _tipoObs.GetTiposDeObs(TokenCookie, tipo);
		}
		protected void ObtenerDiasDeLaSemana()
		{
			var listaTemp = new List<DiaDeLaSemanaDto>();
			var lista = Enum.GetValues(typeof(DiasDeLaSemana)).Cast<DiasDeLaSemana>().ToList();
			foreach (var item in lista)
			{
				var newItem = new DiaDeLaSemanaDto();
				newItem.dia_id = (int)item;
				newItem.dia_desc = item.ToString();
				listaTemp.Add(newItem);
			};
			DiasDeLaSemanaLista = listaTemp;
			lista.ForEach(x => DiasDeLaSemanaLista.Add(new DiaDeLaSemanaDto() { dia_id = (int)x, dia_desc = x.ToString() }));
		}

		#region COMBOS
		protected SelectList ComboAfip()
		{
			var lista = CondicionesAfipLista.Select(x => new ComboGenDto { Id = x.afip_id, Descripcion = x.afip_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		protected SelectList ComboNatJud()
		{
			var lista = NaturalezaJuridicaLista.Select(x => new ComboGenDto { Id = x.nj_id, Descripcion = x.nj_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		protected SelectList ComboTipoDoc()
		{
			var lista = TipoDocumentoLista.Select(x => new ComboGenDto { Id = x.Tdoc_Id, Descripcion = x.Tdoc_Desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		protected SelectList ComboIB()
		{
			var lista = CondicionIBLista.Select(x => new ComboGenDto { Id = x.id_id, Descripcion = x.ib_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		protected SelectList ComboProv()
		{
			var lista = ProvinciaLista.Select(x => new ComboGenDto { Id = x.prov_id, Descripcion = x.prov_nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		protected SelectList ComboTipoCuentaBco()
		{
			var lista = TipoCuentaBcoLista.Select(x => new ComboGenDto { Id = x.tcb_id, Descripcion = x.tcb_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		protected SelectList ComboTipoNegocio()
		{
			var lista = TipoNegocioLista.Select(x => new ComboGenDto { Id = x.ctn_id, Descripcion = x.ctn_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		protected SelectList ComboListaDePrecios()
		{
			var lista = ListaDePreciosLista.Select(x => new ComboGenDto { Id = x.lp_id, Descripcion = x.lp_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		protected SelectList ComboTipoCanal()
		{
			var lista = TipoCanalLista.Select(x => new ComboGenDto { Id = x.ctc_id, Descripcion = x.ctc_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		protected SelectList ComboVendedores()
		{
			var lista = VendedoresLista.Select(x => new ComboGenDto { Id = x.ve_id, Descripcion = x.ve_nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		protected SelectList ComboDiasDeLaSemana()
		{
			var lista = DiasDeLaSemanaLista.Select(x => new ComboGenDto { Id = x.dia_id.ToString(), Descripcion = x.dia_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		protected SelectList ComboZonas()
		{
			var lista = ZonasLista.Select(x => new ComboGenDto { Id = x.zn_id, Descripcion = x.zn_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		protected SelectList ComboRepartidores()
		{
			var lista = RepartidoresLista.Select(x => new ComboGenDto { Id = x.rp_id, Descripcion = x.rp_nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		protected SelectList ComboFormaDePago()
		{
			var lista = FormaDePagoLista.Select(x => new ComboGenDto { Id = x.fp_id, Descripcion = x.fp_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		protected SelectList ComboTipoContacto()
		{
			var lista = TipoContactoLista.Select(x => new ComboGenDto { Id = x.tc_id, Descripcion = x.tc_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		protected SelectList ComboTipoObservaciones()
		{
			var lista = TipoObservacionesLista.Select(x => new ComboGenDto { Id = x.to_id, Descripcion = x.to_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion

		[HttpPost]
		public JsonResult BuscarProvs(string prefix)
		{
			//var nombres = await _provSv.BuscarAsync(new QueryFilters { Search = prefix }, TokenCookie);
			//var lista = nombres.Item1.Select(c => new EmpleadoVM { Nombre = c.NombreCompleto, Id = c.Id, Cuil = c.CUIT });
			var prov = ProveedoresLista.Where(x => x.Cta_Lista.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var proveedores = prov.Select(x => new ComboGenDto { Id = x.Cta_Id, Descripcion = x.Cta_Lista });
			return Json(proveedores);
		}

		[HttpPost]
		public JsonResult BuscarRubros(string prefix)
		{
			//var nombres = await _provSv.BuscarAsync(new QueryFilters { Search = prefix }, TokenCookie);
			//var lista = nombres.Item1.Select(c => new EmpleadoVM { Nombre = c.NombreCompleto, Id = c.Id, Cuil = c.CUIT });
			var rub = RubroLista.Where(x => x.Rub_Desc.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var rubros = rub.Select(x => new ComboGenDto { Id = x.Rub_Id, Descripcion = x.Rub_Desc });
			return Json(rubros);
		}
		#endregion

		protected enum DiasDeLaSemana
		{
			Domingo = 1,
			Lunes = 2,
			Martes = 3,
			Miércoles = 4,
			Jueves = 5,
			Viernes = 6,
			Sábado = 7
		}
	}
}
