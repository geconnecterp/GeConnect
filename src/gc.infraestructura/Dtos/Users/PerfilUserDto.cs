namespace gc.infraestructura.Dtos.Users
{
    public class PerfilUserDto
    {
        public string usu_id { get; set; } = string.Empty;
        public string usu_apellidoynombre { get; set; } = string.Empty;
        public string perfil_id { get; set; } = string.Empty;
        public string perfil_descripcion { get; set; } = string.Empty;
        public string perfil_default { get; set; }=string.Empty;
    }
}
