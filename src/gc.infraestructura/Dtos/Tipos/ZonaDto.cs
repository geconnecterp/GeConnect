
namespace gc.infraestructura.Dtos
{
    public class ZonaDto : Dto
    {
        public string zn_id { get; set; }
        public string zn_desc { get; set; }
        public string zn_lista { get; set; }

        public ZonaDto()
        {
            zn_id = string.Empty;
            zn_desc = string.Empty;
            zn_lista = string.Empty;
        }
    }
}
