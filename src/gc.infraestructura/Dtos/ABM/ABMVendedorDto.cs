namespace gc.infraestructura.Dtos.ABM
{
    public class ABMVendedorDto : ABMVendedorDatoDto
    {
        public int Total_Registros { get; set; }
        public int Total_Paginas { get; set; }

    }

    public class ABMVendedorDatoDto:Dto
    {
        public string ve_id { get; set; } = string.Empty;
        public string ve_nombre { get; set; } = string.Empty;
        public string ve_lista { get; set; } = string.Empty;
        public decimal ve_comision { get; set; }
        public string ve_celu { get; set; } = string.Empty;
        public string ve_mail { get; set; } = string.Empty;
        public string ve_te { get; set; } = string.Empty;
        public char? ve_activo { get; set; }
        private bool veActivo;
        public bool VeActivo
        {
            get
            {
                if (!ve_activo.HasValue ||
                    string.IsNullOrWhiteSpace(char.ToString(ve_activo.Value)))
                    return false;
                return ve_activo.Equals('S');
            }
            set
            {
                veActivo = value;
            }
        }
    }
}