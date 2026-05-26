namespace gc.infraestructura.Dtos.Cajas.Request
{
    /// <summary>
    /// DTO para finalizar el proceso de pago
    /// Contiene todos los valores ingresados y datos del comprobante
    /// </summary>
    public class FinalizarPagoReqDto
    {
        /// <summary>
        /// Tipo de operación: 'CR' (Cliente Registrado) o 'CF' (Consumidor Final)
        /// </summary>
        public string co_tipo { get; set; } = string.Empty;

        /// <summary>
        /// ID de cuenta/cliente
        /// </summary>
        public string cta_id { get; set; } = string.Empty;

        /// <summary>
        /// ID de administración
        /// </summary>
        public string adm_id { get; set; } = string.Empty;

        /// <summary>
        /// Punto de venta
        /// </summary>
        public string pv_id { get; set; } = string.Empty;

        /// <summary>
        /// Total a pagar (antes de recargos/descuentos)
        /// </summary>
        public decimal total_pagar { get; set; }

        /// <summary>
        /// Total de recargos aplicados
        /// </summary>
        public decimal total_recargos { get; set; }

        /// <summary>
        /// Total de descuentos aplicados
        /// </summary>
        public decimal total_descuentos { get; set; }

        /// <summary>
        /// Total de valores ingresados
        /// </summary>
        public decimal total_valores { get; set; }

        /// <summary>
        /// Total final = total_pagar + recargos - descuentos
        /// </summary>
        public decimal total_final { get; set; }

        /// <summary>
        /// Diferencia (total_final - total_valores)
        /// </summary>
        public decimal diferencia { get; set; }

        /// <summary>
        /// Lista de valores ingresados
        /// </summary>
        public List<ValorPagoDto> valores { get; set; } = new List<ValorPagoDto>();

        /// <summary>
        /// ✅ NUEVO: Lista de Notas de Crédito/Créditos usados
        /// </summary>
        public List<NotaCreditoUsadaDto> notas_credito { get; set; } = new List<NotaCreditoUsadaDto>();

        /// <summary>
        /// Lista de productos de la factura (JSON serializado)
        /// </summary>
        public string? json_productos { get; set; }

        /// <summary>
        /// Lista de subtotales de la factura (JSON serializado)
        /// </summary>
        public string? json_subtotales { get; set; }

        /// <summary>
        /// Observaciones del comprobante
        /// </summary>
        public string? observaciones { get; set; }

        // ✅ CAMPOS ADICIONALES REQUERIDOS PARA CajaOpeConfirmarReq
        public string caja_id { get; set; } = string.Empty;
        public string usu_id { get; set; } = string.Empty;
        public string lp_id { get; set; } = string.Empty;
        public string caja_nro_proceso { get; set; } = string.Empty;
        public int caja_nro_cierre { get; set; }
        public string? usu_id_autoriza { get; set; }
        public decimal ctac_dto { get; set; }
        public string ctc_id { get; set; } = string.Empty;
        public string tco_letra { get; set; } = string.Empty;
        public string tco_id_ori { get; set; } = string.Empty;
        public string cm_compte_ori { get; set; } = string.Empty;
        public string afip_id { get; set; } = string.Empty;
        public string tdoc_id { get; set; } = string.Empty;
        public string cta_documento { get; set; } = string.Empty;
        public string cta_denominacion { get; set; } = string.Empty;
        public string cta_domicilio { get; set; } = string.Empty;
        public string ve_id { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO individual de cada valor de pago
    /// Estructura compatible con @json_valores del stored procedure
    /// ✅ ACTUALIZADO: Todos los campos requeridos por el SP
    /// </summary>
    public class ValorPagoDto
    {
        /// <summary>
        /// Número de ítem del valor (secuencial 1...n)
        /// </summary>
        public int rb_nro_valor { get; set; }

        /// <summary>
        /// ID del instrumento de pago
        /// </summary>
        public int ins_id { get; set; }

        /// <summary>
        /// Dato adicional 1 del instrumento (ej: banco, emisor)
        /// </summary>
        public string? rb_dato1_valor { get; set; }

        /// <summary>
        /// Dato adicional 2 del instrumento (ej: sucursal, marca)
        /// </summary>
        public string? rb_dato2_valor { get; set; }

        /// <summary>
        /// Dato adicional 3 del instrumento (ej: número de cheque/tarjeta)
        /// </summary>
        public string? rb_dato3_valor { get; set; }

        /// <summary>
        /// Opción de cuota seleccionada (1 si no aplica)
        /// </summary>
        public int rb_opcion_cuota { get; set; } = 1;

        /// <summary>
        /// ✅ NUEVO: Indica si es cupón manual ('S' o 'N')
        /// </summary>
        public string rb_cupon_manual { get; set; } = "N";

        /// <summary>
        /// ✅ NUEVO: Indica si es cheque diferido ('S' o 'N')
        /// </summary>
        public string rb_ch_dif { get; set; } = "N";

        /// <summary>
        /// Fecha de valor/vencimiento (null si no aplica)
        /// </summary>
        public DateTime? rb_fecha_valor { get; set; }

        /// <summary>
        /// Importe del valor
        /// </summary>
        public decimal rb_importe { get; set; }

        /// <summary>
        /// ✅ NUEVO: Estado del valor ('N' = Nuevo, 'P' = Pendiente, etc.)
        /// </summary>
        public string rb_estado { get; set; } = "N";

        /// <summary>
        /// Recargo asociado al valor (por cuotas, etc.)
        /// </summary>
        public decimal rb_rec { get; set; } = 0;

        /// <summary>
        /// ✅ NUEVO: Campo auxiliar (generalmente 0)
        /// </summary>
        public decimal rb_aux { get; set; } = 0;

        /// <summary>
        /// ID externo del valor (si fue precargado desde valores pendientes)
        /// </summary>
        public string? id_externo { get; set; }

        // ✅ CAMPOS ADICIONALES PARA CONTROL
        /// <summary>
        /// Tipo de cuenta financiera (EF, CH, TC, etc.)
        /// </summary>
        public string? tcf_id { get; set; }

        /// <summary>
        /// Descripción del instrumento
        /// </summary>
        public string? descripcion { get; set; }

        /// <summary>
        /// Origen del valor (MANUAL, PENDIENTE, NC_PREVIA)
        /// </summary>
        public string? origen { get; set; }
    }

    /// <summary>
    /// ✅ NUEVO: DTO para Notas de Crédito usadas (json_union)
    /// Estructura para los créditos aplicados en el pago
    /// </summary>
    public class NotaCreditoUsadaDto
    {
        /// <summary>
        /// ID de cuenta/cliente
        /// </summary>
        public string cta_id { get; set; } = string.Empty;

        /// <summary>
        /// Diario de movimiento de la NC
        /// </summary>
        public string dia_movi { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de comprobante de la NC
        /// </summary>
        public string tco_id { get; set; } = string.Empty;

        /// <summary>
        /// Número de comprobante de la NC
        /// </summary>
        public string cm_compte { get; set; } = string.Empty;

        /// <summary>
        /// Número de cuota del comprobante (si aplica)
        /// </summary>
        public string cm_compte_cuota { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de vencimiento del valor
        /// </summary>
        public DateTime? cv_fecha_vto { get; set; }

        /// <summary>
        /// Importe utilizado de la NC
        /// </summary>
        public decimal cv_importe { get; set; }

        /// <summary>
        /// Importe original de la NC
        /// </summary>
        public decimal cv_importe_ori { get; set; }

        /// <summary>
        /// Fecha de carga del valor
        /// </summary>
        public DateTime? cv_fecha_carga { get; set; }

        /// <summary>
        /// Concepto del valor
        /// </summary>
        public string? cv_concepto { get; set; }

        /// <summary>
        /// ID del vendedor
        /// </summary>
        public string? ve_id { get; set; }

        /// <summary>
        /// ID de cuenta bancaria (si aplica)
        /// </summary>
        public string? ccb_id { get; set; }
    }
}
