namespace gc.infraestructura.Dtos.Libros
{
    public class LMayorAcumuladoDiaDto
    {
        public DateTime fecha { get; set; }
        public string fecha_str { get; set; }
        public decimal total_debe { get; set; }
        public decimal total_haber { get; set; }
        public decimal saldo_final { get; set; }
        public decimal saldo_anterior { get; set; }
    }
}
