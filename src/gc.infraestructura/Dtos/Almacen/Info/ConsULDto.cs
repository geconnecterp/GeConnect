namespace gc.infraestructura.Dtos.Almacen.Info
{
    public class ConsULDto : Dto
    {
        public string UL_id { get; set; } = string.Empty;
        public string ULe_desc { get; set; } = string.Empty;
        public DateTime UL_fecha { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Id_compte { get; set; } = string.Empty;
        public string Obs { get; set; } = string.Empty;
        public string Box_id { get; set; } = string.Empty;
        public string ImgB64 { get; set; } = string.Empty;
    }
}
