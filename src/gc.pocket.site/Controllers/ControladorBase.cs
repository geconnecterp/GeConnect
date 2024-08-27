﻿using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.EntidadesComunes;
using gc.infraestructura.EntidadesComunes.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Dynamic.Core;

namespace gc.pocket.site.Controllers
{
    public class ControladorBase : Controller
    {
        private readonly AppSettings _options;
        private readonly MenuSettings? _menuSettings;
        protected readonly IHttpContextAccessor _context;
        public List<Orden> _orden;


        public ControladorBase(IOptions<AppSettings> options, IOptions<MenuSettings> options1, IHttpContextAccessor contexto)
        {
            _options = options.Value;
            _menuSettings = options1.Value;
            _context = contexto;
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

     
        public string Token
        {
            get { return _context.HttpContext.Session.GetString("JwtToken"); }

            set { HttpContext.Session.SetString("JwtToken", value); }
        }

        public string TokenCookie
        {
            get
            {
                var nombre = User.Claims.First(c => c.Type.Contains("name")).Value;
                return _context.HttpContext.Request.Cookies[nombre];
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
                return adm;
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
        public List<RPRAutorizacionPendienteDto> AutorizacionesPendientes
        {
            get
            {
                var json = _context.HttpContext.Session.GetString("AutorizacionesPendientes");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<RPRAutorizacionPendienteDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("AutorizacionesPendientes", json);
            }
        }

        public RPRAutorizacionPendienteDto RPRAutorizacionPendienteSeleccionada
        {
            get
            {
                var json = _context.HttpContext.Session.GetString("AutorizacionPendienteSeleccionada");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return null;
                }
                return JsonConvert.DeserializeObject<RPRAutorizacionPendienteDto>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("AutorizacionPendienteSeleccionada", json);
            }
        }

        protected RPRProcuctoDto RPRProductoTemp
        {
            get
            {
                string json = _context.HttpContext.Session.GetString("RPRProductoTemp");
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<RPRProcuctoDto>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("RPRProductoTemp", json);
            }
        }

        protected List<RPRProcuctoDto> RPRProductoRegs
        {
            get
            {
                string json = _context.HttpContext.Session.GetString("RPRProductoRegs");
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<List<RPRProcuctoDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("RPRProductoRegs", json);
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
    }
}