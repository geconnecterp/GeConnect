using System.Diagnostics.SymbolStore;

namespace gc.infraestructura.Dtos.Billeteras
{
    public partial class BilleteraDto
    {
        public BilleteraDto()
        {
            Bill_id = string.Empty;
            Bill_desc = string.Empty;
        }
        public string Bill_id { get; set; }= string.Empty;
        public string Bill_desc { get; set; } = string.Empty;
        public string Bill_Ruta_Base { get; set; } = string.Empty;    
        public string Bill_User_Id { get; set; } = string.Empty;    
        public string Bill_Token { get; set; } = string.Empty;
        public string Bill_Url_Base_Notificacion { get; set; } = string.Empty;
        public string Bill_Ruta_Api_Notificacion { get; set; } = string.Empty;
        public string Bill_Ruta_PublicKey { get; set; } = string.Empty;
    }
}
