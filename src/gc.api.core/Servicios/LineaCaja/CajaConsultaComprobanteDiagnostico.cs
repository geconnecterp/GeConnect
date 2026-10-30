using System.Diagnostics;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Interfaces;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace gc.api.core.Servicios.LineaCaja;

// Diagnóstico de las consultas del comprobante; no altera parámetros ni resultados.
internal static class CajaConsultaComprobanteDiagnostico
{
    internal static List<T> Ejecutar<T>(ILoggerHelper logger, string sp,
        List<SqlParameter> parametros, Func<List<T>> consultar)
    {
        var llamada = Guid.NewGuid().ToString("N");
        var entrada = parametros.ToDictionary(p => p.ParameterName,
            p => p.Value == DBNull.Value ? null : p.Value);
        logger.Log(TraceEventType.Information,
            $"[CAJA][COMPROBANTE][SP REQUEST] {sp} | Llamada={llamada}\n" +
            CajaJsonDiagnostico.Formatear(JsonConvert.SerializeObject(entrada)));
        try
        {
            var resultado = consultar();
            logger.Log(TraceEventType.Information,
                $"[CAJA][COMPROBANTE][SP RESPONSE] {sp} | Llamada={llamada}\n" +
                CajaJsonDiagnostico.Formatear(JsonConvert.SerializeObject(resultado), expandirJsonAnidado: false));
            return resultado;
        }
        catch (Exception ex)
        {
            // Un fallo de SQL o mapeo no es una respuesta vacía ni autoriza reintentar.
            logger.Log(TraceEventType.Error,
                $"[CAJA][COMPROBANTE][SP ERROR] {sp} | Llamada={llamada}\n{ex}");
            throw;
        }
    }
}
