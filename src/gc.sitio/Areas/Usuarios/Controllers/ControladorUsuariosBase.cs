using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Dtos.Users;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Usuarios.Controllers
{
    public class ControladorUsuariosBase:ControladorBase
    {
        private readonly AppSettings _setting;
        public ControladorUsuariosBase(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger logger) : base(options, accessor, logger)
        {
            _setting = options.Value;
        }

        public int PaginaActual
        {
            get
            {
                var txt = _context.HttpContext?.Session.GetString("PaginaActual");
                if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
                {
                    return 0;
                }
                return txt.ToInt();
            }
            set
            {
                var valor = value.ToString();
                _context.HttpContext?.Session.SetString("PaginaActual", valor);
            }
        }

        public List<PerfilDto> PerfilesBuscados
        {
            get
            {
                var json = _context.HttpContext?.Session.GetString("PerfilesBuscados");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<PerfilDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("PerfilesBuscados", json);
            }
        }

        protected string PerfilIDSeleccionado
        {
            get
            {
                string json = _context.HttpContext?.Session.GetString("PerfilIDSeleccionado");
                if (string.IsNullOrEmpty(json))
                {
                    return string.Empty;
                }
                return JsonConvert.DeserializeObject<string>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("PerfilIDSeleccionado", json);
            }
        }

        protected PerfilDto? PerfilSeleccionado
        {
            get
            {
                string json = _context.HttpContext?.Session.GetString("PerfilSeleccionado");
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<PerfilDto>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("PerfilSeleccionado", json);
            }
        }

        protected List<PerfilUserDto> UsuariosXPerfil
        {
            get
            {
                var json = _context.HttpContext?.Session.GetString("UsuariosXPerfil");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<PerfilUserDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("UsuariosXPerfil", json);
            }
        }

        #region Variables para CGUsuarios

        protected List<PerfilUserDto>? PerfilesDelUsuario
        {
            get
            {
                var json = _context.HttpContext?.Session.GetString("PerfilesDelUsuario");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<PerfilUserDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("PerfilesDelUsuario", json);
            }
        }

        protected List<AdmUserDto>? AdministracionesDelUsuario
        {
            get
            {
                var json = _context.HttpContext?.Session.GetString("AdministracionesDelUsuario");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<AdmUserDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("AdministracionesDelUsuario", json);
            }
        }

        protected List<DerUserDto>? DerechosDelUsuario
        {
            get
            {
                var json = _context.HttpContext?.Session.GetString("DerechosDelUsuario");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<DerUserDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("DerechosDelUsuario", json);
            }
        }

        protected List<UserDto> ListaDeUsuarios
        {
            get
            {
                var json = _context.HttpContext?.Session.GetString("ListaDeUsuarios");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<UserDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("ListaDeUsuarios", json);
            }
        }

        protected UserDto UsuarioSeleccionado
        {
            get
            {
                var json = _context.HttpContext?.Session.GetString("UsuarioSeleccionado");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return new UserDto();
                }
                return JsonConvert.DeserializeObject<UserDto>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("UsuarioSeleccionado", json);
            }
        }


        #endregion
    }
}
