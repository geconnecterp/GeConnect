using System;
using System.Collections.Generic;
using System.Text;

namespace gc.api.core.Entidades
{
    public class Usuario : EntidadBase
    {
        public Usuario()
        {
            Autorizados = new HashSet<Autorizado>();
            Accesoss = new HashSet<Accesos>();            
            AuditoriaUsuarios = new HashSet<AuditoriaUsuario>();
            Autorizados = new HashSet<Autorizado>();
        }

        public Guid Id { get; set; }
        public string?Contrasena { get; set; }
        public string?Correo { get; set; }
        public bool Bloqueado { get; set; }
        public int Intentos { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime? FechaBloqueo { get; set; }
        public string?UserName { get; set; }
        public bool EstaLogueado { get; set; }


        public virtual ICollection<Autorizado> Autorizados { get; set; }
        public virtual ICollection<Accesos> Accesoss { get; set; }     
        public virtual ICollection<AuditoriaUsuario> AuditoriaUsuarios { get; set; }


    }
}
