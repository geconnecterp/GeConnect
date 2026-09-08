
namespace gc.infraestructura.Dtos.Almacen.Tr
{
    public class TiProductoCarritoDto
    {
        public short Item { get; set; }
        public string Pid { get; set; }=string.Empty;
        public int Unidad_pres { get; set; }
        public int Bulto { get; set; }
        public decimal Us { get; set; }
        public decimal Cantidad { get; set; }
        public string? Fvto { get; set; }
        public string Ti { get; set; } = string.Empty;
        public string AdmId { get; set; } = string.Empty;
        public string UsuId { get; set; } = string.Empty;   
        public string BoxId { get; set; } = string.Empty;
        public bool Desarma { get; set; }
        public bool Remplazar { get; set; }
        public string? RemplazarBoxId { get; set; }
        public string? RemplazarPId { get; set; }
    }
}
