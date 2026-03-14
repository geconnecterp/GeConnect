namespace gc.infraestructura.Dtos.OrdenReparto
{
    public class ORCargaCarritoRequest
    {
        public string or_compte { get; set; } = string.Empty;
        public string adm_id { get; set; } = string.Empty;
        public string usu_id { get; set; } = string.Empty;
        public string box_id { get; set; } = string.Empty;
        public bool desarma_box { get; set; }
        public string p_id { get; set; } = string.Empty;
        public int unidad_pres { get; set; }
        public decimal bulto { get; set; }
        public decimal us { get; set; }
        public decimal cantidad { get; set; }
        public  string fv { get; set; }= string.Empty;
    }
}
