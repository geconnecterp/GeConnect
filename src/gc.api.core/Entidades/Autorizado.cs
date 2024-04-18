namespace gc.api.core.Entidades
{
    public class Autorizado : EntidadBase
    {
        public Autorizado()
        {
            Usuario = new Usuarios();
            Role = new Role();
        }

        public Guid UsuarioId { get; set; }
        public Guid RoleId { get; set; }
        public Usuarios Usuario { get; set; }
        public Role Role { get; set; }
    }
}
