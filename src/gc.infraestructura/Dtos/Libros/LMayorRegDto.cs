namespace gc.infraestructura.Dtos.Libros
{
    public class LMayorRegDto
    {
        public decimal Saldo_inicial { get; set; }
        public decimal Tot_debe { get; set; }
        public decimal Tot_haber { get; set; }
        public string Dia_movi { get; set; } = string.Empty;
        public DateTime Dia_fecha { get; set; }
        public string Dia_tipo { get; set; } = string.Empty;
        public string Dia_lista { get; set; } = string.Empty;
        public string Dia_desc_asiento { get; set; } = string.Empty;
        public short Dia_nro { get; set; }
        public string Ccb_id { get; set; } = string.Empty;
        public string Ccb_desc { get; set; } = string.Empty;
        public string Dia_desc { get; set; } = string.Empty;
        public string Dia_compte { get; set; } = string.Empty;
        public decimal Dia_debe { get; set; }
        public decimal Dia_haber { get; set; }
        public decimal Dia_saldo { get; set; }
        public bool Temporal { get; set; }

    }

    public class LMayorRegListaDto:LMayorRegDto
    {
        public int Total_registros { get; set; }
        public int Total_paginas { get; set; }
    }
}
