namespace gc.infraestructura.Dtos.Productos.PromoCombo
{
    public class ComboReqDto
    {
        public string adm_id { get; set; } = string.Empty;
        public string lp_id { get; set; } = string.Empty;
        public char cmb_estado { get; set; } ='A';
        public string cmb_id { get; set; } = "%";
        public DateTime cmb_carga { get; set; } = new(2020, 1, 1);
    }
}
