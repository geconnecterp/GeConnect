
namespace gc.infraestructura.Dtos
{
    public class FinancieroDto : Dto
    {
        public string ctaf_id { get; set; } = string.Empty;
        public string ctaf_denominacion { get; set; } = string.Empty;
        public string ctaf_lista { get; set; } = string.Empty;
        public char ctaf_activo { get; set; }
    }
}
