
namespace gc.infraestructura.Dtos.Almacen.Tr.Remito
{
    public class RemitoGenDto : Dto
    {
        public string re_compte { get; set; } = string.Empty;
        public DateTime re_fecha { get; set; }
        public string adm_id { get; set; } = string.Empty;
        public string adm_nombre { get; set; } = string.Empty;
        public string ree_id { get; set; } = string.Empty;
        public string ree_desc { get; set; } = string.Empty;
        public string pv_compte { get; set; } = string.Empty;
    }
}
