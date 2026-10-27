let provIdSelected = null;
let provDesdSelected = null;


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
		BuscarValorizacionesRegistradas();
	});
});

function BuscarValorizacionesRegistradas() {
	let cta_id = provIdSelected;
	let desde = $("#FechaDesde").val();
	let hasta = $("#FechaHasta").val();
	let cta_desc = provDesdSelected;
	if (!cta_id || cta_id == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un Proveedor.", function () {
			$("#msjModal").modal("hide");
			$("#Rel01").trigger('focus');
			return false;
		}, false, ["Aceptar"], "error!", null);
	}
	else if (!ValidarFechas()) {
		return false;
	}
	else {
		AbrirWaiting("Cargando Valorizaciones Registradas...")
		var data = { cta_id, cta_desc, desde, hasta };
		PostGenHtml(data, initPantallaPrincipalUrl, function (obj) {
			CerrarWaiting();
			$("#divDetalle").html(obj);
			$("#divFiltro").collapse("hide");
			$("#divDetalle").collapse("show");

			inicializarTabValorizaciones(cta_id, cta_desc, desde, hasta);
			
			return true
		});
		console.log("BuscarRecepcionesDelProveedor");
	}
}

function inicializarTabValorizaciones(cta_id, cta_desc, desde, hasta) {
	AbrirWaiting("Cargando Valorizaciones Registradas...")
	var data = { cta_id, cta_desc, desde, hasta };
	PostGenHtml(data, cargarListaValorizacionesUrl, function (obj) {
		CerrarWaiting();
		$("#divValorizaciones").html(obj);
		InicializarHandlerTBListaValorizacion();
		let $fila = $("#tbListaValorizacion tbody tr").not(".fila-vacia").first();
		if ($fila.length > 0) {
			ProcesarSeleccionFilaEnTBListaValorizacion($fila);
		}
		return true
	});
}

function InicializarHandlerTBListaValorizacion() {
	$(document).off("click", "#tbListaValorizacion tbody tr");
	$(document).on("click", "#tbListaValorizacion tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnTBListaValorizacion($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnTBListaValorizacion($fila) {
	$("#tbListaValorizacion tbody tr").removeClass("selected-row");
	$fila.addClass("selected-row");

	const tcoId = $fila.data("tco-id");
	const cmCompte = $fila.data("cm-compte");
	const diaMovi = $fila.data("dia-movi");
	const ctaId = $fila.data("cta-id");
	const comprobante = $fila.data("comprobante");

	BuscarComprobantesDeValorizacion(tcoId, cmCompte, diaMovi, ctaId);
	BuscarDetalleRpr(tcoId, cmCompte, diaMovi, ctaId, comprobante);
}

function BuscarComprobantesDeValorizacion(tcoId, cmCompte, diaMovi, ctaId) {
	AbrirWaiting("Cargando Comprobantes de Valorización...")
	var data = { tco_id: tcoId, cm_compte: cmCompte, dia_movi: diaMovi, cta_id: ctaId };
	PostGenHtml(data, cargarListaComprobantesDeValorizacionUrl, function (obj) {
		CerrarWaiting();
		$("#divComprobantes").html(obj);
		InicializarHandlerTBListaComptesDeValorizacion();
		return true
	});
}

function InicializarHandlerTBListaComptesDeValorizacion() {
	$(document).off("click", "#tbListaComptesDeValorizacion tbody tr");
	$(document).on("click", "#tbListaComptesDeValorizacion tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnTBListaComptesDeValorizacion($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnTBListaComptesDeValorizacion($fila) {
	$("#tbListaComptesDeValorizacion tbody tr").removeClass("selected-row");
	$fila.addClass("selected-row");
}

function BuscarDetalleRpr(tcoId, cmCompte, diaMovi, ctaId, comprobante) {
	AbrirWaiting("Cargando Comprobantes de Valorización...")
	var data = { tco_id: tcoId, cm_compte: cmCompte, dia_movi: diaMovi, cta_id: ctaId, leyenda: comprobante };
	PostGenHtml(data, cargarListaDetalleRPRUrl, function (obj) {
		CerrarWaiting();
		$("#divDetalleRpr").html(obj);
		InicializarHandlerTBListaDetalleRpr();
		return true
	});
}

function InicializarHandlerTBListaDetalleRpr() {
	$(document).off("click", "#tbListaDetalleRpr tbody tr");
	$(document).on("click", "#tbListaDetalleRpr tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnTBListaDetalleRpr($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnTBListaDetalleRpr($fila) {
	$("#tbListaDetalleRpr tbody tr").removeClass("selected-row");
	$fila.addClass("selected-row");
}

function selectListaValorizacion(x) { }

function selectCompteDeValorizacion(x) { }	

function dblClickCompteDeValorizacion(x) { }

function dblClickListaValorizacion(row) {
}

function selectListaDetalleRpr(x) { }

function esValido(valor) {
	return valor !== null &&
		valor !== undefined &&
		valor !== "" &&
		!(typeof valor === "string" && valor.trim() === "");
}

function procesarDobleClickValorizacion(tcoId, cmCompte, diaMovi) {
	//Cargar comprobantes

}

function ControlaCancelar() {
	$("input#Rel01").trigger('click');
	
	provIdSelected = null;
	provDesdSelected = null;

	SetearFechasPorDefecto();

	$("#Rel01").trigger('focus');
}

function formatDateInput(date) {
	const year = date.getFullYear();
	const month = String(date.getMonth() + 1).padStart(2, "0");
	const day = String(date.getDate()).padStart(2, "0");

	return `${year}-${month}-${day}`;
}

function SetearFechasPorDefecto() {
	const hoy = new Date();

	const fechaDesde = new Date(hoy);
	fechaDesde.setMonth(fechaDesde.getMonth() - 1);

	$("#FechaDesde").val(formatDateInput(fechaDesde));
	$("#FechaHasta").val(formatDateInput(hoy));
}

function ValidarFechas() {

	const fechaDesdeStr = $("#FechaDesde").val();
	const fechaHastaStr = $("#FechaHasta").val();

	if (!fechaDesdeStr || !fechaHastaStr) {
		AbrirMensaje("ATENCIÓN", "Debe indicar ambas fechas.", function () {
			$("#msjModal").modal("hide");
			$("#FechaDesde").trigger('focus');
			return false;
		}, false, ["Aceptar"], "error!", null);
		return false;
	}

	const fechaDesde = new Date(fechaDesdeStr);
	const fechaHasta = new Date(fechaHastaStr);

	if (isNaN(fechaDesde.getTime()) || isNaN(fechaHasta.getTime())) {
		AbrirMensaje("ATENCIÓN", "Las fechas ingresadas no son válidas.", function () {
			$("#msjModal").modal("hide");
			$("#FechaDesde").trigger('focus');
			return false;
		}, false, ["Aceptar"], "error!", null);
		return false;
	}

	// Evita problemas por horas/minutos
	fechaDesde.setHours(0, 0, 0, 0);
	fechaHasta.setHours(0, 0, 0, 0);

	const hoy = new Date();
	hoy.setHours(0, 0, 0, 0);

	// Desde <= Hasta
	if (fechaDesde > fechaHasta) {
		AbrirMensaje("ATENCIÓN", "La fecha Desde no puede ser mayor a la fecha Hasta.", function () {
			$("#msjModal").modal("hide");
			$("#FechaDesde").trigger('focus');
			return false;
		}, false, ["Aceptar"], "error!", null);
		return false;
	}

	// Ninguna puede superar hoy
	if (fechaDesde > hoy || fechaHasta > hoy) {
		AbrirMensaje("ATENCIÓN", "Las fechas no pueden ser mayores a la fecha actual.", function () {
			$("#msjModal").modal("hide");
			$("#FechaDesde").trigger('focus');
			return false;
		}, false, ["Aceptar"], "error!", null);
		return false;
	}

	// Máximo 1 mes calendario
	const fechaMaxima = agregarUnMes(fechaDesde);

	if (fechaHasta > fechaMaxima) {
		AbrirMensaje("ATENCIÓN", "El rango entre fechas no puede superar un mes.", function () {
			$("#msjModal").modal("hide");
			$("#FechaDesde").trigger('focus');
			return false;
		}, false, ["Aceptar"], "error!", null);
		return false;
	}

	return true;
}

function agregarUnMes(fecha) {

	const resultado = new Date(fecha);

	const diaOriginal = resultado.getDate();

	resultado.setDate(1);
	resultado.setMonth(resultado.getMonth() + 1);

	const ultimoDiaMes = new Date(
		resultado.getFullYear(),
		resultado.getMonth() + 1,
		0
	).getDate();

	resultado.setDate(Math.min(diaOriginal, ultimoDiaMes));

	return resultado;
}

function InicializarCamposEnFiltros() {
	$("#btnImprimir").hide();

	$("#chkRel01").prop('checked', true);
	$("#chkRel01").trigger("change");
	$("#chkRel01").prop("disabled", true);

	$("#lbRel01").text("Proveedor");
	$("#lbChkDesdeHasta").text("Rango de fechas");

	$("#chkDesdeHasta").prop('checked', true);
	$("#chkDesdeHasta").trigger("change");
	$("#chkDesdeHasta").prop("disabled", true);

	SetearFechasPorDefecto();

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