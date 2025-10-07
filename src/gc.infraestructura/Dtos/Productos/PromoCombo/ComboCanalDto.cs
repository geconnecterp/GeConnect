namespace gc.infraestructura.Dtos.Productos.PromoCombo
{
    public class ComboCanalDto
    {
        public string adm_id { get; set; } = string.Empty;
        public string adm_nombre { get; set; } = string.Empty;
        public string lp_id { get; set; } = string.Empty;
        public string lp_desc { get; set; } = string.Empty;
        public string canal { get; set; } = string.Empty;
        public char incluida { get; set; } //= string.Empty;
    }
}
