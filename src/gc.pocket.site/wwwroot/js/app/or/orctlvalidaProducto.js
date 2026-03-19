$(function () {
    console.log('✅ Módulo OR Validación de Producto');

    // Inicializar eventos
    inicializarEventosValidacion();
    inicializaPropiedadesOR();
});

function inicializaPropiedadesOR() {
    $("input#Busqueda").prop("disabled", false);
    $("#btnBusquedaBase").prop("disabled", false);
    $("input#Busqueda").on("focus", function () {
        InicializaBusqueda();
    });
}

function inicializarEventosValidacion() {
    //chequea los enter que se dan sobre los controles editables
    $(".inputEditable").on("keypress", analizaEnterInput);

    $("#btnBusquedaBase").on("click", function () {
        buscarProducto();
        return true;
    });

    $("#estadoFuncion").on("change", verificaEstadoOrCtl);

    // ✅ CORRECCIÓN: Vincular el evento UNA SOLA VEZ usando delegación
    // Esto evita múltiples vinculaciones
    //$(document).off("click", "#btnCargarProd").on("click", "#btnCargarProd", cargarCarritoORCtl);
    $(document).on("click", "#btnCargarProd", agregaProductoAListaOrCtl);
}

/**
 * Helper: Mostrar mensajes toast
 */
function mostrarMensaje(mensaje, tipo) {
    var icono = tipo === "success" ? "✅" : tipo === "warning" ? "⚠️" : "❌";
    var clase = tipo === "success" ? "alert-success" : tipo === "warning" ? "alert-warning" : "alert-danger";

    var alertHtml = `
        <div class="alert ${clase} alert-dismissible fade show" role="alert">
            ${icono} ${mensaje}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;

    // Insertar antes del primer row
    $(".row").first().before(alertHtml);

    // Auto-ocultar después de 5 segundos
    setTimeout(function () {
        $(".alert").fadeOut(function () {
            $(this).remove();
        });
    }, 5000);
}

/**
 * Helper: Mostrar indicador de carga
 */
function mostrarCargando(mensaje) {
    if (typeof Swal !== 'undefined') {
        Swal.fire({
            title: mensaje || 'Procesando...',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });
    }
}

/**
 * Helper: Ocultar indicador de carga
 */
function ocultarCargando() {
    if (typeof Swal !== 'undefined') {
        Swal.close();
    }
}

function InicializaBusqueda() {
    $("input#Busqueda").val("");
    $("#P_id").val("");
    $("#Descipcion").val("");
    $("#Rubro").val("");
    $("#up").val(0).prop("disabled", true);
    $("#fvto").val("").prop("disabled", true);

    $("#box").val(0).prop("disabled", true);
    $("#unid").val(0).prop("disabled", true);
    $("#btnCargarProd").prop("disabled", true);

    $("input#Busqueda").prop("disabled", false);
    $("#btnBusquedaBase").prop("disabled", false);
}

function verificaEstadoOrCtl() {
    CerrarWaiting();
    var res = $("#estadoFuncion").val();

    //traigo la variable productoBase e hidrato componentes
    var prod = productoBase;

    $("#P_id").val(prod.p_id);
    $("#Marca").val(prod.p_m_marca || '');
    $("#Descipcion").val(prod.p_desc);
    $("#Rubro").val(prod.rub_desc);

    //charly confirma 12/03/2026 unidad de presentacion que trae producto
    $("#up").val(prod.p_unidad_pres).prop("disabled", false);

    if (prod.up_id === "07") {
        // Unidades enteras
        $("#box").mask("000,000,000,000", { reverse: true });
        $("#box").val(0).prop("disabled", false);

        $("#unid").mask("000,000,000,000", { reverse: true });
        $("#unid").val(0).prop("disabled", false);
    }
    else {
        // Unidades decimales (pesables)
        $("#up").val(1).prop("readonly", true).addClass("backReadOnly");

        $("#box").val(0).prop("disabled", true);

        $("#unid").mask("000,000,000,000.000", { reverse: true });
        $("#unid").val(0).prop("disabled", false);
    }

    //activamos el boton
    $("#btnCargarProd")
        .prop("disabled", false)
        .removeClass("btn-danger")
        .addClass("btn-success");

    // ✅ CORRECCIÓN: NO volver a vincular el evento aquí
    // El evento ya está vinculado en inicializarEventosValidacion()

    //inicializamos el campo de busqueda
    $("#Busqueda").val("");

    // Establecer foco según tipo de producto
    if (prod.p_con_vto !== "N" && prod.p_con_vto !== null && prod.p_con_vto !== " ") {
        $("#fvto").prop("disabled", false).trigger("focus");
    } else {
        if (prod.up_id === "07") {
            $("#up").trigger("focus");
        } else {
            $("#unid").trigger("focus");
        }
    }

    $("#estadoFuncion").val(false);
    $("#btnBusquedaBase").prop("disabled", false);
}

function agregaProductoAListaOrCtl() {
    // Validar contexto básico
    if (!productoBase) {
        mostrarMensaje("No hay producto seleccionado para cargar", "error");
        return;
    }

    console.log("🚀 Ejecutando cargarCarritoORCtl - INICIO");

    AbrirWaiting();

    // Obtener valores de los controles
    const upId = productoBase.up_id;
    const up = parseInt($("#up").val()) || 0;
    const bulto = parseInt($("#box").val()) || 0;
    const unid = parseFloat($("#unid").val()) || 0;
    const fv = $("#fvto").val();
    const pId = productoBase.p_id;
    const pDesc = productoBase.p_desc || '';
    const pIdProv = productoBase.p_id_prov || '';
    const pIdBarrado = productoBase.p_id_barrado || '';

    // Calcular cantidad total
    let cantidad = 0;
    if (upId === "07") {
        cantidad = (up * bulto) + unid;
    } else {
        cantidad = unid;
    }

    // Validación básica: la cantidad debe ser mayor a 0
    if (cantidad <= 0) {
        CerrarWaiting();
        AbrirMensaje(
            "Atención",
            "Debe ingresar una cantidad válida mayor a cero.",
            function () {
                $("#msjModal").modal("hide");
                if (upId === "07") {
                    $("#box").trigger("focus");
                } else {
                    $("#unid").trigger("focus");
                }
            },
            false,
            ["Aceptar"],
            "warn!",
            null
        );
        return;
    }

    // Formatear fecha de vencimiento
    let vtoFormateado = "19700101";
    if (fv) {
        try {
            const fecha = new Date(fv);
            const year = fecha.getFullYear();
            const month = String(fecha.getMonth() + 1).padStart(2, '0');
            const day = String(fecha.getDate()).padStart(2, '0');
            vtoFormateado = `${year}${month}${day}`;
        } catch (e) {
            console.warn("⚠️ Error al formatear fecha, usando fecha por defecto");
        }
    }

    // Construir objeto de datos según OrCtlCargaProductoDto
    const datos = {
        or_compte: orActual || '',
        p_id: pId,
        p_desc: pDesc,
        p_id_prov: pIdProv,
        p_id_barrado: pIdBarrado,
        up_id: upId,
        usu_id: '',
        unidad_pres: up,
        bulto: bulto,
        us: unid,
        vto: vtoFormateado,
        cantidad: cantidad
    };

    console.log("📤 Enviando datos:", datos);

    // Enviar al servidor
    $.ajax({
        url: ResguardarProductoCarritoORUrl,
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        data: JSON.stringify(datos),
        success: function (obj) {
            console.log("✅ Respuesta recibida:", obj);
            CerrarWaiting();

            if (obj.error === true) {
                AbrirMensaje(
                    "Error",
                    obj.msg || "Ocurrió un error al cargar el producto",
                    function () {
                        $("#msjModal").modal("hide");
                    },
                    false,
                    ["Aceptar"],
                    "error!",
                    null
                );
            } else if (obj.warn === true) {
                AbrirMensaje(
                    "Advertencia",
                    obj.msg || "Verifique los datos ingresados",
                    function () {
                        $("#msjModal").modal("hide");
                    },
                    false,
                    ["Aceptar"],
                    "warn!",
                    null
                );
            } else {

                window.location.href = `${proximoProductoUrl}?or_compte=${orActual}`;

            }
        },
        error: function (xhr, status, error) {
            console.error("❌ Error AJAX:", error, xhr);
            CerrarWaiting();

            let mensajeError = "Error de conexión al cargar el producto";
            if (xhr.responseJSON && xhr.responseJSON.msg) {
                mensajeError = xhr.responseJSON.msg;
            } else if (xhr.responseText) {
                try {
                    const response = JSON.parse(xhr.responseText);
                    mensajeError = response.msg || mensajeError;
                } catch (e) {
                    console.error("Error al parsear respuesta:", e);
                }
            }

            AbrirMensaje(
                "Error",
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

    console.log("🚀 Ejecutando cargarCarritoORCtl - FIN");
}