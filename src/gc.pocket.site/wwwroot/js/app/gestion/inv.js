$(function () {
    definirEventosIniInv();
    cargarInventarios();
});

//variable "inventarioSeleccionado" declarada en siteGen

function definirEventosIniInv() {
    $("#btnContinua01").on("click", ejecutaPaso01);

    // Validación estricta: activar solo con 11 caracteres exactos
    $(document).on("input", "#txtBuscarBox", function () {
        const valor = this.value.trim();
        const esValido = valor.length === 11;
        $("#btnContinua01").prop("disabled", !esValido);
    });
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
            // ✅ CORRECCIÓN: Selector correcto para el nombre del radio button
            const $planillaSeleccionada = $('input[name="planillaSeleccionada"]:checked');
            
            if ($planillaSeleccionada.length === 0) {
                AbrirMensaje("Atención", "Debe seleccionar una planilla de la tabla.", 
                    () => $("#msjModal").modal("hide"), false, ["Aceptar"], "warn!", null);
                return;
            }
            
            // ✅ CORRECCIÓN: Obtener carga_nro del atributo data del TR
            const $fila = $planillaSeleccionada.closest('tr');
            const cargaNro = $fila.data('carga-nro');
            
            if (!cargaNro) {
                AbrirMensaje("Atención", "No se pudo obtener el número de planilla seleccionada.", 
                    () => $("#msjModal").modal("hide"), false, ["Aceptar"], "warn!", null);
                return;
            }
            
            datos.tipo_id = cargaNro.toString();
            console.log('Planilla seleccionada - carga_nro:', datos.tipo_id);
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
        url: inv_valida_conteo,
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
                estado.inv_nro = datos.inv_nro;
                estado.tipo = datos.tipo;
                estado.tipo_id = datos.tipo_id;
                window.location.href = inv_conteo + `?invNro=${datos.inv_nro}&tipo=${datos.tipo}&tipoId=${datos.tipo_id}` ;
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
        url: inv_lista,
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
    estado.inv_nro = invNro;
    estado.invt_id = invtId;

    console.log('Inventario seleccionado:', inventarioSeleccionado);

    // Determinar qué tipo de inventario cargar basado en invt_id
    if (invtId === 'B') {
        estado.tipo = 'B';
        cargarBoxesInventario(invNro);
    } else {
        estado.tipo = 'P';
        cargarPlanillasInventario(invNro);
    }
}

function cargarBoxesInventario(invNro) {
    console.log('Cargando boxes para inventario:', invNro);

    const $contenedorDetalle = $('#invDetalle');
    AbrirWaiting("Cargando boxes...");

    $.ajax({
        url: inv_box,
        type: 'POST',
        dataType: 'html',
        contentType: 'application/json',
        data: JSON.stringify(invNro),
        cache: false,
        success: function (html) {
            CerrarWaiting();
            $contenedorDetalle.html(html);
            estado.esBox = true;
            $("#btnContinua01").prop("disabled", true);
            
            // ✅ Inicializar eventos de selección de box
            inicializarSeleccionBox();
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

// ✅ Nueva función optimizada para manejar la selección de boxes
function inicializarSeleccionBox() {
    const $tbody = $('#tbGridInventarioBox tbody');
    
    if ($tbody.length === 0) {
        console.log('Tabla de boxes no encontrada');
        return;
    }

    // Event delegation optimizada - un solo listener para todos los radio buttons
    $tbody.off('change', 'input[name="boxSeleccionado"]').on('change', 'input[name="boxSeleccionado"]', function() {
        const $radio = $(this);
        const $fila = $radio.closest('tr');
        const boxId = $fila.data('box-id');
        const invNro = $fila.data('inv-nro');
        
        if (boxId) {
            // Asignar el box_id al input de búsqueda
            $('#txtBuscarBox').val(boxId);
            
            // Actualizar estado global
            estado.tipo_id = boxId.toString();
            estado.box_id = boxId;
            estado.inv_nro = invNro;
            
            // Marcar visualmente la fila seleccionada
            $tbody.find('tr.row-selected').removeClass('row-selected');
            $fila.addClass('row-selected');
            
            // Habilitar botón continuar (validación de 11 caracteres se ejecuta automáticamente)
            const esValido = boxId.toString().length === 11;
            $("#btnContinua01").prop("disabled", !esValido);
            
            console.log('Box seleccionado:', { box_id: boxId, inv_nro: invNro });
        }
    });

    // Click en la fila también selecciona el radio button
    $tbody.off('click', 'tr[data-box-id]').on('click', 'tr[data-box-id]', function(e) {
        // Evitar doble trigger si se clickea directamente en el radio button
        if (!$(e.target).is('input[type="radio"]')) {
            $(this).find('input[name="boxSeleccionado"]').prop('checked', true).trigger('change');
        }
    });
    
    console.log('Eventos de selección de box inicializados');
}

function cargarPlanillasInventario(invNro) {
    console.log('Cargando planillas para inventario:', invNro);

    const $contenedorDetalle = $('#invDetalle');
    AbrirWaiting("Cargando planillas...");

    $.ajax({
        url: inv_planilla,
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

    // ✅ Desactivar todos los radio buttons del grid al inicio
    const $radiosPlanillas = $('#tbGridInventarioPlanilla input[name="planillaSeleccionada"]');
    $radiosPlanillas.prop('disabled', true).prop('checked', false);

    // Evento para cambio de radio button de opciones
    $radioButtons.off('change').on('change', function () {
        const opcionSeleccionada = $(this).val();
        manejarOpcionPlanilla(opcionSeleccionada);
    });

    // ✅ CORRECCIÓN: Event delegation optimizada para cambios en radio buttons
    const $tbody = $('#tbGridInventarioPlanilla tbody');
    
    // Remover TODOS los event listeners previos para evitar duplicación
    $tbody.off('click', 'tr[data-carga-nro]');
    $tbody.off('change', 'input[name="planillaSeleccionada"]');
    $(document).off('change', '#tbGridInventarioPlanilla input[name="planillaSeleccionada"]');

    // ✅ Event listener ÚNICO para radio buttons (delegado en tbody)
    $tbody.on('change', 'input[name="planillaSeleccionada"]', function(e) {
        // Solo procesar si el radio no está deshabilitado
        if (!$(this).prop('disabled')) {
            e.stopPropagation(); // Evitar propagación
            marcarPlanillaSeleccionada($(this).closest('tr'));
        }
    });

    // ✅ Click en la fila (excluye el radio button para evitar doble trigger)
    $tbody.on('click', 'tr[data-carga-nro]', function(e) {
        // Solo permitir click si está en modo modificar
        if ($('#rbModificarPlanilla').is(':checked')) {
            // ✅ CRÍTICO: No prevenir default si se clickea en el radio button
            const $target = $(e.target);
            
            // Si el click es directamente en el radio button, dejarlo manejar naturalmente
            if ($target.is('input[type="radio"]')) {
                return; // Dejar que el evento change del radio button lo maneje
            }
            
            // Si el click es en cualquier otra parte de la fila
            const $radio = $(this).find('input[name="planillaSeleccionada"]');
            if ($radio.length > 0 && !$radio.prop('disabled')) {
                $radio.prop('checked', true).trigger('change');
            }
        }
    });

    // ✅ Establecer estado inicial: Nueva planilla (deshabilitado)
    $("#btnContinua01").prop("disabled", false);
}

// ✅ CORRECCIÓN: Función optimizada - marcarPlanillaSeleccionada
function marcarPlanillaSeleccionada($row) {
    const $tbody = $row.closest('tbody');
    const cargaNro = $row.data('carga-nro');
    const invNro = $row.data('inv-nro');

    if (!cargaNro) {
        console.error('No se encontró el atributo data-carga-nro en la fila seleccionada');
        return;
    }

    // Remover selección previa visual
    $tbody.find('tr.row-selected').removeClass('row-selected');

    // Marcar nueva selección visual
    $row.addClass('row-selected');

    // ✅ IMPORTANTE: No manipular el estado checked del radio button aquí
    // porque ya viene marcado por el evento change natural del navegador

    // Actualizar estado global
    estado.tipo_id = cargaNro.toString();
    estado.carga_nro = cargaNro;
    estado.inv_nro = invNro;

    console.log('Planilla seleccionada:', {
        carga_nro: cargaNro,
        inv_nro: invNro
    });

    // Si está en modo modificar, habilitar botón
    if ($('#rbModificarPlanilla').is(':checked')) {
        $("#btnContinua01").prop("disabled", false);
    }
}

function manejarOpcionPlanilla(opcion) {
    console.log('Opción planilla seleccionada:', opcion);

    const $gridPlanillas = $('#tbGridInventarioPlanilla');
    const $radiosPlanillas = $gridPlanillas.find('input[name="planillaSeleccionada"]');
    const $planillaSeleccionada = $radiosPlanillas.filter(':checked');

    switch (opcion) {
        case 'nueva':
            console.log('Preparando para crear nueva planilla');
            
            // ✅ Desactivar todos los radio buttons del grid
            $radiosPlanillas.prop('disabled', true).prop('checked', false);
            
            // Limpiar selección visual del grid
            $gridPlanillas.find('tr.row-selected').removeClass('row-selected');
            
            // Actualizar estado
            estado.tipo_id = "0";
            estado.carga_nro = null;
            
            $("#btnContinua01").prop("disabled", false);
            break;

        case 'modificar':
            console.log('Preparando para modificar planilla');
            
            // ✅ Activar todos los radio buttons del grid
            $radiosPlanillas.prop('disabled', false);
            
            if ($planillaSeleccionada.length > 0) {
                const $row = $planillaSeleccionada.closest('tr');
                const cargaNro = $row.data('carga-nro');
                
                estado.tipo_id = cargaNro ? cargaNro.toString() : null;
                estado.carga_nro = cargaNro;
                
                // Marcar visualmente la fila
                $gridPlanillas.find('tr.row-selected').removeClass('row-selected');
                $row.addClass('row-selected');
                
                console.log('Planilla seleccionada - carga_nro:', estado.tipo_id);
                $("#btnContinua01").prop("disabled", false);
            } else {
                // Si no hay planilla seleccionada, deshabilitar botón
                estado.tipo_id = null;
                estado.carga_nro = null;
                $("#btnContinua01").prop("disabled", true);
            }
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