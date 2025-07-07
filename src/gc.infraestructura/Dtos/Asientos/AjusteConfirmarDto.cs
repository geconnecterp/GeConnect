namespace gc.infraestructura.Dtos.Asientos
{
    public class AjusteConfirmarDto
    {
        public int EjeNro { get; set; }
        public string User { get; set; } = string.Empty;
        public string AdmId { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string CcbId { get; set; } = string.Empty;
        public string Json { get; set; } = string.Empty;
    }
}
