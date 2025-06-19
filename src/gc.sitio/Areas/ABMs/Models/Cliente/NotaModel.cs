namespace gc.sitio.Areas.ABMs.Models
{
    public class NotaModel
    {
        public string Cta_Id { get; set; } = string.Empty;
        public string Usu_Id { get; set; } = string.Empty;
        public string Usu_Apellidoynombre { get; set; } = string.Empty;
        public string Usu_Lista { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Nota { get; set; } = string.Empty;
        public bool Puedo_Editar { get; set; } = false;
	}
}
