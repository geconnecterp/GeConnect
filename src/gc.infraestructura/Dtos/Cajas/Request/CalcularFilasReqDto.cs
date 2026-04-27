namespace gc.infraestructura.Dtos.Cajas.Request
{
    public class CalcularFilasReqDto
    {
        public string caja_id { get; set; } = string.Empty;              // varchar(4)
        public string usu_id { get; set; } = string.Empty;               // varchar(10)
        public string adm_id { get; set; } = string.Empty;               // varchar(10)
        public string lp_id { get; set; } = string.Empty;                // char(2)
        public string caja_nro_proceso { get; set; } = string.Empty;     // varchar(15)
        public string caja_nro_cierre { get; set; } = string.Empty;                        // int

        public string cta_id { get; set; } = string.Empty;               // varchar(10)
        public decimal ctac_dto { get; set; }                            // decimal(5,2)
        public string ctc_id { get; set; } = string.Empty;               // char(2)

        public string tco_letra { get; set; } = string.Empty;            // varchar(1)
        public string tco_id { get; set; } = string.Empty;               // varchar(3)
        public string tco_id_ori { get; set; } = string.Empty;           // varchar(3)
        public string cm_compte_ori { get; set; } = string.Empty;        // varchar(3)

        public string afip_id { get; set; } = string.Empty;              // char(2)
        public string afip_desc { get; set; } = string.Empty;            // varchar(80)

        public string cta_ib_nro { get; set; } = string.Empty;           // varchar(15)
        public string ib_id { get; set; } = string.Empty;                // char(1)

        public string pib_cert { get; set; } = string.Empty;             // char(1)
        public DateTime? pib_cert_vto { get; set; }                      // datetime
        public string piva_cert { get; set; } = string.Empty;            // char(1)
        public DateTime? piva_cert_vto { get; set; }                     // datetime

        public short tot_rows { get; set; }                              // smallint
        public decimal tot_cantidad { get; set; }                        // decimal(15,3)
        public decimal tot_pvta { get; set; }                            // decimal(15,2)

        // Este campo debe contener el JSON serializado del array de items.
        public string json_p { get; set; } = "[]";
    }
}
