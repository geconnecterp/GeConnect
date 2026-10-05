using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace gc.caja.Models.Estacion;

public sealed class EstacionPuestoServicio
{
    public const string ClaveSesion = "EstacionPuesto";
    private const string Cookie = ".gccaja.puesto";
    private readonly ConcurrentDictionary<string, (EstacionPuesto Puesto, DateTimeOffset Vence)> pases = new();
    private readonly ITimeLimitedDataProtector protector;
    private readonly TimeProvider reloj;
    public EstacionPuestoServicio(IDataProtectionProvider provider, TimeProvider? reloj = null)
    {
        protector = provider.CreateProtector("gc.caja.puesto.v1").ToTimeLimitedDataProtector();
        this.reloj = reloj ?? TimeProvider.System;
    }

    public string Preparar(EstacionPuesto puesto)
    {
        foreach (var entrada in pases.Where(x => x.Value.Vence < reloj.GetUtcNow()))
            pases.TryRemove(entrada.Key, out _);
        if (pases.Count >= 1000) throw new InvalidOperationException("Intente iniciar Caja nuevamente en unos minutos.");
        var codigo = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        pases[codigo] = (puesto, reloj.GetUtcNow().AddMinutes(2));
        return codigo;
    }

    public EstacionPuesto? Consumir(string? pase) =>
        pase is { Length: 64 } && pases.TryRemove(pase, out var entrada) && entrada.Vence >= reloj.GetUtcNow()
            ? entrada.Puesto : null;

    public void Guardar(HttpContext context, EstacionPuesto puesto)
    {
        var json = JsonSerializer.Serialize(puesto);
        context.Session.SetString(ClaveSesion, json);
        context.Response.Cookies.Append(Cookie, protector.Protect(json, TimeSpan.FromDays(30)), new CookieOptions {
            HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax, IsEssential = true,
            Path = context.Request.PathBase.HasValue ? context.Request.PathBase.Value : "/",
            MaxAge = TimeSpan.FromDays(30)
        });
        context.Session.SetString("CajaActual", Newtonsoft.Json.JsonConvert.SerializeObject(puesto.Configuracion()));
    }

    public EstacionPuesto? Obtener(HttpContext context)
    {
        var json = context.Session.GetString(ClaveSesion);
        if (json == null && context.Request.Cookies.TryGetValue(Cookie, out var valor))
        {
            try { json = protector.Unprotect(valor); }
            catch (CryptographicException) { return null; }
        }
        if (json == null) return null;
        try { return JsonSerializer.Deserialize<EstacionPuesto>(json); }
        catch (JsonException) { return null; }
    }
}
