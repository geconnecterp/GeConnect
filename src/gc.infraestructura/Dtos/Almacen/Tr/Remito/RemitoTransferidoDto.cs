
namespace gc.infraestructura.Dtos.Almacen.Tr.Remito
{
    /// <summary>
    /// Se utiliza en el sitio y tambien en el POCKET en RTI
    /// </summary>
    public class RemitoGenDto : Dto
    {
        public string re_compte { get; set; } = string.Empty;
        public DateTime re_fecha { get; set; }
        public string adm_id { get; set; } = string.Empty;
        public string adm_nombre { get; set; } = string.Empty;
        public string ree_id { get; set; } = string.Empty;
        public string ree_desc { get; set; } = string.Empty;
        public string pv_compte { get; set; } = string.Empty;
        public string ti { get; set; }= string.Empty;
        public bool EsModificacion { get; set; } = true;
        public string Ul { get; set; } = string.Empty;
    }
}
