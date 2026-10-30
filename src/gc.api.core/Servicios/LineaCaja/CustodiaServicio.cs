using System.Diagnostics;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Dtos.Cajas;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace gc.api.core.Servicios.LineaCaja;

public partial class CustodiaServicio(IUnitOfWork uow, ILoggerHelper logger, ICustodiaPersistencia persistencia) : Servicio<EntidadBase>(uow)
{
    public const string SpInstrumentos = "SPGECO_CAJA_Rend_Ins";
    public const string SpPendientes = "SPGECO_Vtas_PV_CTL_Rend_Para_Entrega";
    public const string SpCorrecciones = "SPGECO_Vtas_PV_CTL_Guardar";
    public const string SpEntrega = "SPGECO_Vtas_PV_CTL_Entregas_Guardar";
    public const string SpAcuse = "SPGECO_ENT_Datos";

    private List<T> Consultar<T>(string sp, List<SqlParameter> parametros) where T : class
    {
        var llamada = Guid.NewGuid().ToString("N");
        logger.Log(TraceEventType.Information, $"[CUSTODIA][SP REQUEST] {sp} | Llamada={llamada}\n{JsonConvert.SerializeObject(parametros.ToDictionary(p => p.ParameterName, p => p.Value), Formatting.Indented)}");
        try
        {
            var filas = _repository.EjecutarLstSpExt<T>(sp, parametros, true) ?? [];
            logger.Log(TraceEventType.Information, $"[CUSTODIA][SP RESPONSE] {sp} | Llamada={llamada} | Filas={filas.Count}");
            return filas;
        }
        catch (Exception ex)
        {
            logger.Log(TraceEventType.Error, $"[CUSTODIA][SP ERROR] {sp} | Llamada={llamada} | {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    public List<CustodiaInstrumentoDto> Instrumentos(string sucursal)
    {
        ValidarSucursal(sucursal);
        return Consultar<CustodiaInstrumentoDto>(SpInstrumentos, [new("@adm_id", sucursal), new("@tipo", "P")])
            .Where(i => i.ins_id?.Trim() is "PES" or "DOL")
            .Select(i => new CustodiaInstrumentoDto { ins_id = i.ins_id.Trim(), ins_desc = i.ins_desc?.Trim() ?? i.ins_id })
            .GroupBy(i => i.ins_id).Select(g => g.First()).OrderBy(i => i.ins_id == "PES" ? 0 : 1).ToList();
    }

    public List<CustodiaRendicionDto> Pendientes(string sucursal, string instrumento)
    {
        ValidarSucursal(sucursal);
        if (instrumento is not ("PES" or "DOL")) throw new ArgumentException("Instrumento de efectivo inválido.");
        // El SP instalado no filtra por adm_id/ins_id: el alcance se exige antes de exponer filas o reconstruir un comando.
        var filas = persistencia.Pendientes(sucursal, instrumento)
            .Where(r => r.adm_id?.Trim() == sucursal && r.ins_id?.Trim() == instrumento)
            .OrderBy(r => r.rend_fecha).ThenBy(r => r.caja_nro_proceso).ThenBy(r => r.caja_nro_cierre)
            .ThenBy(r => r.caja_nro_rend).ThenBy(r => r.rend_item).ToList();
        if (filas.Any(r => string.IsNullOrWhiteSpace(r.caja_nro_proceso) || string.IsNullOrWhiteSpace(r.caja_id) || r.rend_fecha == default) ||
            filas.Select(r => r.clave).Distinct().Count() != filas.Count)
            throw new InvalidOperationException("La consulta de rendiciones devolvió identificadores incompletos o duplicados.");
        return filas;
    }

    public CustodiaConsultaDto Consulta(string sucursal)
    {
        var instrumentos = Instrumentos(sucursal);
        return new(instrumentos, instrumentos.SelectMany(i => Pendientes(sucursal, i.ins_id)).ToList());
    }

    public List<CustodiaAcuseFilaDto> Acuse(string sucursal, string entrega)
    {
        ValidarSucursal(sucursal);
        if (string.IsNullOrWhiteSpace(entrega) || entrega.Length > 20) throw new ArgumentException("Identificador de entrega inválido.");
        var filas = Consultar<CustodiaAcuseFilaDto>(SpAcuse, [new("@ent_compte", entrega)]);
        if (filas.Count == 0) throw new ArgumentException("No hay datos disponibles para imprimir este acuse. No registre nuevamente la entrega.");
        if (filas.Any(r => r.adm_id?.Trim() != sucursal || r.ent_compte?.Trim() != entrega.Trim()))
            throw new UnauthorizedAccessException("La entrega no pertenece a la sucursal de la sesión.");
        return filas;
    }

    public List<CustodiaLineaDto> ReconstruirSeleccion(string sucursal, CustodiaOperacionDto operacion) => PrepararSeleccion(sucursal, operacion).Select(c => c.Despues).ToList();

    private List<CustodiaCambio> PrepararSeleccion(string sucursal, CustodiaOperacionDto operacion)
    {
        if (operacion.solicitud == Guid.Empty) throw new ArgumentException("La solicitud de operación no es válida.");
        var seleccion = operacion.seleccion ?? [];
        if (seleccion.Count == 0) throw new ArgumentException("Seleccione al menos una rendición.");
        if (seleccion.Count > 10000 || seleccion.Select(s => s.clave).Distinct().Count() != seleccion.Count)
            throw new ArgumentException("La selección contiene demasiadas filas o rendiciones duplicadas.");
        var instrumentos = Instrumentos(sucursal).Select(i => i.ins_id).ToHashSet();
        if (seleccion.Any(s => !instrumentos.Contains(s.ins_id))) throw new ArgumentException("El instrumento no está habilitado para custodia.");
        var pendientes = seleccion.Select(s => s.ins_id).Distinct().SelectMany(i => Pendientes(sucursal, i)).ToDictionary(r => r.clave);
        var resultado = new List<CustodiaCambio>();
        foreach (var item in seleccion)
        {
            if (!pendientes.TryGetValue(item.clave, out var fila) || item.version != fila.version || item.ins_id != fila.ins_id.Trim())
                throw new ArgumentException("Una rendición cambió o ya fue entregada. Actualice la consulta y revise la selección.");
            resultado.Add(new(fila, CustodiaLineaDto.Crear(fila, item.ObtenerImporte())));
        }
        return resultado;
    }

    public CustodiaResultadoDto Corregir(CustodiaOperacionDto operacion, string sucursal, string usuario)
    {
        if (string.IsNullOrWhiteSpace(usuario) || usuario.Length > 10) throw new ArgumentException("El usuario de la sesión no es válido.");
        var cambios = PrepararSeleccion(sucursal, operacion).Where(c => c.Antes.rend_importe_ok != c.Despues.rend_importe_ok).ToList();
        if (cambios.Count == 0) return new(true, "No hay correcciones pendientes de guardar.");
        var resultado = persistencia.Corregir(cambios, sucursal, usuario, operacion.solicitud);
        return new(resultado.Codigo == 0, string.IsNullOrWhiteSpace(resultado.Mensaje) ? (resultado.Codigo == 0 ? "Las correcciones se guardaron correctamente." : "El servidor rechazó las correcciones.") : resultado.Mensaje);
    }

    private static void ValidarSucursal(string sucursal)
    {
        if (string.IsNullOrWhiteSpace(sucursal) || sucursal.Length > 4) throw new ArgumentException("La sucursal de la sesión no es válida.");
    }
}
