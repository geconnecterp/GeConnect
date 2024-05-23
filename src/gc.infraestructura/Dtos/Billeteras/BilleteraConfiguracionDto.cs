using System;
using System.Collections.Generic;
using System.Text;

namespace geco_0000.Core.DTOs
{
    public partial class BilleteraConfiguracionDto
    {
        public BilleteraConfiguracionDto()
        {
            Bc_Id = string.Empty;
            Bc_Url_Base_Notificacion = string.Empty;
            Bc_Url_Base_Servicio = string.Empty;
            Bc_Ruta_Publickey = string.Empty;
            Bc_Ruta_Privatekey = string.Empty;
        }
        public string Bc_Id { get; set; }
        public string Bc_Url_Base_Notificacion { get; set; }
        public string Bc_Url_Base_Servicio { get; set; }
        public string Bc_Ruta_Publickey { get; set; }
        public string Bc_Ruta_Privatekey { get; set; }

    }
}
