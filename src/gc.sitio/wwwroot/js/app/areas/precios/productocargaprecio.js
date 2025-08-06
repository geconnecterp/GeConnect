const divs = {
    ProductoDetalle: "#divPCP",
    ProductoListas: "#divProdLista"
}
// 1. Agregar variable global para almacenar el p_id del producto actual cargado en la lista
let productoActualEnLista = null;
// ✅ CORREGIDO: Solo mantener la variable de control principal
let procesamientoMasivoActivo = false;

// Variable global para almacenar filas modificadas durante procesamiento masivo
window.filasModificadasGlobal = [];

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

// ✅ PROCESAMIENTO MASIVO SÍNCRONO
function recalcularCostosPorLotes(arrayFilas, inicio, tamanoLote, totalFilas, intervaloEntreLotes) {
    console.log(`🔄 Lote síncrono: ${inicio} a ${Math.min(inicio + tamanoLote, arrayFilas.length)} de ${totalFilas}`);

    const fin = Math.min(inicio + tamanoLote, arrayFilas.length);

    if (fin <= inicio) {
        if (fin < arrayFilas.length) {
            setTimeout(() => {
                recalcularCostosPorLotes(arrayFilas, fin, tamanoLote, totalFilas, intervaloEntreLotes);
            }, intervaloEntreLotes);
        } else {
            finalizarAplicacionCambios();
        }
        return;
    }

    // Activar modo de procesamiento masivo
    procesamientoMasivoActivo = true;

    // ✅ PROCESAR LOTE DE FORMA SÍNCRONA
    for (let i = inicio; i < fin; i++) {
        const fila = $(arrayFilas[i]);
        const productoId = fila.data('p-id');

        console.log(`⚙️ Procesando producto ${productoId} (${i + 1}/${arrayFilas.length})`);

        // Verificar si hay cambios que requieran cálculo
        const hayConceptosCosto = fila.find('.input-tp_plista.campo-modificado, .input-tp_dto1.campo-modificado, .input-tp_dto2.campo-modificado, .input-tp_dto3.campo-modificado, .input-tp_dto4.campo-modificado, .input-tp_dto_pa.campo-modificado, .input-tp_porc_flete.campo-modificado, .input-tp_boni.campo-modificado').length > 0;

        if (hayConceptosCosto) {
            // ✅ CALCULAR DE FORMA SÍNCRONA
            calcularProductoCompleto(fila);
            console.log(`✅ Producto ${productoId} procesado`);
        } else {
            console.log(`⏭️ Producto ${productoId} sin cambios, saltando`);
        }

        // Actualizar progreso cada producto
        const procesadosGlobales = inicio + (i - inicio) + 1;
        const porcentaje = Math.round((procesadosGlobales / totalFilas) * 100);

        $("#barraProgreso").css('width', porcentaje + '%');
        $("#filasCompletadas").text(procesadosGlobales);
        $("#textoProgreso").text(`Procesando productos... ${porcentaje}%`);

        // Permitir que la UI responda
        if (i % 5 === 0) {
            // Pequeña pausa cada 5 productos para no congelar la UI
            const pausa = Date.now() + 10;
            while (Date.now() < pausa) {
                // Pausa mínima
            }
        }
    }

    console.log(`✅ Lote completado: ${fin}/${totalFilas}`);

    // Continuar con el siguiente lote
    if (fin < arrayFilas.length) {
        setTimeout(() => {
            recalcularCostosPorLotes(arrayFilas, fin, tamanoLote, totalFilas, intervaloEntreLotes);
        }, intervaloEntreLotes);
    } else {
        procesamientoMasivoActivo = false;
        console.log(`🎉 ¡Todos los productos procesados síncronamente!`);
        finalizarAplicacionCambios();
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

// ✅ CORREGIR: Función con referencia correcta
function finalizarCalculosConActualizacionDiferida() {
    const dialogoProgreso = $("#dialogoProgresoAvanzado");

    const mostrarMensajeExito = function () {
        // ✅ LIMPIAR: Asegurar que no queden backdrops
        $('.modal-backdrop').remove();
        $('body').removeClass('modal-open');

        AbrirMensaje("Proceso completado",
            "Los precios se han calculado correctamente para todos los productos y sus listas de precios.",
            function () { $("#msjModal").modal("hide"); },
            false, ["Aceptar"], "success!", null);
    };

    // ✅ SIMPLIFICADO: Ya no hay actualizaciones diferidas, finalizar directamente
    procesamientoMasivoActivo = false;

    // ✅ CORREGIDO: Usar función existente
    cerrarModalYMostrarMensaje(dialogoProgreso, mostrarMensajeExito);
}

// ✅ SIMPLIFICADO: Función para finalizar la aplicación de cambios
function finalizarAplicacionCambios() {
    console.log("=== FINALIZANDO APLICACIÓN DE CAMBIOS ===");

    // ✅ LIMPIAR: Desactivar modo masivo y limpiar variables
    procesamientoMasivoActivo = false;
    window.filasModificadasGlobal = [];

    // Limpiar checkboxes y deshabilitar campos después de aplicar
    $('#chkPLista, #chkDto1, #chkDto2, #chkDto3, #chkDto4, #chkDpo, #chkBon, #chkFl').prop('checked', false);
    $('#txtPLista, #txtDto1, #txtDto2, #txtDto3, #txtDto4, #txtDpo, #txtBon, #txtFl').prop('disabled', true);

    // ✅ CORREGIDO: Cerrar diálogo de progreso de forma más robusta
    const dialogo = $("#dialogoProgresoAvanzado");

    // Función para mostrar mensaje final después de cerrar completamente el modal
    const mostrarMensajeFinal = function () {
        // ✅ ASEGURAR: Eliminar cualquier backdrop residual
        $('.modal-backdrop').remove();
        $('body').removeClass('modal-open');

        // Mostrar mensaje de éxito
        AbrirMensaje("Proceso completado",
            "Los cambios se han aplicado correctamente a los productos seleccionados y se han recalculado los costos, precios de venta y listas de precios.",
            function () {
                $("#msjModal").modal("hide");
                console.log("Proceso completamente terminado");
            },
            false, ["Aceptar"], "success!", null);
    };

    if (dialogo.length > 0) {
        console.log("Cerrando diálogo de progreso");

        // ✅ MEJORADO: Usar evento 'hidden.bs.modal' para asegurar cierre completo
        dialogo.off('hidden.bs.modal').on('hidden.bs.modal', function () {
            console.log("Diálogo de progreso cerrado completamente");
            $(this).remove();

            // ✅ SEGURIDAD: Pequeño delay para asegurar limpieza del DOM
            setTimeout(mostrarMensajeFinal, 100);
        });

        // Cerrar el modal
        dialogo.modal('hide');

        // ✅ SEGURIDAD ADICIONAL: Timeout por si el evento no se dispara
        setTimeout(function () {
            if (dialogo.length > 0) {
                console.warn("Timeout de seguridad: forzando cierre del diálogo");
                dialogo.remove();
                $('.modal-backdrop').remove();
                $('body').removeClass('modal-open');
                mostrarMensajeFinal();
            }
        }, 3000);
    } else {
        console.warn("No se encontró el diálogo de progreso para cerrar");
        // Si no hay diálogo, mostrar mensaje inmediatamente
        mostrarMensajeFinal();
    }

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
                // ✅ CORREGIDO: Cálculos completados - cerrar modal correctamente
                const dialogoProgreso = $("#dialogoProgresoAvanzado");

                const mostrarMensajeExito = function () {
                    // ✅ LIMPIAR: Asegurar que no queden backdrops
                    $('.modal-backdrop').remove();
                    $('body').removeClass('modal-open');

                    AbrirMensaje("Proceso completado",
                        "Los precios se han calculado correctamente para todos los productos.",
                        function () { $("#msjModal").modal("hide"); },
                        false, ["Aceptar"], "success!", null);
                };

                if (dialogoProgreso.length > 0) {
                    // Usar evento para asegurar cierre completo
                    dialogoProgreso.off('hidden.bs.modal').on('hidden.bs.modal', function () {
                        $(this).remove();
                        setTimeout(mostrarMensajeExito, 100);
                    });

                    dialogoProgreso.modal('hide');

                    // Timeout de seguridad
                    setTimeout(function () {
                        if (dialogoProgreso.length > 0) {
                            dialogoProgreso.remove();
                            $('.modal-backdrop').remove();
                            $('body').removeClass('modal-open');
                            mostrarMensajeExito();
                        }
                    }, 3000);
                } else {
                    mostrarMensajeExito();
                }
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

// ✅ CORREGIR: Función que se usa en procesarCalculosPrecios
function calcularCostoAPIConCallback(row, callback) {
    const productId = row.data('p-id');

    console.log(`🔄 Cálculo con callback para producto ${productId}`);

    // Evitar cálculos duplicados
    if (row.data('processing') === true) {
        console.log(`⏭️ Producto ${productId} ya en procesamiento`);
        if (callback) callback();
        return;
    }

    // Usar la función unificada con callback
    calcularProductoCompleto(row, callback);
}


// ✅ AGREGAR: Función simplificada para recálculo de relación precio venta
function recalcularRelacionPrecioVenta(row) {
    // Función placeholder para mantener compatibilidad
    // Se puede implementar la lógica específica si es necesaria
    console.log("Recalculando relación precio de venta para producto:", row.data('p-id'));
}

// ✅ NUEVA: Función auxiliar para obtener parámetros sin efectos visuales
function obtenerParametrosSilencioso() {
    // Obtener valores de los filtros
    const proveedor = $("#Rel01Item").val() || $("#Rel01List").val();

    // Validar que se haya seleccionado un proveedor
    if (!proveedor || proveedor === "") {
        console.error("Error: No se ha seleccionado un proveedor para la operación silenciosa");
        return false;
    }

    // Obtener el resto de parámetros
    const buscar = $("#Buscar").val() || "";
    const id = $("#Id").val() || "";
    const id2 = $("#Id2").val() || "";

    // Obtener rubros seleccionados
    const rubros = [];
    $("#Rel02List option").each(function () {
        rubros.push($(this).val());
    });

    // Obtener familias seleccionadas
    const familias = [];
    $("#Rel03List option").each(function () {
        familias.push({
            id: $(this).val(),
            descripcion: $(this).text()
        });
    });

    // Verificar opciones adicionales
    const incluirDiscontinuos = $("#Opt1").prop("checked");
    const generarArchivo = $("#Opt2").prop("checked");

    // ✅ SIN EFECTOS VISUALES - Solo logging para debugging
    console.log("Parámetros obtenidos silenciosamente para procesamiento masivo");

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

function configurarEventosTabla() {
    console.log("🔧 Configurando eventos de tabla...");

    $("#tbProdDet tbody tr").off("click");
    $("#tbProdDet tbody tr input[type='checkbox']").off("click change");

    // Checkboxes
    $("#tbProdDet tbody tr input[type='checkbox']").on("click", function (e) {
        e.stopPropagation();
        $(this).closest('tr').toggleClass("selected", $(this).is(':checked'));
    });

    // ✅ CRÍTICO: Evento click simplificado
    $("#tbProdDet tbody tr").on("click", function (e) {
        if ($(e.target).is('input[type="checkbox"]')) return;

        const productoId = $(this).data('p-id');
        if (!productoId) return;

        console.log(`🎯 Producto seleccionado: ${productoId}`);

        productoActualEnLista = productoId;
        destacarFilaSeleccionada(productoId);
        cargarDatosEnVistaPrevia(productoId);
        buscarProductoListaOptimizado(productoId); // ← FUNCIÓN CLAVE
    });

    console.log("✅ Eventos configurados");
}

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
    calcularProductoCompleto(row);
}, 300);

// ✅ SIMPLE: Solo una función debounced
const calcularPrecioVentaAPIDebounced = debounce(function (row) {
    calcularProductoCompleto(row);
}, 300);

const calcularPrecioVentaMargenAPIDebounced = debounce(function (row) {
    calcularMargenDesdePrecioSincrono(row);
}, 300);

// Función con debounce para cálculo de margen en listas
const calcularPrecioVentaMargenListaDebounced = debounce(function (row, lpId, pId, nuevoPrecioVenta) {
    calcularPrecioVentaMargenLista(row, lpId, pId, nuevoPrecioVenta);
}, 300);

// NUEVA: Función para calcular margen en grid de listas (equivalente a secuencia03)
// ✅ FUNCIÓN REFACTORIZADA: Calcular margen en grid de listas (equivalente a secuencia03)
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

                    // *** PASO 4: RESGUARDAR AUTOMÁTICAMENTE LOS CAMBIOS USANDO FUNCIÓN UNIFICADA ***
                    const datosBase = extraerDatosDesdeFilaLista(row);
                    const datosResguardo = construirDatosResguardoLista({
                        ...datosBase,
                        p_id: pId,
                        tp_margen: parseFloat(response.pvta.p_margen),
                        tp_pvta: nuevoPrecioVenta,
                        tp_iva: parseFloat(response.pvta.p_iva),
                        tp_in: parseFloat(response.pvta.p_in)
                    }, lpId);

                    // ✅ LLAMADA UNIFICADA PARA RESGUARDAR
                    resguardarCambiosListaUnificado(datosResguardo, {
                        modo: 'sync',
                        mostrarErrores: true,
                        logDetallado: true,
                        callback: function (response, success) {
                            if (success && response) {
                                console.log('Cambios de margen calculado resguardados exitosamente');
                                // Actualizar valores originales después del resguardo exitoso
                                campoMargenVisible.data('original-value', parseFloat(response.pvta.p_margen));
                                campoPVenta.data('original-value', nuevoPrecioVenta);
                            } else {
                                console.error('Error al resguardar cambios calculados');
                            }
                        }
                    });

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

function calcularPrecioVentaUniversal(row, callback = null) {
    calcularProductoCompleto(row, callback);
}

// NUEVA: Función específica para resguardar cambios después de cálculos de margen
function resguardarCambiosListaCalculados(row, lpId, pId, nuevoPrecioVenta, datosCalculados) {
    const datos = {
        p_id: pId,
        lp_id: lpId,
        tp_margen: parseFloat(datosCalculados.p_margen) || 0,
        tp_pvta: nuevoPrecioVenta,
        p_pcosto: parseFloat(row.find('input[name="p_pcosto"]').val()) || 0,
        p_pneto: parseFloat(datosCalculados.p_pneto) || 0,
        lp_porc_mg: parseFloat(row.find('input[name="lp_porc_mg"]').val()) || 0,
        iva_situacion: row.find('input[name="iva_situacion"]').val() || 'E',
        iva_alicuota: parseFloat(row.find('input[name="iva_alicuota"]').val()) || 0,
        in_alicuota: parseFloat(row.find('input[name="in_alicuota"]').val()) || 0,
        tp_iva: parseFloat(datosCalculados.p_iva) || 0,
        tp_in: parseFloat(datosCalculados.p_in) || 0
    };

    // ✅ DESPUÉS: Llamada unificada
    resguardarCambiosListaUnificado(datos, {
        modo: 'sync',
        mostrarErrores: true,
        logDetallado: true,
        callback: function (response, success) {
            if (success && response) {
                // Actualizar valores originales
                const campoMargen = row.find('.input-tp_margen_lista');
                const campoPVenta = row.find('.input-tp_pvta_lista');
                campoMargen.data('original-value', parseFloat(datosCalculados.p_margen));
                campoPVenta.data('original-value', nuevoPrecioVenta);
            }
        }
    });
}

// Función de utilidad para destacar la fila seleccionada
// ✅ MEJORADA: Función destacar fila con verificación adicional
function destacarFilaSeleccionada(productoId) {
    console.log(`🎯 Destacando fila para producto ID: ${productoId}`);

    // Remover el destacado de todas las filas
    $("#tbProdDet tbody tr").removeClass("selected");

    // Verificar que existe una fila con ese ID
    const $fila = $("#tbProdDet tbody tr[data-p-id='" + productoId + "']");

    if ($fila.length === 0) {
        console.warn(`⚠️ No se encontró ninguna fila con data-p-id="${productoId}"`);
        return false;
    }

    // Añadir el destacado solo a la fila del producto seleccionado
    $fila.addClass("selected");
    console.log(`✅ Fila destacada correctamente para producto ${productoId}`);

    // Hacer scroll a la fila si está fuera de vista
    scrollAFilaSeleccionada($fila);

    return true;
}

// ✅ NUEVA: Función separada para scroll optimizado
function scrollAFilaSeleccionada($fila) {
    const $tableContainer = $("#tbProdDet").closest('.table-responsive');

    if ($tableContainer.length > 0) {
        const containerTop = $tableContainer.offset().top;
        const containerHeight = $tableContainer.height();
        const rowTop = $fila.offset().top;

        // Solo hacer scroll si la fila está fuera del área visible
        if (rowTop < containerTop || rowTop > containerTop + containerHeight) {
            $tableContainer.animate({
                scrollTop: $tableContainer.scrollTop() + (rowTop - containerTop - containerHeight / 2)
            }, 300);
            console.log(`📜 Realizando scroll a la fila seleccionada`);
        }
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

    // ✅ AGREGAR: Función helper para cerrar modales
    function cerrarModalYMostrarMensaje(modal, callbackMensaje) {
        if (modal.length > 0) {
            modal.off('hidden.bs.modal').on('hidden.bs.modal', function () {
                $(this).remove();
                setTimeout(callbackMensaje, 100);
            });

            modal.modal('hide');

            // Timeout de seguridad
            setTimeout(function () {
                if (modal.length > 0) {
                    modal.remove();
                    $('.modal-backdrop').remove();
                    $('body').removeClass('modal-open');
                    callbackMensaje();
                }
            }, 3000);
        } else {
            callbackMensaje();
        }
    }

    // ✅ AGREGAR: Función faltante para recalcular precio neto
    function recalcularPrecioNeto(row) {
        console.log("Recalculando precio neto para producto:", row.data('p-id'));

        // Esta función se usa cuando cambia el margen
        // Llamar al cálculo completo para mantener consistencia
        calcularProductoCompleto(row);
    }

    // Si el campo cambiado es precio de venta o impuesto interno
    if (changedField.hasClass('input-tp_pvta') || changedField.hasClass('input-tin_alicuota')) {
        // Recalcular relación con precio venta
        recalcularRelacionPrecioVenta(row);
    }
}

// Función auxiliar para los recálculos (modificada para evitar llamadas redundantes)
// ✅ CORREGIDA: Función auxiliar para los recálculos
function recalcularCosto(row) {
    // Si ya estamos calculando el costo para esta fila, no hacer nada
    if (row.data('calculating-cost') === true) {
        return;
    }

    // Esta función ahora simplemente llama a calcularCostoAPI
    calcularCostoAPI(row);
}


// ✅ SIMPLIFICADO: Eventos más concisos
function cargaEventosCP() {
    // Observador para Rel01List
    if (document.getElementById('Rel01List')) {
        new MutationObserver(() => verificarYDesactivarControles())
            .observe(document.getElementById('Rel01List'), { childList: true, subtree: true });
    }

    // Eventos principales
    $("#Rel01List").on("change", verificarYDesactivarControles);
    $("#Rel01").on("autocompleteselect", () => setTimeout(verificarYDesactivarControles, 100));

    // Evento para chkRel01
    $("#chkRel01").on("change", function () {
        if (!$(this).is(":checked")) {
            ["#chkRel03", "#Rel03", "#Rel03List"].forEach(sel => $(sel).prop("checked", false).prop("disabled", true).empty());
        }
    });

    // Evento para Rel03
    $("#Rel03").on("change", function () {
        const selectedValue = $(this).val();
        const selectedText = $(this).find("option:selected").text();

        if (selectedValue && $("#Rel03List option[value='" + selectedValue + "']").length === 0) {
            $("#Rel03List").append($("<option>").attr("value", selectedValue).text(selectedText).prop("selected", true));
            $("#Rel03Item").val(selectedValue);
            $(this).val("");
        }
    });

    // Evento para chkFile simplificado
    $("#chkFile").on("change", function () {
        const isChecked = $(this).is(":checked");
        if (isChecked) {
            $("input[type='checkbox']").not("#chkFile, #chkRel01").prop({ "checked": false, "disabled": true });
            $("input[type='text']").not("#Rel01").prop("disabled", true);
            $("select").not("#Rel01List").prop("disabled", true).empty();
        } else {
            $("input[type='checkbox']").not("#chkFile").prop("disabled", false);
            // Restaurar lógica específica según estado actual
            if ($("#chkRel01").is(":checked") && $("#Rel01List").find("option").length > 0) {
                $("#chkRel03").prop("disabled", false);
            }
        }
    });

    // Evento blur simplificado
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
// ✅ SIMPLIFICADO: Configuración de botones más limpia
function configurarBotonesProdCP() {
    // Configuración básica de botones
    $("#btnCancel, #btnAbmCancelar").on("click", () => window.location.href = homeCPUrl);

    $("#btnBuscar").on("click", function (e) {
        e.preventDefault();
        if (typeof consCta === 'undefined' || !consCta) {
            AbrirMensaje("ATENCIÓN", "Debe seleccionar un proveedor antes de realizar la búsqueda.",
                () => $("#msjModal").modal("hide"), false, ["Entendido"], "warn!", null);
            return false;
        }
        AbrirWaiting("Cargando productos...");
        buscarProductosDetalle();
        inicializaControlCuenta();
    });

    // Configuración de estados iniciales
    $("#btnAbmAceptar").prop("disabled", true);
    $("#lbRel01, #lbRel02, #lbRel03").text((i, txt) => ["PROVEEDOR", "RUBRO", "FAMILIA"][i]);
    $("#chkRel03").prop("disabled", true);

    // Configurar eventos de familia
    $("#chkRel03").on("change", function () {
        const isChecked = $(this).is(":checked");
        $("#Rel03, #Rel03List").prop("disabled", !isChecked);
        if (!isChecked) {
            $("#Rel03").val("");
            $("#Rel03List").empty();
            $("#Rel03Item").val("");
        }
    });

    // Evento click mejorado para desactivar campos
    $(document).off('click.productoCargaPrecio').on('click.productoCargaPrecio', function (e) {
        const $target = $(e.target);
        const enAreaControlada = $target.closest('#tbProdDet, #divPCP').length > 0;

        if (enAreaControlada && !$target.is('.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni, .input-tp_margen, .input-tin_alicuota, .input-tp_pvta')) {
            $('.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni, .input-tp_margen, .input-tin_alicuota, .input-tp_pvta')
                .filter(':not([readonly])').each(function () {
                    this.dispatchEvent(new Event('blur', { bubbles: true }));
                });
        }
    });

    // Verificación inicial diferida
    setTimeout(() => {
        verificarYDesactivarControles(true);
        if ($("#chkFile").is(":checked")) $("#chkFile").trigger("change");
    }, 100);
}
// ✅ CORREGIDO: En procesamiento masivo - función auxiliar
function obtenerParametrosMasivo() {
    // ✅ NUEVA: Función específica para procesamiento masivo
    return obtenerParametros(null, 'masivo'); // Sin div y contexto masivo
}

// ✅ AGREGAR: Función helper para mostrar mensajes de error
function mostrarMensajeError(mensaje) {
    AbrirMensaje("Error", mensaje,
        function () { $("#msjModal").modal("hide"); },
        false, ["Aceptar"], "error!", null);
}
// ✅ SIMPLIFICADO: Versión más limpia y eficiente
function obtenerParametros(div = null, contexto = 'normal') {
    // Validación básica
    const proveedor = $("#Rel01Item").val() || $("#Rel01List").val();
    if (!proveedor) {
        if (contexto !== 'masivo') {
            mostrarMensajeError("Debe seleccionar un proveedor para realizar la búsqueda.");
        }
        return false;
    }

    // Mostrar indicador si es necesario
    if (contexto !== 'masivo' && div) {
        const mensajes = {
            'listas': 'Obteniendo listas de precios...',
            'busqueda': 'Buscando productos...',
            'normal': 'Cargando datos...'
        };
        const mensaje = mensajes[contexto] || mensajes.normal;
        $(div).html(`<div class="text-center p-3"><i class="bx bx-loader bx-spin font-size-24"></i><p class="mt-2">${mensaje}</p></div>`);
    }

    // Obtener parámetros de forma eficiente
    return {
        buscar: $("#Buscar").val() || "",
        id: $("#Id").val() || "",
        id2: $("#Id2").val() || "",
        ctaId: proveedor,
        familias: $("#Rel03List option").map((i, opt) => ({
            id: $(opt).val(),
            descripcion: $(opt).text()
        })).get(),
        rubros: $("#Rel02List option").map((i, opt) => $(opt).val()).get(),
        disc: $("#Opt1").prop("checked"),
        file: $("#Opt2").prop("checked")
    };
}

// Modificar la función buscarProductosDetalle para asegurar la correcta secuencia de inicialización
// ✅ VERIFICADA: Función que también debe cargar listas del primer producto
function buscarProductosDetalle() {
    let datos = obtenerParametros(divs.ProductoDetalle, 'busqueda');
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
                        // ✅ CRÍTICO: Actualizar variable global
                        productoActualEnLista = primerProductoId;
                        $("#divProdLista").attr('data-producto-actual', primerProductoId);

                        destacarFilaSeleccionada(primerProductoId);

                        // NUEVO: Cargar los datos originales en la vista previa
                        cargarDatosEnVistaPrevia(primerProductoId);

                        // ✅ CRÍTICO CORREGIDO: Cargar listas del primer producto
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

// ✅ ASEGURAR: Que esta función esté funcionando correctamente
function buscarProductoListaOptimizado(productoId) {
    console.log(`🔄 BUSCANDO listas para producto: ${productoId}`);

    // Validaciones básicas
    if (!productoId) {
        console.error("❌ No se proporcionó productoId");
        return;
    }

    // ✅ AGREGAR: Log adicional para debugging
    console.log(`📋 Estado actual: procesamientoMasivoActivo=${procesamientoMasivoActivo}, productoActualEnLista=${productoActualEnLista}`);

    // Control de concurrencia
    if (window.currentListasXHR) {
        console.log("🔄 Cancelando request anterior");
        window.currentListasXHR.abort();
        window.currentListasXHR = null;
    }

    // Actualizar variables
    productoActualEnLista = productoId;
    $("#divProdLista").attr('data-producto-actual', productoId);

    // Mostrar indicador
    $("#divProdLista").html('<div class="text-center p-3"><i class="bx bx-loader bx-spin font-size-24"></i><p class="mt-2">🔄 Cargando listas...</p></div>');

    // Obtener parámetros
    let datos = obtenerParametros(null, 'listas');
    if (datos === false) {
        console.error("❌ Error obteniendo parámetros");
        $("#divProdLista").html('<div class="alert alert-danger">❌ Error preparando consulta</div>');
        return;
    }

    datos.id = productoId;
    datos.verificarTemp = true;

    console.log(`📤 Enviando request para producto ${productoId}:`, datos);

    // Request AJAX con logging detallado
    window.currentListasXHR = $.ajax({
        url: buscarProdListaUrl,
        type: "POST",
        data: datos,
        timeout: 30000,
        success: function (responseLista) {
            window.currentListasXHR = null;
            console.log(`✅ Response recibido para producto ${productoId}:`, responseLista ? 'con datos' : 'vacío');

            if (!responseLista || responseLista.trim() === '') {
                $("#divProdLista").html('<div class="alert alert-info">ℹ️ Sin listas disponibles</div>');
                return;
            }

            $("#divProdLista").html(responseLista);
            console.log(`✅ Listas cargadas exitosamente para producto ${productoId}`);

            // Inicializar componentes
            setTimeout(() => inicializarComponentesListas(productoId), 50);
        },
        error: function (xhr, status, error) {
            window.currentListasXHR = null;
            if (status !== 'abort') {
                console.error(`❌ Error cargando listas para ${productoId}:`, error);
                $("#divProdLista").html(`
                    <div class="alert alert-danger">
                        ❌ Error cargando listas: ${error}
                        <button class="btn btn-sm btn-outline-primary ms-2" onclick="buscarProductoListaOptimizado(${productoId})">🔄 Reintentar</button>
                    </div>
                `);
            }
        }
    });
}

// ✅ FUNCIÓN DE DEBUG: Para verificar que todo funcione
function debugEventosTabla() {
    console.log("🐛 DEBUGGING - Estado de eventos de tabla:");

    // Verificar si hay filas
    const filas = $("#tbProdDet tbody tr");
    console.log(`📊 Filas encontradas: ${filas.length}`);

    // Verificar si tienen eventos
    filas.each(function (index) {
        const eventos = $._data(this, 'events');
        console.log(`Fila ${index}: eventos =`, eventos ? Object.keys(eventos) : 'ninguno');
    });

    // Verificar variables globales
    console.log(`🌐 productoActualEnLista: ${productoActualEnLista}`);
    console.log(`🌐 procesamientoMasivoActivo: ${procesamientoMasivoActivo}`);

    // Verificar URLs
    console.log(`🔗 buscarProdListaUrl: ${typeof buscarProdListaUrl !== 'undefined' ? buscarProdListaUrl : 'NO DEFINIDA'}`);
}

// ✅ LLAMAR EN CONSOLA: debugEventosTabla(); para verificar estado

// ✅ NUEVA: Función separada para inicializar componentes de listas
function inicializarComponentesListas(productoId) {
    console.log(`🔧 Inicializando componentes de listas para producto ${productoId}`);

    // Usar setTimeout para permitir que el DOM se actualice completamente
    setTimeout(() => {
        if ($("#tbProdLista").length > 0) {
            // 1. Optimizar visualización de tabla
            optimizarVisualizacionTablaListas();

            // 2. Configurar inputs de listas
            configurarInputsListaPreciosOptimizado();

            // 3. Detectar y resaltar registros temporales
            const registrosTemporales = $("#tbProdLista tbody tr[data-carga='1']").length;
            if (registrosTemporales > 0) {
                console.log(`📋 Detectados ${registrosTemporales} registros temporales en las listas`);
                $("#divProdLista").prepend(
                    `<div class="alert alert-info alert-dismissible fade show" role="alert">
                        <i class="bx bx-info-circle me-1"></i>
                        Se están mostrando <strong>${registrosTemporales} registros modificados</strong> pendientes de confirmación.
                        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                    </div>`
                );
            }

            // 4. Configurar eventos de tabla de listas
            configurarEventosTableListas();

            console.log(`✅ Componentes de listas inicializados para producto ${productoId}`);
        } else {
            console.warn(`⚠️ No se encontró tabla de listas para producto ${productoId}`);
        }
    }, 50); // Pequeño delay para asegurar que el DOM esté listo
}

// ✅ NUEVA: Configurar eventos específicos de la tabla de listas
function configurarEventosTableListas() {
    // Limpiar eventos previos
    $("#tbProdLista tbody tr").off("click.tableListas");

    // Configurar evento click para selección de filas de listas
    $("#tbProdLista tbody tr").on("click.tableListas", function (e) {
        // Solo activar si el clic no fue en un input
        if (!$(e.target).is('input')) {
            $(this).toggleClass("selected");
            console.log(`Fila de lista ${$(this).data('lp-id')} seleccionada`);
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

// ✅ SIMPLIFICADO: Eventos de edición más eficientes
function configurarEventosEdicionOptimizado() {
    const camposEditables = '.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni, .input-tp_margen, .input-tin_alicuota, .input-tp_pvta';
    const camposSecuencia01 = '.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni';

    // Limpiar eventos previos
    $(document).off('click.camposEditables keydown.camposEditables blur.camposSecuencia01 blur.campoMargen blur.campoPVta blur.campoImpuesto');

    // Evento click unificado
    $(document).on('click.camposEditables', camposEditables, function (e) {
        e.stopPropagation();

        const $this = $(this);
        const pIdDetalle = $this.closest('tr').data('p-id');

        // Cambio de producto si es necesario
        if (pIdDetalle !== productoActualEnLista) {
            productoActualEnLista = pIdDetalle;
            $("#divProdLista").attr('data-producto-actual', pIdDetalle);
            destacarFilaSeleccionada(pIdDetalle);
            buscarProductoListaOptimizado(pIdDetalle);
        }

        // Habilitar campo
        $this.prop('readonly', false).removeClass('campo-readonly');
        setTimeout(() => { $this[0].focus(); $this[0].select(); }, 0);
    });

    // Evento keydown unificado
    $(document).on('keydown.camposEditables', camposEditables, function (e) {
        if (e.key === 'Enter' || e.key === 'Tab') {
            e.preventDefault();

            const row = $(this).closest('tr');
            const esSecuencia01 = $(this).is(camposSecuencia01);
            const esMargen = $(this).hasClass('input-tp_margen');
            const esPrecioVenta = $(this).hasClass('input-tp_pvta');

            marcarCampoModificado(this);
            actualizarEstadoCarga(row);
            activarSiguienteCampo(this);

            // Aplicar cálculos según tipo
            if (esSecuencia01) calcularCostoAPIDebounced(row);
            else if (esMargen) calcularPrecioVentaAPIDebounced(row);
            else if (esPrecioVenta) calcularPrecioVentaMargenAPIDebounced(row);
        }
    });

    // Eventos blur simplificados con delegación
    const eventosBlur = {
        [camposSecuencia01]: () => calcularCostoAPIDebounced,
        '.input-tp_margen': () => calcularPrecioVentaAPIDebounced,
        '.input-tp_pvta': () => calcularPrecioVentaMargenAPIDebounced,
        '.input-tin_alicuota': () => recalcularRelacionPrecioVenta
    };

    Object.entries(eventosBlur).forEach(([selector, getCallback]) => {
        $(document).on(`blur.${selector.replace(/[^a-zA-Z]/g, '')}`, selector, function () {
            if ($(this).prop('readonly')) return;

            const row = $(this).closest('tr');
            const value = $(this).val().replace(/,/g, '');
            const numValue = parseFloat(value);

            if (!isNaN(numValue)) {
                const decimals = $(this).hasClass('input-tp_plista') || $(this).hasClass('input-tp_pcosto') || $(this).hasClass('input-tp_pneto') ? 3 :
                    $(this).hasClass('input-tp_dto1') || $(this).hasClass('input-tp_dto2') || $(this).hasClass('input-tp_dto3') || $(this).hasClass('input-tp_dto4') || $(this).hasClass('input-tp_dto_pa') || $(this).hasClass('input-tp_porc_flete') ? 1 : 2;
                $(this).val(numValue.toFixed(decimals));
            }

            $(this).prop('readonly', true).addClass('campo-readonly');
            getCallback()(row);
        });
    });
}

// ✅ FUNCIÓN AUXILIAR: Activar siguiente campo
function activarSiguienteCampo(campoActual) {
    const $campoActual = $(campoActual);
    const $fila = $campoActual.closest('tr');
    const camposEditables = '.input-tp_plista, .input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete, .input-tp_boni, .input-tp_margen, .input-tin_alicuota, .input-tp_pvta';
    const $camposEnFila = $fila.find(camposEditables);
    const indiceActual = $camposEnFila.index($campoActual);

    let $siguienteCampo = null;
    if (indiceActual < $camposEnFila.length - 1) {
        $siguienteCampo = $camposEnFila.eq(indiceActual + 1);
    } else if ($fila.next('tr').length) {
        $siguienteCampo = $fila.next('tr').find(camposEditables).first();
    }

    $campoActual.prop('readonly', true).addClass('campo-readonly');

    if ($siguienteCampo && $siguienteCampo.length) {
        $siguienteCampo.prop('readonly', false).removeClass('campo-readonly');
        setTimeout(() => { $siguienteCampo[0].focus(); $siguienteCampo[0].select(); }, 0);
    }
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

// Función para llamar a la API de cálculo de costo - Versión corregida
function calcularCostoAPI(row) {
    calcularProductoCompleto(row);
}

function calcularPrecioVentaAPI(row) {
    calcularProductoCompleto(row);
}

function calcularPrecioVentaMargenAPI(row) {
    // Para secuencia 3 (precio → margen), usar función específica
    calcularMargenDesdePrecioSincrono(row);
}

// ✅ FUNCIÓN ESPECÍFICA: Calcular margen desde precio (secuencia 3)
function calcularMargenDesdePrecioSincrono(row) {
    const productId = row.data('p-id');

    console.log(`🔢 Calculando margen desde precio SÍNCRONO para producto ${productId}`);

    if (row.data('processing') === true) {
        console.log(`⏭️ Producto ${productId} ya en procesamiento`);
        return;
    }

    row.data('processing', true);

    try {
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

        const campoPrecioNeto = row.find('.input-tp_pneto');
        const valorOriginal = campoPrecioNeto.val();
        campoPrecioNeto.val('...').addClass('calculating');

        const response = realizarLlamadaSincrona(calcularPrecioVentaMargenUrl, datos);

        campoPrecioNeto.removeClass('calculating');

        if (response.error || response.warn) {
            campoPrecioNeto.val(valorOriginal);
            console.error(`❌ Error en cálculo de margen: ${response.msg}`);
            return;
        }

        // Actualizar precio neto calculado
        const pneto = parseFloat(response.pvta.p_pneto).toFixed(3);
        campoPrecioNeto.val(pneto);
        marcarCampoModificado(campoPrecioNeto);

        // Actualizar campos ocultos
        row.find('input[name="p_margen"]').val(response.pvta.p_margen);
        row.find('input[name="tp_iva"]').val(response.pvta.p_iva);
        row.find('input[name="tp_in"]').val(response.pvta.p_in);

        actualizarEstadoCarga(row);
        resguardarCambiosProducto(row);

        console.log(`✅ Margen calculado exitosamente para producto ${productId}`);

    } catch (error) {
        console.error(`💥 Error en cálculo de margen para producto ${productId}:`, error);
    } finally {
        row.data('processing', false);
    }
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
// ✅ VERIFICADA: Función optimizada para marcar campos modificados en listas
function marcarCampoModificadoLista(input) {
    const $input = $(input);

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

    // ✅ MEJORADO: Consideramos siempre modificado si no hay valor original definido
    let esModificado = false;

    if (valorOriginal === undefined) {
        // Si no hay valor original, considerar modificado si hay valor actual
        esModificado = !isNaN(numActual) && numActual !== 0;
    } else {
        // Consideramos diferente si hay una diferencia mayor a 0.01
        esModificado = Math.abs(numOriginal - numActual) > 0.01;
    }

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

    return esModificado;
}

function marcarCamposModificados(row, datosCalculados) {
    console.log(`📝 Marcando campos modificados para producto ${row.data('p-id')}`);

    // Marcar los campos que fueron actualizados por los cálculos
    const campoCosto = row.find('.input-tp_pcosto');
    const campoPrecioNeto = row.find('.input-tp_pneto');
    const campoPrecioVenta = row.find('.input-tp_pvta');

    if (campoCosto.length) marcarCampoModificado(campoCosto);
    if (campoPrecioNeto.length) marcarCampoModificado(campoPrecioNeto);
    if (campoPrecioVenta.length) marcarCampoModificado(campoPrecioVenta);

    // Actualizar el estado de carga de la fila
    actualizarEstadoCarga(row);
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
    const campoMargen = row.find('.input-tp_margen_lista');
    
    if (!marcarCampoModificadoLista(campoMargen)) return;

    const datosBase = extraerDatosDesdeFilaLista(row);
    const datos = construirDatosResguardoLista({
        ...datosBase,
        p_id: pId,
        tp_margen: nuevoMargen,
        tp_pvta: parseFloat(row.find('.input-tp_pvta_lista').val().replace(/,/g, ''))
    }, lpId);

    resguardarCambiosListaUnificado(datos, {
        modo: 'sync',
        callback: (response, success) => {
            if (success) {
                campoMargen.data('original-value', nuevoMargen);
                row.data('carga', 1).attr('data-carga', '1');
            }
        }
    });
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

// ✅ FUNCIÓN PRINCIPAL ACTUALIZADA: Distinguir contexto individual vs masivo
function calcularProductoCompleto(row, callback = null) {
    const productId = row.data('p-id');

    console.log(`🔄 Iniciando cálculo completo ${procesamientoMasivoActivo ? 'MASIVO' : 'INDIVIDUAL'} para producto ${productId}`);

    if (row.data('processing') === true) {
        console.log(`⏭️ Producto ${productId} ya en procesamiento, saltando`);
        if (callback) callback();
        return;
    }

    row.data('processing', true);

    try {
        // ✅ PASO 1: CALCULAR COSTO
        const resultadoCosto = calcularCostoSincrono(row);
        if (!resultadoCosto.success) {
            console.error(`❌ Error en cálculo de costo: ${resultadoCosto.error}`);
            row.data('processing', false);
            if (callback) callback();
            return;
        }

        // ✅ PASO 2: CALCULAR PRECIO DE VENTA
        const resultadoPrecio = calcularPrecioVentaSincrono(row);
        if (!resultadoPrecio.success) {
            console.error(`❌ Error en cálculo de precio: ${resultadoPrecio.error}`);
            row.data('processing', false);
            if (callback) callback();
            return;
        }

        // ✅ PASO 3: ACTUALIZAR LISTAS SEGÚN CONTEXTO
        let resultadoListas = { success: true, skip: true };

        if (procesamientoMasivoActivo) {
            // ✅ MASIVO: Actualizar datos en servidor solamente
            resultadoListas = actualizarListasSincrono(productId, resultadoPrecio.datos);
            logResultadoListas(productId, resultadoListas);
        } else {
            // ✅ INDIVIDUAL: Actualizar servidor Y grilla visible CON MARCADO
            resultadoListas = actualizarListasIndividual(productId, resultadoPrecio.datos);

            // ✅ LOGGING ESPECÍFICO PARA INDIVIDUAL
            if (resultadoListas.success && resultadoListas.camposModificadosGrilla > 0) {
                console.log(`🎯 INDIVIDUAL: ${resultadoListas.camposModificadosGrilla} campos de listas marcados como modificados en grilla`);
            }
        }

        if (!resultadoListas.success && !resultadoListas.skip) {
            console.error(`❌ Error crítico en listas: ${resultadoListas.error}`);
            // Continuar el proceso aunque falle las listas
        }

        // ✅ PASO 4: FINALIZAR
        marcarCamposModificados(row, resultadoPrecio.datos);
        resguardarCambiosProducto(row);

        console.log(`✅ Cálculo completo finalizado para producto ${productId}`);

    } catch (error) {
        console.error(`💥 Error general en cálculo de producto ${productId}:`, error);
    } finally {
        row.data('processing', false);
        if (callback) callback();
    }
}

// ✅ NUEVA: Función específica para actualización individual
function actualizarListasIndividual(productId, datosProducto) {
    console.log(`🎯 Actualizando listas INDIVIDUAL para producto ${productId}`);

    try {
        // ✅ VALIDAR: Datos de entrada
        if (!productId || !datosProducto) {
            console.error("❌ Parámetros inválidos para actualización individual");
            return { success: false, error: "Parámetros inválidos" };
        }

        // ✅ VALIDAR: tp_pneto válido
        if (!datosProducto.tp_pneto || isNaN(datosProducto.tp_pneto) || datosProducto.tp_pneto <= 0) {
            console.warn(`⚠️ tp_pneto inválido (${datosProducto.tp_pneto}) para producto ${productId}`);
            return { success: true, skip: true, reason: "tp_pneto inválido" };
        }

        // ✅ VERIFICAR: Si hay grilla de listas visible para este producto
        const esProductoActual = productoActualEnLista === productId;
        const hayGrillaVisible = $("#tbProdLista").length > 0 && $("#tbProdLista tbody tr").length > 0;

        if (!esProductoActual || !hayGrillaVisible) {
            console.log(`ℹ️ No hay grilla visible para producto ${productId}, solo actualizar servidor`);
            return actualizarListasServidor(productId, datosProducto);
        }

        // ✅ DUAL: Actualizar servidor Y grilla visible
        return actualizarListasServidorYGrilla(productId, datosProducto);

    } catch (error) {
        console.error(`💥 Error en actualización individual:`, error);
        return { success: false, error: error.message };
    }
}

// ✅ MEJORADA: Actualizar servidor Y grilla visible con logging detallado
function actualizarListasServidorYGrilla(productId, datosProducto) {
    console.log(`🔄 Actualizando servidor Y grilla para producto ${productId}`);

    try {
        // ✅ PASO 1: Actualizar en servidor (como masivo)
        const resultadoServidor = actualizarListasServidor(productId, datosProducto);

        if (!resultadoServidor.success) {
            console.error(`❌ Error actualizando servidor: ${resultadoServidor.error}`);
            return resultadoServidor;
        }

        if (resultadoServidor.skip) {
            console.log(`⏭️ Servidor omitido: ${resultadoServidor.reason}`);
            return resultadoServidor;
        }

        // ✅ PASO 2: Actualizar grilla visible
        const resultadoGrilla = actualizarGrillaListasVisible(productId, datosProducto);

        // ✅ LOGGING DETALLADO
        if (resultadoGrilla.success && resultadoGrilla.camposModificados > 0) {
            console.log(`🎯 INDIVIDUAL: ${resultadoGrilla.camposModificados} campos marcados como modificados en grilla visible`);
        }

        // ✅ COMBINAR: Resultados
        return {
            success: resultadoServidor.success && resultadoGrilla.success,
            listasActualizadas: resultadoServidor.listasActualizadas || 0,
            grillaActualizada: resultadoGrilla.success,
            camposModificadosGrilla: resultadoGrilla.camposModificados || 0,
            total: resultadoServidor.total || 0,
            error: !resultadoGrilla.success ? resultadoGrilla.error : null
        };

    } catch (error) {
        console.error(`💥 Error en actualización dual:`, error);
        return { success: false, error: error.message };
    }
}

// ✅ MEJORADA: Función con logging detallado para debugging
function actualizarGrillaListasVisible(productId, datosProducto) {
    console.log(`📋 Actualizando grilla visible para producto ${productId}`);

    try {
        // ✅ VERIFICAR: Grilla visible
        const $filasLista = $("#tbProdLista tbody tr");
        if ($filasLista.length === 0) {
            console.log(`ℹ️ No hay filas visibles en grilla para producto ${productId}`);
            return { success: true, skip: true, reason: "Sin grilla visible" };
        }

        let filasActualizadas = 0;
        let camposModificados = 0;
        let errores = 0;

        // ✅ PROCESAR: Cada fila visible
        $filasLista.each(function () {
            const $fila = $(this);
            const lp_id = $fila.data('lp-id');

            if (!lp_id) {
                console.warn(`⚠️ Fila sin lp-id, omitiendo`);
                return true; // Continuar
            }

            try {
                // ✅ CALCULAR: Nuevo precio para esta lista
                const resultado = calcularPrecioListaParaGrilla(lp_id, datosProducto, $fila);

                if (resultado.success) {
                    // ✅ CONTAR: Campos antes de actualizar
                    const camposAntes = $fila.find('.campo-modificado').length;

                    // ✅ ACTUALIZAR: Campos visibles en la grilla
                    actualizarCamposVisiblesEnGrilla($fila, resultado.datos);

                    // ✅ CONTAR: Campos después de actualizar
                    const camposDespues = $fila.find('.campo-modificado').length;
                    const camposNuevosModificados = camposDespues - camposAntes;

                    filasActualizadas++;
                    camposModificados += camposNuevosModificados;

                    console.log(`✅ Lista ${lp_id}: precio ${resultado.datos.tp_pvta}, margen ${resultado.datos.tp_margen} (+${camposNuevosModificados} campos modificados)`);
                } else {
                    errores++;
                    console.error(`❌ Error calculando grilla lista ${lp_id}: ${resultado.error}`);
                }

            } catch (error) {
                errores++;
                console.error(`💥 Excepción procesando grilla lista ${lp_id}:`, error.message);
            }
        });

        console.log(`📊 Grilla actualizada: ${filasActualizadas} filas, ${camposModificados} campos modificados, ${errores} errores`);

        return {
            success: errores === 0,
            filasActualizadas: filasActualizadas,
            camposModificados: camposModificados,
            errores: errores,
            total: $filasLista.length
        };

    } catch (error) {
        console.error(`💥 Error actualizando grilla visible:`, error);
        return { success: false, error: error.message };
    }
}

// ✅ NUEVA: Calcular precio específico para actualizar grilla
function calcularPrecioListaParaGrilla(lp_id, datosProducto, $fila) {
    try {
        // ✅ RECOPILAR: Datos para el cálculo
        const datosCalculo = {
            p_id: datosProducto.p_id,
            lp_id: lp_id,
            tp_pcosto: datosProducto.tp_pcosto || 0,
            p_pneto_base: datosProducto.tp_pneto || 0,
            lp_porc_mg: parseFloat($fila.find('input[name="lp_porc_mg"]').val()) || 0,
            iva_situacion: $fila.find('input[name="iva_situacion"]').val() || 'E',
            iva_alicuota: parseFloat($fila.find('input[name="iva_alicuota"]').val()) || 0,
            in_alicuota: parseFloat($fila.find('input[name="in_alicuota"]').val()) || 0
        };

        // ✅ LLAMADA SÍNCRONA: Calcular precio
        const response = realizarLlamadaSincrona(calcularPrecioVentaLinkUrl, datosCalculo);

        if (!response || !response.pvta) {
            throw new Error('Respuesta inválida del servidor');
        }

        // ✅ RETORNAR: Datos calculados
        return {
            success: true,
            datos: {
                tp_pvta: parseFloat(response.pvta.p_pvta).toFixed(2),
                tp_margen: parseFloat(response.pvta.p_margen).toFixed(2),
                tp_pneto: parseFloat(response.pvta.p_pneto).toFixed(3),
                tp_iva: parseFloat(response.pvta.p_iva).toFixed(2),
                tp_in: parseFloat(response.pvta.p_in).toFixed(2)
            }
        };

    } catch (error) {
        return { success: false, error: error.message };
    }
}

// ✅ CORREGIDA: Actualizar campos visibles en la grilla de listas con marcado correcto
function actualizarCamposVisiblesEnGrilla($fila, datosCalculados) {
    try {
        // ✅ ACTUALIZAR: Campo de precio de venta visible
        const $campoPVenta = $fila.find('.input-tp_pvta_lista');
        if ($campoPVenta.length > 0) {
            const valorAnterior = $campoPVenta.val();
            $campoPVenta.val(datosCalculados.tp_pvta);

            // ✅ CRÍTICO: SIEMPRE marcar como modificado cuando es calculado automáticamente
            $campoPVenta.data('original-value', parseFloat(datosCalculados.tp_pvta));
            marcarCampoModificadoLista($campoPVenta);

            console.log(`📝 Campo PVenta lista marcado como modificado: ${valorAnterior} → ${datosCalculados.tp_pvta}`);
        }

        // ✅ ACTUALIZAR: Campo de margen visible
        const $campoMargen = $fila.find('.input-tp_margen_lista');
        if ($campoMargen.length > 0) {
            const valorAnterior = $campoMargen.val();
            $campoMargen.val(datosCalculados.tp_margen);

            // ✅ CRÍTICO: SIEMPRE marcar como modificado cuando es calculado automáticamente
            $campoMargen.data('original-value', parseFloat(datosCalculados.tp_margen));
            marcarCampoModificadoLista($campoMargen);

            console.log(`📝 Campo Margen lista marcado como modificado: ${valorAnterior} → ${datosCalculados.tp_margen}`);
        }

        // ✅ ACTUALIZAR: Campos ocultos
        $fila.find('input[name="tp_pneto"]').val(datosCalculados.tp_pneto);
        $fila.find('input[name="tp_margen"]').val(datosCalculados.tp_margen);
        $fila.find('input[name="tp_iva"]').val(datosCalculados.tp_iva);
        $fila.find('input[name="tp_in"]').val(datosCalculados.tp_in);

        // ✅ MARCAR: Fila como con cambios temporales
        $fila.data('carga', 1).attr('data-carga', '1');

        console.log(`✅ Grilla actualizada y campos marcados: PVenta=${datosCalculados.tp_pvta}, Margen=${datosCalculados.tp_margen}`);

    } catch (error) {
        console.error(`💥 Error actualizando campos visibles en grilla:`, error);
    }
}

// ✅ FUNCIÓN SÍNCRONA: Calcular costo
function calcularCostoSincrono(row) {
    const productId = row.data('p-id');

    console.log(`💰 Calculando costo SÍNCRONO para producto ${productId}`);

    // Recopilar datos
    const datos = {
        p_id: productId,
        tp_plista: parseFloat(row.find('.input-tp_plista').val().replace(/,/g, '')) || 0,
        tp_dto1: parseFloat(row.find('.input-tp_dto1').val().replace(/,/g, '')) || 0,
        tp_dto2: parseFloat(row.find('.input-tp_dto2').val().replace(/,/g, '')) || 0,
        tp_dto3: parseFloat(row.find('.input-tp_dto3').val().replace(/,/g, '')) || 0,
        tp_dto4: parseFloat(row.find('.input-tp_dto4').val().replace(/,/g, '')) || 0,
        tp_dto_pa: parseFloat(row.find('.input-tp_dto_pa').val().replace(/,/g, '')) || 0,
        tp_porc_flete: parseFloat(row.find('.input-tp_porc_flete').val().replace(/,/g, '')) || 0,
        tp_boni: row.find('.input-tp_boni').val()
    };

    // Mostrar indicador
    const campoCosto = row.find('.input-tp_pcosto');
    const valorOriginal = campoCosto.val();
    campoCosto.val('...').addClass('calculating');

    try {
        // ✅ LLAMADA SÍNCRONA
        const response = realizarLlamadaSincrona(calcularCostoUrl, datos);

        campoCosto.removeClass('calculating');

        if (response.error || response.warn) {
            campoCosto.val(valorOriginal);
            return {
                success: false,
                error: response.msg || 'Error en cálculo de costo'
            };
        }

        // Actualizar campo de costo
        const nuevoCosto = parseFloat(response.costo).toFixed(3);
        campoCosto.val(nuevoCosto);
        marcarCampoModificado(campoCosto);
        actualizarEstadoCarga(row);

        console.log(`✅ Costo calculado: ${nuevoCosto}`);

        return {
            success: true,
            costo: nuevoCosto,
            datos: datos
        };

    } catch (error) {
        campoCosto.val(valorOriginal).removeClass('calculating');
        return {
            success: false,
            error: error.message
        };
    }
}

// ✅ FUNCIÓN SÍNCRONA: Calcular precio de venta
function calcularPrecioVentaSincrono(row) {
    const productId = row.data('p-id');

    console.log(`💵 Calculando precio de venta SÍNCRONO para producto ${productId}`);

    // Recopilar datos actualizados
    const datos = {
        p_id: productId,
        tp_pcosto: parseFloat(row.find('.input-tp_pcosto').val().replace(/,/g, '')) || 0,
        tp_pneto: parseFloat(row.find('.input-tp_pneto').val().replace(/,/g, '')) || 0,
        lp_prevision_tot: parseFloat(row.find('input[name="lp_prevision_tot"]').val()) || 0,
        lp_prevision_pin: parseFloat(row.find('input[name="lp_prevision_pin"]').val()) || 0,
        tp_margen: parseFloat(row.find('.input-tp_margen').val().replace(/,/g, '')) || 0,
        iva_situacion: row.find('input[name="iva_situacion"]').val() || 'E',
        iva_alicuota: parseFloat(row.find('input[name="iva_alicuota"]').val()) || 0,
        in_alicuota: parseFloat(row.find('input[name="in_alicuota"]').val()) || 0
    };

    // Mostrar indicador
    const campoPrecioNeto = row.find('.input-tp_pneto');
    const valorOriginal = campoPrecioNeto.val();
    campoPrecioNeto.val('...').addClass('calculating');

    try {
        // ✅ LLAMADA SÍNCRONA
        const response = realizarLlamadaSincrona(calcularPrecioVentaBaseUrl, datos);

        campoPrecioNeto.removeClass('calculating');

        if (response.error || response.warn) {
            campoPrecioNeto.val(valorOriginal);
            return {
                success: false,
                error: response.msg || 'Error en cálculo de precio'
            };
        }

        // Actualizar campos de precio
        const pneto = parseFloat(response.pvta.p_pneto).toFixed(3);
        const pvta = parseFloat(response.pvta.p_pvta).toFixed(2);

        campoPrecioNeto.val(pneto);
        marcarCampoModificado(campoPrecioNeto);

        const campoPVenta = row.find('.input-tp_pvta');
        campoPVenta.val(pvta);
        marcarCampoModificado(campoPVenta);

        // Campos ocultos
        row.find('input[name="tp_iva"]').val(response.pvta.p_iva);
        row.find('input[name="tp_in"]').val(response.pvta.p_in);

        // Actualizar ratio
        actualizarRatio(row, pvta);
        actualizarEstadoCarga(row);

        console.log(`✅ Precio calculado: neto=${pneto}, venta=${pvta}`);

        return {
            success: true,
            datos: {
                ...datos,
                tp_pneto: parseFloat(pneto),
                tp_pvta: parseFloat(pvta),
                tp_iva: response.pvta.p_iva,
                tp_in: response.pvta.p_in
            }
        };

    } catch (error) {
        campoPrecioNeto.val(valorOriginal).removeClass('calculating');
        return {
            success: false,
            error: error.message
        };
    }
}

// ✅ SIMPLIFICADO: Siempre operar con el servidor
function actualizarListasSincrono(productId, datosProducto) {
    console.log(`📋 Actualizando listas SÍNCRONAS para producto ${productId} - SIEMPRE desde servidor`);

    try {
        // ✅ DIRECTO: Siempre actualizar desde servidor
        return actualizarListasServidor(productId, datosProducto);

    } catch (error) {
        console.error(`💥 Error en actualización de listas para producto ${productId}:`, error);
        return {
            success: false,
            error: error.message
        };
    }
}

// ✅ OPTIMIZADO: Función servidor más eficiente y robusta
function actualizarListasServidor(productId, datosProducto) {
    console.log(`🌐 Actualizando listas desde servidor para producto ${productId}`);

    // ✅ VALIDACIÓN TEMPRANA: Verificar datos de entrada
    if (!productId || !datosProducto) {
        console.error("❌ Parámetros inválidos para actualización de listas");
        return {
            success: false,
            error: "Parámetros de entrada inválidos"
        };
    }

    // ✅ VALIDACIÓN: Verificar que tp_pneto sea válido
    if (!datosProducto.tp_pneto || isNaN(datosProducto.tp_pneto) || datosProducto.tp_pneto <= 0) {
        console.warn(`⚠️ tp_pneto inválido (${datosProducto.tp_pneto}) para producto ${productId}, saltando listas`);
        return {
            success: true,
            skip: true,
            reason: "tp_pneto inválido"
        };
    }

    try {
        // ✅ OBTENER PARÁMETROS: Reutilizar función existente
        const datos = obtenerParametrosParaListas(productId);
        if (!datos) {
            return {
                success: false,
                error: "Error al obtener parámetros para consulta de listas"
            };
        }

        // ✅ OBTENER LISTAS: Llamada síncrona al servidor
        const responseLista = realizarLlamadaSincrona(buscarProdListaUrl, datos, 3, 'html');

        if (!responseLista || responseLista.trim() === '') {
            console.log(`ℹ️ No hay listas disponibles para producto ${productId}`);
            return {
                success: true,
                skip: true,
                reason: "Sin listas disponibles"
            };
        }

        // ✅ PROCESAR LISTAS: Extraer y procesar de forma eficiente
        return procesarListasDesdeHTML(responseLista, productId, datosProducto);

    } catch (error) {
        console.error(`💥 Error al actualizar listas para producto ${productId}:`, error);
        return {
            success: false,
            error: error.message
        };
    }
}

// ✅ NUEVA: Función específica para obtener parámetros de listas
function obtenerParametrosParaListas(productId) {
    try {
        // ✅ REUTILIZAR: Función existente pero sin efectos visuales
        const datos = obtenerParametrosSilencioso();
        if (datos === false) {
            console.error("❌ Error al obtener parámetros base");
            return null;
        }

        // ✅ CONFIGURAR: Parámetros específicos para listas
        datos.id = productId;
        datos.verificarTemp = false; // No verificar temporales en servidor
        datos.forzarRecarga = true;  // Siempre forzar recarga desde servidor

        console.log(`📋 Parámetros configurados para listas del producto ${productId}`);
        return datos;

    } catch (error) {
        console.error("💥 Error al configurar parámetros para listas:", error);
        return null;
    }
}

// ✅ NUEVA: Función optimizada para procesar HTML de listas
function procesarListasDesdeHTML(responseHTML, productId, datosProducto) {
    console.log(`🔄 Procesando listas HTML para producto ${productId}`);

    try {
        // ✅ EXTRAER: Listas del HTML de forma eficiente
        const $tempDiv = $('<div>').html(responseHTML);
        const $filasLista = $tempDiv.find('#tbProdLista tbody tr');

        if ($filasLista.length === 0) {
            console.log(`ℹ️ No se encontraron filas de listas en HTML para producto ${productId}`);
            return {
                success: true,
                skip: true,
                reason: "Sin filas de listas en HTML"
            };
        }

        console.log(`📊 Encontradas ${$filasLista.length} listas para procesar`);

        // ✅ PROCESAR: Cada lista de forma síncrona y eficiente
        let listasActualizadas = 0;
        let listasOmitidas = 0;
        let errores = 0;

        $filasLista.each(function (index) {
            const $fila = $(this);
            const lp_id = $fila.data('lp-id');
            const lp_porc_mg = parseFloat($fila.find('input[name="lp_porc_mg"]').val());

            // ✅ VALIDACIÓN: Verificar datos de lista
            if (!lp_id) {
                console.warn(`⚠️ Lista sin ID en posición ${index}, omitiendo`);
                listasOmitidas++;
                return true; // Continuar con siguiente
            }

            if (isNaN(lp_porc_mg)) {
                console.warn(`⚠️ Lista ${lp_id} sin margen válido (${lp_porc_mg}), omitiendo`);
                listasOmitidas++;
                return true; // Continuar con siguiente
            }

            try {
                // ✅ PROCESAR: Lista individual
                const resultado = procesarListaIndividualOptimizado(lp_id, datosProducto, $fila);

                if (resultado.success) {
                    listasActualizadas++;
                    console.log(`✅ Lista ${lp_id} actualizada: precio ${resultado.precio}`);
                } else {
                    errores++;
                    console.error(`❌ Error en lista ${lp_id}: ${resultado.error}`);
                }

            } catch (error) {
                errores++;
                console.error(`💥 Excepción procesando lista ${lp_id}:`, error.message);
            }
        });

        // ✅ RESULTADO: Resumen del procesamiento
        const resultado = {
            success: errores === 0,
            listasActualizadas: listasActualizadas,
            listasOmitidas: listasOmitidas,
            errores: errores,
            total: $filasLista.length
        };

        console.log(`📊 Resumen producto ${productId}: ${listasActualizadas} actualizadas, ${listasOmitidas} omitidas, ${errores} errores`);

        return resultado;

    } catch (error) {
        console.error(`💥 Error procesando HTML de listas:`, error);
        return {
            success: false,
            error: error.message
        };
    }
}

// ✅ OPTIMIZADO: Procesamiento individual más eficiente
function procesarListaIndividualOptimizado(lp_id, datosProducto, $fila) {
    // ✅ VALIDACIÓN RÁPIDA: Datos de entrada
    if (!datosProducto.p_id) {
        return {
            success: false,
            error: "Datos de producto incompletos - falta p_id"
        };
    }

    // ✅ RECOPILAR: Datos para cálculo de forma eficiente
    const datosCalculo = {
        p_id: datosProducto.p_id,
        lp_id: lp_id,
        tp_pcosto: datosProducto.tp_pcosto || 0,
        p_pneto_base: datosProducto.tp_pneto || 0,
        lp_porc_mg: parseFloat($fila.find('input[name="lp_porc_mg"]').val()) || 0,
        iva_situacion: $fila.find('input[name="iva_situacion"]').val() || 'E',
        iva_alicuota: parseFloat($fila.find('input[name="iva_alicuota"]').val()) || 0,
        in_alicuota: parseFloat($fila.find('input[name="in_alicuota"]').val()) || 0
    };

    try {
        const response = realizarLlamadaSincrona(calcularPrecioVentaLinkUrl, datosCalculo);

        const datosResguardo = construirDatosResguardo(datosProducto, lp_id, datosCalculo, response.pvta);

        // ✅ DESPUÉS: Llamada unificada síncrona
        const resguardoResult = resguardarCambiosListaUnificado(datosResguardo, {
            modo: 'sync',
            mostrarErrores: false,
            logDetallado: false
        });

        return {
            success: resguardoResult.success,
            precio: parseFloat(response.pvta.p_pvta).toFixed(2),
            margen: parseFloat(response.pvta.p_margen).toFixed(2)
        };

    } catch (error) {
        return { success: false, error: error.message };
    }
}

// ✅ NUEVA: Helper para construir datos de resguardo
function construirDatosResguardo(datosProducto, lp_id, datosCalculo, pvtaCalculado) {
    return {
        p_id: datosProducto.p_id,
        lp_id: lp_id,
        tp_margen: parseFloat(pvtaCalculado.p_margen) || 0,
        tp_pvta: parseFloat(pvtaCalculado.p_pvta) || 0,
        p_pcosto: datosProducto.tp_pcosto || 0,
        p_pneto: parseFloat(pvtaCalculado.p_pneto) || 0,
        lp_porc_mg: datosCalculo.lp_porc_mg || 0,
        iva_situacion: datosCalculo.iva_situacion || 'E',
        iva_alicuota: datosCalculo.iva_alicuota || 0,
        in_alicuota: datosCalculo.in_alicuota || 0,
        tp_iva: parseFloat(pvtaCalculado.p_iva) || 0,
        tp_in: parseFloat(pvtaCalculado.p_in) || 0
    };
}

// ✅ FUNCIÓN MEJORADA: Manejar diferentes tipos de respuesta y errores
function realizarLlamadaSincrona(url, datos, reintentos = 3, tipoRespuesta = 'json') {
    for (let intento = 1; intento <= reintentos; intento++) {
        try {
            console.log(`🔄 Llamada síncrona a ${url} (intento ${intento})`);

            let resultado = null;
            let error = null;
            let statusCode = 0;

            // ✅ AJAX SÍNCRONO con manejo de errores mejorado
            $.ajax({
                url: url,
                type: 'POST',
                data: datos,
                dataType: tipoRespuesta,
                async: false, // ✅ CRÍTICO: Hacer síncrono
                success: function (response, textStatus, xhr) {
                    resultado = response;
                    statusCode = xhr.status;
                },
                error: function (xhr, status, err) {
                    statusCode = xhr.status;
                    error = new Error(`AJAX Error: ${err} (Status: ${status}, Code: ${statusCode})`);
                }
            });

            if (error) {
                // Para ciertos errores, no reintentar
                if (statusCode === 404 || statusCode === 403) {
                    throw error;
                }
                throw error;
            }

            // ✅ VALIDACIÓN ADICIONAL: Para respuestas JSON, verificar estructura
            if (tipoRespuesta === 'json' && resultado) {
                // Si es una respuesta de error del servidor, tratarla como tal
                if (resultado.error === true && resultado.msg) {
                    throw new Error(`Server Error: ${resultado.msg}`);
                }
            }

            console.log(`✅ Llamada síncrona exitosa a ${url}`);
            return resultado;

        } catch (err) {
            console.warn(`⚠️ Intento ${intento}/${reintentos} falló: ${err.message}`);

            if (intento === reintentos) {
                console.error(`❌ Todos los intentos fallaron para ${url}`);
                throw err;
            }

            // Pausa progresiva antes del siguiente intento
            const pausaMs = intento * 100;
            const pausa = Date.now() + pausaMs;
            while (Date.now() < pausa) {
                // Pausa síncrona
            }
        }
    }
}

// ✅ MEJORADO: Logs más informativos y estructurados
function logResultadoListas(productId, resultado) {
    if (resultado.success) {
        if (resultado.skip) {
            console.log(`⏭️ Listas omitidas para producto ${productId}: ${resultado.reason}`);
        } else {
            console.log(`✅ Listas actualizadas para producto ${productId}:`, {
                actualizadas: resultado.listasActualizadas,
                omitidas: resultado.listasOmitidas || 0,
                errores: resultado.errores || 0,
                total: resultado.total || 0
            });
        }
    } else {
        console.error(`❌ Error en listas para producto ${productId}: ${resultado.error}`);
    }
}

/**
* ✅ FUNCIÓN UNIFICADA: Resguardar cambios en listas de precios
* @param {Object} datos - Datos completos para el resguardo
* @param {Object} opciones - Opciones de comportamiento
* @returns {Promise|Object} - Resultado según modo (async/sync)
*/
function resguardarCambiosListaUnificado(datos, opciones = {}) {
    // ✅ CONFIGURACIÓN POR DEFECTO
    const config = {
        modo: 'sync',           // 'async' | 'sync' | 'silent'
        mostrarErrores: true,    // Mostrar mensajes de error
        callback: null,          // Función callback para async
        logDetallado: false,     // Logging detallado
        ...opciones
    };

    // ✅ VALIDACIÓN DE DATOS ESENCIALES
    if (!datos.p_id || !datos.lp_id) {
        const error = new Error('Datos incompletos: p_id y lp_id son requeridos');
        return manejarErrorResguardo(error, config);
    }

    // ✅ NORMALIZAR DATOS
    const datosNormalizados = normalizarDatosResguardo(datos);

    if (config.logDetallado) {
        console.log(`💾 Resguardando lista ${datos.lp_id} para producto ${datos.p_id} (${config.modo})`);
    }

    // ✅ EJECUTAR SEGÚN MODO
    switch (config.modo) {
        case 'sync':
            return ejecutarResguardoSincrono(datosNormalizados, config);
        case 'silent':
            return ejecutarResguardoSilencioso(datosNormalizados, config);
        default:
            return ejecutarResguardoAsincrono(datosNormalizados, config);
    }
}

/**
 * ✅ NORMALIZAR: Datos de entrada consistentes
 */
function normalizarDatosResguardo(datos) {
    return {
        p_id: datos.p_id,
        lp_id: datos.lp_id,
        tp_margen: parseFloat(datos.tp_margen) || 0,
        tp_pvta: parseFloat(datos.tp_pvta) || 0,
        p_pcosto: parseFloat(datos.p_pcosto) || 0,
        p_pneto: parseFloat(datos.p_pneto) || 0,
        lp_porc_mg: parseFloat(datos.lp_porc_mg) || 0,
        iva_situacion: datos.iva_situacion || 'E',
        iva_alicuota: parseFloat(datos.iva_alicuota) || 0,
        in_alicuota: parseFloat(datos.in_alicuota) || 0,
        tp_iva: parseFloat(datos.tp_iva) || 0,
        tp_in: parseFloat(datos.tp_in) || 0
    };
}

/**
 * ✅ MODO ASÍNCRONO: Para edición manual de campos
 */
function ejecutarResguardoAsincrono(datos, config) {
    return $.ajax({
        url: resguardarCambiosProductoListaUrl,
        type: 'POST',
        data: datos,
        dataType: 'json'
    }).done(function (response) {
        manejarRespuestaResguardo(response, datos, config);
        if (config.callback) config.callback(response, true);
    }).fail(function (xhr, status, error) {
        const errorObj = new Error(`Error AJAX: ${error} (${status})`);
        manejarErrorResguardo(errorObj, config);
        if (config.callback) config.callback(null, false, errorObj);
    });
}

/**
 * ✅ MODO SÍNCRONO: Para procesamiento masivo
 */
function ejecutarResguardoSincrono(datos, config) {
    try {
        const response = realizarLlamadaSincrona(resguardarCambiosProductoListaUrl, datos);

        if (response && response.error) {
            throw new Error(response.msg || 'Error del servidor');
        }

        if (config.logDetallado) {
            console.log(`✅ Lista ${datos.lp_id} resguardada síncronamente`);
        }

        return {
            success: true,
            data: response,
            datos: datos
        };

    } catch (error) {
        return manejarErrorResguardo(error, config);
    }
}

/**
 * ✅ MODO SILENCIOSO: Sin logs ni mensajes de error
 */
function ejecutarResguardoSilencioso(datos, config) {
    try {
        $.ajax({
            url: resguardarCambiosProductoListaUrl,
            type: 'POST',
            data: datos,
            dataType: 'json',
            async: true,
            success: function (response) {
                // Solo callback si hay error crítico
                if (response && response.error && config.callback) {
                    config.callback(response, false);
                }
            },
            error: function () {
                // Modo silencioso: no hacer nada con errores
                if (config.callback) {
                    config.callback(null, false);
                }
            }
        });

        return { success: true, mode: 'silent' };

    } catch (error) {
        return { success: false, error: error.message, mode: 'silent' };
    }
}

/**
 * ✅ MANEJAR: Respuestas exitosas
 */
function manejarRespuestaResguardo(response, datos, config) {
    if (response.error && config.mostrarErrores) {
        AbrirMensaje("Error",
            `No se pudieron guardar los cambios: ${response.msg}`,
            () => $("#msjModal").modal("hide"),
            false, ["Aceptar"], "error!", null);
    } else if (response.warn && config.mostrarErrores) {
        console.warn(`⚠️ Advertencia al resguardar lista ${datos.lp_id}: ${response.msg}`);
    } else if (config.logDetallado) {
        console.log(`✅ Lista ${datos.lp_id} resguardada correctamente`);
    }
}

/**
 * ✅ MANEJAR: Errores de forma consistente
 */
function manejarErrorResguardo(error, config) {
    if (config.mostrarErrores && config.modo !== 'silent') {
        console.error('❌ Error al resguardar lista:', error.message);

        if (config.modo === 'async') {
            AbrirMensaje("Error",
                "Error de comunicación al resguardar cambios. Inténtelo nuevamente.",
                () => $("#msjModal").modal("hide"),
                false, ["Aceptar"], "error!", null);
        }
    }

    return {
        success: false,
        error: error.message,
        mode: config.modo
    };
}

// ✅ SIMPLIFICAR: Función de construcción de datos
function construirDatosResguardoLista(datosBase, lpId, datosAdicionales = {}) {
    return {
        p_id: datosBase.p_id || productoActualEnLista,
        lp_id: lpId,
        tp_margen: datosBase.tp_margen || 0,
        tp_pvta: datosBase.tp_pvta || 0,
        p_pcosto: datosBase.p_pcosto || 0,
        p_pneto: datosBase.p_pneto || 0,
        lp_porc_mg: datosBase.lp_porc_mg || 0,
        iva_situacion: datosBase.iva_situacion || 'E',
        iva_alicuota: datosBase.iva_alicuota || 0,
        in_alicuota: datosBase.in_alicuota || 0,
        tp_iva: datosBase.tp_iva || 0,
        tp_in: datosBase.tp_in || 0,
        ...datosAdicionales
    };
}

// ✅ HELPER: Extraer datos desde fila DOM
function extraerDatosDesdeFilaLista(row) {
    return {
        p_pcosto: parseFloat(row.find('input[name="p_pcosto"]').val()) || 0,
        p_pneto: parseFloat(row.find('input[name="tp_pneto"]').val()) || 0,
        lp_porc_mg: parseFloat(row.find('input[name="lp_porc_mg"]').val()) || 0,
        iva_situacion: row.find('input[name="iva_situacion"]').val() || 'E',
        iva_alicuota: parseFloat(row.find('input[name="iva_alicuota"]').val()) || 0,
        in_alicuota: parseFloat(row.find('input[name="in_alicuota"]').val()) || 0,
        tp_iva: parseFloat(row.find('input[name="tp_iva"]').val()) || 0,
        tp_in: parseFloat(row.find('input[name="tp_in"]').val()) || 0
    };
}