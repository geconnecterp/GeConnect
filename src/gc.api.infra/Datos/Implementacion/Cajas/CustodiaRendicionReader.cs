using System.Data.Common;
using gc.infraestructura.Dtos.Cajas;

namespace gc.api.infra.Datos.Implementacion.Cajas;

// Ordinales una sola vez; se descartan otras sucursales/instrumentos antes de construir DTO.
public static class CustodiaRendicionReader
{
    public static List<CustodiaRendicionDto> Leer(DbDataReader reader, string sucursal, string instrumento, CancellationToken cancellationToken = default)
    {
        var nombres = new[] { "adm_id", "adm_nombre", "ins_id", "caja_nro_proceso", "caja_nro_cierre", "caja_nro_rend", "rend_item", "caja_id", "rend_tipo", "rend_tipo_desc", "rend_fecha", "rend_importe_arq", "rend_importe_ok", "usu_id", "usu_apellidoynombre", "rend_estado", "rend_entrega" };
        var columnas = nombres.ToDictionary(n => n, reader.GetOrdinal);
        string Texto(string nombre) => reader.IsDBNull(columnas[nombre]) ? "" : reader.GetValue(columnas[nombre]).ToString() ?? "";
        string? Indicador(string nombre) => reader.IsDBNull(columnas[nombre]) ? null : Texto(nombre);
        object Obligatorio(string nombre) => reader.IsDBNull(columnas[nombre]) ? throw new InvalidOperationException("La consulta de custodia devolvió " + nombre + " nulo.") : reader.GetValue(columnas[nombre]);
        var resultado = new List<CustodiaRendicionDto>();
        while (reader.Read())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (Texto("adm_id").Trim() != sucursal || Texto("ins_id").Trim() != instrumento) continue;
            resultado.Add(new()
            {
                adm_id = Texto("adm_id"), adm_nombre = Texto("adm_nombre"), ins_id = Texto("ins_id"),
                caja_nro_proceso = Texto("caja_nro_proceso"), caja_nro_cierre = Convert.ToInt16(Obligatorio("caja_nro_cierre")),
                caja_nro_rend = Convert.ToInt16(Obligatorio("caja_nro_rend")), rend_item = Convert.ToInt16(Obligatorio("rend_item")),
                caja_id = Texto("caja_id"), rend_tipo = Indicador("rend_tipo"), rend_tipo_desc = Texto("rend_tipo_desc"),
                rend_fecha = Convert.ToDateTime(Obligatorio("rend_fecha")), rend_importe_arq = Convert.ToDecimal(Obligatorio("rend_importe_arq")),
                rend_importe_ok = Convert.ToDecimal(Obligatorio("rend_importe_ok")), usu_id = Texto("usu_id"),
                usu_apellidoynombre = Texto("usu_apellidoynombre"), rend_estado = Indicador("rend_estado"), rend_entrega = Indicador("rend_entrega")
            });
        }
        return resultado;
    }
}
