
namespace gc.infraestructura.Dtos
{
    public class VendedorDto : Dto
    {
        public string ve_id { get; set; } = string.Empty;
        public string ve_nombre { get; set; } = string.Empty;
        public decimal ve_comision { get; set; } = 0.000M;
        public string ve_te { get; set; } = string.Empty;
        public string ve_celu { get; set; } = string.Empty;
        public string ve_mail { get; set; } = string.Empty;
        public string ve_activo { get; set; } = string.Empty;
        public string ve_actu { get; set; } = string.Empty;
        public string ve_lista { get; set; } = string.Empty;
    }
}
