using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Cajas.Request
{
    public class CajaValidaPVDto
    {
        public string caja_id { get; set; } = string.Empty;
        public string usu_id { get; set; } = string.Empty;
        public string adm_id { get; set; } = string.Empty;
        public string? caja_nro_proceso { get; set; } = string.Empty;
        public string? caja_nro_cierre { get; set; } = string.Empty;
        /// <summary>
        /// Se debe informar 'I', si se lo ejecuta al iniciar la facturación y ‘F’ si se le ejecuta al cargar la factura
        /// </summary>
        public string tipo_llamada { get; set; } = string.Empty;

    }

    public class CargaStkDto
    {
        public string box_id { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        /// <summary>
        /// se le debe pasar el comprobante devuelto en el json de Op_Confirmar de la siguiente manera concatenada tco_id + cm_compte+ cm_repetido
        /// </summary>
        public string id { get; set; } = string.Empty;
    }
}
