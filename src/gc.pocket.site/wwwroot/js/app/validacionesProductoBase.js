// Nuevo: validacionProducto.js (código base compartido)
class ValidacionProductoBase {
    calcularCantidad(up, bulto, unid, upId) {
        return upId === "07" ? (up * bulto) + unid : unid;
    }

    validarCantidad(cantidad) {
        return cantidad > 0;
    }

    // ... métodos comunes
}

// tivalidaProducto.js (específico TI)
class ValidacionProductoTI extends ValidacionProductoBase {
    validarContraAutorizacion(cantidad) {
        return cantidad <= this.autorizacionActual.pPedido;
    }
}

// orvalidaProducto.js (específico OR)
class ValidacionProductoOR extends ValidacionProductoBase {
    async validarContraStock(pId, cantidad) {
        const stock = await this.obtenerStockDisponible(pId);
        return cantidad <= stock;
    }
}