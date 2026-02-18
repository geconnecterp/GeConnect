namespace gc.infraestructura.Dtos.Productos.PromoCombo
{
    public class ComboPresetDto
    {
        public char cmb_tipo { get; set; }
        public string cmb_tipo_desc { get; set; } = string.Empty;
        public int cantidad { get; set; }
        public decimal dto_porc { get; set; }
    }
}
