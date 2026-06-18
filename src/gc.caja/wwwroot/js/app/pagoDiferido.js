// ========================================================
// GESTOR PRINCIPAL DEL MÓDULO DE COBRANZA DIFERIDA (CD)
// ✅ v3.1 - FLUJO DUAL: Ver todas las facturas O buscar cliente específico
// ========================================================
let clienteSeleccionadoVFP = null;
let nombreClienteVFP = '';

$(function () {
    console.log('═══════════════════════════════════════════════════');
    console.log('🚀 MÓDULO DE COBRANZA DIFERIDA v3.1 CARGADO');
    console.log('   MODO: Flujo dual (Ver todas / Buscar cliente)');
    console.log('═══════════════════════════════════════════════════');

    // ✅ NUEVO v3.1: NO INICIALIZAR AUTOMÁTICAMENTE
    // El modal de identificación se abrirá primero
    inicializarModuloConModal();

    // ═══════════════════════════════════════════════════════════════════
    // ✅ NUEVO v3.1: SUSCRIPCIÓN AL EVENTO 'clienteConfirmado'
    // Este evento se dispara cuando el usuario busca y confirma un cliente
    // ═══════════════════════════════════════════════════════════════════
    $(document).on('clienteConfirmado', function (event, cliente) {
        console.log('═══════════════════════════════════════════════════');
        console.log('✅ CLIENTE CONFIRMADO EN COBRANZA DIFERIDA v3.1');
        console.log('   Cliente:', cliente.denominacion);
        console.log('   ID:', cliente.id);
        console.log('═══════════════════════════════════════════════════');

        // Ocultar el modal de identificación
        $('#modalIdentificarCliente').modal('hide');

        // Buscar facturas del cliente específico (FILTRADO DESDE SESIÓN)
        obtenerFacturasDeClienteDesdeMemoria(cliente);
    });

    // MANEJAR BOTONES DEL MODAL DE FACTURAS PENDIENTES (modal de cliente específico)
    $('#btnCancelarSeleccionFacturas').on('click', function () {
        $('#modalFacturasPendientes').modal('hide');
        // Volver a abrir el modal de identificación
        setTimeout(() => $('#modalIdentificarCliente').modal('show'), 500);
    });

    $('#chkSeleccionarTodo').on('change', function () {
        const isChecked = $(this).is(':checked');
        $('#tbodyFacturasPendientes').find('input[type="checkbox"]').prop('checked', isChecked).trigger('change');
    });

    $(document).on('change', '#tbodyFacturasPendientes input[type="checkbox"]', function () {
        calcularTotalSeleccionado();
    });

    $('#btnSeguirConCobranza').on('click', function () {
        iniciarCobranza();
    });

    // ═══════════════════════════════════════════════════════════════════
    // LÓGICA PARA EL MODAL _verFacturasPendientes (TODAS las facturas)
    // ═══════════════════════════════════════════════════════════════════

    // ✅ NUEVO v3.1: Manejador del botón "Ver Facturas Pendientes"
    $('#btnVerFacturasPendientes').on('click', function () {
        console.log('═══════════════════════════════════════════════════');
        console.log('👁️ VER TODAS LAS FACTURAS PENDIENTES v3.1');
        console.log('═══════════════════════════════════════════════════');

        // Ocultar modal de identificación
        $('#modalIdentificarCliente').modal('hide');

        // Mostrar modal con TODAS las facturas (ya cargadas en memoria)
        setTimeout(() => {
            mostrarModalVerFacturasPendientes(FACTURAS_PENDIENTES);
        }, 500);
    });

    // Evento para el checkbox "Seleccionar Todo" del modal VFP
    $('#chkSeleccionarTodoVFP').on('change', function () {
        const isChecked = $(this).is(':checked');
        const $checkboxes = $('#tbodyVerFacturasPendientes').find('input[type="checkbox"]');

        if (isChecked && $checkboxes.length > 0) {
            const primerCheckbox = $checkboxes.not(':disabled').first();
            if (primerCheckbox.length === 0) return;

            const clienteId = primerCheckbox.data('cliente-id');
            const clienteDoc = primerCheckbox.data('cliente-doc');
            clienteSeleccionadoVFP = { id: clienteId, doc: clienteDoc };

            $checkboxes.each(function () {
                const $cb = $(this);
                const esMismoCliente = ($cb.data('cliente-id') === clienteId && $cb.data('cliente-doc') === clienteDoc);
                $cb.prop('checked', esMismoCliente);
                $cb.closest('tr').toggleClass('fila-deshabilitada', !esMismoCliente);
            });
        } else {
            clienteSeleccionadoVFP = null;
            $checkboxes.prop('checked', false);
            $('#tbodyVerFacturasPendientes tr').removeClass('fila-deshabilitada');
        }
        calcularTotalSeleccionadoVFP();
    });

    // Evento para los checkboxes individuales del modal VFP
    $(document).on('change', '#tbodyVerFacturasPendientes input[type="checkbox"]', function () {
        const $checkbox = $(this);
        const isChecked = $checkbox.is(':checked');
        const clienteIdActual = $checkbox.data('cliente-id');
        const clienteDocActual = $checkbox.data('cliente-doc');

        if (isChecked) {
            if (clienteSeleccionadoVFP && (clienteSeleccionadoVFP.id !== clienteIdActual || clienteSeleccionadoVFP.doc !== clienteDocActual)) {
                $checkbox.prop('checked', false);
                AbrirMensaje("Atención", "Solo puede seleccionar facturas del mismo cliente.", null, false, ["Aceptar"], "warning");
                return;
            }

            if (!clienteSeleccionadoVFP) {
                clienteSeleccionadoVFP = { id: clienteIdActual, doc: clienteDocActual };
                $('#tbodyVerFacturasPendientes tr').each(function () {
                    const $fila = $(this);
                    const idFila = $fila.find('input[type="checkbox"]').data('cliente-id');
                    const docFila = $fila.find('input[type="checkbox"]').data('cliente-doc');
                    if (idFila !== clienteSeleccionadoVFP.id || docFila !== clienteSeleccionadoVFP.doc) {
                        $fila.addClass('fila-deshabilitada');
                    }
                });
            }
        } else {
            const seleccionados = $('#tbodyVerFacturasPendientes input:checked').length;
            if (seleccionados === 0) {
                clienteSeleccionadoVFP = null;
                $('#tbodyVerFacturasPendientes tr').removeClass('fila-deshabilitada');
                $('#chkSeleccionarTodoVFP').prop('checked', false);
            }
        }

        calcularTotalSeleccionadoVFP();
    });

    // Evento para el botón de cobrar en el modal VFP
    $('#btnCobrarSeleccionVFP').on('click', function () {
        iniciarCobranzaDesdeVFP();
    });

    // Evento para el botón de limpiar selección en el modal VFP
    $('#btnLimpiarSeleccionVFP').on('click', function () {
        clienteSeleccionadoVFP = null;
        $('#tbodyVerFacturasPendientes tr').removeClass('fila-deshabilitada');
        $('#tbodyVerFacturasPendientes input[type="checkbox"]').prop('checked', false);
        $('#chkSeleccionarTodoVFP').prop('checked', false);
        calcularTotalSeleccionadoVFP();
    });

    // ✅ NUEVO v3.1: Manejador del botón de salida
    $('#btnSalirFacturacion').off('click').on('click', function () {
        console.log('🚪 Usuario solicitó salir al menú principal desde Cobranza Diferida...');
        window.location.href = MenuCajaUrl;
    });
});

/**
 * ✅ NUEVO v3.1: Inicializa el módulo mostrando el modal de identificación
 */
function inicializarModuloConModal() {
    console.log('═══════════════════════════════════════════════════');
    console.log('⚙️ INICIALIZAR MÓDULO CON MODAL v3.1');
    console.log('═══════════════════════════════════════════════════');

    // ❶ VERIFICAR SI HUBO ERROR AL CARGAR DATOS
    if (typeof HUBO_ERROR !== 'undefined' && HUBO_ERROR === true) {
        console.error('❌ Hubo un error al cargar las facturas desde el servidor');
        console.error(`   Mensaje: ${MENSAJE_ERROR}`);

        $('#mensajeInicialTexto').text(MENSAJE_ERROR || 'No se pudieron cargar las facturas pendientes.');
        $('#mensajeInicial').fadeIn(500);
        return;
    }

    // ❷ VERIFICAR SI HAY FACTURAS
    if (typeof TIENE_FACTURAS !== 'undefined' && TIENE_FACTURAS === false) {
        console.warn('⚠️ No hay facturas pendientes de cobro');

        $('#mensajeInicialTexto').text('No hay facturas pendientes de cobro en este momento.');
        $('#mensajeInicial').fadeIn(500);
        return;
    }

    // ❸ VERIFICAR QUE LOS DATOS EXISTAN
    if (typeof FACTURAS_PENDIENTES === 'undefined' || !Array.isArray(FACTURAS_PENDIENTES)) {
        console.error('❌ FACTURAS_PENDIENTES no está definida o no es un array');

        $('#mensajeInicialTexto').text('Error al cargar los datos. Por favor, recargue la página.');
        $('#mensajeInicial').fadeIn(500);
        return;
    }

    console.log(`   ✅ Facturas en memoria: ${FACTURAS_PENDIENTES.length}`);

    // ❹ ABRIR EL MODAL DE IDENTIFICACIÓN
    // El usuario decidirá si busca un cliente o ve todas las facturas
    setTimeout(() => {
        console.log('   📂 Abriendo modal de identificación de cliente...');
        inicializaVistaFact(); // Función de fact.js que inicializa el modal
    }, 300);

    console.log('═══════════════════════════════════════════════════');
}

/**
 * ✅ NUEVO v3.1: Obtiene facturas de un cliente específico desde la sesión del servidor.
 * NO hace consulta a la base de datos, solo FILTRA desde FacturasPendientesActuales.
 * 
 * @param {object} cliente - Objeto con datos del cliente confirmado
 */
function obtenerFacturasDeClienteDesdeMemoria(cliente) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔍 OBTENER FACTURAS DE CLIENTE (DESDE MEMORIA) v3.1');
    console.log(`   Cliente: ${cliente.denominacion}`);
    console.log(`   ID: ${cliente.id}`);
    console.log('═══════════════════════════════════════════════════');

    mostrarLoader('Buscando facturas del cliente...');

    $.ajax({
        url: ObtenerFacturasClienteDesdeSesionUrl,
        type: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify(cliente.id), // ✅ Enviamos solo el ID del cliente
        success: function (response) {
            ocultarLoader();
            console.log('   📥 Respuesta del servidor:', response);

            if (!response || !response.ok) {
                const mensajeError = response?.mensaje || "No se encontraron facturas para este cliente.";
                console.warn('⚠️ Sin facturas para el cliente:', mensajeError);
                AbrirMensaje("Información", mensajeError, null, false, ["Aceptar"], "info");

                // Volver a abrir el modal de identificación
                setTimeout(() => $('#modalIdentificarCliente').modal('show'), 500);
                return;
            }

            const facturasDelCliente = response.lista;
            console.log(`   ✅ Facturas encontradas: ${facturasDelCliente.length}`);

            // Mostrar modal con facturas del cliente específico
            mostrarModalFacturasPendientes(cliente, facturasDelCliente);
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

            AbrirMensaje("Error de Comunicación", mensajeError, null, false, ["Aceptar"], "error");

            // Volver a abrir el modal de identificación
            setTimeout(() => $('#modalIdentificarCliente').modal('show'), 500);
        }
    });

    console.log('═══════════════════════════════════════════════════');
}

/**
 * ✅ CORREGIDO v5.0: Muestra el modal con todas las facturas pendientes.
 * Ahora garantiza que los data-* attributes tengan valores válidos.
 */
function mostrarModalVerFacturasPendientes(facturas) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📋 MOSTRAR MODAL VER FACTURAS PENDIENTES v5.0');
    console.log(`   Total facturas recibidas: ${facturas.length}`);
    console.log('═══════════════════════════════════════════════════');

    const $tbody = $('#tbodyVerFacturasPendientes');
    $tbody.empty();

    let facturasAgregadas = 0;

    // ✅ HELPER: Sanitizar valores para data-attributes
    const sanitizarData = (valor) => {
        if (valor === null || valor === undefined) return '';
        return String(valor).trim();
    };

    facturas.forEach((factura, index) => {
        try {
            // Validación básica
            if (!factura.tco_id || !factura.cm_compte) {
                console.warn(`⚠️ Factura ${index} sin datos críticos, omitiendo:`, factura);
                return;
            }

            const fecha = factura.cv_fecha_vto
                ? new Date(factura.cv_fecha_vto).toLocaleDateString('es-AR')
                : 'N/A';

            const importe = parseFloat(factura.cv_importe || 0);
            const clienteId = sanitizarData(factura.cta_id) || 'CF';
            const clienteDoc = sanitizarData(factura.cta_documento) || '';
            const nombreCliente = sanitizarData(factura.cta_denominacion || factura.co_pd_nombre) || 'Cliente sin nombre';

            // ✅ CORRECCIÓN: Sanitizar TODOS los data-* attributes
            const fila = `
                <tr data-importe="${importe}" 
                    data-cta-id="${clienteId}" 
                    data-cta-denominacion="${sanitizarData(factura.cta_denominacion || factura.co_pd_nombre)}"
                    data-cta-nombre="${sanitizarData(factura.cta_nombre)}"
                    data-cta-apellido="${sanitizarData(factura.cta_apellido)}"
                    data-cta-domicilio="${sanitizarData(factura.cta_domicilio)}"
                    data-cta-celu="${sanitizarData(factura.cta_celu)}"
                    data-cta-email="${sanitizarData(factura.cta_email)}"
                    data-tdoc-id="${sanitizarData(factura.tdoc_id)}"
                    data-tdoc-desc="${sanitizarData(factura.tdoc_desc)}"
                    data-cta-documento="${clienteDoc}"
                    data-afip-id="${sanitizarData(factura.afip_id)}"
                    data-afip-desc="${sanitizarData(factura.afip_desc)}"
                    data-lp-id="${sanitizarData(factura.lp_id)}"
                    data-ctc-desc="${sanitizarData(factura.ctc_desc)}"
                    data-valida="${sanitizarData(factura.valida)}">
                    <td>${factura.tco_id || 'N/A'}</td>
                    <td>${factura.cm_compte || 'N/A'}</td>
                    <td>${nombreCliente}</td>
                    <td class="text-center">${fecha}</td>
                    <td class="text-end fw-bold">${formatearNumero(importe, 2)}</td>
                    <td class="text-center">
                        <input type="checkbox" 
                               class="form-check-input" 
                               data-cliente-id="${clienteId}" 
                               data-cliente-doc="${clienteDoc}"
                               data-co-pd-nombre="${sanitizarData(factura.co_pd_nombre)}"
                               data-co-pd-doc="${sanitizarData(factura.co_pd_doc)}"
                               data-dia-movi="${sanitizarData(factura.dia_movi)}"
                               data-tco-id="${sanitizarData(factura.tco_id)}" 
                               data-cm-compte="${sanitizarData(factura.cm_compte)}" 
                               data-cm-compte-cuota="${factura.cm_compte_cuota || 0}"
                               data-cv-fecha-vto="${sanitizarData(factura.cv_fecha_vto)}"
                               data-cv-importe="${factura.cv_importe || 0}"
                               data-cv-importe-ori="${factura.cv_importe_ori || 0}"
                               data-cv-concepto="${sanitizarData(factura.cv_concepto)}"
                               data-ve-id="${factura.ve_id || ''}"
                               data-ccb-id="${factura.ccb_id || ''}"
                               data-ctacte="${sanitizarData(factura.ctacte)}"
                               data-carga="${sanitizarData(factura.carga)}"
                               data-carga-obligatoria="${sanitizarData(factura.carga_obligatoria)}">
                    </td>
                </tr>
            `;
            $tbody.append(fila);
            facturasAgregadas++;

            // ✅ NUEVO: Log de muestra para debugging (solo primera factura)
            if (index === 0) {
                console.log('   🔍 Muestra de data-* de primera factura:');
                console.log(`      data-afip-desc: "${sanitizarData(factura.afip_desc)}"`);
                console.log(`      data-ctc-desc: "${sanitizarData(factura.ctc_desc)}"`);
                console.log(`      data-tdoc-desc: "${sanitizarData(factura.tdoc_desc)}"`);
                console.log(`      data-cta-documento: "${clienteDoc}"`);
            }

        } catch (error) {
            console.error(`❌ Error al procesar factura ${index}:`, error, factura);
        }
    });

    console.log(`   ✅ Facturas agregadas al DOM: ${facturasAgregadas}`);

    // Resetear estado del modal
    clienteSeleccionadoVFP = null;
    nombreClienteVFP = '';
    $('#chkSeleccionarTodoVFP').prop('checked', false);
    calcularTotalSeleccionadoVFP();

    // Mostrar modal
    $('#modalVerFacturasPendientes').modal('show');
    console.log('═══════════════════════════════════════════════════');
}

/**
 * ✅ ACTUALIZADO v27.0: Inicia el proceso de cobranza pasando el contexto correcto.
 */
function iniciarCobranza() {
    console.log('═══════════════════════════════════════════════════');
    console.log('💰 INICIAR COBRANZA DIFERIDA v27.0');
    console.log('═══════════════════════════════════════════════════');

    if (typeof iniciarProcesoPago !== 'function') {
        console.error('❌ CRÍTICO: La función `iniciarProcesoPago` de pagoFactura.js no está disponible.');
        AbrirMensaje("Error", "El módulo de pago no está disponible. Por favor, recargue la página.", null, false, ["Aceptar"], "error");
        return;
    }

    $('#modalFacturasPendientes').modal('hide');

    const totalTexto = $('#txtTotalSeleccionado').val();
    const totalPagar = parsearNumero(totalTexto);

    console.log(`   Total a pagar extraído: ${formatearMoneda(totalPagar)}`);

    if (totalPagar <= 0) {
        console.error('❌ El total a pagar es cero o negativo.');
        AbrirMensaje("Error", "El monto a cobrar debe ser mayor a cero.", null, false, ["Aceptar"], "error");
        return;
    }

    setTimeout(() => {
        console.log('   Invocando el proceso de pago genérico para Cobranza...');
        iniciarProcesoPago({
            totalPagar: totalPagar,
            co_tipo: 'CD',
            puntoVenta: 'GECO PD',
            tituloModal: 'Cobranza Diferida',
            contextoOperacion: 'COBRANZA'
        });
    }, 500);
}

/**
 * ✅ CORREGIDO v6.0: Flujo robusto que BUSCA los datos del cliente en el servidor.
 * 
 * CAMBIO CRÍTICO v6.0:
 * - Después de guardar facturas, BUSCA el cliente en el servidor usando buscarClientePorId()
 * - Ya NO usa construirObjetoClienteDesdeFilaVFP() que tiene datos incompletos
 * - Garantiza que TODOS los datos del cliente estén disponibles
 */
function iniciarCobranzaDesdeVFP() {
    console.log('═══════════════════════════════════════════════════');
    console.log('💰 INICIAR COBRANZA (DESDE VFP) v6.0');
    console.log('═══════════════════════════════════════════════════');

    const $filasSeleccionadas = $('#tbodyVerFacturasPendientes input[type="checkbox"]:checked');

    if ($filasSeleccionadas.length === 0) {
        console.warn('⚠️ No hay facturas seleccionadas');
        AbrirMensaje("Atención", "No hay facturas seleccionadas para cobrar.", null, false, ["Aceptar"], "warning");
        return;
    }

    console.log(`   📦 Facturas seleccionadas: ${$filasSeleccionadas.length}`);

    const facturasSeleccionadas = [];
    let erroresValidacion = 0;

    // ✅ NUEVO v6.0: Guardar ID del cliente para búsqueda posterior
    let criterioClienteBusqueda = null;
    let origenCliente = null;

    $filasSeleccionadas.each(function (index) {
        const $checkbox = $(this);
        const $fila = $checkbox.closest('tr');

        try {
            // ✅ NUEVO v6.0: Capturar datos del cliente de la PRIMERA factura
            if (index === 0) {
                const clienteId = $fila.data('cta-id');
                const clienteDoc = $fila.data('cta-documento');

                // Determinar criterio de búsqueda
                if (clienteId && clienteId !== 'CF' && clienteId !== '') {
                    criterioClienteBusqueda = clienteId;
                    origenCliente = 'C'; // Cliente Registrado
                    console.log(`   📌 Cliente Registrado detectado - ID: ${criterioClienteBusqueda}`);
                } else if (clienteDoc && clienteDoc !== '') {
                    criterioClienteBusqueda = clienteDoc;
                    origenCliente = 'F'; // Consumidor Final
                    console.log(`   📌 Consumidor Final detectado - Documento: ${criterioClienteBusqueda}`);
                } else {
                    console.error('❌ No se pudo determinar criterio de búsqueda del cliente');
                }
            }

            const cv_fecha_vto_raw = $checkbox.data('cv-fecha-vto');
            const dia_movi_raw = $checkbox.data('dia-movi');

            const convertirFecha = (fecha) => {
                if (!fecha || fecha === '' || fecha === 'null') return null;
                if (typeof fecha === 'string' && fecha.match(/^\d{4}-\d{2}-\d{2}/)) {
                    return fecha;
                }
                return null;
            };

            const convertirNumero = (valor, defaultValue = 0) => {
                if (valor === null || valor === undefined || valor === '' || valor === 'null') {
                    return defaultValue;
                }
                const numero = parseFloat(valor);
                return isNaN(numero) ? defaultValue : numero;
            };

            const convertirEntero = (valor, defaultValue = 0) => {
                if (valor === null || valor === undefined || valor === '' || valor === 'null') {
                    return defaultValue;
                }
                const numero = parseInt(valor);
                return isNaN(numero) ? defaultValue : numero;
            };

            const convertirString = (valor) => {
                if (valor === null || valor === undefined || valor === 'null') return '';
                return String(valor).trim();
            };

            const dto = {
                cta_id: convertirString($fila.data('cta-id')),
                co_pd_nombre: convertirString($checkbox.data('co-pd-nombre') || $fila.find('td:eq(2)').text()),
                co_pd_doc: convertirString($checkbox.data('co-pd-doc') || $fila.data('cta-documento')),
                tco_id: convertirString($checkbox.data('tco-id')),
                cm_compte: convertirString($checkbox.data('cm-compte')),
                cm_compte_cuota: convertirEntero($checkbox.data('cm-compte-cuota'), 0),
                cv_fecha_vto: convertirFecha(cv_fecha_vto_raw),
                cv_importe: convertirNumero($checkbox.data('cv-importe') || $fila.data('importe'), 0),
                cv_importe_ori: convertirNumero($checkbox.data('cv-importe-ori') || $fila.data('importe'), 0),
                cv_concepto: convertirString($checkbox.data('cv-concepto')),
                dia_movi: convertirFecha(dia_movi_raw),
                ve_id: convertirEntero($checkbox.data('ve-id'), null),
                ccb_id: convertirEntero($checkbox.data('ccb-id'), null),
                ctacte: convertirString($checkbox.data('ctacte')),
                carga: convertirString($checkbox.data('carga')),
                carga_obligatoria: convertirString($checkbox.data('carga-obligatoria'))
            };

            if (!dto.tco_id || !dto.cm_compte) {
                console.error(`❌ Factura ${index} sin datos críticos:`, dto);
                erroresValidacion++;
                return;
            }

            facturasSeleccionadas.push(dto);
            console.log(`   ✅ Factura ${index + 1}:`, {
                comprobante: `${dto.tco_id} ${dto.cm_compte}`,
                importe: dto.cv_importe,
                cliente: dto.co_pd_nombre
            });

        } catch (error) {
            console.error(`❌ Error al procesar factura ${index}:`, error);
            erroresValidacion++;
        }
    });

    if (facturasSeleccionadas.length === 0) {
        console.error('❌ No se pudo construir ningún DTO válido');
        AbrirMensaje("Error", "No se pudieron procesar las facturas seleccionadas. Por favor, intente nuevamente.", null, false, ["Aceptar"], "error");
        return;
    }

    // ✅ NUEVO v6.0: Validar que tenemos criterio de búsqueda
    if (!criterioClienteBusqueda) {
        console.error('❌ No se pudo determinar el cliente');
        AbrirMensaje("Error", "No se pudo identificar el cliente. Por favor, intente nuevamente.", null, false, ["Aceptar"], "error");
        return;
    }

    if (erroresValidacion > 0) {
        console.warn(`⚠️ Se omitieron ${erroresValidacion} facturas con datos incompletos`);
    }

    console.log(`   📋 DTOs construidos correctamente: ${facturasSeleccionadas.length}`);

    mostrarLoader('Guardando selección de facturas...');

    $.ajax({
        url: ResguardarFacturasPendientesUrl,
        type: 'POST',
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify(facturasSeleccionadas),
        success: function (response) {
            ocultarLoader();
            console.log('   📥 Respuesta del servidor:', response);

            if (!response || !response.ok) {
                const mensajeError = response?.mensaje || "No se pudo guardar la selección de facturas.";
                console.error('❌ Error del servidor:', mensajeError);
                AbrirMensaje("Error", mensajeError, null, false, ["Aceptar"], "error");
                return;
            }

            console.log('   ✅ Facturas resguardadas en sesión del servidor.');

            // ══════════════════════════════════════════════════════════════════
            // ✅ CAMBIO CRÍTICO v6.0: BUSCAR DATOS COMPLETOS DEL CLIENTE
            // Ya NO usamos construirObjetoClienteDesdeFilaVFP()
            // ══════════════════════════════════════════════════════════════════
            console.log('═══════════════════════════════════════════════════');
            console.log('🔍 BUSCANDO DATOS COMPLETOS DEL CLIENTE v6.0');
            console.log(`   Criterio de búsqueda: ${criterioClienteBusqueda}`);
            console.log(`   Origen: ${origenCliente === 'C' ? 'Cliente Registrado' : 'Consumidor Final'}`);
            console.log('═══════════════════════════════════════════════════');

            // Cerrar modal VFP
            const $modalVFP = $('#modalVerFacturasPendientes');
            $modalVFP.modal('hide');

            // Esperar a que el modal se cierre completamente
            $modalVFP.one('hidden.bs.modal', function () {
                console.log('   🔄 Modal VFP cerrado, buscando cliente en servidor...');

                // ✅ USAR LA FUNCIÓN DE fact.js PARA BUSCAR CLIENTE COMPLETO
                buscarClienteYMostrarFacturas(criterioClienteBusqueda, facturasSeleccionadas);
            });
        },
        error: function (xhr, status, error) {
            ocultarLoader();
            console.error('❌ Error AJAX:', {
                status: xhr.status,
                statusText: xhr.statusText,
                responseText: xhr.responseText,
                error: error
            });

            let mensajeError = "No se pudo conectar con el servidor para guardar la selección.";

            if (xhr.responseJSON && xhr.responseJSON.mensaje) {
                mensajeError = xhr.responseJSON.mensaje;
            } else if (xhr.status === 400) {
                mensajeError = "Los datos enviados no son válidos. Por favor, recargue la página e intente nuevamente.";
            } else if (xhr.status === 0) {
                mensajeError = "No se pudo establecer conexión con el servidor. Verifique su conexión a internet.";
            }

            AbrirMensaje("Error de Comunicación", mensajeError, null, false, ["Aceptar"], "error");
        }
    });

    console.log('═══════════════════════════════════════════════════');
}

// ══════════════════════════════════════════════════════════════════
// ✅ NUEVA FUNCIÓN v6.0: Buscar cliente completo y mostrar modal
// ══════════════════════════════════════════════════════════════════
/**
 * ✅ NUEVA v6.0: Busca los datos completos del cliente en el servidor
 * y luego muestra el modal de facturas pendientes.
 * 
 * Esta función garantiza que TODOS los campos del cliente estén poblados
 * correctamente en el modal _facturasPendientesModal.cshtml
 * 
 * @param {string} criterioBusqueda - ID o documento del cliente
 * @param {Array} facturasSeleccionadas - Lista de facturas ya guardadas en sesión
 */
function buscarClienteYMostrarFacturas(criterioBusqueda, facturasSeleccionadas) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔍 BUSCAR CLIENTE Y MOSTRAR FACTURAS v6.0');
    console.log(`   Criterio: ${criterioBusqueda}`);
    console.log('═══════════════════════════════════════════════════');

    mostrarLoader('Cargando datos del cliente...');

    // ✅ USAR LA MISMA URL QUE fact.js
    const url = typeof BuscarClienteUrl !== 'undefined' && BuscarClienteUrl
        ? BuscarClienteUrl
        : '/Facturacion/Cliente/BuscarCliente';

    $.ajax({
        url: url,
        type: 'POST',
        data: { criterio: criterioBusqueda },
        timeout: 30000,
        success: function (response) {
            ocultarLoader();
            console.log('   📥 Respuesta del servidor:', response);

            if (!response.ok) {
                console.error('❌ Error al buscar cliente:', response.mensaje);
                AbrirMensaje("Error", response.mensaje || "No se pudo obtener los datos del cliente.", null, false, ["Aceptar"], "error");

                // Volver a abrir modal de identificación
                setTimeout(() => $('#modalIdentificarCliente').modal('show'), 500);
                return;
            }

            if (response.cantidadResultados !== 1 || !response.cliente) {
                console.error('❌ Respuesta inválida del servidor');
                AbrirMensaje("Error", "No se pudo identificar al cliente. Por favor, intente nuevamente.", null, false, ["Aceptar"], "error");

                // Volver a abrir modal de identificación
                setTimeout(() => $('#modalIdentificarCliente').modal('show'), 500);
                return;
            }

            const clienteCompleto = response.cliente;

            console.log('   ✅ Cliente encontrado correctamente');
            console.log('   📊 Datos del cliente:');
            console.log(`      Nombre: ${clienteCompleto.denominacion}`);
            console.log(`      ID: ${clienteCompleto.id}`);
            console.log(`      Documento: ${clienteCompleto.documento}`);
            console.log(`      Condición AFIP: ${clienteCompleto.condicionAfip}`);
            console.log(`      Tipo Documento: ${clienteCompleto.tdocDesc}`);
            console.log(`      Emite: ${clienteCompleto.emite}`);
            console.log(`      Email: ${clienteCompleto.email}`);
            console.log(`      Móvil: ${clienteCompleto.movil}`);

            // ✅ VALIDAR QUE LOS CAMPOS CRÍTICOS ESTÉN PRESENTES
            const camposVacios = [];
            if (!clienteCompleto.condicionAfip) camposVacios.push('condicionAfip');
            if (!clienteCompleto.emite) camposVacios.push('emite');
            if (!clienteCompleto.tdocDesc) camposVacios.push('tdocDesc');

            if (camposVacios.length > 0) {
                console.warn(`   ⚠️ Campos vacíos en respuesta del servidor: ${camposVacios.join(', ')}`);
            }

            // ✅ MOSTRAR MODAL CON DATOS COMPLETOS
            mostrarModalFacturasPendientes(clienteCompleto, facturasSeleccionadas);

            console.log('   ✅ Modal de facturas pendientes mostrado con datos completos');
            console.log('═══════════════════════════════════════════════════');
        },
        error: function (xhr, status, error) {
            ocultarLoader();
            console.error('❌ Error AJAX al buscar cliente:', {
                status: xhr.status,
                statusText: xhr.statusText,
                error: error
            });

            // ✅ Usar función centralizada de sesión expirada (si está disponible)
            if (typeof esSesionExpirada === 'function' && esSesionExpirada(xhr.status)) {
                if (typeof manejarSesionExpirada === 'function') {
                    manejarSesionExpirada();
                    return;
                }
            }

            let mensajeError = 'Error al buscar los datos del cliente';
            if (status === 'timeout') {
                mensajeError = 'La búsqueda tardó demasiado tiempo. Intente nuevamente.';
            } else if (xhr.status === 404) {
                mensajeError = 'Servicio no encontrado';
            } else if (xhr.status === 500) {
                mensajeError = 'Error interno del servidor';
            }

            AbrirMensaje("Error de Búsqueda", mensajeError, null, false, ["Aceptar"], "error");

            // Volver a abrir modal de identificación
            setTimeout(() => $('#modalIdentificarCliente').modal('show'), 500);
        }
    });
}

// ═══════════════════════════════════════════════════════════
// FUNCIONES AUXILIARES (sin cambios significativos)
// ═══════════════════════════════════════════════════════════

/**
 * ✅ CORREGIDO v3.0: Muestra el modal con las facturas pendientes y puebla los datos del cliente.
 * Ahora maneja correctamente valores vacíos con múltiples fallbacks.
 * 
 * @param {object} cliente - Datos completos del cliente
 * @param {Array} facturas - Lista de facturas pendientes (opcional, se recupera de sesión si no se provee)
 */
function mostrarModalFacturasPendientes(cliente, facturas = null) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📋 MOSTRAR MODAL FACTURAS PENDIENTES v3.0');
    console.log('   Cliente:', cliente.denominacion);
    console.log('   Facturas provistas:', facturas ? facturas.length : 'Se recuperarán de sesión');
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

    // ❶ POBLAR DATOS DEL CLIENTE EN EL MODAL CON FALLBACKS MÚLTIPLES
    $('#txtNombrePendiente').val(
        primerValorNoVacio(cliente.denominacion, cliente.cta_denominacion, cliente.nombre)
    );

    $('#txtClienteIdPendiente').val(
        primerValorNoVacio(cliente.id, cliente.cta_id, 'N/A')
    );

    $('#txtDomicilioPendiente').val(
        primerValorNoVacio(cliente.domicilio, cliente.cta_domicilio)
    );

    // ✅ CORRECCIÓN CRÍTICA: Condición AFIP con múltiples fallbacks
    $('#txtCondicionAfipPendiente').val(
        primerValorNoVacio(cliente.condicionAfip, cliente.afip_desc, cliente.afip_id)
    );

    // ✅ CORRECCIÓN CRÍTICA: Tipo/Número con construcción dinámica si es necesario
    let tipoNumeroFinal = primerValorNoVacio(cliente.tipoNumero);
    if (!tipoNumeroFinal && cliente.tdoc_desc && cliente.documento) {
        tipoNumeroFinal = `${cliente.tdoc_desc} ${cliente.documento}`;
    } else if (!tipoNumeroFinal && (cliente.tdoc_desc || cliente.documento)) {
        tipoNumeroFinal = cliente.tdoc_desc || cliente.documento;
    }
    $('#txtTipoNumeroPendiente').val(tipoNumeroFinal);

    $('#txtEmailPendiente').val(
        primerValorNoVacio(cliente.email, cliente.cta_email)
    );

    $('#txtMovilPendiente').val(
        primerValorNoVacio(cliente.movil, cliente.cta_celu)
    );

    // ✅ NUEVO: Log de valores asignados para debugging
    console.log('   📝 Valores asignados al modal:');
    console.log(`      Nombre: "${$('#txtNombrePendiente').val()}"`);
    console.log(`      ID: "${$('#txtClienteIdPendiente').val()}"`);
    console.log(`      Domicilio: "${$('#txtDomicilioPendiente').val()}"`);
    console.log(`      Condición AFIP: "${$('#txtCondicionAfipPendiente').val()}"`);
    console.log(`      Tipo/Número: "${$('#txtTipoNumeroPendiente').val()}"`);
    console.log(`      Email: "${$('#txtEmailPendiente').val()}"`);
    console.log(`      Móvil: "${$('#txtMovilPendiente').val()}"`);

    // ❷ FUNCIÓN INTERNA PARA POBLAR LA GRILLA (sin cambios)
    const poblarGrillaFacturas = (listaFacturas) => {
        console.log('   🔄 Poblando grilla con facturas:', listaFacturas.length);

        const $tbody = $('#tbodyFacturasPendientes');
        $tbody.empty();

        if (!Array.isArray(listaFacturas) || listaFacturas.length === 0) {
            console.warn('⚠️ Lista de facturas vacía o inválida');
            $tbody.append('<tr><td colspan="6" class="text-center text-muted py-4">No hay facturas pendientes</td></tr>');
            $('#modalFacturasPendientes').modal('show');
            return;
        }

        listaFacturas.forEach((factura, index) => {
            try {
                const fecha = factura.cv_fecha_vto
                    ? new Date(factura.cv_fecha_vto).toLocaleDateString('es-AR')
                    : 'N/A';

                const importe = parseFloat(factura.cv_importe || 0);

                const fila = `
                    <tr data-importe="${importe}">
                        <td>${factura.tco_id || 'N/A'}</td>
                        <td>${factura.cm_compte || 'N/A'}</td>
                        <td>${factura.co_pd_nombre || cliente.denominacion || 'N/A'}</td>
                        <td class="text-center">${fecha}</td>
                        <td class="text-end fw-bold">${formatearNumero(importe, 2)}</td>
                        <td class="text-center">
                            <input type="checkbox" 
                                   class="form-check-input" 
                                   data-tco-id="${factura.tco_id || ''}" 
                                   data-cm-compte="${factura.cm_compte || ''}" 
                                   data-cm-compte-cuota="${factura.cm_compte_cuota || 0}">
                        </td>
                    </tr>
                `;
                $tbody.append(fila);
            } catch (error) {
                console.error(`❌ Error al renderizar factura ${index}:`, error, factura);
            }
        });

        // Resetear controles
        $('#chkSeleccionarTodo').prop('checked', false);
        calcularTotalSeleccionado();

        // Mostrar modal
        $('#modalFacturasPendientes').modal('show');
        console.log('   ✅ Modal de facturas pendientes mostrado correctamente');
    };

    // ❸ DECIDIR FUENTE DE DATOS (sin cambios en la lógica)
    if (facturas && Array.isArray(facturas) && facturas.length > 0) {
        console.log('   📌 Usando facturas provistas por parámetro');
        poblarGrillaFacturas(facturas);
    } else {
        console.log('   📡 Recuperando facturas desde sesión del servidor...');
        mostrarLoader('Cargando facturas del cliente...');

        $.ajax({
            url: ObtenerFacturasPendientesSesionUrl,
            type: 'POST',
            success: function (response) {
                ocultarLoader();
                console.log('   📥 Respuesta de sesión:', response);

                if (response.ok && response.lista && response.lista.length > 0) {
                    poblarGrillaFacturas(response.lista);
                } else {
                    console.warn('⚠️ No se encontraron facturas en la sesión');
                    AbrirMensaje("Información", "No se encontraron facturas pendientes para este cliente.", null, false, ["Aceptar"], "info");
                }
            },
            error: function (xhr, status, error) {
                ocultarLoader();
                console.error('❌ Error al recuperar facturas de sesión:', error);
                AbrirMensaje("Error de Comunicación", "No se pudieron recuperar las facturas del cliente.", null, false, ["Aceptar"], "error");
            }
        });
    }

    console.log('═══════════════════════════════════════════════════');
}

function calcularTotalSeleccionadoVFP() {
    let total = 0;
    const $filasSeleccionadas = $('#tbodyVerFacturasPendientes input[type="checkbox"]:checked');

    if ($filasSeleccionadas.length > 0) {
        const $primeraFila = $filasSeleccionadas.first().closest('tr');
        const ctaDenominacion = $primeraFila.data('cta-denominacion');
        const textoColumna = $primeraFila.find('td:eq(2)').text().trim();
        nombreClienteVFP = ctaDenominacion || textoColumna || 'Cliente no identificado';
    } else {
        nombreClienteVFP = '';
    }

    $filasSeleccionadas.each(function () {
        const $fila = $(this).closest('tr');
        const importe = parseFloat($fila.data('importe') || 0);
        total += importe;
    });

    $('#txtTotalSeleccionadoVFP').val(`$ ${formatearNumero(total, 2)}`);
    $('#btnCobrarSeleccionVFP').prop('disabled', total === 0);

    const $spanCliente = $('#spanClienteSeleccionado');
    if (nombreClienteVFP) {
        $spanCliente.text(nombreClienteVFP).fadeIn(300);
    } else {
        $spanCliente.fadeOut(200).text('');
    }
}

function calcularTotalSeleccionado() {
    let total = 0;
    $('#tbodyFacturasPendientes tr').each(function () {
        const $checkbox = $(this).find('input[type="checkbox"]');
        if ($checkbox.is(':checked')) {
            total += parseFloat($(this).data('importe') || 0);
        }
    });

    $('#txtTotalSeleccionado').val(`$ ${formatearNumero(total, 2)}`);
    $('#btnSeguirConCobranza').prop('disabled', total === 0);
}

function formatearNumero(numero, decimales = 2) {
    if (isNaN(numero)) {
        return '0.00';
    }
    return new Intl.NumberFormat('en-US', {
        minimumFractionDigits: decimales,
        maximumFractionDigits: decimales
    }).format(numero);
}

function parsearNumero(texto) {
    if (!texto || typeof texto !== 'string') {
        return 0;
    }
    const limpio = texto.replace(/[$\s,]/g, '');
    return parseFloat(limpio) || 0;
}

/**
 * ✅ CORREGIDO v2.0: Construye un objeto cliente completo desde los atributos data-* de una fila.
 * Ahora maneja correctamente valores vacíos y utiliza fallbacks apropiados.
 * 
 * @param {jQuery} $fila - Fila del DOM con los atributos data-*
 * @returns {object} Objeto cliente con todos los campos necesarios
 */
function construirObjetoClienteDesdeFilaVFP($fila) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🏗️ CONSTRUIR OBJETO CLIENTE DESDE FILA VFP v2.1 - DEBUG');
    console.log('═══════════════════════════════════════════════════');

    // ✅ HELPER: Obtener valor o cadena vacía
    const obtenerValor = (valor) => {
        if (valor === null || valor === undefined || valor === 'null' || valor === 'undefined') {
            return '';
        }
        return String(valor).trim();
    };

    // ✅ NUEVO: Extraer TODOS los atributos data-* para debugging
    const datosRaw = {
        'cta-id': $fila.data('cta-id'),
        'cta-denominacion': $fila.data('cta-denominacion'),
        'cta-nombre': $fila.data('cta-nombre'),
        'cta-apellido': $fila.data('cta-apellido'),
        'cta-domicilio': $fila.data('cta-domicilio'),
        'cta-celu': $fila.data('cta-celu'),
        'cta-email': $fila.data('cta-email'),
        'tdoc-id': $fila.data('tdoc-id'),
        'tdoc-desc': $fila.data('tdoc-desc'),
        'cta-documento': $fila.data('cta-documento'),
        'afip-id': $fila.data('afip-id'),
        'afip-desc': $fila.data('afip-desc'),
        'lp-id': $fila.data('lp-id'),
        'ctc-desc': $fila.data('ctc-desc'),
        'valida': $fila.data('valida')
    };

    console.log('   🔍 DATOS RAW COMPLETOS (antes de sanitizar):');
    console.table(datosRaw);

    // Extraer datos procesados
    const clienteId = obtenerValor($fila.data('cta-id'));
    const clienteDoc = obtenerValor($fila.data('cta-documento'));
    const tdocId = obtenerValor($fila.data('tdoc-id'));
    const tdocDesc = obtenerValor($fila.data('tdoc-desc'));
    const denominacion = obtenerValor($fila.data('cta-denominacion'));
    const afipId = obtenerValor($fila.data('afip-id'));
    const afipDesc = obtenerValor($fila.data('afip-desc'));
    const ctcDesc = obtenerValor($fila.data('ctc-desc'));

    console.log('   📊 Datos procesados:');
    console.log(`      clienteId: "${clienteId}"`);
    console.log(`      clienteDoc: "${clienteDoc}"`);
    console.log(`      tdocDesc: "${tdocDesc}"`);
    console.log(`      afipDesc: "${afipDesc}"`);
    console.log(`      ctcDesc: "${ctcDesc}"`);

    // ✅ NUEVO: Validar si los campos críticos están vacíos
    const camposVacios = [];
    if (!tdocDesc) camposVacios.push('tdoc_desc');
    if (!afipDesc) camposVacios.push('afip_desc');
    if (!ctcDesc) camposVacios.push('ctc_desc');

    if (camposVacios.length > 0) {
        console.warn(`   ⚠️ CAMPOS VACÍOS DETECTADOS: ${camposVacios.join(', ')}`);
        console.warn('   💡 Esto indica que el servidor no envió estos datos correctamente');
    }

    // ✅ CONSTRUCCIÓN CON VALIDACIONES
    const cliente = {
        // Identificadores
        id: clienteId || 'CF',
        cta_id: clienteId || 'CF',
        documento: clienteDoc,
        cta_documento: clienteDoc,

        // Tipo de documento
        tdoc_id: tdocId,
        tdoc_desc: tdocDesc,
        tipoNumero: (tdocDesc && clienteDoc) ? `${tdocDesc} ${clienteDoc}` : (tdocDesc || clienteDoc || ''),

        // Origen (F = Consumidor Final, C = Cliente Registrado)
        origen: (clienteId === 'CF' || !clienteId) ? 'F' : 'C',

        // Nombres
        denominacion: denominacion || 'Sin nombre',
        cta_denominacion: denominacion || 'Sin nombre',
        nombre: obtenerValor($fila.data('cta-nombre')),
        apellido: obtenerValor($fila.data('cta-apellido')),

        // Contacto y Ubicación
        domicilio: obtenerValor($fila.data('cta-domicilio')),
        cta_domicilio: obtenerValor($fila.data('cta-domicilio')),
        movil: obtenerValor($fila.data('cta-celu')),
        cta_celu: obtenerValor($fila.data('cta-celu')),
        email: obtenerValor($fila.data('cta-email')),
        cta_email: obtenerValor($fila.data('cta-email')),

        // Fiscal y Negocio
        afip_id: afipId,
        afip_desc: afipDesc,
        condicionAfip: afipDesc,

        lp_id: obtenerValor($fila.data('lp-id')),

        ctc_desc: ctcDesc,
        emite: ctcDesc,

        // Estado
        valida: obtenerValor($fila.data('valida'))
    };

    console.log('   ✅ Objeto cliente construido:');
    console.log(`      denominacion: "${cliente.denominacion}"`);
    console.log(`      tipoNumero: "${cliente.tipoNumero}"`);
    console.log(`      condicionAfip: "${cliente.condicionAfip}"`);
    console.log(`      emite: "${cliente.emite}"`);
    console.log('═══════════════════════════════════════════════════');

    return cliente;
}