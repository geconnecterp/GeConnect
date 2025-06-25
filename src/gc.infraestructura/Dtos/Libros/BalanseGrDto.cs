namespace gc.infraestructura.Dtos.Libros
{
    public class BalanseGrDto
    {
        public string Ccb_id { get; set; }=string.Empty;
        public string Ccb_desc { get; set; } = string.Empty;
        public string Ccb_id_padre { get; set; } = string.Empty;
        public char Ccb_tipo { get; set; }
        public decimal Debe { get; set; }
        public decimal Haber { get; set; }
        public bool Cargado { get; set; }
        public int NivelIndentacion { get; set; } = 0;
    }

    
}
