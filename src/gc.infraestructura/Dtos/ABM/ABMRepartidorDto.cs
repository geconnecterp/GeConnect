namespace gc.infraestructura.Dtos.ABM
{
    public class ABMRepartidorDto : ABMRepartidorDatoDto
    {
        public int Total_Registros { get; set; }
        public int Total_Paginas { get; set; }

    }

    public class ABMRepartidorDatoDto : Dto
    {
        public string rp_id { get; set; } = string.Empty;
        public string rp_nombre { get; set; } = string.Empty;
        public string rp_lista { get; set; } = string.Empty;
        public decimal rp_comision { get; set; }
        public string rp_celu { get; set; } = string.Empty;
        public string rp_mail { get; set; } = string.Empty;
        public string rp_te { get; set; } = string.Empty;
        public char? rp_activo { get; set; }
        private bool rpActivo;
        public bool RpActivo
        {
            get
            {
                if (!rp_activo.HasValue ||
                    string.IsNullOrWhiteSpace(char.ToString(rp_activo.Value)))
                    return false;
                return rp_activo.Equals('S');
            }
            set
            {
                rpActivo = value;
            }
        }
    }
}