
namespace gc.api.core.Entidades
{
    public class Zona : EntidadBase
    {
        public string zn_id { get; set; }
        public string zn_desc { get; set; }
        public string zn_lista { get; set; }

        public Zona()
        {
            zn_id = string.Empty;
            zn_desc = string.Empty;
            zn_lista = string.Empty;
        }
    }
}
