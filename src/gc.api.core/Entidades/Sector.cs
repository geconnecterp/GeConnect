
namespace gc.api.core.Entidades
{
    public class Sector : EntidadBase
    {
        public string sec_id { get; set; } = string.Empty;
        public string sec_desc { get; set; } = string.Empty;
        public char sec_prefactura { get; set; }
        public char sec_actu { get; set; }
        public string sec_lista { get; set; } = string.Empty;
    }
}
