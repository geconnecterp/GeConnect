using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Users;
using log4net.Filter;
using Microsoft.Data.SqlClient;
using System.Runtime.Intrinsics.Arm;

namespace gc.api.core.Servicios
{
    public class ApiUsuarioServicio : Servicio<Usuario>, IApiUsuarioServicio
    {
        public ApiUsuarioServicio(IUnitOfWork uow) : base(uow)
        {

        }

        public RespuestaDto DefinePerfilDefault(PerfilUserDto perfil)
        {
            var sp = ConstantesGC.StoredProcedures.SP_USU_PERFIL_DEFAULT;
            var ps = new List<SqlParameter>() { 
                    new SqlParameter("@usu_id",perfil.usu_id),
                    new SqlParameter("@perfil_id",perfil.perfil_id),
            };

            List<RespuestaDto> menu = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            return menu.First();
        }

        public List<MenuDto> GetMenu()
        {
            var sp = ConstantesGC.StoredProcedures.SP_MENU_ID;
            var ps = new List<SqlParameter>(); 

            List<MenuDto> menu = _repository.EjecutarLstSpExt<MenuDto>(sp, ps,true);
            return menu;
        }

        public List<MenuItemsDto> GetMenuItems(string menuId,string perfil)
        {
            var sp = ConstantesGC.StoredProcedures.SP_MENU_ITEMS;
            var ps = new List<SqlParameter>() { 
                new SqlParameter("@mnu_id",menuId) ,
                new SqlParameter("@perfil_id",perfil)
            };

            List<MenuItemsDto> menu = _repository.EjecutarLstSpExt<MenuItemsDto>(sp, ps, true);
            return menu;
        }

        public PerfilDto GetPerfil(string id)
        {
            var sp = ConstantesGC.StoredProcedures.SP_USU_PERFIL_DATOS;
            var ps = new List<SqlParameter>() { new SqlParameter("@id", id) };

            List<PerfilDto> resp = _repository.EjecutarLstSpExt<PerfilDto>(sp, ps, true);
            if (resp.Count == 0)
            {
                throw new NegocioException("No se encontro el Perfíl solicitado.");
            }
            return resp.First();
        }

        public List<PerfilDto> GetPerfiles(QueryFilters filters)
        {
            var sp = ConstantesGC.StoredProcedures.SP_USU_PERFILES_LISTA;
            var ps = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(filters.Id))
            {
                ps.Add(new SqlParameter("@id", true));
                ps.Add(new SqlParameter("@id_d", filters.Id));
                if (!string.IsNullOrEmpty(filters.Id2))
                {
                    ps.Add(new SqlParameter("@id_h", filters.Id2));
                }
                else
                {
                    ps.Add(new SqlParameter("@id_h", filters.Id));
                }
            }
            else
            {
                ps.Add(new SqlParameter("@id", false));
                ps.Add(new SqlParameter("@id_d", "000"));
                ps.Add(new SqlParameter("@id_h", "000"));
            }

            ps.Add(new SqlParameter("@registros", filters.Registros));
            ps.Add(new SqlParameter("@pagina", filters.Pagina));
            ps.Add(new SqlParameter("@ordenar", "perfil_descripcion"));

            List<PerfilDto> resp = _repository.EjecutarLstSpExt<PerfilDto>(sp, ps, true);

            return resp;
        }

        public List<PerfilUserDto> GetPerfilUsers(string perfilId)
        {
            var sp = ConstantesGC.StoredProcedures.SP_USU_PERFxUSU_LISTA;
            var ps = new List<SqlParameter>() { new SqlParameter("@id", perfilId) };

            List<PerfilUserDto> resp = _repository.EjecutarLstSpExt<PerfilUserDto>(sp, ps, true);
            
            return resp;
        }

        public List<PerfilUserDto> GetUserPerfiles(string? userName)
        {
            var sp = ConstantesGC.StoredProcedures.SP_USU_PERFILES;
            var ps = new List<SqlParameter>() { new SqlParameter("@usu_id", userName) };

            List<PerfilUserDto> resp = _repository.EjecutarLstSpExt<PerfilUserDto>(sp, ps, true);

            return resp;
        }

        public Usuario GetUsuarioByUserName(string userName)
        {
            //#pragma warning disable CS1061 // "Usuario" no contiene una definición para "User" ni un método de extensión accesible "User" que acepte un primer argumento del tipo "Usuario" (¿falta alguna directiva using o una referencia de ensamblado?)
            //var usuario = GetAllIq().Where(u => u.UserName.Equals(userName)).FirstOrDefault();
            ////#pragma warning restore CS1061 // "Usuario" no contiene una definición para "User" ni un método de extensión accesible "User" que acepte un primer argumento del tipo "Usuario" (¿falta alguna directiva using o una referencia de ensamblado?)
            //if (usuario != null)
            //{
            //    return usuario;
            //}
            throw new NotImplementedException("El metodo aun está TODO."); //new SecurityException("Usuario no encontrado");
        }

        public List<MenuPpalDto> ObtenerMenu(string perfilId,string user,string menuId, string adm)
        {
            var sp = ConstantesGC.StoredProcedures.SP_USU_MENU_PERSONAL;
            var ps = new List<SqlParameter>() { 
                new SqlParameter("@usu_id", user),
                new SqlParameter("@perfil_id",perfilId),
                new SqlParameter("@mnu_id",menuId),
                new SqlParameter("@adm_id",adm)
            };

            List<MenuPpalDto> resp = _repository.EjecutarLstSpExt<MenuPpalDto>(sp, ps, true);

            return resp;
            
        }

        public List<UsuarioDto> BuscarUsuarios(QueryFilters filtro)
        {
            var sp = ConstantesGC.StoredProcedures.SP_USU_FILTRO;
            var ps = new List<SqlParameter>();

            if (string.IsNullOrEmpty(filtro.Buscar))
            {
                ps.Add(new SqlParameter("@deno", false));
                ps.Add(new SqlParameter("@deno_like", ""));
            }
            else
            {
                ps.Add(new SqlParameter("@deno", true));
                ps.Add(new SqlParameter("@deno_like", filtro.Buscar.Trim()));
            }

            if (!string.IsNullOrEmpty(filtro.Id))
            {
                ps.Add(new SqlParameter("@id", true));
                ps.Add(new SqlParameter("@id_d", filtro.Id));
                if (!string.IsNullOrEmpty(filtro.Id2))
                {
                    ps.Add(new SqlParameter("@id_h", filtro.Id2));
                }
                else
                {
                    ps.Add(new SqlParameter("@id_h", filtro.Id));
                }
            }
            else
            {
                ps.Add(new SqlParameter("@id", false));
                ps.Add(new SqlParameter("@id_d", ""));
                ps.Add(new SqlParameter("@id_h", ""));
            }

            ps.Add(new SqlParameter("@registros", filtro.Registros));
            ps.Add(new SqlParameter("@pagina", filtro.Pagina));
            ps.Add(new SqlParameter("@ordenar",string.IsNullOrEmpty(filtro.Sort)? "usu_apellidoynombre":filtro.Sort));

            List<UsuarioDto> resp = _repository.EjecutarLstSpExt<UsuarioDto>(sp, ps, true);

            return resp;
        }

        public List<PerfilUserDto> ObtenerPerfilesDelUsuario(string usuId)
        {
            var sp = ConstantesGC.StoredProcedures.SP_USU_PERFIL;
            var ps = new List<SqlParameter>() { new SqlParameter("@usu_id", usuId) };
            List<PerfilUserDto> resp = _repository.EjecutarLstSpExt<PerfilUserDto>(sp, ps, true);

            return resp;
        }

        public List<AdmUserDto> ObtenerAdministracionesDelUsuario(string usuId)
        {
            var sp = ConstantesGC.StoredProcedures.SP_USU_ADM;
            var ps = new List<SqlParameter>() { new SqlParameter("@usu_id", usuId) };
            List<AdmUserDto> resp = _repository.EjecutarLstSpExt<AdmUserDto>(sp, ps, true);

            return resp;
        }

        public List<DerUserDto> ObtenerDerechosDelUsuario(string usuId)
        {
            var sp = ConstantesGC.StoredProcedures.SP_USU_DER;
            var ps = new List<SqlParameter>() { new SqlParameter("@usu_id", usuId) };
            List<DerUserDto> resp = _repository.EjecutarLstSpExt<DerUserDto>(sp, ps, true);

            return resp;
        }
    }
}
