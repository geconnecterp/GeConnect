
namespace gc.infraestructura.Dtos.Almacen.Tr
{
    public class TiProductoCarritoDto
    {
        public string Pid { get; set; }=string.Empty;
        public int Unidad_pres { get; set; }
        public int Bulto { get; set; }
        public decimal Us { get; set; }
        public decimal Cantidad { get; set; }
        public string? Fvto { get; set; }
        public string Ti { get; set; } = string.Empty;
        public string AdmId { get; set; } = string.Empty;
        public string UsuId { get; set; }
        public string BoxId { get; set; }
        public bool Desarma { get; set; }
    }
}
