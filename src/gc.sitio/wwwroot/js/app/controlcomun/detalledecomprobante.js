/**
 * Detale de Comprobante
 * 
 * Este módulo se encarga de la visualización de información del comprobante seleccionado
 */
var tco_id = "";
var cm_compte = "";
var dia_movi = "";
$(function () {
	$(document).on("comprobanteSeleccionadoParaVisualizar", function (e, data) {
		console.log("Evento captado en componente:", data);

		invocarComponenteDeDetalleDeComprobante({
			tco_id: data.tco_id,
			cm_compte: data.cm_compte,
			dia_movi: data.dia_movi
		});
	});

	// Cerrar popup
	$(document).on("click", "#btnCerrarDivDetalleCompte", function () {
		const el = document.getElementById("divComponenteDetalleComprobante");
		if (el) el.style.display = "none";
	});
});

function restoreDivDetComptePosition() {
	const el = document.getElementById("divComponenteDetalleComprobante");
	if (!el) return;

	const top = localStorage.getItem("divInfoDetalleCompte_top");
	const left = localStorage.getItem("divInfoDetalleCompte_left");

	if (top && left) {
		el.style.top = top;
		el.style.left = left;
	}
}

function invocarComponenteDeDetalleDeComprobante(p) {
	let tco_id = p.tco_id;
	let cm_compte = p.cm_compte;
	let dia_movi = p.dia_movi;
	var data = { tco_id, cm_compte, dia_movi };
	PostGenHtml(data, abrirComponenteDetalleDeComprobanteUrl, function (obj) {
		// Detecta si vino un partial de error
		if (esRespuestaDeError(obj)) {
			console.info(`Error al abrir componente de detalle de comprobante: Tipo: ${tco_id} Comprobante: ${cm_compte} Movimiento: ${dia_movi}`);
			return; // No renderiza nada
		}

		$("#divInfoDetalleDeComprobante").html(obj);

		restoreDivDetComptePosition();
		makeDivDetCompteDraggable();

		$("#divInfoDetalleDeComprobante").collapse("show");

	});
}

function esRespuestaDeError(html) {
	if (!html) return true;

	// Detecta el hidden que siempre viene en los mensajes del backend
	return html.includes('id="msgWarn"') || html.includes('id="msgError"');
}

function makeDivDetCompteDraggable() {
	const el = document.getElementById("divComponenteDetalleComprobante");
	if (!el) return;

	let posX = 0, posY = 0, mouseX = 0, mouseY = 0;

	// Usamos la barra de tabs como “handler”
	//const header = el.querySelector(".nav-tabs");
	//const dragHandle = header || el;
	const dragHandle = document.getElementById("divInfoHeaderDetalleCompte");

	dragHandle.style.cursor = "move";

	dragHandle.onmousedown = dragMouseDown;

	function dragMouseDown(e) {
		e.preventDefault();

		mouseX = e.clientX;
		mouseY = e.clientY;

		document.onmouseup = closeDragElement;
		document.onmousemove = elementDrag;
	}

	function elementDrag(e) {
		e.preventDefault();

		posX = mouseX - e.clientX;
		posY = mouseY - e.clientY;

		mouseX = e.clientX;
		mouseY = e.clientY;

		el.style.top = (el.offsetTop - posY) + "px";
		el.style.left = (el.offsetLeft - posX) + "px";

		// Guardamos posición en localStorage
		localStorage.setItem("divInfoDetalleCompte_top", el.style.top);
		localStorage.setItem("divInfoDetalleCompte_left", el.style.left);
	}

	function closeDragElement() {
		document.onmouseup = null;
		document.onmousemove = null;
	}
}