let provIdSelected = null;
let provDesdSelected = null;
let rpIdSelected = null;
const selectorTablaControl = "#tbControlRecepcion";
const selectorInputControl = "#tbControlRecepcion .js-control-cantidad:not(:disabled)";
const DECIMALES_PESABLES = 2;

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
    $(document)
        .off("keydown.controlRecepcion", "#tbControlRecepcion .js-control-cantidad")
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
					}
	);
	$(document).off("focus.controlRecepcion", "#tbControlRecepcion .js-control-cantidad")
				.on("focus.controlRecepcion", "#tbControlRecepcion .js-control-cantidad",
					function () {
						this.select();
					}
	);
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

        alternarFilasVisiblesControlRecepcion();
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

        const valorActual = obtenerNumeroControlRecepcion($input.val());
        const valorOriginal = obtenerNumeroControlRecepcion(
            $input.attr("data-original-value")
        );

        const fueModificado =
            Math.abs(valorActual - valorOriginal) > 0.0000001;

        $input.toggleClass("campo-modificado", fueModificado);
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

    function calcularCantidadControlRecepcion(
        unidadPresentacion,
        bultos,
        unidadSuelta
    ) {
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

    //function controlarTeclaInputRecepcion(event, input) {
    //    const teclasPermitidas = [
    //        "Backspace",
    //        "Delete",
    //        "Tab",
    //        "Enter",
    //        "Escape",
    //        "ArrowLeft",
    //        "ArrowRight",
    //        "ArrowUp",
    //        "ArrowDown",
    //        "Home",
    //        "End"
    //    ];

    //    if (
    //        event.ctrlKey ||
    //        event.metaKey ||
    //        teclasPermitidas.includes(event.key)
    //    ) {
    //        if (event.key === "Enter") {
    //            event.preventDefault();

    //            moverFocoControlRecepcion(
    //                input,
    //                event.shiftKey ? -1 : 1
    //            );
    //        }

    //        if (event.key === "ArrowDown") {
    //            event.preventDefault();
    //            moverFocoVerticalControlRecepcion(input, 1);
    //        }

    //        if (event.key === "ArrowUp") {
    //            event.preventDefault();
    //            moverFocoVerticalControlRecepcion(input, -1);
    //        }

    //        return;
    //    }

    //    const $input = $(input);
    //    const tipo = $input.data("tipo");
    //    const decimales = parseInt($input.data("decimales"), 10) || 0;
    //    const esDigito = /^\d$/.test(event.key);
    //    const esSeparadorDecimal =
    //        event.key === "." || event.key === ",";

    //    if (esDigito) {
    //        return;
    //    }

    //    if (
    //        tipo === "decimal" &&
    //        decimales > 0 &&
    //        esSeparadorDecimal &&
    //        !String($input.val()).includes(".") &&
    //        !String($input.val()).includes(",")
    //    ) {
    //        return;
    //    }

    //    event.preventDefault();
    //}

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

    function moverFocoVerticalControlRecepcion(
        inputActual,
        desplazamiento
    ) {
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
            $(
                selectorTablaControl +
                " .campo-modificado"
            ).length > 0 ||
            $(
                selectorTablaControl +
                ' tbody tr[data-eliminado="true"]'
            ).length > 0
        );
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
        alternarFilasVisiblesControlRecepcion();
    };
});

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