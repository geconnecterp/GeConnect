$(function () {
    definirEventosIniInv();
    cargarInventarios();
});

let inventarioSeleccionado = null;

function definirEventosIniInv() {
    $("#btnContinua01").on("click", ejecutaPaso01);
}

//el paso 01 es determinar que se esta ejecutando.
//determina también si es nuevo o algo ya existente
function ejecutaPaso01() {
    // Validar inventario seleccionado
    if (!inventarioSeleccionado || !inventarioSeleccionado.inv_nro) {
        AbrirMensaje("Atención", "Debe seleccionar un inventario antes de continuar.", 
            () => $("#msjModal").modal("hide"), false, ["Aceptar"], "warn!", null);
        return;
    }

    const datos = {
        inv_nro: inventarioSeleccionado.inv_nro,
        tipo: estado.esBox ? 'B' : 'P',
        tipo_id: null,
        usu_id: ''
    };

    // Procesar según el tipo (Box o Planilla)
    if (estado.esBox) {
        const boxId = $("#txtBuscarBox").val()?.trim();
        if (!boxId) {
            AbrirMensaje("Atención", "Debe ingresar un BOX para continuar.", 
                () => $("#msjModal").modal("hide"), false, ["Aceptar"], "warn!", null);
            return;
        }
        datos.tipo_id = boxId;
    } else {
        // Procesar planilla
        if ($("#rbNuevaPlanilla").is(":checked")) {
            datos.tipo_id = "0";
        } else if ($("#rbModificarPlanilla").is(":checked")) {
            const $planillaSeleccionada = $('input[name="planillaExistente"]:checked');
            if ($planillaSeleccionada.length === 0) {
                AbrirMensaje("Atención", "Debe seleccionar una planilla existente.", 
                    () => $("#msjModal").modal("hide"), false, ["Aceptar"], "warn!", null);
                return;
            }
            const cargaNro = $planillaSeleccionada.data("carga-nro") || $planillaSeleccionada.val();
            if (!cargaNro) {
                AbrirMensaje("Atención", "No se pudo obtener el número de planilla seleccionada.", 
                    () => $("#msjModal").modal("hide"), false, ["Aceptar"], "warn!", null);
                return;
            }
            datos.tipo_id = cargaNro.toString();
        } else {
            AbrirMensaje("Atención", "Debe seleccionar una opción de planilla.", 
                () => $("#msjModal").modal("hide"), false, ["Aceptar"], "warn!", null);
            return;
        }
    }

    console.log("Datos a enviar:", datos);

    // Invocar la acción de validación
    AbrirWaiting("Validando...");
    $.ajax({
        url: estado.inv_valida_conteo,
        type: 'POST',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(datos),
        cache: false,
        success: function (obj) {
            CerrarWaiting();
            if (obj.error === true) {
                AbrirMensaje("Atención", obj.msg, 
                    () => $("#msjModal").modal("hide"), false, ["Aceptar"], "error!", null);
            } else if (obj.warn === true) {
                AbrirMensaje("Atención", obj.msg, function () {
                    if (obj.auth === true) {
                        window.location.href = login;
                    }
                    $("#msjModal").modal("hide");
                }, false, ["Aceptar"], "warn!", null);
            } else {
                ControlaMensajeInfo(obj.msg);
                $.ajax()
            }
        },
        error: function (xhr) {
            CerrarWaiting();
            const errorMsg = xhr.status === 401
                ? 'Sesión expirada. Por favor, inicie sesión nuevamente.'
                : `Error al validar el conteo. ${xhr.responseText || 'Intente nuevamente.'}`;
            
            console.error('Error en validación:', {
                status: xhr.status,
                statusText: xhr.statusText,
                responseText: xhr.responseText
            });
            
            AbrirMensaje("Error", errorMsg, 
                () => $("#msjModal").modal("hide"), false, ["Aceptar"], "error!", null);
        }
    });
}

function cargarInventarios() {
    const $invAuto = $('#invAuto');
    AbrirWaiting("Espere mientras se cargan los datos...");

    $.ajax({
        url: estado.inv_lista,
        type: 'POST',
        dataType: 'html',
        cache: false,
        beforeSend: function () {
            $invAuto.html('<div class="text-center py-5"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Cargando...</span></div></div>');
        },
        success: function (html) {
            CerrarWaiting();
            $invAuto.html(html);
            inicializarEventosInventario();
        },
        error: function (xhr) {
            CerrarWaiting();
            const errorMsg = xhr.status === 401
                ? 'Sesión expirada. Por favor, inicie sesión nuevamente.'
                : 'Error al cargar los inventarios. Intente nuevamente.';
            $invAuto.html(`<div class="alert alert-danger" role="alert"><i class="bx bx-error-circle me-2"></i>${errorMsg}</div>`);
            console.error('Error cargando inventarios:', xhr);
        }
    });
}

function inicializarEventosInventario() {
    const $tbody = $('#tbGridInventarios tbody');

    // Event delegation para mejor rendimiento
    $tbody.off('click', 'tr[data-inv-nro]').on('click', 'tr[data-inv-nro]', function (e) {
        e.preventDefault();
        marcarInventarioSeleccionado($(this));
    });
}

function marcarInventarioSeleccionado($row) {
    const $tbody = $row.closest('tbody');
    const invNro = $row.data('inv-nro');
    const invtId = $row.data('invt-id');

    // Validar que tenemos el invt-id
    if (!invtId) {
        console.error('No se encontró el atributo data-invt-id en la fila seleccionada');
        return;
    }

    // Remover selección previa
    $tbody.find('tr.row-selected').removeClass('row-selected');

    // Marcar nueva selección
    $row.addClass('row-selected');

    // Guardar inventario seleccionado
    inventarioSeleccionado = {
        inv_nro: invNro,
        invt_id: invtId,
        $elemento: $row
    };

    console.log('Inventario seleccionado:', inventarioSeleccionado);

    // Determinar qué tipo de inventario cargar basado en invt_id
    if (invtId === 'B') {
        cargarBoxesInventario(invNro);
    } else {
        cargarPlanillasInventario(invNro);
    }
}

function cargarBoxesInventario(invNro) {
    console.log('Cargando boxes para inventario:', invNro);

    const $contenedorDetalle = $('#invDetalle');
    AbrirWaiting("Cargando boxes...");

    $.ajax({
        url: estado.inv_box,
        type: 'POST',
        dataType: 'html',
        contentType: 'application/json',
        data: JSON.stringify(invNro),
        cache: false,
        success: function (html) {
            CerrarWaiting();
            $contenedorDetalle.html(html);
            //especificamos que es box
            estado.esBox = true;
            $("#btnContinua01").prop("disabled", true);

            //inicializarBusquedaBox();
        },
        error: function (xhr) {
            CerrarWaiting();
            const errorMsg = xhr.status === 401
                ? 'Sesión expirada. Por favor, inicie sesión nuevamente.'
                : 'Error al cargar los boxes. Intente nuevamente.';
            $contenedorDetalle.html(`<div class="alert alert-danger" role="alert"><i class="bx bx-error-circle me-2"></i>${errorMsg}</div>`);
            console.error('Error cargando boxes:', xhr);
        }
    });
}

function cargarPlanillasInventario(invNro) {
    console.log('Cargando planillas para inventario:', invNro);

    const $contenedorDetalle = $('#invDetalle');
    AbrirWaiting("Cargando planillas...");

    $.ajax({
        url: estado.inv_planilla,
        type: 'POST',
        dataType: 'html',
        contentType: 'application/json',
        data: JSON.stringify(invNro),
        cache: false,
        success: function (html) {
            CerrarWaiting();
            $contenedorDetalle.html(html);
            //especificamos que no es box
            estado.esBox = false;
            inicializarOpcionesPlanilla();

        },
        error: function (xhr) {
            CerrarWaiting();
            const errorMsg = xhr.status === 401
                ? 'Sesión expirada. Por favor, inicie sesión nuevamente.'
                : 'Error al cargar las planillas. Intente nuevamente.';
            $contenedorDetalle.html(`<div class="alert alert-danger" role="alert"><i class="bx bx-error-circle me-2"></i>${errorMsg}</div>`);
            console.error('Error cargando planillas:', xhr);
        }
    });
}

function inicializarBusquedaBox() {
    const $inputBuscarBox = $('#txtBuscarBox');

    if ($inputBuscarBox.length === 0) {
        console.log('Campo de búsqueda de box no encontrado');
        return;
    }

    console.log('Inicializando búsqueda de boxes');

    // Búsqueda con debounce para optimizar rendimiento
    let timerBusqueda;
    $inputBuscarBox.off('input').on('input', function () {
        const valorBusqueda = $(this).val().trim().toLowerCase();

        clearTimeout(timerBusqueda);
        timerBusqueda = setTimeout(function () {
            filtrarBoxes(valorBusqueda);
        }, 300);
    });

    // Focus automático en el campo de búsqueda
    $inputBuscarBox.trigger("focus");
}

function filtrarBoxes(valorBusqueda) {
    const $filas = $('#tbGridInventarioBox tbody tr[data-inv-nro]');
    let filasVisibles = 0;

    if (!valorBusqueda) {
        $filas.show();
        console.log('Mostrando todos los boxes');
        return;
    }

    $filas.each(function () {
        const $fila = $(this);
        const textoFila = $fila.text().toLowerCase();

        if (textoFila.indexOf(valorBusqueda) !== -1) {
            $fila.show();
            filasVisibles++;
        } else {
            $fila.hide();
        }
    });

    console.log(`Filtro aplicado: "${valorBusqueda}" - ${filasVisibles} boxes encontrados`);
}

function inicializarOpcionesPlanilla() {
    const $radioButtons = $('input[name="opcionPlanilla"]');

    if ($radioButtons.length === 0) {
        console.log('Radio buttons de planilla no encontrados');
        return;
    }

    console.log('Inicializando opciones de planilla');

    // Evento para cambio de radio button
    $radioButtons.off('change').on('change', function () {
        const opcionSeleccionada = $(this).val();
        manejarOpcionPlanilla(opcionSeleccionada);
    });

    // Verificar si hay alguno seleccionado y ejecutar la lógica correspondiente
    const $radioChecked = $radioButtons.filter(':checked');
    if ($radioChecked.length > 0) {
        manejarOpcionPlanilla($radioChecked.val());
    }

    $("#btnContinua01").prop("disabled", false);
}

function manejarOpcionPlanilla(opcion) {
    console.log('Opción planilla seleccionada:', opcion);

    switch (opcion) {
        case 'nueva':
            // Lógica para crear nueva planilla
            console.log('Preparando para crear nueva planilla');
            // TODO: Implementar lógica para nueva planilla
            $("#btnContinua01").prop("disabled", false);



            break;
        case 'modificar':
            // Lógica para modificar planilla existente
            console.log('Preparando para modificar planilla');
            // TODO: Implementar lógica para modificar planilla
            $("#btnContinua01").prop("disabled", true);


            break;
        default:
            console.warn('Opción no reconocida:', opcion);
    }
}

function obtenerInventarioSeleccionado() {
    return inventarioSeleccionado;
}

function limpiarSeleccion() {
    if (inventarioSeleccionado && inventarioSeleccionado.$elemento) {
        inventarioSeleccionado.$elemento.removeClass('row-selected');
    }
    inventarioSeleccionado = null;

    // Limpiar el contenedor de detalle
    $('#invDetalle').empty();

    console.log('Selección limpiada');
}

function invBuscar() {
    limpiarSeleccion();
    cargarInventarios();
}