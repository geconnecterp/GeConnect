using gc.infraestructura.Dtos.Cajas.Response;
namespace gc.infraestructura.Dtos.Cajas;

// Preparación de un cobro integrado. El servidor obtiene el contexto y valida catálogo/saldo.
public class PagoIntegradoContextoDto
{
    public string adm_id { get; set; } = "";
    public string caja_id { get; set; } = "";
    public string caja_nro_proceso { get; set; } = "";
    public short caja_nro_cierre { get; set; }
    public string cta_id { get; set; } = "";
    public string usu_id { get; set; } = "";
    public string co_tipo { get; set; } = "";
    public string operacion_id { get; set; } = "";
    public string huella { get; set; } = "";
}
public class PagoIntegradoOpcionDto
{
    public string ins_id { get; set; } = ""; // Código real del catálogo; el SP de opciones recibe char(3).
    public int cuota { get; set; }
    public decimal recargo_porcentaje { get; set; }
    public int pos_plan { get; set; }
    public string pos_desc { get; set; } = "";
}
public class PagoIntegradoPreparacionDto
{
    public string ins_id { get; set; } = "";
    public string co_tipo { get; set; } = "";
    public decimal monto_base { get; set; }
    public decimal monto_total { get; set; }
    public decimal recargo { get; set; } // Importe calculado; se confirma en json_valores.rb_aux.
    public int cuotas_reales { get; set; }
    public short pos_plan { get; set; }
    public string paymentid { get; set; } = "";
    public string intento_id { get; set; } = "";
    public List<Json_Valor> valores { get; set; } = [];
    public List<Json_Union> uniones { get; set; } = [];
    // El navegador nunca decide PV/sucursal/proceso/cierre/cliente.
}
public class PagoIntegradoRespuestaSpDto
{
    public short respuesta { get; set; }
    public string respuesta_msj { get; set; } = "";
    public string cta_id { get; set; } = "";
    public string? ins_id { get; set; }
    public string? rb_dato1_valor { get; set; } // paymentid, sin relleno ni transformación.
    public string? rb_dato2_valor { get; set; } // lote Clover / primera parte de order_id MP
    public string? rb_dato3_valor { get; set; } // cupón Clover / segunda parte de order_id MP
    public short rb_opcion_cuota { get; set; } // SP devuelve @cuota (recibe pos_plan).
    public DateTime rb_fecha_valor { get; set; }
    public decimal rb_importe { get; set; }
    public decimal pos_importe { get; set; }
    public decimal pos_cashback { get; set; }
    public string tcf_id { get; set; } = "";
}
public enum TipoPagoPoint : short { Debito = 1, Credito = 2, Qr = 3 }
public sealed class MercadoPagoPreparacionDto : PagoIntegradoPreparacionDto
{
    public short tipo { get; set; }
}
public sealed class MercadoPagoPeticionDto
{
    public PagoIntegradoContextoDto contexto { get; set; } = new();
    public MercadoPagoPreparacionDto pago { get; set; } = new();
}
