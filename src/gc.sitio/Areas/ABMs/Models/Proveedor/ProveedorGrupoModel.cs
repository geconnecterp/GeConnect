namespace gc.sitio.Areas.ABMs.Models
{
    public class ProveedorGrupoModel
    {
        public string Cta_Id { get; set; } = string.Empty;
        public string Pg_Id { get; set; } = string.Empty;
        public string Pg_Desc { get; set; } = string.Empty;
        public string Pg_Lista { get; set; } = string.Empty;
        public DateTime? Pg_Fecha_Carga_Precios { get; set; }
        public DateTime? Pg_Fecha_Consulta_Precios { get; set; }
        public DateTime? Pg_Fecha_Cambio_Precios { get; set; }
        public string Pg_Observaciones { get; set; } = string.Empty;
        public DateTime? Pg_Actu_Fecha { get; set; }
        public char? Pg_Actu { get; set; }
    }
}
