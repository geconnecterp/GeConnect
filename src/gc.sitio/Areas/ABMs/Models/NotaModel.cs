namespace gc.sitio.Areas.ABMs.Models
{
    public class NotaModel
    {
        public string cta_id { get; set; } = string.Empty;
        public string usu_id { get; set; } = string.Empty;
        public string usu_apellidoynombre { get; set; } = string.Empty;
        public string usu_lista { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public string nota { get; set; } = string.Empty;
    }
}
