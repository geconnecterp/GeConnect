$(function () {
    //const orCompte = obtenerOrCompte();
    
    //if (!orCompte) {
    //    console.error("No se encontró el número de comprobante OR");
    //    return;
    //}

    cargarProductosOrCtl(orCompte);
});

//function obtenerOrCompte() {
//    // Buscar el or_compte desde el ViewBag o elemento HTML
//    const elemento = document.querySelector('[data-or-compte]');
//    return elemento ? elemento.getAttribute('data-or-compte') : null;
//}

function cargarProductosOrCtl(orCompte) {
    const url = CargaProductosOrCtlUrl;
    const datos = { or_compte: orCompte };

    $.ajax({
        url: url,
        type: 'POST',
        data: datos,
        beforeSend: function () {
            mostrarCargando();
        },
        success: function (response) {
            ocultarCargando();
            
            if (!response.success) {
                mostrarError(response.message || "Error al cargar productos");
                return;
            }

            if (response.data && response.data.listaEntidad) {
                renderizarProductos(response.data.listaEntidad);
            }
        },
        error: function (xhr, status, error) {
            ocultarCargando();
            mostrarError("Error de conexión al cargar productos");
            console.error("Error:", error);
        }
    });
}

function renderizarProductos(productos) {
    const tbody = $("#tbGridListaControl tbody");
    tbody.empty();

    if (!productos || productos.length === 0) {
        tbody.append('<tr><td colspan="5" class="text-center">No hay productos disponibles</td></tr>');
        return;
    }

    productos.forEach(function (producto) {
        const fila = construirFila(producto);
        tbody.append(fila);
    });
}

function construirFila(producto) {
    const tr = $('<tr></tr>');
    tr.attr('data-pid', producto.p_id);
    
    tr.append(`<td class="text-start">${producto.p_id || ''}</td>`);
    tr.append(`<td class="text-start">${producto.p_desc || ''}</td>`);
    tr.append(`<td class="text-start">${producto.bultos || 0}</td>`);
    tr.append(`<td class="text-start">${formatearDecimal(producto.us)}</td>`);
    tr.append(`<td class="text-start">${formatearDecimal(producto.cantidad)}</td>`);
    
    return tr;
}

function formatearDecimal(valor) {
    if (!valor && valor !== 0) return '0';
    return parseFloat(valor).toFixed(2);
}

function mostrarCargando() {
    const tbody = $("#tbGridListaControl tbody");
    tbody.html('<tr><td colspan="5" class="text-center"><i class="bx bx-loader-alt bx-spin"></i> Cargando...</td></tr>');
}

function ocultarCargando() {
    // Se limpia al renderizar productos
}

function mostrarError(mensaje) {
    const tbody = $("#tbGridListaControl tbody");
    tbody.html(`<tr><td colspan="5" class="text-center text-danger">${mensaje}</td></tr>`);
}