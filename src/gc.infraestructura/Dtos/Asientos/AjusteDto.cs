namespace gc.infraestructura.Dtos.Asientos
{
    public class AjusteDto
    {
        public int eje_nro { get; set; }
        public string ccb_id { get; set; } = string.Empty;
        public decimal ajuste { get; set; }
        public bool ajusta { get; set; }

    }
}
