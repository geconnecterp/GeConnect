const divs = {
    ProductoDetalle: "#divPCP",
    ProductoListas: "#divProdLista"
}
// 1. Agregar variable global para almacenar el p_id del producto actual cargado en la lista
let productoActualEnLista = null;

// REEMPLAZAR TODO EL BLOQUE DE ESTILOS EN $(function()) POR ESTO:
$(function () {
    
    // Inicializar el observador DOM para detectar cambios
    inicializarObservadorDOM();

    // Inicializar el resto de componentes
    configurarBotonesProdCP();
    cargaEventosCP();

    // Delegación de eventos para manejar la activación de campos de edición en _datosGenerales
    $(document).on('change', '#chkPLista, #chkDto1, #chkDto2, #chkDto3, #chkDto4, #chkDpo, #chkBon, #chkFl', function () {
        const campoId = $(this).attr('id').replace('chk', 'txt');
        const $campo = $('#' + campoId);

        if ($(this).is(':checked')) {
            $campo.prop('disabled', false);
            // Usar setTimeout para asegurar que la selección ocurra después de que el campo esté habilitado
            setTimeout(function () {
                $campo.trigger("focus");
                // Seleccionar el texto utilizando el método nativo selectText en lugar del método jQuery deprecado
                if ($campo[0]) {
                    $campo[0].select(); 
                }
            }, 0);
        } else {
            $campo.prop('disabled', true);
        }
    });


    // Evento para el botón Aplicar en _datosGenerales
    $(document).on('click', '#btnAplicar', function () {
        aplicarCambiosDatosGenerales();
    });

    // Evento para el botón Cancelar en _datosGenerales
    $(document).on('click', '#btnCancelar', function () {
        cancelarCambiosDatosGenerales();
    });

    // Inicializar tabla si ya está cargada
    if ($('#tbProdDet tbody tr').length > 0) {
        console.log("Detectada tabla de productos ya cargada, iniciando optimizada...");
        inicializarTablaProductos();
    }

    $(document).ajaxError(function (event, jqXHR, settings, thrownError) {
        // Verificar si el indicador de listas está presente y eliminarlo
        if ($("#listasLoadingIndicator").length > 0) {
            console.warn("Error AJAX detectado: eliminando indicador de actualización de listas");
            $("#listasLoadingIndicator").fadeOut(300, function () {
                $(this).remove();
            });
        }
    });
});

// Función para aplicar los cambios de _datosGenerales a todas las filas seleccionadas
function aplicarCambiosDatosGenerales() {
    console.log("Aplicando cambios de datos generales...");

    // Verificar que hay filas seleccionadas
    const filasSeleccionadas = $("#tbProdDet tbody tr").filter(function () {
        return $(this).find('input[type="checkbox"]').is(':checked');
    });

    if (filasSeleccionadas.length === 0) {
        AbrirMensaje("Atención", "Debe seleccionar al menos un producto (marcando su checkbox) para aplicar los cambios.",
            function () { $("#msjModal").modal("hide"); },
            false, ["Aceptar"], "warn!", null);
        return;
    }

    // Obtener los valores de los campos de edición que están habilitados
    const cambios = {};

    if ($('#chkPLista').is(':checked')) cambios.plista = $('#txtPLista').val();
    if ($('#chkDto1').is(':checked')) cambios.dto1 = $('#txtDto1').val();
    if ($('#chkDto2').is(':checked')) cambios.dto2 = $('#txtDto2').val();
    if ($('#chkDto3').is(':checked')) cambios.dto3 = $('#txtDto3').val();
    if ($('#chkDto4').is(':checked')) cambios.dto4 = $('#txtDto4').val();
    if ($('#chkDpo').is(':checked')) cambios.dpo = $('#txtDpo').val();
    if ($('#chkBon').is(':checked')) cambios.bon = $('#txtBon').val();
    if ($('#chkFl').is(':checked')) cambios.fl = $('#txtFl').val();

    // Verificar que hay cambios para aplicar
    if (Object.keys(cambios).length === 0) {
        AbrirMensaje("Atención", "No hay cambios para aplicar. Seleccione al menos un campo y modifique su valor.",
            function () { $("#msjModal").modal("hide"); },
            false, ["Aceptar"], "warn!", null);
        return;
    }

    // NUEVO: Mostrar indicador de progreso mejorado para grandes cantidades de filas
    const totalFilas = filasSeleccionadas.length;

    // Mostrar advertencia si hay muchas filas
    if (totalFilas > 500) {
        AbrirMensaje("Procesando gran cantidad de datos",
            `Está aplicando cambios a ${totalFilas} productos. Este proceso puede tardar varios minutos. ¿Desea continuar?`,
            function () {
                $("#msjModal").modal("hide");
                iniciarProcesamiento(filasSeleccionadas, cambios, totalFilas);
            },
            true, ["Continuar", "Cancelar"], "warn!", null);
    } else {
        // Si son pocas filas, proceder directamente
        iniciarProcesamiento(filasSeleccionadas, cambios, totalFilas);
    }
}

// NUEVO: Función mejorada para iniciar el procesamiento por lotes
function iniciarProcesamiento(filasSeleccionadas, cambios, totalFilas) {
    // ✅ CRÍTICO: Inicializar/limpiar la variable global para este proceso
    window.filasModificadasGlobal = [];

    // Mostrar indicador de progreso avanzado
    crearDialogoProgresoAvanzado(totalFilas);

    // Constantes para el procesamiento por lotes
    const TAMANO_LOTE = 50; // Procesar 50 filas a la vez
    const INTERVALO_ENTRE_LOTES = 100; // 100ms entre lotes para permitir respuesta de UI

    // Convertir la colección jQuery a un array para facilitar la división en lotes
    const arrayFilas = filasSeleccionadas.toArray();

    // Comenzar el procesamiento por lotes, usando la nueva versión de procesarLoteDeFilas
    procesarLoteDeFilas(arrayFilas, 0, TAMANO_LOTE, cambios, totalFilas, INTERVALO_ENTRE_LOTES);
}


// NUEVO: Crear un diálogo de progreso más avanzado
function crearDialogoProgresoAvanzado(totalFilas) {
    // Eliminar cualquier diálogo existente
    $("#dialogoProgresoAvanzado").remove();

    // Crear nuevo diálogo
    const dialogoHTML = `
        <div id="dialogoProgresoAvanzado" class="modal fade" tabindex="-1" role="dialog" data-backdrop="static" data-keyboard="false">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Aplicando cambios</h5>
                    </div>
                    <div class="modal-body text-center">
                        <div class="mb-3">
                            <i class="bx bx-loader bx-spin font-size-32"></i>
                        </div>
                        <div id="textoProgreso">Preparando procesamiento...</div>
                        <div class="progress mt-3">
                            <div id="barraProgreso" class="progress-bar" role="progressbar" style="width: 0%"></div>
                        </div>
                        <div class="mt-2">
                            <span id="filasCompletadas">0</span> de <span id="filasTotal">${totalFilas}</span> productos procesados
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `;

    // Agregar al DOM y mostrar
    $('body').append(dialogoHTML);
    $("#dialogoProgresoAvanzado").modal('show');
}

// CORREGIDO: Función para procesar lotes de filas con recálculo de costos
// ALTERNATIVA SIMPLE: Pasar las filas modificadas como parámetro
function procesarLoteDeFilas(arrayFilas, inicio, tamanoLote, cambios, totalFilas, intervaloEntreLotes, filasModificadasAcumuladas = []) {
    const fin = Math.min(inicio + tamanoLote, arrayFilas.length);

    // Procesar las filas de este lote y acumular las modificadas
    for (let i = inicio; i < fin; i++) {
        const fila = $(arrayFilas[i]);
        const fueModificado = aplicarCambiosAFila(fila, cambios);

        if (fueModificado) {
            filasModificadasAcumuladas.push(fila[0]);
        }
    }

    // Actualizar progreso...
    const procesados = fin;
    const porcentaje = Math.round((procesados / totalFilas) * 100);
    $("#barraProgreso").css('width', porcentaje + '%');
    $("#filasCompletadas").text(procesados);
    $("#textoProgreso").text(`Procesando... ${porcentaje}%`);

    if (fin < arrayFilas.length) {
        setTimeout(function () {
            procesarLoteDeFilas(arrayFilas, fin, tamanoLote, cambios, totalFilas, intervaloEntreLotes, filasModificadasAcumuladas);
        }, intervaloEntreLotes);
    } else {
        // Solo recalcular costos para las filas que realmente se modificaron EN ESTE PROCESO
        $("#textoProgreso").text("Aplicando cambios completado. Iniciando recálculo de costos...");
        $("#barraProgreso").css('width', '0%');
        $("#filasCompletadas").text('0');

        // Pasar solo las filas modificadas en este proceso
        iniciarRecalculoCostos(filasModificadasAcumuladas, totalFilas);
    }
}

// CORREGIDO: Función para iniciar el recálculo de costos por lotes
function iniciarRecalculoCostos(arrayFilas, totalFilasOriginales) {
    // ✅ CORRECCIÓN: Usar la cantidad real de filas modificadas, no las originales
    const totalFilasModificadas = arrayFilas.length;

    console.log(`Iniciando recálculo de costos: ${totalFilasModificadas} filas modificadas de ${totalFilasOriginales} originales`);

    // Si no hay filas modificadas, finalizar inmediatamente
    if (totalFilasModificadas === 0) {
        console.log("No hay filas modificadas para recalcular, finalizando proceso");
        finalizarAplicacionCambios();
        return;
    }

    // Usamos un tamaño de lote más pequeño para los cálculos porque son más intensivos
    const TAMANO_LOTE_CALCULO = 10;
    const INTERVALO_ENTRE_CALCULOS = 300; // ms entre lotes de cálculo

    // ✅ CORRECCIÓN: Pasar el total correcto de filas modificadas
    recalcularCostosPorLotes(arrayFilas, 0, TAMANO_LOTE_CALCULO, totalFilasModificadas, INTERVALO_ENTRE_CALCULOS);
}

// CORREGIDO: Función para recalcular costos por lotes
function recalcularCostosPorLotes(arrayFilas, inicio, tamanoLote, totalFilas, intervaloEntreLotes) {
    console.log(`Procesando lote de costos: ${inicio} a ${Math.min(inicio + tamanoLote, arrayFilas.length)} de ${totalFilas}`);

    // Calcular el fin de este lote
    const fin = Math.min(inicio + tamanoLote, arrayFilas.length);

    // ✅ CORRECCIÓN: Si no hay filas en este rango, avanzar al siguiente lote inmediatamente
    if (fin <= inicio) {
        console.log("Lote vacío, avanzando al siguiente");
        if (fin < arrayFilas.length) {
            setTimeout(function () {
                recalcularCostosPorLotes(arrayFilas, fin, tamanoLote, totalFilas, intervaloEntreLotes);
            }, intervaloEntreLotes);
        } else {
            console.log("Todos los lotes procesados, finalizando");
            finalizarAplicacionCambios();
        }
        return;
    }

    // Variable para contar las filas que han completado su cálculo EN ESTE LOTE
    let procesadosLote = 0;
    const filasEnEsteLote = fin - inicio;

    console.log(`Procesando ${filasEnEsteLote} filas en este lote (${inicio} a ${fin - 1})`);

    // Función de callback para manejar la finalización de cada cálculo
    function calculoCompletado() {
        procesadosLote++;
        console.log(`Cálculo completado: ${procesadosLote}/${filasEnEsteLote} en lote actual`);

        // ✅ CORRECCIÓN: Verificar que se completaron TODAS las filas de este lote
        if (procesadosLote === filasEnEsteLote) {
            // Actualizar el progreso visual basado en filas completadas globalmente
            const procesadosGlobales = fin; // fin representa cuántas filas llevamos procesadas en total
            const porcentaje = Math.round((procesadosGlobales / totalFilas) * 100);

            $("#barraProgreso").css('width', porcentaje + '%');
            $("#filasCompletadas").text(procesadosGlobales);
            $("#textoProgreso").text(`Recalculando costos... ${porcentaje}%`);

            console.log(`Lote completado. Progreso global: ${procesadosGlobales}/${totalFilas} (${porcentaje}%)`);

            // Si quedan filas por procesar, programar el siguiente lote
            if (fin < arrayFilas.length) {
                console.log(`Programando siguiente lote: ${fin} a ${Math.min(fin + tamanoLote, arrayFilas.length)}`);
                setTimeout(function () {
                    recalcularCostosPorLotes(arrayFilas, fin, tamanoLote, totalFilas, intervaloEntreLotes);
                }, intervaloEntreLotes);
            } else {
                // Todo el proceso completado
                console.log("¡Todos los cálculos completados! Finalizando proceso");
                finalizarAplicacionCambios();
            }
        }
    }

    // ✅ CORRECCIÓN: Procesar exactamente las filas del lote actual
    for (let i = inicio; i < fin; i++) {
        const fila = $(arrayFilas[i]);
        const productoId = fila.data('p-id');

        // Verificamos si hay cambios que afectan al costo (secuencia01)
        const hayConceptosCosto = fila.find('.input-tp_plista.campo-modificado, .input-tp_dto1.campo-modificado, .input-tp_dto2.campo-modificado, .input-tp_dto3.campo-modificado, .input-tp_dto4.campo-modificado, .input-tp_dto_pa.campo-modificado, .input-tp_porc_flete.campo-modificado, .input-tp_boni.campo-modificado').length > 0;

        console.log(`Producto ${productoId}: ${hayConceptosCosto ? 'requiere' : 'no requiere'} recálculo de costo`);

        if (hayConceptosCosto) {
            // Si hay conceptos que afectan el costo, llamamos a calcularCostoAPI
            calcularCostoAPIConCallback(fila, calculoCompletado);
        } else {
            // Si no hay cambios que afecten al costo, simplemente marcamos como completado
            calculoCompletado();
        }
    }
}

// NUEVO: Función mejorada para aplicar cambios a una única fila
// MODIFICADO: Función mejorada para aplicar cambios a una única fila
function aplicarCambiosAFila(fila, cambios) {
    let fueModificado = false;

    // Aplicar cada cambio a la fila y marcarlos como modificados
    if (cambios.plista !== undefined) {
        const campo = fila.find('.input-tp_plista');
        const valorAnterior = campo.val();
        campo.val(cambios.plista);
        marcarCampoModificado(campo);

        // Verificar si realmente cambió el valor
        if (valorAnterior !== cambios.plista) {
            fueModificado = true;
        }
    }

    if (cambios.dto1 !== undefined) {
        const campo = fila.find('.input-tp_dto1');
        const valorAnterior = campo.val();
        campo.val(cambios.dto1);
        marcarCampoModificado(campo);

        if (valorAnterior !== cambios.dto1) {
            fueModificado = true;
        }
    }

    if (cambios.dto2 !== undefined) {
        const campo = fila.find('.input-tp_dto2');
        const valorAnterior = campo.val();
        campo.val(cambios.dto2);
        marcarCampoModificado(campo);

        if (valorAnterior !== cambios.dto2) {
            fueModificado = true;
        }
    }

    if (cambios.dto3 !== undefined) {
        const campo = fila.find('.input-tp_dto3');
        const valorAnterior = campo.val();
        campo.val(cambios.dto3);
        marcarCampoModificado(campo);

        if (valorAnterior !== cambios.dto3) {
            fueModificado = true;
        }
    }

    if (cambios.dto4 !== undefined) {
        const campo = fila.find('.input-tp_dto4');
        const valorAnterior = campo.val();
        campo.val(cambios.dto4);
        marcarCampoModificado(campo);

        if (valorAnterior !== cambios.dto4) {
            fueModificado = true;
        }
    }

    if (cambios.dpo !== undefined) {
        const campo = fila.find('.input-tp_dto_pa');
        const valorAnterior = campo.val();
        campo.val(cambios.dpo);
        marcarCampoModificado(campo);

        if (valorAnterior !== cambios.dpo) {
            fueModificado = true;
        }
    }

    if (cambios.bon !== undefined) {
        const campo = fila.find('.input-tp_boni');
        const valorAnterior = campo.val();
        campo.val(cambios.bon);
        marcarCampoModificado(campo);

        if (valorAnterior !== cambios.bon) {
            fueModificado = true;
        }
    }

    if (cambios.fl !== undefined) {
        const campo = fila.find('.input-tp_porc_flete');
        const valorAnterior = campo.val();
        campo.val(cambios.fl);
        marcarCampoModificado(campo);

        if (valorAnterior !== cambios.fl) {
            fueModificado = true;
        }
    }

    // Si hubo cambios, actualizar el estado de carga
    if (fueModificado) {
        actualizarEstadoCarga(fila);
    }

    // ✅ CRÍTICO: Retornar si realmente se modificó algo
    return fueModificado;
}


// MEJORADO: Función para finalizar la aplicación de cambios con mejor logging
function finalizarAplicacionCambios() {
    console.log("=== FINALIZANDO APLICACIÓN DE CAMBIOS ===");

    // ✅ LIMPIAR: Limpiar la variable global
    window.filasModificadasGlobal = [];

    // Limpiar checkboxes y deshabilitar campos después de aplicar
    $('#chkPLista, #chkDto1, #chkDto2, #chkDto3, #chkDto4, #chkDpo, #chkBon, #chkFl').prop('checked', false);
    $('#txtPLista, #txtDto1, #txtDto2, #txtDto3, #txtDto4, #txtDpo, #txtBon, #txtFl').prop('disabled', true);

    // ✅ ASEGURAR que el diálogo se cierre correctamente
    const dialogo = $("#dialogoProgresoAvanzado");
    if (dialogo.length > 0) {
        console.log("Cerrando diálogo de progreso");
        dialogo.modal('hide');

        // ✅ SEGURIDAD ADICIONAL: Remover el diálogo después de un tiempo
        setTimeout(function () {
            dialogo.remove();
            console.log("Diálogo de progreso removido del DOM");
        }, 1000);
    } else {
        console.warn("No se encontró el diálogo de progreso para cerrar");
    }

    // Mostrar mensaje de éxito
    AbrirMensaje("Proceso completado",
        "Los cambios se han aplicado correctamente a los productos seleccionados y se han recalculado los costos y precios de venta.",
        function () {
            $("#msjModal").modal("hide");
            console.log("Proceso completamente terminado");
        },
        false, ["Aceptar"], "success!", null);

    console.log("=== PROCESO DE APLICACIÓN DE CAMBIOS COMPLETADO ===");
}

// NUEVO: Función para iniciar cálculo de precios en segundo plano
function iniciarCalculoPrecios() {
    // Seleccionar todas las filas modificadas
    const filasModificadas = $("#tbProdDet tbody tr[data-carga='1']");
    const totalFilas = filasModificadas.length;

    if (totalFilas === 0) {
        AbrirMensaje("Información", "No hay filas con cambios para procesar.",
            function () { $("#msjModal").modal("hide"); },
            false, ["Aceptar"], "info", null);
        return;
    }

    // Crear diálogo de progreso para el cálculo
    crearDialogoProgresoAvanzado(totalFilas);
    $("#textoProgreso").text("Iniciando cálculo de precios...");

    // Constantes para el procesamiento
    const TAMANO_LOTE_CALCULO = 10; // Menos filas por lote para evitar sobrecarga
    const INTERVALO_ENTRE_LOTES = 500; // Mayor intervalo para dar tiempo a los cálculos

    // Convertir a array
    const arrayFilas = filasModificadas.toArray();

    // Comenzar el procesamiento de cálculos
    procesarCalculosPrecios(arrayFilas, 0, TAMANO_LOTE_CALCULO, totalFilas, INTERVALO_ENTRE_LOTES);
}

// NUEVO: Función para procesar lotes de cálculos
function procesarCalculosPrecios(arrayFilas, inicio, tamanoLote, totalFilas, intervaloEntreLotes) {
    // Calcular el fin de este lote
    const fin = Math.min(inicio + tamanoLote, arrayFilas.length);

    // Variable para contar filas procesadas en este lote
    let procesadosLote = 0;

    // Función para manejar la finalización de un cálculo
    function calculoCompletado() {
        procesadosLote++;

        // Si se completaron todos los cálculos de este lote, continuar con el siguiente
        if (procesadosLote === (fin - inicio)) {
            // Actualizar progreso visual
            const procesados = fin;
            const porcentaje = Math.round((procesados / totalFilas) * 100);

            $("#barraProgreso").css('width', porcentaje + '%');
            $("#filasCompletadas").text(procesados);
            $("#textoProgreso").text(`Calculando precios... ${porcentaje}%`);

            // Si quedan filas, programar el siguiente lote
            if (fin < arrayFilas.length) {
                setTimeout(function () {
                    procesarCalculosPrecios(arrayFilas, fin, tamanoLote, totalFilas, intervaloEntreLotes);
                }, intervaloEntreLotes);
            } else {
                // Cálculos completados
                $("#dialogoProgresoAvanzado").modal('hide');
                AbrirMensaje("Proceso completado",
                    "Los precios se han calculado correctamente para todos los productos.",
                    function () { $("#msjModal").modal("hide"); },
                    false, ["Aceptar"], "success!", null);
            }
        }
    }

    // Procesar cada fila de este lote
    for (let i = inicio; i < fin; i++) {
        const fila = $(arrayFilas[i]);

        // Llamar a calcularCostoAPI con una función de callback personalizada
        calcularCostoAPIConCallback(fila, calculoCompletado);
    }
}

function calcularCostoAPIConCallback(row, callback) {
    const productId = row.data('p-id');

    // Evitar cálculos redundantes
    if (row.data('calculating-cost') === true) {
        console.log(`Ya hay un cálculo de costo en proceso para producto ${productId}, evitando duplicación`);
        if (callback) {
            console.log(`Ejecutando callback inmediatamente para producto ${productId} (cálculo en progreso)`);
            callback();
        }
        return;
    }

    console.log(`Iniciando cálculo de costo para producto ${productId}`);

    // Marcar que estamos calculando
    row.data('calculating-cost', true);

    // Recopilar los valores de los campos
    const plistaValue = row.find('.input-tp_plista').val().replace(/,/g, '');

    const datos = {
        p_id: productId,
        tp_plista: plistaValue === '' ? 0 : parseFloat(plistaValue),
        tp_dto1: parseFloat(row.find('.input-tp_dto1').val().replace(/,/g, '')) || 0,
        tp_dto2: parseFloat(row.find('.input-tp_dto2').val().replace(/,/g, '')) || 0,
        tp_dto3: parseFloat(row.find('.input-tp_dto3').val().replace(/,/g, '')) || 0,
        tp_dto4: parseFloat(row.find('.input-tp_dto4').val().replace(/,/g, '')) || 0,
        tp_dto_pa: parseFloat(row.find('.input-tp_dto_pa').val().replace(/,/g, '')) || 0,
        tp_porc_flete: parseFloat(row.find('.input-tp_porc_flete').val().replace(/,/g, '')) || 0,
        tp_boni: row.find('.input-tp_boni').val()
    };

    // Mostrar indicador de carga en el campo
    const campoCoste = row.find('.input-tp_pcosto');
    const valorOriginal = campoCoste.val();
    campoCoste.val('...').addClass('calculating');

    // Llamar a la API
    $.ajax({
        url: calcularCostoUrl,
        type: 'POST',
        data: datos,
        dataType: 'json',
        success: function (obj) {
            // Desmarcar estado de cálculo
            row.data('calculating-cost', false);

            if (obj.error === true || obj.warn === true) {
                // En caso de error, restaurar el valor original
                campoCoste.val(valorOriginal).removeClass('calculating');
                console.log(`Error en cálculo para producto ID ${productId}: ${obj.msg}`);
                if (callback) {
                    console.log(`Ejecutando callback después de error para producto ${productId}`);
                    callback();
                }
            } else {
                // Éxito: actualizar el valor del costo
                campoCoste.val(parseFloat(obj.costo).toFixed(3)).removeClass('calculating');
                marcarCampoModificado(campoCoste);
                actualizarEstadoCarga(row);

                console.log(`Costo actualizado para producto ${productId}, continuando con precio de venta`);

                // Continuar con el cálculo del precio de venta, pero pasando el callback para mantener la secuencia
                calcularPrecioVentaAPIConCallbackSecuencial(row, callback);
            }
        },
        error: function (xhr, status, error) {
            // Error en la petición
            row.data('calculating-cost', false);
            campoCoste.val(valorOriginal).removeClass('calculating');
            console.error(`Error en la llamada para calcular costo del producto ID ${productId}: ${error}`);

            if (callback) callback();
        }
    });
}

// NUEVO: Versión mejorada de calcularPrecioVentaAPIConCallback que mantiene la secuencia de cálculos
// NUEVO: Versión mejorada de calcularPrecioVentaAPIConCallback que mantiene la secuencia de cálculos
function calcularPrecioVentaAPIConCallbackSecuencial(row, callback) {
    const productId = row.data('p-id');

    // Evitar cálculos redundantes
    if (row.data('calculating-price') === true) {
        console.log('Ya hay un cálculo de precio en proceso para este producto, evitando duplicación');
        if (callback) callback();
        return;
    }

    // Marcar que estamos calculando
    row.data('calculating-price', true);

    // Actualizar variable global
    productoActualEnLista = productId;
    $("#divProdLista").attr('data-producto-actual', productId);

    // Recopilar valores
    const pcosto = row.find('.input-tp_pcosto').val().replace(/,/g, '');
    const margen = row.find('.input-tp_margen').val().replace(/,/g, '');

    const datos = {
        p_id: productId,
        tp_pcosto: pcosto === '' ? 0 : parseFloat(pcosto),
        lp_prevision_tot: parseFloat(row.find('input[name="lp_prevision_tot"]').val()) || 0,
        lp_prevision_pin: parseFloat(row.find('input[name="lp_prevision_pin"]').val()) || 0,
        tp_margen: margen === '' ? 0 : parseFloat(margen),
        iva_situacion: row.find('input[name="iva_situacion"]').val() || 'E',
        iva_alicuota: parseFloat(row.find('input[name="iva_alicuota"]').val()) || 0,
        in_alicuota: parseFloat(row.find('input[name="in_alicuota"]').val()) || 0
    };

    // Mostrar indicador de carga
    const campoPrecioNeto = row.find('.input-tp_pneto');
    const valorOriginalPNeto = campoPrecioNeto.val();
    campoPrecioNeto.val('...').addClass('calculating');

    // Llamar a la API
    $.ajax({
        url: calcularPrecioVentaBaseUrl,
        type: 'POST',
        data: datos,
        dataType: 'json',
        success: function (response) {
            // Desmarcar estado de cálculo
            row.data('calculating-price', false);

            if (response.error === true || response.warn === true) {
                // Error: restaurar valor
                campoPrecioNeto.val(valorOriginalPNeto).removeClass('calculating');
                console.log(`Error en cálculo de precio para producto ID ${productId}: ${response.msg}`);
            } else {
                // Éxito: actualizar valores

                // 1. Precio neto
                const pneto = parseFloat(response.pvta.p_pneto).toFixed(3);
                campoPrecioNeto.val(pneto).removeClass('calculating');
                marcarCampoModificado(campoPrecioNeto);

                // 2. Precio venta
                const campoPVenta = row.find('.input-tp_pvta');
                const pvta = parseFloat(response.pvta.p_pvta).toFixed(2);
                campoPVenta.val(pvta);
                marcarCampoModificado(campoPVenta);

                // 3. Campos ocultos
                row.find('input[name="tp_iva"]').val(response.pvta.p_iva);
                row.find('input[name="tp_in"]').val(response.pvta.p_in);

                // 4. Ratio (sin actualizar listas para mejorar rendimiento)
                actualizarRatio(row, pvta);

                // 5. Resguardar cambios
                resguardarCambiosProducto(row);

                // *** PASO CRÍTICO: ACTUALIZAR LISTAS SI ES EL PRODUCTO ACTUALMENTE VISIBLE ***
                actualizarListasSiEsProductoActual(productId, datos, pvta, callback);
            }

            // NO llamar al callback aquí - se llama desde actualizarListasSiEsProductoActual
        },
        error: function (xhr, status, error) {
            // Error en la petición
            row.data('calculating-price', false);
            campoPrecioNeto.val(valorOriginalPNeto).removeClass('calculating');
            console.error(`Error en la llamada para calcular precio del producto ID ${productId}: ${error}`);

            if (callback) callback();
        }
    });
}

// NUEVA: Procesador de lotes silencioso para no interferir con el proceso masivo
function procesarLoteListasSilencioso(listas, inicio, tamanoLote, totalListas, datosProducto, precioNetoBase, pvta, callback) {
    const fin = Math.min(inicio + tamanoLote, totalListas);
    const loteActual = listas.slice(inicio, fin);
    const promesas = [];

    // Procesar este lote en paralelo
    loteActual.forEach(lista => {
        // Crear promesa para cada actualización de lista
        promesas.push(new Promise((resolve, reject) => {
            const datosLista = {
                p_id: lista.p_id,
                lp_id: lista.lp_id,
                tp_pcosto: datosProducto.tp_pcosto,
                p_pneto_base: precioNetoBase,
                lp_porc_mg: lista.lp_porc_mg,
                iva_situacion: datosProducto.iva_situacion,
                iva_alicuota: datosProducto.iva_alicuota,
                in_alicuota: datosProducto.in_alicuota
            };

            $.ajax({
                url: calcularPrecioVentaLinkUrl,
                type: 'POST',
                data: datosLista,
                dataType: 'json',
                success: function (respLista) {
                    if (respLista && respLista.pvta) {
                        // Actualizar los campos de la lista
                        const listaRow = lista.row;

                        // Actualizar precio neto
                        listaRow.find('input[name="tp_pneto"]').val(parseFloat(respLista.pvta.p_pneto).toFixed(3));

                        // Actualizar precio venta lista
                        const campoPVtaLista = listaRow.find('.input-tp_pvta_lista');
                        const nuevoPVta = parseFloat(respLista.pvta.p_pvta).toFixed(2);

                        // *** ACTUALIZAR SIEMPRE EL VALOR ***
                        campoPVtaLista.val(nuevoPVta);

                        // *** MARCAR COMO MODIFICADO Y RESGUARDAR ***
                        campoPVtaLista.data('original-value', parseFloat(nuevoPVta));
                        marcarCampoModificadoLista(campoPVtaLista);

                        // Actualizar en servidor de forma silenciosa
                        actualizarPrecioVentaListaSilencioso(listaRow, lista.lp_id, lista.p_id, parseFloat(nuevoPVta));

                        // Actualizar campos ocultos
                        listaRow.find('input[name="tp_iva"]').val(respLista.pvta.p_iva);
                        listaRow.find('input[name="tp_in"]').val(respLista.pvta.p_in);

                        // Calcular y actualizar ratio
                        if (pvta > 0) {
                            const ratio = (parseFloat(nuevoPVta) / parseFloat(pvta)).toFixed(2);
                            listaRow.find('td:eq(4)').text(ratio);
                        }
                    }
                    resolve();
                },
                error: function (error) {
                    console.error(`Error al actualizar lista ${lista.lp_id}:`, error);
                    resolve(); // Continuar con las demás
                }
            });
        }));
    });

    // Esperar a que se completen todas las actualizaciones del lote
    Promise.all(promesas)
        .then(() => {
            console.log(`Listas actualizadas silenciosamente: ${fin}/${totalListas}`);

            // Llamar al callback final
            if (callback) callback();
        })
        .catch(error => {
            console.error("Error al procesar listas silenciosamente:", error);
            if (callback) callback();
        });
}

// NUEVA: Procesador de lotes silencioso para no interferir con el proceso masivo
function procesarLoteListasSilencioso(listas, inicio, tamanoLote, totalListas, datosProducto, precioNetoBase, pvta, callback) {
    const fin = Math.min(inicio + tamanoLote, totalListas);
    const loteActual = listas.slice(inicio, fin);
    const promesas = [];

    // Procesar este lote en paralelo
    loteActual.forEach(lista => {
        // Crear promesa para cada actualización de lista
        promesas.push(new Promise((resolve, reject) => {
            const datosLista = {
                p_id: lista.p_id,
                lp_id: lista.lp_id,
                tp_pcosto: datosProducto.tp_pcosto,
                p_pneto_base: precioNetoBase,
                lp_porc_mg: lista.lp_porc_mg,
                iva_situacion: datosProducto.iva_situacion,
                iva_alicuota: datosProducto.iva_alicuota,
                in_alicuota: datosProducto.in_alicuota
            };

            $.ajax({
                url: calcularPrecioVentaLinkUrl,
                type: 'POST',
                data: datosLista,
                dataType: 'json',
                success: function (respLista) {
                    if (respLista && respLista.pvta) {
                        // Actualizar los campos de la lista
                        const listaRow = lista.row;

                        // Actualizar precio neto
                        listaRow.find('input[name="tp_pneto"]').val(parseFloat(respLista.pvta.p_pneto).toFixed(3));

                        // Actualizar precio venta lista
                        const campoPVtaLista = listaRow.find('.input-tp_pvta_lista');
                        const nuevoPVta = parseFloat(respLista.pvta.p_pvta).toFixed(2);

                        // *** ACTUALIZAR SIEMPRE EL VALOR ***
                        campoPVtaLista.val(nuevoPVta);

                        // *** MARCAR COMO MODIFICADO Y RESGUARDAR ***
                        campoPVtaLista.data('original-value', parseFloat(nuevoPVta));
                        marcarCampoModificadoLista(campoPVtaLista);

                        // Actualizar en servidor de forma silenciosa
                        actualizarPrecioVentaListaSilencioso(listaRow, lista.lp_id, lista.p_id, parseFloat(nuevoPVta));

                        // Actualizar campos ocultos
                        listaRow.find('input[name="tp_iva"]').val(respLista.pvta.p_iva);
                        listaRow.find('input[name="tp_in"]').val(respLista.pvta.p_in);

                        // Calcular y actualizar ratio
                        if (pvta > 0) {
                            const ratio = (parseFloat(nuevoPVta) / parseFloat(pvta)).toFixed(2);
                            listaRow.find('td:eq(4)').text(ratio);
                        }
                    }
                    resolve();
                },
                error: function (error) {
                    console.error(`Error al actualizar lista ${lista.lp_id}:`, error);
                    resolve(); // Continuar con las demás
                }
            });
        }));
    });

    // Esperar a que se completen todas las actualizaciones del lote
    Promise.all(promesas)
        .then(() => {
            console.log(`Listas actualizadas silenciosamente: ${fin}/${totalListas}`);

            // Llamar al callback final
            if (callback) callback();
        })
        .catch(error => {
            console.error("Error al procesar listas silenciosamente:", error);
            if (callback) callback();
        });
}

// NUEVA: Función eficiente para actualizar listas solo del producto actualmente visible
function actualizarListasSiEsProductoActual(productId, datosProducto, pvta, callback) {
    // Verificar si este producto es el que está actualmente visible en el panel de listas
    const productoVisibleEnListas = $("#divProdLista").attr('data-producto-actual');

    if (productoVisibleEnListas == productId && $('#tbProdLista tbody tr').length > 0) {
        console.log(`Actualizando listas para producto visible: ${productId}`);

        // Actualizar las listas de forma optimizada (sin indicador de carga para no interferir con el proceso masivo)
        actualizarPreciosListasOptimizadoSilencioso(datosProducto, pvta, function () {
            // Callback después de actualizar las listas
            if (callback) callback();
        });
    } else {
        // Si no es el producto visible, simplemente continuar
        console.log(`Producto ${productId} no está visible en listas, omitiendo actualización`);
        if (callback) callback();
    }
}

// NUEVA: Versión optimizada y silenciosa para procesamiento masivo
function actualizarPreciosListasOptimizadoSilencioso(datosProducto, pvta, callback) {
    // Obtener las filas de la tabla de listas
    const filasLista = $('#tbProdLista tbody tr');

    // Si no hay filas, no hacer nada
    if (filasLista.length === 0) {
        if (callback) callback();
        return;
    }

    // Verificar datos necesarios
    if (!datosProducto.tp_pcosto || isNaN(datosProducto.tp_pcosto)) {
        console.error("Falta el costo del producto para actualizar listas");
        if (callback) callback();
        return;
    }

    // Obtener el precio neto base
    let precioNetoBase;
    const productoFila = $(`#tbProdDet tbody tr[data-p-id='${productoActualEnLista}']`);
    if (productoFila.length > 0) {
        const pNetoValue = productoFila.find('.input-tp_pneto').val();
        if (pNetoValue) {
            precioNetoBase = parseFloat(pNetoValue.replace(/,/g, ''));
        }
    }

    // Si no tenemos precio neto base, intentar calcularlo
    if (!precioNetoBase || isNaN(precioNetoBase)) {
        if (datosProducto.tp_pcosto && datosProducto.tp_margen) {
            precioNetoBase = datosProducto.tp_pcosto * (1 + datosProducto.tp_margen / 100);
        } else {
            console.warn("No se pudo determinar p_pneto_base");
            if (callback) callback();
            return;
        }
    }

    // Preparar datos para actualización masiva
    const listasData = [];
    filasLista.each(function () {
        const listaRow = $(this);
        const lp_id = listaRow.data('lp-id');
        const p_id = listaRow.find('.input-tp_margen_lista').data('p-id') || productoActualEnLista;
        const lp_porc_mg = parseFloat(listaRow.find('input[name="lp_porc_mg"]').val());

        if (!isNaN(lp_porc_mg) && lp_id && p_id) {
            listasData.push({
                row: listaRow,
                lp_id: lp_id,
                p_id: p_id,
                lp_porc_mg: lp_porc_mg
            });
        }
    });

    if (listasData.length === 0) {
        if (callback) callback();
        return;
    }

    // *** PROCESAMIENTO SILENCIOSO SIN INDICADORES VISUALES ***
    procesarLoteListasSilencioso(listasData, 0, listasData.length, listasData.length, datosProducto, precioNetoBase, pvta, callback);
}

// NUEVA: Función silenciosa para actualizar precio de venta sin logs ni mensajes
function actualizarPrecioVentaListaSilencioso(row, lpId, pId, nuevoPrecioVenta) {
    // Validaciones básicas
    if (!row || !row.length || !lpId || !pId || isNaN(nuevoPrecioVenta)) {
        return;
    }

    const datos = {
        p_id: pId,
        lp_id: lpId,
        tp_margen: parseFloat(row.find('.input-tp_margen_lista').val().replace(/,/g, '')) || 0,
        tp_pvta: nuevoPrecioVenta,
        p_pcosto: parseFloat(row.find('input[name="p_pcosto"]').val()) || 0,
        p_pneto: parseFloat(row.find('input[name="tp_pneto"]').val()) || 0,
        lp_porc_mg: parseFloat(row.find('input[name="lp_porc_mg"]').val()) || 0,
        iva_situacion: row.find('input[name="iva_situacion"]').val() || 'E',
        iva_alicuota: parseFloat(row.find('input[name="iva_alicuota"]').val()) || 0,
        in_alicuota: parseFloat(row.find('input[name="in_alicuota"]').val()) || 0,
        tp_iva: parseFloat(row.find('input[name="tp_iva"]').val()) || 0,
        tp_in: parseFloat(row.find('input[name="tp_in"]').val()) || 0
    };

    // Llamada AJAX silenciosa
    $.ajax({
        url: resguardarCambiosProductoListaUrl,
        type: 'POST',
        data: datos,
        dataType: 'json',
        success: function (response) {
            // Sin logs ni mensajes para no interferir con el proceso masivo
            if (response.error) {
                console.error('Error silencioso al resguardar lista:', response.msg);
            }
        },
        error: function (xhr, status, error) {
            console.error('Error silencioso AJAX lista:', error);
        }
    });
}

// NUEVO: Versión modificada de calcularPrecioVentaAPI que acepta un callback
function calcularPrecioVentaAPIConCallback(row, callback) {
    const productId = row.data('p-id');

    // Evitar cálculos redundantes
    if (row.data('calculating-price') === true) {
        console.log('Ya hay un cálculo de precio en proceso para este producto, evitando duplicación');
        if (callback) callback();
        return;
    }

    // Marcar que estamos calculando
    row.data('calculating-price', true);

    // Actualizar variable global
    productoActualEnLista = productId;
    $("#divProdLista").attr('data-producto-actual', productId);

    // Recopilar valores
    const pcosto = row.find('.input-tp_pcosto').val().replace(/,/g, '');
    const margen = row.find('.input-tp_margen').val().replace(/,/g, '');

    const datos = {
        p_id: productId,
        tp_pcosto: pcosto === '' ? 0 : parseFloat(pcosto),
        lp_prevision_tot: parseFloat(row.find('input[name="lp_prevision_tot"]').val()) || 0,
        lp_prevision_pin: parseFloat(row.find('input[name="lp_prevision_pin"]').val()) || 0,
        tp_margen: margen === '' ? 0 : parseFloat(margen),
        iva_situacion: row.find('input[name="iva_situacion"]').val() || 'E',
        iva_alicuota: parseFloat(row.find('input[name="iva_alicuota"]').val()) || 0,
        in_alicuota: parseFloat(row.find('input[name="in_alicuota"]').val()) || 0
    };

    // Mostrar indicador de carga
    const campoPrecioNeto = row.find('.input-tp_pneto');
    const valorOriginalPNeto = campoPrecioNeto.val();
    campoPrecioNeto.val('...').addClass('calculating');

    // Llamar a la API
    $.ajax({
        url: calcularPrecioVentaBaseUrl,
        type: 'POST',
        data: datos,
        dataType: 'json',
        success: function (response) {
            // Desmarcar estado de cálculo
            row.data('calculating-price', false);

            if (response.error === true || response.warn === true) {
                // Error: restaurar valor
                campoPrecioNeto.val(valorOriginalPNeto).removeClass('calculating');
                console.log(`Error en cálculo de precio para producto ID ${productId}: ${response.msg}`);
            } else {
                // Éxito: actualizar valores

                // 1. Precio neto
                const pneto = parseFloat(response.pvta.p_pneto).toFixed(3);
                campoPrecioNeto.val(pneto).removeClass('calculating');
                marcarCampoModificado(campoPrecioNeto);

                // 2. Precio venta
                const campoPVenta = row.find('.input-tp_pvta');
                const pvta = parseFloat(response.pvta.p_pvta).toFixed(2);
                campoPVenta.val(pvta);
                marcarCampoModificado(campoPVenta);

                // 3. Campos ocultos
                row.find('input[name="tp_iva"]').val(response.pvta.p_iva);
                row.find('input[name="tp_in"]').val(response.pvta.p_in);

                // 4. Ratio (sin actualizar listas para mejorar rendimiento)
                actualizarRatio(row, pvta);

                // 5. Resguardar cambios
                resguardarCambiosProducto(row);
            }

            // Llamar al callback una vez completado
            if (callback) callback();
        },
        error: function (xhr, status, error) {
            // Error en la petición
            row.data('calculating-price', false);
            campoPrecioNeto.val(valorOriginalPNeto).removeClass('calculating');
            console.error(`Error en la llamada para calcular precio del producto ID ${productId}: ${error}`);

            if (callback) callback();
        }
    });
}

// NUEVO: Función separada para actualizar el ratio
function actualizarRatio(row, pvta) {
    const precioVentaOriginal = parseFloat(row.find('.input-tp_pvta').data('original-value') || '0');
    const precioVentaNuevo = parseFloat(pvta);

    // Encontrar la celda
    const celdaRatio = row.find('.tdRe');
    if (celdaRatio.length === 0) return;

    // Calcular ratio
    let ratio = precioVentaOriginal > 0 ? (precioVentaNuevo / precioVentaOriginal).toFixed(2) :
        (precioVentaNuevo > 0 ? "999.99" : "0.00");

    // Actualizar celda
    celdaRatio.text(ratio);

    // Aplicar estilo
    const ratioNum = parseFloat(ratio);
    if (ratioNum > 1) {
        celdaRatio.css({
            'color': 'blue',
            'font-weight': 'bold'
        });
    } else if (ratioNum < 1) {
        celdaRatio.css({
            'color': 'red',
            'font-weight': 'bold'
        });
    } else {
        celdaRatio.css({
            'color': '',
            'font-weight': 'normal'
        });
    }
}



// Función para cancelar los cambios en _datosGenerales
function cancelarCambiosDatosGenerales() {
    // Limpiar checkboxes y deshabilitar campos
    $('#chkPLista, #chkDto1, #chkDto2, #chkDto3, #chkDto4, #chkDpo, #chkBon, #chkFl').prop('checked', false);
    $('#txtPLista, #txtDto1, #txtDto2, #txtDto3, #txtDto4, #txtDpo, #txtBon, #txtFl').prop('disabled', true).val('');

    // Si hay un producto seleccionado, recargar sus datos originales
    if (productoActualEnLista) {
        cargarDatosEnVistaPrevia(productoActualEnLista);
    }
}

function inicializaControlCuenta() {
    $("#controlConsultaCambio" + nnControlCta01).val(true);
    window["AsignaDatosCuenta" + nnControlCta01]();

    //muestro el control
    $("#controlCta" + nnControlCta01).show("fast");
}

// Función corregida para configurar eventos de tabla
function configurarEventosTabla() {
    console.log("Configurando eventos de tabla...");

    // Primero eliminar cualquier evento click previo para evitar duplicados
    $("#tbProdDet tbody tr").off("click");
    $("#tbProdDet tbody tr input[type='checkbox']").off("click change");

    // PASO 1: Evento EXCLUSIVO para checkboxes individuales - Solo maneja el estado del checkbox
    $("#tbProdDet tbody tr input[type='checkbox']").on("click", function (e) {
        // CRÍTICO: Detener completamente la propagación del evento
        e.stopPropagation();
        e.stopImmediatePropagation();

        // SOLO manejar el estado visual de la fila según el checkbox
        const fila = $(this).closest('tr');
        const isChecked = $(this).is(':checked');

        if (isChecked) {
            fila.addClass("selected");
        } else {
            fila.removeClass("selected");
        }

        // NO hacer nada más - el checkbox maneja su propio estado automáticamente
        console.log(`Checkbox ${isChecked ? 'marcado' : 'desmarcado'} para producto:`, fila.data('p-id'));
    });

    // PASO 2: Evento para seleccionar filas - EXCLUYE COMPLETAMENTE los checkboxes y sus celdas contenedoras
    $("#tbProdDet tbody tr").on("click", function (e) {
        // CRÍTICO: Verificaciones exhaustivas para excluir checkboxes
        const target = $(e.target);
        const clickedElement = e.target;

        // 1. Verificar si el clic fue directamente en un checkbox
        if (target.is('input[type="checkbox"]')) {
            console.log("Click detectado en checkbox - ignorando evento de fila");
            return;
        }

        // 2. Verificar si el clic fue en un elemento que contiene un checkbox
        if (target.closest('input[type="checkbox"]').length > 0) {
            console.log("Click detectado en contenedor de checkbox - ignorando evento de fila");
            return;
        }

        // 3. Verificar si el clic fue en la celda que contiene el checkbox (primera columna típicamente)
        const celda = target.closest('td');
        if (celda.length > 0 && celda.find('input[type="checkbox"]').length > 0) {
            console.log("Click detectado en celda de checkbox - ignorando evento de fila");
            return;
        }

        // 4. Verificar si el clic fue en un label asociado a un checkbox
        if (target.is('label') && target.attr('for') && target.attr('for').includes('check')) {
            console.log("Click detectado en label de checkbox - ignorando evento de fila");
            return;
        }

        // 5. Verificar si el elemento clickeado es hijo de un label de checkbox
        if (target.closest('label[for*="check"]').length > 0) {
            console.log("Click detectado en elemento hijo de label de checkbox - ignorando evento de fila");
            return;
        }

        // 6. Verificación adicional por posición: si el click fue en los primeros 40px de la fila (donde típicamente está el checkbox)
        const filaOffset = $(this).offset();
        const clickX = e.pageX;
        if (clickX - filaOffset.left < 40) {
            // Verificar si hay un checkbox en esa área
            const primeraColumna = $(this).find('td:first');
            if (primeraColumna.find('input[type="checkbox"]').length > 0) {
                console.log("Click detectado en área de checkbox (primeros 40px) - ignorando evento de fila");
                return;
            }
        }

        // PASO 3: Solo si pasó todas las verificaciones, proceder con la selección de fila
        const productoId = $(this).data('p-id');

        if (!productoId) {
            console.warn("No se pudo obtener el ID del producto para la fila seleccionada");
            return;
        }

        console.log("Fila seleccionada para producto ID:", productoId);

        // Actualizar variable global
        productoActualEnLista = productoId;
        $("#divProdLista").attr('data-producto-actual', productoId);

        // Destacar visualmente la fila seleccionada (sin afectar checkboxes)
        destacarFilaSeleccionada(productoId);

        // Cargar los datos originales en la vista previa
        cargarDatosEnVistaPrevia(productoId);

        // Cargar las listas de precios para este producto
        buscarProductoListaOptimizado(productoId);
    });

    // PASO 4: Evento para el checkbox de seleccionar todos - Simplificado
    $("#checkAllProd").off("change").on("change", function (e) {
        // Detener propagación para evitar conflictos
        e.stopPropagation();

        const isChecked = $(this).prop("checked");

        // Actualizar todos los checkboxes individuales y las clases de fila
        $("#tbProdDet tbody tr").each(function () {
            const checkbox = $(this).find('input[type="checkbox"]');
            const fila = $(this);

            // Actualizar el estado del checkbox
            checkbox.prop('checked', isChecked);

            // Actualizar la clase visual de la fila
            if (isChecked) {
                fila.addClass("selected");
            } else {
                fila.removeClass("selected");
            }
        });

        // Si se marca todo, cargar las listas del primer producto
        if (isChecked) {
            const primerProductoSeleccionado = $("#tbProdDet tbody tr:first").data('p-id');
            if (primerProductoSeleccionado) {
                destacarFilaSeleccionada(primerProductoSeleccionado);
                buscarProductoListaOptimizado(primerProductoSeleccionado);
            }
        } else {
            // Si se desmarca todo, limpiar el panel de listas
            $("#divProdLista").html('<div class="alert alert-info">Seleccione un producto para ver sus listas de precios.</div>');
        }

        console.log(`Checkbox "Seleccionar todo" ${isChecked ? 'marcado' : 'desmarcado'}`);
    });

    console.log("Eventos de tabla configurados correctamente");
}



// Configuración optimizada de elementos de tabla
//function configuracionElementosTablaDetalle() {
//    console.log("Configurando elementos de tabla detalle...");

//    // Remover máscaras previas para evitar conflictos en todos los campos
//    $('.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni, .input-tp_pcosto, .input-tp_margen, .input-tp_pneto, .input-tin_alicuota, .input-tp_pvta').inputmask('remove');

//    // Establecer todos los campos como readonly inicialmente (excepto los que ya tienen readonly)
//    $('.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni, .input-tp_pcosto, .input-tp_margen, .input-tp_pneto, .input-tin_alicuota, .input-tp_pvta')
//        .prop('readonly', true)
//        .addClass('campo-readonly');

//    // Formatear los valores
//    formatearValoresIniciales();

//    // Configurar eventos para activar/desactivar edición
//    configurarEventosEdicion();

//    // Configuración para campos con 3 decimales (P.Lista, P.Costo y P.Neto)
//    Inputmask({
//        alias: "numeric",
//        groupSeparator: ",",
//        radixPoint: ".",
//        autoGroup: true,
//        digits: 3,
//        digitsOptional: false,
//        rightAlign: true,
//        prefix: '',
//        placeholder: "0",
//        clearMaskOnLostFocus: false,
//        showMaskOnHover: false,
//        showMaskOnFocus: false,
//        onBeforeMask: function (value) {
//            if (value) {
//                let numValue = parseFloat(value.toString().replace(/,/g, ''));
//                return isNaN(numValue) ? value : numValue.toFixed(3);
//            }
//            return value;
//        }
//    }).mask('.input-tp_plista, .input-tp_pcosto, .input-tp_pneto');

//    // Configuración para campos con 1 decimal (descuentos y flete)
//    Inputmask({
//        alias: "numeric",
//        groupSeparator: ",",
//        radixPoint: ".",
//        autoGroup: true,
//        digits: 1,
//        digitsOptional: false,
//        rightAlign: true,
//        integerDigits: 2, // Máximo 2 dígitos enteros
//        min: 0,
//        max: 99.9, // Máximo valor permitido: 99.9
//        prefix: '',
//        placeholder: "0",
//        clearMaskOnLostFocus: false,
//        showMaskOnHover: false,
//        showMaskOnFocus: false,
//        onBeforeMask: function (value) {
//            if (value) {
//                let numValue = parseFloat(value.toString().replace(/,/g, ''));
//                if (numValue > 99.9) numValue = 99.9; // Limitar al máximo permitido
//                return isNaN(numValue) ? value : numValue.toFixed(1);
//            }
//            return value;
//        }
//    }).mask('.input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete');

//    // Configuración para campos con 2 decimales (los demás campos numéricos)
//    Inputmask({
//        alias: "numeric",
//        groupSeparator: ",",
//        radixPoint: ".",
//        autoGroup: true,
//        digits: 2,
//        digitsOptional: false,
//        rightAlign: true,
//        prefix: '',
//        placeholder: "0",
//        clearMaskOnLostFocus: false,
//        showMaskOnHover: false,
//        showMaskOnFocus: false,
//        onBeforeMask: function (value) {
//            if (value) {
//                let numValue = parseFloat(value.toString().replace(/,/g, ''));
//                return isNaN(numValue) ? value : numValue.toFixed(2);
//            }
//            return value;
//        }
//    }).mask('.input-tp_margen, .input-tin_alicuota, .input-tp_pvta');

//    // Configuración para campo de bonificación (formato 999/999)
//    Inputmask({
//        mask: "999/999",
//        placeholder: "",
//        showMaskOnHover: false,
//        showMaskOnFocus: false
//    }).mask('.input-tp_boni');

//    console.log("Configuración de elementos de tabla detalle completada");
//}

// Función optimizada para aplicar InputMask
function configuracionInputMaskOptimizada() {
    console.log("Aplicando configuración InputMask optimizada...");

    // Establecer todos los campos como readonly de una sola vez
    $('.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni, .input-tp_pcosto, .input-tp_margen, .input-tp_pneto, .input-tin_alicuota, .input-tp_pvta')
        .prop('readonly', true)
        .addClass('campo-readonly');

    // Definir configuraciones de máscara fuera de los bucles
    const maskConfig3Decimales = {
        alias: "numeric",
        groupSeparator: ",",
        radixPoint: ".",
        autoGroup: true,
        digits: 3,
        digitsOptional: false,
        rightAlign: true,
        prefix: '',
        placeholder: "0",
        clearMaskOnLostFocus: false,
        showMaskOnHover: false,
        showMaskOnFocus: false,
        min: 0, // Explícitamente permitir 0 como valor mínimo
        allowMinus: false, // No permitir valores negativos
        onBeforeMask: function (value) {
            // Si es null, undefined o cadena vacía, retornar '0'
            if (value === null || value === undefined || value === '') {
                return '0';
            }

            // Para otros valores, formatear correctamente
            try {
                let numValue = parseFloat(value.toString().replace(/,/g, ''));
                return isNaN(numValue) ? '0' : numValue.toFixed(3);
            } catch (e) {
                console.error('Error al formatear valor:', e);
                return '0';
            }
        }
    };

    const maskConfig1Decimal = {
        alias: "numeric",
        groupSeparator: ",",
        radixPoint: ".",
        autoGroup: true,
        digits: 1,
        digitsOptional: false,
        rightAlign: true,
        integerDigits: 2,
        min: 0,
        max: 99.9,
        prefix: '',
        placeholder: "0",
        clearMaskOnLostFocus: false,
        showMaskOnHover: false,
        showMaskOnFocus: false,
        onBeforeMask: function (value) {
            if (value) {
                let numValue = parseFloat(value.toString().replace(/,/g, ''));
                if (numValue > 99.9) numValue = 99.9;
                return isNaN(numValue) ? value : numValue.toFixed(1);
            }
            return value;
        }
    };

    const maskConfig2Decimales = {
        alias: "numeric",
        groupSeparator: ",",
        radixPoint: ".",
        autoGroup: true,
        digits: 2,
        digitsOptional: false,
        rightAlign: true,
        prefix: '',
        placeholder: "0",
        clearMaskOnLostFocus: false,
        showMaskOnHover: false,
        showMaskOnFocus: false,
        onBeforeMask: function (value) {
            if (value) {
                let numValue = parseFloat(value.toString().replace(/,/g, ''));
                return isNaN(numValue) ? value : numValue.toFixed(2);
            }
            return value;
        }
    };

    const maskConfigBoni = {
        mask: "999/999",
        placeholder: "",
        showMaskOnHover: false,
        showMaskOnFocus: false
    };

    // Aplicar máscaras de forma eficiente con selección optimizada
    Inputmask(maskConfig3Decimales).mask('.input-tp_plista, .input-tp_pcosto, .input-tp_pneto');
    Inputmask(maskConfig1Decimal).mask('.input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete');
    Inputmask(maskConfig2Decimales).mask('.input-tp_margen, .input-tin_alicuota, .input-tp_pvta');
    Inputmask(maskConfigBoni).mask('.input-tp_boni');

    // Configurar eventos de edición
    configurarEventosEdicionOptimizado();

    console.log("Configuración InputMask aplicada");
}

// Función de debounce para evitar llamadas repetidas
function debounce(func, wait) {
    let timeout;
    return function () {
        const context = this, args = arguments;
        clearTimeout(timeout);
        timeout = setTimeout(function () {
            func.apply(context, args);
        }, wait);
    };
}

// Aplicar debounce a funciones de cálculo intensivas
const calcularCostoAPIDebounced = debounce(function (row) {
    calcularCostoAPI(row);
}, 300);

const calcularPrecioVentaAPIDebounced = debounce(function (row) {
    calcularPrecioVentaAPI(row);
}, 300);

const calcularPrecioVentaMargenAPIDebounced = debounce(function (row) {
    calcularPrecioVentaMargenAPI(row);
}, 300);

// Función con debounce para cálculo de margen en listas
const calcularPrecioVentaMargenListaDebounced = debounce(function (row, lpId, pId, nuevoPrecioVenta) {
    calcularPrecioVentaMargenLista(row, lpId, pId, nuevoPrecioVenta);
}, 300);

// NUEVA: Función para calcular margen en grid de listas (equivalente a secuencia03)
// NUEVA: Función para calcular margen en grid de listas (equivalente a secuencia03)
function calcularPrecioVentaMargenLista(row, lpId, pId, nuevoPrecioVenta) {
    console.log(`Iniciando cálculo de margen para lista LP ID: ${lpId}, P ID: ${pId}, Precio: ${nuevoPrecioVenta}`);

    // Validaciones de seguridad
    if (!row || !row.length) {
        console.error('Error: No se proporcionó una fila válida para calcular margen');
        return;
    }

    if (isNaN(nuevoPrecioVenta) || nuevoPrecioVenta <= 0) {
        console.error(`Error: Precio de venta inválido: ${nuevoPrecioVenta}`);
        return;
    }

    // Evitar cálculos redundantes
    const calculatingKey = `calculating-margin-lista-${lpId}`;
    if (row.data(calculatingKey) === true) {
        console.log('Ya hay un cálculo de margen en proceso para esta lista, evitando duplicación');
        return;
    }

    // Marcar que estamos calculando
    row.data(calculatingKey, true);

    // Recopilar parámetros desde los campos ocultos de la fila de lista
    const datos = {
        p_id: pId,
        lp_id: lpId,
        tp_pcosto: parseFloat(row.find('input[name="p_pcosto"]').val()) || 0,
        lp_prevision_tot: parseFloat(row.find('input[name="lp_prevision_tot"]').val()) || 0,
        lp_prevision_pin: parseFloat(row.find('input[name="lp_prevision_pin"]').val()) || 0,
        tp_pvta: nuevoPrecioVenta,
        iva_situacion: row.find('input[name="iva_situacion"]').val() || 'E',
        iva_alicuota: parseFloat(row.find('input[name="iva_alicuota"]').val()) || 0,
        in_alicuota: parseFloat(row.find('input[name="in_alicuota"]').val()) || 0
    };

    console.log('Parámetros para cálculo de margen en lista:', datos);

    // Mostrar indicador de carga en el campo de precio de venta
    const campoPVenta = row.find('.input-tp_pvta_lista');
    const valorOriginal = campoPVenta.val();
    campoPVenta.addClass('calculating');

    // Llamar a la API usando la misma URL que secuencia03
    $.ajax({
        url: calcularPrecioVentaMargenUrl,
        type: 'POST',
        data: datos,
        dataType: 'json',
        success: function (response) {
            // Desmarcar estado de cálculo
            row.data(calculatingKey, false);
            campoPVenta.removeClass('calculating');

            console.log('Respuesta del cálculo de margen en lista:', response);

            if (response.error === true) {
                // Manejo del error - restaurar valor original
                campoPVenta.val(valorOriginal);
                console.error('Error en cálculo de margen para lista:', response.msg);
                AbrirMensaje("Error", "Error al calcular margen: " + response.msg,
                    function () { $("#msjModal").modal("hide"); },
                    false, ["Aceptar"], "error!", null);
            } else if (response.warn === true) {
                // Manejo de advertencia
                campoPVenta.val(valorOriginal);
                console.warn('Advertencia en cálculo de margen para lista:', response.msg);
                AbrirMensaje("Atención", response.msg,
                    function () { $("#msjModal").modal("hide"); },
                    false, ["Aceptar"], "warn!", null);
            } else {
                // Éxito: actualizar los campos ocultos con los nuevos valores calculados
                if (response.pvta) {
                    // *** PASO 1: ACTUALIZAR CAMPOS OCULTOS SEGÚN ESPECIFICACIÓN ***
                    row.find('input[name="tp_pneto"]').val(response.pvta.p_pneto);
                    row.find('input[name="tp_margen"]').val(response.pvta.p_margen);
                    row.find('input[name="tp_iva"]').val(response.pvta.p_iva);
                    row.find('input[name="tp_in"]').val(response.pvta.p_in);

                    // *** PASO 2: ACTUALIZAR EL CAMPO VISIBLE DE MARGEN ***
                    const campoMargenVisible = row.find('.input-tp_margen_lista');
                    if (campoMargenVisible.length > 0) {
                        campoMargenVisible.val(parseFloat(response.pvta.p_margen).toFixed(2));
                        campoMargenVisible.data('original-value', parseFloat(response.pvta.p_margen));
                        marcarCampoModificadoLista(campoMargenVisible);
                    }

                    // *** PASO 3: MARCAR EL CAMPO DE PRECIO DE VENTA COMO MODIFICADO ***
                    campoPVenta.data('original-value', nuevoPrecioVenta);
                    marcarCampoModificadoLista(campoPVenta);

                    // *** PASO 4: RESGUARDAR AUTOMÁTICAMENTE LOS CAMBIOS ***
                    resguardarCambiosListaCalculados(row, lpId, pId, nuevoPrecioVenta, response.pvta);

                    console.log('Margen calculado y actualizado en lista:');
                    console.log('  Precio neto:', response.pvta.p_pneto);
                    console.log('  Margen:', response.pvta.p_margen);
                    console.log('  IVA:', response.pvta.p_iva);
                    console.log('  Impuesto interno:', response.pvta.p_in);
                } else {
                    console.error('La respuesta no contiene los datos esperados:', response);
                }
            }
        },
        error: function (xhr, status, error) {
            // Error en la petición
            row.data(calculatingKey, false);
            campoPVenta.removeClass('calculating').val(valorOriginal);

            console.error('Error en la llamada AJAX para calcular margen en lista:', error);
            AbrirMensaje("Error", "Error de comunicación con el servidor. Inténtelo nuevamente.",
                function () { $("#msjModal").modal("hide"); },
                false, ["Aceptar"], "error!", null);
        }
    });
}

// NUEVA: Función específica para resguardar cambios después de cálculos de margen
function resguardarCambiosListaCalculados(row, lpId, pId, nuevoPrecioVenta, datosCalculados) {
    console.log(`Resguardando cambios calculados para lista LP ID: ${lpId}, P ID: ${pId}`);

    // Recopilar datos completos para el resguardo, incluyendo los valores calculados
    const datos = {
        p_id: pId,
        lp_id: lpId,
        tp_margen: parseFloat(datosCalculados.p_margen) || 0, // *** USAR VALOR CALCULADO ***
        tp_pvta: nuevoPrecioVenta,
        p_pcosto: parseFloat(row.find('input[name="p_pcosto"]').val()) || 0,
        p_pneto: parseFloat(datosCalculados.p_pneto) || 0, // *** USAR VALOR CALCULADO ***
        lp_porc_mg: parseFloat(row.find('input[name="lp_porc_mg"]').val()) || 0,
        iva_situacion: row.find('input[name="iva_situacion"]').val() || 'E',
        iva_alicuota: parseFloat(row.find('input[name="iva_alicuota"]').val()) || 0,
        in_alicuota: parseFloat(row.find('input[name="in_alicuota"]').val()) || 0,
        tp_iva: parseFloat(datosCalculados.p_iva) || 0, // *** USAR VALOR CALCULADO ***
        tp_in: parseFloat(datosCalculados.p_in) || 0 // *** USAR VALOR CALCULADO ***
    };

    console.log('Datos que se resguardarán después del cálculo:', datos);

    // Llamar al servidor para resguardar los cambios
    $.ajax({
        url: resguardarCambiosProductoListaUrl,
        type: 'POST',
        data: datos,
        dataType: 'json',
        success: function (response) {
            if (response.error) {
                console.error('Error al resguardar cambios calculados en lista:', response.msg);
                AbrirMensaje("Error", "No se pudieron guardar los cambios calculados: " + response.msg,
                    function () { $("#msjModal").modal("hide"); },
                    false, ["Aceptar"], "error!", null);
            } else if (response.warn) {
                console.warn('Advertencia al resguardar cambios calculados en lista:', response.msg);
            } else {
                console.log('Cambios calculados en lista resguardados correctamente:', response.msg);

                // *** ACTUALIZAR VALORES ORIGINALES PARA FUTURAS COMPARACIONES ***
                const campoMargen = row.find('.input-tp_margen_lista');
                const campoPVenta = row.find('.input-tp_pvta_lista');

                campoMargen.data('original-value', parseFloat(datosCalculados.p_margen));
                campoPVenta.data('original-value', nuevoPrecioVenta);

                console.log('Valores originales actualizados después del resguardo');
            }
        },
        error: function (xhr, status, error) {
            console.error('Error en la llamada AJAX al resguardar cambios calculados:', error);
            AbrirMensaje("Error", "Error de comunicación al resguardar cambios. Inténtelo nuevamente.",
                function () { $("#msjModal").modal("hide"); },
                false, ["Aceptar"], "error!", null);
        }
    });
}

// Función de depuración mejorada que verifica todos los campos
function depurarValoresIniciales() {
    console.log("=== DEPURACIÓN DE VALORES INICIALES ===");

    // Agrupar todos los selectores para campos con 3 decimales
    $('.input-tp_plista').each(function (index) {
        let value = $(this).val();
        let originalValue = $(this).data('original-value');
        console.log(`Campo tp_plista ${index + 1}: valor=${value}, original=${originalValue}`);
    });

    // Agrupar todos los selectores para campos con 2 decimales
    $('.input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete').each(function (index) {
        let value = $(this).val();
        let originalValue = $(this).data('original-value');
        let fieldClass = $(this).attr('class').match(/input-tp_[^\s]+/)[0];
        console.log(`Campo ${fieldClass} ${index + 1}: valor=${value}, original=${originalValue}`);
    });

    // Revisar campos de bonificación
    $('.input-tp_boni').each(function (index) {
        let value = $(this).val();
        console.log(`Campo tp_boni ${index + 1}: valor=${value}`);
    });
}

// Función de utilidad para destacar la fila seleccionada
function destacarFilaSeleccionada(productoId) {
    console.log("Destacando fila para producto ID:", productoId);

    // Remover el destacado de todas las filas
    $("#tbProdDet tbody tr").removeClass("selected");

    // Verificar que existe una fila con ese ID
    const $fila = $("#tbProdDet tbody tr[data-p-id='" + productoId + "']");

    if ($fila.length === 0) {
        console.warn(`No se encontró ninguna fila con data-p-id="${productoId}"`);
        return;
    }

    // Añadir el destacado solo a la fila del producto seleccionado
    $fila.addClass("selected");
    console.log("Fila destacada correctamente");

    // Opcionalmente, hacer scroll a la fila seleccionada si está fuera de la vista
    const $tableContainer = $("#tbProdDet").closest('.table-responsive');

    if ($tableContainer.length > 0) {
        const containerTop = $tableContainer.offset().top;
        const rowTop = $fila.offset().top;

        if (rowTop < containerTop || rowTop > containerTop + $tableContainer.height()) {
            $tableContainer.animate({
                scrollTop: $tableContainer.scrollTop() + (rowTop - containerTop)
            }, 300);
            console.log("Realizando scroll a la fila seleccionada");
        }
    } else {
        console.warn("No se encontró un contenedor .table-responsive para la tabla");
    }
}

// Nueva función para recalcular valores cuando cambia un campo
function recalcularValores(changedField) {
    // Obtener el ID del producto
    let productId = changedField.data('p-id');
    let row = changedField.closest('tr');

    // Si el campo cambiado es uno de los que afecta al costo
    if (changedField.hasClass('input-tp_plista') ||
        changedField.hasClass('input-tp_dto1') ||
        changedField.hasClass('input-tp_dto2') ||
        changedField.hasClass('input-tp_dto3') ||
        changedField.hasClass('input-tp_dto4') ||
        changedField.hasClass('input-tp_dto_pa') ||
        changedField.hasClass('input-tp_porc_flete') ||
        changedField.hasClass('input-tp_boni')) {

        // Recalcular costo (esto sería un ejemplo, el cálculo real dependería de la lógica de negocio)
        recalcularCosto(row);
    }

    // Si el campo cambiado es el margen
    if (changedField.hasClass('input-tp_margen')) {
        // Recalcular precio neto basado en el costo y el nuevo margen
        recalcularPrecioNeto(row);
    }

    // Si el campo cambiado es precio de venta o impuesto interno
    if (changedField.hasClass('input-tp_pvta') || changedField.hasClass('input-tin_alicuota')) {
        // Recalcular relación con precio venta
        recalcularRelacionPrecioVenta(row);
    }
}

// Función auxiliar para los recálculos (modificada para evitar llamadas redundantes)
function recalcularCosto(row) {
    // Si ya estamos calculando el costo para esta fila, no hacer nada
    if (row.data('calculating-cost') === true) {
        return;
    }

    // Esta función ahora simplemente llama a calcularCostoAPI
    // que se encargará de todo el proceso de cálculo
    calcularCostoAPI(row);
}


function recalcularPrecioNeto(row) {
    // Obtener el costo
    let costo = parseFloat(row.find('input[data-original-value]').filter(function () {
        return $(this).closest('td').hasClass(row.find('.input-tp_pcosto').closest('td').attr('class'));
    }).val().replace(/,/g, ''));

    // Obtener el margen
    let margen = parseFloat(row.find('.input-tp_margen').val().replace(/,/g, ''));

    // Calcular precio neto
    let precioNeto = costo;
    if (!isNaN(margen) && margen > 0) {
        precioNeto = costo * (1 + margen / 100);
    }

    // Actualizar el campo de precio neto (readonly)
    row.find('input[data-original-value]').filter(function () {
        return $(this).closest('td').hasClass(row.find('input[data-original-value="' + row.find('.input-tp_pneto').data('original-value') + '"]').closest('td').attr('class'));
    }).val(precioNeto.toFixed(2));
}

function recalcularRelacionPrecioVenta(row) {
    // Esta función calcularía la relación entre precio de venta y otros valores
    // La implementación dependería de la lógica de negocio específica
}


function cargaEventosCP() {
    // Observar la adición de elementos mediante MutationObserver
    const listObserver = new MutationObserver(function (mutations) {
        mutations.forEach(function (mutation) {
            if (mutation.type === 'childList' && mutation.addedNodes.length > 0) {
                verificarYDesactivarControles();
            }
        });
    });

    // Configurar y comenzar la observación si el elemento existe
    if (document.getElementById('Rel01List')) {
        listObserver.observe(document.getElementById('Rel01List'), {
            childList: true,
            subtree: true
        });
    }

    // Manejar los eventos específicos para el control de lista (solo change)
    $("#Rel01List").on("change", function () {
        verificarYDesactivarControles();
    });

    // Si el autocompletado de Rel01 selecciona un ítem
    $("#Rel01").on("autocompleteselect", function () {
        // Esperar brevemente para que el autocompletado actualice la lista
        setTimeout(verificarYDesactivarControles, 100);
    });

    // Verificar también después de que el documento está completamente cargado
    $(function () {
        verificarYDesactivarControles();
    });

    // Verificación periódica más controlada (con menos ruido en la consola)
    let checkCount = 0;
    const intervalCheck = setInterval(function () {
        // Verificar solo si aún no hay elementos y no estamos deshabilitados
        if ($("#Rel01List").find("option").length === 0 && !$("#Rel01").prop("disabled")) {
            checkCount++;
            verificarYDesactivarControles(false); // Pasar false para no loguear en cada intento
        } else {
            // Si ya encontramos elementos o ya están deshabilitados, parar el intervalo
            clearInterval(intervalCheck);
        }

        // Detener después de 10 intentos incluso si no encontramos nada
        if (checkCount >= 10) {
            clearInterval(intervalCheck);
        }
    }, 500);

    // Extender la funcionalidad del evento click en chkRel01
    // Este evento ya está definido en siteGen.js, pero necesitamos añadir más comportamiento
    $("#chkRel01").on("change", function () {
        // Si el checkbox se desmarca, desactivar y limpiar los controles de Rel03
        if (!$(this).is(":checked")) {
            // Desactivar y desmarcar chkRel03
            $("#chkRel03").prop("checked", false);
            $("#chkRel03").prop("disabled", true);

            // Limpiar y desactivar Rel03
            $("#Rel03").val("");
            $("#Rel03").prop("disabled", true);

            // Limpiar y desactivar Rel03List
            $("#Rel03List").empty();
            $("#Rel03List").prop("disabled", true);

            console.log("Se ha desactivado el filtro de proveedor y se ha limpiado el filtro de familia");
        }
        // Si se marca, no hacemos nada especial aquí, el código existente ya maneja ese caso
    });

    // Evento change para el combo Rel03
    $("#Rel03").on("change", function () {
        const selectedValue = $(this).val();
        const selectedText = $(this).find("option:selected").text();

        if (selectedValue && selectedValue !== "") {
            // Agregar la opción seleccionada a Rel03List si no existe ya
            if ($("#Rel03List option[value='" + selectedValue + "']").length === 0) {
                $("#Rel03List").append(
                    $("<option></option>")
                        .attr("value", selectedValue)
                        .text(selectedText)
                        .prop("selected", true)
                );

                console.log("Familia seleccionada agregada a la lista: " + selectedText);

                // También guardar el valor en el campo oculto Rel03Item si existe
                if ($("#Rel03Item").length > 0) {
                    $("#Rel03Item").val(selectedValue);
                }
            }

            // Limpiar la selección en el combo original después de agregarla a la lista
            $(this).val("");
        }
    });

    // Evento change para el checkbox chkFile
    $("#chkFile").on("change", function () {
        // Si el checkbox se activa, desactivar todos los controles excepto los relacionados con Rel01
        if ($(this).is(":checked")) {
            // Guardar el estado actual de los controles Rel01 antes de desactivar todo
            const rel01Checked = $("#chkRel01").is(":checked");
            const rel01Disabled = $("#Rel01").prop("disabled");
            const rel01ListDisabled = $("#Rel01List").prop("disabled");
            const rel01Value = $("#Rel01").val();
            const rel01ItemValue = $("#Rel01Item").val();
            const rel01ListOptions = $("#Rel01List").html();

            // Desactivar todos los checkboxes excepto chkFile y chkRel01
            $("input[type='checkbox']").not("#chkFile, #chkRel01").prop({
                "checked": false,
                "disabled": true
            });

            // Desactivar todos los inputs de texto excepto Rel01
            $("input[type='text']").not("#Rel01").prop("disabled", true);

            // Desactivar todos los select excepto Rel01List
            $("select").not("#Rel01List").prop("disabled", true).empty();


            // Limpiar específicamente los controles de Rel02
            $("#Rel02").val("");
            $("#Rel02Item").val("");
            $("#Rel02List").empty();
            $("#chkRel02").prop("checked", false);
            $("#chkRel02").prop("disabled", true);

            // Restaurar el estado de los controles Rel01
            $("#chkRel01").prop("checked", rel01Checked);

            // Solo si Rel01 no estaba desactivado previamente, lo dejamos activo
            if (!rel01Disabled) {
                $("#Rel01").prop("disabled", false);
            }

            // Solo si Rel01List no estaba desactivado previamente, lo dejamos activo
            if (!rel01ListDisabled) {
                $("#Rel01List").prop("disabled", false);
            }

            console.log("Modo archivo activado: Solo se permite filtrar por proveedor");
        } else {
            // Al desactivar el checkbox, restaurar el comportamiento normal
            // Primero habilitamos los controles básicos
            $("input[type='checkbox']").not("#chkFile").prop("disabled", false);

            // Luego verificamos la lógica de negocio específica
            if ($("#chkRel01").is(":checked") && $("#Rel01List").find("option").length > 0) {
                // Si hay un proveedor seleccionado, habilitar filtro de familia
                $("#chkRel03").prop("disabled", false);

                // Si el filtro de familia está activado, habilitar sus controles
                if ($("#chkRel03").is(":checked")) {
                    $("#Rel03").prop("disabled", false);
                    $("#Rel03List").prop("disabled", false);
                }
            } else {
                // Si no hay proveedor, deshabilitar familia
                $("#chkRel03").prop("disabled", true);
                $("#Rel03").prop("disabled", true);
                $("#Rel03List").prop("disabled", true);
            }


            // Habilitar controles de Rel02 (Rubro)
            $("#chkRel02").prop("disabled", false);
            if ($("#chkRel02").is(":checked")) {
                $("#Rel02").prop("disabled", false);
                $("#Rel02List").prop("disabled", false);
            }

            console.log("Modo archivo desactivado: Se restauran los filtros normales");
        }
    });

    $(document).on('blur', 'input.form-control-sm', function () {
        marcarCampoModificado(this);
    });
}

// Función centralizada para verificar y desactivar los controles
function verificarYDesactivarControles(mostrarLog = true) {
    // Verificar si hay opciones en la lista
    if ($("#Rel01List").find("option").length > 0) {
        if (mostrarLog) {
            console.log("Se encontraron opciones en Rel01List, desactivando controles...");
        }

        // Asegurar que solo hay un elemento seleccionado
        const opciones = $("#Rel01List option");
        if (opciones.length > 0) {
            // Seleccionar solo el primer elemento
            const primerValor = opciones.first().val();
            $("#Rel01List").val([primerValor]);

            // Aplicar la desactivación inmediatamente
            $("#Rel01List").prop("disabled", true);
            $("#Rel01").prop("disabled", true);

            // Habilitar el control de familia ya que ahora podemos seleccionar familia
            // Pero solo si chkFile no está marcado
            if (!$("#chkFile").is(":checked")) {
                $("#chkRel03").prop("disabled", false);
            }

            // Obtener el ID del proveedor seleccionado
            const proveedorId = $("#Rel01Item").val() || primerValor;

            // Cargar las familias relacionadas con este proveedor, solo si chkFile no está marcado
            if (!$("#chkFile").is(":checked")) {
                cargarFamiliasDelProveedor(proveedorId);
            }

            if (mostrarLog) {
                console.log("Controles desactivados correctamente");
            }
        }
    } else if (mostrarLog && $("#Rel01").val()) {
        console.log("No hay opciones en Rel01List todavía, pero hay texto en Rel01");
    }
}

function cargarDatosEnVistaPrevia(productoId) {
    console.log("Cargando datos originales en vista previa para producto ID:", productoId);

    // Verificar que el producto exista en la tabla
    const $fila = $("#tbProdDet tbody tr[data-p-id='" + productoId + "']");

    if ($fila.length === 0) {
        console.warn(`No se encontró ninguna fila con data-p-id="${productoId}"`);
        return;
    }

    // Obtener los valores originales de los campos relevantes
    const datosOriginales = {
        plista: $fila.find('.input-tp_plista').data('original-value') || '',
        dto1: $fila.find('.input-tp_dto1').data('original-value') || '',
        dto2: $fila.find('.input-tp_dto2').data('original-value') || '',
        dto3: $fila.find('.input-tp_dto3').data('original-value') || '',
        dto4: $fila.find('.input-tp_dto4').data('original-value') || '',
        dpo: $fila.find('.input-tp_dto_pa').data('original-value') || '',
        bon: $fila.find('.input-tp_boni').data('original-value') || '',
        fl: $fila.find('.input-tp_porc_flete').data('original-value') || ''
    };

    // Verificar si existe la vista previa (_datosGenerales)
    if ($('.input-PListaValor, .input-Dto1Valor, .input-Dto2Valor, .input-Dto3Valor, .input-Dto4Valor, .input-DpoValor, .input-BonValor, .input-FlValor').length > 0) {
        // Asignar los valores a los campos correspondientes de la vista previa
        $('.input-PListaValor').val(datosOriginales.plista);
        $('.input-Dto1Valor').val(datosOriginales.dto1);
        $('.input-Dto2Valor').val(datosOriginales.dto2);
        $('.input-Dto3Valor').val(datosOriginales.dto3);
        $('.input-Dto4Valor').val(datosOriginales.dto4);
        $('.input-DpoValor').val(datosOriginales.dpo);
        $('.input-BonValor').val(datosOriginales.bon);
        $('.input-FlValor').val(datosOriginales.fl);

        console.log("Datos originales cargados en vista previa:", datosOriginales);
    } else {
        console.warn("No se encontraron los campos de vista previa en _datosGenerales");
    }
}

// Función para cargar las familias relacionadas con un proveedor
function cargarFamiliasDelProveedor(proveedorId) {
    // No cargar familias si estamos en modo archivo
    if ($("#chkFile").is(":checked")) {
        return;
    }

    if (!proveedorId) {
        console.error("No se pudo determinar el ID del proveedor");
        return;
    }

    console.log("Cargando familias para el proveedor con ID: " + proveedorId);
    let datos = { ctaId: proveedorId };
    // Usar PostGen para llamar al controlador
    PostGen(datos, buscarFamiliaUrl, // URL del action 
        function (obj) { // Función de éxito
            if (obj.error === true) {
                CerrarWaiting();
                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    $("#msjModal").modal("hide");
                }, false, ["Entendido"], "error!", null);
            }
            else if (obj.warn === true) {
                CerrarWaiting();
                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    $("#msjModal").modal("hide");
                }, false, ["Entendido"], "warn!", null);
            }
            else {
                //armado del ddl de Familia
                var combo = $("#Rel03");
                // Limpiar el dropdown actual
                combo.empty();
                var opc = "<option value=''>Seleccionar...</option>";
                combo.append(opc);
                $.each(obj.lista, function (i, item) {
                    opc = "<option value='" + item.id + "'>" + item.descripcion + "</option>";
                    combo.append(opc);
                });
                CerrarWaiting();
            }
        },
        function (error) { // Función de error
            console.error("Error al cargar las familias del proveedor:", error);
        }
    );
}

function configurarBotonesProdCP() {
    // Botón de cancelar
    $("#btnCancel").on("click", function () {
        window.location.href = homeCPUrl;
    });

    // Evento para el botón buscar
    $("#btnBuscar").on("click", function (e) {
        e.preventDefault();

        // Verificar si se ha seleccionado un proveedor mediante la variable consCta
        // Esta variable debería contener el ID de la cuenta del proveedor si se ha seleccionado
        if (typeof consCta === 'undefined' || !consCta) {
            // Si no hay proveedor seleccionado, mostrar mensaje de advertencia
            AbrirMensaje(
                "ATENCIÓN",
                "Debe seleccionar un proveedor antes de realizar la búsqueda.",
                function () {
                    $("#msjModal").modal("hide");
                },
                false,
                ["Entendido"],
                "warn!",
                null
            );
            return false; // Detener la ejecución
        }

        AbrirWaiting("Cargando los productos del proveedor según el filtro especificado. Por favor espere...");
        // Si hay un proveedor seleccionado, continuar con la búsqueda
        buscarProductosDetalle();
        //Presenta el control comun de cuenta
        inicializaControlCuenta();
    });

    //inicializo botones aceptar y confirmar desactivados y ocultos
    $("#btnAbmAceptar").prop("disabled", true);//.hide();
    $("#btnAbmCancelar").prop("disabled", true);//.hide();

    $("#btnFiltro").on("mousedown", function () {
        if ($("#divFiltro").is(":hidden")) {
            $("#divDetalle").collapse("hide");
        }
    });

    $("#btnDetalle").on("mousedown", function () {
        if ($("#divDetalle").is(":visible")) {
            $("divPCP").empty();
            $("#btnDetalle").collapse("hide");
            $("#btnFiltro").collapse("show");
        }
    });

    $("#lbRel01").text("PROVEEDOR");
    $("#lbRel02").text("RUBRO");
    $("#lbRel03").text("FAMILIA");

    //al inicializar el modulo, la familia debe estar desactivada hasta que se seleccione un proveedor
    $("#chkRel03").prop("disabled", true);

    // Verificar si los controles deben estar desactivados cuando se configuren los botones
    setTimeout(function () {
        verificarYDesactivarControles(true);

        // Verificar también si chkFile está marcado al inicio
        if ($("#chkFile").is(":checked")) {
            $("#chkFile").trigger("change");
        }
    }, 100);

    // Agregar un manejador específico para cuando siteGen.js haya completado su trabajo
    $(document).on("autocompleteready", function () {
        verificarYDesactivarControles(true);
    });

    // Agregar evento al checkbox de familia para habilitar/deshabilitar la selección
    $("#chkRel03").on("change", function () {
        if ($(this).is(":checked")) {
            $("#Rel03").prop("disabled", false);
            $("#Rel03List").prop("disabled", false);
        } else {
            $("#Rel03").prop("disabled", true);
            $("#Rel03List").prop("disabled", true);

            // Limpiar la selección cuando se desmarca el checkbox
            $("#Rel03").val("");
            $("#Rel03List").empty();
            if ($("#Rel03Item").length > 0) {
                $("#Rel03Item").val("");
            }
        }
    });

    // Asegurarse de que al iniciar, si chkRel01 no está marcado, Rel03 esté desactivado
    if (!$("#chkRel01").is(":checked")) {
        $("#chkRel03").prop("checked", false);
        $("#chkRel03").prop("disabled", true);
        $("#Rel03").val("");
        $("#Rel03").prop("disabled", true);
        $("#Rel03List").empty();
        $("#Rel03List").prop("disabled", true);
    }

    //// Asegurarse de que los campos vuelvan a estado readonly si el usuario hace clic en otra parte
    //$(document).off('click').on('click', function (e) {
    //    if (!$(e.target).is('.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni, .input-tp_margen, .input-tin_alicuota, .input-tp_pvta')) {
    //        // Si se hizo clic fuera de los inputs y hay alguno activo, desactivarlo
    //        $('.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni, .input-tp_margen, .input-tin_alicuota, .input-tp_pvta').each(function () {
    //            if (!$(this).prop('readonly')) {
    //                // En lugar de usar .blur() directamente, que está deprecated
    //                // Disparamos el evento blur de forma manual en el elemento DOM nativo
    //                const event = new Event('blur', { bubbles: true });
    //                this.dispatchEvent(event);
    //            }
    //        });
    //    }
    //});

    // Usar un namespace específico para evitar conflictos
    $(document).off('click.productoCargaPrecio').on('click.productoCargaPrecio', function (e) {
        // Solo actuar si el clic fue en el área de la tabla de productos o en elementos relacionados
        const $target = $(e.target);

        // Verificar si el clic fue en un área donde queremos controlar los inputs
        const enAreaControlada = $target.closest('#tbProdDet, #divPCP').length > 0;

        // Si no estamos en el área controlada, no hacer nada (permitir otros eventos)
        if (!enAreaControlada) {
            return;
        }

        // Verificar si el clic fue en inputs específicos de precio
        const esInputPrecio = $target.is('.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni, .input-tp_margen, .input-tin_alicuota, .input-tp_pvta');

        if (!esInputPrecio) {
            // Si se hizo clic fuera de los inputs de precio Y estamos en el área controlada, desactivar campos activos
            $('.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni, .input-tp_margen, .input-tin_alicuota, .input-tp_pvta').each(function () {
                if (!$(this).prop('readonly')) {
                    // En lugar de usar .blur() directamente
                    const event = new Event('blur', { bubbles: true });
                    this.dispatchEvent(event);
                }
            });
        }
    });
}

function obtenerParametros(div) {
    // Obtener valores de los filtros
    const proveedor = $("#Rel01Item").val() || $("#Rel01List").val();

    // Validar que se haya seleccionado un proveedor
    if (!proveedor || proveedor === "") {
        mostrarMensajeError("Debe seleccionar un proveedor para realizar la búsqueda.");
        return false;
    }

    // Obtener el resto de parámetros
    const buscar = $("#Buscar").val() || "";
    const id = $("#Id").val() || "";
    const id2 = $("#Id2").val() || "";

    // Obtener rubros seleccionados
    const rubros = [];
    $("#Rel02List option").each(function () {
        // Agregar todos los elementos de la lista, estén seleccionados o no
        rubros.push($(this).val());
    });


    // Obtener familias seleccionadas
    const familias = [];
    $("#Rel03List option").each(function () {
        // Agregar todos los elementos de la lista, estén seleccionados o no
        familias.push({
            id: $(this).val(),
            descripcion: $(this).text()
        });
    });

    // Verificar opciones adicionales
    const incluirDiscontinuos = $("#Opt1").prop("checked");
    const generarArchivo = $("#Opt2").prop("checked");

    // Mostrar indicador de carga
    $(div).html('<div class="text-center p-3"><i class="bx bx-loader bx-spin font-size-24"></i><p class="mt-2">Cargando datos...</p></div>');
    return {
        buscar: buscar,
        id: id,
        id2: id2,
        ctaId: proveedor,
        familias: familias,
        rubros: rubros,
        disc: incluirDiscontinuos,
        file: generarArchivo
    };
}

// Modificar la función buscarProductosDetalle para asegurar la correcta secuencia de inicialización
function buscarProductosDetalle() {
    let datos = obtenerParametros(divs.ProductoDetalle);
    if (!datos) return false;

    // Mostrar indicador de carga general
    AbrirWaiting("Cargando productos. Por favor espere...");

    // Realizar petición AJAX
    $.ajax({
        url: buscarProdDetUrl,
        type: "POST",
        data: datos,
        success: function (response) {
            // Mostrar resultados en el contenedor
            $("#divPCP").html(response);

            // Si hay resultados, inicializar la tabla con carga progresiva
            if ($(response).find("tbody tr").length > 0) {
                // Configurar visualización
                $("#divFiltro").removeClass("show");
                $("#divDetalle").addClass("show");

                // Inicializar con método optimizado
                inicializarTablaProductos();

                // Obtener el primer producto después de completar la inicialización básica
                setTimeout(function () {
                    const primerProductoId = $("#tbProdDet tbody tr:not(.table-secondary):first").data("p-id");
                    if (primerProductoId) {
                        destacarFilaSeleccionada(primerProductoId);

                        // NUEVO: Cargar los datos originales en la vista previa
                        cargarDatosEnVistaPrevia(primerProductoId);

                        buscarProductoListaOptimizado(primerProductoId);
                    }

                    // Cerrar el indicador de carga general cuando todo está listo
                    CerrarWaiting();
                }, 300);
            } else {
                CerrarWaiting();
                mostrarMensajeError("No se encontraron productos que coincidan con los criterios de búsqueda.");
            }
        },
        error: function (error) {
            CerrarWaiting();
            console.error("Error al obtener productos:", error);
            mostrarMensajeError("Se produjo un error al buscar los productos. Por favor, inténtelo de nuevo más tarde.");
        }
    });

    return false;
}

// Nueva función que agrupa las inicializaciones en el orden correcto
function inicializarTablaProductos() {
    console.time("inicializacionTabla");

    // Mostrar indicador de progreso
    $('<div id="loadingIndicator" class="position-fixed top-50 start-50 translate-middle bg-white p-3 rounded shadow-lg" style="z-index:1050;">' +
        '<div class="text-center"><i class="bx bx-loader bx-spin font-size-24"></i>' +
        '<p class="mt-2 mb-0">Inicializando Productos...</p>' +
        '<div id="progressInfo" class="mt-2 small text-muted">Preparando componentes...</div></div></div>')
        .appendTo('body');

    // Procesar en modo asíncrono para no bloquear la UI
    setTimeout(function () {
        // Fase 1: Configuración básica (rápida)
        asegurarEstilosCamposModificados();
        asegurarAtributosCarga();

        // Actualizar progreso
        $("#progressInfo").text("Configurando eventos (25%)...");

        // Fase 2: Eventos y marcado (media)
        setTimeout(function () {
            configurarEventosTabla();
            actualizarCamposModificadosOptimizado();

            // Actualizar progreso
            $("#progressInfo").text("Aplicando formato (50%)...");

            // Fase 3: Formato y validación (lenta - procesamiento por lotes)
            setTimeout(function () {
                iniciarProcesamientoLotes();
            }, 10);
        }, 10);
    }, 10);
}

// Procesamiento por lotes para evitar congelar la UI
function iniciarProcesamientoLotes() {
    const filas = $('#tbProdDet tbody tr:not(.table-secondary)');
    const totalFilas = filas.length;
    const tamanoLote = 25; // Procesar 25 filas a la vez

    // Iniciar el procesamiento
    procesarLote(filas, 0, tamanoLote, totalFilas);
}

function procesarLote(filas, inicio, tamanoLote, totalFilas) {
    const fin = Math.min(inicio + tamanoLote, totalFilas);

    // Procesar este lote
    for (let i = inicio; i < fin; i++) {
        const fila = filas[i];
        const $fila = $(fila);

        // Formatear valores en esta fila
        $fila.find('.input-tp_plista, .input-tp_pcosto, .input-tp_pneto').each(function () {
            const $input = $(this);
            let originalValue = $input.data('original-value');

            if (originalValue !== undefined && !$input.hasClass('campo-modificado')) {
                let numValue = parseFloat(originalValue);
                if (!isNaN(numValue)) {
                    $input.val(numValue.toFixed(3));
                }
            }
        });

        // Formatear campos con 1 decimal
        $fila.find('.input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete').each(function () {
            const $input = $(this);
            let originalValue = $input.data('original-value');

            if (originalValue !== undefined && !$input.hasClass('campo-modificado')) {
                let numValue = parseFloat(originalValue);
                if (!isNaN(numValue)) {
                    numValue = Math.min(numValue, 99.9);
                    $input.val(numValue.toFixed(1));
                }
            }
        });

        // Formatear campos con 2 decimales
        $fila.find('.input-tp_margen, .input-tin_alicuota, .input-tp_pvta').each(function () {
            const $input = $(this);
            let originalValue = $input.data('original-value');

            if (originalValue !== undefined && !$input.hasClass('campo-modificado')) {
                let numValue = parseFloat(originalValue);
                if (!isNaN(numValue)) {
                    $input.val(numValue.toFixed(2));
                }
            }
        });

        // Normalizar bonificaciones
        $fila.find('.input-tp_boni').each(function () {
            const $input = $(this);
            let originalValue = $input.data('original-value');

            if (originalValue !== undefined && originalValue.toString().trim() === '0' && !$input.hasClass('campo-modificado')) {
                $input.val('');
                $input.data('original-value', '');
            }
        });
    }

    // Actualizar progreso
    const porcentaje = Math.round((fin / totalFilas) * 100);
    $("#progressInfo").text(`Procesando filas... (${porcentaje}%)`);

    // Si quedan filas por procesar, programar el siguiente lote
    if (fin < totalFilas) {
        setTimeout(function () {
            procesarLote(filas, fin, tamanoLote, totalFilas);
        }, 1); // Mínimo retraso para permitir actualizar la UI
    } else {
        // Todas las filas procesadas, completar la inicialización
        finalizarInicializacion();
    }
}

function finalizarInicializacion() {
    $("#progressInfo").text("Aplicando máscaras de entrada (75%)...");

    setTimeout(function () {
        // Fase 4: Configuración final (InputMask y optimización visual)
        configuracionInputMaskOptimizada();
        optimizarVisualizacionTabla();

        // Eliminar indicador de carga
        $("#progressInfo").text("¡Completado! (100%)");
        setTimeout(function () {
            $("#loadingIndicator").fadeOut(300, function () {
                $(this).remove();
            });
        }, 500);

        console.timeEnd("inicializacionTabla");
    }, 10);
}




// Añadir un observador de mutaciones para mantener las marcas tras manipulaciones del DOM
function inicializarObservadorDOM() {
    // Si el navegador soporta MutationObserver
    if (window.MutationObserver) {
        const config = {
            childList: true,
            subtree: true,
            characterData: true,
            attributeFilter: ['value', 'class']
        };

        const observer = new MutationObserver(function (mutations) {
            let requiereActualizacion = false;

            mutations.forEach(function (mutation) {
                // FILTRAR CAMBIOS RELEVANTES PARA CAMPOS MODIFICADOS
                if (mutation.type === 'attributes' && mutation.attributeName === 'value') {
                    // Solo cambios en valores de campos
                    const target = $(mutation.target);
                    if (target.is('input[data-original-value]') && target.closest('#tbProdDet').length > 0) {
                        requiereActualizacion = true;
                    }
                } else if (mutation.type === 'childList') {
                    // Solo cambios en estructura que afecten campos de entrada
                    const addedNodes = Array.from(mutation.addedNodes);
                    const hasInputs = addedNodes.some(node =>
                        $(node).find('input[data-original-value]').length > 0
                    );
                    if (hasInputs && $(mutation.target).closest('#tbProdDet').length > 0) {
                        requiereActualizacion = true;
                    }
                }
                // IGNORAR cambios de clase 'selected' que solo son visuales
            });

            if (requiereActualizacion) {
                clearTimeout(window.actualizacionTimeout);
                window.actualizacionTimeout = setTimeout(function () {
                    console.log("Cambios relevantes detectados en campos, actualizando...");
                    actualizarCamposModificadosOptimizado();
                }, 300);
            }
        });
        //const observer = new MutationObserver(function (mutations) {
        //    let requiereActualizacion = false;

        //    mutations.forEach(function (mutation) {
        //        // Si se agregaron nodos o se cambió un atributo
        //        if (mutation.type === 'childList' ||
        //            (mutation.type === 'attributes' &&
        //                (mutation.attributeName === 'value' ||
        //                    mutation.attributeName === 'class'))) {

        //            // Solo si afecta a elementos dentro de tbProdDet
        //            if ($(mutation.target).closest('#tbProdDet').length > 0) {
        //                requiereActualizacion = true;
        //            }
        //        }
        //    });

        //    // Si hubo cambios relevantes, actualizar después de un breve retraso
        //    if (requiereActualizacion) {
        //        clearTimeout(window.actualizacionTimeout);
        //        window.actualizacionTimeout = setTimeout(function () {
        //            console.log("Cambios detectados en el DOM, actualizando campos modificados...");
        //            actualizarCamposModificadosOptimizado();
        //        }, 300);
        //    }
        //});

        // Iniciar observación cuando la tabla exista
        const iniciarObservador = function () {
            const tabla = document.getElementById('tbProdDet');
            if (tabla) {
                observer.observe(tabla, config);
                console.log("Observador DOM inicializado para #tbProdDet");
            }
        };

        // Verificar periódicamente hasta que la tabla exista
        const verificadorTabla = setInterval(function () {
            if ($('#tbProdDet').length > 0) {
                iniciarObservador();
                clearInterval(verificadorTabla);
            }
        }, 100);
    } else {
        console.warn("MutationObserver no soportado en este navegador");
    }
}


// Nueva función para asegurar que existan los estilos necesarios
function asegurarEstilosCamposModificados() {
    if (!$('style:contains(".campo-modificado")').length) {
        $('<style>')
            .prop('type', 'text/css')
            .html(`
                /* Estilo para campos modificados */
                .campo-modificado {
                    background-color: #d4f1f9 !important; /* Celeste pastel claro */
                    border-color: #a8e1f5 !important;
                }
                
                /* Indicador visual de cambio */
                .indicador-cambio {
                    position: absolute;
                    top: 0;
                    right: 0;
                    width: 0;
                    height: 0;
                    border-style: solid;
                    border-width: 0 8px 8px 0;
                    border-color: transparent #4bacc6 transparent transparent;
                }
                
                /* Contenedor para posicionar el indicador */
                .input-container {
                    position: relative;
                }
            `)
            .appendTo('head');

        console.log("Estilos para campos modificados añadidos");
    }
}


// Función para optimizar la visualización de la tabla
function optimizarVisualizacionTabla() {
    // Asegurarnos de que la tabla existe
    if ($("#tbProdDet").length === 0) {
        return;
    }

    // Ajustar columnas con texto para que no sean demasiado anchas
    $("#tbProdDet th:nth-child(2)").css('max-width', '180px'); // Descripción
    $("#tbProdDet td:nth-child(2)").css({
        'max-width': '180px',
        'white-space': 'nowrap',
        'overflow': 'hidden',
        'text-overflow': 'ellipsis'
    });

    // Asegurarnos que la tabla tenga scroll horizontal si es necesario
    $("#tbProdDet").closest('.table-responsive').css('overflow-x', 'auto');

    console.log("Tabla optimizada para mejor visualización");
}



// 2. Modificar la función buscarProductoLista para guardar el p_id del producto actual
function buscarProductoLista(productoId) {
    console.log(`Iniciando búsqueda de listas para producto ID: ${productoId}`);

    // Validar que se haya proporcionado un ID de producto
    if (!productoId) {
        console.warn("No se proporcionó un ID de producto válido para cargar listas");
        $("#divProdLista").html('<div class="alert alert-warning">No se pudo obtener información de listas de precios.</div>');
        productoActualEnLista = null; // Limpiar la referencia al no haber producto
        return;
    }

    // Actualizar título o información del producto seleccionado si existe
    const productoSeleccionado = $("#tbProdDet tbody tr[data-p-id='" + productoId + "']");
    if (productoSeleccionado.length > 0) {
        // Obtenemos el código o nombre del producto para mostrarlo en el panel
        const productoNombre = productoSeleccionado.find("td:eq(1)").text().trim();
        console.log(`Producto seleccionado: ${productoNombre} (ID: ${productoId})`);

        // Si existe un título del panel, actualizarlo
        if ($("#tituloListas").length > 0) {
            $("#tituloListas").text(`Listas de precios - ${productoNombre}`);
        }
    }

    // Mostrar indicador de carga
    $("#divProdLista").html('<div class="text-center p-3"><i class="bx bx-loader bx-spin font-size-24"></i><p class="mt-2">Cargando listas de precios...</p></div>');

    // Obtener parámetros para la consulta
    let datos = obtenerParametros(divs.ProductoListas);

    // Verificar que obtenerParametros haya devuelto datos válidos
    if (datos === false) {
        console.error("Error al obtener parámetros para la consulta de listas");
        $("#divProdLista").html('<div class="alert alert-danger">Error al preparar la consulta de listas de precios.</div>');
        productoActualEnLista = null; // Limpiar la referencia
        return;
    }

    // Añadir el ID del producto a los parámetros
    datos.id = productoId;
    // IMPORTANTE: Añadir un parámetro para indicar que debe verificar datos temporales
    datos.verificarTemp = true;

    console.log("Parámetros para la consulta de listas:", datos);

    // Realizar petición AJAX
    $.ajax({
        url: buscarProdListaUrl,
        type: "POST",
        data: datos,
        success: function (responseLista) {
            CerrarWaiting();

            // Verificar si la respuesta está vacía
            if (!responseLista || responseLista.trim() === '') {
                console.warn(`No se recibieron datos para las listas del producto ID: ${productoId}`);
                $("#divProdLista").html('<div class="alert alert-info">No hay listas de precios disponibles para este producto.</div>');
                productoActualEnLista = null; // Limpiar la referencia
                return;
            }

            // Verificar si hay elementos en la respuesta (podría ser HTML vacío)
            const tempElement = $('<div>').html(responseLista);
            if (tempElement.find("table tbody tr").length === 0) {
                console.log(`Respuesta recibida pero sin filas para el producto ID: ${productoId}`);
                $("#divProdLista").html('<div class="alert alert-info">No hay listas de precios disponibles para este producto.</div>');
                productoActualEnLista = null; // Limpiar la referencia
                return;
            }

            // Mostrar resultados de listas de precios
            $("#divProdLista").html(responseLista);

            // IMPORTANTE: Guardar el ID del producto cargado en la lista
            productoActualEnLista = productoId;
            // También almacenar el producto ID como atributo de datos en el contenedor para mayor seguridad
            $("#divProdLista").attr('data-producto-actual', productoId);

            console.log(`Listas de precios cargadas correctamente para producto ID: ${productoId}`);

            // Ejecutar código específico si hay ciertos elementos en la respuesta
            if ($("#tbProdLista").length > 0) {
                // Aplicar configuraciones a la tabla de listas
                optimizarVisualizacionTablaListas();
                configurarInputsListaPreciosOptimizado();

                console.log(`Se encontraron ${$("#tbProdLista tbody tr").length} listas de precios`);

                // Verificar si hay elementos temporales (marcados con alguna clase especial)
                const elementosTemporales = $("#tbProdLista .campo-modificado").length;
                if (elementosTemporales > 0) {
                    console.log(`Se encontraron ${elementosTemporales} registros temporales`);
                }

                // Configurar eventos para la tabla de listas
                $("#tbProdLista tbody tr").off("click").on("click", function (e) {
                    // Solo activar si el clic no fue en un input
                    if (!$(e.target).is('input')) {
                        $(this).toggleClass("selected");
                    }
                });
            } else {
                console.warn("No se encontró la tabla #tbProdLista en la respuesta");
            }
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            console.error("Error en la petición AJAX de listas:", error);
            console.error("Estado:", status);
            console.error("Respuesta:", xhr.responseText);

            // Mostrar mensaje de error
            $("#divProdLista").html(
                '<div class="alert alert-danger">' +
                '<h5>Error al cargar las listas de precios</h5>' +
                '<p>Se produjo un error al intentar cargar la información. Detalles: ' + status + '</p>' +
                '</div>'
            );

            // Limpiar la referencia al producto en caso de error
            productoActualEnLista = null;
        }
    });
}

function buscarProductoListaOptimizado(productoId) {
    console.log(`Iniciando búsqueda de listas para producto ID: ${productoId}`);

    // Validar que se haya proporcionado un ID de producto
    if (!productoId) {
        console.warn("No se proporcionó un ID de producto válido");
        $("#divProdLista").html('<div class="alert alert-warning">No se pudo obtener información de listas de precios.</div>');
        productoActualEnLista = null;
        return;
    }

    // Actualizar variable global inmediatamente
    productoActualEnLista = productoId;
    $("#divProdLista").attr('data-producto-actual', productoId);

    // Mostrar indicador de carga
    $("#divProdLista").html('<div class="text-center p-3"><i class="bx bx-loader bx-spin font-size-24"></i><p class="mt-2">Cargando listas de precios...</p></div>');

    // Obtener parámetros para la consulta
    let datos = obtenerParametros(divs.ProductoListas);

    // Verificar que obtenerParametros haya devuelto datos válidos
    if (datos === false) {
        console.error("Error al obtener parámetros para la consulta");
        $("#divProdLista").html('<div class="alert alert-danger">Error al preparar la consulta de listas de precios.</div>');
        return;
    }

    // Añadir el ID del producto a los parámetros
    datos.id = productoId;
    datos.verificarTemp = true;

    // Realizar petición AJAX
    $.ajax({
        url: buscarProdListaUrl,
        type: "POST",
        data: datos,
        success: function (responseLista) {
            CerrarWaiting();

            // Verificar si la respuesta está vacía
            if (!responseLista || responseLista.trim() === '') {
                console.warn(`No se recibieron datos para el producto ID: ${productoId}`);
                $("#divProdLista").html('<div class="alert alert-info">No hay listas de precios disponibles para este producto.</div>');
                return;
            }

            // Mostrar resultados de listas de precios
            $("#divProdLista").html(responseLista);
            console.log(`Listas de precios cargadas para producto ID: ${productoId}`);

            // Inicializar componentes de las listas de forma asíncrona
            setTimeout(() => {
                if ($("#tbProdLista").length > 0) {
                    // Aplicar optimizaciones
                    optimizarVisualizacionTablaListas();
                    configurarInputsListaPreciosOptimizado();

                    // Configurar eventos de tabla simplificados
                    $("#tbProdLista tbody tr").off("click").on("click", function (e) {
                        if (!$(e.target).is('input')) {
                            $(this).toggleClass("selected");
                        }
                    });
                }
            }, 10);
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            console.error("Error en la petición AJAX de listas:", error);
            $("#divProdLista").html(
                '<div class="alert alert-danger">' +
                '<h5>Error al cargar las listas de precios</h5>' +
                '<p>Se produjo un error al intentar cargar la información.</p>' +
                '</div>'
            );
        }
    });
}

function configurarInputsListaPreciosOptimizado() {
    console.log("Configurando inputs para grid de listas de precios (optimizado)...");

    // Remover máscaras previas una sola vez
    $('.input-tp_margen_lista, .input-tp_pvta_lista').inputmask('remove');

    // Establecer todos los campos como readonly de una vez
    $('.input-tp_margen_lista, .input-tp_pvta_lista')
        .prop('readonly', true)
        .addClass('campo-readonly');

    // Una sola configuración de InputMask para todos los campos
    Inputmask({
        alias: "numeric",
        groupSeparator: ",",
        radixPoint: ".",
        autoGroup: true,
        digits: 2,
        digitsOptional: false,
        rightAlign: true,
        prefix: '',
        placeholder: "0",
        clearMaskOnLostFocus: false,
        showMaskOnHover: false,
        showMaskOnFocus: false
    }).mask('.input-tp_margen_lista, .input-tp_pvta_lista');

    // Función para activar el siguiente campo en la tabla de listas
    function activarSiguienteCampoLista(campoActual) {
        const $campoActual = $(campoActual);
        const $fila = $campoActual.closest('tr');
        const camposLista = '.input-tp_margen_lista, .input-tp_pvta_lista';

        // Obtener todos los campos editables en la fila
        const $camposEnFila = $fila.find(camposLista);

        // Encontrar el índice del campo actual
        const indiceActual = $camposEnFila.index($campoActual);

        // Si hay un siguiente campo en la fila, activarlo
        if (indiceActual < $camposEnFila.length - 1) {
            const $siguienteCampo = $camposEnFila.eq(indiceActual + 1);

            // Desactivar el campo actual
            $campoActual.prop('readonly', true).addClass('campo-readonly');

            // Activar el siguiente campo
            $siguienteCampo.prop('readonly', false).removeClass('campo-readonly');

            // Enfocar y seleccionar el siguiente campo
            setTimeout(function () {
                $siguienteCampo[0].focus();
                $siguienteCampo[0].select();
            }, 0);

            return true;
        } else if ($fila.next('tr').length) {
            // Si estamos en el último campo de la fila, pasar a la primera celda de la siguiente fila
            const $siguienteFila = $fila.next('tr');
            const $primerCampo = $siguienteFila.find(camposLista).first();

            if ($primerCampo.length) {
                // Desactivar el campo actual
                $campoActual.prop('readonly', true).addClass('campo-readonly');

                // Activar el primer campo de la siguiente fila
                $primerCampo.prop('readonly', false).removeClass('campo-readonly');

                // Enfocar y seleccionar el siguiente campo
                setTimeout(function () {
                    $primerCampo[0].focus();
                    $primerCampo[0].select();
                }, 0);

                return true;
            }
        }

        // Si no hay siguiente campo, solo desactivar el actual
        $campoActual.prop('readonly', true).addClass('campo-readonly');
        return false;
    }

    // Usar delegación de eventos en lugar de asignar a cada elemento
    $(document)
        .off('click.habilitarCamposLista')
        .on('click.habilitarCamposLista', '.input-tp_margen_lista, .input-tp_pvta_lista', function (e) {
            e.stopPropagation();

            // Obtener referencia al elemento
            const $this = $(this);

            // Activar edición solo para este campo
            $this.prop('readonly', false).removeClass('campo-readonly');

            // Enfocar y seleccionar texto
            setTimeout(function () {
                $this[0].focus();
                $this[0].select();
            }, 0);
        })
        .off('keydown.enterCamposLista')
        .on('keydown.enterCamposLista', '.input-tp_margen_lista, .input-tp_pvta_lista', function (e) {
            if (e.key === 'Enter' || e.key === 'Tab') {
                e.preventDefault();

                const $this = $(this);
                const lpId = $this.data('lp-id');
                const pId = $this.data('p-id') || productoActualEnLista;
                const row = $this.closest('tr');

                // Formatear valor
                let value = $this.val().replace(/,/g, '');
                let numValue = parseFloat(value);

                if (!isNaN(numValue)) {
                    $this.val(numValue.toFixed(2));
                }

                // Determinar el tipo de campo y procesar
                if ($this.hasClass('input-tp_margen_lista')) {
                    // Actualizar margen con debounce
                    actualizarMargenListaDebounced(row, lpId, pId, numValue);
                } else if ($this.hasClass('input-tp_pvta_lista')) {
                    // *** CORREGIDO: Solo calcular margen, no llamar a actualizarPrecioVentaListaDebounced ***
                    if (!isNaN(numValue) && numValue > 0) {
                        console.log(`Precio de venta confirmado con Enter/Tab, calculando margen: ${numValue}`);
                        calcularPrecioVentaMargenLista(row, lpId, pId, numValue);
                    } else {
                        console.warn(`Valor de precio de venta inválido: ${numValue}, no se realizará cálculo`);
                    }
                }

                // Avanzar al siguiente campo
                activarSiguienteCampoLista(this);
            }
        })
        .off('blur.margenLista')
        .on('blur.margenLista', '.input-tp_margen_lista', function () {
            const $this = $(this);

            // Si ya está en readonly, no hacer nada
            if ($this.prop('readonly')) return;

            const lpId = $this.data('lp-id');
            const pId = $this.data('p-id') || productoActualEnLista;
            const row = $this.closest('tr');

            // Formatear valor
            let value = $this.val().replace(/,/g, '');
            let numValue = parseFloat(value);

            if (!isNaN(numValue)) {
                $this.val(numValue.toFixed(2));
            }

            // Volver a readonly
            $this.prop('readonly', true).addClass('campo-readonly');

            // Actualizar con debounce
            actualizarMargenListaDebounced(row, lpId, pId, numValue);
        })
        .off('blur.pvtaLista')
        .on('blur.pvtaLista', '.input-tp_pvta_lista', function () {
            const $this = $(this);

            // Si ya está en readonly, no hacer nada
            if ($this.prop('readonly')) return;

            const lpId = $this.data('lp-id');
            const pId = $this.data('p-id') || productoActualEnLista;
            const row = $this.closest('tr');

            // Formatear valor
            let value = $this.val().replace(/,/g, '');
            let numValue = parseFloat(value);

            if (!isNaN(numValue)) {
                $this.val(numValue.toFixed(2));
            }

            // Volver a readonly
            $this.prop('readonly', true).addClass('campo-readonly');

            // *** CORREGIDO: Solo calcular margen, no llamar a actualizarPrecioVentaListaDebounced ***
            if (!isNaN(numValue) && numValue > 0) {
                console.log(`Precio de venta editado en lista, calculando margen: ${numValue}`);
                calcularPrecioVentaMargenLista(row, lpId, pId, numValue);
            } else {
                console.warn(`Valor de precio de venta inválido: ${numValue}, no se realizará cálculo`);
            }
        })
        .off('click.desactivarCamposLista')
        .on('click.desactivarCamposLista', function (e) {
            if (!$(e.target).is('.input-tp_margen_lista, .input-tp_pvta_lista')) {
                $('.input-tp_margen_lista, .input-tp_pvta_lista').filter(function () {
                    return !$(this).prop('readonly');
                }).each(function () {
                    // Disparar blur manualmente
                    $(this).trigger('blur');
                });
            }
        });

    console.log("Configuración de inputs para grid de listas completada");
}


// Funciones con debounce para listas
const actualizarMargenListaDebounced = debounce(function (row, lpId, pId, nuevoMargen) {
    actualizarMargenLista(row, lpId, pId, nuevoMargen);
}, 300);

const actualizarPrecioVentaListaDebounced = debounce(function (row, lpId, pId, nuevoPrecioVenta) {
    actualizarPrecioVentaLista(row, lpId, pId, nuevoPrecioVenta);
}, 300);

function configurarEventosEdicionOptimizado() {
    // Eliminar eventos previos para evitar duplicación
    $(document).off('click.camposEditables')
        .off('keydown.camposEditables')
        .off('blur.camposSecuencia01')
        .off('blur.campoMargen')
        .off('blur.campoPVta')
        .off('blur.campoImpuesto')
        .off('click.desactivarCampos');

    // Definir selectores reutilizables
    const camposEditables = '.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni, .input-tp_margen, .input-tin_alicuota, .input-tp_pvta';
    const camposSecuencia01 = '.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni';

    // Función para encontrar y activar el siguiente campo editable
    function activarSiguienteCampo(campoActual) {
        const $campoActual = $(campoActual);
        const $fila = $campoActual.closest('tr');

        // Obtener todos los campos editables en la fila
        const $camposEnFila = $fila.find(camposEditables);

        // Encontrar el índice del campo actual
        const indiceActual = $camposEnFila.index($campoActual);

        // Si hay un siguiente campo en la fila, activarlo
        if (indiceActual < $camposEnFila.length - 1) {
            const $siguienteCampo = $camposEnFila.eq(indiceActual + 1);

            // Desactivar el campo actual
            $campoActual.prop('readonly', true).addClass('campo-readonly');

            // Activar el siguiente campo
            $siguienteCampo.prop('readonly', false).removeClass('campo-readonly');

            // Enfocar y seleccionar el siguiente campo
            setTimeout(function () {
                $siguienteCampo[0].focus();
                $siguienteCampo[0].select();
            }, 0);

            return true;
        } else if ($fila.next('tr').length) {
            // Si estamos en el último campo de la fila, pasar a la primera celda de la siguiente fila
            const $siguienteFila = $fila.next('tr');
            const $primerCampo = $siguienteFila.find(camposEditables).first();

            if ($primerCampo.length) {
                // Desactivar el campo actual
                $campoActual.prop('readonly', true).addClass('campo-readonly');

                // Activar el primer campo de la siguiente fila
                $primerCampo.prop('readonly', false).removeClass('campo-readonly');

                // Enfocar y seleccionar el siguiente campo
                setTimeout(function () {
                    $primerCampo[0].focus();
                    $primerCampo[0].select();
                }, 0);

                return true;
            }
        }

        // Si no hay siguiente campo, solo desactivar el actual
        $campoActual.prop('readonly', true).addClass('campo-readonly');
        return false;
    }

    // Evento click: habilita edición
    $(document).on('click.camposEditables', camposEditables, function (e) {
        e.stopPropagation();

        // Obtener el p_id del producto actual en detalle
        const $this = $(this);
        const $rowDetalle = $this.closest('tr');
        const pIdDetalle = $rowDetalle.data('p-id');

        // Verificar si estamos cambiando de producto
        const cambioDeProducto = pIdDetalle !== productoActualEnLista;

        // Si hay un cambio de producto, actualizamos la interfaz
        if (cambioDeProducto) {
            console.log(`Cambiando de producto ${productoActualEnLista} a ${pIdDetalle}`);

            // Actualizar variable global
            productoActualEnLista = pIdDetalle;
            $("#divProdLista").attr('data-producto-actual', pIdDetalle);

            // Destacar la fila del nuevo producto
            destacarFilaSeleccionada(pIdDetalle);

            // Cargar las listas del producto
            buscarProductoListaOptimizado(pIdDetalle);
        }

        // Habilitar el campo para edición
        $this.prop('readonly', false).removeClass('campo-readonly');
        setTimeout(function () {
            $this[0].focus();
            $this[0].select();
        }, 0);
    });

    // Evento keydown para detectar ENTER y TAB
    $(document).on('keydown.camposEditables', camposEditables, function (e) {
        if (e.key === 'Enter' || e.key === 'Tab') {
            e.preventDefault(); // Evitar comportamiento predeterminado

            // Aplicar cambios al campo actual
            const row = $(this).closest('tr');

            // Guardar el tipo de campo para calcular después
            const esSecuencia01 = $(this).is(camposSecuencia01);
            const esMargen = $(this).hasClass('input-tp_margen');
            const esPrecioVenta = $(this).hasClass('input-tp_pvta');

            // IMPORTANTE: Primero marcar este campo como modificado (o no)
            marcarCampoModificado(this);

            // NUEVO: Actualizar el estado de carga de la fila según las reglas
            actualizarEstadoCarga(row);

            // Avanzar al siguiente campo
            const seActivoSiguiente = activarSiguienteCampo(this);

            // Aplicar los cálculos según el tipo de campo
            if (esSecuencia01) {
                calcularCostoAPIDebounced(row);
            } else if (esMargen) {
                calcularPrecioVentaAPIDebounced(row);
            } else if (esPrecioVenta) {
                calcularPrecioVentaMargenAPIDebounced(row);
            }
        }
    });

    // Evento blur para campos de la secuencia01 (utilizando delegación)
    $(document).on('blur.camposSecuencia01', camposSecuencia01, function () {
        const $this = $(this);

        // Si ya está en readonly, ignorar
        if ($this.prop('readonly')) return;

        const row = $this.closest('tr');
        let value = $this.val().replace(/,/g, '');
        let numValue = parseFloat(value);

        // Formatear según el tipo de campo
        if (!isNaN(numValue)) {
            if ($this.hasClass('input-tp_plista')) {
                $this.val(numValue.toFixed(3));
            } else if ($this.hasClass('input-tp_dto1') ||
                $this.hasClass('input-tp_dto2') ||
                $this.hasClass('input-tp_dto3') ||
                $this.hasClass('input-tp_dto4') ||
                $this.hasClass('input-tp_dto_pa') ||
                $this.hasClass('input-tp_porc_flete')) {
                numValue = Math.min(numValue, 99.9);
                $this.val(numValue.toFixed(1));
            }
        }

        // Procesar bonificación si es el campo correspondiente
        if ($this.hasClass('input-tp_boni')) {
            let val = $this.val();
            let partes = val.split('/');
            if (partes.length === 2) {
                let num = parseInt(partes[0], 10);
                let den = parseInt(partes[1], 10);
                if (num > den && den > 0) {
                    alert('El denominador debe ser mayor al numerador. Se corregirá automáticamente.');
                    $this.val(den + '/' + num);
                }
            }
        }

        // Volver a readonly si no estamos en navegación con Tab/Enter
        $this.prop('readonly', true).addClass('campo-readonly');

        // IMPORTANTE: Marcar este campo como modificado (o no)
        marcarCampoModificado($this);

        // NUEVO: Actualizar el estado de carga
        actualizarEstadoCarga(row);

        // Utilizar debounce para evitar cálculos repetidos
        calcularCostoAPIDebounced(row);
    });

    // Evento blur para margen (secuencia02)
    $(document).on('blur.campoMargen', '.input-tp_margen', function () {
        const $this = $(this);

        // Si ya está en readonly, ignorar
        if ($this.prop('readonly')) return;

        const row = $this.closest('tr');
        let value = $this.val().replace(/,/g, '');
        let numValue = parseFloat(value);

        if (!isNaN(numValue)) {
            $this.val(numValue.toFixed(2));
        }

        $this.prop('readonly', true).addClass('campo-readonly');
        calcularPrecioVentaAPIDebounced(row);
    });

    // Evento blur para precio venta (secuencia03)
    $(document).on('blur.campoPVta', '.input-tp_pvta', function () {
        const $this = $(this);

        // Si ya está en readonly, ignorar
        if ($this.prop('readonly')) return;

        const row = $this.closest('tr');
        let value = $this.val().replace(/,/g, '');
        let numValue = parseFloat(value);

        if (!isNaN(numValue)) {
            $this.val(numValue.toFixed(2));
        }

        $this.prop('readonly', true).addClass('campo-readonly');
        calcularPrecioVentaMargenAPIDebounced(row);
    });

    // Evento blur para impuesto interno
    $(document).on('blur.campoImpuesto', '.input-tin_alicuota', function () {
        const $this = $(this);

        // Si ya está en readonly, ignorar
        if ($this.prop('readonly')) return;

        const row = $this.closest('tr');
        let value = $this.val().replace(/,/g, '');
        let numValue = parseFloat(value);

        if (!isNaN(numValue)) {
            $this.val(numValue.toFixed(2));
        }

        $this.prop('readonly', true).addClass('campo-readonly');
        recalcularRelacionPrecioVenta(row);
    });

    // Evento para desactivar campos al hacer clic fuera
    $(document).on('click.desactivarCampos', function (e) {
        if (!$(e.target).is(camposEditables)) {
            $(camposEditables).filter(function () {
                return !$(this).prop('readonly');
            }).each(function () {
                const event = new Event('blur', { bubbles: true });
                this.dispatchEvent(event);
            });
        }
    });
}


// Nueva función para resguardar los cambios del producto
function resguardarCambiosProducto(row) {
    // Recopilar todos los valores del producto
    const datos = {
        p_id: row.data('p-id'),
        tp_plista: parseFloat(row.find('.input-tp_plista').val().replace(/,/g, '')),
        tp_dto1: parseFloat(row.find('.input-tp_dto1').val().replace(/,/g, '')),
        tp_dto2: parseFloat(row.find('.input-tp_dto2').val().replace(/,/g, '')),
        tp_dto3: parseFloat(row.find('.input-tp_dto3').val().replace(/,/g, '')),
        tp_dto4: parseFloat(row.find('.input-tp_dto4').val().replace(/,/g, '')),
        tp_dto_pa: parseFloat(row.find('.input-tp_dto_pa').val().replace(/,/g, '')),
        tp_porc_flete: parseFloat(row.find('.input-tp_porc_flete').val().replace(/,/g, '')),
        tp_boni: row.find('.input-tp_boni').val(),
        tp_pcosto: parseFloat(row.find('.input-tp_pcosto').val().replace(/,/g, '')),
        tp_margen: parseFloat(row.find('.input-tp_margen').val().replace(/,/g, '')),
        tp_pneto: parseFloat(row.find('.input-tp_pneto').val().replace(/,/g, '')),
        tin_alicuota: parseFloat(row.find('.input-tin_alicuota').val().replace(/,/g, '')),
        tp_pvta: parseFloat(row.find('.input-tp_pvta').val().replace(/,/g, '')),
        tp_iva: parseFloat(row.find('input[name="tp_iva"]').val()),
        tp_in: parseFloat(row.find('input[name="tp_in"]').val()),
        iva_situacion: row.find('input[name="iva_situacion"]').val(),
        iva_alicuota: parseFloat(row.find('input[name="iva_alicuota"]').val()),
        in_alicuota: parseFloat(row.find('input[name="in_alicuota"]').val())
    };

    // Llamar al servidor para resguardar los cambios
    $.ajax({
        url: resguardarCambiosProductoUrl,
        type: 'POST',
        data: datos,
        dataType: 'json',
        success: function (response) {
            if (response.error) {
                console.error('Error al resguardar cambios:', response.msg);
            } else if (response.warn) {
                console.warn('Advertencia al resguardar cambios:', response.msg);
            } else {
                console.log('Cambios resguardados correctamente:', response.msg);
            }
        },
        error: function (xhr, status, error) {
            console.error('Error en la llamada AJAX al resguardar cambios:', error);
        }
    });
}

//// Añadir el botón para actualizar manualmente las listas
//function agregarBotonActualizarListas() {
//    // Verificar si el botón ya existe
//    if ($("#btnActualizarListas").length === 0) {
//        $('<button>')
//            .attr('id', 'btnActualizarListas')
//            .addClass('btn btn-sm btn-golden float-end mb-2')
//            .html('<i class="bx bx-refresh me-1"></i>Actualizar listas')
//            .on('click', function () {
//                const productoSeleccionado = $("#tbProdDet tbody tr.selected").data('p-id');
//                if (productoSeleccionado) {
//                    buscarProductoLista(productoSeleccionado);
//                } else {
//                    AbrirMensaje(
//                        "ATENCIÓN",
//                        "Debe seleccionar un producto para actualizar sus listas de precios.",
//                        function () { $("#msjModal").modal("hide"); },
//                        false,
//                        ["Entendido"],
//                        "warn!",
//                        null
//                    );
//                }
//            })
//            .insertBefore("#divProdLista");
//    }
//}


// Función para asegurar que todas las filas tengan el atributo data-carga
function asegurarAtributosCarga() {
    console.log("Asegurando atributos data-carga en todas las filas...");

    // Para cada fila de producto (no encabezados de familia)
    $('#tbProdDet tbody tr:not(.table-secondary)').each(function () {
        const row = $(this);

        // Si la fila no tiene el atributo data-carga explícito
        if (row.data('carga') === undefined) {
            // Verificar si hay algún campo modificado (reutilizar la lógica)
            actualizarEstadoCarga(row);
        } else {
            // Asegurar que data-carga está también como atributo HTML
            const cargaValue = row.data('carga');
            row.attr('data-carga', cargaValue);
        }
    });

    console.log("Atributos data-carga verificados");
}

function volverAFiltro() {
    $("#divDetalle").removeClass("show");
    $("#divFiltro").addClass("show");
}

// Opcionalmente, agregar una función helper para seleccionar contenido
function seleccionarContenido(element) {
    setTimeout(function () {
        element.focus();
        element.select();
    }, 0);
}

// Función para llamar a la API de cálculo de costo - Versión corregida
function calcularCostoAPI(row) {
    const productId = row.data('p-id');

    // Evitar cálculos redundantes
    if (row.data('calculating-cost') === true) {
        console.log('Ya hay un cálculo de costo en proceso para este producto, evitando duplicación');
        return;
    }

    // Marcar que estamos calculando
    row.data('calculating-cost', true);

    // Recopilar los valores de los campos del Segmento01
    // IMPORTANTE: Tratar correctamente el valor 0 en tp_plista
    const plistaValue = row.find('.input-tp_plista').val().replace(/,/g, '');

    const datos = {
        p_id: productId,
        // Asegurar que un string vacío se convierta a 0 y no a NaN
        tp_plista: plistaValue === '' ? 0 : parseFloat(plistaValue),
        tp_dto1: parseFloat(row.find('.input-tp_dto1').val().replace(/,/g, '')) || 0,
        tp_dto2: parseFloat(row.find('.input-tp_dto2').val().replace(/,/g, '')) || 0,
        tp_dto3: parseFloat(row.find('.input-tp_dto3').val().replace(/,/g, '')) || 0,
        tp_dto4: parseFloat(row.find('.input-tp_dto4').val().replace(/,/g, '')) || 0,
        tp_dto_pa: parseFloat(row.find('.input-tp_dto_pa').val().replace(/,/g, '')) || 0,
        tp_porc_flete: parseFloat(row.find('.input-tp_porc_flete').val().replace(/,/g, '')) || 0,
        tp_boni: row.find('.input-tp_boni').val()
    };

    // Mostrar indicador de carga en el campo de costo
    const campoCoste = row.find('.input-tp_pcosto');
    const valorOriginal = campoCoste.val();
    campoCoste.val('Calculando...').addClass('calculating');

    // Llamar a la API usando PostGen (sin mostrar indicador global de carga)
    PostGen(datos, calcularCostoUrl, function (obj) {
        // Desmarcar el estado de cálculo
        row.data('calculating-cost', false);

        if (obj.error === true) {
            // Manejo del error
            campoCoste.val(valorOriginal).removeClass('calculating');
            AbrirMensaje("¡¡Algo no fué bien!!", obj.msg, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        } else if (obj.warn === true) {
            // Manejo de advertencia
            campoCoste.val(valorOriginal).removeClass('calculating');
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                if (obj.auth === true) {
                    window.location.href = login;
                } else {
                    $("#msjModal").modal("hide");
                }
                return true;
            }, false, ["Aceptar"], "warn!", null);
        } else {
            // Éxito: actualizar el valor del costo con el resultado de la API
            campoCoste.val(parseFloat(obj.costo).toFixed(3)).removeClass('calculating');
            marcarCampoModificado(campoCoste);

            // Actualizar el estado de carga
            actualizarEstadoCarga(row);

            console.log('Costo actualizado para producto ID:', productId, 'Nuevo valor:', obj.costo);

            // Continuar automáticamente con el cálculo del precio de venta
            calcularPrecioVentaAPI(row);
        }
    }, function (error) {
        // Función de error
        row.data('calculating-cost', false);
        console.error('Error en la llamada al servidor:', error);
        campoCoste.val(valorOriginal).removeClass('calculating');
        AbrirMensaje("ERROR", "Se produjo un error al comunicarse con el servidor. Por favor, inténtelo nuevamente.", function () {
            $("#msjModal").modal("hide");
        }, false, ["Aceptar"], "error!", null);
    });
}



// Función para calcular precio de venta mediante API
// Función para calcular precio de venta mediante API - Versión corregida
function calcularPrecioVentaAPI(row) {
    const productId = row.data('p-id');

    // Evitar cálculos redundantes si ya estamos calculando
    if (row.data('calculating-price') === true) {
        console.log('Ya hay un cálculo de precio en proceso para este producto, evitando duplicación');
        return;
    }

    // Marcar que estamos calculando el precio
    row.data('calculating-price', true);

    // Actualizar la variable global productoActualEnLista
    productoActualEnLista = productId;
    $("#divProdLista").attr('data-producto-actual', productId);

    // IMPORTANTE: Tratar adecuadamente los valores 0
    const pcosto = row.find('.input-tp_pcosto').val().replace(/,/g, '');
    const margen = row.find('.input-tp_margen').val().replace(/,/g, '');

    // Recopilar los valores de los campos de la Secuencia02
    const datos = {
        p_id: productId,
        tp_pcosto: pcosto === '' ? 0 : parseFloat(pcosto),
        lp_prevision_tot: parseFloat(row.find('input[name="lp_prevision_tot"]').val()) || 0,
        lp_prevision_pin: parseFloat(row.find('input[name="lp_prevision_pin"]').val()) || 0,
        tp_margen: margen === '' ? 0 : parseFloat(margen),
        iva_situacion: row.find('input[name="iva_situacion"]').val() || 'E',
        iva_alicuota: parseFloat(row.find('input[name="iva_alicuota"]').val()) || 0,
        in_alicuota: parseFloat(row.find('input[name="in_alicuota"]').val()) || 0
    };

    // Mostrar indicador de carga en el campo de precio neto
    const campoPrecioNeto = row.find('.input-tp_pneto');
    const valorOriginalPNeto = campoPrecioNeto.val();
    campoPrecioNeto.val('Calculando...').addClass('calculating');

    // Llamar a la API usando Ajax
    AbrirWaiting("Calculando precio de venta...");
    $.ajax({
        url: calcularPrecioVentaBaseUrl,
        type: 'POST',
        data: datos,
        dataType: 'json',
        success: function (response) {
            CerrarWaiting();
            // Desmarcar el estado de cálculo
            row.data('calculating-price', false);

            if (response.error === true) {
                // Manejo del error
                campoPrecioNeto.val(valorOriginalPNeto).removeClass('calculating');
                AbrirMensaje("¡¡Algo no fué bien!!", response.msg, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "error!", null);
            } else if (response.warn === true) {
                // Manejo de advertencia
                campoPrecioNeto.val(valorOriginalPNeto).removeClass('calculating');
                AbrirMensaje("ATENCIÓN", response.msg, function () {
                    if (response.auth === true) {
                        window.location.href = login;
                    } else {
                        $("#msjModal").modal("hide");
                    }
                    return true;
                }, false, ["Aceptar"], "warn!", null);
            } else {
                // Éxito: actualizar los valores con los resultados de la API

                // 1. Actualizar tp_pneto (con 3 decimales)
                const pneto = parseFloat(response.pvta.p_pneto).toFixed(3);
                campoPrecioNeto.val(pneto).removeClass('calculating');
                marcarCampoModificado(campoPrecioNeto);

                // 2. Actualizar tp_pvta (con 2 decimales)
                const campoPVenta = row.find('.input-tp_pvta');
                const pvta = parseFloat(response.pvta.p_pvta).toFixed(2);
                campoPVenta.val(pvta);
                marcarCampoModificado(campoPVenta);

                // 3. Actualizar campos ocultos tp_iva y tp_in
                row.find('input[name="tp_iva"]').val(response.pvta.p_iva);
                row.find('input[name="tp_in"]').val(response.pvta.p_in);

                // 4. Actualizar %Re (tp_pvta / p_pvta)
                const precioVentaOriginal = parseFloat(row.find('.input-tp_pvta').data('original-value') || '0');
                const precioVentaNuevo = parseFloat(pvta);

                // Manejar el cálculo del ratio incluso cuando el precio original es 0
                let ratio = "0.0";
                if (precioVentaOriginal > 0) {
                    ratio = (precioVentaNuevo / precioVentaOriginal).toFixed(1);
                } else if (precioVentaNuevo > 0) {
                    // Si el precio original es 0 pero el nuevo no, establecer un valor alto
                    ratio = "999.9";
                }

                // Encontrar la celda %Re usando la clase .tdRe
                const celdaRatio = row.find('.tdRe');

                // Si se encuentra la celda con la clase .tdRe
                if (celdaRatio.length > 0) {
                    // Actualizar el valor con 2 decimales (cambiado de toFixed(1) a toFixed(2))
                    ratio = precioVentaOriginal > 0 ? (precioVentaNuevo / precioVentaOriginal).toFixed(2) :
                        (precioVentaNuevo > 0 ? "999.99" : "0.00");

                    celdaRatio.text(ratio);

                    // Aplicar color según valor (azul si > 1, rojo si < 1)
                    const ratioNum = parseFloat(ratio);
                    if (ratioNum > 1) {
                        celdaRatio.css({
                            'color': 'blue',
                            'font-weight': 'bold' // Texto en negrita para valores > 1
                        });
                    } else if (ratioNum < 1) {
                        celdaRatio.css({
                            'color': 'red',
                            'font-weight': 'bold' // Texto en negrita para valores < 1
                        });
                    } else {
                        celdaRatio.css({
                            'color': '',
                            'font-weight': 'normal' // Peso normal para valor = 1
                        });
                    }
                }

                // 5. Resguardar los cambios
                resguardarCambiosProducto(row);

                // Actualizar precios en grid de listas si hay filas
                const hayFilasLista = $('#tbProdLista tbody tr').length > 0;
                if (hayFilasLista) {
                    console.log('Actualizando precios en grid de listas para producto ID:', productId);
                    actualizarPreciosListasOptimizado(datos, pvta);
                }
            }
        },
        error: function (xhr, status, error) {
            // Función de error
            CerrarWaiting();
            row.data('calculating-price', false);

            // Asegurar que el indicador de carga de listas también se elimine en caso de error
            $("#listasLoadingIndicator").fadeOut(300, function () {
                $(this).remove();
            });

            console.error('Error en la llamada al servidor:', error);
            campoPrecioNeto.val(valorOriginalPNeto).removeClass('calculating');
            AbrirMensaje("ERROR", "Se produjo un error al comunicarse con el servidor. Por favor, inténtelo nuevamente.", function () {
                $("#msjModal").modal("hide");
            }, false, ["Aceptar"], "error!", null);
        }
    });
}

function actualizarPreciosListasOptimizado(datosProducto, pvta) {
    // Obtener las filas de la tabla de listas
    const filasLista = $('#tbProdLista tbody tr');

    // Si no hay filas, no hacer nada
    if (filasLista.length === 0) {
        console.log('No hay filas en la tabla de listas para actualizar');
        return;
    }

    console.log("Iniciando actualización de precios en listas...");

    // IMPORTANTE: Primero eliminar cualquier indicador previo que pueda haber quedado
    $("#listasLoadingIndicator").remove();

    // Verificar datos necesarios
    if (!datosProducto.tp_pcosto || isNaN(datosProducto.tp_pcosto)) {
        console.error("Falta el costo del producto para actualizar listas");
        return;
    }

    // Obtener el precio neto base
    let precioNetoBase;
    const productoFila = $(`#tbProdDet tbody tr[data-p-id='${productoActualEnLista}']`);
    if (productoFila.length > 0) {
        const pNetoValue = productoFila.find('.input-tp_pneto').val();
        if (pNetoValue) {
            precioNetoBase = parseFloat(pNetoValue.replace(/,/g, ''));
        }
    }

    // Si no tenemos precio neto base, intentar calcularlo
    if (!precioNetoBase || isNaN(precioNetoBase)) {
        if (datosProducto.tp_pcosto && datosProducto.tp_margen) {
            precioNetoBase = datosProducto.tp_pcosto * (1 + datosProducto.tp_margen / 100);
        } else {
            console.warn("No se pudo determinar p_pneto_base");
            return;
        }
    }

    // Preparar datos para actualización masiva
    const listasData = [];
    filasLista.each(function () {
        const listaRow = $(this);
        const lp_id = listaRow.data('lp-id');
        const p_id = listaRow.find('.input-tp_margen_lista').data('p-id') || productoActualEnLista;
        const lp_porc_mg = parseFloat(listaRow.find('input[name="lp_porc_mg"]').val());

        if (!isNaN(lp_porc_mg) && lp_id && p_id) {
            listasData.push({
                row: listaRow,
                lp_id: lp_id,
                p_id: p_id,
                lp_porc_mg: lp_porc_mg
            });
        }
    });

    if (listasData.length === 0) {
        console.log("No hay listas válidas para actualizar");
        return;
    }

    // Mostrar indicador de carga específico para las listas
    $("#divProdLista").append(
        '<div id="listasLoadingIndicator" class="position-absolute bg-white p-2 rounded shadow-sm" ' +
        'style="top:50%; left:50%; transform:translate(-50%,-50%); z-index:1000;">' +
        '<div class="d-flex align-items-center">' +
        '<i class="bx bx-loader bx-spin me-2"></i>' +
        '<span>Actualizando listas...</span>' +
        '</div></div>'
    );

    // Establecer un timeout de seguridad para eliminar el indicador después de 30 segundos
    // Esto asegura que, incluso si algo falla, el indicador desaparecerá
    const seguridadTimeout = setTimeout(() => {
        $("#listasLoadingIndicator").fadeOut(300, function () {
            $(this).remove();
        });
        console.warn("Timeout de seguridad: eliminando indicador de actualización de listas");
    }, 30000);

    // Procesamiento de listas por lotes con el timeout como parámetro
    procesarLoteListas(listasData, 0, 5, listasData.length, datosProducto, precioNetoBase, pvta, seguridadTimeout);
}

function procesarLoteListas(listas, inicio, tamanoLote, totalListas, datosProducto, precioNetoBase, pvta, seguridadTimeout) {
    const fin = Math.min(inicio + tamanoLote, totalListas);
    const loteActual = listas.slice(inicio, fin);
    const promesas = [];

    // Procesar este lote en paralelo
    loteActual.forEach(lista => {
        // Crear promesa para cada actualización de lista
        promesas.push(new Promise((resolve, reject) => {
            const datosLista = {
                p_id: lista.p_id,
                lp_id: lista.lp_id,
                tp_pcosto: datosProducto.tp_pcosto,
                p_pneto_base: precioNetoBase,
                lp_porc_mg: lista.lp_porc_mg,
                iva_situacion: datosProducto.iva_situacion,
                iva_alicuota: datosProducto.iva_alicuota,
                in_alicuota: datosProducto.in_alicuota
            };

            $.ajax({
                url: calcularPrecioVentaLinkUrl,
                type: 'POST',
                data: datosLista,
                dataType: 'json',
                success: function (respLista) {
                    if (respLista && respLista.pvta) {
                        // Actualizar los campos de la lista
                        const listaRow = lista.row;

                        // Actualizar precio neto
                        listaRow.find('input[name="tp_pneto"]').val(parseFloat(respLista.pvta.p_pneto).toFixed(3));

                        // Actualizar precio venta lista
                        const campoPVtaLista = listaRow.find('.input-tp_pvta_lista');
                        const nuevoPVta = parseFloat(respLista.pvta.p_pvta).toFixed(2);
                        const valorAnterior = parseFloat(campoPVtaLista.val().replace(/,/g, ''));

                        // Actualizar el valor solo si es diferente
                        if (Math.abs(valorAnterior - nuevoPVta) > 0.01) {
                            campoPVtaLista.val(nuevoPVta);
                            actualizarPrecioVentaLista(listaRow, lista.lp_id, lista.p_id, parseFloat(nuevoPVta));
                        }

                        // Actualizar campos ocultos
                        listaRow.find('input[name="tp_iva"]').val(respLista.pvta.p_iva);
                        listaRow.find('input[name="tp_in"]').val(respLista.pvta.p_in);

                        // Calcular y actualizar ratio si corresponde
                        if (pvta > 0) {
                            const ratio = (parseFloat(nuevoPVta) / parseFloat(pvta)).toFixed(2);
                            listaRow.find('td:eq(4)').text(ratio);
                        }
                    }
                    resolve();
                },
                error: function (error) {
                    console.error(`Error al actualizar lista ${lista.lp_id}:`, error);
                    resolve(); // Resolvemos igual para continuar con las demás
                }
            });
        }));
    });

    // Esperar a que se completen todas las actualizaciones del lote
    Promise.all(promesas)
        .then(() => {
            // Actualizar progreso
            const porcentaje = Math.round((fin / totalListas) * 100);
            const indicador = $("#listasLoadingIndicator");
            if (indicador.length > 0) {
                indicador.find("span").text(`Actualizando listas... ${porcentaje}%`);
            }

            // Si quedan listas por procesar, programar el siguiente lote
            if (fin < totalListas) {
                setTimeout(() => {
                    procesarLoteListas(listas, fin, tamanoLote, totalListas, datosProducto, precioNetoBase, pvta, seguridadTimeout);
                }, 10);
            } else {
                // Todas las listas procesadas, eliminar indicador y limpiar timeout
                if (seguridadTimeout) clearTimeout(seguridadTimeout);
                $("#listasLoadingIndicator").fadeOut(300, function () {
                    $(this).remove();
                });
                console.log('Todas las listas actualizadas correctamente');
            }
        })
        .catch(error => {
            // En caso de error en las promesas, asegurarnos de limpiar
            console.error("Error al procesar listas:", error);
            if (seguridadTimeout) clearTimeout(seguridadTimeout);
            $("#listasLoadingIndicator").fadeOut(300, function () {
                $(this).remove();
            });
        });
}


// Función para calcular margen a partir del precio de venta (Secuencia 3)
function calcularPrecioVentaMargenAPI(row) {
    const productId = row.data('p-id');

    // Evitar cálculos redundantes si ya estamos calculando
    if (row.data('calculating-margin') === true) {
        console.log('Ya hay un cálculo de margen en proceso para este producto, evitando duplicación');
        return;
    }

    // Marcar que estamos calculando el margen
    row.data('calculating-margin', true);

    // Recopilar los valores de los campos de la Secuencia 3
    const datos = {
        p_id: productId,
        tp_pcosto: parseFloat(row.find('.input-tp_pcosto').val().replace(/,/g, '')),
        lp_prevision_tot: parseFloat(row.find('input[name="lp_prevision_tot"]').val()),
        lp_prevision_pin: parseFloat(row.find('input[name="lp_prevision_pin"]').val()),
        tp_pvta: parseFloat(row.find('.input-tp_pvta').val().replace(/,/g, '')),
        iva_situacion: row.find('input[name="iva_situacion"]').val(),
        iva_alicuota: parseFloat(row.find('input[name="iva_alicuota"]').val()),
        in_alicuota: parseFloat(row.find('input[name="in_alicuota"]').val())
    };

    // Mostrar indicador de carga en el campo de precio neto (no en el margen)
    const campoPrecioNeto = row.find('.input-tp_pneto');
    const valorOriginalPNeto = campoPrecioNeto.val();
    campoPrecioNeto.val('Calculando...').addClass('calculating');

    // Guardar un log detallado de los datos que estamos enviando para depuración
    console.log('Enviando datos para cálculo de margen:', JSON.stringify(datos));

    // Llamar a la API usando Ajax
    AbrirWaiting("Calculando margen...");
    $.ajax({
        url: calcularPrecioVentaMargenUrl,
        type: 'POST',
        data: datos,
        dataType: 'json',
        success: function (response) {
            CerrarWaiting();
            // Desmarcar el estado de cálculo
            row.data('calculating-margin', false);

            // Log detallado de la respuesta para depuración
            console.log('Respuesta del cálculo de margen:', JSON.stringify(response));

            if (response.error === true) {
                // Manejo del error
                campoPrecioNeto.val(valorOriginalPNeto).removeClass('calculating');
                AbrirMensaje("¡¡Algo no fué bien!!", response.msg, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "error!", null);
            } else if (response.warn === true) {
                // Manejo de advertencia
                campoPrecioNeto.val(valorOriginalPNeto).removeClass('calculating');
                AbrirMensaje("ATENCIÓN", response.msg, function () {
                    if (response.auth === true) {
                        window.location.href = login;
                    } else {
                        $("#msjModal").modal("hide");
                    }
                    return true;
                }, false, ["Aceptar"], "warn!", null);
            } else {
                try {
                    // Verificar que los datos de la respuesta sean válidos
                    if (!response.pvta || typeof response.pvta.p_pneto === 'undefined') {
                        throw new Error('La respuesta no contiene el precio neto (p_pneto)');
                    }

                    // 1. Asegurar que p_pneto es un número válido y formatearlo con 3 decimales
                    const pnetoValue = parseFloat(response.pvta.p_pneto);
                    if (isNaN(pnetoValue)) {
                        throw new Error('El valor de p_pneto no es un número válido: ' + response.pvta.p_pneto);
                    }

                    // Formatear p_pneto con 3 decimales y asignarlo al campo
                    const pneto = pnetoValue.toFixed(3);
                    console.log('Asignando precio neto:', pneto, 'a partir de', response.pvta.p_pneto);

                    // Actualizar el campo de precio neto con el nuevo valor
                    campoPrecioNeto.val(pneto).removeClass('calculating');

                    // Asegurar que el valor se ha asignado correctamente
                    console.log('Precio neto después de asignar:', campoPrecioNeto.val());

                    // Marcar el campo como modificado
                    marcarCampoModificado(campoPrecioNeto);

                    // 2. Actualizar solo el campo oculto p_margen
                    row.find('input[name="p_margen"]').val(response.pvta.p_margen);

                    // 3. Actualizar campos ocultos tp_iva y tp_in
                    row.find('input[name="tp_iva"]').val(response.pvta.p_iva);
                    row.find('input[name="tp_in"]').val(response.pvta.p_in);

                    // 4. Resguardar los cambios
                    resguardarCambiosProducto(row);

                    console.log('Margen calculado para producto ID:', productId);
                    console.log('  Precio neto (actualizado):', campoPrecioNeto.val());
                    console.log('  Margen calculado (campo oculto):', response.pvta.p_margen);
                    console.log('  IVA:', response.pvta.p_iva);
                    console.log('  Impuesto interno:', response.pvta.p_in);
                } catch (e) {
                    // Capturar cualquier error durante el procesamiento de la respuesta
                    console.error('Error al procesar la respuesta del cálculo de margen:', e);
                    campoPrecioNeto.val(valorOriginalPNeto).removeClass('calculating');
                    AbrirMensaje("ERROR", "Se produjo un error al procesar la respuesta: " + e.message, function () {
                        $("#msjModal").modal("hide");
                    }, false, ["Aceptar"], "error!", null);
                }
            }
        },
        error: function (xhr, status, error) {
            // Función de error
            CerrarWaiting();
            // Desmarcar el estado de cálculo
            row.data('calculating-margin', false);

            console.error('Error en la llamada al servidor:', error);
            campoPrecioNeto.val(valorOriginalPNeto).removeClass('calculating');
            AbrirMensaje("ERROR", "Se produjo un error al comunicarse con el servidor. Por favor, inténtelo nuevamente.", function () {
                $("#msjModal").modal("hide");
            }, false, ["Aceptar"], "error!", null);
        }
    });
}

// Agregamos este comentario en las funciones actualizadas:
// Nota: Usamos addClass/removeClass explícitos en lugar de toggleClass
// para evitar comportamientos inconsistentes cuando se llama repetidamente
// a esta función en diferentes contextos.
function actualizarCamposModificadosOptimizado() {
    console.log("Actualizando campos modificados (optimizado)...");

    // Asegurar atributos de carga primero
    asegurarAtributosCarga();

    // Recolectar todos los cambios primero, para aplicarlos en batch
    const cambios = [];

    // Iterar por cada fila de la tabla
    $('#tbProdDet tbody tr:not(.table-secondary)').each(function () {
        const row = $(this);
        const modoTemporal = row.data('carga') === 1;

        if (modoTemporal) {
            // Verificar campos con posibles cambios
            row.find('input[data-original-value]').each(function () {
                const $input = $(this);
                const valorOriginal = $input.data('original-value');
                let valorActual = $input.val().replace(/,/g, '');

                // Determinar si está modificado
                let esModificado = false;

                // Para el campo de bonificación (caso especial)
                if ($input.hasClass('input-tp_boni')) {
                    const originalTrim = (valorOriginal || '').toString().trim();
                    const actualTrim = (valorActual || '').toString().trim();

                    esModificado = !(originalTrim === actualTrim ||
                        (originalTrim === "0" && actualTrim === "") ||
                        (originalTrim === "" && actualTrim === "0"));
                } else {
                    // Para campos numéricos
                    try {
                        const numOriginal = parseFloat(valorOriginal);
                        const numActual = parseFloat(valorActual);

                        if (!isNaN(numOriginal) && !isNaN(numActual)) {
                            // Determinar tolerancia según el tipo de campo
                            let tolerancia = 0.009; // Base para campos con 2 decimales

                            if ($input.hasClass('input-tp_dto1') ||
                                $input.hasClass('input-tp_dto2') ||
                                $input.hasClass('input-tp_dto3') ||
                                $input.hasClass('input-tp_dto4') ||
                                $input.hasClass('input-tp_dto_pa') ||
                                $input.hasClass('input-tp_porc_flete')) {
                                tolerancia = 0.09; // Para campos con 1 decimal
                            } else if ($input.hasClass('input-tp_plista') ||
                                $input.hasClass('input-tp_pcosto') ||
                                $input.hasClass('input-tp_pneto')) {
                                tolerancia = 0.0009; // Para campos con 3 decimales
                            }

                            esModificado = Math.abs(numOriginal - numActual) > tolerancia;
                        }
                    } catch (e) {
                        console.error("Error al comparar valores:", e);
                    }
                }

                // Guardar información para aplicación en batch
                cambios.push({
                    elemento: $input,
                    modificado: esModificado
                });
            });
        } else {
            // Si no estamos en modo temporal, quitar todas las marcas
            row.find('.campo-modificado').each(function () {
                cambios.push({
                    elemento: $(this),
                    modificado: false
                });
            });
        }
    });

    // Aplicar todos los cambios en batch (reduce los reflows)
    requestAnimationFrame(() => {
        cambios.forEach(function (cambio) {
            if (cambio.modificado) {
                cambio.elemento.addClass('campo-modificado');
            } else {
                cambio.elemento.removeClass('campo-modificado');
            }

            // Manejar el indicador visual
            const container = cambio.elemento.closest('.input-container');
            if (cambio.modificado) {
                if (container.find('.indicador-cambio').length === 0) {
                    container.append('<div class="indicador-cambio"></div>');
                }
            } else {
                container.find('.indicador-cambio').remove();
            }
        });

        console.log(`Campos marcados como modificados: ${$('.campo-modificado').length}`);
    });
}

// Agregamos este comentario en las funciones actualizadas:
// Nota: Usamos addClass/removeClass explícitos en lugar de toggleClass
// para evitar comportamientos inconsistentes cuando se llama repetidamente
// a esta función en diferentes contextos.

// Función para marcar un campo como modificado - Versión corregida para manejar el valor 0
function marcarCampoModificado(input) {
    // Usar el parámetro input en lugar de this
    const $input = $(input);

    // Validar que el input existe
    if (!$input.length) {
        console.warn('marcarCampoModificado: Input no válido', input);
        return false;
    }

    const valorOriginal = $input.data('original-value');

    // Obtener valor actual con manejo de errores
    let valorActual = '';
    try {
        valorActual = $input.val() ? $input.val().replace(/,/g, '') : '';
    } catch (e) {
        console.error('Error al obtener valor del campo:', e);
        return false;
    }

    // Si no hay valor original definido, no podemos comparar
    if (valorOriginal === undefined) {
        return false;
    }

    // Determinar si el campo está modificado
    let esModificado = false;

    // Para el campo de bonificación (caso especial)
    if ($input.hasClass('input-tp_boni')) {
        const originalTrim = (valorOriginal || '').toString().trim();
        const actualTrim = (valorActual || '').toString().trim();

        // Casos especiales: "0" y "" se consideran iguales
        if ((originalTrim === "0" && actualTrim === "") ||
            (originalTrim === "" && actualTrim === "0")) {
            esModificado = false;
        } else {
            esModificado = originalTrim !== actualTrim;
        }
    } else {
        // Para campos numéricos - manejar correctamente el caso del valor 0
        try {
            // Convertir valores a números, manejando cadenas vacías como 0
            let numOriginal = valorOriginal === '' || valorOriginal === null ? 0 : parseFloat(valorOriginal);
            let numActual = valorActual === '' ? 0 : parseFloat(valorActual);

            // Si ambos valores son realmente cero (o equivalentes a cero), no están modificados
            if ((numOriginal === 0 || isNaN(numOriginal)) &&
                (numActual === 0 || isNaN(numActual))) {
                esModificado = false;
            } else if (!isNaN(numOriginal) && !isNaN(numActual)) {
                // Ambos son números válidos, usar tolerancias específicas según el campo
                let tolerancia = 0.009; // Base para campos con 2 decimales

                if ($input.hasClass('input-tp_dto1') ||
                    $input.hasClass('input-tp_dto2') ||
                    $input.hasClass('input-tp_dto3') ||
                    $input.hasClass('input-tp_dto4') ||
                    $input.hasClass('input-tp_dto_pa') ||
                    $input.hasClass('input-tp_porc_flete')) {
                    tolerancia = 0.09; // Para campos con 1 decimal
                } else if ($input.hasClass('input-tp_plista') ||
                    $input.hasClass('input-tp_pcosto') ||
                    $input.hasClass('input-tp_pneto')) {
                    tolerancia = 0.0009; // Para campos con 3 decimales
                }

                // Si la diferencia supera la tolerancia, está modificado
                esModificado = Math.abs(numOriginal - numActual) > tolerancia;
            } else if (isNaN(numOriginal) !== isNaN(numActual)) {
                // Si uno es NaN y el otro no, están diferentes
                esModificado = true;
            }
        } catch (e) {
            console.error("Error al comparar valores:", e);
            esModificado = false; // En caso de error, no marcar como modificado
        }
    }

    // Aplicar o quitar la clase según corresponda
    if (esModificado) {
        $input.addClass('campo-modificado');
    } else {
        $input.removeClass('campo-modificado');
    }

    // Manejar el indicador visual
    const container = $input.closest('.input-container');
    if (esModificado) {
        if (container.find('.indicador-cambio').length === 0) {
            container.append('<div class="indicador-cambio"></div>');
        }
    } else {
        container.find('.indicador-cambio').remove();
    }

    return esModificado;
}



// Función para marcar un campo como modificado (similar a la existente en productocargaprecio.js)
// Función optimizada para marcar un campo modificado (unificada para ambos grids)
function marcarCampoModificadoLista(input) {
    const $input = $(input);  // Usar el parámetro input

    // Validar que el input existe
    if (!$input.length) {
        console.warn('marcarCampoModificadoLista: Input no válido', input);
        return false;
    }

    const valorOriginal = $input.data('original-value');

    // Obtener valor actual con manejo de errores
    let valorActual = '';
    try {
        valorActual = $input.val() ? $input.val().replace(/,/g, '') : '';
    } catch (e) {
        console.error('Error al obtener valor del campo de lista:', e);
        return false;
    }

    // Para campos numéricos, convertimos a números y comparamos con tolerancia
    const numOriginal = parseFloat(valorOriginal);
    const numActual = parseFloat(valorActual);

    // Consideramos diferente si hay una diferencia mayor a 0.01 (para valores con 2 decimales)
    let esModificado = Math.abs(numOriginal - numActual) > 0.01;

    // Aplicar o quitar la clase según corresponda
    if (esModificado) {
        $input.addClass('campo-modificado');

        // Si no existe el indicador de cambio, agregarlo
        if ($input.parent().find('.indicador-cambio').length === 0) {
            $input.parent().append('<div class="indicador-cambio"></div>');
        }
    } else {
        $input.removeClass('campo-modificado');
        $input.parent().find('.indicador-cambio').remove();
    }

    return esModificado; // Devolver si se modificó para uso posterior
}

/**
 * Actualiza el atributo data-carga de una fila según las reglas:
 * - Si hay cambios y carga=0, establecer carga=1
 * - Si no hay cambios y carga=1, establecer carga=0
 * - En otros casos, mantener valor actual
 * @param {jQuery} row - La fila (tr) a verificar
 * @returns {boolean} - Indica si la fila tiene algún campo modificado
 */
function actualizarEstadoCarga(row) {
    // Obtener el estado actual de carga
    const estadoCargaActual = row.data('carga') === 1;

    // Verificación rápida: si ya hay campos con la clase 'campo-modificado', hay cambios
    const camposModificados = row.find('.campo-modificado').length;

    if (camposModificados > 0) {
        // Hay campos modificados, asegurar que carga=1
        if (!estadoCargaActual) {
            row.data('carga', 1);
            row.attr('data-carga', '1');
            console.log(`Fila ${row.data('p-id')}: Cambiando data-carga a 1 (detectados ${camposModificados} campos modificados)`);
        }
        return true; // Hay campos modificados
    } else {
        // No hay campos con la clase, verificar si realmente hay diferencias
        // (esta es una verificación más profunda y costosa)
        let hayAlgunCampoModificado = false;

        row.find('input[data-original-value]').each(function () {
            const $input = $(this);
            const valorOriginal = $input.data('original-value');
            const valorActual = $input.val().replace(/,/g, '');

            // Verificar si está modificado según el tipo de campo
            if ($input.hasClass('input-tp_boni')) {
                // Lógica para bonificación
                const originalTrim = (valorOriginal || '').toString().trim();
                const actualTrim = (valorActual || '').toString().trim();

                if (!((originalTrim === actualTrim) ||
                    (originalTrim === "0" && actualTrim === "") ||
                    (originalTrim === "" && actualTrim === "0"))) {
                    hayAlgunCampoModificado = true;
                    return false; // Salir del bucle
                }
            } else {
                // Lógica para campos numéricos (simplificada para rendimiento)
                try {
                    const numOriginal = parseFloat(valorOriginal);
                    const numActual = parseFloat(valorActual);

                    if (!isNaN(numOriginal) && !isNaN(numActual) &&
                        Math.abs(numOriginal - numActual) > 0.0001) {
                        hayAlgunCampoModificado = true;
                        return false; // Salir del bucle
                    }
                } catch (e) { }
            }
        });

        // Actualizar según resultado
        if (hayAlgunCampoModificado && !estadoCargaActual) {
            row.data('carga', 1);
            row.attr('data-carga', '1');
            console.log(`Fila ${row.data('p-id')}: Cambiando data-carga a 1 (hay campos modificados no marcados)`);
        } else if (!hayAlgunCampoModificado && estadoCargaActual) {
            row.data('carga', 0);
            row.attr('data-carga', '0');
            console.log(`Fila ${row.data('p-id')}: Cambiando data-carga a 0 (no hay campos modificados)`);
        }

        return hayAlgunCampoModificado;
    }
}




// Función para actualizar el margen en una lista
function actualizarMargenLista(row, lpId, pId, nuevoMargen) {
    // Marcar el campo como modificado
    const campoMargen = row.find('.input-tp_margen_lista');
    const fueModificado = marcarCampoModificadoLista(campoMargen);

    // Solo proceder si realmente hubo un cambio
    if (fueModificado) {
        // Marcar la fila como modificada (siempre será 1 en este caso)
        row.data('carga', 1);
        row.attr('data-carga', '1');

        // Recopilar todos los datos necesarios para el resguardo
        const datos = {
            p_id: productoActualEnLista, 
            lp_id: lpId,
            tp_margen: nuevoMargen,
            tp_pvta: parseFloat(row.find('.input-tp_pvta_lista').val().replace(/,/g, '')),
            p_pcosto: parseFloat(row.find('input[name="p_pcosto"]').val()),
            p_pneto: parseFloat(row.find('input[name="tp_pneto"]').val()),
            lp_porc_mg: parseFloat(row.find('input[name="lp_porc_mg"]').val()),
            iva_situacion: row.find('input[name="iva_situacion"]').val(),
            iva_alicuota: parseFloat(row.find('input[name="iva_alicuota"]').val()),
            in_alicuota: parseFloat(row.find('input[name="in_alicuota"]').val()),
            tp_iva: parseFloat(row.find('input[name="tp_iva"]').val()),
            tp_in: parseFloat(row.find('input[name="tp_in"]').val())
        };

        // Actualizar el valor original para futuras comparaciones
        campoMargen.data('original-value', nuevoMargen);

        // Llamar al servidor para resguardar los cambios
        $.ajax({
            url: resguardarCambiosProductoListaUrl,
            type: 'POST',
            data: datos,
            dataType: 'json',
            success: function (response) {
                if (response.error) {
                    console.error('Error al resguardar cambios en lista:', response.msg);
                    AbrirMensaje("Error", "No se pudieron guardar los cambios: " + response.msg,
                        function () { $("#msjModal").modal("hide"); },
                        false, ["Aceptar"], "error!", null);
                } else if (response.warn) {
                    console.warn('Advertencia al resguardar cambios en lista:', response.msg);
                } else {
                    console.log('Cambios de margen en lista resguardados correctamente:', response.msg);
                    // Si el backend devuelve valores actualizados, podríamos aplicarlos
                    if (response.pvta) {
                        const campoPVta = row.find('.input-tp_pvta_lista');
                        campoPVta.val(parseFloat(response.pvta).toFixed(2));
                        campoPVta.data('original-value', parseFloat(response.pvta));
                    }
                }
            },
            error: function (xhr, status, error) {
                console.error('Error en la llamada AJAX al resguardar cambios en lista:', error);
                AbrirMensaje("Error", "Ocurrió un error al comunicarse con el servidor. Por favor, inténtelo nuevamente.",
                    function () { $("#msjModal").modal("hide"); },
                    false, ["Aceptar"], "error!", null);
            }
        });

        console.log(`Actualizando margen en servidor: Lista=${lpId}, Producto=${pId}, Valor=${nuevoMargen}`);
    }
}


// Función para actualizar el precio de venta en una lista
function actualizarPrecioVentaLista(row, lpId, pId, nuevoPrecioVenta) {
    // Log de diagnóstico
    console.log(`Entrando en actualizarPrecioVentaLista. LP ID: ${lpId}, P ID: ${pId}, Nuevo precio: ${nuevoPrecioVenta}`);

    // Validaciones de seguridad
    if (!row || !row.length) {
        console.error('Error: No se proporcionó una fila válida para actualizar el precio de venta');
        return;
    }

    if (!lpId) {
        console.error('Error: No se proporcionó un ID de lista de precio válido');
        lpId = row.data('lp-id');
        if (!lpId) {
            console.error('No se pudo obtener el ID de lista de precio de la fila');
            return;
        }
    }

    if (!pId) {
        console.error('Error: No se proporcionó un ID de producto válido');
        pId = productoActualEnLista;
        if (!pId) {
            console.error('No se pudo obtener el ID de producto actual');
            return;
        }
    }

    if (isNaN(nuevoPrecioVenta) || nuevoPrecioVenta <= 0) {
        console.error(`Error: Precio de venta inválido: ${nuevoPrecioVenta}`);
        return;
    }

    // Marcar el campo como modificado
    const campoPrecioVenta = row.find('.input-tp_pvta_lista');

    if (!campoPrecioVenta.length) {
        console.error('No se encontró el campo de precio de venta en la fila');
        return;
    }

    const fueModificado = marcarCampoModificadoLista(campoPrecioVenta);

    // Solo proceder si realmente hubo un cambio
    if (fueModificado) {
        console.log(`=== CAMPO MODIFICADO ===`);
        console.log(`Lista: ${lpId}, Producto: ${pId}`);
        console.log(`Valor original: ${campoPrecioVenta.data('original-value')}`);
        console.log(`Nuevo valor: ${nuevoPrecioVenta}`);

        // Recopilar todos los datos necesarios para el resguardo con validaciones
        try {
            const datos = {
                p_id: pId,
                lp_id: lpId,
                tp_margen: parseFloat(row.find('.input-tp_margen_lista').val().replace(/,/g, '')) || 0,
                tp_pvta: nuevoPrecioVenta,
                p_pcosto: parseFloat(row.find('input[name="p_pcosto"]').val()) || 0,
                p_pneto: parseFloat(row.find('input[name="tp_pneto"]').val()) || 0,
                lp_porc_mg: parseFloat(row.find('input[name="lp_porc_mg"]').val()) || 0,
                iva_situacion: row.find('input[name="iva_situacion"]').val() || 'E',
                iva_alicuota: parseFloat(row.find('input[name="iva_alicuota"]').val()) || 0,
                in_alicuota: parseFloat(row.find('input[name="in_alicuota"]').val()) || 0,
                tp_iva: parseFloat(row.find('input[name="tp_iva"]').val()) || 0,
                tp_in: parseFloat(row.find('input[name="tp_in"]').val()) || 0
            };

            console.log(`Datos que se enviarán:`, datos);

            // Actualizar el valor original para futuras comparaciones
            campoPrecioVenta.data('original-value', nuevoPrecioVenta);

            // Llamar al servidor para resguardar los cambios
            console.log(`Iniciando llamada AJAX a: ${resguardarCambiosProductoListaUrl}`);

            $.ajax({
                url: resguardarCambiosProductoListaUrl,
                type: 'POST',
                data: datos,
                dataType: 'json',
                success: function (response) {
                    console.log('Respuesta recibida del servidor:', response);

                    if (response.error) {
                        console.error('Error al resguardar cambios en lista:', response.msg);
                        AbrirMensaje("Error", "No se pudieron guardar los cambios: " + response.msg,
                            function () { $("#msjModal").modal("hide"); },
                            false, ["Aceptar"], "error!", null);
                    } else if (response.warn) {
                        console.warn('Advertencia al resguardar cambios en lista:', response.msg);
                    } else {
                        console.log('Cambios de precio en lista resguardados correctamente:', response.msg);
                        // Si el backend devuelve valores actualizados, podríamos aplicarlos
                        if (response.margen) {
                            const campoMargen = row.find('.input-tp_margen_lista');
                            campoMargen.val(parseFloat(response.margen).toFixed(2));
                            campoMargen.data('original-value', parseFloat(response.margen));
                        }
                    }
                },
                error: function (xhr, status, error) {
                    console.error('Error en la llamada AJAX al resguardar cambios en lista:', error);
                    console.error('Estado:', status);
                    console.error('Respuesta:', xhr.responseText);

                    AbrirMensaje("Error", "Ocurrió un error al comunicarse con el servidor. Por favor, inténtelo nuevamente.",
                        function () { $("#msjModal").modal("hide"); },
                        false, ["Aceptar"], "error!", null);
                }
            });

            console.log(`Actualizando precio venta en servidor: Lista=${lpId}, Producto=${pId}, Valor=${nuevoPrecioVenta}`);
        } catch (ex) {
            console.error('Excepción al procesar actualizarPrecioVentaLista:', ex);
        }
    } else {
        console.log('El campo de precio venta no fue modificado, no se enviarán cambios al servidor');
    }
}



// Función para optimizar la visualización de la tabla de listas
function optimizarVisualizacionTablaListas() {
    // Asegurarnos de que la tabla existe
    if ($("#tbProdLista").length === 0) {
        return;
    }

    // Ajustar columnas para mejor visualización
    $("#tbProdLista th:nth-child(2)").css('max-width', '180px'); // Lista
    $("#tbProdLista td:nth-child(2)").css({
        'max-width': '180px',
        'white-space': 'nowrap',
        'overflow': 'hidden',
        'text-overflow': 'ellipsis'
    });

    // Asegurarnos que la tabla tenga scroll horizontal si es necesario
    $("#tbProdLista").closest('.table-responsive').css('overflow-x', 'auto');

    console.log("Tabla de listas optimizada para mejor visualización");
}

function buscarProductoLista(primerProductoId) {
    // Si encontramos un producto, cargar sus listas de precios
    if (primerProductoId) {
        console.log("Cargando listas de precios para el producto ID:", primerProductoId);

        let datos = obtenerParametros(divs.ProductoListas);
        // Añadir el ID del producto a los parámetros
        datos.id = primerProductoId;

        // Mostrar indicador de carga en el div de listas de precios
        $("#divProdLista").html('<div class="text-center p-3"><i class="bx bx-loader bx-spin font-size-24"></i><p class="mt-2">Cargando listas de precios...</p></div>');

        // Realizar la segunda petición AJAX para obtener las listas de precios
        $.ajax({
            url: buscarProdListaUrl,
            type: "POST",
            data: datos,
            success: function (responseLista) {
                CerrarWaiting();
                // Mostrar resultados de listas de precios
                $("#divProdLista").html(responseLista);
                console.log("Listas de precios cargadas correctamente");

                // Aplicar configuraciones a la tabla de listas
                optimizarVisualizacionTablaListas();
                configurarInputsListaPreciosOptimizado();

                // Configurar eventos para la tabla de listas
                $("#tbProdLista tbody tr").on("click", function (e) {
                    // Solo activar si el clic no fue en un input
                    if (!$(e.target).is('input')) {
                        $(this).toggleClass("selected");
                    }
                });
            },
            error: function (error) {
                CerrarWaiting();
                console.error("Error al obtener las listas de precios:", error);
                $("#divProdLista").html('<div class="alert alert-danger">Error al cargar las listas de precios.</div>');
            }
        });
    } else {
        CerrarWaiting();
        console.warn("No se pudo obtener el ID del primer producto");
        $("#divProdLista").html('<div class="alert alert-warning">No se pudo obtener información de listas de precios.</div>');
    }
}
