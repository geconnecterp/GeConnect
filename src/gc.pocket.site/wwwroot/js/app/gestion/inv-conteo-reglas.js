/* Reglas de cantidades de Inventario: up_tipo es la autoridad, no up_id. */
const InventarioCantidades = Object.freeze({
    pesable: tipo => String(tipo || "").trim().toUpperCase() === "P",
    calcular(tipo, bultos, presentacion, sueltas) {
        const p = this.pesable(tipo);
        if (!String(tipo || "").trim()) throw new Error("No se recibió el tipo de unidad del producto.");
        if (![bultos, presentacion, sueltas].every(Number.isFinite) ||
            !Number.isInteger(bultos) || bultos < 0 || bultos > 2147483647 ||
            !Number.isInteger(presentacion) || presentacion < 1 || presentacion > 32767 ||
            sueltas < 0 || Math.abs(sueltas * 1000 - Math.round(sueltas * 1000)) > 0.000001 ||
            (!p && !Number.isInteger(sueltas)))
            throw new Error("Revise Bultos, U. presentación y US. Solo los productos pesables permiten hasta 3 decimales.");
        if (p && (bultos !== 0 || presentacion !== 1))
            throw new Error("Para un producto pesable use US, sin bultos y con presentación 1.");
        const cantidad = Number((p ? sueltas : bultos * presentacion + sueltas).toFixed(3));
        // Precisión segura en el navegador al operar en milésimas.
        if (cantidad <= 0 || !Number.isSafeInteger(Math.round(cantidad * 1000)))
            throw new Error("Ingrese una cantidad positiva dentro del rango permitido.");
        return { invd_bulto: bultos, invd_unidad_pres: presentacion, invd_unidad_suelta: sueltas, invd_cantidad: cantidad };
    },
    acumular(tipo, anterior, nueva) {
        const total = Number((anterior.invd_cantidad + nueva.invd_cantidad).toFixed(3));
        if (this.pesable(tipo)) return this.calcular(tipo, 0, 1, total);
        if (anterior.invd_unidad_pres === nueva.invd_unidad_pres)
            return this.calcular(tipo, anterior.invd_bulto + nueva.invd_bulto,
                nueva.invd_unidad_pres, anterior.invd_unidad_suelta + nueva.invd_unidad_suelta);
        // Presentaciones diferentes: conservar el total en la presentación nueva.
        const bultos = Math.floor(total / nueva.invd_unidad_pres);
        return this.calcular(tipo, bultos, nueva.invd_unidad_pres, total - bultos * nueva.invd_unidad_pres);
    }
});
if (typeof module !== "undefined") module.exports = InventarioCantidades;
