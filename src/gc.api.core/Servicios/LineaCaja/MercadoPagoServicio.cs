using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
namespace gc.api.core.Servicios.LineaCaja;

public class MercadoPagoServicio : PagoIntegradoServicioBase
{
    public MercadoPagoServicio(IUnitOfWork uow, ILoggerHelper logger) : base(uow, logger) { }
    public List<PagoIntegradoOpcionDto> Opciones(PagoIntegradoContextoDto c, string? insId)
    {
        if (!string.IsNullOrWhiteSpace(insId) && !string.Equals(insId.Trim(), "M00", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("El instrumento solicitado no corresponde a Mercado Pago Point.");
        return ObtenerOpciones<PagoIntegradoOpcionDto>(c, "M00", "T2", "Mercado Pago Point");
    }
    public RespuestaDto Procesar(MercadoPagoPeticionDto req)
    {
        var c = req.contexto; var p = req.pago; ContextoValido(c);
        if (!Guid.TryParse(p.intento_id, out _) || p.co_tipo != c.co_tipo || p.ins_id != "M00")
            throw new InvalidOperationException("La solicitud de Mercado Pago no es válida.");
        var manual = !string.IsNullOrWhiteSpace(p.paymentid);
        p.paymentid = (p.paymentid ?? "").Trim().ToUpperInvariant();
        // LEN > 3 es la rama de consulta del SP vigente. Tres nunca debe iniciar otro cobro.
        if (manual && !Regex.IsMatch(p.paymentid, "^[A-Z0-9]{4,30}$"))
            throw new InvalidOperationException("La carga manual requiere entre 4 y 30 caracteres alfanuméricos. Los IDs de tres caracteres están bloqueados por el contrato actual del SP.");
        var opcion = Opciones(c, p.ins_id).SingleOrDefault(o => o.pos_plan == p.pos_plan && o.cuota == p.cuotas_reales)
            ?? throw new InvalidOperationException("La opción de cuotas de Mercado Pago no está vigente.");
        if (p.tipo is < 1 or > 3 || opcion.cuota > 1 && p.tipo != (short)TipoPagoPoint.Credito)
            throw new InvalidOperationException("Seleccione Débito, Crédito o QR. Con más de una cuota sólo se admite Crédito.");
        ValidarImportes(p, opcion);
        var parametros = new List<SqlParameter> {
            new("@adm_id",SqlDbType.Char,4){Value=c.adm_id}, new("@caja_id",SqlDbType.Char,4){Value=c.caja_id},
            new("@caja_nro_proceso",SqlDbType.VarChar,15){Value=c.caja_nro_proceso}, new("@caja_nro_cierre",SqlDbType.SmallInt){Value=c.caja_nro_cierre},
            new("@cta_id",SqlDbType.VarChar,10){Value=c.cta_id}, new("@monto",SqlDbType.Decimal){Precision=15,Scale=2,Value=p.monto_total},
            new("@cashback",SqlDbType.Decimal){Precision=15,Scale=2,Value=0m}, new("@cuota",SqlDbType.SmallInt){Value=p.pos_plan},
            new("@type",SqlDbType.SmallInt){Value=p.tipo}, new("@paymentid",SqlDbType.VarChar,30){Value=p.paymentid}
        };
        var llamada = Guid.NewGuid().ToString("N"); var reloj = Stopwatch.StartNew();
        _logger.Log(TraceEventType.Information, $"[MP][SP REQUEST] SPGECO_CAJA_MP_Pay | Llamada={llamada} | Operacion={c.operacion_id} | Intento={p.intento_id}\n" + gc.infraestructura.Core.Helpers.CajaJsonDiagnostico.Formatear(JsonConvert.SerializeObject(parametros.ToDictionary(x=>x.ParameterName,x=>x.Value))));
        // El SP decide el resultado. No consultar estado Point ni reintentar cobros.
        List<PagoIntegradoRespuestaSpDto> filas;
        try { filas = Ejecutar<PagoIntegradoRespuestaSpDto>("SPGECO_CAJA_MP_Pay", parametros); }
        catch (Exception ex) {
            _logger.Log(TraceEventType.Error, $"[MP][SP SIN_RESPONSE] Llamada={llamada} | {reloj.ElapsedMilliseconds} ms | Tipo={ex.GetType().Name}");
            throw; // Resultado incierto: la aplicación no repite la llamada bancaria.
        }
        _logger.Log(TraceEventType.Information, $"[MP][SP RESPONSE] Llamada={llamada} | {reloj.ElapsedMilliseconds} ms\n" + JsonConvert.SerializeObject(filas, Formatting.Indented));
        if (filas.Count != 1) throw new InvalidOperationException("Mercado Pago no devolvió una única respuesta. Verifique antes de repetir.");
        var r=filas[0];
        if (r.respuesta == 0 && (string.IsNullOrWhiteSpace(r.ins_id) || string.IsNullOrWhiteSpace(r.rb_dato1_valor) || r.rb_importe <= 0))
            throw new InvalidOperationException("La respuesta no identifica el pago aprobado. Conserve la referencia y verifique antes de repetir.");
        return new RespuestaDto { resultado=r.respuesta, resultado_msj=r.respuesta_msj ?? "", resultado_id=r.respuesta==0 ? JsonConvert.SerializeObject(new[]{CrearValorBase(r,p,opcion.recargo_porcentaje)}) : "" };
    }
}
