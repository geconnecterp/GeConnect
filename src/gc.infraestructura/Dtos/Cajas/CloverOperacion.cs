using gc.infraestructura.Dtos.Gen;

namespace gc.infraestructura.Dtos.Cajas;

public sealed class CloverContextoDto
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

public sealed class CloverPeticionDto
{
    public CloverContextoDto contexto { get; set; } = new();
    public CloverPreparacionDto pago { get; set; } = new();
}

public sealed class CloverResultadoDto
{
    public bool ok { get; set; }
    public bool incierto { get; set; }
    public bool pendiente { get; set; }
    public bool registrado { get; set; }
    public string mensaje { get; set; } = "";
    public string operacion_id { get; set; } = "";
    public Dictionary<string, string> catalogo { get; set; } = [];
    public List<Json_Valor> valores_previos { get; set; } = [];
    public List<gc.infraestructura.Dtos.Cajas.Response.Json_Union> uniones_previas { get; set; } = [];
    public List<CloverAprobadoDto> aprobados { get; set; } = [];
    public List<CloverRespuestaSpDto> datos { get; set; } = [];
    public RespuestaDto? confirmacion { get; set; }
}

public sealed class CloverAprobadoDto
{
    public string intento_id { get; set; } = "";
    public decimal monto_base { get; set; }
    public decimal recargo { get; set; }
    public CloverRespuestaSpDto dato { get; set; } = new();
    public Json_Valor valor { get; set; } = new();
}
