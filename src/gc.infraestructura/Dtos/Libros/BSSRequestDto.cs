namespace gc.infraestructura.Dtos.Libros
{
    public class BSSRequestDto
    {
        public int Eje_nro { get; set; } = 0;
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public bool ConTemporal { get; set; }
    }
}
