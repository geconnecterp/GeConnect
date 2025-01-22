namespace gc.infraestructura.Dtos.Users
{
    public class PerfilDto : Dto
    {
        public int Total_Registros { get; set; }
        public int Total_Paginas { get; set; }
        public string perfil_id { get; set; } = string.Empty;//+
        public string perfil_descripcion { get; set; } = string.Empty;//+
        public char? perfil_activo { get; set; }

        private bool perfilactivo { get; set; }//
        public bool Perfilactivo
        {
            get
            {
                if (!perfil_activo.HasValue ||
                    string.IsNullOrWhiteSpace(char.ToString(perfil_activo.Value)))
                    return false;
                return perfil_activo.Equals('S');
            }
            set
            {
                perfilactivo = value;
            }
        }
        public string perfil_activo_desc { get; set; } = string.Empty;//+

    }
}
