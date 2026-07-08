$(function () {
	InicializarEventos();
});

function InicializarEventos() {
	$(document).on("click", "#btnCargar", btnCargarClick);
	$(document).on("click", "#btnConfirmar", btnConfirmarClick);
	$(document).on("click", "#btnCancelar", btnCancelarClick);
	$(document).on("change", "#listaDeposito", listaDepositoChange);
	$(document).on("keyup", "#PtoVta", ControlaKeyUpComptePtoVta);
	$(document).on("focusout", "#PtoVta", ControlaFocusOutComptePtoVta);
	$(document).on("keyup", "#NroComprobante", ControlaKeyUpCompteNro);
	$(document).on("focusout", "#NroComprobante", ControlaFocusOutCompteNro);
	$("#PtoVta").inputmask("9999");
	$("#NroComprobante").inputmask("99999999");
	$(document).ready(function () {
		// Selecciona el primer radio
		$("#DesdeFactura").prop("checked", true);

		// Dispara el evento para aplicar habilitación + limpieza
		$("input[name='TipoRelacion']:checked").trigger("change");
	});
	$(document).on("change", "input[name='TipoRelacion']", function () {

		const tipo = $(this).val();

		// Controles Factura
		const ddlTipo = $("#listaTipoComprobante");
		const txtPtoVta = $("#PtoVta");
		const txtNroComprobante = $("#NroComprobante");

		// Controles Cotización
		const txtNroCotizacion = $("#NroCotizacion");

		// Autocompletar Sin Relación
		const txtAutocompletar = $("#Rel01");
		const hiddenAutocompletar = $("#Rel01Item");

		// Función auxiliar para limpiar
		function limpiarFactura() {
			ddlTipo.val("");              // limpia selección
			txtPtoVta.val("");            // limpia texto
			txtNroComprobante.val("");    // limpia texto
		}

		function limpiarCotizacion() {
			txtNroCotizacion.val("");
		}

		function limpiarSinRelacion() {
			txtAutocompletar.val("");
			hiddenAutocompletar.val("");
		}

		// ============================
		// Estado según radio seleccionado
		// ============================
		if (tipo === "Factura") {

			// Habilitar Factura
			ddlTipo.prop("disabled", false);
			txtPtoVta.prop("disabled", false);
			txtNroComprobante.prop("disabled", false);

			// Deshabilitar Cotización + limpiar
			txtNroCotizacion.prop("disabled", true);
			limpiarCotizacion();

			// Deshabilitar Autocompletar + limpiar
			txtAutocompletar.prop("disabled", true);
			limpiarSinRelacion();
		}

		else if (tipo === "Cotizacion") {

			// Deshabilitar Factura + limpiar
			ddlTipo.prop("disabled", true);
			txtPtoVta.prop("disabled", true);
			txtNroComprobante.prop("disabled", true);
			limpiarFactura();

			// Habilitar Cotización
			txtNroCotizacion.prop("disabled", false);

			// Deshabilitar Autocompletar + limpiar
			txtAutocompletar.prop("disabled", true);
			limpiarSinRelacion();
		}

		else if (tipo === "SinRelacion") {

			// Deshabilitar Factura + limpiar
			ddlTipo.prop("disabled", true);
			txtPtoVta.prop("disabled", true);
			txtNroComprobante.prop("disabled", true);
			limpiarFactura();

			// Deshabilitar Cotización + limpiar
			txtNroCotizacion.prop("disabled", true);
			limpiarCotizacion();

			// Habilitar Autocompletar
			txtAutocompletar.prop("disabled", false);
		}
	});
	CancelarAjuste();
}

function btnCargarClick() {
	let v = ValidarFiltrosParaCarga();

	if (!v.ok) {
		AbrirMensaje("Atención", v.msg, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "warn!", null);
		return;
	}

	// Si todo está OK → habilitar carga
	HabilitarCargaDeProductos();
}

function btnConfirmarClick() {
}

function btnCancelarClick() {
	CancelarAjuste();
}

function CancelarAjuste() {

	// Habilitar sección superior
	$(".row-block input, .row-block select").prop("disabled", false);
	$("input[name='TipoRelacion']").prop("disabled", false);

	// Deshabilitar sección inferior
	$("#ProdID, #ProdUP, #ProdBto, #ProdUnid").prop("disabled", true);
	$("#btnAgregarProducto, #btnQuitarProducto").prop("disabled", true);

	// Botones
	$("#btnCargar").prop("disabled", false);
	$("#btnConfirmar").prop("disabled", true);
	$("#btnCancelar").prop("disabled", true);
}


function HabilitarCargaDeProductos() {

	// Bloquear sección superior
	$(".row-block input, .row-block select").prop("disabled", true);
	$("input[name='TipoRelacion']").prop("disabled", true);

	// Habilitar sección inferior
	$("#ProdID, #ProdUP, #ProdBto, #ProdUnid").prop("disabled", false);
	$("#btnAgregarProducto, #btnQuitarProducto").prop("disabled", false);

	// Botones
	$("#btnCargar").prop("disabled", true);
	$("#btnConfirmar").prop("disabled", false);
	$("#btnCancelar").prop("disabled", false);
}


function ValidarFiltrosParaCarga() {

	let tipo = $("input[name='TipoRelacion']:checked").val();

	// Validaciones según radio
	if (tipo === "Factura") {
		if ($("#PtoVta").val().trim() === "" || $("#NroComprobante").val().trim() === "") {
			return { ok: false, msg: "Debe completar Punto de Venta y Número de Comprobante." };
		}
	}

	if (tipo === "Cotizacion") {
		if ($("#NroCotizacion").val().trim() === "") {
			return { ok: false, msg: "Debe completar el Número de Cotización." };
		}
	}

	if (tipo === "SinRelacion") {
		if ($("#Rel01").val().trim() === "") {
			return { ok: false, msg: "Debe completar el campo de búsqueda de relación." };
		}
	}

	// Validaciones comunes
	if ($("#listaDeposito").val().trim() === "") {
		return { ok: false, msg: "Debe seleccionar un Depósito." };
	}

	if ($("#listaBoxes").val().trim() === "") {
		return { ok: false, msg: "Debe seleccionar un BOX." };
	}

	if ($("#Obs").val().trim() === "") {
		return { ok: false, msg: "Debe completar la Observación." };
	}

	return { ok: true };
}


function ControlaFocusOutComptePtoVta() {
	var ptv = $("#PtoVta").inputmask('unmaskedvalue');
	if (ptv != "") {
		var aux = $("#PtoVta").inputmask('unmaskedvalue').padStart(4, '0');
		$("#PtoVta").val(aux);
		$("#NroComprobante").trigger("focus");
	}
}

function ControlaKeyUpComptePtoVta(e) {
	if (e.which == 13 || e.which == 109) {
		var aux = $("#PtoVta").inputmask('unmaskedvalue').padStart(4, '0');
		$("#PtoVta").val(aux);
		$("#NroComprobante").trigger("focus");
	}
}

function ControlaFocusOutCompteNro() {
	var nro = $("#NroComprobante").inputmask('unmaskedvalue');
	if (nro != "") {
		var aux = $("#NroComprobante").inputmask('unmaskedvalue').padStart(8, '0');
		$("#NroComprobante").val(aux);
		$("#listaDeposito").trigger("focus");
	}
}

function ControlaKeyUpCompteNro(e) {
	if (e.which == 13 || e.which == 109) {
		var aux = $("#NroComprobante").inputmask('unmaskedvalue').padStart(8, '0');
		$("#NroComprobante").val(aux);
		$("#listaDeposito").trigger("focus");
	}
}

function listaDepositoChange() {
	if ($("#listaDeposito").val() == "") {
		BlanquearComboBoxes();
		return false;
	}
	if ($("#listaDeposito").val() == "0") {
		BlanquearComboBoxes();
		return false;
	}
	BuscarBoxDesdeDeposito();
}

function BlanquearComboBoxes() {
	var depoId = "0";
	var datos = { depoId };
	PostGenHtml(datos, BuscarBoxesDesdeDepositoURL, function (obj) {
		$("#divComboBoxes").html(obj);
		$("#listaBoxes").on("change", listaBoxesChange);
		CerrarWaiting();
		return true
	});
}

function listaBoxesChange() {
}

function BuscarBoxDesdeDeposito() {
	AbrirWaiting();
	var depoId = $("#listaDeposito").val();
	var datos = { depoId };
	PostGenHtml(datos, BuscarBoxesDesdeDepositoURL, function (obj) {
		$("#divComboBoxes").html(obj);
		$("#listaBoxes").on("change", listaBoxesChange);
		CerrarWaiting();
		return true
	});
}

$("#Rel01").autocomplete({
	source: function (request, response) {

		data = { prefix: request.term }; Rel01

		$.ajax({
			url: autoComRel01Url,
			type: "POST",
			dataType: "json",
			data: data,
			success: function (obj) {
				response($.map(obj, function (item) {
					var texto = item.descripcion;
					return { label: texto, value: item.descripcion, id: item.id, prov: item.provId };
				}));
			}
		})
	},
	minLength: 3,
	select: function (event, ui) {
		if ($("#Rel01List").has('option:contains("' + ui.item.id + '")').length === 0) {
			$("#Rel01Item").val(ui.item.id);
			var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
			$("#Rel01List").append(opc);
		}
		return true;
	}
});

function VerificarExistenciaDeProductosDesdeComprobantes(datos) {

	PostGen(datos, validarExistenciaDeProdsURL, function (o) {

		CerrarWaiting();

		if (o.error === true) {

			AbrirMensaje("Atención", o.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);

		} else if (o.warn === true) {

			AbrirMensaje("Atención", o.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "warn!", null);

		} else {
			// OK
			//Mostrar mensaje OK Cancel para levantar los productos del comprobante
		}
	});
}
