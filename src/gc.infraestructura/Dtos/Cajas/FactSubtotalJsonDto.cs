namespace gc.infraestructura.Dtos.Cajas
{
    public class FactSubtotalJsonDto
    {
        public int orden { get; set; }
        public string tipo { get; set; }= string.Empty;
        public string concepto { get; set; }= string.Empty;
        public decimal @base { get; set; }
        public decimal alicuota { get; set; }
        public decimal importe { get; set; }
        public string id_aux { get; set; }= string.Empty;   
    }
}
