namespace gc.caja.Models;

public static class SesionOperacionFactura
{
    public const string ClaveOperacion = "Facturacion:OperacionId";
    public const string ClaveAutorizacionLista = "AutorizacionRemota:Vigente:FACTURACION_CAMBIO_LP";

    public static string Reiniciar(Action<string> eliminar, Action<string, string> guardar)
    {
        foreach (var clave in new[] { "ProductosSeleccionados", "FacturaProductos", "FacturaSubtotales", "FacturaSorteos", ClaveAutorizacionLista })
            eliminar(clave);
        var operacion = Guid.NewGuid().ToString("N");
        guardar(ClaveOperacion, operacion);
        return operacion;
    }
}
