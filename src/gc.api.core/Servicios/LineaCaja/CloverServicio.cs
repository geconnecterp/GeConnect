using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace gc.api.core.Servicios.LineaCaja;

public class CloverServicio : PagoIntegradoServicioBase
{
    public CloverServicio(IUnitOfWork uow, ILoggerHelper logger) : base(uow, logger) { }
    public static decimal Recargo(decimal monto, decimal porcentaje) => gc.infraestructura.Core.ReglasCaja.PagosIntegrados.Recargo(monto, porcentaje);
    public List<CloverOpcionDto> Opciones(CloverContextoDto c, string? insId) => ObtenerOpciones<CloverOpcionDto>(c, insId, "T1", "POS Clover");
    // Cada solicitud representa un medio de pago independiente. El SP decide su resultado;
    // no se consulta ni se valida un estado persistido del POS o de otro detalle de Caja.
    public RespuestaDto Procesar(CloverPeticionDto req)
    {
        var c = req.contexto; var p = req.pago; ContextoValido(c);
        if (!Guid.TryParse(p.intento_id, out _) || string.IsNullOrWhiteSpace(p.ins_id) || p.ins_id.Length > 3 || p.co_tipo != c.co_tipo)
            throw new InvalidOperationException("La solicitud Clover no es válida.");
        var manual = !string.IsNullOrWhiteSpace(p.paymentid);
        p.paymentid = (p.paymentid ?? "").Trim().ToUpperInvariant();
        if (manual && !Regex.IsMatch(p.paymentid, "^[A-Z0-9]{4,30}$"))
            throw new InvalidOperationException("La recuperación requiere entre 4 y 30 caracteres alfanuméricos. El SP actual no admite IDs de tres sin iniciar un cobro.");
        var opcion = Opciones(c, p.ins_id).SingleOrDefault(o => o.pos_plan == p.pos_plan && o.cuota == p.cuotas_reales);
        if (opcion == null) throw new InvalidOperationException("La opción de cuotas no está disponible. Consulte la configuración de Clover con el administrador.");
        ValidarImportes(p, opcion);
        var parametros = new List<SqlParameter> { new("@adm_id", c.adm_id), new("@caja_id", c.caja_id), new("@caja_nro_proceso", c.caja_nro_proceso), new("@caja_nro_cierre", c.caja_nro_cierre), new("@cta_id", c.cta_id),
            new("@monto", SqlDbType.Decimal) { Precision = 15, Scale = 2, Value = p.monto_total }, new("@cashback", SqlDbType.Decimal) { Precision = 15, Scale = 2, Value = 0m }, new("@cuota", p.pos_plan), new("@qr", p.qr), new("@paymentid", SqlDbType.VarChar, 30) { Value = p.paymentid } };
        _logger.Log(TraceEventType.Information, $"[CLOVER][SP REQUEST] Operacion={c.operacion_id}; Intento={p.intento_id}\n" + JsonConvert.SerializeObject(parametros.ToDictionary(x => x.ParameterName, x => x.Value), Formatting.Indented));
        // Sin reintentos automáticos: una pérdida de respuesta no autoriza otro cobro.
        var filas = Ejecutar<CloverRespuestaSpDto>("SPGECO_CAJA_Clover_Pay", parametros);
        _logger.Log(TraceEventType.Information, "[CLOVER][SP RESPONSE]\n" + JsonConvert.SerializeObject(filas, Formatting.Indented));
        if (filas.Count != 1) throw new InvalidOperationException("El SP no devolvió una única respuesta de pago. Verifique el resultado antes de repetir.");
        var r = filas[0];
        // El SP vigente llama respuesta/ respuesta_msj a estas columnas. Se adapta
        // al contrato tradicional de la API sin modificar el procedimiento SQL.
        return new RespuestaDto {
            resultado = r.respuesta,
            resultado_msj = r.respuesta_msj ?? "",
            resultado_id = r.respuesta == 0 ? JsonConvert.SerializeObject(new[] { CrearValor(r, p, opcion.recargo_porcentaje) }) : ""
        };
    }
    public static Json_Valor CrearValor(CloverRespuestaSpDto r, CloverPreparacionDto p, decimal porcentajeRecargo) => CrearValorBase(r, p, porcentajeRecargo);
}
