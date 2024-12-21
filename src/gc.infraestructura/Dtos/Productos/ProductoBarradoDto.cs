using System.ComponentModel.DataAnnotations;

namespace gc.infraestructura.Dtos.Productos
{
    public class ProductoBarradoDto
    {
        [Display(Name = "ID")]
        public string P_Id { get; set; } = string.Empty;
        [Display(Name = "BARRADO")]
        public string P_Id_barrado { get; set; } = string.Empty;
        public string P_Unidad_Pres { get; set; } = string.Empty;
        public string P_Unidad_X_Bulto { get; set; } = string.Empty;
        public string P_Bulto_X_Piso { get; set; } = string.Empty;
        public string P_Piso_X_Pallet { get; set; } = string.Empty;
        public string Tba_Id { get; set; } = string.Empty;
        public string Tba_Desc { get; set; } = string.Empty;
        public string Tba_Lista { get; set; } = string.Empty;
    }
}
