namespace gc.infraestructura.Dtos.Users
{
    public class PerfilDto : Dto
    {
        public int Total_Registros { get; set; }
        public int Total_Paginas { get; set; }
        public string Perfil_id { get; set; } = string.Empty;//+
        public string Perfil_descripcion { get; set; } = string.Empty;//+
        public char Perfil_activo { get; set; }
        public string Perfil_activo_descripcion { get; set; } = string.Empty;//+

    }
}
