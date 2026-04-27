namespace gc.infraestructura.Dtos.Cajas.Response
{
    public class PrefacturaResDto
    {
        public int cpf_nro { get; set; }

        public DateTime cpf_fecha { get; set; }

        public string sec_id { get; set; } = string.Empty;

        public string cta_id { get; set; } = string.Empty;

        public string cpf_nombre { get; set; } = string.Empty;

        public string sec_desc { get; set; } = string.Empty;        

        public string usada { get; set; } = string.Empty;

        public string cpf_documento { get; set; } = string.Empty;
    }
}
