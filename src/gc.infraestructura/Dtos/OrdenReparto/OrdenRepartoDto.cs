namespace gc.infraestructura.Dtos.OrdenReparto
{
    public class OrdenRepartoListDto : OrdenRepartoDto
    {
        public int total_registros { get; set; }
        public int total_paginas { get; set; }
    }
    public class OrdenRepartoDto
    {
        public string or_compte { get; set; } = string.Empty;
        public string or_obs { get; set; } = string.Empty;
        public DateTime or_fecha { get; set; }
        public DateTime or_fecha_fac { get; set; }
        public string ore_id { get; set; } = string.Empty;
        public string ore_desc { get; set; } = string.Empty;
        public string rp_id { get; set; } = string.Empty;
        public string rp_nombre { get; set; } = string.Empty;

    }
}
