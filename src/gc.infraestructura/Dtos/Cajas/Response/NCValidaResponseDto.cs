using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Cajas.Response
{
    public class NCValidaResponseDto
    {
        public string tco_id { get; set; }=string.Empty;
        public string tco_desc { get; set; }= string.Empty;
        public string cm_compte { get; set; }= string.Empty;
        public int? cm_repetido { get; set; }
        public string dia_movi { get; set; } = string.Empty;
        public string afip_id { get; set; }= string.Empty;
        public string afip_desc { get; set; }= string.Empty;
        public string adm_id { get; set; }= string.Empty;
        public string cta_id { get; set; }= string.Empty;
        public string mon_codigo { get; set; }= string.Empty;
        public string mv_id { get; set; }= string.Empty;
        public string usu_id { get; set; }= string.Empty;
        public string tdoc_id { get; set; }= string.Empty;
        public string cm_cuit { get; set; }= string.Empty;
        public string cm_nombre { get; set; }= string.Empty;
        public string cm_domicilio { get; set; }= string.Empty;
        public DateTime? cm_fecha { get; set; }
        public string cm_libro_iva { get; set; }= string.Empty; 
        public decimal? cm_gravado { get; set; }
        public decimal? cm_no_gravado { get; set; }
        public decimal? cm_exento { get; set; }
        public decimal? cm_dto { get; set; }
        public decimal? cm_dto_porc { get; set; }
        public decimal? cm_ii { get; set; }
        public decimal? cm_iva { get; set; }
        public decimal? cm_percepciones { get; set; }
        public decimal? cm_total { get; set; }
        public string cm_compte_obs { get; set; }= string.Empty;
        public string cm_controlador_fiscal { get; set; }= string.Empty;
        public decimal? cm_percep_imp_nacionales { get; set; }
        public decimal? cm_percep_imp_municipales { get; set; }
        public decimal? cm_ib { get; set; }
        public string cm_cae { get; set; }= string.Empty;
        public DateTime? cm_cae_vto { get; set; }
        public string cm_caea_procesado { get; set; }= string.Empty;
        public DateTime? cm_fecha_carga { get; set; }
        public string caja_nro_proceso { get; set; }= string.Empty;
        public short? caja_nro_cierre { get; set; }
        public string caja_id { get; set; }= string.Empty;
        public string lp_id { get; set; }= string.Empty;
        public string ve_id { get; set; }= string.Empty;
        /// <summary>
        /// Tipo de NC determinado por el SP.
        /// </summary>
        public string nc_tco_letra { get; set; } = string.Empty;
        public string nc_tco_id { get; set; } = string.Empty;
        public string nc_tco_desc { get; set; } = string.Empty;

        public int nc_dv_dist { get; set; }
        public int nc_dv_pago_diferido { get; set; }
        public int nc_ctacte { get; set; }
        public int nc_ya_emitida { get; set; }
        public int nc_sin_detalle { get; set; }
        public int nc_fecha_supero_dias { get; set; }       
    }
}
