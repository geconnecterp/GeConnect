$(function () {
	CerrarWaiting();
	//ValidarTipoTR();
});

function regresarATRLista() {
	window.location.href = TRRegresarAAutorizacionesListaUrl;
}

function ValidarTipoTR() {
	var tipo = $("#tipo").val();
	if (tipo === "S") {
		$('#btnConfirmar').prop('disabled', true);
	}
}

function dblClickTRRow(x) {
	var $fila = $(x);
	var pId = $fila.data('p-id');

	// 🔥 Si la fila YA tiene un detalle justo debajo → cerrarlo
	var $next = $fila.next("tr.tr-detalle");
	if ($next.length > 0) {
		$next.remove();
		return; // ⬅️ Toggle: si estaba abierto, lo cerramos y terminamos
	}

	// 🔥 Cerrar cualquier otro detalle abierto en la tabla
	$("#tbListaConteos tr.tr-detalle").remove();

	// 🔥 Llamar al backend
	PostGenHtml({ p_id: pId }, TIVerProductosUrl, function (html) {

		// Crear fila de detalle
		var detalle = `
            <tr class="tr-detalle">
                <td colspan="9" style="padding:0;">
                    <div class="subtabla-wrapper">
                        ${html}
                    </div>
                </td>
            </tr>
        `;

		// Insertar debajo de la fila clickeada
		$fila.after(detalle);
	});
}


function selectTRRow(x) {
}
function validarTablaConteos() {
	const filas = document.querySelectorAll("#tbListaConteos tbody tr");

	let todasDiferenciasCero = true;
	let existeConteoDistintoCero = false;

	filas.forEach(fila => {
		const diferencia = parseFloat(fila.dataset.diferencia || "0");
		const conteo = parseFloat(fila.dataset.cantidad || "0");

		if (diferencia !== 0) {
			todasDiferenciasCero = false;
		}

		if (conteo !== 0) {
			existeConteoDistintoCero = true;
		}
	});

	if (todasDiferenciasCero && existeConteoDistintoCero) {
		return false;
	}

	return true; // si no se cumple la condición, sigue normal
}
function iniciarValidarAutorizacion() {
	var ti = $("#ti").val();
	if (ti === "") {
		AbrirMensaje("Atención", "La Transferencia seleccionado no es válida.", function () {
			$("#msjModal").modal("hide");
			return false;
		}, false, ["Aceptar"], "error!", null);
	}
	else if (!validarTablaConteos()) {
		AbrirMensaje("Confirmación", "Existen productos no colectados en la Autorización de TR. Si confirma no podrá operar más con esta Autorización de TR. ¿Está seguro de continuar?", function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI": //
					validar(ti);
					break;
				case "NO":
					break;
				default: //NO
					break;
			}
			return true;
		}, true, ["Aceptar", "Cancelar"], "info!", null);
	}
	else {
		validar(ti);
	}
}

function validar(ti) {
	AbrirWaiting();
	var datos = { ti }
	PostGen(datos, TRValidarTransferenciaUrl, function (o) {
		if (o.error === true) {
			CerrarWaiting();
			AbrirMensaje("Atención", o.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		} else if (o.warn === true) {
			CerrarWaiting();
			AbrirMensaje("Atención", o.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "warn!", null);
		} else if (o.msg !== "") {
			CerrarWaiting();
			AbrirMensaje("Atención", o.msg, function (e) {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "info!", null);
		} else {
			CerrarWaiting();
			confirmarAutorizacion(ti);
		}
	});
}

function confirmarAutorizacion(ti) {
	AbrirMensaje("Confirmación", "Desea confirmar?", function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI": //
				AbrirWaiting();
				var datos = { ti }
				PostGen(datos, TRConfirmarAutorizacionUrl, function (o) {
					if (o.error === true) {
						CerrarWaiting();
						AbrirMensaje("Atención", o.msg, function () {
							$("#msjModal").modal("hide");
							return true;
						}, false, ["Aceptar"], "error!", null);
					} else if (o.warn === true) {
						CerrarWaiting();
						AbrirMensaje("Atención", o.msg, function () {
							$("#msjModal").modal("hide");
							return true;
						}, false, ["Aceptar"], "warn!", null);
					} else if (o.msg !== "") {
						CerrarWaiting();
						AbrirMensaje("Atención", o.msg, function (e) {
							$("#msjModal").modal("hide");
							return true;
						}, false, ["Aceptar"], "info!", null);
					} else {
						CerrarWaiting();
						//TODO: Imprimir Remito
						window.location.href = TRRegresarAAutorizacionesListaUrl;
					}
				});
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;
	}, true, ["Aceptar", "Cancelar"], "info!", null);
}
