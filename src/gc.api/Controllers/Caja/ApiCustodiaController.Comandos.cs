using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Cajas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace gc.api.Controllers.Caja;

public partial class ApiCustodiaController
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Sucursales = new();
    private record OperacionRegistrada(string Huella, CustodiaResultadoDto Resultado);

    [HttpPost("corregir")]
    public IActionResult Corregir(CustodiaOperacionDto operacion) => Ejecutar(operacion, false);

    [HttpPost("entregar")]
    public IActionResult Entregar(CustodiaOperacionDto operacion) => Ejecutar(operacion, true);

    private IActionResult Ejecutar(CustodiaOperacionDto operacion, bool entrega)
    {
        (string Sucursal, string Usuario) c;
        try { c = Contexto(); } catch (UnauthorizedAccessException) { return Unauthorized(new CustodiaResultadoDto(false, "La sesión no identifica usuario y sucursal.")); }
        if (operacion == null || operacion.solicitud == Guid.Empty) return BadRequest(new CustodiaResultadoDto(false, "La solicitud no es válida."));
        var clave = $"custodia:{c.Sucursal}:{c.Usuario}:{entrega}:{operacion.solicitud}";
        var huella = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(operacion))));
        var gate = Sucursales.GetOrAdd(c.Sucursal, _ => new SemaphoreSlim(1, 1));
        if (!gate.Wait(0)) return Conflict(new CustodiaResultadoDto(false, "Hay una operación de custodia en curso en la sucursal. Espere su resultado."));
        try
        {
            if (cache.TryGetValue<OperacionRegistrada>(clave, out var anterior))
                return anterior!.Huella == huella ? Ok(anterior.Resultado) : Conflict(new CustodiaResultadoDto(false, "Esta referencia ya se utilizó con otros datos. Actualice y revise la selección."));
            // Cada comando deriva usuario y sucursal del token. No se reciben desde la pantalla.
            var resultado = entrega ? EntregarSeleccion(operacion, c.Sucursal, c.Usuario) : servicio.Corregir(operacion, c.Sucursal, c.Usuario);
            if (resultado.ok || resultado.incierto) cache.Set(clave, new OperacionRegistrada(huella, resultado), TimeSpan.FromHours(24));
            return Ok(resultado);
        }
        catch (ArgumentException ex) { return BadRequest(new CustodiaResultadoDto(false, ex.Message)); }
        catch (CustodiaPersistenciaException ex)
        {
            logger.LogError(ex, "[CUSTODIA] Error de persistencia. Solicitud={Solicitud}, Sucursal={Sucursal}, Incierto={Incierto}", operacion.solicitud, c.Sucursal, ex.Incierto);
            var resultado = new CustodiaResultadoDto(false, ex.Incierto ? "No se pudo confirmar el resultado. Verifique las rendiciones antes de repetir. Referencia: " + operacion.solicitud : "No se guardaron cambios. Revise los datos o actualice la consulta.", incierto: ex.Incierto);
            if (ex.Incierto) cache.Set(clave, new OperacionRegistrada(huella, resultado), TimeSpan.FromHours(24));
            return StatusCode(503, resultado);
        }
        catch (Exception ex) { logger.LogError(ex, "[CUSTODIA] Preparación fallida. Solicitud={Solicitud}", operacion.solicitud); return StatusCode(503, new CustodiaResultadoDto(false, "No se pudo preparar la operación. Actualice y vuelva a intentarlo.")); }
        finally { gate.Release(); }
    }

    // No se invoca un contrato de escritura supuesto: se habilita al recibir la firma del SP de entregas.
    private CustodiaResultadoDto EntregarSeleccion(CustodiaOperacionDto operacion, string sucursal, string usuario) =>
        new(false, "La confirmación de entregas todavía no está disponible. Puede consultar rendiciones y guardar correcciones.");
}
