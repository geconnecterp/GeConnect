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
		public string GetIngresoFull() => $"Ingreso: {(e_compra + e_ri + e_otros).ToString()}";
		public string GetEgresoFull() => $"Egreso: {(s_ventas + s_ri + s_otros).ToString()}";
	}
}
