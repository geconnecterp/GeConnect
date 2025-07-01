// Función segura para parsear fechas en formato dd/mm/yyyy
function parseFechaES(fechaString) {
    if (!fechaString) return null;

    // Verificar si la fecha ya tiene el formato correcto (ISO)
    if (fechaString.includes('-') && !fechaString.includes('/')) {
        return new Date(fechaString);
    }

    // Parsear fecha en formato dd/mm/yyyy
    const partes = fechaString.split('/');
    if (partes.length !== 3) {
        console.error(`Formato de fecha inválido: ${fechaString}`);
        return null;
    }

    // Crear fecha con formato yyyy-mm-dd para evitar problemas de zona horaria
    const dia = parseInt(partes[0], 10);
    const mes = parseInt(partes[1], 10) - 1; // Los meses en JS van de 0 a 11
    const anio = parseInt(partes[2], 10);

    // Verificar valores válidos
    if (isNaN(dia) || isNaN(mes) || isNaN(anio)) {
        console.error(`Valores de fecha inválidos: ${fechaString}`);
        return null;
    }

    return new Date(anio, mes, dia);
}

$(function () {
    inicializarComponentesEj();
    configurarBotonesEj();

    // Agregar delegación de eventos para botones dentro de divEj (que se carga dinámicamente)
    $(document).on("click", "#btnConfirmar", confirmarCambiosEjercicio);
    $(document).on("click", "#btnCancelar", cancelarCambiosEjercicio);
    $(document).on("click", "#btnNuevoEjercicio", crearNuevoEjercicio);
    $(document).on("click", "#btnImprimir", imprimirEjercicio);
});

function inicializarComponentesEj() {
    // Inicializar selectores con Select2 si está disponible
    if ($.fn.select2) {
        $("#Eje_nro").select2({
            placeholder: "Seleccione ejercicio",
            width: '100%'
        });
    }

    // Configurar datepickers para fechas con localización explícita
    $.fn.datepicker.dates['es'] = {
        days: ["Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado"],
        daysShort: ["Dom", "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb"],
        daysMin: ["Do", "Lu", "Ma", "Mi", "Ju", "Vi", "Sa"],
        months: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"],
        monthsShort: ["Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic"],
        today: "Hoy",
        clear: "Borrar",
        format: "dd/mm/yyyy",
        titleFormat: "MM yyyy",
        weekStart: 1
    };

    //$('.datepicker').datepicker({
    //    format: 'dd/mm/yyyy',
    //    autoclose: true,
    //    language: 'es',
    //    todayHighlight: true,
    //    orientation: 'bottom'
    //});

    // Aplicar datepicker globalmente (se aplicará también a elementos cargados dinámicamente)
    $(document).on('focus', '.datepicker', function () {
        $(this).datepicker({
            format: 'dd/mm/yyyy',
            autoclose: true,
            language: 'es',
            todayHighlight: true,
            orientation: 'bottom'
        });
    });

    $.datepicker.setDefaults($.datepicker.regional["es"]);
}
function configurarBotonesEj() {
    // Botón de cancelar
    $("#btnCancel").on("click", function () {
        window.location.href = homeEjUrl;
    });

    // Botón de búsqueda
    $("#btnBuscar").on("click", function () {        
        // Es una nueva búsqueda, no resguardamos la búsqueda anterior
        ldDataBak = {};

        // Es una búsqueda por filtro, siempre será página 1
        pagina = 1;

        // Realizar búsqueda
        buscarEj();
    });

    $("#btnFiltro").on("mousedown", function () {
        if ($("#divFiltro").is(":hidden")) {
            $("#divDetalle").collapse("hide");
        }
    });

    $("#btnDetalle").on("mousedown", function () {
        if ($("#divDetalle").is(":visible")) {
            $("divEj").empty();
            $("#btnDetalle").collapse("hide");
            $("#btnFiltro").collapse("show");
        }
    });
    //$("#btnDetalle").on("mousedown", fuction(){
    //    if($("#divDetalle").is(":visible")) {
    //    // HAY QUE LIMPIAR 
    //    bssDataBak = {};
    //    limpiarBss();
    //}
    //});

    // Selección de ejercicio (asumiendo que hay un selector de ejercicio)
    $("#Eje_nro").on("change", function () {
        var ejercicioId = $(this).val();
        if (ejercicioId) {
            cargarDatosEjercicio(ejercicioId);
        }
    });
}

function buscarEj() {
    // Implementar lógica de búsqueda si es necesario
    // Por ejemplo, filtrar ejercicios por año o estado
    var filtro = $("#Eje_nro").val();
    accion = 'M' 
    AbrirWaiting("Buscando ejercicios...");

    var data = {
        ejercicioId: filtro
    };

    PostGenHtml(data, obtenerDatosEjercicioUrl, function (obj) {
        $("#divEj").html(obj);
        $("#divFiltro").collapse("hide");
        $("#divDetalle").collapse("show");
        CerrarWaiting();
    }, function (error) {
        CerrarWaiting();
        AbrirMensaje("Error", "No se pudieron buscar los ejercicios", null, false, ["Aceptar"], "error!", null);
    });
}


// También es importante aplicar formato a las fechas después de cargar datos
function cargarDatosEjercicio(ejercicioId) {
    if (!ejercicioId) {
        AbrirMensaje("Advertencia", "Debe seleccionar un ejercicio", null, false, ["Aceptar"], "warn!", null);
        return;
    }

    accion = 'M' //por default la accion es MODIFICACION
    AbrirWaiting("Cargando datos del ejercicio...");
    var data = { ejercicioId: ejercicioId };

    PostGenHtml(data, obtenerDatosEjercicioUrl, function (obj) {
        $("#divEj").html(obj);

        // Aplicar formato a las fechas inmediatamente después de cargar los datos
        setTimeout(function () {
            configurarControlesEjercicio();
        }, 100);

        // Mostrar el panel de detalles
        $("#divDetalle").collapse("show");
        CerrarWaiting();
    }, function (error) {
        CerrarWaiting();
        AbrirMensaje("Error", "No se pudieron cargar los datos del ejercicio", null, false, ["Aceptar"], "error!", null);
    });
}

// Modificar la función configurarControlesEjercicio
function configurarControlesEjercicio() {
    // Aplicar datepickers específicos con formato español
    $("#ejeDesde, #ejeHasta, #ejeControl").datepicker({
        format: 'dd/mm/yyyy',
        autoclose: true,
        language: 'es',
        todayHighlight: true,
        orientation: 'bottom'
    });

    // El campo Desde siempre deshabilitado
    $("#ejeDesde").prop("disabled", true);

    // El campo Hasta solo editable si es ejercicio corriente
    var esEjercicioCorriente = $("#ejercicioCorriente").is(":visible");
    $("#ejeHasta").prop("disabled", !esEjercicioCorriente);

    // Formatear fechas para mejor visualización
    formatearFechasEjercicio();
}


// Función para formatear fechas del ejercicio
function formatearFechasEjercicio() {
    // Para cada campo de fecha
    ['#ejeDesde', '#ejeHasta', '#ejeControl'].forEach(selector => {
        const $campo = $(selector);
        const valorActual = $campo.val();

        // Solo procesar si hay un valor
        if (valorActual) {
            // Intentar parsear la fecha
            const fecha = parseFechaES(valorActual);

            // Si se pudo parsear, formatear y actualizar
            if (fecha && !isNaN(fecha.getTime())) {
                $campo.val(formatDate(fecha));
            } else {
                // Si no se pudo parsear, mostrar advertencia pero no cambiar el valor
                console.warn(`No se pudo parsear la fecha "${valorActual}" en el campo ${selector}`);
            }
        }
    });
}

// Función mejorada para formatear fechas
function formatDate(date) {
    if (!date || isNaN(date.getTime())) {
        console.error("Fecha inválida en formatDate:", date);
        return "";
    }

    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();
    return `${day}/${month}/${year}`;
}

// Modificar la función parseDateForServer para ser más robusta
function parseDateForServer(dateString) {
    // Convertir formato dd/mm/yyyy a yyyy-mm-dd para enviar al servidor
    if (!dateString) return null;

    var parts = dateString.split('/');
    if (parts.length !== 3) {
        // Intentar interpretar como formato ISO
        if (dateString.includes('-')) {
            return dateString;
        }
        console.warn(`Formato de fecha inesperado para enviar al servidor: ${dateString}`);
        return dateString;
    }

    // Validar partes
    const dia = parseInt(parts[0], 10);
    const mes = parseInt(parts[1], 10);
    const anio = parseInt(parts[2], 10);

    if (isNaN(dia) || isNaN(mes) || isNaN(anio) || dia < 1 || dia > 31 || mes < 1 || mes > 12) {
        console.error(`Valores de fecha inválidos: ${dateString}`);
        return null;
    }

    return `${parts[2]}-${parts[1]}-${parts[0]}`; // yyyy-mm-dd
}

function confirmarCambiosEjercicio() {
    AbrirWaiting("Guardando cambios...");

    var ejercicioId = $("#ejeId").val();
    var desde = $("#ejeDesde").val();
    var hasta = $("#ejeHasta").val();
    var control = $("#ejeControl").val();

    // Validación de campos requeridos
    if (!desde || !hasta || !control) {
        CerrarWaiting();
        AbrirMensaje("Validación", "Todos los campos de fecha son obligatorios", null, false, ["Aceptar"], "warn!", null);
        return;
    }

    // Convertir fechas al formato esperado por el servidor (si es necesario)
    var desdeServer = parseDateForServer(desde);
    var hastaServer = parseDateForServer(hasta);
    var controlServer = parseDateForServer(control);

    // Validar que la conversión fue exitosa
    if (!desdeServer || !hastaServer || !controlServer) {
        CerrarWaiting();
        AbrirMensaje("Error", "Uno o más campos de fecha tienen formato inválido", null, false, ["Aceptar"], "error!", null);
        return;
    }

    var data = {
        Eje_nro: ejercicioId,
        Eje_desde: desdeServer,
        Eje_hasta: hastaServer,
        Eje_ctl: controlServer
    };
    //tengo que indicar la accion que realiza

    PostGen(data, confirmarEjercicioUrl+ "?accion="+accion, function (response) {
        CerrarWaiting();

        if (response.error) {
            AbrirMensaje("Error", response.msg, function () {
                $("#msjModal").modal("hide");
            }, false, ["Aceptar"], "error!", null);
        } else
            if (response.warn) {
                AbrirMensaje("Atención", response.msg, function () {
                    $("#msjModal").modal("hide");
                }, false, ["Aceptar"], "warn!", null);
            } else
        {
            AbrirMensaje("Éxito", "Los cambios se han guardado correctamente", function () {
                $("#msjModal").modal("hide");
                // Recargar los datos del ejercicio
                //cargarDatosEjercicio(ejercicioId); 
                buscarEj();
            }, false, ["Aceptar"], "succ!", null);
        }
    }, function (error) {
        CerrarWaiting();
        AbrirMensaje("Error", "No se pudieron guardar los cambios", null, false, ["Aceptar"], "error!", null);
    });
}

function cancelarCambiosEjercicio() {
   //var ejercicioId = $("#Eje_nro").val();

    AbrirMensaje("Confirmar", "¿Está seguro que desea cancelar los cambios?", function (resp) {
        $("#msjModal").modal("hide");
        if (resp === "SI") {

            // Recargar los datos del ejercicio
            //cargarDatosEjercicio(ejercicioId);
            buscarEj();
        }
        
    }, true, ["Sí", "No"], "question!", null);
}

function crearNuevoEjercicio() {
    AbrirWaiting("Preparando formulario...");
    accion = 'A';
    PostGenHtml({}, nuevoEjercicioUrl, function (obj) {
        $("#divEj").html(obj);
        configurarControlesEjercicio();
        CerrarWaiting();
    }, function (error) {
        CerrarWaiting();
        AbrirMensaje("Error", "No se pudo preparar el formulario para un nuevo ejercicio", null, false, ["Aceptar"], "error!", null);
    });
}

function imprimirEjercicio() {
    var ejercicioId = $("#ejeId").val();

    if (!ejercicioId) {
        AbrirMensaje("Advertencia", "Debe seleccionar un ejercicio para imprimir", null, false, ["Aceptar"], "warn!", null);
        return;
    }

    window.open(imprimirEjercicioUrl + "?ejercicioId=" + ejercicioId, "_blank");
}