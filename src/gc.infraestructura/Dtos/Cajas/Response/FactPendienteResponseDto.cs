using gc.infraestructura.Dtos.Almacen.AjusteDeStock;

namespace gc.infraestructura.Dtos.Cajas.Response
{
    public class FactPendienteResponseDto
    {
        public string cta_id { get; set; } = string.Empty;
        public string co_pd_nombre { get; set; } = string.Empty;

        public string co_pd_doc { get; set; } = string.Empty;

        public string dia_movi { get; set; } = string.Empty;

        public string tco_id { get; set; } = string.Empty;

        public string cm_compte { get; set; } = string.Empty;

        public int cm_compte_cuota { get; set; }

        public DateTime? cv_fecha_vto { get; set; }

        public decimal cv_importe { get; set; }

        public decimal cv_importe_ori { get; set; }

        public string cv_concepto { get; set; } = string.Empty;

        public int? ve_id { get; set; }

        public int? ccb_id { get; set; }

        public string ctacte { get; set; } = string.Empty;

        public string carga { get; set; } = string.Empty;

        public string carga_obligatoria { get; set; } = string.Empty;

        public string caja_nro_proceso { get; set; } = string.Empty;
        public short caja_nro_cierre { get; set; }
        public int caja_nro_operacion { get; set; }

        public bool seleccionado { get; set; }


    }
}
