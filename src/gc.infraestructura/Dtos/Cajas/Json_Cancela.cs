using System.Text.Json.Serialization;

namespace gc.infraestructura.Dtos.Cajas
{
    public class Json_Cancela
    {
        public string? cta_id { get; set; }              // varchar(10)

        public string? dia_movi { get; set; }            // varchar(15)

        public string? tco_id { get; set; }              // char(3)

        public string? cm_compte { get; set; }           // varchar(15)

        public short? cm_compte_cuota { get; set; }      // smallint

        public DateTime? cv_fecha_vto { get; set; }      // datetime

        public decimal? cv_importe { get; set; }         // decimal(15,2)

        public decimal? cv_importe_ori { get; set; }     // decimal(15,2)

        public string? cv_estado { get; set; }           // char(1)

        public DateTime? cv_fecha_carga { get; set; }    // datetime

        public string? cv_concepto { get; set; }         // varchar(100)

        public string? ve_id { get; set; }               // char(2)

        public string? ccb_id { get; set; }              // varchar(10)

    }
}
