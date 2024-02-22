namespace gc.api.core.Entidades
{
    public class Autorizado : EntidadBase
    {
        public Autorizado()
        {
            Usuario = new Usuario();
            Role = new Role();
        }

        public Guid UsuarioId { get; set; }
        public Guid RoleId { get; set; }
        public Usuario Usuario { get; set; }
        public Role Role { get; set; }
    }
}
