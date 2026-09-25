using System.Globalization;
using System.Security.Claims;
using gc.infraestructura.Dtos.Seguridad;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace gc.caja.Models.Cuenta;

public class CuentaViewModel
{
    public string ClaveActual { get; set; } = "";
    public string ClaveNueva { get; set; } = "";
    public string ConfirmacionClave { get; set; } = "";
    [BindNever] public PoliticaClaveDto? Politica { get; set; }
    [BindNever] public bool Obligatoria { get; set; }
    [BindNever] public bool Vencida { get; set; }
    [BindNever] public string? Error { get; set; }
    [BindNever] public OperadorCuenta Operador { get; set; } = new();
}

public class OperadorCuenta
{
    public string Nombre { get; init; } = "Operador";
    public string Usuario { get; init; } = "";
    public string Email { get; init; } = "";
    public string Sucursal { get; init; } = "No informada";
    public string Perfil { get; set; } = "No informado";
    public string Iniciales => string.Concat(Nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Take(2).Select(p => StringInfo.GetNextTextElement(p))).ToUpperInvariant();

    public static OperadorCuenta Desde(ClaimsPrincipal user)
    {
        string Valor(params string[] nombres) => nombres.Select(n => user.FindFirst(n)?.Value)
            .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim() ?? "";
        var usuario = Valor("user", ClaimTypes.Name, "unique_name", "name");
        var nombre = Valor("nya");
        var adm = Valor("AdmId").Split('#', 2, StringSplitOptions.TrimEntries);
        return new OperadorCuenta {
            Usuario = usuario,
            Nombre = string.IsNullOrWhiteSpace(nombre) ? (usuario.Length > 0 ? usuario : "Operador") : nombre,
            Email = Valor(ClaimTypes.Email, "email"),
            Sucursal = adm.Length > 1 && adm[1].Length > 0 ? adm[1] : adm[0].Length > 0 ? adm[0] : "No informada"
        };
    }
}
