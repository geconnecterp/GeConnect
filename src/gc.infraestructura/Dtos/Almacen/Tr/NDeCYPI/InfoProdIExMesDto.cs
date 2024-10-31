namespace gc.infraestructura.Dtos.Almacen.Tr.NDeCYPI
{
    public class InfoProdIExMesDto : Dto
    {
        public string periodo { get; set; } = string.Empty;
        public string mes { get; set; } = string.Empty;
        public int e_compra { get; set; }
        public int e_ri { get; set; }
        public int e_otros { get; set; }
        public int s_ventas { get; set; }
        public int s_ri { get; set; }
        public int s_otros { get; set; }
    }
}
