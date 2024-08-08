namespace gc.api.core.Entidades
{
    public partial class UsuarioAdministracion : EntidadBase
    {
        public UsuarioAdministracion()
        {
            Usu_Id = string.Empty;
            Adm_Id = string.Empty;
        }

        public string Usu_Id { get; set; }
        public string Adm_Id { get; set; }

        public virtual Usuario Usuario { get; set; }

    }
}
