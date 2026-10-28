let provIdSelected = null;
let provDesdSelected = null;
let rpIdSelected = null;
const selectorTablaControl = "#tbControlRecepcion";
const selectorInputControl = "#tbControlRecepcion .js-control-cantidad:not(:disabled)";
const DECIMALES_PESABLES = 2;
let controlRecepcionEnEdicion = false;
let rpSeleccionada = null;

var productoBase = null; //se hidrata en busquedas.
var p_id_selected = "";
var funcionBusquedaAvanzada = true;

$(function () {
	InicializarCamposEnFiltros();
	$("#divFiltro").on("shown.bs.collapse hidden.bs.collapse", function () {
		const abierto = $(this).hasClass("show");

		if (abierto) {
			$("#divDetalle").collapse("hide");
		} else {
			$("#divDetalle").collapse("show");
		}
	});

	$("input#Rel01").on("click", function () {
		$("input#Rel01").val("");
		$("#Rel01Item").val("");
	});
	$(document).off("click", "#btnCancel");
	$(document).on("click", "#btnCancel", ControlaCancelar);
	$("#btnBuscar").on("click", function () {
		BuscarRecepcionesDelProveedor();
	});

	/*
	 * Se utilizan eventos delegados porque la tabla se incorpora
	 * dinámicamente dentro de #divDetalle.
	 */
	$(document).off("click.controlRecepcion", "#btnAgregarControlRecepcion")
				.on("click.controlRecepcion", "#btnAgregarControlRecepcion",
					function () {
						agregarProductoAControlRecepcion();
					}
	);
	$(document).off("click.controlRecepcion", "#tbControlRecepcion .btn-eliminar-control-recepcion")
				.on("click.controlRecepcion", "#tbControlRecepcion .btn-eliminar-control-recepcion",
					function (event) {
						event.preventDefault();
						event.stopPropagation();

						eliminarFilaControlRecepcion($(this).closest("tr"));
					}
	);
    $(document).off("keydown.controlRecepcion", "#tbControlRecepcion .js-control-cantidad")
                .on("keydown.controlRecepcion", "#tbControlRecepcion .js-control-cantidad",
            function (event) {

                if (event.key === "Enter") {

                    event.preventDefault();

                    moverFocoControlRecepcion(
                        this,
                        event.shiftKey ? -1 : 1
                    );
                }

                if (event.key === "ArrowDown") {

                    event.preventDefault();

                    moverFocoVerticalControlRecepcion(
                        this,
                        1
                    );
                }

                if (event.key === "ArrowUp") {

                    event.preventDefault();

                    moverFocoVerticalControlRecepcion(
                        this,
                        -1
                    );
                }
            }
    );
    $(document).off("click.controlRecepcion", "#tbControlRecepcion tbody tr.fila-control-recepcion")
                .on("click.controlRecepcion", "#tbControlRecepcion tbody tr.fila-control-recepcion", function (e) {

                    // Ignorar clicks sobre inputs y botones
                    if ($(e.target).closest("input, button, .btn-opcion-tabla").length) {
                        return;
                    }

                    $("#tbControlRecepcion tbody tr")
                        .removeClass("selected-row");

                    $(this)
                        .addClass("selected-row");
                });
	$(document).off("input.controlRecepcion", "#tbControlRecepcion .js-control-cantidad")
				.on("input.controlRecepcion", "#tbControlRecepcion .js-control-cantidad",
					function () {
						/*normalizarEntradaControlRecepcion(this);*/
						marcarCampoModificadoControlRecepcion(this);
						recalcularFilaControlRecepcion($(this).closest("tr"));
					}
	);
	$(document).off("blur.controlRecepcion", "#tbControlRecepcion .js-control-cantidad")
				.on("blur.controlRecepcion", "#tbControlRecepcion .js-control-cantidad",
					function () {
						formatearInputControlRecepcion(this);
						marcarCampoModificadoControlRecepcion(this);
                        recalcularFilaControlRecepcion($(this).closest("tr"));
                        enviarCambioControlRecepcion(this);
					}
	);
	$(document).off("focus.controlRecepcion", "#tbControlRecepcion .js-control-cantidad")
				.on("focus.controlRecepcion", "#tbControlRecepcion .js-control-cantidad",
                    function () {

                        $("#tbControlRecepcion tbody tr")
                            .removeClass("selected-row");

                        $(this)
                            .closest("tr")
                            .addClass("selected-row");

                        this.select();
                    }
    );
    $(document).off("click.controlRecepcion", "#btnConfirmarControl")
                .on("click.controlRecepcion", "#btnConfirmarControl", function () {

            console.log("click.controlRecepcion #btnConfirmarControl");
            confirmarControlRpr();
                });
    $(document).off("click.controlRecepcion", "#btnCambiarTipoComp")
        .on("click.controlRecepcion", "#btnCambiarTipoComp", function () {

            console.log("click.controlRecepcion #btnCambiarTipoComp");
            cambiarTcoEnRpr();
        });
    $(document).off("click.controlRecepcion", "#btnCancelarControl")
                .on("click.controlRecepcion", "#btnCancelarControl", function () {

            if (typeof controlRecepcionTieneCambios === "function" && controlRecepcionTieneCambios()) {
                AbrirMensaje("ATENCIÓN", "Tiene cambios sin guardar. Si cancela perderá los cambios realizados. ¿Desea continuar?", function (e) {
                    $("#msjModal").modal("hide");
                    switch (e) {
                        case "SI":
                            habilitarAgregarControlRecepcion();
                            ejecutarCancelacionControl();
                            break;
                        case "NO":
                            break;
                        default: //NO
                            break;
                    }
                    return true;

                }, true, ["SI", "NO"], "question!", null);
            }
            else {
                habilitarAgregarControlRecepcion();

                ejecutarCancelacionControl();
            }
            
    });

    /*
     * Función pública para enviar el contenido actualizado al backend.
     *
     * Uso:
     * const detalle = obtenerDetalleControlRecepcion();
     */
    window.obtenerDetalleControlRecepcion = function () {
        const detalle = [];

        $(
            selectorTablaControl + " tbody tr.fila-control-recepcion"
        ).each(function () {
            detalle.push(
                construirDetalleFilaControlRecepcion($(this))
            );
        });

        return detalle;
    };

    /*
     * Función pública para comprobar si hubo modificaciones.
     */
    window.controlRecepcionTieneCambios = function () {
        return (
            $(selectorTablaControl + " .campo-modificado").length > 0 ||
            $(selectorTablaControl + ' tbody tr[data-eliminado="true"]').length > 0 ||
            $(selectorTablaControl + ' tbody tr[data-nuevo="true"]').length > 0
        );

        //TODO: cuando agreguemos un producto lo tenemos que marcar como:
        //<tr class="fila-control-recepcion" data-nuevo="true">
        //o
        //$fila.attr("data-nuevo", "true");
    };

    /*
     * Inicialización después de recibir el partial.
     */
    window.inicializarTablaControlRecepcion = function () {
        $(
            selectorTablaControl +
            " tbody tr:not(.fila-eliminada)"
        ).each(function () {
            recalcularFilaControlRecepcion($(this));
        });
        aplicarMascarasControlRecepcion();
        $(selectorTablaControl + " .campo-modificado").each(function () {

            const $contenedor = $(this).closest(".input-container");

            if ($contenedor.find(".indicador-cambio").length === 0) {
                $contenedor.append(
                    '<div class="indicador-cambio"></div>'
                );
            }
        });
        alternarFilasVisiblesControlRecepcion();
        deshabilitarConfirmarControlRecepcion();
    };

    window.abrirBuscadorProductoControlRecepcion = function () {
        AbrirModalAgregarProducto();
    };
});

function confirmarControlRpr() {
    AbrirMensaje("ATENCIÓN", "¿Desea Confirmar el Control?", function (e) {
        $("#msjModal").modal("hide");
        switch (e) {
            case "SI":
                handlerconfirmarControlRpr();
                break;
            case "NO":
                break;
            default: //NO
                break;
        }
        return true;

    }, true, ["SI", "NO"], "question!", null);
}

function handlerconfirmarControlRpr() {
    AbrirWaiting("Guardando cambios....")
    let rp_compte = rpSeleccionada.rp_compte;
    PostGen({ rp_compte }, confirmarRprCtlUrl, function (obj) {
        CerrarWaiting();
        if (obj.error === true || obj.warn === true) {
            console.error('❌ Response:', obj.msg);
            AbrirMensaje("ATENCIÓN", 'Error al intentar confirmar el control de recepción de proveedor: ' + (obj.msg || 'Error desconocido'), function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else {
            setTimeout(() => {
                AbrirMensaje(
                    'CONFIRMACIÓN EXITOSA',
                    'Se ha confirmado el Control de Recepcion de Proveedor.',
                    function () {
                        $('#msjModal').modal('hide');
                        BuscarRecepcionesDelProveedor();
                        //Imprimir Remito
                        //ImprimirPlanilla(obj.id);
                        //console.log("Planilla de Elaboración y Fracción Generada: ", obj.id);
                        //ResetearPantalla();
                    },
                    false,
                    ['Aceptar'],
                    'success!',
                    null
                );
            }, 200);
        }
    });
}

function cambiarTcoEnRpr() {
    AbrirMensaje("ATENCIÓN", "¿Desea Cambiar el Tipo de Comprobante en la RPR?", function (e) {
        $("#msjModal").modal("hide");
        switch (e) {
            case "SI":
                handlerCambiarTcoEnRpr();
                break;
            case "NO":
                break;
            default: //NO
                break;
        }
        return true;

    }, true, ["SI", "NO"], "question!", null);
}

function handlerCambiarTcoEnRpr() {
    AbrirWaiting("Guardando cambios....")
    let rp_compte = rpSeleccionada.rp_compte;
    PostGen({ rp_compte }, cambiarTcoEnRprUrl, function (obj) {
        CerrarWaiting();
        if (obj.error === true || obj.warn === true) {
            console.error('❌ Response:', obj.msg);
            AbrirMensaje("ATENCIÓN", 'Error al intentar cambiar el tipo de comprobante en la RPR: ' + (obj.msg || 'Error desconocido'), function () {
                $("#msjModal").modal("hide");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else {
            setTimeout(() => {
                AbrirMensaje(
                    'CONFIRMACIÓN EXITOSA',
                    'Se ha cambiado el tipo de comprobante en la RPR.',
                    function () {
                        $('#msjModal').modal('hide');
                        BuscarRecepcionesDelProveedor();
                        //Imprimir Remito
                        //ImprimirPlanilla(obj.id);
                        //console.log("Planilla de Elaboración y Fracción Generada: ", obj.id);
                        //ResetearPantalla();
                    },
                    false,
                    ['Aceptar'],
                    'success!',
                    null
                );
            }, 200);
        }
    });
}

function actualizarDetalleRecepcion(dto) {
    console.log("dto", dto);
    var data = JSON.stringify(dto);
    $.ajax({
        url: "/Compras/ControlYCorreccionDeRP/ActualizarValorEnDetalle",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(dto),

        success: function (response) {

            console.log("Actualizado correctamente");
        },

        error: function (xhr) {

            console.error("Error al actualizar", xhr);
        }
    });
}

function quitarItemEnDetalleRecepcion(pId, item) {
    var dto = { p_id: pId, item };
    var data = JSON.stringify(dto);
    $.ajax({
        url: "/Compras/ControlYCorreccionDeRP/QuitarItemEnDetalleRecepcion",
        type: "POST",
        contentType: "application/json",
        data: data,

        success: function (response) {

            console.log("Actualizado correctamente");
        },

        error: function (xhr) {

            console.error("Error al actualizar", xhr);
        }
    });
}

function normalizarNumero(valor) {

    if (!valor) return "";

    // Quitar separadores de miles (coma)
    valor = valor.replace(/,/g, "");

    // Trim por seguridad
    valor = valor.trim();

    return valor;
}

function ejecutarCancelacionControl() {
    //Recargar la grilla
    BuscarRecepcionesDelProveedor();
}

function agregarProductoAControlRecepcion() {
    /*
     * Punto de integración con el buscador de productos.
     *
     * Si ya existe una función global para abrir el buscador,
     * se reutiliza. En caso contrario se publica un evento para
     * mantener este módulo desacoplado.
     */
    if (typeof window.abrirBuscadorProductoControlRecepcion === "function") {
        window.abrirBuscadorProductoControlRecepcion();
        return;
    }

    $(document).trigger("controlRecepcion:agregarProducto");
}

function eliminarFilaControlRecepcion($fila) {
    if (!$fila || !$fila.length) {
        return;
    }

    const productoId = String($fila.data("p-id") || "");
    const item = parseInt($fila.data("rpd-item"), 10) || 0;

    /*
     * Si el registro ya existe en base de datos, no se elimina
     * físicamente del DOM. Se marca para incluirlo en el payload.
     *
     * Si es un registro nuevo, se puede retirar directamente.
     */
    if (item > 0) {
        $fila
            .attr("data-eliminado", "true")
            .data("eliminado", true)
            .addClass("fila-eliminada");
    } else {
        $fila.remove();
    }

    $(document).trigger("controlRecepcion:filaEliminada", [{
        p_id: productoId,
        rpd_item: item
    }]);
    quitarItemEnDetalleRecepcion(productoId, item);
    alternarFilasVisiblesControlRecepcion();
    habilitarConfirmarControlRecepcion();

}

function deshabilitarAgregarControlRecepcion() {
    controlRecepcionEnEdicion = true;

    $("#btnAgregarControlRecepcion")
        .prop("disabled", true)
        .addClass("disabled");
}

function habilitarAgregarControlRecepcion() {
    controlRecepcionEnEdicion = false;

    $("#btnAgregarControlRecepcion")
        .prop("disabled", false)
        .removeClass("disabled");
}

function habilitarConfirmarControlRecepcion() {

    $("#btnConfirmarControl")
        .prop("disabled", false)
        .removeClass("disabled");
}

function deshabilitarConfirmarControlRecepcion() {

    $("#btnConfirmarControl")
        .prop("disabled", true)
        .addClass("disabled");
}

function normalizarEntradaControlRecepcion(input) {
    const $input = $(input);
    const tipo = $input.data("tipo");
    const decimales = parseInt($input.data("decimales"), 10) || 0;

    let valor = String($input.val() || "")
        .replace(/\s/g, "")
        .replace(",", ".");

    if (tipo === "entero") {
        valor = valor.replace(/[^\d]/g, "");
    } else {
        valor = valor.replace(/[^\d.]/g, "");

        const partes = valor.split(".");

        if (partes.length > 2) {
            valor = partes.shift() + "." + partes.join("");
        }

        if (decimales === 0) {
            valor = valor.split(".")[0];
        } else if (valor.includes(".")) {
            const parteEntera = valor.split(".")[0];
            const parteDecimal = valor.split(".")[1] || "";

            valor =
                parteEntera +
                "." +
                parteDecimal.substring(0, decimales);
        }
    }

    $input.val(valor);
}

function formatearInputControlRecepcion(input) {
    const $input = $(input);
    const tipo = $input.data("tipo");
    const decimales = parseInt($input.data("decimales"), 10) || 0;

    const valor = obtenerNumeroControlRecepcion($input.val());

    $input.removeClass("input-invalido");

    if (!Number.isFinite(valor) || valor < 0) {
        $input
            .val(tipo === "entero" ? "0" : (0).toFixed(decimales))
            .addClass("input-invalido");

        return;
    }

    if (tipo === "entero") {
        $input.val(Math.trunc(valor).toString());
        return;
    }

    $input.val(valor.toFixed(decimales));
}

function obtenerNumeroControlRecepcion(valor) {

    if (
        valor === null ||
        valor === undefined ||
        valor === ""
    ) {
        return 0;
    }

    const textoNormalizado = String(valor)
        .replace(/,/g, '')
        .trim();

    const numero = parseFloat(textoNormalizado);

    return Number.isFinite(numero)
        ? numero
        : 0;
}

function marcarCampoModificadoControlRecepcion(input) {

    const $input = $(input);

    const valorActual =
        obtenerNumeroControlRecepcion($input.val());

    const valorOriginal =
        obtenerNumeroControlRecepcion(
            $input.attr("data-original-value")
        );

    const fueModificado =
        Math.abs(valorActual - valorOriginal) > 0.0000001;

    $input.toggleClass("campo-modificado", fueModificado);

    const $contenedor = $input.closest(".input-container");

    if (fueModificado) {

        if ($contenedor.find(".indicador-cambio").length === 0) {
            $contenedor.append(
                '<div class="indicador-cambio"></div>'
            );
        }

        deshabilitarAgregarControlRecepcion();
        habilitarConfirmarControlRecepcion();
    }
    else {

        $contenedor.find(".indicador-cambio").remove();
    }
}

function recalcularFilaControlRecepcion($fila) {
    if (!$fila || !$fila.length) {
        return;
    }

    const unidadPresentacion =
        obtenerNumeroControlRecepcion(
            $fila.children("td").eq(2).text()
        );

    const bultosFactura =
        obtenerValorCampoFila(
            $fila,
            "rpd_bulto_compte"
        );

    const unidadSueltaFactura =
        obtenerValorCampoFila(
            $fila,
            "rpd_unidad_suelta_compte"
        );

    const bultosRecepcion =
        obtenerValorCampoFila(
            $fila,
            "rpd_bulto_recibidos"
        );

    const unidadSueltaRecepcion =
        obtenerValorCampoFila(
            $fila,
            "rpd_unidad_suelta"
        );

    const cantidadOriginal =
        obtenerNumeroControlRecepcion(
            $fila
                .find('[data-field="rpd_cantidad_ori"]')
                .attr("data-value")
        );

    const cantidadFactura =
        calcularCantidadControlRecepcion(
            unidadPresentacion,
            bultosFactura,
            unidadSueltaFactura
        );

    const cantidadRecepcion =
        calcularCantidadControlRecepcion(
            unidadPresentacion,
            bultosRecepcion,
            unidadSueltaRecepcion
        );

    const diferencia =
        cantidadOriginal - cantidadRecepcion;

    actualizarValorCalculado(
        $fila,
        "rpd_cantidad_compte",
        cantidadFactura
    );

    actualizarValorCalculado(
        $fila,
        "rpd_cantidad",
        cantidadRecepcion
    );

    actualizarDiferenciaControlRecepcion(
        $fila,
        diferencia
    );

    $(document).trigger("controlRecepcion:filaModificada", [
        construirDetalleFilaControlRecepcion($fila)
    ]);
}

function enviarCambioControlRecepcion(input) {

    const $input = $(input);
    const $fila = $input.closest("tr");

    const dto = {
        p_id: String($fila.data("p-id")),
        campo: String($input.data("field")),
        valor: Number($input.inputmask("unmaskedvalue"))
    };

    console.log(dto);

    actualizarDetalleRecepcion(dto);
}

function calcularCantidadControlRecepcion( unidadPresentacion, bultos, unidadSuelta) {
    return (
        (obtenerNumeroControlRecepcion(bultos) *
            obtenerNumeroControlRecepcion(unidadPresentacion)) +
        obtenerNumeroControlRecepcion(unidadSuelta)
    );
}

function obtenerValorCampoFila($fila, nombreCampo) {
    const $campo = $fila.find(
        '.js-control-cantidad[data-field="' +
        nombreCampo +
        '"]'
    );

    return obtenerNumeroControlRecepcion($campo.val());
}

function actualizarValorCalculado($fila, nombreCampo, valor) {
    const $elemento = $fila.find(
        '[data-field="' + nombreCampo + '"]'
    );

    const decimales = obtenerDecimalesFila($fila);

    $elemento
        .attr("data-value", valor)
        .text(formatearCantidadControlRecepcion(valor, decimales));
}

function actualizarDiferenciaControlRecepcion($fila, diferencia) {
    const $diferencia = $fila.find(
        '[data-field="diferencia"]'
    );

    const decimales = obtenerDecimalesFila($fila);

    $diferencia
        .removeClass(
            "diferencia-positiva " +
            "diferencia-negativa " +
            "diferencia-cero"
        )
        .addClass(
            diferencia > 0
                ? "diferencia-positiva"
                : diferencia < 0
                    ? "diferencia-negativa"
                    : "diferencia-cero"
        )
        .attr("data-value", diferencia)
        .text(
            formatearCantidadControlRecepcion(
                diferencia,
                decimales
            )
        );
}

function obtenerDecimalesFila($fila) {
    const permiteDecimales =
        String($fila.attr("data-permite-decimales"))
            .toLowerCase() === "true";

    return permiteDecimales ? DECIMALES_PESABLES : 0;
}

function formatearCantidadControlRecepcion(valor, decimales) {

    const numero = obtenerNumeroControlRecepcion(valor);

    return numero.toLocaleString(
        'en-US',
        {
            minimumFractionDigits: decimales,
            maximumFractionDigits: decimales
        }
    );
}


function moverFocoControlRecepcion(inputActual, desplazamiento) {
    const $campos = $(
        selectorInputControl + ":visible"
    );

    const indiceActual = $campos.index(inputActual);

    if (indiceActual < 0 || !$campos.length) {
        return;
    }

    let nuevoIndice =
        (indiceActual + desplazamiento) % $campos.length;

    if (nuevoIndice < 0) {
        nuevoIndice = $campos.length - 1;
    }

    $campos.eq(nuevoIndice).trigger("focus");
}

function moverFocoVerticalControlRecepcion(inputActual, desplazamiento) {
    const $inputActual = $(inputActual);
    const nombreCampo = $inputActual.data("field");
    const $filaActual = $inputActual.closest("tr");

    const $filasVisibles = $(
        selectorTablaControl + " tbody tr:not(.fila-eliminada)"
    );

    const indiceFilaActual = $filasVisibles.index($filaActual);
    const indiceDestino = indiceFilaActual + desplazamiento;

    if (
        indiceFilaActual < 0 ||
        indiceDestino < 0 ||
        indiceDestino >= $filasVisibles.length
    ) {
        return;
    }

    const $inputDestino = $filasVisibles
        .eq(indiceDestino)
        .find(
            '.js-control-cantidad[data-field="' +
            nombreCampo +
            '"]:not(:disabled)'
        );

    if ($inputDestino.length) {
        $inputDestino.trigger("focus");
    }
}

function alternarFilasVisiblesControlRecepcion() {
    $(
        selectorTablaControl +
        " tbody tr:not(.fila-eliminada)"
    ).each(function (indice) {
        $(this).toggleClass("alt", indice % 2 === 0);
    });
}

function construirDetalleFilaControlRecepcion($fila) {
    const cantidadOriginal =
        obtenerNumeroControlRecepcion(
            $fila
                .find('[data-field="rpd_cantidad_ori"]')
                .attr("data-value")
        );

    const cantidadFactura =
        obtenerNumeroControlRecepcion(
            $fila
                .find('[data-field="rpd_cantidad_compte"]')
                .attr("data-value")
        );

    const cantidadRecepcion =
        obtenerNumeroControlRecepcion(
            $fila
                .find('[data-field="rpd_cantidad"]')
                .attr("data-value")
        );

    const diferencia =
        obtenerNumeroControlRecepcion(
            $fila
                .find('[data-field="diferencia"]')
                .attr("data-value")
        );

    return {
        p_id: String($fila.data("p-id") || ""),
        rpd_item: parseInt($fila.data("rpd-item"), 10) || 0,
        permiteDecimales:
            String($fila.attr("data-permite-decimales"))
                .toLowerCase() === "true",
        up_tipo: String($fila.data("up-tipo") || ""),
        rpd_bulto_compte:
            Math.trunc(
                obtenerValorCampoFila(
                    $fila,
                    "rpd_bulto_compte"
                )
            ),
        rpd_unidad_suelta_compte:
            obtenerValorCampoFila(
                $fila,
                "rpd_unidad_suelta_compte"
            ),
        rpd_cantidad_compte: cantidadFactura,
        rpd_bulto_recibidos:
            Math.trunc(
                obtenerValorCampoFila(
                    $fila,
                    "rpd_bulto_recibidos"
                )
            ),
        rpd_unidad_suelta:
            obtenerValorCampoFila(
                $fila,
                "rpd_unidad_suelta"
            ),
        rpd_cantidad: cantidadRecepcion,
        rpd_cantidad_ori: cantidadOriginal,
        diferencia: diferencia,
        eliminado:
            String($fila.attr("data-eliminado"))
                .toLowerCase() === "true"
    };
}
function ControlaCancelar() {
	$("input#Rel01").trigger('click');
	$("#listaRecepciones").val("");
	provIdSelected = null;
	provDesdSelected = null;
}

function BuscarRecepcionesDelProveedor() {
    let rpCompte = $("#listaRecepciones").val();
    let proveedor = provDesdSelected;
    let rpCompteDesc = $("#listaRecepciones").find("option:selected").text();
	if (!rpCompte || rpCompte == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar una Recepción.", function () {
			$("#msjModal").modal("hide");
			$("#listaRecepciones").trigger('focus');
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	AbrirWaiting("Cargando Recepción del Proveedor.")
    var data = { rp_compte: rpCompte, prov: proveedor, rp_compte_desc: rpCompteDesc };
	PostGenHtml(data, obtenerDetalleDeRecepcionURL, function (obj) {
		$("#divDetalle").html(obj);
		$("#divFiltro").collapse("hide");
        $("#divDetalle").collapse("show");

        if (typeof inicializarTablaControlRecepcion === "function") {
            inicializarTablaControlRecepcion();
        }

		CerrarWaiting();
		return true
	});
	console.log("BuscarRecepcionesDelProveedor");
}

function InicializarCamposEnFiltros() {
	$("#btnImprimir").hide();

	$("#chkRel01").prop('checked', true);
	$("#chkRel01").trigger("change");
	$("#chkRel01").prop("disabled", true);

	$("#lbRel01").text("Proveedor");
	$("#lbRecepciones").text("Recepciones");

	$("#chkRecepciones").prop('checked', true);
	$("#chkRecepciones").trigger("change");
	$("#chkRecepciones").prop("disabled", true);

	$("#divFiltro").collapse("show");
	$("#divDetalle").collapse("hide");
}

$("#Rel01").autocomplete({
	source: function (request, response) {

		const data = { prefix: request.term };

		$.ajax({
			url: autoComRel01Url,
			type: "POST",
			dataType: "json",
			data: data,
			success: function (obj) {
				response($.map(obj, function (item) {
					return {
						label: item.descripcion,   // Ej: "UNILEVER PC#BIENES DE CAMBIO"
						value: item.descripcion,
						id: item.id,
						prov: item.provId
					};
				}));
			}
		});
	},
	minLength: 3,

	focus: function (event, ui) {
		// evita que el # aparezca mientras navegas con flechas
		const partes = ui.item.value.split("#");
		$("#Rel01").val(partes.join(" "));
		return false;
	},

	select: function (event, ui) {

		const partes = ui.item.value.split("#");
		const textoSinSeparador = partes.join(" ");

		provIdSelected = ui.item.id;
        provDesdSelected = partes[0];

		// Mostrar SIN el "#"
		$("#Rel01").val(textoSinSeparador);
		CargarRepeccionesDelProveedor(provIdSelected);
		$("#Rel01Item").val(ui.item.id);
		event.preventDefault();
		return false;
	}
}).autocomplete("instance")._renderItem = function (ul, item) {

		const partes = item.label.split("#");

		const ctaLista = partes[0];
		const tipoDesc = partes[1];

		return $("<li>")
			.append(
				`<div>
                <span style="font-weight:bold; font-size:14px;">
                    ${ctaLista}
                </span>
                <span style="font-size:13px; color:#555;">
                    ${tipoDesc}
                </span>
            </div>`
			)
			.appendTo(ul);
	};

function CargarRepeccionesDelProveedor(provId) {
	AbrirWaiting("Cargando recepciones del proveedor...")
	var data = { cta_id: provId };
	PostGenHtml(data, cargarRecepcionesDelProveedorUrl, function (obj) {
		CerrarWaiting();
		$("#divListaRecepciones").html(obj);
		const $select = $("#listaRecepciones");
		const opciones = $select.find("option").not('[value=""]'); // excluye "Seleccionar"

		if (opciones.length === 1) {
			const unicoValor = opciones.first().val();
			$select.val(unicoValor).trigger("change");
			rpIdSelected = unicoValor;
		} else {
			rpIdSelected = null;
		}

		$(document).off("change", "#listaRecepciones");
		$(document).on("change", "#listaRecepciones", ControlaChangeListaRecepciones);

		return true
	});
}

function ControlaChangeListaRecepciones() {
    rpIdSelected = $("#listaRecepciones").val();
    PostGen({ rp_compte: rpIdSelected }, obtenerDatosDeRecepcionSeleccionadaUrl, function (obj) {
        if (obj.error) {
			console.error("Error al obtener datos de la recepción seleccionada:", obj.msg);
        } else {
            rpSeleccionada = obj.data;
            console.log("rpSeleccionada", rpSeleccionada);
        }
    });
}

function aplicarMascaraCantidad(selector, decimales) {
    $(selector).inputmask({
        alias: 'numeric',
        groupSeparator: '',
        radixPoint: '.',
        digits: decimales,
        digitsOptional: false,
        allowMinus: false,
        rightAlign: true,
        unmaskAsNumber: true,
        autoGroup: false,
        clearMaskOnLostFocus: false
    });
}

function aplicarMascarasControlRecepcion() {

    $("#tbControlRecepcion tbody tr").each(function () {

        const $fila = $(this);
        const upTipo = $fila.data("up-tipo");

        const $camposEditables = $fila.find(
            ".js-control-cantidad:not(:disabled)"
        );

        $camposEditables.inputmask('remove');

        if (upTipo === "P") {
            aplicarMascaraCantidad($camposEditables, DECIMALES_PESABLES);
        } else {
            aplicarMascaraCantidad($camposEditables, 0);
        }
    });
}


function AbrirModalAgregarProducto() {
    let rp_compte = rpSeleccionada.rp_compte;
    let cta_id = rpSeleccionada.cta_id;
    let dia_movi = rpSeleccionada.dia_movi;
    let tco_id = rpSeleccionada.tco_id_rp;
    let tco_desc = rpSeleccionada.tco_desc_rp;
    let cm_compte = rpSeleccionada.cm_compte_rp;
    AbrirWaiting();
    let datos = { rp_compte, cta_id, dia_movi, tco_id, tco_desc, cm_compte };
    PostGenHtml(datos, obtenerDatosModalAgregarProductoUrl, function (obj) {
        $("#divAgregarProducto").html(obj);
        $('#modalAgregarProducto').modal({
            backdrop: 'static',
        });
        $('#modalAgregarProducto').modal('show');

        const $listaRP = $("#listaRP");

        if ($listaRP.find("option[value!='']").length === 1) {
            $listaRP.val($listaRP.find("option[value!='']").first().val()).trigger("change");
        }
        $("#chkIncluyeRp").prop("checked", true);
        getMaskForMoneyType("#p_cantidad", 0);

        $("#btnBusquedaBase").on("click", function () {
            buscarProducto();
            return true;
        });
        $("#estadoFuncion").on("change", verificaEstado); //este control debe ser insertado el mismo o similar para cada modulo.

        $(document).on("keyup", "#Busqueda", verificaTeclaDeBusqueda);

        CerrarWaiting();
        return true
    });
}


function buscarProducto() {
    AbrirWaiting();
    var _post = busquedaProdBaseUrl;
    var valor = $("#Busqueda").val();
    var validarEstado = true;

    var datos = {};
    if (typeof validarEstado !== 'undefined') {
        datos = { busqueda: valor, validarEstado };
    }
    else {
        datos = { busqueda: valor };
    }

    PostGen(datos, _post, function (obj) {
        if (obj.error === true) {
            CerrarWaiting();
            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                productoBase = null;
                $("#estadoFuncion").val(false);
                $("#btnBusquedaBase").prop("disabled", false);
                $("#msjModal").modal("hide");
                LimpiarCamposEnModalDeAgregarProductoParaValorizar();
                $("#Busqueda").trigger("focus");
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else if (obj.warn === true) {
            CerrarWaiting();
            if (obj.producto.p_id === "0000-0000") {
                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    productoBase = null;
                    $("#estadoFuncion").val(false);
                    $("#btnBusquedaBase").prop("disabled", false);
                    $("#msjModal").modal("hide");
                    LimpiarCamposEnModalDeAgregarProductoParaValorizar();
                    $("#Busqueda").trigger("focus");
                    return true;
                }, false, ["Aceptar"], "error!", null);
            }
            else if (obj.producto.p_id === "NO") {
                if (funcionBusquedaAvanzada === true) {
                    AbrirMensaje("ATENCIÓN", "NO SE ENCONTRO EL PRODUCTO QUE INTENTO BUSCAR. SE ABRIRÁ LA BUSQUEDA AVANZADA.", function () {
                        $("#msjModal").modal("hide");
                        productoBase = null;
                        $("#estadoFuncion").val(false);
                        inicializaBusquedaAvanzada();
                        $("#busquedaModal").modal("toggle");
                        return true;
                    }, false, ["Aceptar"], "error!", null);

                    return true;
                }
                else {
                    AbrirMensaje("ATENCIÓN", "NO SE ENCONTRO EL PRODUCTO QUE INTENTO BUSCAR.", function () {
                        $("#msjModal").modal("hide");
                        LimpiarCamposEnModalDeAgregarProductoParaValorizar();
                        $("#Busqueda").trigger("focus");
                        return true;
                    }, false, ["Aceptar"], "error!", null);

                }
            } else {
                //encontro producto pero hay warning
                AbrirMensaje("ATENCIÓN!", obj.msg, function (resp) {
                    if (resp === "SI") {
                        productoBase = obj.producto;
                        $("#estadoFuncion").val(true);
                        $("#estadoFuncion").trigger("change");
                        $("#msjModal").modal("hide");
                        $("#Busqueda").trigger("focus");
                        return true;
                    }
                    else {
                        //se deniega
                        productoBase = null;
                        $("#estadoFuncion").val(false);
                        $("#btnBusquedaBase").prop("disabled", false);
                        $("#msjModal").modal("hide");
                        $("#Busqueda").trigger("focus");
                        return true;
                    }
                },
                    true, ["Aceptar", "Denegar"], "Warning!", null);
            }
        }
        else {
            //encontro y se presenta
            productoBase = obj.producto;
            $("#estadoFuncion").val(true);
            $("#estadoFuncion").trigger("change");
            return true;
        }
    });
    return true;
}

function LimpiarCamposEnModalDeAgregarProductoParaValorizar() {
    $("#p_id").val("");
    $("#p_desc").val("");
    $("#p_cantidad").val("");
}

function verificaEstado(e) {
    FunctionCallback = null; //inicializo funcion por si tiene alguna funcionalidad asignada.
    var res = $("#estadoFuncion").val();
    CerrarWaiting();
    if (res === "true") {
        if (productoBase.cta_id !== rpSeleccionada.cta_id) { // Producto del proveedor de la RP
            AbrirMensaje("ATENCIÓN", "El producto seleccionado no pertenece al proveedor.", function () {
                $("#msjModal").modal("hide");
                return false;
            }, false, ["Aceptar"], "error!", null);
            return false;
        }
        else if (ExisteProductoEnTabla(productoBase.p_id)) {
            AbrirMensaje("ATENCIÓN", "El producto seleccionado ya existe en la recepción.", function () {
                $("#msjModal").modal("hide");
                return false;
            }, false, ["Aceptar"], "error!", null);
            return false;
        }
        else {
            //traigo la variable productoBase e hidrato componentes
            var prod = productoBase;
            console.log(prod);
            $("#p_id").val(prod.p_id);
            $("#p_desc").val(prod.p_desc);
            $("#estadoFuncion").val(false);
            if (prod.up_id == "07") {
                getMaskForMoneyType("#p_cantidad", 0);
            }
            else {
                getMaskForMoneyType("#p_cantidad", 2);
            }
            $("#p_cantidad").val("");
            $("#Busqueda").val("");
            $("#p_cantidad").trigger("focus");
        }
    }
    return true;
}

function verificaTeclaDeBusqueda(e) {
    if (e.which == "13") {

        $("#btnBusquedaBase").trigger("click");
        $("#btnBusquedaBase").prop("disabled", true);
        return true;

    }
}

function getMaskForMoneyType(selector, decimales) {
    $(selector).inputmask({
        alias: 'numeric',
        groupSeparator: '.',
        radixPoint: ',',
        digits: decimales,
        digitsOptional: false,
        allowMinus: false,
        prefix: '',
        suffix: '',
        min: 0,
        rightAlign: true,
        unmaskAsNumber: true,
        positionCaretOnClick: "lvp",
        onBeforeWrite: function (event, buffer, caretPos, opts) {
            //console.log("event: " + event);
        }
    });
}

function AgregarProducto() {
    //p_cantidad
    var cantidad = $("#p_cantidad").inputmask('unmaskedvalue');
    if (cantidad <= 0) {
        AbrirMensaje("ATENCIÓN", "La cantidad debe ser mayor a 0.", function () {
            $("#msjModal").modal("hide");
            $("#p_cantidad").trigger("focus");
            return true;
        }, false, ["Aceptar"], "error!", null);
    }
    else if ($("#chkIncluyeRp")[0].checked && $("#listaRP").val() == "") {
        AbrirMensaje("ATENCIÓN", "Debe seleccionar un valor para RP.", function () {
            $("#msjModal").modal("hide");
            $("#listaRP").trigger("focus");
            return true;
        }, false, ["Aceptar"], "error!", null);
    }
    else {
        AbrirWaiting("Agregando producto para valorizar...");
        var cta_id = rpSeleccionada.cta_id; //TODO: Probar esto
        var tco_id = rpSeleccionada.tco_id_rp;
        var cm_compte = rpSeleccionada.cm_compte_rp;
        var dia_movi = rpSeleccionada.dia_movi;
        var incluye_rp = $("#chkIncluyeRp")[0].checked;
        var p_id = $("#p_id").val();
        var cantidad = $("#p_cantidad").inputmask('unmaskedvalue');
        var rp_compte = $("#listaRP").val();
        var data = { cta_id, tco_id, cm_compte, dia_movi, rp_compte, p_id, cantidad, incluye_rp };
        PostGen(data, agregarProductoParaValorizarUrl, function (obj) {
            CerrarWaiting();
            if (obj.error === true) {
                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    $("#msjModal").modal("hide");
                    return true;
                }, false, ["Aceptar"], "error!", null);
            }
            else {
                $("#modalAgregarProducto").modal("hide");
                BuscarRecepcionesDelProveedor();
                habilitarConfirmarControlRecepcion();
            }
        });

    }
}

function ExisteProductoEnTabla(pId) {
    return $("#tbControlRecepcion tbody tr.fila-control-recepcion")
        .not("[data-eliminado='true']")
        .filter(function () {
            return String($(this).data("p-id")) === String(pId);
        })
        .length > 0;
}