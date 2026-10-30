using gc.infraestructura.Core.Helpers;

namespace gc.caja.core.Servicios.Implementacion.Cajas;

// Compatibilidad del diagnóstico existente de Clover con el formateador común de Caja.
public static class CloverJsonDiagnostico
{
    public static string Formatear(string texto, string token = "") => CajaJsonDiagnostico.Formatear(texto, token);
}
