
namespace gc.infraestructura.Dtos
{
    public class DepartamentoDto : Dto
    {
        public string dep_id { get; set; } = string.Empty;
        public string dep_nombre { get; set; } = string.Empty;
        public string prov_id { get; set; } = string.Empty;
        public string dep_lista { get; set; } = string.Empty;
    }
}
