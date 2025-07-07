namespace gc.infraestructura.Dtos.Asientos
{
    public class AsientoResultadoDto
    {
        public int Eje_nro { get; set; }
        public string Ccb_id { get; set; } = string.Empty;
        public string Ccb_desc { get; set; } = string.Empty;
        public decimal Saldo { get; set; }
    }

    public class AsientoResultadoDatoDto
    {
        public int eje_nro { get; set; }
        public string ccb_id { get; set; } = string.Empty;
        public decimal saldo { get; set; }
    }
}
