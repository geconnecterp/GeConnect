// ========================================================
// GESTOR PRINCIPAL DEL MÓDULO DE COBRANZA CUENTA CORRIENTE (CC)
// ✅ v1.0 - FLUJO UNICO: BUSCAR SOLO CLIENTES REGISTRADOS EN LA BASE DE DATOS (CR)
// ========================================================
let clienteSeleccionadoCC = null;
let nombreClienteCC = '';
let $filaCuentaCorrienteEnEdicion = null;
let $filaImputaCCEnEdicion = null;


$(function () {
    console.log('═══════════════════════════════════════════════════');
    console.log('🚀 MÓDULO DE COBRANZA CUENTA CORRIENTE v1.0 CARGADO');
    console.log('   MODO: Flujo único (Buscar solo clientes registrados en la base de datos)');
    console.log('═══════════════════════════════════════════════════');

    // El modal de identificación se abrirá primero
    inicializarModuloConModal();

    $(document).on('clienteConfirmado', function (event, cliente) {
        console.log('═══════════════════════════════════════════════════');
        console.log('✅ CLIENTE CONFIRMADO EN COBRANZA CC v1.0');
        console.log('   Cliente:', cliente.denominacion);
        console.log('   ID:', cliente.id);
        console.log('═══════════════════════════════════════════════════');

        // Ocultar el modal de identificación
        $('#modalIdentificarCliente').modal('hide');

        // Buscar facturas del cliente específico (FILTRADO DESDE SESIÓN)
        obtenerDatosCCDesdeMemoria(cliente);
    });

    // ========================================================
    // EVENTOS DE SELECCIÓN - CUENTA CORRIENTE PENDIENTE
    // Namespace: ccPendientes
    // ========================================================

    // Checkbox general: seleccionar / deseleccionar todos los movimientos
    $(document)
        .off('change.ccPendientes', '#chkSeleccionarTodoCC')
        .on('change.ccPendientes', '#chkSeleccionarTodoCC', function () {
            seleccionarTodosMovimientosCC($(this).is(':checked'));
        });

    // Checkboxes individuales generados dinámicamente dentro de la grilla
    $(document)
        .off(
            'change.ccPendientes',
            '#tbodyCuentaCorriente input.form-check-input[type="checkbox"]'
        )
        .on(
            'change.ccPendientes',
            '#tbodyCuentaCorriente input.form-check-input[type="checkbox"]',
            function () {
                actualizarEstadoSeleccionCC();
            }
    );

    registrarEventosCuentaCorriente();
});

// ================================================================
// EVENTOS - COBRANZA CUENTA CORRIENTE
// Namespace: ccPendientes
// ================================================================
function registrarEventosCuentaCorriente() {

    // Check general
    $(document)
        .off('change.ccPendientes', '#chkSeleccionarTodoCC')
        .on('change.ccPendientes', '#chkSeleccionarTodoCC', function () {
            seleccionarTodosMovimientosCC($(this).is(':checked'));
        });

    // Checks individuales
    $(document)
        .off(
            'change.ccPendientes',
            '#tbodyCuentaCorriente input.form-check-input[type="checkbox"]'
        )
        .on(
            'change.ccPendientes',
            '#tbodyCuentaCorriente input.form-check-input[type="checkbox"]',
            function () {
                actualizarEstadoSeleccionCC();
            }
        );

    // Botón verde: modificar Imputa
    $(document)
        .off(
            'click.ccPendientes',
            '#tbodyCuentaCorriente .btn-modificar-imputa-cc'
        )
        .on(
            'click.ccPendientes',
            '#tbodyCuentaCorriente .btn-modificar-imputa-cc',
            function (event) {
                event.preventDefault();
                event.stopPropagation();

                abrirModalImputaCC($(this).closest('tr'));
            }
        );

    // Botón rojo: restaurar Imputa original
    $(document)
        .off(
            'click.ccPendientes',
            '#tbodyCuentaCorriente .btn-restaurar-imputa-cc'
        )
        .on(
            'click.ccPendientes',
            '#tbodyCuentaCorriente .btn-restaurar-imputa-cc',
            function (event) {
                event.preventDefault();
                event.stopPropagation();

                restaurarImputaOriginalCC($(this).closest('tr'));
            }
        );

    // Guardar desde el modal
    $(document)
        .off('click.ccPendientes', '#btnGuardarImputaCC')
        .on('click.ccPendientes', '#btnGuardarImputaCC', function () {
            guardarImputaCC();
        });

    // Permite Enter dentro del formulario
    $(document)
        .off('submit.ccPendientes', '#formDetalleImputaCC')
        .on('submit.ccPendientes', '#formDetalleImputaCC', function (event) {
            event.preventDefault();
            guardarImputaCC();
        });

    // Al cerrar el modal se descarta la referencia temporal a la fila.
    $('#modalDetalleImputaCC')
        .off('hidden.bs.modal.ccPendientes')
        .on('hidden.bs.modal.ccPendientes', function () {
            $filaImputaCCEnEdicion = null;
        });


    $('#btnCancelarCC').on('click', function () {
        $('#modalCuentaCorriente').modal('hide');
        // Volver a abrir el modal de identificación
        setTimeout(() => $('#modalIdentificarCliente').modal('show'), 500);
    });

    $(document)
        .off('click.ccPendientes', '#btnSeguirCC')
        .on('click.ccPendientes', '#btnSeguirCC', function (event) {
            event.preventDefault();
            event.stopPropagation();

            iniciarCobranza();
        });
}

function restaurarImputaOriginalCC($fila) {
    if (!$fila || $fila.length === 0) {
        return;
    }

    const $checkbox = $fila.find(
        'input.form-check-input[type="checkbox"]'
    );

    if (!$checkbox.is(':checked')) {
        mostrarMensajeCC(
            'Atención',
            'Debe seleccionar la factura antes de restaurar el importe a imputar.'
        );
        return;
    }

    const importe = obtenerImporteCC($fila);
    const imputaOriginal = obtenerImputaOriginalCC($fila);

    if (!Number.isFinite(imputaOriginal) ||
        imputaOriginal <= 0 ||
        imputaOriginal > importe) {

        mostrarMensajeCC(
            'Error',
            'El importe original a imputar es inválido.'
        );
        return;
    }

    actualizarImputaFilaCC($fila, imputaOriginal);

    calcularTotalCC();
}

function actualizarImputaFilaCC($fila, imputa) {
    const valor = redondearMontoCC(imputa);

    /*
        attr() actualiza el HTML.
        data() evita que jQuery siga leyendo un valor cacheado anterior.
    */
    $fila
        .attr('data-imputa', valor.toFixed(2))
        .data('imputa', valor);

    $fila.find('.celda-imputa-cc').text(
        `$ ${formatearMontoCC(valor)}`
    );
}

function guardarImputaCC() {
    if (!$filaImputaCCEnEdicion ||
        $filaImputaCCEnEdicion.length === 0) {

        mostrarMensajeCC(
            'Atención',
            'No se pudo determinar la factura que desea modificar.'
        );
        return;
    }

    const importe = obtenerImporteCC($filaImputaCCEnEdicion);

    const imputaNueva = normalizarMontoCC(
        $('#txtImputaCC').val()
    );

    if (!Number.isFinite(imputaNueva)) {
        mostrarMensajeCC(
            'Atención',
            'Debe ingresar un importe válido.'
        );
        return;
    }

    if (imputaNueva <= 0) {
        mostrarMensajeCC(
            'Atención',
            'El importe a imputar debe ser mayor a cero.'
        );
        return;
    }

    if (imputaNueva > importe) {
        mostrarMensajeCC(
            'Atención',
            `El importe a imputar no puede superar el importe de la factura: $ ${formatearMontoCC(importe)}.`
        );
        return;
    }

    actualizarImputaFilaCC(
        $filaImputaCCEnEdicion,
        imputaNueva
    );

    $('#modalDetalleImputaCC').modal('hide');

    calcularTotalCC();
}

function abrirModalImputaCC($fila) {
    if (!$fila || $fila.length === 0) {
        return;
    }

    const $checkbox = $fila.find(
        'input.form-check-input[type="checkbox"]'
    );

    if (!$checkbox.is(':checked')) {
        mostrarMensajeCC(
            'Atención',
            'Debe seleccionar la factura antes de modificar el importe a imputar.'
        );
        return;
    }

    const importe = obtenerImporteCC($fila);
    const imputaActual = obtenerImputaCC($fila);

    if (!Number.isFinite(importe) || importe <= 0) {
        mostrarMensajeCC(
            'Error',
            'La factura posee un importe inválido.'
        );
        return;
    }

    if (!Number.isFinite(imputaActual) || imputaActual <= 0) {
        mostrarMensajeCC(
            'Error',
            'La factura posee un importe inicial a imputar inválido.'
        );
        return;
    }

    $filaImputaCCEnEdicion = $fila;

    $('#lblImporteFacturaCC').text(
        `$ ${formatearMontoCC(importe)}`
    );

    $('#txtImputaCC')
        .val(imputaActual.toFixed(2))
        .attr('data-maximo-permitido', importe.toFixed(2));

    $('#modalDetalleImputaCC').modal('show');

    setTimeout(() => {
        $('#txtImputaCC').trigger('focus').select();
    }, 200);
}

function obtenerCheckboxesCuentaCorriente() {
    return $('#tbodyCuentaCorriente input.form-check-input[type="checkbox"]');
}

function seleccionarTodosMovimientosCC(seleccionar) {
    const $checkboxes = obtenerCheckboxesCuentaCorriente();

    $checkboxes.prop('checked', seleccionar);

    actualizarEstadoSeleccionCC();
}

function actualizarEstadoSeleccionCC() {
    const $checkboxes = obtenerCheckboxesCuentaCorriente();
    const total = $checkboxes.length;
    const seleccionados = $checkboxes.filter(':checked').length;

    $('#chkSeleccionarTodoCC').prop({
        checked: total > 0 && seleccionados === total,
        indeterminate: seleccionados > 0 && seleccionados < total
    });

    sincronizarAccionesImputaCC();
    calcularTotalCC();
}

function sincronizarAccionesImputaCC() {
    $('#tbodyCuentaCorriente tr').each(function () {
        const $fila = $(this);

        const estaSeleccionada = $fila
            .find('input.form-check-input[type="checkbox"]')
            .is(':checked');

        $fila.find('.btn-accion-imputa-cc')
            .prop('disabled', !estaSeleccionada)
            .attr('aria-disabled', !estaSeleccionada);
    });
}

function calcularTotalCC() {
    let total = 0;

    $('#tbodyCuentaCorriente tr').each(function () {
        const $fila = $(this);

        const $checkbox = $fila.find(
            'input.form-check-input[type="checkbox"]'
        );

        if (!$checkbox.is(':checked')) {
            return;
        }

        const imputa = obtenerImputaCC($fila);

        if (Number.isFinite(imputa) && imputa > 0) {
            total += imputa;
        }
    });

    total = redondearMontoCC(total);

    $('#txtTotalCC').val(
        `$ ${formatearMontoCC(total)}`
    );

    $('#btnSeguirCC').prop('disabled', total <= 0);
}

/**
 * ✅ NUEVO v1.0: Inicializa el módulo mostrando el modal de identificación
 */
function inicializarModuloConModal() {
    console.log('═══════════════════════════════════════════════════');
    console.log('⚙️ INICIALIZAR MÓDULO CON MODAL v1.0');
    console.log('═══════════════════════════════════════════════════');

    // ❶ VERIFICAR SI HUBO ERROR AL CARGAR DATOS

    // ❷ VERIFICAR SI HAY FACTURAS


    // ❸ VERIFICAR QUE LOS DATOS EXISTAN


    // ❹ ABRIR EL MODAL DE IDENTIFICACIÓN
    // El usuario busca un cliente registrado en la base de datos y lo selecciona para continuar con el flujo de cobranza.
    setTimeout(() => {
        console.log('   📂 Abriendo modal de identificación de cliente...');
        inicializaVistaFact(); // Función de fact.js que inicializa el modal
    }, 300);

    console.log('═══════════════════════════════════════════════════');
}

/**
 * ✅ NUEVO v1.0: Obtiene los registros de Cuenta Corriente de una cuenta especifica.
 * 
 * @param {object} cliente - Objeto con datos del cliente seleccionado
 */
function obtenerDatosCCDesdeMemoria(cliente) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔍 OBTENER REGISTROS DE CTA CTE v01.01');
    console.log(`   Cta: ${cliente.cta_id}`);
    console.log(`   ID: ${cliente.cta_denominacion}`);
    console.log('═══════════════════════════════════════════════════');

    mostrarLoader('Buscando datos en Cuenta Corriente');

    $.ajax({
        url: obtenerCtaCteUrl,
        type: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify({ cta_id: cliente.cta_id }), // ✅ Enviamos solo el ID del cliente
        success: function (response) {
            ocultarLoader();
            console.log('   📥 Respuesta de obtenerCtaCte:', response);

            if (!response || !response.ok) {
                const mensajeError = response?.mensaje || "No se encontraron registros en CC para este cliente.";
                console.warn('⚠️ Sin registros CC para el cliente:', mensajeError);
                AbrirMensaje("Información", mensajeError, function () { $("#msjModal").modal("hide"); }, false, ["Aceptar"], "info");

                //VERIFICAR CREO QUE AUN NO LO HEMOS CERRADO

                // Volver a abrir el modal de identificación
                //setTimeout(() => $('#modalIdentificarCliente').modal('show'), 500);
                return;
            }

            const regCC = response.lista;
            console.log(`   ✅ Registros CC encontradas: ${regCC.length ?? 0}`);

            // Mostrar modal con facturas del cliente específico
            mostrarCtaCtePendientes(cliente, regCC);
        },
        error: function (xhr, status, error) {
            ocultarLoader();
            console.error('❌ Error AJAX:', {
                status: xhr.status,
                statusText: xhr.statusText,
                responseText: xhr.responseText,
                error: error
            });

            let mensajeError = "No se pudo obtener las facturas del cliente.";

            if (xhr.responseJSON && xhr.responseJSON.mensaje) {
                mensajeError = xhr.responseJSON.mensaje;
            } else if (xhr.status === 400) {
                mensajeError = "Datos inválidos. Por favor, intente nuevamente.";
            } else if (xhr.status === 0) {
                mensajeError = "No se pudo establecer conexión con el servidor.";
            }

            AbrirMensaje("Error de Comunicación", mensajeError, function () { $("#msjModal").modal("hide"); }, false, ["Aceptar"], "error");

            // Volver a abrir el modal de identificación
            setTimeout(() => $('#modalIdentificarCliente').modal('show'), 500);
        }
    });

    console.log('═══════════════════════════════════════════════════');
}

/**
* ✅ CORREGIDO v4.0: Muestra el modal con las facturas pendientes y puebla los datos del cliente.
* CRÍTICO v4.0: Ahora agrega TODOS los data-* attributes necesarios para el resguardo posterior
* 
* @param {object} cliente - Datos completos del cliente
* @param {Array} regCC - Lista de facturas pendientes (opcional, se recupera de sesión si no se provee)
*/
function mostrarCtaCtePendientes(cliente, regCC = null) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📋 MOSTRAR MODAL CTA CTE PENDIENTES v4.0');
    console.log('   Cliente:', cliente.denominacion);
    console.log('   Registros provistos:', regCC ? regCC.length : 'Se recuperarán de sesión');
    console.log('═══════════════════════════════════════════════════');

    // ✅ HELPER: Obtener primer valor no vacío
    const primerValorNoVacio = (...valores) => {
        for (let valor of valores) {
            if (valor && valor !== '' && valor !== 'null' && valor !== 'undefined') {
                return valor;
            }
        }
        return '';
    };

    // ✅ HELPER: Sanitizar valores para data-attributes
    const sanitizarData = (valor) => {
        if (valor === null || valor === undefined) return '';
        return String(valor).trim();
    };

    // ❶ POBLAR DATOS DEL CLIENTE EN EL MODAL CON FALLBACKS MÚLTIPLES
    $('#txtNombreCC').val(
        primerValorNoVacio(cliente.denominacion, cliente.cta_denominacion, cliente.nombre)
    );

    $('#txtClienteIdCC').val(
        primerValorNoVacio(cliente.id, cliente.cta_id, '')
    );

    $('#txtDomicilioCC').val(
        primerValorNoVacio(cliente.domicilio, cliente.cta_domicilio)
    );

    $('#txtCondicionAfipCC').val(
        primerValorNoVacio(cliente.condicionAfip, cliente.afip_desc, cliente.afip_id)
    );

    let tipoNumeroFinal = primerValorNoVacio(cliente.tipoNumero);
    if (!tipoNumeroFinal && cliente.tdoc_desc && cliente.documento) {
        tipoNumeroFinal = `${cliente.tdoc_desc} ${cliente.documento}`;
    } else if (!tipoNumeroFinal && (cliente.tdoc_desc || cliente.documento)) {
        tipoNumeroFinal = cliente.tdoc_desc || cliente.documento;
    }
    $('#txtTipoNumeroCC').val(tipoNumeroFinal);

    $('#txtEmailCC').val(
        primerValorNoVacio(cliente.email, cliente.cta_email)
    );

    $('#txtMovilCC').val(
        primerValorNoVacio(cliente.movil, cliente.cta_celu)
    );

    console.log('   📝 Valores asignados al modal:');
    console.log(`      Nombre: "${$('#txtNombreCC').val()}"`);
    console.log(`      ID: "${$('#txtClienteIdCC').val()}"`);
    console.log(`      Domicilio: "${$('#txtDomicilioCC').val()}"`);
    console.log(`      Condición AFIP: "${$('#txtCondicionAfipCC').val()}"`);
    console.log(`      Tipo/Número: "${$('#txtTipoNumeroCC').val()}"`);
    console.log(`      Email: "${$('#txtEmailCC').val()}"`);
    console.log(`      Móvil: "${$('#txtMovilCC').val()}"`);

    // ❷ ✅ CORREGIDO v4.0: FUNCIÓN INTERNA PARA POBLAR LA GRILLA CON TODOS LOS DATA-ATTRIBUTES
    const poblarGrillaCC = (listaCC) => {
        console.log('   🔄 Poblando grilla con facturas:', listaCC.length);

        const $tbody = $('#tbodyCuentaCorriente');
        $tbody.empty();

        if (!Array.isArray(listaCC) || listaCC.length === 0) {
            console.warn('⚠️ Lista de Cuenta Corriente vacía o inválida');
            $tbody.append('<tr><td colspan="6" class="text-center text-muted py-4">No hay facturas pendientes</td></tr>');
            $('#modalCuentaCorriente').modal('show');
            return;
        }

        listaCC.forEach((ctacte, index) => {
            try {
                const fecha = ctacte.cv_fecha_vto
                    ? new Date(ctacte.cv_fecha_vto).toLocaleDateString('es-AR')
                    : 'N/A';

                const importe = parseFloat(ctacte.cv_importe || 0);
                const imputa = parseFloat(ctacte.cv_importe_ori || 0);
                const clienteId = sanitizarData(ctacte.cta_id) || '---';
                const nombre = `${cliente.denominacion || 'N/A'} (${clienteId})`;
                // ✅ CRÍTICO v4.0: Agregar TODOS los data-* attributes necesarios
                const fila = `
                    <tr data-importe="${importe}"
                        data-imputa="${imputa}"
                        data-imputa-ori="${imputa}">

                        <td>${ctacte.tco_id || 'N/A'}</td>
                        <td>${ctacte.cm_compte || 'N/A'}</td>
                        <td>${nombre}</td>
                        <td>${tipoNumeroFinal}</td>
                        <td class="text-center">${fecha}</td>

                        <td class="text-end fw-bold">
                            ${formatearNumero(importe, 2)}
                        </td>

                        <td class="text-end fw-bold celda-imputa-cc">
                            ${formatearNumero(imputa, 2)}
                        </td>

                        <td class="text-center py-1">
                            <button type="button"
                                    class="btn btn-xs btn-success btn-modificar-imputa-cc btn-accion-imputa-cc"
                                    title="Modificar importe a imputar"
                                    aria-label="Modificar importe a imputar">
                                <i class="bx bx-edit-alt" style="font-size: 0.9rem;"></i>
                            </button>

                            <button type="button"
                                    class="btn btn-xs btn-danger btn-restaurar-imputa-cc btn-accion-imputa-cc ms-1"
                                    title="Restaurar importe original"
                                    aria-label="Restaurar importe original">
                                <i class="bx bx-undo" style="font-size: 0.9rem;"></i>
                            </button>
                        </td>

                        <td class="text-center">
                            <input type="checkbox"
                                   class="form-check-input"

                                   data-cta-id="${sanitizarData(ctacte.cta_id)}"
                                   data-dia-movi="${sanitizarData(ctacte.dia_movi)}"
                                   data-tco-id="${sanitizarData(ctacte.tco_id)}"
                                   data-cm-compte="${sanitizarData(ctacte.cm_compte)}"
                                   data-cm-compte-cuota="${ctacte.cm_compte_cuota || 0}"
                                   data-cv-fecha-vto="${sanitizarData(ctacte.cv_fecha_vto)}"
                                   data-cv-importe="${ctacte.cv_importe || 0}"
                                   data-cv-importe-ori="${ctacte.cv_importe_ori || 0}"
                                   data-cv-concepto="${sanitizarData(ctacte.cv_concepto)}"
                                   data-ve-id="${sanitizarData(ctacte.ve_id)}"
                                   data-ccb-id="${sanitizarData(ctacte.ccb_id)}"
                                   data-ctacte="${sanitizarData(ctacte.ctacte)}"
                                   data-carga="${sanitizarData(ctacte.carga)}"
                                   data-carga-obligatoria="${sanitizarData(ctacte.carga_obligatoria)}">
                        </td>
                    </tr>
                `;
                $tbody.append(fila);

                // ✅ Log de muestra para debugging (primera factura)
                if (index === 0) {
                    console.log('   🔍 Data-attributes agregados a primera factura:');
                    console.log(`      tco_id: "${ctacte.tco_id}"`);
                    console.log(`      cm_compte: "${ctacte.cm_compte}"`);
                    console.log(`      cv_fecha_vto: "${ctacte.cv_fecha_vto}"`);
                    console.log(`      cv_importe: ${ctacte.cv_importe}`);
                    console.log(`      cv_importe_ori: ${ctacte.cv_importe_ori}`);
                }

            } catch (error) {
                console.error(`❌ Error al renderizar factura ${index}:`, error, ctacte);
            }
        });

      
        seleccionarTodosMovimientosCC(true);
        //// Resetear controles
        cerrarTecladoDigital();

        //$('#chkSeleccionarTodo').prop('checked', false);
        //calcularTotalSeleccionado();

        // Mostrar modal
        $('#modalCuentaCorriente').modal('show');
        console.log('   ✅ Modal de cuenta corriente mostrado correctamente');
    };

    // ❸ DECIDIR FUENTE DE DATOS
    if (regCC && Array.isArray(regCC) && regCC.length > 0) {
        console.log('   📌 Usando registros de cuenta corriente provistos por parámetro');
        poblarGrillaCC(regCC);
    } else {
        console.log('   📡 Recuperando la cuenta corriente  desde sesión del servidor...');
        mostrarLoader('Cargando registros de Cuenta Corriente del cliente...');

        $.ajax({
            url: obtenerCtaCteUrl,
            type: 'POST',
            success: function (response) {
                ocultarLoader();
                console.log('   📥 Respuesta de sesión:', response);

                if (response.ok && response.lista && response.lista.length > 0) {
                    poblarGrillaCC(response.lista);
                } else {
                    console.warn('⚠️ No se encontraron registros de cuenta corriente en la sesión');
                    AbrirMensaje("Información", "No se encontraron registros de cuenta corriente pendientes para este cliente.", function () { $("#msjModal").modal("hide"); }, false, ["Aceptar"], "info");
                }
            },
            error: function (xhr, status, error) {
                ocultarLoader();
                console.error('❌ Error al recuperar registros de cuenta corriente de sesión:', error);
                AbrirMensaje("Error de Comunicación", "No se pudieron recuperar los registros de cuenta corriente del cliente.", function () { $("#msjModal").modal("hide"); }, false, ["Aceptar"], "error");
            }
        });
    }

    console.log('═══════════════════════════════════════════════════');
}


function seleccionarTodosRegCtaCte(seleccionar) {
    const $checkboxes = obtenerCheckboxesCtaCte();

    $checkboxes.prop('checked', seleccionar);

    actualizarEstadoSeleccionCtaCte();
}

function obtenerCheckboxesCtaCte() {
    return $('#tbodyCuentaCorriente input.form-check-input[type="checkbox"]');
}

function actualizarEstadoSeleccionCtaCte() {
    const $checkboxes = obtenerCheckboxesCtaCte();
    const total = $checkboxes.length;
    const seleccionados = $checkboxes.filter(':checked').length;

    $('#chkSeleccionarTodo').prop({
        checked: total > 0 && seleccionados === total,
        indeterminate: seleccionados > 0 && seleccionados < total
    });

    calcularTotalSeleccionadoCC();
}
function calcularTotalSeleccionadoCC() {
    let total = 0;
    $('#tbodyCuentaCorriente tr').each(function () {
        const $checkbox = $(this).find('input[type="checkbox"]');
        if ($checkbox.is(':checked')) {
            total += parseFloat($(this).data('importe') || 0);
        }
    });

    $('#txtTotalSeleccionado').val(`$ ${formatearNumero(total, 2)}`);
    $('#btnSeguirConCobranza').prop('disabled', total === 0);
}

function abrirModalModificarImporteCC($fila) {
    if (!$fila || $fila.length === 0) {
        return;
    }

    const importeOriginal = obtenerImporteActualCC($fila);

    if (importeOriginal < 0) {
        mostrarMensajeImporteCC(
            'Error',
            'El importe original del registro es inválido.'
        );
        return;
    }

    $filaCuentaCorrienteEnEdicion = $fila;

    $('#lblImporteOriginalCC').text(
        `$ ${formatearNumero(importeOriginal, 2)}`
    );

    // Requerimiento: cargar el modal con data-importe-ori.
    $('#txtImportePagarCC')
        .val(importeOriginal.toFixed(2))
        .attr('data-maximo-permitido', importeOriginal);

    $('#modalModificarImporteCC').modal('show');

    setTimeout(() => {
        const $input = $('#txtImportePagarCC');

        $input.trigger('focus');
        $input.select();
    }, 250);
}

function guardarImporteModificadoCC() {
    if (!$filaCuentaCorrienteEnEdicion ||
        $filaCuentaCorrienteEnEdicion.length === 0) {

        mostrarMensajeImporteCC(
            'Atención',
            'No se pudo identificar el registro que desea modificar.'
        );
        return;
    }

    const importeOriginal = obtenerImporteOriginalCC($filaCuentaCorrienteEnEdicion);
    const importeIngresado = parsearImporteCC($('#txtImportePagarCC').val());

    if (!Number.isFinite(importeIngresado)) {
        mostrarMensajeImporteCC(
            'Atención',
            'Debe ingresar un importe válido.'
        );
        return;
    }

    if (importeIngresado < 0) {
        mostrarMensajeImporteCC(
            'Atención',
            'El importe a pagar no puede ser menor a cero.'
        );
        return;
    }

    if (importeIngresado > importeOriginal) {
        mostrarMensajeImporteCC(
            'Atención',
            `El importe a pagar no puede superar el importe original de $ ${formatearNumero(importeOriginal, 2)}.`
        );
        return;
    }

    actualizarImporteFilaCC(
        $filaCuentaCorrienteEnEdicion,
        importeIngresado
    );

    $('#modalModificarImporteCC').modal('hide');

    recalcularTotalCuentaCorriente();
}

function restaurarImporteOriginalCC($fila) {
    if (!$fila || $fila.length === 0) {
        return;
    }

    const importeOriginal = obtenerImporteOriginalCC($fila);

    actualizarImporteFilaCC($fila, importeOriginal);

    recalcularTotalCuentaCorriente();
}

function actualizarImporteFilaCC($fila, importe) {
    const importeNormalizado = redondearImporteCC(importe);

    /*
        Se actualizan ambos lugares:

        1. attr: mantiene actualizado el HTML real.
        2. data: mantiene actualizado el caché interno de jQuery.

        Esto evita inconsistencias al consultar luego:
        $fila.data('importe')
        o
        $fila.attr('data-importe')
    */
    $fila
        .attr('data-importe', importeNormalizado.toFixed(2))
        .data('importe', importeNormalizado);

    $fila.find('.celda-importe-cc').text(
        `$ ${formatearNumero(importeNormalizado, 2)}`
    );
}

function obtenerImporteActualCC($fila) {
    return parsearImporteCC($fila.attr('data-importe'));
}

function obtenerImporteOriginalCC($fila) {
    return parsearImporteCC($fila.attr('data-importe-ori'));
}

function recalcularTotalCuentaCorriente() {
    if (typeof calcularTotalCC === 'function') {
        calcularTotalCC();
        return;
    }

    console.warn('No se encontró la función calcularTotalCC().');
}

function redondearImporteCC(valor) {
    return Math.round((Number(valor) + Number.EPSILON) * 100) / 100;
}

function parsearImporteCC(valor) {
    if (valor === null || valor === undefined || valor === '') {
        return NaN;
    }

    if (typeof valor === 'number') {
        return Number.isFinite(valor)
            ? redondearImporteCC(valor)
            : NaN;
    }

    let texto = String(valor)
        .trim()
        .replace(/\$/g, '')
        .replace(/\s/g, '');

    if (!texto) {
        return NaN;
    }

    const ultimaComa = texto.lastIndexOf(',');
    const ultimoPunto = texto.lastIndexOf('.');

    /*
        Admite:
        1250.50
        1250,50
        1,250.50
        1.250,50
    */
    if (ultimaComa !== -1 && ultimoPunto !== -1) {
        if (ultimaComa > ultimoPunto) {
            texto = texto.replace(/\./g, '').replace(',', '.');
        } else {
            texto = texto.replace(/,/g, '');
        }
    } else if (ultimaComa !== -1) {
        texto = texto.replace(/\./g, '').replace(',', '.');
    }

    const importe = Number(texto);

    return Number.isFinite(importe)
        ? redondearImporteCC(importe)
        : NaN;
}

function mostrarMensajeImporteCC(titulo, mensaje) {
    if (typeof AbrirMensaje === 'function') {
        AbrirMensaje(
            titulo,
            mensaje,
            function () {
                $('#msjModal').modal('hide');
            },
            false,
            ['Aceptar'],
            'warning'
        );

        return;
    }

    window.alert(`${titulo}: ${mensaje}`);
}

/**********************
* 
* HELPERS DE LECTURA Y FORMATO
*
***********************/
function obtenerImporteCC($fila) {
    return normalizarMontoCC(
        $fila.attr('data-importe')
    );
}

function obtenerImputaCC($fila) {
    return normalizarMontoCC(
        $fila.attr('data-imputa')
    );
}

function obtenerImputaOriginalCC($fila) {
    return normalizarMontoCC(
        $fila.attr('data-imputa-ori')
    );
}

function redondearMontoCC(valor) {
    return Math.round(
        (Number(valor) + Number.EPSILON) * 100
    ) / 100;
}

function normalizarMontoCC(valor) {
    if (valor === null || valor === undefined || valor === '') {
        return NaN;
    }

    if (typeof valor === 'number') {
        return Number.isFinite(valor)
            ? redondearMontoCC(valor)
            : NaN;
    }

    let texto = String(valor)
        .trim()
        .replace(/\$/g, '')
        .replace(/\s/g, '');

    if (!texto) {
        return NaN;
    }

    const ultimaComa = texto.lastIndexOf(',');
    const ultimoPunto = texto.lastIndexOf('.');

    // Admite: 2500.50 / 2500,50 / 2,500.50 / 2.500,50
    if (ultimaComa !== -1 && ultimoPunto !== -1) {
        if (ultimaComa > ultimoPunto) {
            texto = texto
                .replace(/\./g, '')
                .replace(',', '.');
        } else {
            texto = texto.replace(/,/g, '');
        }
    } else if (ultimaComa !== -1) {
        texto = texto
            .replace(/\./g, '')
            .replace(',', '.');
    }

    const monto = Number(texto);

    return Number.isFinite(monto)
        ? redondearMontoCC(monto)
        : NaN;
}

function formatearMontoCC(monto) {
    const valor = Number.isFinite(monto) ? monto : 0;

    // Reutiliza el formato común del proyecto, si ya existe.
    if (typeof formatearNumero === 'function') {
        return formatearNumero(valor, 2);
    }

    return new Intl.NumberFormat('en-US', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }).format(valor);
}

function mostrarMensajeCC(titulo, mensaje) {
    if (typeof AbrirMensaje === 'function') {
        AbrirMensaje(
            titulo,
            mensaje,
            function () {
                $('#msjModal').modal('hide');
            },
            false,
            ['Aceptar'],
            'warning'
        );

        return;
    }

    window.alert(`${titulo}: ${mensaje}`);
}

let cobranzaCCEnCurso = false;

function iniciarCobranza() {
    console.log('═══════════════════════════════════════════════════');
    console.log('💰 INICIAR COBRANZA CUENTA CORRIENTE');
    console.log('═══════════════════════════════════════════════════');

    if (cobranzaCCEnCurso) {
        console.warn('⚠️ Ya existe una solicitud de cobranza en curso.');
        return;
    }

    // Validar disponibilidad del módulo de pago.
    if (typeof iniciarProcesoPago !== 'function') {
        console.error('❌ No está disponible la función iniciarProcesoPago.');

        AbrirMensaje(
            'Error',
            'El módulo de pago no está disponible. Recargue la página e intente nuevamente.',
            function () { $('#msjModal').modal('hide'); },
            false,
            ['Aceptar'],
            'error!'
        );

        return;
    }

    const $checkboxesSeleccionados = $(
        '#tbodyCuentaCorriente input.form-check-input[type="checkbox"]:checked'
    );

    console.log(
        `   📋 Registros seleccionados: ${$checkboxesSeleccionados.length}`
    );

    if ($checkboxesSeleccionados.length === 0) {
        AbrirMensaje(
            'Atención',
            'Debe seleccionar al menos un registro de Cuenta Corriente para cobrar.',
            function () { $('#msjModal').modal('hide'); },
            false,
            ['Aceptar'],
            'warn!'
        );

        return;
    }

    const registrosSeleccionados = [];
    const errores = [];

    const obtenerData = ($elemento, nombre) => {
        const valor = $elemento.attr(`data-${nombre}`);

        if (valor === undefined || valor === null) {
            return '';
        }

        return String(valor).trim();
    };

    const convertirADecimal = (valor) => {
        if (valor === null || valor === undefined || valor === '') {
            return NaN;
        }

        if (typeof valor === 'number') {
            return Number.isFinite(valor) ? valor : NaN;
        }

        let texto = String(valor)
            .trim()
            .replace(/\$/g, '')
            .replace(/\s/g, '');

        const tieneComa = texto.includes(',');
        const tienePunto = texto.includes('.');

        // Ejemplo AR: 1.234,56
        if (tieneComa && tienePunto) {
            if (texto.lastIndexOf(',') > texto.lastIndexOf('.')) {
                texto = texto.replace(/\./g, '').replace(',', '.');
            } else {
                texto = texto.replace(/,/g, '');
            }
        } else if (tieneComa) {
            texto = texto.replace(',', '.');
        }

        const numero = Number(texto);

        return Number.isFinite(numero) ? numero : NaN;
    };

    const convertirAEntero = (valor) => {
        const texto = String(valor ?? '').trim();

        if (texto === '') {
            return NaN;
        }

        const numero = Number.parseInt(texto, 10);

        return Number.isInteger(numero) ? numero : NaN;
    };

    const normalizarFechaParaServidor = (valor) => {
        const texto = String(valor ?? '').trim();

        if (!texto) {
            return null;
        }

        // Si ya viene como fecha ISO desde .NET, la conservamos.
        if (/^\d{4}-\d{2}-\d{2}(T.*)?$/.test(texto)) {
            return texto.replace(' ', 'T');
        }

        const fecha = new Date(texto);

        if (Number.isNaN(fecha.getTime())) {
            return null;
        }

        return fecha.toISOString();
    };

    $checkboxesSeleccionados.each(function (index) {
        const $checkbox = $(this);
        const $fila = $checkbox.closest('tr');

        // IMPORTANTE:
        // El importe a cobrar es el valor actual editable de la fila.
        const imputaActual = convertirADecimal(
            $fila.attr('data-imputa')
        );

        // Importe máximo disponible para validar la imputación.
        const importeDisponible = convertirADecimal(
            $fila.attr('data-importe')
        );

        // Importe de imputación original para conservar trazabilidad.
        const imputaOriginal = convertirADecimal(
            $fila.attr('data-imputa-ori')
        );

        const registro = {
            cta_id: obtenerData($checkbox, 'cta-id'),
            dia_movi: obtenerData($checkbox, 'dia-movi'),
            tco_id: obtenerData($checkbox, 'tco-id'),
            cm_compte: obtenerData($checkbox, 'cm-compte'),
            cm_compte_cuota: convertirAEntero(
                obtenerData($checkbox, 'cm-compte-cuota')
            ),
            cv_fecha_vto: normalizarFechaParaServidor(
                obtenerData($checkbox, 'cv-fecha-vto')
            ),

            // Valor final que será cobrado.
            cv_importe: imputaActual,

            // Valor original antes de una eventual modificación manual.
            cv_importe_ori: imputaOriginal,

            cv_concepto: obtenerData($checkbox, 'cv-concepto'),

            // Ambos son string según CtaCteResponseDto.
            ve_id: obtenerData($checkbox, 've-id'),
            ccb_id: obtenerData($checkbox, 'ccb-id'),

            ctacte: obtenerData($checkbox, 'ctacte'),
            carga: obtenerData($checkbox, 'carga'),
            carga_obligatoria: obtenerData($checkbox, 'carga-obligatoria')
        };

        const identificador = `${registro.tco_id} ${registro.cm_compte}`;

        const camposFaltantes = [];

        if (!registro.cta_id) camposFaltantes.push('cta_id');
        if (!registro.tco_id) camposFaltantes.push('tco_id');
        if (!registro.cm_compte) camposFaltantes.push('cm_compte');
        if (!registro.ctacte) camposFaltantes.push('ctacte');

        if (camposFaltantes.length > 0) {
            errores.push(
                `Registro ${index + 1} (${identificador}): faltan ${camposFaltantes.join(', ')}.`
            );

            return;
        }

        if (!Number.isInteger(registro.cm_compte_cuota) ||
            registro.cm_compte_cuota < 0) {

            errores.push(
                `Registro ${index + 1} (${identificador}): la cuota es inválida.`
            );

            return;
        }

        if (!registro.cv_fecha_vto) {
            errores.push(
                `Registro ${index + 1} (${identificador}): la fecha de vencimiento es inválida.`
            );

            return;
        }

        if (!Number.isFinite(importeDisponible) || importeDisponible <= 0) {
            errores.push(
                `Registro ${index + 1} (${identificador}): el importe disponible es inválido.`
            );

            return;
        }

        if (!Number.isFinite(registro.cv_importe) ||
            registro.cv_importe <= 0) {

            errores.push(
                `Registro ${index + 1} (${identificador}): el importe a imputar debe ser mayor a cero.`
            );

            return;
        }

        if (registro.cv_importe > importeDisponible) {
            errores.push(
                `Registro ${index + 1} (${identificador}): el importe a imputar no puede superar $ ${formatearMontoCC(importeDisponible)}.`
            );

            return;
        }

        if (!Number.isFinite(registro.cv_importe_ori)) {
            errores.push(
                `Registro ${index + 1} (${identificador}): el importe original es inválido.`
            );

            return;
        }

        registrosSeleccionados.push(registro);
    });

    if (errores.length > 0) {
        console.error('❌ Errores de validación de Cuenta Corriente:', errores);

        AbrirMensaje(
            'Datos incompletos',
            `No se puede iniciar la cobranza.<br><br>${errores.join('<br>')}`,
            function () { $('#msjModal').modal('hide'); },
            false,
            ['Aceptar'],
            'error!'
        );

        return;
    }

    if (registrosSeleccionados.length === 0) {
        AbrirMensaje(
            'Atención',
            'No se pudo construir ningún registro válido para la cobranza.',
            function () { $('#msjModal').modal('hide'); },
            false,
            ['Aceptar'],
            'error!'
        );

        return;
    }

    const totalPagar = registrosSeleccionados.reduce(
        (acumulado, registro) => acumulado + registro.cv_importe,
        0
    );

    const totalRedondeado = Math.round(
        (totalPagar + Number.EPSILON) * 100
    ) / 100;

    if (totalRedondeado <= 0) {
        AbrirMensaje(
            'Error',
            'El total a cobrar debe ser mayor a cero.',
            function () { $('#msjModal').modal('hide'); },
            false,
            ['Aceptar'],
            'error!'
        );

        return;
    }

    console.log('   ✅ Registros válidos:', registrosSeleccionados.length);
    console.log('   💰 Total a cobrar:', totalRedondeado);
    console.log(
        '   📦 Primer registro:',
        JSON.stringify(registrosSeleccionados[0], null, 2)
    );

    cobranzaCCEnCurso = true;
    $('#btnSeguirCC').prop('disabled', true);

    mostrarLoader('Preparando cobranza de Cuenta Corriente...');

    $.ajax({
        url: resguardarCuentaCorrienteSeleccionadaUrl,
        type: 'POST',
        contentType: 'application/json',
        dataType: 'json',

        // El nombre "Registros" debe coincidir con el request DTO C#.
        data: JSON.stringify({
            Registros: registrosSeleccionados
        }),

        success: function (response) {
            ocultarLoader();

            if (!response || !response.ok) {
                cobranzaCCEnCurso = false;
                calcularTotalCC();

                const mensaje =
                    response?.mensaje ||
                    'No se pudieron resguardar los registros seleccionados.';

                console.error('❌ Error al resguardar Cuenta Corriente:', mensaje);

                AbrirMensaje(
                    'Error',
                    mensaje,
                    function () { $('#msjModal').modal('hide'); },
                    false,
                    ['Aceptar'],
                    'error!'
                );

                return;
            }

            console.log(
                '✅ Registros seleccionados resguardados correctamente en sesión.'
            );

            // Útil para diagnóstico o pasos posteriores del cliente.
            window._cuentaCorrienteDelClienteSeleccionadaParaElCobro =
                registrosSeleccionados;

            const iniciarPago = () => {
                try {
                    iniciarProcesoPago({
                        totalPagar: totalRedondeado,
                        co_tipo: 'CC',
                        puntoVenta: 'GECO PD',
                        tituloModal: 'Cobranza Cuenta Corriente',
                        contextoOperacion: 'COBRANZA'
                    });

                    console.log('✅ Proceso de pago iniciado.');
                } catch (error) {
                    console.error('❌ Error al iniciar el proceso de pago:', error);

                    AbrirMensaje(
                        'Error',
                        'No se pudo iniciar el proceso de pago.',
                        function () { $('#msjModal').modal('hide'); },
                        false,
                        ['Aceptar'],
                        'error!'
                    );
                } finally {
                    cobranzaCCEnCurso = false;
                }
            };

            const $modalCuentaCorriente = $('#modalCuentaCorriente');

            // Espera el cierre real del modal antes de abrir el flujo de pago.
            if ($modalCuentaCorriente.hasClass('show')) {
                $modalCuentaCorriente
                    .off('hidden.bs.modal.iniciarCobranzaCC')
                    .one('hidden.bs.modal.iniciarCobranzaCC', iniciarPago)
                    .modal('hide');
            } else {
                iniciarPago();
            }
        },

        error: function (xhr, status, error) {
            ocultarLoader();

            cobranzaCCEnCurso = false;
            calcularTotalCC();

            console.error('❌ Error AJAX al resguardar Cuenta Corriente:', {
                status: xhr.status,
                statusText: xhr.statusText,
                responseText: xhr.responseText,
                error: error
            });

            let mensaje =
                'No se pudieron guardar los registros seleccionados en el servidor.';

            if (xhr.responseJSON?.mensaje) {
                mensaje = xhr.responseJSON.mensaje;
            } else if (xhr.status === 400) {
                mensaje = 'Los datos enviados son inválidos.';
            } else if (xhr.status === 0) {
                mensaje = 'No se pudo establecer conexión con el servidor.';
            }

            AbrirMensaje(
                'Error de Comunicación',
                mensaje,
                function () { $('#msjModal').modal('hide'); },
                false,
                ['Aceptar'],
                'error!'
            );
        }
    });

    console.log('═══════════════════════════════════════════════════');
}