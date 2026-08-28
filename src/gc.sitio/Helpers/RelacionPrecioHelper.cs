namespace gc.sitio.Helpers
{
    public static class RelacionPrecioHelper
    {
        public static decimal Calcular(decimal precioNuevo, decimal precioVigente)
        {
            if (precioVigente <= 0)
            {
                return 0M;
            }

            return Math.Truncate((precioNuevo / precioVigente) * 100M) / 100M;
        }

        public static string ObtenerEstilo(decimal precioNuevo, decimal precioVigente)
        {
            if (precioVigente <= 0 || precioNuevo == precioVigente)
            {
                return "color: black; font-weight: normal;";
            }

            return precioNuevo > precioVigente
                ? "color: blue; font-weight: bold;"
                : "color: red; font-weight: bold;";
        }
    }
}
