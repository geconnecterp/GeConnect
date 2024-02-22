using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.api.core.Servicios;
using gc.api.Core.Interfaces.Servicios;
using System.Linq.Dynamic.Core;
using System.Security;

namespace gc.api.Core.Servicios
{
    public class UsuarioServicio : Servicio<Usuario>, IUsuarioServicio
   {
        public UsuarioServicio(IUnitOfWork uow) : base(uow)
        {

        }

        public Usuario GetUsuarioByUserName(string userName)
        {
            //#pragma warning disable CS1061 // "Usuario" no contiene una definición para "User" ni un método de extensión accesible "User" que acepte un primer argumento del tipo "Usuario" (¿falta alguna directiva using o una referencia de ensamblado?)
            var usuario = GetAllIq().Where(u => u.UserName.Equals(userName)).FirstOrDefault();
            //#pragma warning restore CS1061 // "Usuario" no contiene una definición para "User" ni un método de extensión accesible "User" que acepte un primer argumento del tipo "Usuario" (¿falta alguna directiva using o una referencia de ensamblado?)
            if (usuario != null)
            {
                return usuario;
            }
            throw new SecurityException("Usuario no encontrado");
        }
    }
}
