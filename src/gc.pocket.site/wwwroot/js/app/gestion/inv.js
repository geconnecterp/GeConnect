$(function () {
    cargarInventarios();
});

let inventarioSeleccionado = null;

function cargarInventarios() {
    const $invAuto = $('#invAuto');
    AbrirWaiting("Espere mientras se cargan los datos...");
    $.ajax({
        url: ObtenerInventarioListaUrl,
        type: 'POST',
        cache: false,
        beforeSend: function() {
            $invAuto.html('<div class="text-center py-5"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Cargando...</span></div></div>');
        },
        success: function (response) {
            CerrarWaiting();
            $invAuto.html(response);
            inicializarEventosInventario();
        },
        error: function (xhr) {
            CerrarWaiting();
            const errorMsg = xhr.status === 401 
                ? 'Sesión expirada. Por favor, inicie sesión nuevamente.'
                : 'Error al cargar los inventarios. Intente nuevamente.';
            $invAuto.html(`<div class="alert alert-danger" role="alert"><i class="bx bx-error-circle me-2"></i>${errorMsg}</div>`);
        }
    });
}

function inicializarEventosInventario() {
    const $tbody = $('#tbGridInventarios tbody');
    
    // Usar event delegation para mejor rendimiento
    $tbody.off('click', 'tr[data-inv-nro]').on('click', 'tr[data-inv-nro]', function(e) {
        e.preventDefault();
        marcarInventarioSeleccionado($(this));
    });
}

function marcarInventarioSeleccionado($row) {
    const $tbody = $row.closest('tbody');
    const invNro = $row.data('inv-nro');
    
    // Remover selección previa
    $tbody.find('tr.row-selected').removeClass('row-selected');
    
    // Marcar nueva selección
    $row.addClass('row-selected');
    
    // Guardar inventario seleccionado
    inventarioSeleccionado = {
        inv_nro: invNro,
        $elemento: $row
    };
    
    console.log('Inventario seleccionado:', inventarioSeleccionado.inv_nro);
}

function obtenerInventarioSeleccionado() {
    return inventarioSeleccionado;
}

function limpiarSeleccion() {
    if (inventarioSeleccionado && inventarioSeleccionado.$elemento) {
        inventarioSeleccionado.$elemento.removeClass('row-selected');
    }
    inventarioSeleccionado = null;
}

function invBuscar() {
    cargarInventarios();
}