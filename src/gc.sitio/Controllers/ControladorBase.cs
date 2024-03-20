using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.EntidadesComunes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Dynamic.Core;

namespace gc.sitio.Controllers
{
    public class ControladorBase:Controller
    {
        private readonly IOptions<AppSettings> _options;

        public List<Orden> _orden;

        public ControladorBase(IOptions<AppSettings> options)
        {
            _options = options;

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
                string etiqueta = $"{User.Identity.Name}";
                return HttpContext.Request.Cookies[etiqueta];
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
                    var tokenS = handler.ReadToken(Token) as JwtSecurityToken;
                    var venc = tokenS.Claims.First(c => c.Type.Contains("expires")).Value;
                    expira = venc.ToDateTimeOrNull();
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
                var usuario = tokenS.Claims.First(c => c.Type.Contains("User")).Value;
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
    }
}
