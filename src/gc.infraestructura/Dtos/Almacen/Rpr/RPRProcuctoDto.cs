namespace gc.infraestructura.Dtos.Almacen.Rpr
{
    public class RPRProcuctoDto
    {
        public int item { get; set; }//0
        //public string Ope { get; set; }
        public string rp { get; set; }//0
        public string ul_id { get; set; }//0
        //public string Nro_auto { get; set; } = string.Empty;
        public string p_id { get; set; } = string.Empty;//0
        public string p_id_prov { get; set; }//0
        public string p_id_barrado { get; set; }//0
        public string p_desc { get; set; } = string.Empty;//0
        public string up_id { get; set; } = string.Empty; //= 00 => decimal -> acepto lo que traiga (3 decimales)
        //public string Cta_id { get; set; } = string.Empty;
        public string usu_id { get; set; } = string.Empty;//0
        public int unidad_pres { get; set; } //0
        public int bulto { get; set; } //0
        public decimal us { get; set; }//0
        public DateTime? vto { get; set; }//0
        public decimal cantidad { get; set; }//0
        
    }
}
