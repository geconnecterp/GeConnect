namespace gc.infraestructura.Dtos
{
    public class RegistroUserDto : Dto
    {
        /// <summary>
        /// usu_id
        /// </summary>
        public string?User { get; set; }
        public string?Password { get; set; }
        public string ApellidoYNombre { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = "00";
        public string Documento { get; set; } = string.Empty;
        public string?Correo { get; set; }
        public string?Role { get; set; }
    }
}
