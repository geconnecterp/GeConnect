// ════════════════════════════════════════════════════════════
// GESTOR DE MODAL DE CÁLCULO DE FACTURA
// ════════════════════════════════════════════════════════════
// VERSIÓN v6.0 - Integración completa con prodfact.js
// ════════════════════════════════════════════════════════════

// ════════════════════════════════════════════════════════════
// INICIALIZACIÓN DEL MODAL
// ════════════════════════════════════════════════════════════
$(function () {
    console.log('📊 Modal de Cálculo de Factura inicializado v6.0');
    inicializarEventosCalculoFactura();
    inicializarProteccionCierreCalculoFactura(); // ✅ NUEVO
});

// ════════════════════════════════════════════════════════════
// EVENTOS
// ════════════════════════════════════════════════════════════
function inicializarEventosCalculoFactura() {
    console.log('🔧 Configurando eventos del modal de cálculo...');
    
    // Botón VOLVER
    $('#btnVolverCalculoFactura').on('click', function () {
        console.log('🔙 Volver a carga de productos');
        cerrarModalCalculoFactura();
    });

    // Botón DIFERIR PAGO (en header de tabla)
    $('#btnDiferirPago').on('click', function () {
        console.log('⏱️ Diferir pago');
        mostrarModalDiferirPago();
    });

    // Botón DIFERIR FACTURA (en footer)
    $('#btnDiferirFactura').on('click', function () {
        console.log('💾 Diferir factura completa');
        confirmarDiferirFactura();
    });

    // Botón PAGAR
    $('#btnPagarFactura').on('click', function () {
        console.log('💰 Proceder al pago');
        procesarPagoFactura();
    });
    
    console.log('✅ Eventos del modal de cálculo configurados');
}

// ════════════════════════════════════════════════════════════
// PROCESAR PAGO DE FACTURA
/**
 * ✅ ACTUALIZADO v13.1: Abre el modal de pago con validación simplificada
 * CAMBIO: Llama directamente a abrirModalPago() en lugar de PagoFactura.abrirModal()
 */
function procesarPagoFactura() {
    console.log('═══════════════════════════════════════════════════');
    console.log('💰 PROCESAR PAGO DE FACTURA v13.1');
    console.log('═══════════════════════════════════════════════════');

    // ❶ VALIDACIÓN: Verificar que la función abrirModalPago esté disponible
    console.log('🔍 Verificando disponibilidad de la función abrirModalPago...');

    if (typeof abrirModalPago !== 'function') {
        console.error('═══════════════════════════════════════════════════');
        console.error('❌ CRÍTICO: Función abrirModalPago NO está disponible');
        console.error('═══════════════════════════════════════════════════');
        console.error('Diagnóstico:');
        console.error('   1. Verificar que el archivo pagoFactura.js esté cargado');
        console.error('   2. Ruta esperada: ~/js/app/pagoFactura.js');
        console.error('   3. Revisar consola del navegador para errores de carga');
        console.error('═══════════════════════════════════════════════════');

        mostrarMensajeError('El módulo de pago no está disponible.\nPor favor, recargue la página e intente nuevamente.');
        return;
    }

    console.log('✅ Función abrirModalPago disponible');

    // ❷ Extraer el total final de la tabla
    const $tdTotalFinal = $('#tdTotalFinal');

    if ($tdTotalFinal.length === 0) {
        console.error('❌ No se encontró el elemento #tdTotalFinal en el DOM');
        mostrarMensajeError('Error: No se pudo obtener el total de la factura');
        return;
    }

    const totalFinalTexto = $tdTotalFinal.text().trim();
    const totalFinal = parseFloat(totalFinalTexto.replace(/[^\d.-]/g, '')) || 0;

    console.log(`💵 Total final extraído: $ ${totalFinal.toFixed(2)}`);

    // ❸ Validar que el total sea mayor a 0
    if (totalFinal <= 0) {
        console.warn('⚠️ Total final es $0.00 o negativo');
        mostrarMensajeAdvertencia('El total de la factura debe ser mayor a $0.00');
        return;
    }

    // ❹ Preparar datos para el modal de pago
    const datosPago = {
        totales: {
            totalPagar: totalFinal,
            recargos: 0,
            descuentos: 0,
            totalValores: 0
        },
        puntoVenta: $('#lblPuntoVentaCalculo').text().trim() || 'GECO PV'
    };

    console.log('📋 Datos preparados para modal de pago:', datosPago);

    // ❺ ✅ CAMBIO CRÍTICO: Llamar directamente a la función
    try {
        console.log('🔓 Invocando abrirModalPago()...');

        const resultado = abrirModalPago(datosPago);

        if (resultado === false) {
            console.error('❌ abrirModalPago() retornó false');
            mostrarMensajeError('Error al abrir el modal de pago. Revise la consola para más detalles.');
        } else {
            console.log('✅ Modal de pago abierto correctamente');
        }
    } catch (error) {
        console.error('═══════════════════════════════════════════════════');
        console.error('❌ EXCEPCIÓN AL ABRIR MODAL DE PAGO');
        console.error('═══════════════════════════════════════════════════');
        console.error('Error:', error);
        console.error('Stack:', error.stack);
        console.error('═══════════════════════════════════════════════════');

        mostrarMensajeError(`Error al abrir el modal de pago: ${error.message}`);
    }

    console.log('═══════════════════════════════════════════════════');
}

// ════════════════════════════════════════════════════════════
// ABRIR MODAL CON DATOS
// ════════════════════════════════════════════════════════════
/**
 * ✅ ACTUALIZADO v8.1: Abre modal con datos parseados
 * RENOMBRADO: abrirModalCalculoFactura → abrirModalCalculo (evitar conflicto)
 * 
 * @param {Object} datosCalculo - Objeto con subtotales, sorteos y productos parseados
 */
function abrirModalCalculo(datosCalculo) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📊 ABRIR MODAL CÁLCULO DE FACTURA v8.1');
    console.log('═══════════════════════════════════════════════════');
    console.log('Datos recibidos:', datosCalculo);

    try {
        // ❶ EXTRAER DATOS PARSEADOS (ya vienen procesados desde prodfact.js)
        const subtotales = datosCalculo.subtotales || [];
        const sorteos = datosCalculo.sorteos || [];
        const productos = datosCalculo.productos || [];

        console.log('📋 Subtotales recibidos:', subtotales);
        console.log('🎁 Sorteos recibidos:', sorteos);
        console.log('📦 Productos recibidos:', productos);

        // ❷ VALIDACIÓN: Verificar que haya subtotales
        if (!subtotales || subtotales.length === 0) {
            console.warn('⚠️ No hay subtotales para mostrar');

            AbrirMensaje(
                "Advertencia",
                "No se pudieron calcular los totales correctamente.\n\n" +
                "Por favor, intente nuevamente.",
                function () {
                    $("#msjModal").modal("hide");
                },
                false,
                ["Aceptar"],
                "warning",
                null
            );
            return;
        }

        console.log(`✅ Validación exitosa: ${subtotales.length} subtotales`);

        // ❸ ✅ HIDRATAR DATOS DEL CLIENTE EN EL HEADER
        hidratarDatosClienteCalculo();

        // ❹ CARGAR CONCEPTOS EN LA TABLA
        cargarConceptosCalculoFactura(subtotales);

        // ❺ CARGAR SORTEOS (si existen)
        cargarSorteosCalculoFactura(sorteos);

        // ❻ ABRIR MODAL
        $('#modalCalculoFactura').modal('show');

        console.log('═══════════════════════════════════════════════════');
        console.log('✅ MODAL DE CÁLCULO ABIERTO EXITOSAMENTE');
        console.log(`   - ${subtotales.length} conceptos cargados`);
        console.log(`   - ${sorteos.length} sorteos cargados`);
        console.log(`   - ${productos.length} productos procesados`);
        console.log('═══════════════════════════════════════════════════');

    } catch (error) {
        console.error('═══════════════════════════════════════════════════');
        console.error('❌ ERROR AL ABRIR MODAL DE CÁLCULO');
        console.error('═══════════════════════════════════════════════════');
        console.error('Error:', error);
        console.error('Stack:', error.stack);
        console.error('Datos recibidos:', datosCalculo);

        AbrirMensaje(
            "Error del Sistema",
            "Error al cargar los datos de cálculo.\n\n" +
            "Por favor, intente nuevamente o contacte al administrador.",
            function () {
                $("#msjModal").modal("hide");
            },
            false,
            ["Aceptar"],
            "error!",
            null
        );
    }
}

/**
 * ✅ ACTUALIZADO v8.0: Hidrata datos del cliente en el header del modal de cálculo
 * Copia los datos desde el modal de productos (sufijo "Prod") al modal de cálculo (sufijo "Calc")
 */
function hidratarDatosClienteCalculo() {
    console.log('═══════════════════════════════════════════════════');
    console.log('📝 HIDRATAR DATOS DEL CLIENTE EN MODAL CÁLCULO');
    console.log('═══════════════════════════════════════════════════');

    // ❶ Mapeo de IDs: Origen (Prod) → Destino (Calc)
    const mapeoIds = {
        'txtClienteNombreProd': 'txtClienteNombreCalc',
        'txtClienteIdProd': 'txtClienteIdCalc',
        'txtClienteDomicilioProd': 'txtClienteDomicilioCalc',
        'txtCondicionAfipProd': 'txtCondicionAfipCalc',
        'txtClienteCuitProd': 'txtClienteCuitCalc',
        'txtClienteEmailProd': 'txtClienteEmailCalc',
        'txtClienteMovilProd': 'txtClienteMovilCalc'
    };

    // ❷ Copiar valores de cada campo
    Object.keys(mapeoIds).forEach(function (idOrigen) {
        const idDestino = mapeoIds[idOrigen];
        const valorOrigen = $(`#${idOrigen}`).val() || '';

        $(`#${idDestino}`).val(valorOrigen);

        console.log(`   ✅ ${idOrigen} → ${idDestino}: "${valorOrigen}"`);
    });

    // ❸ Actualizar badge de tipo de comprobante
    const badgeOrigenHtml = $('#badgeTipoComprobante').html();
    $('#badgeTipoComprobanteCalc').html(badgeOrigenHtml);

    console.log('═══════════════════════════════════════════════════');
    console.log('✅ Datos del cliente hidratados correctamente');
    console.log('═══════════════════════════════════════════════════');
}

/**
 * ✅ NUEVO v6.0: Parsea JSON de forma segura
 * 
 * @param {string} jsonString - String JSON a parsear
 * @param {string} descripcion - Descripción para logs
 * @returns {Array|Object|null} - Datos parseados o null si falla
 */
function parsearJSON(jsonString, descripcion) {
    try {
        if (!jsonString || jsonString.trim() === '') {
            console.warn(`⚠️ ${descripcion}: JSON vacío o nulo`);
            return [];
        }
        
        const datos = JSON.parse(jsonString);
        console.log(`✅ ${descripcion} parseado exitosamente:`, datos);
        
        return datos;
    } catch (error) {
        console.error(`❌ Error parseando ${descripcion}:`, error);
        console.error(`   JSON recibido: "${jsonString}"`);
        
        return [];
    }
}

// ════════════════════════════════════════════════════════════
// CARGAR CONCEPTOS EN TABLA
// ════════════════════════════════════════════════════════════
/**
 * ✅ ACTUALIZADO v7.0: Carga conceptos en tabla con cálculo CORRECTO del total
 * 
 * CAMBIOS CRÍTICOS:
 * - Usa campo "importe" como fuente principal (prioridad sobre "total")
 * - Suma TODOS los importes sin excepciones
 * - El total del footer se calcula INDEPENDIENTEMENTE de las descripciones
 * 
 * Detecta filas de SUBTOTAL y TOTAL para aplicar estilos (solo visual)
 */
function cargarConceptosCalculoFactura(subtotales) {
    console.log('═══════════════════════════════════════════════════');
    console.log('📝 CARGAR CONCEPTOS EN TABLA v7.0');
    console.log('═══════════════════════════════════════════════════');

    const $tbody = $('#tbodyConceptosCalculo');
    $tbody.empty();

    let totalFinal = 0;

    // ❶ Validar que haya datos
    if (!subtotales || subtotales.length === 0) {
        console.warn('⚠️ No hay conceptos para mostrar');
        $tbody.html(`
            <tr>
                <td colspan="2" class="text-center text-muted py-4">
                    <i class='bx bx-error-circle'></i> No hay conceptos para mostrar
                </td>
            </tr>
        `);
        $('#tdTotalFinal').text('$ 0.00');
        return;
    }

    console.log(`📊 Total de conceptos a procesar: ${subtotales.length}`);

    // ❷ Recorrer cada subtotal y generar filas
    subtotales.forEach(function (concepto, index) {
        // ✅ CORREGIDO: Priorizar "importe" sobre "total"
        const descripcion = concepto.concepto || concepto.descripcion || 'Sin descripción';
        const importe = parseFloat(concepto.importe || concepto.total || 0);

        console.log(`   [${index}] ${descripcion}: $ ${importe.toFixed(2)}`);

        // ✅ CRÍTICO: Sumar TODOS los importes sin excepción
        totalFinal += importe;

        // ❸ Determinar clases especiales (SOLO para estilos visuales)
        let rowClass = '';
        const descripcionUpper = descripcion.toUpperCase();

        if (descripcionUpper.includes('SUBTOTAL')) {
            rowClass = 'row-subtotal';
        } else if (descripcionUpper.includes('TOTAL')) {
            rowClass = 'row-total';
        }

        // ❹ Generar HTML de la fila
        const row = `
            <tr class="${rowClass}">
                <td class="text-start">${escapeHtml(descripcion)}</td>
                <td class="text-end">$ ${formatearNumero(importe, 2)}</td>
            </tr>
        `;

        $tbody.append(row);
    });

    // ❺ Actualizar el total final en el footer
    $('#tdTotalFinal').text(`$ ${formatearNumero(totalFinal, 2)}`);

    console.log('═══════════════════════════════════════════════════');
    console.log(`✅ ${subtotales.length} conceptos cargados`);
    console.log(`💰 TOTAL FINAL CALCULADO: $ ${formatearNumero(totalFinal, 2)}`);
    console.log('═══════════════════════════════════════════════════');
}

// ════════════════════════════════════════════════════════════
// CARGAR SORTEOS
// ════════════════════════════════════════════════════════════
/**
 * ✅ ACTUALIZADO v6.0: Carga sorteos en panel lateral
 */
function cargarSorteosCalculoFactura(sorteos) {
    console.log('🎁 Cargando sorteos...');

    const $tbody = $('#tbodySorteos');
    $tbody.empty();

    // ❶ Sin sorteos o array vacío
    if (!sorteos || sorteos.length === 0 || (sorteos.length === 1 && Object.keys(sorteos[0]).length === 0)) {
        $tbody.html(`
            <tr>
                <td class="text-center text-muted py-4">
                    <i class='bx bx-info-circle'></i> No hay sorteos disponibles
                </td>
            </tr>
        `);
        console.log('ℹ️ No hay sorteos para mostrar');
        return;
    }

    // ❷ Recorrer sorteos y generar filas
    sorteos.forEach(function (sorteo, index) {
        // Validar que el sorteo tenga datos
        if (Object.keys(sorteo).length === 0) {
            return; // Skip sorteos vacíos
        }

        const descripcion = sorteo.descripcion || sorteo.nombre || sorteo.sorteo || 'Sorteo sin nombre';
        const detalle = sorteo.detalle || sorteo.observacion || sorteo.premio || '';

        const row = `
            <tr>
                <td>
                    <div class="d-flex align-items-center">
                        <i class='bx bx-gift text-warning fs-4 me-2'></i>
                        <div>
                            <strong>${escapeHtml(descripcion)}</strong>
                            ${detalle ? `<br><small class="text-muted">${escapeHtml(detalle)}</small>` : ''}
                        </div>
                    </div>
                </td>
            </tr>
        `;

        $tbody.append(row);
    });

    console.log(`✅ ${sorteos.length} sorteos cargados`);
}

// ════════════════════════════════════════════════════════════
// CERRAR MODAL
// ════════════════════════════════════════════════════════════
/**
 * ✅ ACTUALIZADO v6.0: Cierra el modal y limpia datos
 */
function cerrarModalCalculoFactura() {
    console.log('🔙 Cerrando modal de cálculo...');
    
    // Cerrar modal
    $('#modalCalculoFactura').modal('hide');
    
    // Limpiar tablas
    $('#tbodyConceptosCalculo').empty();
    $('#tbodySorteos').empty();
    $('#tdTotalFinal').text('$ 0.00');
    
    console.log('✅ Modal de cálculo cerrado');
}

// ════════════════════════════════════════════════════════════
// FUNCIONES DE NEGOCIO - DIFERIMIENTO
// ════════════════════════════════════════════════════════════

/**
 * ✅ CORREGIDO v9.1: Confirma y ejecuta el diferimiento de factura
 * Usa funciones de mensaje de siteGen.js
 */
function confirmarDiferirFactura() {
    console.log('═══════════════════════════════════════════════════');
    console.log('💾 CONFIRMAR DIFERIR FACTURA v9.1');
    console.log('═══════════════════════════════════════════════════');

    // ❶ Confirmar acción con el cajero
    AbrirMensaje(
        "¿Diferir Factura?",
        `<div class="text-start">
            <p class="mb-2"><i class='bx bx-info-circle text-info'></i> Esta acción creará una <strong>Factura Diferida (Pre-Factura)</strong>:</p>
            <ul class="list-unstyled ms-3">
                <li class="mb-1"><i class='bx bx-check text-success'></i> Se guardará la venta temporalmente</li>
                <li class="mb-1"><i class='bx bx-x text-danger'></i> NO se afectará el stock</li>
                <li class="mb-1"><i class='bx bx-x text-danger'></i> NO se generará comprobante fiscal</li>
                <li class="mb-1"><i class='bx bx-time-five text-warning'></i> El cliente podrá volver más tarde a finalizar la compra</li>
            </ul>
        </div>`,
        function(respuesta) {
            $("#msjModal").modal("hide");
            
            if (respuesta === "SI") {
                setTimeout(() => {
                    ejecutarDiferirFactura();
                }, 300);
            }
        },
        true, // Es confirmación
        ["Sí, Diferir Factura", "Cancelar"],
        "info!",
        null
    );
}

// ════════════════════════════════════════════════════════════
// REEMPLAZAR LA FUNCIÓN ejecutarDiferirFactura (LÍNEA 357 APROX)
// ════════════════════════════════════════════════════════════

/**
 * ✅ ACTUALIZADO v12.0: Ejecuta la llamada AJAX para diferir factura CON BLOQUEO
 * NUEVO: Bloqueo completo de interfaz durante operación
 */
function ejecutarDiferirFactura() {
    console.log('═══════════════════════════════════════════════════');
    console.log('📡 EJECUTAR DIFERIR FACTURA v12.0 CON BLOQUEO');
    console.log('═══════════════════════════════════════════════════');

    // ❶ ✅ NUEVO: BLOQUEAR PANTALLA
    bloquearPantallaCalculoFactura('Creando Factura Diferida...');

    // ❷ Llamada AJAX
    $.ajax({
        url: DiferirFacturaUrl,
        type: 'POST',
        dataType: 'json',
        timeout: 30000,
        success: function (response) {
            console.log('✅ RESPUESTA DE DIFERIR FACTURA RECIBIDA');
            console.log('Response:', response);

            // ❸ ✅ DESBLOQUEAR PANTALLA
            desbloquearPantallaCalculoFactura();

            // ❹ Validar respuesta básica
            if (response.ok === false) {
                console.error('❌ Error en respuesta:', response.mensaje);

                AbrirMensaje(
                    "Error al Diferir Factura",
                    response.mensaje || 'No se pudo crear la factura diferida',
                    function () {
                        $("#msjModal").modal("hide");
                    },
                    false,
                    ["Aceptar"],
                    "error!",
                    null
                );
                return;
            }

            // ❺ Detectar formato de respuesta: Array JSON o Objeto con .ok
            let comprobantes = [];

            if (Array.isArray(response)) {
                comprobantes = response;
                console.log('📋 Formato detectado: Array JSON directo');
            } else if (response.ok === true && response.data) {
                comprobantes = Array.isArray(response.data) ? response.data : [response.data];
                console.log('📋 Formato detectado: Objeto con data');
            } else if (response.ok === true) {
                console.warn('⚠️ Respuesta OK pero sin datos de comprobantes');
                mostrarMensajeExitoGenerico(response);
                return;
            } else {
                console.error('❌ Formato de respuesta desconocido:', response);
                mostrarMensajeError('Formato de respuesta inválido');
                return;
            }

            // ❻ Validar que haya al menos un comprobante
            if (comprobantes.length === 0) {
                console.error('❌ No se recibieron comprobantes en la respuesta');
                mostrarMensajeError('No se recibió información del comprobante diferido');
                return;
            }

            // ❼ Procesar primer comprobante
            const comprobante = comprobantes[0];

            console.log('═══════════════════════════════════════════════════');
            console.log('📄 DATOS DEL COMPROBANTE DIFERIDO');
            console.log(`   tco_letra: ${comprobante.tco_letra}`);
            console.log(`   tco_id: ${comprobante.tco_id}`);
            console.log(`   cm_compte: ${comprobante.cm_compte}`);
            console.log(`   cm_repetido: ${comprobante.cm_repetido}`);
            console.log('═══════════════════════════════════════════════════');

            // ❽ Determinar tipo de comprobante
            const tipoComprobante = obtenerTipoComprobante(comprobante.tco_letra, comprobante.tco_id);
            const numeroComprobante = comprobante.cm_compte || 'Pendiente de asignación';
            const esRepetido = comprobante.cm_repetido === "1" || comprobante.cm_repetido === 1;

            console.log(`✅ Tipo de comprobante identificado: ${tipoComprobante}`);

            if (esRepetido) {
                console.warn('⚠️ Comprobante marcado como REPETIDO');
            }

            // ❾ Construir mensaje de éxito
            AbrirMensaje(
                "¡Factura Diferida Creada!",
                `<div class="text-center">
                    <div class="mb-3">
                        <i class='bx bx-check-circle text-success' style="font-size: 4rem;"></i>
                    </div>
                    <h4 class="text-golden mb-3">Factura diferida creada exitosamente</h4>
                    
                    <div class="alert alert-info mb-3">
                        <div class="mb-2">
                            <strong class="d-block text-uppercase">${tipoComprobante}</strong>
                            <span class="badge bg-primary fs-6">${comprobante.tco_letra}</span>
                        </div>
                        ${numeroComprobante !== 'Pendiente de asignación'
                    ? `<div class="mt-2">
                                 <small class="text-muted">Número:</small><br>
                                 <strong>${numeroComprobante}</strong>
                               </div>`
                    : '<div class="mt-2"><small class="text-muted">El número se asignará al momento de facturar</small></div>'}
                        ${esRepetido ? '<div class="mt-2"><span class="badge bg-warning">Comprobante Repetido</span></div>' : ''}
                    </div>
                    
                    <p class="text-muted mb-0">
                        <i class='bx bx-time-five'></i> El cliente podrá retomar esta compra más tarde
                    </p>
                </div>`,
                function () {
                    $("#msjModal").modal("hide");

                    // ═══════════════════════════════════════════════════
                    // ✅ FLUJO DE LIMPIEZA Y REINICIO
                    // ═══════════════════════════════════════════════════

                    setTimeout(() => {
                        console.log('🔄 INICIANDO REINICIO DEL MÓDULO DE VENTAS');

                        // Cerrar modal de cálculo
                        cerrarModalCalculoFactura();

                        setTimeout(() => {
                            // Limpiar venta completa
                            if (typeof limpiarVentaCompleta === 'function') {
                                limpiarVentaCompleta();
                                console.log('✅ Módulo de ventas limpiado');
                            }

                            setTimeout(() => {
                                // Abrir modal de identificar cliente
                                if (typeof abrirModalIdentificarCliente === 'function') {
                                    abrirModalIdentificarCliente();
                                    console.log('✅ Modal de identificar cliente abierto');
                                }

                                console.log('✅ REINICIO COMPLETADO');
                            }, 200);
                        }, 300);
                    }, 300);
                },
                false,
                ["Aceptar"],
                "succ!",
                null
            );
        },
        error: function (xhr, status, error) {
            console.log('═══════════════════════════════════════════════════');
            console.error('❌ ERROR EN AJAX DIFERIR FACTURA');
            console.error(`   Status: ${status}`);
            console.error(`   Error: ${error}`);
            console.error(`   HTTP Status: ${xhr.status}`);
            console.log('═══════════════════════════════════════════════════');

            // ❶ ✅ DESBLOQUEAR PANTALLA
            desbloquearPantallaCalculoFactura();

            // ❷ Verificar sesión expirada
            if (esSesionExpirada(xhr.status)) {
                manejarSesionExpirada('No se pudo diferir la factura porque su sesión ha expirado.');
                return;
            }

            // ❸ Determinar mensaje de error
            let mensajeError = 'Error de comunicación con el servidor';

            if (xhr.status === 500) {
                mensajeError = 'Error interno del servidor. Contacte al administrador.';
            } else if (xhr.status === 0) {
                mensajeError = 'No se pudo conectar con el servidor. Verifique su conexión.';
            } else if (xhr.responseJSON && xhr.responseJSON.mensaje) {
                mensajeError = xhr.responseJSON.mensaje;
            }

            AbrirMensaje(
                "Error de Comunicación",
                mensajeError,
                function () {
                    $("#msjModal").modal("hide");
                },
                false,
                ["Aceptar"],
                "error!",
                null
            );
        }
    });

    // ❿ ✅ Timeout de seguridad (30 segundos)
    setTimeout(function () {
        if ($('#overlayDiferimiento').length > 0 && $('#overlayDiferimiento').is(':visible')) {
            console.warn('⚠️ Timeout de seguridad alcanzado - Desbloqueando pantalla');
            desbloquearPantallaCalculoFactura();

            AbrirMensaje(
                "Tiempo de Espera Agotado",
                "La operación está tomando más tiempo del esperado.\n\n" +
                "Por favor, verifique el resultado en el sistema.",
                function () {
                    $("#msjModal").modal("hide");
                },
                false,
                ["Aceptar"],
                "warning",
                null
            );
        }
    }, 30000);
}

/**
 * ✅ NUEVO v10.0: Determina el tipo de comprobante según letra e ID
 * 
 * @param {string} letra - Letra del comprobante (A, B, C, etc.)
 * @param {string} id - ID del tipo de comprobante
 * @returns {string} - Descripción del tipo de comprobante
 */
function obtenerTipoComprobante(letra, id) {
    console.log(`🔍 Identificando comprobante: Letra="${letra}", ID="${id}"`);

    // Normalizar letra a mayúscula
    const letraNorm = (letra || '').toUpperCase().trim();

    // Normalizar ID (remover ceros a la izquierda para comparación)
    const idNorm = (id || '').trim().replace(/^0+/, '');

    // ═══════════════════════════════════════════════════
    // MAPEO DE TIPOS DE COMPROBANTES
    // ═══════════════════════════════════════════════════

    // Factura A (ID: 007 o 7)
    if (letraNorm === 'A' && (idNorm === '7' || id === '007')) {
        return 'Factura A';
    }

    // Factura B (ID: 006 o 6)
    if (letraNorm === 'B' && (idNorm === '6' || id === '006')) {
        return 'Factura B';
    }

    // Factura C (ID: 011 o 11) - Común en sistemas AFIP
    if (letraNorm === 'C' && (idNorm === '11' || id === '011')) {
        return 'Factura C';
    }

    // Factura M (ID: 051 o 51) - Monotributista
    if (letraNorm === 'M' && (idNorm === '51' || id === '051')) {
        return 'Factura M';
    }

    // Nota de Crédito A (ID: 008 o 8)
    if (letraNorm === 'A' && (idNorm === '8' || id === '008')) {
        return 'Nota de Crédito A';
    }

    // Nota de Crédito B (ID: 009 o 9)
    if (letraNorm === 'B' && (idNorm === '9' || id === '009')) {
        return 'Nota de Crédito B';
    }

    // Nota de Débito A (ID: 010 o 10)
    if (letraNorm === 'A' && (idNorm === '10' || id === '010')) {
        return 'Nota de Débito A';
    }

    // ═══════════════════════════════════════════════════
    // TIPO GENÉRICO (fallback)
    // ═══════════════════════════════════════════════════

    console.warn(`⚠️ Tipo de comprobante no reconocido: Letra="${letra}", ID="${id}"`);
    return `Comprobante ${letraNorm || 'Desconocido'}`;
}

/**
 * ✅ NUEVO v10.0: Muestra mensaje de éxito genérico (compatibilidad con formato anterior)
 * 
 * @param {Object} response - Respuesta del servidor
 */
function mostrarMensajeExitoGenerico(response) {
    console.log('📋 Mostrando mensaje de éxito genérico');

    AbrirMensaje(
        "¡Factura Diferida Creada!",
        `<div class="text-center">
            <div class="mb-3">
                <i class='bx bx-check-circle text-success' style="font-size: 4rem;"></i>
            </div>
            <h4 class="text-golden mb-3">${response.mensaje || 'Factura diferida creada exitosamente'}</h4>
            <p class="text-muted mb-0">El cliente podrá retomar esta compra más tarde</p>
        </div>`,
        function () {
            $("#msjModal").modal("hide");

            setTimeout(() => {
                cerrarModalCalculoFactura();

                setTimeout(() => {
                    if (typeof limpiarVentaCompleta === 'function') {
                        limpiarVentaCompleta();
                    }

                    setTimeout(() => {
                        if (typeof abrirModalIdentificarCliente === 'function') {
                            abrirModalIdentificarCliente();
                        }
                    }, 200);
                }, 300);
            }, 300);
        },
        false,
        ["Aceptar"],
        "succ!",
        null
    );
}

/**
 * ✅ NUEVO v10.0: Muestra mensaje de error genérico
 * 
 * @param {string} mensaje - Mensaje de error
 */
function mostrarMensajeError(mensaje) {
    AbrirMensaje(
        "Error al Diferir Factura",
        mensaje,
        function () {
            $("#msjModal").modal("hide");
        },
        false,
        ["Aceptar"],
        "error!",
        null
    );
}
/**
 * ✅ CORREGIDO v9.1: Modal de diferir pago con advertencias
 * Emite factura fiscal sin cobrar
 */
function mostrarModalDiferirPago() {
    console.log('═══════════════════════════════════════════════════');
    console.log('⏱️ MOSTRAR MODAL DIFERIR PAGO v9.1');
    console.log('═══════════════════════════════════════════════════');

    // ❶ Confirmar acción con ADVERTENCIAS críticas
    AbrirMensaje(
        "⚠️ ¿Diferir Pago?",
        `<div class="text-start">
            <div class="alert alert-danger mb-3">
                <i class='bx bx-error-alt'></i> <strong>OPERACIÓN DE ALTA COMPLEJIDAD</strong>
            </div>
            <p class="mb-2">Esta acción <strong>EMITIRÁ LA FACTURA FISCAL</strong> pero <strong>SIN COBRAR</strong>:</p>
            <ul class="list-unstyled ms-3">
                <li class="mb-1"><i class='bx bx-check text-success'></i> ✅ Se generará el comprobante fiscal</li>
                <li class="mb-1"><i class='bx bx-check text-success'></i> ✅ Se afectará el stock</li>
                <li class="mb-1"><i class='bx bx-check text-success'></i> ✅ Se registrará en Libro IVA Ventas</li>
                <li class="mb-1"><i class='bx bx-check text-warning'></i> ⚠️ El pago quedará PENDIENTE en cuenta del cliente</li>
                <li class="mb-1"><i class='bx bx-check text-warning'></i> ⚠️ Se imprimirá el comprobante</li>
            </ul>
            <div class="alert alert-warning mt-3 mb-0">
                <i class='bx bx-info-circle'></i> Esta operación NO puede deshacerse fácilmente
            </div>
        </div>`,
        function(respuesta) {
            $("#msjModal").modal("hide");
            
            if (respuesta === "SI") {
                setTimeout(() => {
                    ejecutarDiferirPago();
                }, 300);
            }
        },
        true, // Es confirmación
        ["Sí, Emitir Factura sin Cobrar", "Cancelar"],
        "warn!",
        null
    );
}

// ════════════════════════════════════════════════════════════
// REEMPLAZAR LA FUNCIÓN ejecutarDiferirPago (LÍNEA 661 APROX)
// ════════════════════════════════════════════════════════════

/**
 * ✅ ACTUALIZADO v12.0: Ejecuta la llamada AJAX para diferir pago CON BLOQUEO
 * NUEVO: Bloqueo completo de interfaz durante operación
 * MANTIENE: Generación de reporte antes del mensaje de éxito
 */
function ejecutarDiferirPago() {
    console.log('═══════════════════════════════════════════════════');
    console.log('📡 EJECUTAR DIFERIR PAGO v12.0 CON BLOQUEO');
    console.log('═══════════════════════════════════════════════════');

    // ❶ ✅ NUEVO: BLOQUEAR PANTALLA
    bloquearPantallaCalculoFactura('Emitiendo Factura con Pago Diferido...');

    // ❷ Llamada AJAX
    $.ajax({
        url: DiferirPagoUrl,
        type: 'POST',
        dataType: 'json',
        timeout: 30000,
        success: function (response) {
            console.log('✅ RESPUESTA DE DIFERIR PAGO RECIBIDA');
            console.log('Response:', response);

            // ❸ ⚠️ NO DESBLOQUEAR AÚN - El reporte necesita tiempo

            // ❹ Validar respuesta básica
            if (!response.ok) {
                console.error('❌ Error en respuesta:', response.mensaje);

                // ✅ DESBLOQUEAR antes de mostrar error
                desbloquearPantallaCalculoFactura();

                mostrarMensajeError(response.mensaje || 'No se pudo emitir la factura');
                return;
            }

            // ❺ Validar que response.data exista y sea un array
            if (!response.data || !Array.isArray(response.data) || response.data.length === 0) {
                console.error('❌ No se recibieron datos del comprobante');

                // ✅ DESBLOQUEAR antes de mostrar error
                desbloquearPantallaCalculoFactura();

                mostrarMensajeError('Error: No se recibió información del comprobante');
                return;
            }

            // ❻ Extraer datos del comprobante
            const comprobante = response.data[0];

            console.log('═══════════════════════════════════════════════════');
            console.log('📄 DATOS DEL COMPROBANTE EMITIDO');
            console.log(`   tco_letra: ${comprobante.tco_letra}`);
            console.log(`   tco_id: ${comprobante.tco_id}`);
            console.log(`   cm_compte: ${comprobante.cm_compte}`);
            console.log(`   cm_repetido: ${comprobante.cm_repetido}`);
            console.log('═══════════════════════════════════════════════════');

            // ❼ Determinar tipo de comprobante
            const tipoComprobante = obtenerTipoComprobante(comprobante.tco_letra, comprobante.tco_id);
            const numeroComprobante = comprobante.cm_compte || 'Sin número';
            const esRepetido = comprobante.cm_repetido === "1" || comprobante.cm_repetido === 1;

            console.log(`✅ Tipo de comprobante identificado: ${tipoComprobante}`);

            if (esRepetido) {
                console.warn('⚠️ Comprobante marcado como REPETIDO');
            }

            // ═══════════════════════════════════════════════════
            // ✅ GENERAR REPORTE PRIMERO (mantener lógica existente)
            // ═══════════════════════════════════════════════════

            console.log('📄 GENERANDO REPORTE DEL COMPROBANTE');

            // ❽ VALIDAR que ModuloReportes esté disponible
            if (typeof ModuloReportes === 'undefined') {
                console.error('❌ ModuloReportes no está disponible');

                // ✅ DESBLOQUEAR antes de mostrar error
                desbloquearPantallaCalculoFactura();

                mostrarMensajeError('Error: Módulo de reportes no cargado');
                return;
            }

            // ❾ GENERAR Y VISUALIZAR REPORTE
            ModuloReportes.generarYVisualizarReporte({
                tco_letra: comprobante.tco_letra,
                tco_id: comprobante.tco_id,
                cm_compte: comprobante.cm_compte,
                cm_repetido: comprobante.cm_repetido
            }).then(function (exitoso) {
                console.log(`📄 Generación de reporte: ${exitoso ? '✅ Exitosa' : '❌ Fallida'}`);

                // ═══════════════════════════════════════════════════
                // ✅ AHORA SÍ: DESBLOQUEAR Y MOSTRAR MENSAJE DE ÉXITO
                // ═══════════════════════════════════════════════════

                // Esperar 500ms para que el PDF se abra completamente
                setTimeout(function () {
                    // ✅ DESBLOQUEAR PANTALLA
                    desbloquearPantallaCalculoFactura();

                    // Mostrar mensaje de éxito
                    mostrarMensajeExitoDiferirPago(tipoComprobante, comprobante, numeroComprobante, esRepetido);
                }, 500);

            }).catch(function (error) {
                console.error('❌ Error al generar reporte:', error);

                // ✅ DESBLOQUEAR PANTALLA aún con error
                desbloquearPantallaCalculoFactura();

                // Aún así mostrar mensaje de éxito (la factura ya se emitió)
                mostrarMensajeExitoDiferirPago(tipoComprobante, comprobante, numeroComprobante, esRepetido);
            });
        },
        error: function (xhr, status, error) {
            console.log('═══════════════════════════════════════════════════');
            console.error('❌ ERROR EN AJAX DIFERIR PAGO');
            console.error(`   Status: ${status}`);
            console.error(`   Error: ${error}`);
            console.error(`   HTTP Status: ${xhr.status}`);
            console.log('═══════════════════════════════════════════════════');

            // ❶ ✅ DESBLOQUEAR PANTALLA
            desbloquearPantallaCalculoFactura();

            // ❷ Verificar sesión expirada
            if (esSesionExpirada(xhr.status)) {
                manejarSesionExpirada('No se pudo diferir el pago porque su sesión ha expirado.');
                return;
            }

            // ❸ Determinar mensaje de error
            let mensajeError = 'Error de comunicación con el servidor';

            if (xhr.status === 500) {
                mensajeError = 'Error interno del servidor. Contacte al administrador.';
            } else if (xhr.status === 0) {
                mensajeError = 'No se pudo conectar con el servidor. Verifique su conexión.';
            } else if (xhr.responseJSON && xhr.responseJSON.mensaje) {
                mensajeError = xhr.responseJSON.mensaje;
            }

            AbrirMensaje(
                "Error de Comunicación",
                mensajeError,
                function () {
                    $("#msjModal").modal("hide");
                },
                false,
                ["Aceptar"],
                "error!",
                null
            );
        }
    });

    // ❿ ✅ Timeout de seguridad (30 segundos)
    setTimeout(function () {
        if ($('#overlayDiferimiento').length > 0 && $('#overlayDiferimiento').is(':visible')) {
            console.warn('⚠️ Timeout de seguridad alcanzado - Desbloqueando pantalla');
            desbloquearPantallaCalculoFactura();

            AbrirMensaje(
                "Tiempo de Espera Agotado",
                "La operación está tomando más tiempo del esperado.\n\n" +
                "Por favor, verifique el resultado en el sistema.",
                function () {
                    $("#msjModal").modal("hide");
                },
                false,
                ["Aceptar"],
                "warning",
                null
            );
        }
    }, 30000);
}

/**
 * ✅ ACTUALIZADO v11.0: Muestra mensaje de éxito al diferir pago
 * Ajustado para NO mencionar "nueva pestaña" hasta después del mensaje
 */
function mostrarMensajeExitoDiferirPago(tipoComprobante, comprobante, numeroComprobante, esRepetido) {
    AbrirMensaje(
        "¡Factura Emitida!",
        `<div class="text-center">
            <div class="mb-3">
                <i class='bx bx-receipt text-success' style='font-size: 4rem;'></i>
            </div>
            <h4 class="text-golden mb-3">Factura emitida con pago diferido</h4>
            
            <div class="alert alert-info mb-3">
                <div class="mb-2">
                    <strong class="d-block text-uppercase">${tipoComprobante}</strong>
                    <span class="badge bg-primary fs-6">${comprobante.tco_letra}</span>
                </div>
                <div class="mt-2">
                    <small class="text-muted">Número:</small><br>
                    <strong>${numeroComprobante}</strong>
                </div>
                ${esRepetido ? '<div class="mt-2"><span class="badge bg-warning">Comprobante Repetido</span></div>' : ''}
            </div>
            
            <p class="text-muted mb-0">
                <i class='bx bx-check-circle'></i> El comprobante fue visualizado exitosamente
            </p>
        </div>`,
        function () {
            $("#msjModal").modal("hide");

            // ═══════════════════════════════════════════════════
            // ✅ FLUJO DE LIMPIEZA Y REINICIO
            // ═══════════════════════════════════════════════════

            setTimeout(() => {
                console.log('═══════════════════════════════════════════════════');
                console.log('🔄 INICIANDO REINICIO DEL MÓDULO DE VENTAS');
                console.log('═══════════════════════════════════════════════════');

                // ❶ PASO 1: Cerrar modal de cálculo
                cerrarModalCalculoFactura();
                console.log('✅ Paso 1: Modal de cálculo cerrado');

                // ❷ PASO 2: Esperar cierre completo del modal (300ms)
                setTimeout(() => {

                    // ❸ PASO 3: Limpiar completamente el módulo de ventas
                    if (typeof limpiarVentaCompleta === 'function') {
                        limpiarVentaCompleta();
                        console.log('✅ Paso 2: Módulo de ventas limpiado');
                    } else {
                        console.error('❌ Función limpiarVentaCompleta no existe');
                    }

                    // ❹ PASO 4: Esperar limpieza completa (200ms)
                    setTimeout(() => {

                        // ❺ PASO 5: Abrir modal de identificar cliente
                        if (typeof abrirModalIdentificarCliente === 'function') {
                            abrirModalIdentificarCliente();
                            console.log('✅ Paso 3: Modal de identificar cliente abierto');
                        } else {
                            console.error('❌ Función abrirModalIdentificarCliente no existe');
                        }

                        console.log('═══════════════════════════════════════════════════');
                        console.log('✅ REINICIO COMPLETADO - Listo para nueva venta');
                        console.log('═══════════════════════════════════════════════════');

                    }, 200); // ← Esperar limpieza

                }, 300); // ← Esperar cierre de modal

            }, 300); // ← Esperar cierre de mensaje de éxito
        },
        false,
        ["Aceptar"],
        "succ!",
        null
    );
}

/**
 * ✅ ACTUALIZADO v9.1: Imprime el comprobante de venta
 * TODO: Integrar con sistema de impresión fiscal
 * 
 * @param {Object} comprobante - Datos del comprobante a imprimir
 */
function imprimirComprobante(comprobante) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🖨️ IMPRIMIR COMPROBANTE');
    console.log('═══════════════════════════════════════════════════');
    console.log('Datos del comprobante:', comprobante);

    // TODO: Implementar según sistema de impresión fiscal
    // Opciones:
    // 1. Llamar a servicio de impresión fiscal (controlador fiscal)
    // 2. Generar PDF y enviarlo a impresora
    // 3. Enviar a servicio de facturación electrónica

    console.log('⚠️ TODO: Integrar con sistema de impresión fiscal');
    console.log(`   Comprobante: ${comprobante.letra} ${comprobante.numero}`);
    console.log(`   ID: ${comprobante.id}`);
    console.log(`   Identificador: ${comprobante.identificador_completo}`);
    console.log('═══════════════════════════════════════════════════');
    
    // Placeholder: Mostrar alerta de que se debe imprimir
    AbrirMensaje(
        "Imprimir Comprobante",
        `<div class="text-center">
            <i class='bx bx-printer' style='font-size: 3rem;'></i>
            <p class="mt-3">Comprobante ${comprobante.letra} ${comprobante.numero}</p>
            <small class="text-muted">La impresión debe ser implementada</small>
        </div>`,
        function() {
            $("#msjModal").modal("hide");
        },
        false,
        ["Aceptar"],
        "info!",
        null
    );
}

// ════════════════════════════════════════════════════════════
// AGREGAR DESPUÉS DE LA LÍNEA 28 (después de inicializarEventosCalculoFactura)
// ════════════════════════════════════════════════════════════

// ════════════════════════════════════════════════════════════
// GESTIÓN DE BLOQUEO DE PANTALLA
// ════════════════════════════════════════════════════════════

/**
 * ✅ NUEVO v12.0: Bloquea la interfaz durante operaciones de diferimiento
 * @param {string} mensaje - Mensaje a mostrar durante el bloqueo
 */
function bloquearPantallaCalculoFactura(mensaje = 'Procesando...') {
    console.log('🔒 Bloqueando pantalla de cálculo de factura...');

    // ❶ Crear overlay si no existe
    if ($('#overlayDiferimiento').length === 0) {
        const overlay = `
            <div id="overlayDiferimiento" class="loading-overlay">
                <div class="loading-content">
                    <div class="spinner-border text-golden" role="status" style="width: 3rem; height: 3rem;">
                        <span class="visually-hidden">Cargando...</span>
                    </div>
                    <p class="loading-message mt-3 mb-0 fw-bold text-golden-dark" id="mensajeDiferimiento">
                        ${mensaje}
                    </p>
                    <small class="text-muted d-block mt-2">
                        Por favor, espere un momento...
                    </small>
                </div>
            </div>
        `;
        $('body').append(overlay);
    } else {
        // �② Actualizar mensaje si ya existe
        $('#mensajeDiferimiento').text(mensaje);
        $('#overlayDiferimiento').fadeIn(200);
    }

    // ❸ Deshabilitar TODOS los botones del modal de cálculo
    $('#btnVolverCalculoFactura, #btnDiferirPago, #btnDiferirFactura, #btnPagarFactura').prop('disabled', true);

    // ❹ Prevenir cierre del modal con ESC o clic fuera
    $('#modalCalculoFactura').data('bs-keyboard', false);
    $('#modalCalculoFactura').data('bs-backdrop', 'static');

    console.log('✅ Pantalla de cálculo bloqueada');
}

/**
 * ✅ NUEVO v12.0: Desbloquea la interfaz después de completar la operación
 */
function desbloquearPantallaCalculoFactura() {
    console.log('🔓 Desbloqueando pantalla de cálculo...');

    // ❶ Remover overlay con animación
    $('#overlayDiferimiento').fadeOut(300, function () {
        $(this).remove();
    });

    // ❷ Rehabilitar botones
    $('#btnVolverCalculoFactura, #btnDiferirPago, #btnDiferirFactura, #btnPagarFactura').prop('disabled', false);

    // ❸ Restaurar cierre con ESC y backdrop
    $('#modalCalculoFactura').data('bs-keyboard', true);
    $('#modalCalculoFactura').data('bs-backdrop', true);

    console.log('✅ Pantalla de cálculo desbloqueada');
}

/**
 * ✅ NUEVO v12.0: Protección contra cierre accidental durante operación
 */
function inicializarProteccionCierreCalculoFactura() {
    $('#modalCalculoFactura').on('hide.bs.modal', function (e) {
        // Si hay un overlay activo, prevenir cierre
        if ($('#overlayDiferimiento').length > 0 && $('#overlayDiferimiento').is(':visible')) {
            console.warn('⚠️ Intento de cerrar modal durante operación - BLOQUEADO');
            e.preventDefault();
            return false;
        }
    });
}