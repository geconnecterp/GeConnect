using gc.infraestructura.Dtos.Productos.Ofertas;

namespace gc.infraestructura.Dtos.Productos.PromoCombo
{
    public class ComboListaDto : ComboDatosDto
    {
        public int Total_Registros { get; set; }
        public int Total_Paginas { get; set; }
    }

    public class ComboDatosDto
    {
        public string cmb_id { get; set; } = string.Empty;
        public string cmb_desc { get; set; } = string.Empty;
        public char cmb_tipo { get; set; }
        public string cmb_tipo_desc { get; set; } = string.Empty;
        public char cmb_estado { get; set; }
        public string cmb_estados_desc { get; set; } = string.Empty;
        public DateTime cmb_desde { get; set; }
        public DateTime cmb_hasta { get; set; }
        public bool pasa_activar { get; set; }
        public bool pasa_historico { get; set; }
    }

    public class ConfirmacionRequestDto
    {
        public ComboDatosDto Datos { get; set; } = new();
        public List<ComboCanalDto> Canales { get; set; } = [];
        public List<ComboProductoDto> Productos { get; set; } = [];
    }
}
