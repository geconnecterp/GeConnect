using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Users;
using Microsoft.Data.SqlClient;

namespace gc.api.core.Servicios
{
    public class ApiUsuarioServicio : Servicio<Usuario>, IApiUsuarioServicio
    {
        public ApiUsuarioServicio(IUnitOfWork uow) : base(uow)
        {

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
            if (resp.Count == 0)
            {
                throw new NegocioException("No se encontro el Perfíl solicitado.");
            }
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
    }
}
