using gc.api.core.Entidades;
using gc.infraestructura.Constantes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.AjusteDeStock;
using gc.infraestructura.Dtos.Almacen.Info;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Almacen.Tr;
using gc.infraestructura.Dtos.Almacen.Tr.Remito;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.EntidadesComunes;
using gc.infraestructura.EntidadesComunes.Options;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Dynamic.Core;
using X.PagedList;

namespace gc.pocket.site.Controllers
{
    public class ControladorBase : Controller
    {
        private readonly AppSettings _options;
        private readonly MenuSettings? _menuSettings;
        protected readonly IHttpContextAccessor _context;
        public List<Orden> _orden;
        private readonly ILogger _logger;

        public ControladorBase(IOptions<AppSettings> options, IOptions<MenuSettings> options1, IHttpContextAccessor contexto,ILogger logger)
        {
            _options = options.Value;
            _menuSettings = options1.Value;
            _context = contexto;
            _logger = logger;
        }
        public ControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto)
        {
            _options = options.Value;
            _menuSettings = null;
            _context = contexto;
        }

        protected AppItem ObtenerModulo(string sigla)
        {
            if (_menuSettings == null)
            {
                throw new NegocioException("No se encuentra cargada la configuración de los modulos. Verifique funcianalidad.");
            }
            return _menuSettings.Aplicaciones.SingleOrDefault(m => m.Sigla.Equals(sigla));
        }



        public string NombreSitio
        {
            get { return _options.Nombre; }
        }

        #region Autenticación

        public string Etiqueta
        {
            get { return _context.HttpContext.Session.GetString("Etiqueta"); }

            set { HttpContext.Session.SetString("Etiqueta", value); }
        }

        public string Token
        {
            get { return _context.HttpContext.Session.GetString("JwtToken"); }

            set { HttpContext.Session.SetString("JwtToken", value); }
        }

        public string TokenCookie
        {
            get
            {
                //la Cookie se resgardará con una etiqueta distinta cada vez que se mandara desde el servidor. 
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
        #endregion

        public string BoxSeleccionado
        {
            get
            {
                var box = _context.HttpContext.Session.GetString("BoxSeleccionado");
                if (string.IsNullOrEmpty(box) || string.IsNullOrWhiteSpace(box))
                {
                    return string.Empty;
                }
                return box.Trim().ToUpper();
            }
            set { _context.HttpContext.Session.SetString("BoxSeleccionado", value); }
        }

        public string TipoAjusteStk
        {
            get
            {
                var taj = _context.HttpContext.Session.GetString("TipoAjusteStk");
                if (string.IsNullOrEmpty(taj) || string.IsNullOrWhiteSpace(taj))
                {
                    return string.Empty;
                }
                return taj.Trim().ToUpper();
            }
            set { _context.HttpContext.Session.SetString("TipoAjusteStk", value); }
        }

        public List<TipoAjusteDeStockDto> ListaTipoAjuste
        {
            get
            {
                var json = _context.HttpContext.Session.GetString("ListaTipoAjuste");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return new List<TipoAjusteDeStockDto>();
                }
                return JsonConvert.DeserializeObject<List<TipoAjusteDeStockDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("ListaTipoAjuste", json);
            }
        }

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

        public string IdentificaModulo
        {
            get
            {
                AutorizacionTIDto sel = TIActual;
                return sel.TipoTI.Equals("S") ? "S => S" :
                    sel.TipoTI.Equals("D") ? "D => D " :
                    sel.TipoTI.Equals("E") ? "D => D (S/AU)" :
                    sel.TipoTI.Equals("B") ? "B => B" :
                    sel.TipoTI.Equals("O") ? "B => B (S/AU)" : "DEBE REINICIAR MODULO";
            }
        }

        public List<TiListaProductoDto> ProductosParaControlar
        {

            get
            {
                var json = _context.HttpContext.Session.GetString("ProductosParaControlar");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<TiListaProductoDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("ProductosParaControlar", json);
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

        #region Variables de Productos que podrían ser utilizados en InfoProd
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
        #endregion

        #region Variables de Session para modulo InfoProd
        #region InfoProdStkD
        protected string InfoProdStkDId
        {

            get
            {
                string json = _context.HttpContext.Session.GetString("InfoProdStkDId");
                if (string.IsNullOrEmpty(json))
                {
                    return string.Empty;
                }
                return JsonConvert.DeserializeObject<string>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("InfoProdStkDId", json);
            }
        }
        protected List<InfoProdStkD> InfoProdStkDRegs
        {
            get
            {
                string json = _context.HttpContext.Session.GetString("InfoProdStkDRegs");
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<List<InfoProdStkD>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("InfoProdStkDRegs", json);
            }
        }
        #endregion
        #region InfoProdStkBoxes
        /// <summary>
        /// esta propiedad tiene 2 parametros (id y depo)
        /// </summary>
        protected (string, string) InfoProdStkBoxesIds
        {
            get
            {
                string json = _context.HttpContext.Session.GetString("InfoProdStkBoxesIds");
                if (string.IsNullOrEmpty(json))
                {
                    return (string.Empty, string.Empty);
                }
                return JsonConvert.DeserializeObject<(string, string)>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("InfoProdStkBoxesIds", json);
            }
        }
        protected List<InfoProdStkBox> InfoProdStkBoxesRegs
        {
            get
            {
                string json = _context.HttpContext.Session.GetString("InfoProdStkBoxesRegs");
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<List<InfoProdStkBox>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("InfoProdStkBoxesRegs", json);
            }
        }
        #endregion
        #region InfoProdStkA
        protected string InfoProdStkAId
        {

            get
            {
                string json = _context.HttpContext.Session.GetString("InfoProdStkAId");
                if (string.IsNullOrEmpty(json))
                {
                    return string.Empty;
                }
                return JsonConvert.DeserializeObject<string>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("InfoProdStkAId", json);
            }
        }
        protected List<InfoProdStkA> InfoProdStkARegs
        {
            get
            {
                string json = _context.HttpContext.Session.GetString("InfoProdStkARegs");
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<List<InfoProdStkA>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("InfoProdStkARegs", json);
            }
        }
        #endregion
        #region InfoProdMovStk
        /// <summary>
        /// las propiedades id depo tmov desde y hasta estan separadas por #
        /// id#depo#tmov#desde#hasta en un solo string.
        /// con ella se splitea el string generando un array de 5 componentes
        /// </summary>
        protected string InfoProdMovStkIds
        {
            get
            {
                string json = _context.HttpContext.Session.GetString("InfoProdMovStkIds");
                if (string.IsNullOrEmpty(json))
                {
                    return string.Empty;
                }
                return JsonConvert.DeserializeObject<string>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("InfoProdMovStkIds", json);
            }
        }
        protected List<InfoProdMovStk> InfoProdMovStkRegs
        {
            get
            {
                string json = _context.HttpContext.Session.GetString("InfoProdMovStkRegs");
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<List<InfoProdMovStk>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("InfoProdMovStkRegs", json);
            }
        }
        #endregion
        #region InfoProdLP
        protected string InfoProdLPId
        {

            get
            {
                string json = _context.HttpContext.Session.GetString("InfoProdLPId");
                if (string.IsNullOrEmpty(json))
                {
                    return string.Empty;
                }
                return JsonConvert.DeserializeObject<string>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("InfoProdLPId", json);
            }
        }
        protected List<InfoProdLP> InfoProdLPRegs
        {
            get
            {
                string json = _context.HttpContext.Session.GetString("InfoProdLPRegs");
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<List<InfoProdLP>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("InfoProdLPRegs", json);
            }
        }
        #endregion
        #endregion

        #region Variables de Session para módulo RPR
        public List<AutorizacionPendienteDto> AutorizacionesPendientes
        {
            get
            {
                var json = _context.HttpContext.Session.GetString("AutorizacionesPendientes");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<AutorizacionPendienteDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("AutorizacionesPendientes", json);
            }
        }

        public AutorizacionPendienteDto AutorizacionPendienteSeleccionada
        {
            get
            {
                var json = _context.HttpContext.Session.GetString("AutorizacionPendienteSeleccionada");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return null;
                }
                return JsonConvert.DeserializeObject<AutorizacionPendienteDto>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("AutorizacionPendienteSeleccionada", json);
            }
        }

        protected ProductoGenDto ProductoTemp
        {
            get
            {
                string json = _context.HttpContext.Session.GetString("RPRProductoTemp");
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<ProductoGenDto>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("RPRProductoTemp", json);
            }
        }

        protected List<ProductoGenDto> ProductoGenRegs
        {
            get
            {
                string json = _context.HttpContext.Session.GetString("ProductoGenRegs");
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<List<ProductoGenDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("ProductoGenRegs", json);
            }
        }



        #endregion
        #region Variables para RTI
        public List<RemitoGenDto> RemitosPendientes
        {
            get
            {
                var json = _context.HttpContext.Session.GetString("RemitosPendientes");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<RemitoGenDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("RemitosPendientes", json);
            }
        }

        public RemitoGenDto RemitoActual
        {
            get
            {
                var json = _context.HttpContext.Session.GetString("RemitoActual");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return null;
                }
                return JsonConvert.DeserializeObject<RemitoGenDto>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("RemitoActual", json);
            }
        }

        #endregion

        #region Variables de Session para módulo TI

        public string TI_ModId
        {
            get
            {
                var txt = _context.HttpContext.Session.GetString("TIModuloActual");
                if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
                {
                    return Constantes.ValoresDefault.TI_MODID;
                }
                return txt;
            }
            set
            {

                _context.HttpContext.Session.SetString("TIModuloActual", value);
            }
        }

        public bool TI_CS
        {
            get
            {
                var txt = _context.HttpContext.Session.GetString("TIControlSalida");
                if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
                {
                    return false;
                }
                return txt.ToBoolean();
            }
            set
            {
                var valor = value.ToString();
                _context.HttpContext.Session.SetString("TIControlSalida", valor);
            }
        }


        public AutorizacionTIDto TIActual
        {
            get
            {
                var json = _context.HttpContext.Session.GetString("TIActual");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<AutorizacionTIDto>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("TIActual", json);
            }
        }
        public List<AutorizacionTIDto> ListadoTIAutoPendientes
        {
            get
            {
                var json = _context.HttpContext.Session.GetString("ListadoTIAutoPendientes");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<AutorizacionTIDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("ListadoTIAutoPendientes", json);
            }
        }

        public List<TiListaProductoDto> ListaProductosActual
        {
            get
            {
                var json = _context.HttpContext.Session.GetString("ListaProductosActual");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<TiListaProductoDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("ListaProductosActual", json);
            }
        }

        public List<TiListaProductoDto> ListaProductosSegunBox
        {
            get
            {
                var json = _context.HttpContext.Session.GetString("ListaProductosSegunBox");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<TiListaProductoDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("ListaProductosSegunBox", json);
            }
        }

        public List<TipoMotivoDto> TiposMotivo
        {
            get
            {
                var json = _context.HttpContext.Session.GetString("TiposMotivo");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<TipoMotivoDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("TiposMotivo", json);
            }
        }

        #endregion

        protected void PresentaMensaje(string error, string warn, string info)
        {
            if (!string.IsNullOrEmpty(error))
            {
                TempData["error"] = error;
            }
            if (!string.IsNullOrEmpty(warn))
            {
                TempData["warn"] = warn;
            }
            if (!string.IsNullOrEmpty(info))
            {
                TempData["info"] = info;
            }
        }

        public IQueryable<T> OrdenarEntidad<T>(IQueryable<T> lista, string sortdir, string sort) where T : Dto
        {
            IQueryable<T> query = null;
            query = lista.AsQueryable().OrderBy($"{sort} {sortdir}");

            return query;
        }

        public List<T> OrdenarEntidad<T>(List<T> lista,string sortdir,string sort) where T : Dto
        {
            IQueryable<T> result ;
            result = lista.AsQueryable().OrderBy($"{sort} {sortdir}");
            return result.ToList();
        }

        protected GridCore<T> GenerarGrilla<T>(List<T>? lista, string nnCol,int cantReg,int pagina,int totalReg,int totalPag=0,string sortDir = "ASC")
        {
            var l = new StaticPagedList<T>(lista,pagina, cantReg, lista.Count);

            return new GridCore<T>() { ListaDatos = l, CantidadReg = cantReg, PaginaActual = pagina, CantidadPaginas = totalPag, Sort = nnCol, SortDir = sortDir};
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

   
        protected async Task<IActionResult> BusquedaAvanzada(string ri01,string ri02,bool act,bool dis,bool ina,bool cstk,bool sstk,string search, IProductoServicio _productoServicio)
        {
            GridCore<ProductoListaDto> grillaDatos;
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                var busc = new BusquedaProducto { Busqueda=search,ConStock=cstk,SinStock=sstk,CtaProveedorId=ri01,
                RubroId=ri02,EstadoActivo=act,EstadoDiscont=dis,EstadoInactivo=ina};

                var res = await _productoServicio.BusquedaListaProductos(busc, TokenCookie);
                if(res.Item1.Count > 0){

                }
                grillaDatos = GenerarGrilla<ProductoListaDto>(res.Item1, "p_desc");
                return PartialView("_gridProdsAdv", grillaDatos);
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda Avanzada";
                _logger.LogError(ex, "Error en la invocación de la API - Busqueda Avanzada");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        #endregion
    }
}
