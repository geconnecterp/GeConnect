namespace gc.infraestructura.Dtos.Cajas.Response
{
    public class CotizacionResDto
    {
        public string pre_id { get; set; }

        public string? pre_descripcion { get; set; }

        public DateTime? pre_fecha { get; set; }

        public string? pre_nombre { get; set; }

        public string? pre_domicilio { get; set; }

        public DateTime? pre_vigencia_desde { get; set; }

        public DateTime? pre_vigencia_hasta { get; set; }

        public string? pre_obs_pago { get; set; }

        public string? pre_obs_entrega { get; set; }

        public string? pree_id { get; set; }

        public string? pree_desc { get; set; }

        public string? pret_id { get; set; }

        public string? pret_desc { get; set; }

        public string? cta_id { get; set; }

        public string? usu_id { get; set; }

        public string? adm_id { get; set; }

        public decimal? importe { get; set; }
    }
}
