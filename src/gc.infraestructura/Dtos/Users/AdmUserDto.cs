namespace gc.infraestructura.Dtos.Users
{
    public class AdmUserDto
    {
        public string usu_id { get; set; } = string.Empty;
        public string adm_id { get; set; } = string.Empty;
        public string adm_nombre { get; set; } = string.Empty;
        public bool asignado { get; set; }
    }
}
