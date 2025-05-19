$(function () {

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

    $(document).on("click", "#btnCancelarAsiento", function () {
        // Oculta el panel del asiento
        $("#divpanel01").empty(); // O usa .hide() si prefieres
        InicializaVistaAsientos();
    });

    //**** EVENTOS PARA CONTROLAR LOS BOTONES DE OPERACION
    $("#btnAbmNuevo").on("click", ejecutarAlta);
    $("#btnAbmModif").on("click", ejecutarModificacion);
    $("#btnAbmElimi").on("click", ejecutarBaja);

    $("#btnAbmCancelar").on("click", InicializaVistaAsientos);
    $("#btnAbmAceptar").on("click", confirmarOperacionAsiento);
    //****FIN BOTONES DE OPERACION*/


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

    $(document).on("dblclick", "#tbGridAsiento tbody tr", function () {
        x = $(this);
        //se resguarda el registro de la tabla
        regSelected = x;
        ejecutaDblClickGrid1(x);
    });

    InicializaVistaAsientos();
})

//***FUNCIONES DE OPERACION***/
function ejecutarAlta() {
    AbrirWaiting("Espere, se blanquea el ASIENTO...");
    var data = {};
    PostGenHtml(data, nuevoAsientoUrl, function (obj) {
        $("#divpanel01").html(obj);

        $("#btnDetalle").prop("disabled", false);
        $("#divFiltro").collapse("hide");
        $("#divDetalle").collapse("show");

        accionBotones(AbmAction.ALTA);
        activarControles(true);

        CerrarWaiting();
    });
}

function ejecutarModificacion() {
    $("#divFiltro").collapse("hide");
    accionBotones(AbmAction.MODIFICACION);
    activarControles(true);
}

function ejecutarBaja() {
    $("#divFiltro").collapse("hide");
    accionBotones(AbmAction.BAJA);
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
            // Añade botón para agregar líneas si no existe
            const addButton = `
                <button id="btnAddLinea" class="btn btn-sm btn-success ms-2">
                    <i class="bx bx-plus"></i> Agregar línea
                </button>`;
            $(".card-header").append(addButton);

            // Si es nuevo o modificación, permitir editar líneas del asiento
            $("#tbAsientoDetalle thead tr").append(`<th style="width: 5%">Acciones</th>`);
            $("#tbAsientoDetalle tbody tr").append(`
                <td class="text-center">
                    <button class="btn btn-sm btn-danger btn-eliminar-linea">
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
    $(document).on("click", "#btnAddLinea", function() {
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
                <td contenteditable="true"></td>
                <td contenteditable="true"></td>
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
    $(document).on("click", ".btn-eliminar-linea", function() {
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
        // Al entrar al campo, quitar formato y seleccionar todo el contenido
        const valorLimpio = $(this).text().replace(/[$\s.]/g, "").replace(",", ".");
        $(this).text(valorLimpio);

        // Seleccionar todo el contenido
        document.execCommand('selectAll', false, null);

        // Agregar una clase para identificar que está siendo editado
        $(this).addClass("editando-importe");
    });

    // Permitir solo entrada numérica y punto/coma decimal
    $("#tbAsientoDetalle tbody").on("keypress", "td.text-end", function (e) {
        // Permitir: números, punto, coma y teclas de control
        const charCode = (e.which) ? e.which : e.keyCode;

        // Permitir backspace, tab, enter, etc.
        if (e.ctrlKey || e.altKey || e.metaKey || charCode < 32) {
            return true;
        }

        // Permitir números (0-9)
        if (charCode >= 48 && charCode <= 57) {
            return true;
        }

        // Permitir punto o coma (solo uno)
        if ((charCode === 44 || charCode === 46) && $(this).text().indexOf('.') === -1 && $(this).text().indexOf(',') === -1) {
            return true;
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
            // Normalizar el valor (aceptar punto o coma como separador decimal)
            let valor = $(this).text().trim();

            // Si está vacío, usar cero
            if (valor === "") {
                valor = "0";
            }

            // Convertir coma a punto para el procesamiento
            valor = valor.replace(",", ".");

            // Verificar que sea un número válido
            if (isNaN(parseFloat(valor))) {
                valor = "0";
            }

            // Convertir a número y limitar a 2 decimales
            const numeroFormateado = parseFloat(valor).toFixed(2);

            // Formatear con Inputmask (usando coma como separador decimal)
            const formateado = Inputmask.format(numeroFormateado, {
                alias: "numeric",
                groupSeparator: ".",  // Punto como separador de miles
                radixPoint: ",",      // Coma como separador decimal
                digits: 2,
                digitsOptional: false,
                autoGroup: true,
                prefix: "$ "          // Símbolo de moneda
            });

            $(this).text(formateado);
        } catch (e) {
            console.error("Error al formatear importe:", e);
            $(this).text("$ 0,00");   // Valor por defecto en caso de error
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

        // Limpiar el formato ($, espacios y puntos de miles)
        const debeLimpio = celdaDebe.replace(/[$\s.]/g, "").replace(",", ".");
        const haberLimpio = celdaHaber.replace(/[$\s.]/g, "").replace(",", ".");

        // Convertir a número (decimal) - usar 0 si no es válido
        const debe = !isNaN(parseFloat(debeLimpio)) ? parseFloat(debeLimpio) : 0;
        const haber = !isNaN(parseFloat(haberLimpio)) ? parseFloat(haberLimpio) : 0;

        // Sumar a los totales
        totalDebe += debe;
        totalHaber += haber;
    });

    // Formatear y mostrar los totales en el pie de tabla
    const tdTotalDebe = $("#tbAsientoDetalle tfoot tr:first-child td:nth-child(4)");
    const tdTotalHaber = $("#tbAsientoDetalle tfoot tr:first-child td:nth-child(5)");

    // Usar toFixed para asegurar 2 decimales y luego formatear
    tdTotalDebe.text(Inputmask.format(totalDebe.toFixed(2), {
        alias: "numeric",
        groupSeparator: ".",
        radixPoint: ",",
        digits: 2,
        digitsOptional: false,
        autoGroup: true,
        prefix: "$ "
    }));

    tdTotalHaber.text(Inputmask.format(totalHaber.toFixed(2), {
        alias: "numeric",
        groupSeparator: ".",
        radixPoint: ",",
        digits: 2,
        digitsOptional: false,
        autoGroup: true,
        prefix: "$ "
    }));

    // Verificar si hay diferencia y actualizar la fila de diferencia
    const diferencia = Math.abs(totalDebe - totalHaber);
    const hayDiferencia = diferencia > 0.01; // Usar un umbral pequeño para evitar problemas de precisión

    // Si existe una fila de diferencia, actualizarla; si no, crearla si es necesario
    let filaDiferencia = $("#tbAsientoDetalle tfoot tr:nth-child(2)");

    if (hayDiferencia) {
        if (filaDiferencia.length === 0) {
            // Crear la fila si no existe
            const etiquetaDif = totalDebe > totalHaber ? "Diferencia en el DEBE" : "Diferencia en el HABER";
            filaDiferencia = $(`
                <tr>
                    <td colspan="3" class="text-end fw-bold">${etiquetaDif}</td>
                    <td colspan="2" class="text-end bg-black text-warning fw-bold"></td>
                </tr>
            `);
            $("#tbAsientoDetalle tfoot").append(filaDiferencia);
        }

        // Actualizar el valor de la diferencia
        filaDiferencia.find("td:last-child").text(Inputmask.format(diferencia.toFixed(2), {
            alias: "numeric",
            groupSeparator: ".",
            radixPoint: ",",
            digits: 2,
            digitsOptional: false,
            autoGroup: true,
            prefix: "$ "
        }));

        // Actualizar la etiqueta según el tipo de diferencia
        const etiquetaDif = totalDebe > totalHaber ? "Diferencia en el DEBE" : "Diferencia en el HABER";
        filaDiferencia.find("td:first-child").text(etiquetaDif);

        // Añadir clase de estilo a los totales para resaltar el desbalance
        tdTotalDebe.addClass("total-desbalanceado");
        tdTotalHaber.addClass("total-desbalanceado");
    } else {
        // Si no hay diferencia, eliminar la fila si existe
        filaDiferencia.remove();

        // Quitar clases de desbalance
        tdTotalDebe.removeClass("total-desbalanceado");
        tdTotalHaber.removeClass("total-desbalanceado");
    }
}


function confirmarOperacionAsiento() {

}


//*** FIN */
function InicializaVistaAsientos() {

    // Inicialización: Deshabilitar componentes si los checkboxes no están marcados
    if (typeof asientoTemporal !== 'undefined' && asientoTemporal === true) {
        // Si está en modo temporal, no ejecutary desactivar el checkbox
        $('#chkEjercicio').prop('checked', true).prop('disabled', true);
        $("#Eje_nro").prop("disabled", true);
    }
    else {
        toggleComponent('chkEjercicio', '#Eje_nro');
    }

    toggleComponent('Movi', '#Movi_like');
    toggleComponent('Usu', '#Usu_like');
    toggleComponent('Tipo', '#Tipo_like');
    toggleComponent('Rango', 'input[name="Desde"]');
    toggleComponent('Rango', 'input[name="Hasta"]');

    grilla = Grids.GridAsiento;

    if ($("#divDetalle").is(":visible")) {
        $("#divDetalle").collapse("hide");
    }
    nng = "#" + grilla;
    tb = $(nng + " tbody tr");
    if (tb.length === 0) {
        $("#divFiltro").collapse("show");
    }
    accionBotones(AbmAction.CANCEL);
    $("#" + grilla + " tbody tr").each(function (index) {
        $(this).removeClass("selectedEdit-row");
    });

    activarGrilla(grilla);
    CerrarWaiting();
}
function ejecutaDblClickGrid1(x) {
    AbrirWaiting("Espere mientras se busca el Asiento solicitado...");
    selectAsientoDbl(x, Grids.GridAsiento);
}

function selectAsientoDbl(x, gridId) {
    $("#" + gridId + " tbody tr").each(function (index) {
        $(this).removeClass("selectedEdit-row");
    });
    $(x).addClass("selectedEdit-row");
    var id = x.find("td:nth-child(1)").text();

    //se agrega por inyection el tab con los datos del producto
    EntidadEstado = x.find("td:nth-child(3)").text();
    var data = { id: id };
    EntidadSelect = id;
    desactivarGrilla(gridId);
    //se busca el perfil
    buscarAsiento(data);
    //se posiciona el registro seleccionado
    posicionarRegOnTop(x);

}

function buscarAsiento(data) {
    //se busca el asiento
    PostGenHtml(data, buscarAsientoUrl, function (obj) {
        $("#divpanel01").html(obj);

        // Aplica formato a los decimales
        formatearDecimalesAsiento();

        $("#btnDetalle").prop("disabled", false);
        $("#divFiltro").collapse("hide");
        $("#divDetalle").collapse("show");

        //activar botones de acción
        activarBotones(true);

        CerrarWaiting();
    });
}

function buscarAsientos(pagina) {
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
};



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

function formatearDecimalesAsiento() {
    // Aplica formato a las celdas con valores decimales en la tabla de asientos
    $("#tbAsientoDetalle td.text-end").each(function () {
        const valor = $(this).text().trim();
        if (valor !== "" && !isNaN(parseFloat(valor.replace(",", ".")))) {
            const formateado = Inputmask.format(valor, {
                alias: "numeric",
                groupSeparator: ".",
                radixPoint: ",",
                digits: 2,
                digitsOptional: false,
                autoGroup: true,
                prefix: "$ " // Agregamos el signo peso argentino con un espacio
            });
            $(this).text(formateado);
        }
    });

    // También formatea los totales en el pie de tabla
    $("tfoot td.text-end").each(function () {
        const valor = $(this).contents().filter(function () {
            return this.nodeType === 3; // Nodo de texto
        }).text().trim();

        if (valor !== "" && !isNaN(parseFloat(valor.replace(",", ".")))) {
            const formateado = Inputmask.format(valor, {
                alias: "numeric",
                groupSeparator: ".",
                radixPoint: ",",
                digits: 2,
                digitsOptional: false,
                autoGroup: true,
                prefix: "$ " // Agregamos el signo peso argentino con un espacio
            });
            $(this).contents().filter(function () {
                return this.nodeType === 3; // Reemplaza solo el nodo de texto
            }).replaceWith(formateado);
        }
    });
}
