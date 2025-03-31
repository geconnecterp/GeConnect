namespace gc.infraestructura.Dtos.ABM
{
    public class ABMVendedorDto : ABMVendedorDatoDto
    {
        public int Total_Registros { get; set; }
        public int Total_Paginas { get; set; }

    }

    public class ABMVendedorDatoDto
    {
        public string Ve_Id { get; set; } = string.Empty;
        public string Ve_Nombre { get; set; } = string.Empty;
        public string Ve_LIsta { get; set; } = string.Empty;
        public string Ve_Comision { get; set; } = string.Empty;
        public string Ve_Celu { get; set; } = string.Empty;
        public string Ve_Mail { get; set; } = string.Empty;
        public string Ve_Te { get; set; } = string.Empty;
        public string Ve_Activo { get; set; } = string.Empty;
    }
}