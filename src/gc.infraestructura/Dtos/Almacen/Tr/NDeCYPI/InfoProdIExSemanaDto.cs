namespace gc.infraestructura.Dtos.Almacen.Tr.NDeCYPI
{
    public class InfoProdIExSemanaDto : Dto
    {
        public DateTime desde { get; set; }
        public DateTime hasta { get; set; }
        public int e_compra { get; set; }
        public int e_ri { get; set; }
        public int e_otros { get; set; }
        public int s_ventas { get; set; }
        public int s_ri { get; set; }
        public int s_otros { get; set; }
    }
}
