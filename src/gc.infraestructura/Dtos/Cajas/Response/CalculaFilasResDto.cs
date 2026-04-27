namespace gc.infraestructura.Dtos.Cajas.Response
{
    public class CalculaFilasResDto
    {
        public string json_subtotal { get; set; } = string.Empty;   // varchar(max)
        public string json_sorteo { get; set; } = string.Empty;      // varchar(max)
        public string json_p { get; set; } = string.Empty;        // varchar(max)
    }
}
