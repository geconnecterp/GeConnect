namespace gc.infraestructura.Dtos.Cajas.Response
{
    public class CalculaFilasResDto
    {
        // En rechazo, Calcula_Filas devuelve una fila de #sub en lugar de los JSON.
        public string tipo { get; set; } = string.Empty;
        public string concepto { get; set; } = string.Empty;
        public string json_subtotal { get; set; } = string.Empty;   // varchar(max)
        public string json_sorteo { get; set; } = string.Empty;      // varchar(max)
        public string json_p { get; set; } = string.Empty;        // varchar(max)
    }
}
