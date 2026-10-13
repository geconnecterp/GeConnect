let desdeProdElaborado = false;
let bloqueDestino = null;

$(function () {
    InicializarEventos();
    if ($("#divDetalle").is(":visible")) {
        ModificacionesLocales();
    }
});

function InicializarEventos() {
    // Delegación para ambos botones con ID duplicado
    $(document).on("click", "#btnBusquedaBase", function () {

        // Detectar el bloque padre
        const bloque = $(this).closest(".grid-golden");

        let origen = "";

        if (bloque.find("strong").text().includes("Elaborados")) {
            origen = "elaborados";
        } else if (bloque.find("strong").text().includes("Materia Prima")) {
            origen = "materiaPrima";
        }

        console.log("Botón presionado desde:", origen);

        // Obtener el input asociado
        const inputBusqueda = bloque.find("input[asp-for='Busqueda']");
        console.log("Valor ingresado:", inputBusqueda.val());

        // Lógica según origen
        if (origen === "elaborados") {
			desdeProdElaborado = true;
		} else {
			desdeProdElaborado = false;
		}
		// Guardamos el bloque destino
		bloqueDestino = bloque;
    });
    // Handler para el input de búsqueda del componente común
    $(document).on("keydown", "input[name='Busqueda']", function (e) {
        const bloque = $(this).closest(".grid-golden");

        let origen = "";

        if (bloque.find("strong").text().includes("Elaborados")) {
            origen = "elaborados";
            desdeProdElaborado = true;
        } else {
            origen = "materiaPrima";
            desdeProdElaborado = false;
        }

        console.log("ENTER en input desde:", origen);

        bloqueDestino = bloque;

        if (e.key === "Enter") {
            e.preventDefault();
            buscarProducto();
        }
    });


    $(document).off("change", "#listaDeposito");
    $(document).on("change", "#listaDeposito", listaDepositoChange);

    $(document).on("keyup", "#txtUP", function (e) {
        analizaInputUP(e, $(this));
    });
    $(document).on("keyup", "#txtBto", function (e) {
        analizaInputBto(e, $(this));
    });
    $(document).on("keyup", "#txtUnid", function (e) {
        analizaInputUnid(e, $(this));
    });
}

function analizaInputUP(e, $input) {
    if (e.key === "Enter" || e.which === 13) {
        e.preventDefault();
        e.stopPropagation();

        const bloque = $input.closest(".grid-golden");
        bloque.find("#txtBto").trigger("focus");
    }
}

function analizaInputBto(e, $input) {
    if (e.key === "Enter" || e.which === 13) {
        e.preventDefault();
        e.stopPropagation();

        const bloque = $input.closest(".grid-golden");

        if (bloque.find("#txtUnid").prop("disabled")) {
            bloque.find("#btnAddProd").trigger("focus");
        } else {
            bloque.find("#txtUnid").trigger("focus");
        }
    }
}


function analizaInputUnid(e, $input) {
    if (e.key === "Enter" || e.which === 13) {
        e.preventDefault();
        e.stopPropagation();

        const bloque = $input.closest(".grid-golden");
        bloque.find("#btnAddProd").trigger("focus");
    }
}


function listaDepositoChange() {
    BuscarBoxDesdeDeposito();
}

function listaBoxesChange() {
}

function BuscarBoxDesdeDeposito() {
    AbrirWaiting();
    var depoId = $("#listaDeposito").val();
    var datos = { depoId };
    PostGenHtml(datos, BuscarBoxesDesdeDepositoURL, function (obj) {
        $("#divComboBox").html(obj);
        const $lista = $("#listaBox");
        const opciones = $lista.find("option");

        // Si solo hay 1 opción real (excluyendo "Seleccionar...")
        if (opciones.length === 2) {
            // opciones[0] = "Seleccionar..."
            // opciones[1] = único box
            const unicoValor = opciones.eq(1).val();
            $lista.val(unicoValor).trigger("change");
        }

        $lista.on("change", listaBoxesChange);
        CerrarWaiting();
        return true
    });
}

function ModificacionesLocales() {
    // Quitar texto del botón Buscar
    $("#divTablaProductosElaborados").find("#btnBusquedaBase").each(function () {
        $(this).contents().filter(function () {
            return this.nodeType === 3; // nodos de texto
        }).remove();
    });
    $("#divTablaProductosMateriaPrima").find("#btnBusquedaBase").each(function () {
        $(this).contents().filter(function () {
            return this.nodeType === 3; // nodos de texto
        }).remove();
    });
    $("#divDetalle .filter-golden .input-group-text").each(function () {
        if ($(this).text().trim() === "ID:") {
            $(this).hide(); // Oculta y NO ocupa espacio
        }
    });
}

function OcultarSpanID() {
    
}

async function verificaEstado(e) {
	FunctionCallback = null; //inicializo funcion por si tiene alguna funcionalidad asignada.
	var res = $("#estadoFuncion").val();
	CerrarWaiting();
	if (res === "true") {
		//traigo la variable productoBase e hidrato componentes
		var prod = productoBase;

		let tipo = $("input[name='TipoRelacion']:checked").val();
		if (tipo === "Factura" || tipo === "Cotizacion") {
			const ok = await ValidarExistenciaDeProducto(prod.p_id);
			if (!ok) return;   // corta el flujo
			// seguir con la carga
			ProcesarCargaProducto(prod);
		}
		ProcesarCargaProducto(prod);
	}
	return true;
}

function ProcesarCargaProducto(prod) {

	// Si no hay bloque destino, no podemos continuar
	if (!bloqueDestino) {
		console.warn("No hay bloque destino definido.");
		return;
	}

	// Buscar los inputs dentro del bloque destino
	const $idProd = bloqueDestino.find("#txtIdProd");
	const $desc = bloqueDestino.find("#txtProDescripcion");
	const $up = bloqueDestino.find("#txtUP");
	const $bto = bloqueDestino.find("#txtBto");
	const $unid = bloqueDestino.find("#txtUnid");
	const $upId = bloqueDestino.find("#txtUP_ID");
	const $barradoId = bloqueDestino.find("#txtBARRADO_ID");
	const $provId = bloqueDestino.find("#txtID_PROV");

	// Hidratar valores
	$idProd.val(prod.p_id);
	$desc.val(prod.p_desc);
	$upId.val(prod.up_id);
	$barradoId.val(prod.p_id_barrado);
	$provId.val(prod.p_id_prov);

	// Máscaras
	$up.mask("000.000.000.000", { reverse: true });

	$bto.mask('#,##0', {
		reverse: true,
		translation: {
			'#': { pattern: /-|\d/, recursive: true }
		},
		onChange: function (value, e) {
			e.target.value = value.replace(/(?!^)-/g, '').replace(/^,/, '').replace(/^-,/, '-');
		}
	});

	// Cargar valores
	$up.val(prod.p_unidad_pres);
	$bto.val(prod.bulto);

	// Lógica según UP_ID
	if (prod.up_id === "07") {
		getMaskForInteger($unid);
		$unid.val(0).prop("disabled", false);
		$up.prop("disabled", false);
		$bto.prop("disabled", false);
		$up.trigger("focus");
	} else {
		$up.prop("disabled", true);
		$bto.prop("disabled", true);
		getMaskForTwoDecimals($unid);
		$unid.trigger("focus");
	}

	// Limpiar búsqueda del componente común
	bloqueDestino.find("input[asp-for='Busqueda']").val("");
}

function cargarProductos() {
}

function InicializaPantalla() {
}

function EliminarProducto(id) {
}

function getMaskForInteger(selector) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: ',',       // separador de miles
		digits: 0,                 // sin decimales
		digitsOptional: false,
		allowMinus: false,
		prefix: '',
		suffix: '',
		rightAlign: true,
		unmaskAsNumber: true,
		min: 0
	});
}

function getMaskForTwoDecimals(selector) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: ',',
		radixPoint: '.',
		digits: 2,
		digitsOptional: false,
		allowMinus: false,
		prefix: '',
		suffix: '',
		rightAlign: true,
		unmaskAsNumber: true
	});
}

function buscarProducto() {

    if (!bloqueDestino) {
        console.warn("No hay bloque destino definido.");
        return;
    }

    AbrirWaiting();

    var _post = busquedaProdBaseUrl;

    // Obtener el input de búsqueda del bloque correcto
    var inputBusqueda = bloqueDestino.find("input[name='Busqueda']");
    var valor = inputBusqueda.val();

    var validarEstado = false;
    var datos = { busqueda: valor, validarEstado };

    // Limpiar tabla del bloque correspondiente
    bloqueDestino.find("#tbGridProd").empty();

    PostGen(datos, _post, function (obj) {

        // ERROR
        if (obj.error === true) {
            CerrarWaiting();
            AbrirMensaje("ATENCIÓN", obj.msg, function () {

                productoBase = null;

                bloqueDestino.find("#estadoFuncion").val(false);
                bloqueDestino.find("#btnBusquedaBase").prop("disabled", false);

                $("#msjModal").modal("hide");
                inputBusqueda.trigger('focus');

                return true;
            }, false, ["Aceptar"], "error!", null);

            return;
        }

        // WARNING
        if (obj.warn === true) {

            CerrarWaiting();

            if (obj.producto.p_id === "0000-0000") {
                AbrirMensaje("ATENCIÓN", obj.msg, function () {

                    productoBase = null;

                    bloqueDestino.find("#estadoFuncion").val(false);
                    bloqueDestino.find("#btnBusquedaBase").prop("disabled", false);

                    $("#msjModal").modal("hide");
                    inputBusqueda.trigger('focus');

                    return true;
                }, false, ["Aceptar"], "error!", null);

                return;
            }

            if (obj.producto.p_id === "NO" && valor != "") {

                if (funcionBusquedaAvanzada === true) {

                    AbrirMensaje("ATENCIÓN",
                        "NO SE ENCONTRO EL PRODUCTO QUE INTENTO BUSCAR. SE ABRIRÁ LA BUSQUEDA AVANZADA.",
                        function () {

                            $("#msjModal").modal("hide");

                            productoBase = null;
                            bloqueDestino.find("#estadoFuncion").val(false);

                            inicializaBusquedaAvanzada();
                            $("#busquedaModal").modal("toggle");

                            return true;
                        },
                        false, ["Aceptar"], "error!", null
                    );

                    return;
                }

                AbrirMensaje("ATENCIÓN",
                    "NO SE ENCONTRO EL PRODUCTO QUE INTENTO BUSCAR.",
                    function () {
                        $("#msjModal").modal("hide");
                        inputBusqueda.trigger('focus');
                        return true;
                    },
                    false, ["Aceptar"], "error!", null
                );

                return;
            }

            if (obj.producto.p_id === "NO" && valor == "") {

                if (funcionBusquedaAvanzada === true) {
                    productoBase = null;
                    bloqueDestino.find("#estadoFuncion").val(false);

                    inicializaBusquedaAvanzada();
                    $("#busquedaModal").modal("toggle");
                }

                return;
            }

            // Encontró producto pero con warning
            AbrirMensaje("ATENCIÓN!", obj.msg, function (resp) {

                if (resp === "SI") {

                    productoBase = obj.producto;

                    bloqueDestino.find("#estadoFuncion").val(true).trigger("change");

                    $("#msjModal").modal("hide");

                    ProcesarCargaProducto(prod, bloqueDestino);

                    return true;
                }

                // Denegado
                productoBase = null;

                bloqueDestino.find("#estadoFuncion").val(false);
                bloqueDestino.find("#btnBusquedaBase").prop("disabled", false);

                $("#msjModal").modal("hide");
                inputBusqueda.trigger('focus');

                return true;

            }, true, ["Aceptar", "Denegar"], "Warning!", null);

            return;
        }

        CerrarWaiting();
        // OK — producto encontrado
        productoBase = obj.producto;

        bloqueDestino.find("#estadoFuncion").val(true).trigger("change");

        ProcesarCargaProducto(productoBase, bloqueDestino);

        return true;
    });

    return true;
}
