using gc.infraestructura.Dtos.Inventario.Dto;
using gc.infraestructura.Dtos.Inventario.Request;

namespace gc.infraestructura.Dtos.Inventario
{
    public static class InventarioConteoReglas
    {
        public static string? ValidarContexto(InventarioRequestDto r)
        {
            if (string.IsNullOrWhiteSpace(r.inv_nro) || r.inv_nro.Length > 15 ||
                string.IsNullOrWhiteSpace(r.usu_id) || r.usu_id.Length > 10 ||
                r.usu_id.IndexOfAny(new[] { '%', '_', '[', ']' }) >= 0)
                return "Inventario o usuario inválido.";
            if (r.tipo == 'B')
                return r.tipo_id?.Length == 11 && r.tipo_id.All(c => c >= '0' && c <= '9')
                    ? null : "El BOX debe contener 11 dígitos.";
            if (r.tipo == 'P')
                return short.TryParse(r.tipo_id, out var numero) && numero >= 0
                    ? null : "El número de planilla debe estar entre 0 y 32767.";
            return "La modalidad de conteo debe ser BOX o planilla.";
        }

        public static string? ValidarCantidad(InventarioConteoDto p, string upTipo)
        {
            if (string.IsNullOrWhiteSpace(upTipo))
                return "No se recibió el tipo de unidad del producto.";
            if (p.invd_unidad_pres < 1 || p.invd_unidad_pres > short.MaxValue ||
                p.invd_bulto < 0 || p.invd_unidad_suelta < 0 || p.invd_cantidad <= 0 ||
                p.invd_cantidad > 99999999999999.999m || p.invd_unidad_suelta > 99999999999999.999m ||
                decimal.Round(p.invd_unidad_suelta, 3) != p.invd_unidad_suelta ||
                decimal.Round(p.invd_cantidad, 3) != p.invd_cantidad)
                return "Revise bultos, unidad de presentación, US y cantidad (hasta 3 decimales).";
            var pesable = upTipo.Trim().Equals("P", StringComparison.OrdinalIgnoreCase);
            if (pesable && (p.invd_bulto != 0 || p.invd_unidad_pres != 1))
                return "Para un producto pesable ingrese la cantidad en US, sin bultos y con presentación 1.";
            if (!pesable && decimal.Truncate(p.invd_unidad_suelta) != p.invd_unidad_suelta)
                return "Las unidades sueltas de este producto deben ser enteras.";
            var total = pesable ? p.invd_unidad_suelta : (decimal)p.invd_bulto * p.invd_unidad_pres + p.invd_unidad_suelta;
            return total == p.invd_cantidad ? null : "La cantidad no coincide con Bultos × U. presentación + US.";
        }
    }
}
