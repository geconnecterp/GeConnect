using gc.infraestructura.Dtos.Cajas.Response;

namespace gc.infraestructura.Dtos.Cajas;

// Contratos de preparación. No ejecutan SP ni contienen acceso al POS.
public sealed class CloverOpcionDto
{
    public string ins_id { get; set; } = ""; // Código real del catálogo; el SP de opciones recibe char(3).
    public int cuota { get; set; }
    public decimal recargo_porcentaje { get; set; }
    public int pos_plan { get; set; }
    public string pos_desc { get; set; } = "";
}

public sealed class CloverPreparacionDto
{
    public string ins_id { get; set; } = "";
    public string co_tipo { get; set; } = "";
    public decimal monto_base { get; set; }
    public decimal monto_total { get; set; }
    public decimal recargo { get; set; } // Importe calculado; se confirma en json_valores.rb_aux.
    public int cuotas_reales { get; set; }
    public short pos_plan { get; set; }
    public bool qr { get; set; }
    public string paymentid { get; set; } = "";
    public string intento_id { get; set; } = "";
    public List<Json_Valor> valores { get; set; } = [];
    public List<Json_Union> uniones { get; set; } = [];
    // El navegador nunca decide PV/sucursal/proceso/cierre/cliente.
}

public sealed class CloverRespuestaSpDto
{
    public short respuesta { get; set; }
    public string respuesta_msj { get; set; } = "";
    public string cta_id { get; set; } = "";
    public string? ins_id { get; set; }
    public string? rb_dato1_valor { get; set; } // paymentid, sin relleno ni transformación.
    public string? rb_dato2_valor { get; set; } // lote
    public string? rb_dato3_valor { get; set; } // cupón
    public short rb_opcion_cuota { get; set; } // SP devuelve @cuota (recibe pos_plan).
    public DateTime rb_fecha_valor { get; set; }
    public decimal rb_importe { get; set; }
    public decimal pos_importe { get; set; }
    public decimal pos_cashback { get; set; }
    public string tcf_id { get; set; } = "";
}
