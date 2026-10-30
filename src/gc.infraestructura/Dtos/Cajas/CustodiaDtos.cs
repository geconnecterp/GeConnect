using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace gc.infraestructura.Dtos.Cajas;

public class CustodiaInstrumentoDto
{
    public string ins_id { get; set; } = "";
    public string ins_desc { get; set; } = "";
}

public class CustodiaRendicionDto
{
    public string adm_id { get; set; } = "";
    public string adm_nombre { get; set; } = "";
    public string ins_id { get; set; } = "";
    public string caja_nro_proceso { get; set; } = "";
    public short caja_nro_cierre { get; set; }
    public short caja_nro_rend { get; set; }
    public short rend_item { get; set; }
    public string caja_id { get; set; } = "";
    public string? rend_tipo { get; set; }
    public string rend_tipo_desc { get; set; } = "";
    public DateTime rend_fecha { get; set; }
    public decimal rend_importe_arq { get; set; }
    public decimal rend_importe_ok { get; set; }
    public string usu_id { get; set; } = "";
    public string usu_apellidoynombre { get; set; } = "";
    public string? rend_estado { get; set; }
    public string? rend_entrega { get; set; }
    public string clave => Huella(JsonConvert.SerializeObject(new object?[] { adm_id.Trim(), ins_id.Trim(), caja_nro_proceso.Trim(), caja_nro_cierre, caja_nro_rend, rend_item, caja_id.Trim() }));
    public string version => Huella(JsonConvert.SerializeObject(new object?[] { clave, rend_fecha.ToString("O", CultureInfo.InvariantCulture), importe_rendido, importe_corregido, rend_tipo, rend_estado, rend_entrega }));
    // Se exponen cadenas decimales para que el navegador no pierda centavos por IEEE-754.
    public string importe_rendido => rend_importe_arq.ToString("0.00", CultureInfo.InvariantCulture);
    public string importe_corregido => rend_importe_ok.ToString("0.00", CultureInfo.InvariantCulture);
    private static string Huella(string valor) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(valor)));
}

public record CustodiaConsultaDto(List<CustodiaInstrumentoDto> instrumentos, List<CustodiaRendicionDto> rendiciones, bool entrega_habilitada = false);

public class CustodiaSeleccionDto
{
    public string clave { get; set; } = "";
    public string version { get; set; } = "";
    public string ins_id { get; set; } = "";
    public string importe { get; set; } = "";
    public decimal ObtenerImporte()
    {
        if (!Regex.IsMatch(importe ?? "", @"^\d{1,15}(?:\.\d{1,2})?$", RegexOptions.CultureInvariant) ||
            !decimal.TryParse(importe, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var valor) || valor < 0 || valor > 999999999999999.99m)
            throw new ArgumentException("Ingrese un importe corregido de cero o mayor, con hasta dos decimales.");
        return valor;
    }
}

public class CustodiaOperacionDto
{
    public Guid solicitud { get; set; }
    public List<CustodiaSeleccionDto> seleccion { get; set; } = [];
    public string responsable_entrega { get; set; } = "";
    public string responsable_custodia { get; set; } = "";
    public string comprobante_entrega { get; set; } = "";
}

public record CustodiaResultadoDto(bool ok, string mensaje, string entrega_id = "", bool incierto = false);

// Contrato documentado: exactamente estos 14 campos, reconstruidos desde los registros del servidor.
public class CustodiaLineaDto
{
    public string ins_id { get; set; } = "";
    public string tcf_id { get; set; } = "EF";
    public string caja_nro_proceso { get; set; } = "";
    public short caja_nro_cierre { get; set; }
    public short caja_nro_rend { get; set; }
    public short rend_item { get; set; }
    public string adm_id { get; set; } = "";
    public string caja_id { get; set; } = "";
    public string? rend_tipo { get; set; }
    public DateTime rend_fecha { get; set; }
    public decimal rend_importe_arq { get; set; }
    public decimal rend_importe_ok { get; set; }
    public string? rend_estado { get; set; }
    public string? rend_entrega { get; set; }
    public static CustodiaLineaDto Crear(CustodiaRendicionDto fila, decimal importe) => new()
    {
        ins_id = fila.ins_id.Trim(), caja_nro_proceso = fila.caja_nro_proceso.Trim(),
        caja_nro_cierre = fila.caja_nro_cierre, caja_nro_rend = fila.caja_nro_rend, rend_item = fila.rend_item,
        adm_id = fila.adm_id.Trim(), caja_id = fila.caja_id.Trim(), rend_tipo = fila.rend_tipo,
        rend_fecha = fila.rend_fecha, rend_importe_arq = fila.rend_importe_arq,
        rend_importe_ok = importe, rend_estado = fila.rend_estado, rend_entrega = fila.rend_entrega
    };
}

public class CustodiaAcuseFilaDto
{
    public string ent_compte { get; set; } = "";
    public string adm_id { get; set; } = "";
    public string adm_nombre { get; set; } = "";
    public DateTime ent_fecha { get; set; }
    public string ent_resp_entrega { get; set; } = "";
    public string ent_resp_recibe { get; set; } = "";
    public string ent_compte_retiro { get; set; } = "";
    public string caja_nro_proceso { get; set; } = "";
    public short caja_nro_cierre { get; set; }
    public short caja_nro_rend { get; set; }
    public short rend_item { get; set; }
    public string caja_id { get; set; } = "";
    public string rend_tipo { get; set; } = "";
    public DateTime rend_fecha { get; set; }
    public string ins_id { get; set; } = "";
    public decimal rend_importe_arq { get; set; }
    public decimal rend_importe_ok { get; set; }
}
