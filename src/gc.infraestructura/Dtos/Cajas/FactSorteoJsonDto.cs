using Newtonsoft.Json;

namespace gc.infraestructura.Dtos.Cajas
{
    public class FactSorteoJsonDto
    {
        [JsonProperty("so_sorteo")]
        public string So_Sorteo { get; set; }= string.Empty;
        [JsonProperty("so_desc")]
        public string So_Desc { get; set; }= string.Empty;
    }
}
