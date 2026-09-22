namespace gc.caja.core.Servicios.Implementacion.Cajas;

public static class TotalesCobranzaCtaCte
{
    // Mantiene la tolerancia del circuito de CC e incluye las NC validadas en el servidor.
    public static bool Coinciden(decimal deuda, decimal valores, decimal notasCredito) =>
        deuda > 0m && valores >= 0m && notasCredito >= 0m &&
        Math.Abs(deuda - valores - notasCredito) <= 0.01m;
}
