namespace gc.infraestructura.Dtos.Almacen
{
    public class RPRProcuctoDto
    {
        public int Item { get; set; }
        public string P_id { get; set; } = string.Empty;
        public string P_desc { get; set; } = string.Empty;
        public string Up_id { get; set; } = string.Empty; //= 00 => decimal -> acepto lo que traiga (3 decimales)
        public string Cta_id { get; set; } = string.Empty;
        public string Usu_id { get; set; } = string.Empty;
        public int Bulto_up { get; set; } 
        public int Bulto { get; set; } 
        public decimal Uni_suelta { get; set; }
        public DateTime? Vto { get; set; }
        public decimal Cantidad { get; set; }
        public string Nro_tra { get; set; } = string.Empty;
    }
}
