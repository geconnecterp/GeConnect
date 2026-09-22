namespace gc.infraestructura.Dtos.OrdenReparto
{
    // Una única política para el servidor y la grilla. El SP conserva la decisión final.
    public static class ORColeccionReglas
    {
        public static string Codigo(ORProductoDto p) => (p.resultado ?? "PE").Trim().ToUpperInvariant();
        public static bool EsReemplazo(ORProductoDto p) =>
            string.Equals(p.remplazo?.Trim(), "S", StringComparison.OrdinalIgnoreCase) || Codigo(p) == "40";
        public static bool EsPropio(ORProductoDto p, string usuario) => !EsReemplazo(p) ||
            (!string.IsNullOrWhiteSpace(usuario) && string.Equals(p.remplazo_usu_id?.Trim(), usuario.Trim(), StringComparison.OrdinalIgnoreCase));
        public static bool PermiteCarga(ORProductoDto p, string usuario) => p.item.HasValue &&
            (EsReemplazo(p)
                ? EsPropio(p, usuario) && Codigo(p) != "E1" && Codigo(p) != "21"
                : p.colectado_x_p < p.pedido || p.colectado > 0) && (p.pedido > 0 || p.colectado > 0);
        public static bool PermiteReemplazo(ORProductoDto p, string usuario) =>
            PermiteCarga(p, usuario) && !EsReemplazo(p) && p.pedido > 0;
        public static bool PermiteEliminar(ORProductoDto p, string usuario) =>
            p.item.HasValue && p.colectado > 0 && EsPropio(p, usuario);
        public static decimal Diferencia(ORProductoDto p) => p.pedido - p.colectado - p.colectado_remplazo - p.colectado_otros;
        public static string Mensaje(ORProductoDto p) => !string.IsNullOrWhiteSpace(p.resultado_msj)
            ? p.resultado_msj.Trim()
            : Codigo(p) switch
            {
                "PE" => "Pendiente sin colección",
                "E0" => "Colección compartida en este BOX",
                "E1" => "Colección realizada por otros usuarios",
                "00" => "OK Colectado",
                "01" => "OK Colectado x Otros",
                "02" => "OK Colectado con Remp.",
                "03" => "OK Cumple PI",
                "04" => "OK Cumple PI con TR",
                "40" => "Reemplazo",
                "10" => "Colectado Incompleto",
                "11" => "Colectado Incompleto x Otros",
                "20" => "Colectado Demás",
                "21" => "Colectado Demás x Otros",
                _ => "Estado informado por el servidor"
            };
        public static string ClaseEstado(ORProductoDto p) => Codigo(p) switch
        {
            "00" or "01" or "02" or "03" or "04" => "bg-success",
            "10" or "11" or "E0" or "40" => "bg-warning text-dark",
            "20" or "21" or "E1" => "bg-danger",
            _ => "bg-secondary"
        };

        public static bool CoincideColeccion(ORProductoDto p, decimal? cantidad, decimal? bultos, decimal? unidades, int? up) =>
            cantidad == p.colectado && bultos == p.bulto && unidades == p.us && up == p.unidad_pres;

        // Recibe la carga nueva, nunca un total calculado por el navegador.
        public static (decimal Bultos, decimal Unidades, decimal Cantidad) ResolverCarga(
            ORProductoDto p, string modo, string upId, int up, decimal bultos, decimal unidades, decimal cantidad)
        {
            if (modo != "nueva" && modo != "sobrescribir" && modo != "acumular")
                throw new InvalidOperationException("Seleccione cómo registrar la carga.");
            if (cantidad <= 0 || bultos < 0 || unidades < 0 || up < 1 || up > short.MaxValue)
                throw new InvalidOperationException("Verifique las cantidades y la unidad de presentación.");
            if (decimal.Round(cantidad, 3) != cantidad || decimal.Round(unidades, 3) != unidades || decimal.Round(bultos, 1) != bultos)
                throw new InvalidOperationException("Se permiten hasta 3 decimales en cantidades y 1 en bultos.");
            if (upId == "07" && (decimal.Truncate(cantidad) != cantidad || decimal.Truncate(unidades) != unidades))
                throw new InvalidOperationException("El producto por unidades no permite cantidades fraccionarias.");
            if (upId != "07" && (up != 1 || bultos != 0))
                throw new InvalidOperationException("En productos pesables utilice presentación 1, sin bultos.");
            if (cantidad != (upId == "07" ? up * bultos + unidades : unidades))
                throw new InvalidOperationException("La cantidad no coincide con los bultos y unidades ingresados.");
            if (modo == "nueva" && p.colectado > 0)
                throw new InvalidOperationException("Ya existe colección propia. Elija sobrescribir o acumular.");
            if (modo == "acumular")
            {
                if (p.unidad_pres != up)
                    throw new InvalidOperationException("Para acumular conserve la unidad de presentación de la carga previa.");
                if (p.colectado != (upId == "07" ? up * p.bulto + p.us : p.us))
                    throw new InvalidOperationException("La carga previa tiene un desglose inconsistente. Actualice el listado y verifique antes de acumular.");
                return (p.bulto + bultos, p.us + unidades, p.colectado + cantidad);
            }
            return (bultos, unidades, cantidad);
        }
    }
}
