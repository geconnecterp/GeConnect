var boxIdSeleccionado = "";
$(function () {
	InicializarCamposEnFiltros(false);

	$(document).off("click", "#btnImprimir");
	$(document).on("click", "#btnImprimir", ControlaImprimirSelected);
	$(document).off("click", "#btnCancel");
	$(document).on("click", "#btnCancel", ControlaCancelar);
	$(document).off("change", "#listaDepositos");
	$(document).on("change", "#listaDepositos", ControlalistaDepositosSelected);
	$(document).off("click", "#btnBuscarMov");
	$(document).on("click", "#btnBuscarMov", ControlaBtnBuscarMov);
	//
	$("#btnFiltro").on("click", function () {
		if ($("#divFiltros").hasClass("show")) {
			$("#divFiltros").collapse("hide");
			$("#divDetalle").collapse("show");
		}
		else {
			$("#divFiltros").collapse("show");
			$("#divDetalle").collapse("hide");
		}
	});

	$("#btnBuscar").on("click", function () {
		BuscarinfoBoxes();
	});

	$(document).off("click", "#tbGridBoxes tbody tr");
	$(document).on("click", "#tbGridBoxes tbody tr", function (e) {

		// Evitar que botones o links disparen la selección
		if ($(e.target).is("button, a, .btn, i")) return;

		const $row = $(this);

		// Quitar selección previa
		$("#tbGridBoxes tbody tr").removeClass("selected-row");

		// Marcar fila seleccionada
		$row.addClass("selected-row");

		// Obtener el Box_id desde el data attribute
		const boxId = $row.data("box-id");
		boxIdSeleccionado = boxId;
		// Aquí manejás lo que necesites
		ManejarSeleccionBox(boxId);
	});

});

function ControlaBtnBuscarMov() {
	AbrirWaiting("Obteniendo información de movimientos de stock...");
	console.log("Box seleccionado:", boxIdSeleccionado);
	var sm = $("#listaTipoMovimientos").val();
	if (!sm || sm == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un tipo de movimiento.", function () {
			$("#msjModal").modal("hide");
			$("#listaDepositos").trigger('focus');
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		var desde = $("#FechaDesde").val();
		var hasta = $("#FechaHasta").val();
		var data = {
			boxId: boxIdSeleccionado,
			sm,
			desde,
			hasta
		};
		PostGenHtml(data, obtenerBoxInfoMovStkURL, function (obj) {
			$("#divMov").html(obj);

			CerrarWaiting();
			return true
		});
	}
}

function ManejarSeleccionBox(boxId) {
	AbrirWaiting("Obteniendo información del box...");
	console.log("Box seleccionado:", boxId);
	var data = {
		boxId: boxId
	};
	PostGenHtml(data, obtenerBoxInfoStkURL, function (obj) {
		$("#divStkBox").html(obj);

		CerrarWaiting();
		return true
	});
}


function obtenerFiltrosDeBox() {

	const filtros = {
		Gondola: $("#chkGondola").is(":checked") ? $("#GondolaValue").val() || "%" : "%",
		Nivel: $("#chkNivel").is(":checked") ? $("#NivelValue").val() || "%" : "%",
		Rack: $("#chkRack").is(":checked") ? $("#RackValue").val() || "%" : "%",
		Zona: $("#chkZona").is(":checked") ? $("#ZonaValue").val() || "%" : "%",
		SoloLibre: $("#chkSoloLibre").is(":checked") ? "L" : "%"
	};

	return filtros;
}


function BuscarinfoBoxes() {
	var depo_id = $("#listaDepositos").val();
	if (!depo_id || depo_id == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un Depósito.", function () {
			$("#msjModal").modal("hide");
			$("#listaDepositos").trigger('focus');
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		let filtros = obtenerFiltrosDeBox();
		var gondola = filtros.Gondola;
		var nivel = filtros.Nivel;
		var rack = filtros.Rack;
		var zona = filtros.Zona;
		var sololibre = filtros.SoloLibre;
		var depo_desc = $("#listaDepositos option:selected").text();

		var data = {
			depo_id: depo_id,
			box_gondola: gondola,
			box_nivel: nivel,
			box_rack: rack,
			box_zona: zona,
			boxe_id: sololibre,
			depo_desc
		}
		AbrirWaiting("Buscando información de boxes...");
		PostGenHtml(data, buscarInfoBoxesURL, function (obj) {
			$("#divDetalle").html(obj);
			$("#divFiltros").collapse("hide");
			$("#divDetalle").collapse("show");
			//$("#btnImprimir").show();

			CerrarWaiting();
			viendeDesdeBusquedaDeProducto = false;
			return true
		});
	}
}

function ControlaImprimirSelected() { }

function ControlaCancelar() { }

function ControlalistaDepositosSelected() { }

function padLeftZeros(value, length) {
	value = value.replace(/\D/g, ""); // solo números
	return value.padStart(length, "0");
}

function inicializarFiltrosDeBox() {

	// GondolaValue → 3 dígitos
	$(document).on("blur", "#GondolaValue", function () {
		const val = $(this).val();
		$(this).val(padLeftZeros(val, 3));
	});

	$(document).on("input", "#GondolaValue", function () {
		let val = $(this).val().replace(/\D/g, "");
		if (val.length > 3) val = val.substring(0, 3);
		$(this).val(val);
	});

	// NivelValue → 2 dígitos
	$(document).on("blur", "#NivelValue", function () {
		const val = $(this).val();
		$(this).val(padLeftZeros(val, 2));
	});

	$(document).on("input", "#NivelValue", function () {
		let val = $(this).val().replace(/\D/g, "");
		if (val.length > 2) val = val.substring(0, 2);
		$(this).val(val);
	});

	// RackValue → 3 dígitos
	$(document).on("blur", "#RackValue", function () {
		const val = $(this).val();
		$(this).val(padLeftZeros(val, 3));
	});

	$(document).on("input", "#RackValue", function () {
		let val = $(this).val().replace(/\D/g, "");
		if (val.length > 3) val = val.substring(0, 3);
		$(this).val(val);
	});

	// ZonaValue → 1 dígito
	$(document).on("blur", "#ZonaValue", function () {
		let val = $(this).val().replace(/\D/g, "");
		if (val.length > 1) val = val.substring(0, 1);
		$(this).val(val);
	});

	$(document).on("input", "#ZonaValue", function () {
		let val = $(this).val().replace(/\D/g, "");
		if (val.length > 1) val = val.substring(0, 1);
		$(this).val(val);
	});
}


function InicializarCamposEnFiltros(vieneDeCancelar) {
	if (!vieneDeCancelar) {
	}
	$("#btnImprimir").hide();
	$("#lbDepositos").text("Depósitos");
	$("#lbChkGondola").text("Góndola");
	$("#lbChkNivel").text("Nivel");
	$("#lbChkRack").text("Rack");
	$("#lbChkZona").text("Zona");
	$("#lbChkSoloLibre").text("Solo Libres");

	$("#chkDepositos").prop('checked', true);
	$("#chkDepositos").prop('disabled', true);
	$("#chkDepositos").trigger("change");
	$("#chkGondola").prop('checked', false);
	$("#chkGondola").trigger("change");
	$("#GondolaValue").prop('disabled', true);
	$("#chkNivel").prop('checked', false);
	$("#chkNivel").trigger("change");
	$("#NivelValue").prop('disabled', true);
	$("#chkRack").prop('checked', false);
	$("#chkRack").trigger("change");
	$("#RackValue").prop('disabled', true);
	$("#chkZona").prop('checked', false);
	$("#chkZona").trigger("change");
	$("#ZonaValue").prop('disabled', true);
	$("#chkSoloLibre").prop('checked', false);
	$("#chkSoloLibre").trigger("change");

	$("#listaDepositos").val("");

	$("#divFiltros").collapse("show");
	$("#divDetalle").collapse("hide");
	if (!vieneDeCancelar) {
		HandlerCheckBox();
		inicializarFiltrosDeBox();
	}
}

function HandlerCheckBox() {
	$("#chkDepositos").on("click", function () {
		if ($("#chkDepositos").is(":checked")) {
			$("#listaDepositos").prop("disabled", false);
			$("#listaDepositos").trigger("focus");
		}
		else {
			$("#listaDepositos").prop("disabled", true);
			$("#listaDepositos").val("");
		}
	});
	$("#chkGondola").on("click", function () {
		if ($("#chkGondola").is(":checked")) {
			$("#GondolaValue").prop("disabled", false);
			$("#GondolaValue").trigger("focus");
		}
		else {
			$("#GondolaValue").prop("disabled", true);
			$("#GondolaValue").val("");
		}
	});
	
	$("#chkNivel").on("click", function () {
		if ($("#chkNivel").is(":checked")) {
			$("#NivelValue").prop("disabled", false);
			$("#NivelValue").trigger("focus");
		}
		else {
			$("#NivelValue").prop("disabled", true);
			$("#NivelValue").val("");
		}
	});
	$("#chkRack").on("click", function () {
		if ($("#chkRack").is(":checked")) {
			$("#RackValue").prop("disabled", false);
			$("#RackValue").trigger("focus");
		}
		else {
			$("#RackValue").prop("disabled", true);
			$("#RackValue").val("");
		}
	});
	$("#chkZona").on("click", function () {
		if ($("#chkZona").is(":checked")) {
			$("#ZonaValue").prop("disabled", false);
			$("#ZonaValue").trigger("focus");
		}
		else {
			$("#ZonaValue").prop("disabled", true);
			$("#ZonaValue").val("");
		}
	});
}