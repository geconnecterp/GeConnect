namespace gc.infraestructura.Dtos.Libros
{
    public class BSumaSaldoRegDto
    {
        public string Ccb_id { get; set; } = string.Empty;
        public string Ccb_desc { get; set; } = string.Empty;
        public decimal Debe { get; set; }
        public decimal Haber { get; set; }
        public decimal Saldo { get; set; }
        public decimal Saldo_anterior { get; set; }
        public decimal Saldo_suma { get; set; }
    }
}
