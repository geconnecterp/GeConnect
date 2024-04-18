namespace gc.api.core.Entidades
{
    public partial class Usuario : EntidadBase
    {
        public Usuario()
        {
            Accesos = new HashSet<Acceso>();
            AuditoriaUsuarios = new HashSet<AuditoriaUsuario>();
            Autorizados = new HashSet<Autorizado>();
            Contrasena = string.Empty;
            Correo = string.Empty;
            UserName = string.Empty;
        }

        public Guid Id { get; set; }
        public string Contrasena { get; set; }
        public string Correo { get; set; }
        public bool Bloqueado { get; set; }
        public int Intentos { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime? FechaBloqueo { get; set; }
        public string UserName { get; set; }
        public bool EstaLogueado { get; set; }


        public virtual ICollection<Acceso> Accesos { get; set; }
        public virtual ICollection<AuditoriaUsuario> AuditoriaUsuarios { get; set; }
        public virtual ICollection<Autorizado> Autorizados { get; set; }

    }
}
