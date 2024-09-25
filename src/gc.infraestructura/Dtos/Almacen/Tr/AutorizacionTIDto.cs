namespace gc.infraestructura.Dtos.Almacen.Tr
{
    public class AutorizacionTIDto
    {
        public string Ti { get; set; } = string.Empty;
        public string Adm_id_des { get; set; } = string.Empty;
        public string Adm_nombre { get; set; } = string.Empty;
        public string Usu_id { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string? Nota { get; set; }
        public string? Tie_id { get; set; }
        public string? Pi_compte { get; set; }
        public bool EsBox { get; set; }
        public string? BoxId { get; set; }
        public bool EsRubro { get; set; }
        public string? RubroId { get; set; }
        public string? RubroGId { get; set; }
        public string TipoTI { get; set; }=string.Empty;
        public string PId { get; set; } = string.Empty;
        public string PBoxId { get; set; } = string.Empty;
        public DateTime Pfvto { get; set; }
        public int PUnidPres { get; set; }
        public decimal PPedido { get; set; }
        public bool SinAU { get; set; }

    }
}
