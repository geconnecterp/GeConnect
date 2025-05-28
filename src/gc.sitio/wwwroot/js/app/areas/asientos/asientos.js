// Al cargar, establecer variables para control de selección
let filaClicDoble = null; // Fila seleccionada con doble clic (detalle)
let filasSeleccionadas = []; // Filas seleccionadas para operaciones batch

// Función para actualizar el mensaje informativo
function actualizarMensajeSeleccion() {
    if ($("#seleccion-info").length === 0) {
        const mensaje = `
            <div id="seleccion-info" class="alert alert-info mb-2 py-1 small">
                <i class="bx bx-info-circle"></i> 
                Haga clic en un asiento para seleccionarlo. Use Ctrl+Clic para selección múltiple.
                <span class="badge bg-primary ms-1" id="selected-count">0</span> asiento(s) seleccionado(s).
            </div>
        `;
        $("#tbGridAsiento").after(mensaje);
    }
}

// Llamar a esta función después de cargar la grilla
function inicializarSeleccionAsientos() {
    // Limpiar variables
    filaClicDoble = null;
    filasSeleccionadas = [];

    // Agregar mensaje de selección
    actualizarMensajeSeleccion();

    // Deshabilitar botón inicialmente
    $("#btnPasarConta").prop("disabled", true);

    // Actualizar contador
    $("#selected-count").text("0");
}

// Guardar la referencia original de la función
const buscarAsientosOriginal = buscarAsientos;

// Redefinir la función para incluir la inicialización de selección
function buscarAsientos(pag) {
    AbrirWaiting();
    //desactivamos los botones de acción
    activarBotones2(false);

    // Obtenemos los valores de los campos del filtro
    var data1 = {
        Eje_nro: $("#Eje_nro").val(), // Ejercicio
        Movi: $("#Movi").is(":checked"), // bit Movimiento
        Movi_like: $("#Movi_like").val(), // Nro Movimiento
        Usu: $("#Usu").is(":checked"), // bit Usuario
        Usu_like: $("#Usu_like").val(), // Usuario
        Tipo: $("#Tipo").is(":checked"), // bit Tipo
        Tipo_like: $("#Tipo_like").val(), // Tipo de Asiento
        Rango: $("#Rango").is(":checked"), // bit Rango
        Desde: $("input[name='Desde']").val(), // Fecha Desde
        Hasta: $("input[name='Hasta']").val() // Fecha Hasta
    };

    var buscaNew = JSON.stringify(dataBak) != JSON.stringify(data1)

    if (buscaNew === false) {
        //son iguales las condiciones cambia de pagina
        pagina = pag;
    }
    else {
        dataBak = data1;
        pagina = 1;
        pag = 1;
    }

    var sort = null;
    var sortDir = null

    var data2 = { sort, sortDir, pag, buscaNew }

    var data = $.extend({}, data1, data2);

    PostGenHtml(data, buscarAsientosUrl, function (obj) {
        $("#divGrilla").html(obj);
        $("#divFiltro").collapse("hide")

        // Agregar un pequeño retraso para asegurar que la grilla ya está cargada
        setTimeout(inicializarSeleccionAsientos, 100);

        PostGen({}, buscarMetadataURL, function (obj) {
            if (obj.error === true) {
                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "error!", null);
            }
            else {
                totalRegs = obj.metadata.totalCount;
                pags = obj.metadata.totalPages;
                pagRegs = obj.metadata.pageSize;

                $("#pagEstado").val(true).trigger("change");
            }

        });
        CerrarWaiting();
    }, function (obj) {
        ControlaMensajeError(obj.message);
        CerrarWaiting();
    });
}

$(function () {
    // Agregar estilos CSS para diferencias visuales claras
    $("<style>")
        .prop("type", "text/css")
        .html(`
        /* Estilo para selección simple (un clic) */
        #tbGridAsiento tbody tr.selected-simple {
            background-color: lightgray !important;
            color: white !important;
        }
        
        /* Estilo para selección múltiple (Ctrl+clic) */
        #tbGridAsiento tbody tr.selected-multiple {
            background-color: #e7f3ff !important;
            box-shadow: inset 0 0 0 1px #0d6efd !important;
        }
        
        /* Estilo para fila con detalle abierto (doble clic) */
        #tbGridAsiento tbody tr.selectedEdit-row {
            background-color: firebrick !important;
            color: white !important;
        }
        
        /* Asegurar que todas las filas tengan cursor de puntero */
        #tbGridAsiento tbody tr {
            cursor: pointer;
        }

         /* Estilos para los checkboxes */
        .checkbox-column {
            width: 40px;
            text-align: center;
        }
        
        .form-check-input {
            cursor: pointer;
        }
        
        /* Estilo para checkboxes deshabilitados */
        .form-check-input:disabled {
            opacity: 0.6;
            cursor: not-allowed;
        }
        
        /* Cambiar el color del fondo de las filas con checkboxes marcados */
        #tbGridAsiento tbody tr:has(.asiento-checkbox:checked) {
            background-color: #e7f3ff !important;
        }
    `)
        .appendTo("head");


    // Eliminar todos los manejadores previos para evitar duplicación
    $(document).off("click", "#tbGridAsiento tbody tr");
    $(document).off("dblclick", "#tbGridAsiento tbody tr");




    // Modificar la función buscarAsientos para inicializar selección
    buscarAsientos = function (pag) {
        buscarAsientosOriginal(pag);
        // Agregar un pequeño retraso para asegurar que la grilla ya está cargada
        setTimeout(inicializarSeleccionAsientos, 100);
    };

    $("#btnBuscar").on("click", function () {

        //es nueva la busqueda no resguardamos la busqueda anterior. es util para paginado
        $("#divpanel01").empty();
        dataBak = "";
        //es una busqueda por filtro. siempre sera pagina 1
        pagina = 1;
        buscarAsientos(pagina);
    });
    //callback para que funcione la paginación
    funcCallBack = buscarAsientos;
    $("#pagEstado").on("change", function () {
        var div = $("#divPaginacion");
        presentaPaginacion(div);
    });
    $("#btnCancel").on("click", function () {
        window.location.href = homeAsientosUrl;
    });

    //**** EVENTOS PARA CONTROLAR LOS BOTONES DE OPERACION
    $("#btnAbmNuevo").on("click", ejecutarAltaAsientoTemp);
    $("#btnAbmModif").on("click", ejecutarModificacionAsientoTemp);
    $("#btnAbmElimi").on("click", ejecutarBajaAsientoTemp);

    $("#btnAbmCancelar").on("click", InicializaVistaAsientos);
    $("#btnAbmAceptar").on("click", confirmarOperacionAsiento);
    //****FIN BOTONES DE OPERACION*/

    // Evento para el botón Imprimir
    $(document).on("click", "#btnImprimir", imprimirAsiento);
    // Usar mousedown en lugar de click para evitar conflictos con el collapse
    $("#btnDetalle").on("mousedown", analizaEstadoBtnDetalle);

    // Evento para el botón "Pasar a contabilidad"
    $(document).on("click", "#btnPasarConta", function () {
        // Confirmar la acción
        AbrirMensaje(
            "CONFIRMACIÓN",
            `¿Está seguro que desea pasar ${filasSeleccionadas.length} asiento(s) temporal(es) seleccionado(s) a la contabilidad definitiva?`,
            function (resp) {
                if (resp === "SI") {
                    ejecutarPaseAContabilidad();
                }
                $("#msjModal").modal("hide");
                return true;
            },
            true, // Mostrar botones SI/NO
            ["SI", "NO"],
            "warn!",
            null
        );
    });

    // Eventos para activar/desactivar componentes
    $('#chkEjercicio').on('change', function () {
        if (typeof asientoTemporal !== 'undefined' && asientoTemporal === true) {
            // Si está en modo temporal, no ejecutar y desactivar el checkbox
            //$('#chkEjercicio').prop('checked', false).prop('disabled', true);
            return;
        }
        toggleComponent('chkEjercicio', '#Eje_nro');
    });

    $('#Movi').on('change', function () {
        toggleComponent('Movi', '#Movi_like');
    });

    $('#Usu').on('change', function () {
        toggleComponent('Usu', '#Usu_like');
    });

    $('#Tipo').on('change', function () {
        toggleComponent('Tipo', '#Tipo_like');
    });

    $('#Rango').on('change', function () {
        toggleComponent('Rango', 'input[name="Desde"]');
        toggleComponent('Rango', 'input[name="Hasta"]');
    });

    // Agregar manejo de clic simple en filas para selección múltiple
    // Modificar el manejador del clic para mostrar checkboxes cuando se usa Ctrl+click
    // Agregar manejo de clic simple en filas para selección múltiple - SOLUCIÓN CORREGIDA
$(document).on("click", "#tbGridAsiento tbody tr", function (e) {
    // Si se hace clic en un checkbox o en su contenedor, no hacer nada aquí
    if ($(e.target).is('.asiento-checkbox') || $(e.target).closest('.form-check').length > 0) {
        return;
    }
    
    const $this = $(this);
    const id = $this.find("td:nth-child(2)").text().trim(); // Ajustado por la nueva columna
    const esRevisable = $this.attr("data-revisable") === "true";

    // Si es doble clic, no hacer nada (el evento dblclick lo manejará)
    if (e.detail === 2) return;

    // Prevenir la propagación del evento
    e.stopPropagation();

    // Limpiar estilo de selección de doble clic (firebrick) de todas las filas
    $("#tbGridAsiento tbody tr").removeClass("selectedEdit-row");

    // Selección simple vs. múltiple
    if (e.ctrlKey || e.metaKey) {
        // Selección múltiple (Ctrl+clic)
        if ($this.hasClass("selected-multiple")) {
            // Si ya estaba en selección múltiple, quitar
            $this.removeClass("selected-multiple");

            // Desmarcar el checkbox correspondiente
            $(`#check_${id}`).prop("checked", false);

            // Eliminar de la lista de seleccionados
            filasSeleccionadas = filasSeleccionadas.filter(item => item !== id);
        } else {
            // Si no estaba seleccionada y no es revisable, seleccionar como múltiple
            if (!esRevisable) {
                $this.addClass("selected-multiple");

                // Marcar el checkbox correspondiente
                $(`#check_${id}`).prop("checked", true);

                // Agregar a la lista si no existe
                if (!filasSeleccionadas.includes(id)) {
                    filasSeleccionadas.push(id);
                }
            }
        }
    } else {
        // Selección simple (clic)
        // Quitar selección múltiple de todas las filas
        $("#tbGridAsiento tbody tr").removeClass("selected-multiple");

        // Desmarcar todos los checkboxes
        $(".asiento-checkbox").prop("checked", false);

        filasSeleccionadas = [];

        // Si no es revisable, seleccionar
        if (!esRevisable) {
            // Aplicar selección simple solo a esta fila
            $this.addClass("selected-simple");
            $("#tbGridAsiento tbody tr").not($this).removeClass("selected-simple");

            // Marcar solo el checkbox correspondiente
            $(`#check_${id}`).prop("checked", true);

            // Agregar a la lista de seleccionados
            filasSeleccionadas = [id];
        }
    }

    // Actualizar estado del botón
    const haySeleccionados = filasSeleccionadas.length > 0;
    $("#btnPasarConta").prop("disabled", !haySeleccionados);

    // Actualizar mensaje informativo
    $("#selected-count").text(filasSeleccionadas.length);
});


    // Evento para el checkbox "selectAll"
    $(document).on("change", "#selectAllAsientos", function () {
        const isChecked = $(this).prop("checked");

        // Limpiar todas las selecciones existentes
        $("#tbGridAsiento tbody tr").removeClass("selected-multiple selected-simple");
        filasSeleccionadas = [];

        // Marcar/desmarcar todos los checkboxes (excepto los deshabilitados)
        $(".asiento-checkbox:not(:disabled)").prop("checked", isChecked);

        if (isChecked) {
            // Recopilar todos los IDs seleccionados de los checkboxes marcados
            $(".asiento-checkbox:checked").each(function () {
                const id = $(this).val();
                if (id && !filasSeleccionadas.includes(id)) {
                    filasSeleccionadas.push(id);
                }
            });

            // Seleccionar todas las filas correspondientes
            $("#tbGridAsiento tbody tr").each(function () {
                const $row = $(this);
                const id = $row.find("td:nth-child(2)").text().trim();

                if (filasSeleccionadas.includes(id)) {
                    $row.addClass("selected-multiple");
                }
            });
        }

        // Actualizar estado del botón
        const haySeleccionados = filasSeleccionadas.length > 0;
        $("#btnPasarConta").prop("disabled", !haySeleccionados);

        // Actualizar contador
        $("#selected-count").text(filasSeleccionadas.length);

        console.log("IDs seleccionados:", filasSeleccionadas); // Debug para verificar
    });

    // Evento para los checkboxes individuales - SOLUCIÓN
    $(document).on("click", ".asiento-checkbox", function (e) {
        e.stopPropagation(); // Evitar que se propague al tr
        //e.preventDefault(); // Evitar el comportamiento predeterminado del checkbox

        const $checkbox = $(this);
        const id = $checkbox.val();
        const isChecked = $checkbox.prop("checked"); // Obtenemos el estado DESPUÉS del clic
        const $row = $checkbox.closest("tr");

        if (isChecked) {
            // Seleccionar la fila
            $row.addClass("selected-multiple").removeClass("selected-simple");

            // Agregar a la lista de seleccionados
            if (!filasSeleccionadas.includes(id)) {
                filasSeleccionadas.push(id);
            }
        } else {
            // Deseleccionar la fila
            $row.removeClass("selected-multiple selected-simple");

            // Quitar de la lista de seleccionados
            filasSeleccionadas = filasSeleccionadas.filter(item => item !== id);

            // Desmarcar el checkbox "selectAll" si alguno se desmarca
            $("#selectAllAsientos").prop("checked", false);
        }

        // Verificar si todos los checkboxes no deshabilitados están marcados
        const todosSeleccionados = $(".asiento-checkbox:not(:disabled)").length ===
            $(".asiento-checkbox:not(:disabled):checked").length;
        $("#selectAllAsientos").prop("checked", todosSeleccionados);

        // Actualizar estado del botón
        const haySeleccionados = filasSeleccionadas.length > 0;
        $("#btnPasarConta").prop("disabled", !haySeleccionados);

        // Actualizar contador
        $("#selected-count").text(filasSeleccionadas.length);
    });

    // Manejo de doble clic para ver detalle
    $(document).on("dblclick", "#tbGridAsiento tbody tr", function (e) {
        e.stopPropagation();
        const $this = $(this);

        // Limpiar selección simple/múltiple (visual) para evitar confusión
        $("#tbGridAsiento tbody tr").removeClass("selected-simple selected-multiple");

        // Aplicar estilo de fila seleccionada para detalle
        $("#tbGridAsiento tbody tr").removeClass("selectedEdit-row");
        $this.addClass("selectedEdit-row");
        filaClicDoble = $this;

        // Llamar a la función existente de detalle
        ejecutaDblClickGrid1($this);
    });

    InicializaVistaAsientos();
})

//***FUNCIONES DE OPERACION***/
function ejecutarAltaAsientoTemp() {
    AbrirWaiting("Espere, se blanquea el ASIENTO...");
    accion = AbmAction.ALTA;
    var data = {};
    PostGenHtml(data, nuevoAsientoUrl, function (obj) {
        $("#divpanel01").html(obj);

        $("#btnDetalle").prop("disabled", false);
        $("#divFiltro").collapse("hide");
        $("#divDetalle").collapse("show");

        accionBotones(accion);
        activarControles(true);

        CerrarWaiting();
    });
}

function ejecutarModificacionAsientoTemp() {
    accion = AbmAction.MODIFICACION;
    $("#divFiltro").collapse("hide");
    accionBotones(accion);
    activarControles(true);
}

function ejecutarBajaAsientoTemp() {
    accion = AbmAction.BAJA;
    $("#divFiltro").collapse("hide");
    accionBotones(accion);
}


/**
 * Ejecuta el pase del asiento temporal a contabilidad definitiva
 */
function ejecutarPaseAContabilidad() {
    // Verificar si hay asientos seleccionados
    if (filasSeleccionadas.length === 0) {
        AbrirMensaje("ATENCIÓN", "Debe seleccionar al menos un asiento para pasar a contabilidad.", function () {
            $("#msjModal").modal("hide");
            return true;
        }, false, ["Aceptar"], "warn!", null);
        return;
    }

    // Obtener el número de ejercicio seleccionado
    const eje_nro = parseInt($("#Eje_nro").val()) || 0;

    // Validar que se haya seleccionado un ejercicio
    if (eje_nro <= 0) {
        AbrirMensaje("ATENCIÓN", "Debe seleccionar un ejercicio contable válido para realizar el pase a contabilidad.", function () {
            $("#msjModal").modal("hide");
            return true;
        }, false, ["Aceptar"], "warn!", null);
        return;
    }

    // Mostrar indicador de espera
    AbrirWaiting("Procesando el pase a contabilidad...");

    // Construimos el objeto que coincide con AsientoAccionDto
    const datos = {
        asientosIds: filasSeleccionadas,  // El objeto del asiento
        eje_nro: eje_nro         // La acción (por ejemplo, 'A', 'M', 'B')
    };

    // Llamar al servicio con los IDs seleccionados
    $.ajax({
        url: pasarAContabilidadUrl,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(datos),
        success: function (obj) {
            CerrarWaiting();

            if (obj.error === true) {
                // Caso 1: Algunos asientos procesados correctamente, otros con error
                if (obj.parcial === true) {
                    let mensaje = obj.msg;

                    // Mostrar mensaje principal
                    AbrirMensaje("ATENCIÓN", mensaje, function () {
                        // Si hay pocos errores, mostrar detalles en un segundo mensaje
                        if (obj.fallidos <= 3) {
                            let detalles = "<ul>";
                            obj.detalles.forEach(function (detalle) {
                                detalles += `<li>Asiento ${detalle.asientoId}: ${detalle.mensaje}</li>`;
                            });
                            detalles += "</ul>";

                            AbrirMensaje("DETALLES DE ERRORES", detalles, function () {
                                $("#msjModal").modal("hide");
                                // Refrescar la lista de asientos
                                $("#divpanel01").empty();
                                buscarAsientos(1);
                                return true;
                            }, false, ["Aceptar"], "error!", null);
                        }
                        // Si hay muchos errores, ofrecer generar un reporte
                        else {
                            // Por:
                            AbrirMensaje("CONFIRMACIÓN", "¿Desea ver el detalle de los errores?", function (resp) {
                                if (resp === "SI") {
                                    presentarReporteErrores(obj.detalles);
                                }
                                $("#msjModal").modal("hide");
                            }, true, ["SI", "NO"], "warn!", null);

                            // Refrescar la lista de asientos
                            $("#divpanel01").empty();
                            buscarAsientos(1);
                        }
                        $("#msjModal").modal("hide");
                        return true;
                    }, false, ["Aceptar"], "warn!", null);
                }
                // Caso 2: Muchos errores (todos los asientos fallaron)
                else if (obj.muchos === true) {
                    let mensaje = obj.msg + "\n¿Desea ver el detalle de los errores?";

                    AbrirMensaje("ERROR", mensaje, function (resp) {
                        if (resp === "SI") {
                            //generarReporteErrores(obj.detalles);
                            presentarReporteErrores(obj.detalles);
                        }
                        $("#msjModal").modal("hide");
                        // Refrescar la lista de asientos
                        $("#divpanel01").empty();
                        buscarAsientos(1);
                        return true;
                    }, true, ["SI", "NO"], "error!", null);
                }
                // Caso 3: Error general o pocos errores
                else {
                    AbrirMensaje("ERROR", obj.msg || "Ocurrió un error al pasar los asientos a contabilidad.", function () {
                        $("#msjModal").modal("hide");
                        return true;
                    }, false, ["Aceptar"], "error!", null);
                }
            } else {
                // Caso 4: Operación completamente exitosa
                AbrirMensaje("ÉXITO", obj.msg || "Los asientos han sido pasados a contabilidad correctamente.", function () {
                    $("#msjModal").modal("hide");
                    // Refrescar la lista de asientos después de un pase exitoso
                    $("#divpanel01").empty();
                    buscarAsientos(1);
                    return true;
                }, false, ["Aceptar"], "success", null);
            }
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            AbrirMensaje("ERROR", "Error al pasar los asientos a contabilidad: " + error, function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
    });
}

/**
 * Función para presentar un reporte con los errores en html
 * Esta función es un placeholder - necesitarás implementarla según tus necesidades
 */
function presentarReporteErrores(detalles) {
    if (!detalles || detalles.length === 0) {
        AbrirMensaje("ATENCIÓN", "No hay errores para mostrar.", function () {
            $("#msjModal").modal("hide");
        }, false, ["Aceptar"], "warn!", null);
        return;
    }

    // Para una implementación inicial, podemos abrir una ventana con los detalles
    let contenido = "<h3>Detalle de Errores</h3><table class='table table-striped'>";
    contenido += "<thead><tr><th>Asiento</th><th>Mensaje de Error</th></tr></thead><tbody>";

    detalles.forEach(function (detalle) {
        contenido += `<tr><td>${detalle.asientoId}</td><td>${detalle.mensaje}</td></tr>`;
    });

    contenido += "</tbody></table>";

    // Crear una ventana emergente con el contenido HTML
    const ventana = window.open("", "_blank", "width=800,height=600");
    ventana.document.write(`
        <!DOCTYPE html>
        <html>
        <head>
            <title>Reporte de Errores</title>
            <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css">
            <style>
                body { padding: 20px; }
                h3 { margin-bottom: 20px; }
                .table { width: 100%; }
            </style>
        </head>
        <body>
            <div class="container">
                ${contenido}
                <div class="mt-4">
                    <button class="btn btn-primary" onclick="window.print()">Imprimir</button>
                    <button class="btn btn-secondary" onclick="window.close()">Cerrar</button>
                </div>
            </div>
        </body>
        </html>
    `);

    // En una implementación más avanzada, podrías llamar a un endpoint del controlador
    // que genere un PDF y lo descargue o abra en una nueva pestaña
    // window.open('/Asientos/AsientoTemporal/GenerarReporteErrores?ids=' + JSON.stringify(detalles), '_blank');
}

/**
 * Genera un informe PDF con los detalles de errores al enviar asientos a contabilidad
 * @param {Array} detalles - Lista de objetos con los detalles de los errores
 */
function generarReporteErrores(detalles) {
    if (!detalles || detalles.length === 0) {
        AbrirMensaje("ATENCIÓN", "No hay errores para generar el informe.", function () {
            $("#msjModal").modal("hide");
        }, false, ["Aceptar"], "warn!", null);
        return;
    }

    // Mostrar indicador de espera
    AbrirWaiting("Generando informe de errores...");

    // Convertir los detalles a JSON para pasarlo como parámetro
    const erroresJson = JSON.stringify(detalles);

    // Preparar el objeto de solicitud para el informe
    const reporteSolicitud = {
        Reporte: 9, // InfoReporte.R009_InfoErrAsTemp
        Parametros: {
            "errores": erroresJson,
            "fecha": new Date().toISOString()
        },
        Titulo: "Informe de Errores en Asientos Temporales",
        Observacion: "Detalle de errores al pasar asientos temporales a contabilidad",        
        Formato: "P" // P = PDF
    };

    // Realizar la solicitud al servidor
    $.ajax({
        url: gestorImpresionUrl,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(reporteSolicitud),
        success: function (response) {
            CerrarWaiting();

            if (response.error) {
                AbrirMensaje("ERROR", response.msg || "Error al generar el informe", function () {
                    $("#msjModal").modal("hide");
                }, false, ["Aceptar"], "error!", null);
                return;
            }

            if (response.warn) {
                AbrirMensaje("ADVERTENCIA", response.msg, function () {
                    if (response.auth) {
                        window.location.href = loginUrl; // Redirigir al login si es necesario
                    }
                    $("#msjModal").modal("hide");
                }, false, ["Aceptar"], "warn!", null);
                return;
            }

            // Si todo salió bien, mostrar el PDF
            const pdfData = response.base64;
            const fileName = response.name || "errores_asientos.pdf";

            // Crear un objeto Blob con los datos del PDF
            const blob = b64toBlob(pdfData, "application/pdf");

            // Crear una URL para el Blob
            const url = URL.createObjectURL(blob);

            // Abrir el PDF en una nueva ventana/pestaña
            window.open(url, "_blank");
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            AbrirMensaje("ERROR", "Error al generar el informe: " + error, function () {
                $("#msjModal").modal("hide");
            }, false, ["Aceptar"], "error!", null);
        }
    });
}

/**
 * Convierte un string Base64 a un objeto Blob
 * @param {string} b64Data - Datos en formato Base64
 * @param {string} contentType - Tipo de contenido MIME
 * @param {number} sliceSize - Tamaño de las porciones de datos (opcional)
 * @returns {Blob} - Objeto Blob con los datos
 */
function b64toBlob(b64Data, contentType, sliceSize) {
    contentType = contentType || "";
    sliceSize = sliceSize || 512;

    const byteCharacters = atob(b64Data);
    const byteArrays = [];

    for (let offset = 0; offset < byteCharacters.length; offset += sliceSize) {
        const slice = byteCharacters.slice(offset, offset + sliceSize);

        const byteNumbers = new Array(slice.length);
        for (let i = 0; i < slice.length; i++) {
            byteNumbers[i] = slice.charCodeAt(i);
        }

        const byteArray = new Uint8Array(byteNumbers);
        byteArrays.push(byteArray);
    }

    return new Blob(byteArrays, { type: contentType });
}




/**
 * Activa o desactiva los controles del asiento según el modo (alta/modificación/baja)
 * @param {boolean} act - Si es true activa los controles, si es false los desactiva
 */
function activarControles(act) {
    // Inicializar selector de cuentas si se están activando los controles
    if (act && typeof selectorCuentas !== 'undefined') {
        selectorCuentas.inicializar(buscarPlanCuentasUrl, buscarCuentaUrl);
    }

    // Se invierte el valor porque 'disabled=true' significa desactivar
    // y queremos que act=true signifique habilitar controles
    const disabled = !act;

    // Header del asiento
    $("#Dia_tipo").prop("disabled", disabled);
    // El campo dia_movi (Nº Mov) siempre debe estar en modo solo lectura
    // ya que es la PK generada por la base de datos
    $(".card-header input.form-control:not(:first)").prop("readonly", disabled);

    // Añadir/eliminar líneas y botones operativos
    if (act) {
        // Si estamos activando los controles, añadimos botones de edición
        if ($("#btnAddLinea").length === 0) {
            // Si es nuevo o modificación, permitir editar líneas del asiento
            $("#tbAsientoDetalle thead tr").append(`<th style="width: 5%">Acciones</th>`);

            // Añadir un botón + en la primera celda de la última columna del thead
            $("#tbAsientoDetalle thead th:last-child").html(`
                <button id="btnAddLinea" class="btn btn-sm btn-success float-end" title="Nueva Linea">
                    <i class="bx bx-plus"></i>
                </button>
            `);

            $("#tbAsientoDetalle tbody tr").append(`
                <td class="text-center">
                    <button class="btn btn-sm btn-danger btn-eliminar-linea" title="Eliminar Linea">
                        <i class="bx bx-trash"></i>
                    </button>
                </td>
            `);

            // Mostrar botones de búsqueda de cuenta
            $("#tbAsientoDetalle tbody .btn-buscar-cuenta").show();


            // Agrega funcionalidad a los nuevos botones
            configurarEventosEdicion();
        }

        // Habilitar edición de datos de líneas
        $("#tbAsientoDetalle tbody").addClass("editable-grid");
        $("#tbAsientoDetalle tbody td:not(:last-child)").attr("contenteditable", true);

        // Configurar máscaras y validaciones para campos editables
        configurarCamposEditables();
    } else {
        // Si desactivamos, quitamos los controles de edición
        $("#tbAsientoDetalle thead th:last-child").remove();
        $("#tbAsientoDetalle tbody tr td:last-child").remove();
        $("#btnAddLinea").remove();

        // Deshabilitar edición
        $("#tbAsientoDetalle tbody").removeClass("editable-grid");
        $("#tbAsientoDetalle tbody td").removeAttr("contenteditable");

        // Ocultar botones de búsqueda de cuenta
        $("#tbAsientoDetalle tbody .btn-buscar-cuenta").hide();
    }

    // Control de botones de acción del footer
    $("#btnPasarContabilidad, #btnImprimir").prop("disabled", act); // Se deshabilitan al editar
}

/**
 * Configura los eventos para los botones de edición
 */
function configurarEventosEdicion() {
    // Evento para agregar nueva línea
    $(document).on("click", "#btnAddLinea", function () {
        const nuevaLinea = `
                <tr class="">
                    <td contenteditable="true">
                        <div class="d-flex align-items-center">
                            <span class="cuenta-id"></span>
                            <button type="button" class="btn btn-sm btn-link text-primary btn-buscar-cuenta ms-1">
                                <i class="bx bx-search"></i>
                            </button>
                        </div>
                    </td>
                    <td class="cuenta-desc" contenteditable="false"></td>
                    <td class="linea-concepto" contenteditable="true"></td>
                    <td contenteditable="true" class="text-end">0,00</td>
                    <td contenteditable="true" class="text-end">0,00</td>
                    <td class="text-center">
                        <button class="btn btn-sm btn-danger btn-eliminar-linea">
                            <i class="bx bx-trash"></i>
                        </button>
                    </td>
                </tr>
            `;
        $("#tbAsientoDetalle tbody").append(nuevaLinea);
        actualizarTotales();
    });

    // Evento para eliminar línea
    $(document).on("click", ".btn-eliminar-linea", function () {
        $(this).closest("tr").remove();
        actualizarTotales();
    });
}

/**
 * Configura los campos editables con máscaras y validaciones
 */
function configurarCamposEditables() {
    // Campos de importe (Debe y Haber)
    $("#tbAsientoDetalle tbody").on("focus", "td.text-end", function () {
        // Al entrar al campo, quitar formato pero mantener el punto decimal
        const valorOriginal = $(this).text().trim();

        try {
            // Quitar símbolo de moneda, espacios y comas (separadores de miles)
            let valorLimpio = valorOriginal.replace(/[$\s,]/g, "");

            // Si está vacío, poner 0
            if (!valorLimpio || valorLimpio === "") {
                valorLimpio = "0";
            }

            // Aplicar el valor limpio
            $(this).text(valorLimpio);

            // Seleccionar todo el contenido
            const range = document.createRange();
            range.selectNodeContents(this);
            const selection = window.getSelection();
            selection.removeAllRanges();
            selection.addRange(range);

            // Agregar clase para identificar que está siendo editado
            $(this).addClass("editando-importe");
        } catch (e) {
            console.error("Error al limpiar el formato:", e);
            $(this).text("0");

            // Seleccionar todo el contenido
            const range = document.createRange();
            range.selectNodeContents(this);
            const selection = window.getSelection();
            selection.removeAllRanges();
            selection.addRange(range);

            $(this).addClass("editando-importe");
        }
    });

    // Control de teclas permitidas: sólo números y un punto decimal
    $("#tbAsientoDetalle tbody").on("keypress", "td.text-end", function (e) {
        // Permitir teclas de control (flechas, backspace, etc.)
        if (e.ctrlKey || e.altKey || e.metaKey) {
            return true;
        }

        // Permitir números (0-9)
        if (/^[0-9]$/.test(e.key)) {
            return true;
        }

        // Permitir punto decimal (solo uno)
        if (e.key === '.' && $(this).text().indexOf('.') === -1) {
            return true;
        }

        // Si es coma, ignorarla (no queremos comas en la edición)
        if (e.key === ',') {
            e.preventDefault();
            return false;
        }

        // Bloquear cualquier otra entrada
        e.preventDefault();
        return false;
    });

    // Al perder el foco, formatear el número
    $("#tbAsientoDetalle tbody").on("blur", "td.text-end", function () {
        // Remover la clase de edición
        $(this).removeClass("editando-importe");

        try {
            // Obtener el valor actual
            let valor = $(this).text().trim();

            // Si está vacío, usar cero
            if (valor === "") {
                valor = "0";
            }

            // Asegurar que sea un número válido
            const numeroDecimal = !isNaN(parseFloat(valor)) ? parseFloat(valor) : 0;

            // Formatear con NumberFormat para consistencia (usando InvariantCulture - punto decimal)
            const formateado = numeroDecimal.toLocaleString('en-US', {
                style: 'currency',
                currency: 'USD', // No importa la moneda, solo el formato
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            }).replace('$', '$ '); // Ajustar el símbolo de moneda

            // Aplicar el formato
            $(this).text(formateado);
        } catch (e) {
            console.error("Error al formatear importe:", e);
            $(this).text("$ 0.00"); // Valor por defecto en caso de error
        }

        // Actualizar totales
        actualizarTotales();
    });
}


/**
 * Actualiza los totales del asiento
 */
function actualizarTotales() {
    let totalDebe = 0;
    let totalHaber = 0;

    // Sumar todos los valores de Debe y Haber
    $("#tbAsientoDetalle tbody tr").each(function () {
        // Extraer los valores de las celdas Debe y Haber
        const celdaDebe = $(this).find("td:nth-child(4)").text().trim();
        const celdaHaber = $(this).find("td:nth-child(5)").text().trim();

        // Limpiar el formato ($, espacios, comas de miles) y mantener el punto decimal
        const debeLimpio = celdaDebe.replace(/[$\s,]/g, "");
        const haberLimpio = celdaHaber.replace(/[$\s,]/g, "");

        // Convertir a número (decimal) - usar 0 si no es válido
        const debe = !isNaN(parseFloat(debeLimpio)) ? parseFloat(debeLimpio) : 0;
        const haber = !isNaN(parseFloat(haberLimpio)) ? parseFloat(haberLimpio) : 0;

        // Sumar a los totales
        totalDebe += debe;
        totalHaber += haber;
    });

    // Formatear los totales usando toLocaleString con formato en-US (punto decimal)
    const formateadoDebe = totalDebe.toLocaleString('en-US', {
        style: 'currency',
        currency: 'USD',
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }).replace('$', '$ ');

    const formateadoHaber = totalHaber.toLocaleString('en-US', {
        style: 'currency',
        currency: 'USD',
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }).replace('$', '$ ');

    // Actualizar los totales en el pie de tabla
    $("#tbAsientoDetalle tfoot tr:first-child td:nth-child(2)").text(formateadoDebe);
    $("#tbAsientoDetalle tfoot tr:first-child td:nth-child(3)").text(formateadoHaber);

    // Verificar si hay diferencia y actualizar la fila de diferencia
    const diferencia = Math.abs(totalDebe - totalHaber);
    const hayDiferencia = diferencia > 0.01; // Usar un umbral pequeño para evitar problemas de precisión

    // Si existe una fila de diferencia, actualizarla; si no, crearla si es necesario
    let filaDiferencia = $("#tbAsientoDetalle tfoot tr:nth-child(2)");

    const formateadoDiferencia = diferencia.toLocaleString('en-US', {
        style: 'currency',
        currency: 'USD',
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }).replace('$', '$ ');

    if (hayDiferencia) {
        if (filaDiferencia.length === 0) {
            // Crear la fila si no existe
            const etiquetaDif = totalDebe > totalHaber ? "Diferencia en el DEBE" : "Diferencia en el HABER";
            filaDiferencia = $(`
                <tr>
                    <td colspan="3" class="text-end fw-bold">${etiquetaDif}</td>
                    <td colspan="2" class="text-end bg-black text-warning fw-bold">${formateadoDiferencia}</td>
                </tr>
            `);
            $("#tbAsientoDetalle tfoot").append(filaDiferencia);
        } else {
            // Actualizar la fila existente
            const etiquetaDif = totalDebe > totalHaber ? "Diferencia en el DEBE" : "Diferencia en el HABER";
            filaDiferencia.find("td:first-child").text(etiquetaDif);
            filaDiferencia.find("td:last-child").text(formateadoDiferencia);
        }

        // Añadir clase de estilo a los totales para resaltar el desbalance
        $("#tbAsientoDetalle tfoot tr:first-child td:nth-child(4), #tbAsientoDetalle tfoot tr:first-child td:nth-child(5)")
            .addClass("total-desbalanceado");
    } else {
        // Si no hay diferencia, eliminar la fila si existe
        filaDiferencia.remove();

        // Quitar clases de desbalance
        $("#tbAsientoDetalle tfoot tr:first-child td:nth-child(4), #tbAsientoDetalle tfoot tr:first-child td:nth-child(5)")
            .removeClass("total-desbalanceado");
    }
}


/**
 * Función para formatear decimales al cargar el asiento
 */
function formatearDecimalesAsiento() {
    // Aplica formato a las celdas con valores decimales que NO han sido formateadas aún
    $("#tbAsientoDetalle td.text-end:not([data-formatting-done='true'])").each(function () {
        const valor = $(this).text().trim();
        if (valor !== "") {
            try {
                // Limpiar el valor eliminando formato actual (pero manteniendo el punto decimal)
                const valorLimpio = valor
                    .replace(/[$\s,]/g, ""); // Quitar $ y espacios y comas

                // Convertir a número - si no es válido, usar 0
                const numeroDecimal = !isNaN(parseFloat(valorLimpio)) ? parseFloat(valorLimpio) : 0;

                // Usar toLocaleString para formato consistente (punto decimal)
                const formateado = numeroDecimal.toLocaleString('en-US', {
                    style: 'currency',
                    currency: 'USD',
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                }).replace('$', '$ ');

                // Aplicar el formato
                $(this).text(formateado);

                // Marcar como formateado
                $(this).attr("data-formatting-done", "true");
            } catch (e) {
                console.error("Error al formatear:", valor, e);
                $(this).text("$ 0.00");
            }
        }
    });

    // Similar para los totales en el pie de tabla 
    $("tfoot td.text-end:not([data-formatting-done='true'])").each(function () {
        const valor = $(this).contents().filter(function () {
            return this.nodeType === 3; // Nodo de texto
        }).text().trim();

        if (valor !== "") {
            try {
                // Limpiar el valor eliminando formato actual (pero manteniendo el punto decimal)
                const valorLimpio = valor
                    .replace(/[$\s,]/g, ""); // Quitar $ y espacios y comas

                // Convertir a número - si no es válido, usar 0
                const numeroDecimal = !isNaN(parseFloat(valorLimpio)) ? parseFloat(valorLimpio) : 0;

                // Usar toLocaleString para formato consistente (punto decimal)
                const formateado = numeroDecimal.toLocaleString('en-US', {
                    style: 'currency',
                    currency: 'USD',
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                }).replace('$', '$ ');

                // Aplicar el formato - reemplazando solo el nodo de texto
                $(this).contents().filter(function () {
                    return this.nodeType === 3;
                }).replaceWith(formateado);

                // Marcar como formateado
                $(this).attr("data-formatting-done", "true");
            } catch (e) {
                console.error("Error al formatear:", valor, e);
                $(this).contents().filter(function () {
                    return this.nodeType === 3;
                }).replaceWith("$ 0.00");
            }
        }
    });
}


/**
 * Confirma la operación de ABM del asiento (Alta, Baja, Modificación)
 */
function confirmarOperacionAsiento() {
    AbrirWaiting("Procesando operación de asiento...");

    // Recopilamos los datos del asiento según la estructura requerida
    const asientoData = recopilarDatosAsiento();

    // Validamos los datos básicos del asiento
    if (!validarAsiento(asientoData)) {
        CerrarWaiting();
        return;
    }

    // Construimos el objeto que coincide con AsientoAccionDto
    const datos = {
        asiento: asientoData,  // El objeto del asiento
        accion: accion         // La acción (por ejemplo, 'A', 'M', 'B')
    };

    // Hacemos la llamada al servicio
    $.ajax({
        url: confirmarAsientoTemporalUrl,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(datos),  // Enviamos el objeto completo
        success: function (obj) {
            CerrarWaiting();

            if (obj.error === true) {
                // Mostrar mensaje de error
                AbrirMensaje("ERROR", obj.msg || "Ocurrió un error al procesar el asiento.", function () {
                    $("#msjModal").modal("hide");
                }, false, ["Aceptar"], "error!", null);
            } else if (obj.warn === true) {
                // Mostrar advertencia
                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    if (obj.auth === true) {
                        window.location.href = loginUrl; // Redirigir al login si es necesario
                    } else {
                        $("#msjModal").modal("hide");
                        if (obj.focus) {
                            $(`#${obj.focus}`).focus();
                        }
                    }
                }, false, ["Continuar"], "warn!", null);
            } else {
                // Operación exitosa
                AbrirMensaje("ÉXITO", obj.msg || "Operación realizada con éxito.", function () {
                    $("#msjModal").modal("hide");
                    InicializaVistaAsientos(); // Cierra el panel de edición y limpia la vista
                    // Refrescar la grilla
                    buscarAsientos(1); // Llama a la función que recarga la grilla
                }, false, ["Aceptar"], "succ!", null);
            }
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            AbrirMensaje("ERROR", "Error al procesar la operación: " + error, function () {
                $("#msjModal").modal("hide");
            }, false, ["Aceptar"], "error!", null);
        }
    });
}


/**
 * Recopila los datos del asiento desde el formulario
 * @returns {Object} Objeto con los datos del asiento
 */
function recopilarDatosAsiento() {
    // Primero intentamos obtener el ID del asiento de varias formas posibles
    // 1. Del atributo data
    let dia_movi = $("#tbAsientoDetalle").data("dia_movi");

    // 2. Si no está en el atributo data, buscar en el campo específico
    if (!dia_movi) {
        dia_movi = $("#Dia_movi").val() || $("input#Dia_movi").val() || $("input[name='Dia_movi']").val();
    }

    // 3. Si seguimos sin tenerlo y estamos en modo modificación, usar EntidadSelect
    if ((!dia_movi || dia_movi === "") && accion === AbmAction.MODIFICACION && EntidadSelect) {
        dia_movi = EntidadSelect;
    }

    // Si aún así no lo tenemos, usar cadena vacía
    dia_movi = dia_movi || "";

    // Tipo de asiento (código)
    const dia_tipo = $("#Dia_tipo").val() || "";

    // Descripción del tipo de asiento (texto)
    const dia_lista = $("#Dia_tipo option:selected").text() || "";

    // Descripción del asiento - ahora usando el ID específico
    let dia_desc_asiento = "";

    // Intentar diferentes selectores para asegurar que encontramos el campo
    if ($("#Dia_desc_asiento").length > 0) {
        dia_desc_asiento = $("#Dia_desc_asiento").val();
    } else if ($("input[name='Dia_desc_asiento']").length > 0) {
        dia_desc_asiento = $("input[name='Dia_desc_asiento']").val();
    } else if ($(".card-header input.form-control").length >= 3) {
        // Fallback a método anterior si no encontramos por ID o nombre
        dia_desc_asiento = $(".card-header input.form-control").eq(2).val();
    }

    // Fecha del asiento - usando el ID específico
    let dia_fecha_val = "";
    if ($("#Dia_fecha").length > 0) {
        dia_fecha_val = $("#Dia_fecha").val();
    } else if ($("input[name='Dia_fecha']").length > 0) {
        dia_fecha_val = $("input[name='Dia_fecha']").val();
    } else {
        dia_fecha_val = $("#tbAsientoDetalle").data("dia_fecha");
    }

    const dia_fecha = obtenerFechaFormateada(dia_fecha_val || new Date());

    // Log para depuración
    console.log("Valores obtenidos:", {
        dia_movi,
        dia_fecha_val,
        dia_fecha,
        dia_tipo,
        dia_lista,
        dia_desc_asiento,
        "desc_selector": $("#Dia_desc_asiento").length > 0 ? "#Dia_desc_asiento" :
            $("input[name='Dia_desc_asiento']").length > 0 ? "input[name='Dia_desc_asiento']" :
                ".card-header input.form-control eq(2)"
    });

    // Recopilamos las líneas de detalle
    const detalles = [];
    let totalDebe = 0;
    let totalHaber = 0;

    $("#tbAsientoDetalle tbody tr").each(function (index) {
        const $row = $(this);

        // Si es una fila informativa (sin cuenta), la saltamos
        if ($row.find("td").length < 5) {
            return;
        }

        // Extraer valores de la fila
        const ccb_id = $row.find(".cuenta-id").text().trim();
        const ccb_desc = $row.find(".cuenta-desc").text().trim();
        const dia_desc = $row.find("td:nth-child(3)").text().trim();

        // Procesar valores monetarios
        let debe = $row.find("td:nth-child(4)").text().trim();
        let haber = $row.find("td:nth-child(5)").text().trim();

        // Convertir moneda a valor numérico
        debe = convertirMonedaANumero(debe);
        haber = convertirMonedaANumero(haber);

        // Actualizar totales
        totalDebe += debe;
        totalHaber += haber;

        // Agregar línea al array
        detalles.push({
            Dia_movi: dia_movi,
            Dia_nro: index + 1,
            Ccb_id: ccb_id,
            Ccb_desc: ccb_desc,
            Dia_desc: dia_desc,
            Debe: debe,
            Haber: haber
        });
    });

    // Retornar el objeto completo
    return {
        Dia_movi: dia_movi,
        Dia_fecha: dia_fecha,
        Dia_tipo: dia_tipo,
        Dia_lista: dia_lista,
        Dia_desc_asiento: dia_desc_asiento,
        TotalDebe: totalDebe,
        TotalHaber: totalHaber,
        Detalles: detalles
    };
}



/**
 * Valida los datos básicos del asiento
 * @param {Object} asiento - Datos del asiento a validar
 * @returns {boolean} True si los datos son válidos, false si no
 */
function validarAsiento(asiento) {
    // Validar tipo de asiento
    if (!asiento.Dia_tipo) {
        AbrirMensaje("VALIDACIÓN", "Debe seleccionar un tipo de asiento.", function () {
            $("#msjModal").modal("hide");
            $("#Dia_tipo").trigger("focus");
        }, false, ["Aceptar"], "warn!", null);
        return false;
    }

    // Validar descripción
    if (!asiento.Dia_desc_asiento) {
        AbrirMensaje("VALIDACIÓN", "Debe ingresar una descripción para el asiento.", function () {
            $("#msjModal").modal("hide");
            $(".card-header input.form-control").eq(2).trigger("focus");
        }, false, ["Aceptar"], "warn!", null);
        return false;
    }

    // Validar que haya al menos una línea
    if (asiento.Detalles.length <= 1) {
        AbrirMensaje("VALIDACIÓN", "El asiento debe tener al menos dos líneas de detalle.", function () {
            $("#msjModal").modal("hide");
        }, false, ["Aceptar"], "warn!", null);
        return false;
    }

    // Validar que todas las líneas tengan cuenta asignada
    for (let i = 0; i < asiento.Detalles.length; i++) {
        const linea = asiento.Detalles[i];
        if (!linea.Ccb_id) {
            AbrirMensaje("VALIDACIÓN", `La línea ${i + 1} debe tener una cuenta contable asignada.`, function () {
                $("#msjModal").modal("hide");
                // Intentar dar foco a la fila correspondiente
                $("#tbAsientoDetalle tbody tr").eq(i).find(".cuenta-id").trigger("focus");
            }, false, ["Aceptar"], "warn!", null);
            return false;
        }
    }

    // Validar que haya al menos una cuenta con importe en el DEBE y otra en el HABER
    let tieneDebe = false;
    let tieneHaber = false;

    // Recorrer todas las líneas para comprobar si hay valores en DEBE y HABER
    for (let i = 0; i < asiento.Detalles.length; i++) {
        const linea = asiento.Detalles[i];
        if (linea.Debe > 0) {
            tieneDebe = true;
        }
        if (linea.Haber > 0) {
            tieneHaber = true;
        }

        // Si ya encontramos ambos, podemos salir del bucle
        if (tieneDebe && tieneHaber) {
            break;
        }
    }

    // Verificar que exista al menos una cuenta en el DEBE
    if (!tieneDebe) {
        AbrirMensaje("VALIDACIÓN", "El asiento debe tener al menos una cuenta con importe en el DEBE.", function () {
            $("#msjModal").modal("hide");
        }, false, ["Aceptar"], "warn!", null);
        return false;
    }

    // Verificar que exista al menos una cuenta en el HABER
    if (!tieneHaber) {
        AbrirMensaje("VALIDACIÓN", "El asiento debe tener al menos una cuenta con importe en el HABER.", function () {
            $("#msjModal").modal("hide");
        }, false, ["Aceptar"], "warn!", null);
        return false;
    }

    // Validar que los totales cuadren
    if (Math.abs(asiento.TotalDebe - asiento.TotalHaber) > 0.01) {
        AbrirMensaje("VALIDACIÓN", "El Saldo no es igual a 0. Verifique importes de Debe y Haber para detectar que valor es incorrecto.", function () {
            $("#msjModal").modal("hide");
        }, false, ["Aceptar"], "warn!", null);
        return false;
    }

    return true;
}


/**
 * Convierte un string con formato de moneda a un número decimal
 * @param {string} moneda - String con formato de moneda (ej: "$ 1.234,56" o "1.234.56")
 * @returns {number} Valor numérico (ej: 1234.56)
 */
function convertirMonedaANumero(moneda) {
    if (!moneda || moneda === "0.00" || moneda === "0") {
        return 0;
    }

    try {
        // Eliminar símbolo de moneda, espacios y comas (separadores de miles)
        let valor = moneda.replace(/[$\s,]/g, "");

        // Convertir a número
        const numero = parseFloat(valor);

        // Devolver 0 si no es un número válido
        return isNaN(numero) ? 0 : numero;
    } catch (e) {
        console.error("Error al convertir moneda a número:", e, moneda);
        return 0;
    }
}

/**
 * Convierte una fecha al formato esperado por el servidor (ISO string)
 * @param {string|Date} fecha - Fecha a formatear
 * @returns {string} Fecha en formato ISO (YYYY-MM-DDTHH:mm:ss.sssZ)
 */
function obtenerFechaFormateada(fecha) {
    if (!fecha) {
        return new Date().toISOString();
    }

    // Si es un string en formato dd/mm/yyyy, convertirlo a Date
    if (typeof fecha === "string" && fecha.includes("/")) {
        const partes = fecha.split("/");
        if (partes.length === 3) {
            const fechaObj = new Date(
                parseInt(partes[2]), // año
                parseInt(partes[1]) - 1, // mes (0-11)
                parseInt(partes[0]) // día
            );
            return fechaObj.toISOString();
        }
    }

    // Si ya es un objeto Date, convertirlo a ISO string
    if (fecha instanceof Date) {
        return fecha.toISOString();
    }

    // Si es otro formato, intentar crear un Date y convertirlo
    try {
        return new Date(fecha).toISOString();
    } catch (e) {
        console.error("Error al formatear fecha:", e);
        return new Date().toISOString();
    }
}



/**
 * Inicializa la vista de asientos y configura los componentes
 * @param {any} e - Evento opcional
 */
function InicializaVistaAsientos(e) {
    // Si NO es la primera ejecución y no hay acción pendiente, limpiar y regresar
    if (!primerArranque && accion === "") {
        // Oculta el panel del asiento
        $("#divpanel01").empty();

        // Limpiar selecciones y ocultar checkboxes
        limpiarSeleccionAsientos();

        // Configuración de componentes
        configurarComponentesIniciales();

        // Mostrar el filtro si no hay datos en la grilla
        mostrarFiltroSiNecesario();

        // Configurar botones y grilla
        accionBotones(AbmAction.CANCEL); // Usar tercer parámetro para mantener el botón cancelar
        removerSeleccion();
        activarGrilla(Grids.GridAsiento);

        CerrarWaiting();
        return; // Importante: salir de la función para evitar recursión
    }

    // Primera ejecución o acción pendiente
    primerArranque = false; // Marcar que ya no es la primera ejecución

    // Limpiar selecciones y ocultar checkboxes
    limpiarSeleccionAsientos();

    // Configuración de componentes
    configurarComponentesIniciales();

    // Mostrar el filtro si no hay datos en la grilla
    mostrarFiltroSiNecesario();

    // Configurar botones y grilla
    accionBotones(AbmAction.CANCEL); // Usar tercer parámetro para mantener el botón cancelar
    removerSeleccion();
    activarGrilla(Grids.GridAsiento);

    CerrarWaiting();
}

/**
 * Limpia todas las selecciones de asientos y oculta los checkboxes
 */
function limpiarSeleccionAsientos() {
    // Limpiar las variables de selección
    filaClicDoble = null;
    filasSeleccionadas = [];

    // Quitar todas las clases de selección
    $("#tbGridAsiento tbody tr").removeClass("selected-simple selected-multiple selectedEdit-row");

    // Desmarcar todos los checkboxes
    $(".asiento-checkbox").prop("checked", false);
    $("#selectAllAsientos").prop("checked", false);

    // Deshabilitar botón de pasar a contabilidad
    $("#btnPasarConta").prop("disabled", true);

    // Actualizar contador de seleccionados
    $("#selected-count").text("0");
}

/**
 * Configura los componentes iniciales de la vista
 */
function configurarComponentesIniciales() {
    // Configurar checkbox de ejercicio según el modo
    if (typeof asientoTemporal !== 'undefined' && asientoTemporal === true) {
        $('#chkEjercicio').prop('checked', true).prop('disabled', true);
        $("#Eje_nro").prop("disabled", false);
    } else {
        toggleComponent('chkEjercicio', '#Eje_nro');
    }

    // Configurar otros componentes
    toggleComponent('Movi', '#Movi_like');
    toggleComponent('Usu', '#Usu_like');
    toggleComponent('Tipo', '#Tipo_like');
    toggleComponent('Rango', 'input[name="Desde"]');
    toggleComponent('Rango', 'input[name="Hasta"]');
}

/**
 * Muestra el filtro si la grilla no tiene datos
 */
function mostrarFiltroSiNecesario() {
    if ($("#divDetalle").is(":visible")) {
        $("#divDetalle").collapse("hide");
    }

    var nng = "#" + Grids.GridAsiento;
    var tb = $(nng + " tbody tr");
    if (tb.length === 0) {
        $("#divFiltro").collapse("show");
    }
}

/**
 * Remueve la selección de todos los registros de la grilla
 */
function removerSeleccion() {
    $("#" + Grids.GridAsiento + " tbody tr").each(function (index) {
        $(this).removeClass("selectedEdit-row");
    });
}

//function InicializaVistaAsientos(e) {
//    if (accion === "" && primerArranque === false) {
//        // Oculta el panel del asiento
//        $("#divpanel01").empty(); // O usa .hide() si prefieres
//        InicializaVistaAsientos();
//    }
//    else {
//        //variable que me permite diferenciar el arranque de la ejecucion normal.
//        primerArranque = false;
//        // Inicialización: Deshabilitar componentes si los checkboxes no están marcados
//        if (typeof asientoTemporal !== 'undefined' && asientoTemporal === true) {
//            // Si está en modo temporal, no ejecutary desactivar el checkbox
//            $('#chkEjercicio').prop('checked', true).prop('disabled', true);
//            $("#Eje_nro").prop("disabled", true);
//        }
//        else {
//            toggleComponent('chkEjercicio', '#Eje_nro');
//        }

//        toggleComponent('Movi', '#Movi_like');
//        toggleComponent('Usu', '#Usu_like');
//        toggleComponent('Tipo', '#Tipo_like');
//        toggleComponent('Rango', 'input[name="Desde"]');
//        toggleComponent('Rango', 'input[name="Hasta"]');

//        grilla = Grids.GridAsiento;

//        if ($("#divDetalle").is(":visible")) {
//            $("#divDetalle").collapse("hide");
//        }
//        nng = "#" + grilla;
//        tb = $(nng + " tbody tr");
//        if (tb.length === 0) {
//            $("#divFiltro").collapse("show");
//        }
//        /** permito con el true final dejar que el boton cancelar este siempre visible */
//        accionBotones(AbmAction.CANCEL,"",true);

//        $("#" + grilla + " tbody tr").each(function (index) {
//            $(this).removeClass("selectedEdit-row");
//        });

//        activarGrilla(grilla);
//        CerrarWaiting();
//    }
//}
function ejecutaDblClickGrid1(x) {
    AbrirWaiting("Espere mientras se busca el Asiento solicitado...");
    selectAsientoDbl(x, Grids.GridAsiento);
}

// Modificar la función selectAsientoDbl para que no elimine otras selecciones
function selectAsientoDbl(x, gridId) {
    // Ya no eliminamos otras selecciones aquí
    // $("#" + gridId + " tbody tr").each(function (index) {
    //     $(this).removeClass("selectedEdit-row");
    // });

    // Solo seleccionamos la actual si no está seleccionada
    if (!$(x).hasClass("selectedEdit-row")) {
        $(x).addClass("selectedEdit-row");
    }

    var id = x.find("td:nth-child(2)").text().trim();

    //se agrega por inyection el tab con los datos del producto
    EntidadEstado = x.find("td:nth-child(4)").text().trim();
    var data = { id: id };
    EntidadSelect = id;
    desactivarGrilla(gridId);
    //se busca el perfil
    buscarAsiento(data);
    //se posiciona el registro seleccionado
    posicionarRegOnTop(x);
}

//function selectAsientoDbl(x, gridId) {
//    $("#" + gridId + " tbody tr").each(function (index) {
//        $(this).removeClass("selectedEdit-row");
//    });
//    $(x).addClass("selectedEdit-row");
//    var id = x.find("td:nth-child(1)").text();

//    //se agrega por inyection el tab con los datos del producto
//    EntidadEstado = x.find("td:nth-child(3)").text();
//    var data = { id: id };
//    EntidadSelect = id;
//    desactivarGrilla(gridId);
//    //se busca el perfil
//    buscarAsiento(data);
//    //se posiciona el registro seleccionado
//    posicionarRegOnTop(x);

//}

function buscarAsiento(data) {
    //se busca el asiento
    PostGenHtml(data, buscarAsientoUrl, function (obj) {
        $("#divpanel01").html(obj);

        // Marcar los elementos para evitar doble formateo
        $("#tbAsientoDetalle td.text-end").attr("data-formatting-done", "true");

        // Aplica formato a los decimales
        formatearDecimalesAsiento();

        $("#btnDetalle").prop("disabled", false);
        $("#divFiltro").collapse("hide");
        $("#divDetalle").collapse("show");

        //activar botones de acción
        activarBotones(true, true);//vamos a dejar el boton cancelar

        CerrarWaiting();
    });
}

// Agregar un mensaje informativo al inicio de la grilla
function agregarMensajeSeleccion() {
    if ($("#seleccion-info").length === 0) {
        const mensaje = `
            <div id="seleccion-info" class="alert alert-info mb-2 py-1 small">
                <i class="bx bx-info-circle"></i> 
                Para seleccionar varios asientos: haga clic en cada fila o use Ctrl+clic para selección múltiple.
                <span class="badge bg-primary ms-1">${$("#tbGridAsiento tbody tr.selectedEdit-row").length}</span> asiento(s) seleccionado(s).
            </div>
        `;
        $("#tbGridAsiento").before(mensaje);
    } else {
        // Actualizar contador
        $("#seleccion-info .badge").text($("#tbGridAsiento tbody tr.selectedEdit-row").length);
    }
}




// Función genérica para habilitar/deshabilitar componentes
function toggleComponent(checkboxId, componentSelector) {
    try {
        const isChecked = $(`#${checkboxId}`).is(':checked');
        const $component = $(componentSelector);

        if (isChecked) {
            $component.prop('disabled', false).css({
                'background-color': '',
                'font-weight': 'normal'
            });
        } else {
            $component.prop('disabled', true).css({
                'background-color': 'rgb(251,255,195)',
                'font-weight': '900'
            });
        }
    } catch (error) {
        console.error(`Error al procesar el checkbox ${checkboxId}:`, error);
    }
}

/**
 * Maneja la impresión de asientos
 */
function imprimirAsiento() {
    // Caso 1: Si hay un asiento abierto (seleccionado con doble clic)
    if (filaClicDoble !== null) {
        // Obtener el ID del asiento abierto
        const asientoId = EntidadSelect;
        if (asientoId) {
            generarReporteAsiento(asientoId);
            return;
        }
    }

    // Caso 2: Si hay asientos seleccionados en la grilla
    if (filasSeleccionadas.length === 1) {
        // Hay un solo asiento seleccionado, obtener su ID
        const asientoId = filasSeleccionadas[0];
        generarReporteAsiento(asientoId);
    } else if (filasSeleccionadas.length > 1) {
        // Hay múltiples asientos seleccionados
        AbrirMensaje(
            "ATENCIÓN",
            "Hay múltiples asientos seleccionados. Por favor, deseleccione todos y seleccione solo uno para imprimir.",
            function () {
                $("#msjModal").modal("hide");
            },
            false,
            ["Aceptar"],
            "warn!",
            null
        );
    } else {
        // No hay ningún asiento seleccionado
        AbrirMensaje(
            "ATENCIÓN",
            "No hay ningún asiento seleccionado para imprimir. Por favor, seleccione uno.",
            function () {
                $("#msjModal").modal("hide");
            },
            false,
            ["Aceptar"],
            "warn!",
            null
        );
    }
}

/**
 * Genera el reporte PDF de un asiento específico
 * @param {string} asientoId - ID del asiento a imprimir (dia_movi)
 */
function generarReporteAsiento(asientoId) {
    // Mostrar indicador de espera
    AbrirWaiting("Generando informe del asiento...");

    // Preparar el objeto de solicitud para el informe
    const reporteSolicitud = {
        Reporte: 9, 
        Parametros: {
            "dia_movi": asientoId
        },
        Titulo: "Informe de Asiento Temporal",
        Observacion: "Detalle del asiento temporal",
        Formato: "P" // P = PDF
    };

    // Realizar la solicitud al servidor
    $.ajax({
        url: gestorImpresionUrl,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(reporteSolicitud),
        success: function (response) {
            CerrarWaiting();

            if (response.error) {
                AbrirMensaje("ERROR", response.msg || "Error al generar el informe", function () {
                    $("#msjModal").modal("hide");
                }, false, ["Aceptar"], "error!", null);
                return;
            }

            if (response.warn) {
                AbrirMensaje("ADVERTENCIA", response.msg, function () {
                    if (response.auth) {
                        window.location.href = loginUrl; // Redirigir al login si es necesario
                    }
                    $("#msjModal").modal("hide");
                }, false, ["Aceptar"], "warn!", null);
                return;
            }

            // Si todo salió bien, mostrar el PDF
            const pdfData = response.base64;

            // Crear un objeto Blob con los datos del PDF
            const blob = b64toBlob(pdfData, "application/pdf");

            // Crear una URL para el Blob
            const url = URL.createObjectURL(blob);

            // Abrir el PDF en una nueva ventana/pestaña
            window.open(url, "_blank");
        },
        error: function (xhr, status, error) {
            CerrarWaiting();
            AbrirMensaje("ERROR", "Error al generar el informe: " + error, function () {
                $("#msjModal").modal("hide");
            }, false, ["Aceptar"], "error!", null);
        }
    });
}

/**
 * Analiza el estado del botón Detalle y realiza las acciones correspondientes
 * Si hay un asiento abierto, lo cierra y limpia el panel
 * @returns {boolean} - Siempre devuelve true para permitir que el evento continúe
 */
function analizaEstadoBtnDetalle() {
    // Verificar si hay un asiento abierto (panel de detalle visible)
    if ($("#divDetalle").is(":visible") && $("#divpanel01").children().length > 0) {
        // Hay un asiento abierto, limpiarlo y cerrar el panel
        limpiarAsientoAbierto();
    }

    // Permitir que el evento siga propagándose (para que funcione el collapse)
    return true;
}

/**
 * Limpia el asiento abierto actualmente y restablece la interfaz
 */
function limpiarAsientoAbierto() {
    // Vaciar el panel que contiene el asiento
    $("#divpanel01").empty();

    // Restablecer variables de control
    filaClicDoble = null;
    EntidadSelect = "";
    EntidadEstado = "";

    InicializaVistaAsientos();


    //// Cerrar el panel de detalle y mostrar el de filtro
    //$("#divDetalle").collapse("hide");
    //$("#divFiltro").collapse("show");

    //// Reactivar la grilla si estaba desactivada
    //activarGrilla(Grids.GridAsiento);

    //// Quitar selección visual en la grilla
    //removerSeleccion();

    //// Actualizar estado de los botones
    //$("#btnDetalle").prop("disabled", true);
    //activarBotones2(false);
}