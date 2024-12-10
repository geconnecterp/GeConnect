
namespace gc.api.core.Entidades
{
    public partial class TipoNegocio : EntidadBase
    {
        public string ctn_id { get; set; }
        public string ctn_desc { get; set; }
        public string ctn_lista { get; set; }
        public TipoNegocio()
        {
            ctn_id = string.Empty;
            ctn_desc = string.Empty;
            ctn_lista = string.Empty;
        }
    }
}
