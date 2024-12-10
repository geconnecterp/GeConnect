
namespace gc.api.core.Entidades
{
    public class Departamento : EntidadBase
    {
        public string Dep_id { get; set; } = string.Empty;
        public string Dep_nombre { get; set; } = string.Empty;
        public string Prov_id { get; set; } = string.Empty;
        public string Dep_lista { get; set; } = string.Empty;
    }
}
