using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Cajas
{
    public class Json_Valor
    {
        public string rb_nro_valor { get; set; } = string.Empty;

        public string ins_id { get; set; } = string.Empty;

        public string rb_dato1_valor { get; set; } = string.Empty;

        public string rb_dato2_valor { get; set; } = string.Empty;          
        public string rb_dato3_valor { get; set; } = string.Empty;

        public string rb_opcion_cuota { get; set; } = string.Empty;

        public string rb_cupon_manual { get; set; } = string.Empty;     
        public string rb_ch_dif { get; set; } = string.Empty;

        public DateTime rb_fecha_valor { get; set; }

        public decimal rb_importe { get; set; }

        public decimal rb_rec { get; set; }

        public decimal rb_aux { get; set; }

        public string rb_estado { get; set; } = string.Empty;

        public string id_externo { get; set; } = string.Empty;
    }
}
