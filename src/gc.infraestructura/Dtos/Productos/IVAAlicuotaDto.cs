namespace gc.infraestructura.Dtos.Productos
{
    public class IVAAlicuotaDto
    {
        public decimal IVA_Alicuota { get; set; }
        public string IVA_Grl { get; set; }=string.Empty ;
        public string IVA_Extra { get; set; } = string.Empty;
        public string IVA_Afip { get; set; } = string.Empty;
    }
}
