using gc.infraestructura.Dtos.Cajas.Request;

namespace gc.infraestructura.Dtos.Cajas
{
    public class ProductoFactJsonDto
    {
        public ProductoFactJsonDto()
        {
           
        }

        /// <summary>
        /// ✅ NUEVO v1.0: Constructor que mapea desde ProductoDatosResponseDto.
        /// Resuelve el error de conversión en ProductoFactController.
        /// </summary>
        /// <param name="p">El objeto de respuesta del servicio de productos.</param>
        public ProductoFactJsonDto(ProductoDatosResponseDto p)
        {
            p_id = p.p_id ?? string.Empty;
            p_id_barrado = p.p_id_barrado ?? string.Empty;
            p_desc = p.p_desc ?? string.Empty;
            p_pcosto = p.p_pcosto;
            p_pcosto_repo = p.p_pcosto_repo;
            in_alicuota = p.in_alicuota;
            p_in = p.p_in;
            iva_situacion = p.iva_situacion ?? string.Empty;
            iva_alicuota = p.iva_alicuota;
            p_iva = p.p_iva;
            po = p.po;
            po_limite = (int)p.po_limite;
            p_pneto = p.p_pneto;
            p_margen_imp = 0; // Valor por defecto, ya que no viene en el DTO de origen
            p_margen_vig = 0; // Valor por defecto
            p_pvta = p.p_pvta;
            lp_prevision_tot = 0; // Valor por defecto
            lp_prevision_pin = 0; // Valor por defecto
            cantidad_tot = p.cantidad_tot;
            p_pvta_tot = p.p_pvta * p.cantidad_tot; // Cálculo directo
            bultos = 0; // Valor por defecto
            cm_gravado = p.cm_gravado;
            cm_no_gravado = p.cm_no_gravado;
            cm_exento = p.cm_exento;
            cm_iva = p.cm_iva;
            cm_ii = p.cm_ii;
            cm_dto = p.cm_dto;
            cm_dto_porc = p.cm_dto_porc;
            cta_id = p.cta_id ?? string.Empty;
            pre_id = p.pre_id ?? string.Empty;
            cpf_nro = p.cpf_nro ?? string.Empty;
            cmb_p_id = string.Empty; // Valor por defecto
            cmd_cmb = string.Empty; // Valor por defecto
            cmd_cmb_id = string.Empty; // Valor por defecto
            cmd_cmb_dto = 0; // Valor por defecto
            cmd_cmb_cant = 0; // Valor por defecto
            cmd_cmb_desc = string.Empty; // Valor por defecto
            barre = string.Empty; // Valor por defecto
            item = p.item;
        }

        public string p_id { get; set; }= string.Empty;
        public string p_id_barrado { get; set; } = string.Empty;
        public string p_desc { get; set; } = string.Empty;  
        public decimal p_pcosto { get; set; }
        public decimal p_pcosto_repo { get; set; }
        public decimal in_alicuota { get; set; }
        public decimal p_in { get; set; }
        public string iva_situacion { get; set; } = string.Empty;
        public decimal iva_alicuota { get; set; }
        public decimal p_iva { get; set; }
        public bool po { get; set; }
        public int po_limite { get; set; }
        public decimal p_pneto { get; set; }
        public decimal p_margen_imp { get; set; }
        public decimal p_margen_vig { get; set; }
        public decimal p_pvta { get; set; }
        public decimal lp_prevision_tot { get; set; }
        public decimal lp_prevision_pin { get; set; }
        public decimal cantidad_tot { get; set; }
        public decimal p_pvta_tot { get; set; }
        public int bultos { get; set; }
        public decimal cm_gravado { get; set; }
        public decimal cm_no_gravado { get; set; }
        public decimal cm_exento { get; set; }
        public decimal cm_iva { get; set; }
        public decimal cm_ii { get; set; }
        public decimal cm_dto { get; set; }
        public decimal cm_dto_porc { get; set; }
        public string cta_id { get; set; } = string.Empty;
        public string pre_id { get; set; } = string.Empty;
        public string cpf_nro { get; set; } = string.Empty;
        public string cmb_p_id { get; set; } = string.Empty;
        public string cmd_cmb { get; set; } = string.Empty;
        public string cmd_cmb_id { get; set; } = string.Empty;
        public decimal cmd_cmb_dto { get; set; }
        public decimal cmd_cmb_cant { get; set; }
        public string cmd_cmb_desc { get; set; } = string.Empty;
        public string barre { get; set; } = string.Empty;
        public int item { get; set; }
    }
}
