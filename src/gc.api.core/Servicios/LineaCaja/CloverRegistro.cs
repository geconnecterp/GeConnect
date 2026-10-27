using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Gen;

namespace gc.api.core.Servicios.LineaCaja;

// Diario del servidor API, no del puesto. Escritura durable antes de cada efecto externo.
// FileShare.None coordina también dos procesos que usen el MISMO directorio de estado.
public sealed class CloverRegistro
{
    public string Directorio { get; }
    public CloverRegistro(string directorio) { Directorio = Path.GetFullPath(directorio); Directory.CreateDirectory(Directorio); }
    public static string Hash(string texto) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(texto)));
    public string Puesto(CloverContextoDto c) => Hash(c.adm_id + "|" + c.caja_id);
    private string Ruta(string key) => Path.Combine(Directorio, key + ".json");
    public IDisposable Bloquear(CloverContextoDto c)
    {
        try { return new FileStream(Path.Combine(Directorio, Puesto(c) + ".lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
        catch (IOException) { throw new InvalidOperationException("Este puesto tiene una operación Clover en curso. Espere y consulte su estado; no repita el cobro."); }
    }
    public CloverDiario? LeerActivo(CloverContextoDto c) => Leer(Puesto(c));
    public CloverDiario? LeerOperacion(string id) => Guid.TryParse(id, out _) ? Leer("op-" + id) : null;
    private CloverDiario? Leer(string key) => File.Exists(Ruta(key)) ? JsonConvert.DeserializeObject<CloverDiario>(File.ReadAllText(Ruta(key))) ?? throw new IOException("El diario Clover no es válido. No repita el cobro.") : null;
    public void Guardar(CloverDiario diario)
    {
        diario.actualizado_utc = DateTime.UtcNow;
        var json = JsonConvert.SerializeObject(diario);
        Escribir("op-" + diario.contexto.operacion_id, json);
        Escribir(Puesto(diario.contexto), json);
    }
    public void ReservarId(string paymentId, string operacion)
    {
        var ruta = Path.Combine(Directorio, "id-" + Hash(paymentId) + ".txt");
        try { using var stream = new FileStream(ruta, FileMode.CreateNew, FileAccess.Write, FileShare.None); using var w = new StreamWriter(stream); w.Write(operacion); w.Flush(); stream.Flush(true); }
        catch (IOException) { if (!File.Exists(ruta) || File.ReadAllText(ruta) != operacion) throw new InvalidOperationException("El ID Clover ya pertenece a otra operación. No puede imputarse nuevamente."); }
    }
    private void Escribir(string key, string json)
    {
        var temporal = Path.Combine(Directorio, key + "." + Guid.NewGuid() + ".tmp");
        try {
            using (var f = new FileStream(temporal, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
            { using var w = new StreamWriter(f, Encoding.UTF8, 4096, true); w.Write(json); w.Flush(); f.Flush(true); }
            File.Move(temporal, Ruta(key), true);
        } finally { if (File.Exists(temporal)) File.Delete(temporal); }
    }
}

public sealed class CloverDiario
{
    public CloverContextoDto contexto { get; set; } = new();
    public List<CloverIntento> intentos { get; set; } = [];
    public string estado_registro { get; set; } = "PENDIENTE";
    public string huella_confirmacion { get; set; } = "";
    public RespuestaDto? confirmacion { get; set; }
    public DateTime actualizado_utc { get; set; }
}
public sealed class CloverIntento
{
    public CloverPreparacionDto solicitud { get; set; } = new();
    public string estado { get; set; } = "ESPERANDO";
    public string mensaje { get; set; } = "";
    public List<CloverRespuestaSpDto> respuesta { get; set; } = [];
    public CloverAprobadoDto? aprobado { get; set; }
}
