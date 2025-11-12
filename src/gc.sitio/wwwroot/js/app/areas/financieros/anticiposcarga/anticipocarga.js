$(function () {

	InicializarControles();
	$(document).on("click", "#btnAgregar", AbrirModalAgregarAnticipo);
	$(document).on("click", "#btnConfirmar", AgregarAnticipo);
	$(document).on("click", "#btnSalir", CerrarModal);
	$(document).on("click", "#btnConfirmarCargaDeAnticipo", ConfirmarAnticipos);
	$(document).on("click", "#btnCancelar", CancelarAnticipos);

	$(document).on("keydown.autocomplete", "input#Rel01", function () {
		$(this).autocomplete({
			source: function (request, response) {

				data = { prefix: request.term };

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
				ctaIdSelected = ui.item.id;
				ctaDescSelected = ui.item.value;


				return true;
			}
		});
	});

	let valorOriginal = null;
	$("#porc_interes").on("focus", function () {
		valorOriginal = $(this).val();
	});
	$("#porc_interes").on("blur", function () {
		const valorActual = $(this).val();
		const filas = $("#tbListaAnticipos tbody tr").length;
		if (valorActual !== valorOriginal && filas > 0) {
			console.log("El valor de % Interés ha cambiado:", valorOriginal, "→", valorActual);
			$(this).trigger("valorInteresModificado", [valorOriginal, valorActual]);
		}
	});
	$("#porc_interes").on("valorInteresModificado", function (e, anterior, nuevo) {
		console.log("Cambio detectado:", anterior, "→", nuevo);
		valorInteresModificado(nuevo);
	});


	ctaIdSelected = $("#prov_id_selected").val();
	ctaDescSelected = $("#prov_denominacion_selected").val();
	$("#Rel01").val(`${ctaDescSelected} (${ctaIdSelected})`);
});

function valorInteresModificado(nuevo_interes) {
	AbrirWaiting();
	var data = { nuevo_interes };
	PostGen(data, actualizarInteresDeAnticiposUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			setTimeout(() => {
				ActualizarListaDeAnticipos();
			}, 500);
		}
	});
}

function CancelarAnticipos() {
	var filas = $("#tbListaAnticipos tbody tr").length;
	if (filas > 0) {
		AbrirMensaje("ATENCIÓN", "¿Esta seguro que desea cancelar la carga de anticipos? Se eliminarán todos los anticipos cargados.", function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI":
					handlerCancelarAnticipos();
					break;
				case "NO":
					break;
				default: //NO
					break;
			}
			return true;

		}, true, ["Aceptar", "Cancelar"], "question!", null);

	}
}

function handlerCancelarAnticipos() {
	AbrirWaiting();
	PostGen({}, cancelarAnticiposUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			setTimeout(() => {
				$('#modalCargaDeAnticipo').modal('hide');
				$("#listaTipoAnticipo").val("");
				$("#Concepto").val("");
				$("#porc_interes").val("0");
				$("#Rel01").val("");
				ActualizarListaDeAnticipos();
			}, 500);
		}
	});
}

function ConfirmarAnticipos() {
	var filas = $("#tbListaAnticipos tbody tr").length;
	if (filas > 0) {
		AbrirMensaje("ATENCIÓN", "¿Esta seguro que desea confirmar la carga de anticipos?", function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI":
					handlerConfirmarCargaDeAnticipos();
					break;
				case "NO":
					break;
				default: //NO
					break;
			}
			return true;

		}, true, ["Aceptar", "Cancelar"], "question!", null);

	}
	else {
		AbrirMensaje("ATENCIÓN", "No se han cargado registros de anticipos de empleados.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}

function handlerConfirmarCargaDeAnticipos() {
	AbrirWaiting("Guardando Anticipo...");
	var ant_id = $("#listaTipoAnticipo").val();
	var an_concepto = $("#Concepto").val();
	var an_porc_interes = $("#porc_interes").inputmask('unmaskedvalue');
	var cta_id = ctaIdSelected;
	var data = { ant_id, an_concepto, an_porc_interes, cta_id };
	PostGen(data, confirmarCargaDeAnticipoUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			console.log(obj.id);
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				console.log(obj.id); //Tomar este valor para imprimir.
				ImprimirAnticipo_Generado(obj.id, ctaIdSelected);
				handlerCancelarAnticipos();
				return true;
			}, false, ["Aceptar"], "succ!", null);
		}
	});
}
function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function ImprimirAnticipo_Generado(id, cta_id) {
	ReseteoDeReportes();
	setTimeout(() => {
		let data = { id, ctaId: cta_id };
		cargarReporteEnArre(39, data, "ANTICIPO DE EMPLEADOS", "", "");
		invocacionGestorDoc({});
	}, 500);
}

//############ COMENTAR AL FINALIZAR ############
// Botón de imprimir
$(document).on("click", ".btnImprimir", function () {
	imprimirOPP();
});

$("#btnImprimirTemp").on("click", function () {
	ImprimirAnticipo_Generado("00-00006214", "C0030000");
});

function imprimirOPP() {
	// Invocar gestor documental
	invocacionGestorDoc({});
}
//############ COMENTAR AL FINALIZAR ############

function AgregarAnticipo() {
	AbrirWaiting();
	var cta_id = clienteIdSelected;
	var cta_desc = clienteDescSelected;
	var cuotas = $("#cuotas").inputmask('unmaskedvalue');
	var importe = $("#importe").inputmask('unmaskedvalue');
	var intereses = $("#porc_interes").val();
	var data = { cta_id, cta_desc, cuotas, importe, intereses };
	PostGen(data, agregarAnticipoUrl, function (obj) {
		if (obj.error === true) {
			CerrarWaiting();
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);

		}
		else {
			setTimeout(() => {
				//$('#modalCargaDeAnticipo').modal('hide');
				CerrarWaiting();
				ActualizarListaDeAnticipos();
				limpiarCamposEnModal();
			}, 300);
		}
	});
}

function CerrarModal() {
	$('#modalCargaDeAnticipo').modal('hide');
}

function ActualizarListaDeAnticipos() {
	AbrirWaiting();
	PostGen({}, actualizarListaDeAnticiposUrl, function (obj) {
		$("#divGrillaAnticipos").html(obj);
		CerrarWaiting();
	});
}

function AbrirModalAgregarAnticipo() {
	var intereses = $("#porc_interes").inputmask('unmaskedvalue');
	if (intereses < 0) {
		AbrirMensaje("ATENCIÓN", "Debe establecer un valor para Intereses, mayor o igual a 0.", function () {
			$("#msjModal").modal("hide");
			$("#porc_interes").trigger('focus');
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting();
		var cta_id = ctaIdSelected;
		var cta_desc = ctaDescSelected;
		var data = { intereses, cta_id, cta_desc };
		PostGenHtml(data, abrirModalAgregarAnticipoUrl, function (obj) {
			$("#divCargaDeAnticipo").empty();
			$("#divCargaDeAnticipo").html(obj);
			const $modal = $("#modalCargaDeAnticipo");

			$modal.modal({
				backdrop: 'static',
			});

			inicializarCamposEnModal();

			CerrarWaiting();
			$modal.modal('show');

			setTimeout(() => {
				const $rel02 = $("#Rel02");
				if ($rel02.length > 0) {
					$rel02.trigger("focus");
					console.log("Foco aplicado a #Rel02");
				} else {
					console.warn("No se encontró el input #Rel02");
				}
			}, 500);

			return true
		});
	}
}

function limpiarCamposEnModal() {
	$("#Rel02").val("");
	$("#Rel02Item").empty();
	$("#cuotas").val("1");
	$("#importe").val("0");
	$("#Rel02").trigger("focus");
}

function inicializarCamposEnModal() {
	$("#modalCenterTitle").text("Carga de Anticipo de Empleado");
	getMaskForIntegerMax24("#cuotas");
	getMaskForMoneyType("#importe");
	$(document).on("keydown.autocomplete", "input#Rel02", function () {
		$(this).autocomplete({
			source: function (request, response) {

				data = { prefix: request.term };

				$.ajax({
					url: autoComRel02Url,
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
				clienteIdSelected = ui.item.id;
				clienteDescSelected = ui.item.value;

				return true;
			}
		});
	});

	// 🧠 Lista ordenada manualmente
	const $ordenFoco = [
		$("#Rel02"),
		$("#cuotas"),
		$("#importe"),
		$("#btnConfirmar")
	];

	$("#importe").off("keydown.enterDirect").on("keydown.enterDirect", function (e) {
		if (e.key === "Enter") {
			e.preventDefault();
			e.stopImmediatePropagation(); // 🔒 Evita salto al btn-close

			// Foco directo al botón Confirmar
			$("#btnConfirmar").trigger('focus');
		}
	});

	// 🔘 Enter en botón dispara acción
	$("#btnConfirmar").off("keydown.enterClick").on("keydown.enterClick", function (e) {
		if (e.key === "Enter") {
			e.preventDefault();
			$(this).click();
		}
	});
}

function eliminarItem(id) {
	AbrirMensaje("ATENCIÓN", `¿Esta seguro de eliminar el item Anticipo?`, function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI":
				handlerEliminarItemAnticipo(id);
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;

	}, true, ["Aceptar", "Cancelar"], "question!", null);
}

function handlerEliminarItemAnticipo(id) {
	var data = { id };
	PostGen(data, eliminarItemAnticipoUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			ActualizarListaDeAnticipos();
		}
	});
}

function selectItemGrillaAnticipo(x) {
	$("#tbListaAnticipos tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
}

function InicializarControles() {
	getMaskForIntegerMax1000("#porc_interes");
	$("#listaTipoAnticipo").trigger('focus');

	const $div = $("#divInputs");
	$div.find("input").on("keydown", function (e) {
		if (e.key === "Enter") {
			e.preventDefault();

			const $campos = $div.find("select, input")
				.filter(":visible:enabled");

			const index = $campos.index(this);

			if (index !== -1) {
				if (index < $campos.length - 1) {
					$campos.eq(index + 1).focus();
				} else {
					$div.find("#btnAgregar").focus();
				}
			}
		}
	});
}

function getMaskForMoneyType(selector) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: '.',
		radixPoint: ',',
		digits: 2,
		digitsOptional: false,
		allowMinus: false,
		prefix: '',
		suffix: '',
		rightAlign: true,
		unmaskAsNumber: true
	});
}

function getMaskForIntegerMax24(selector) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: '.',       // separador de miles
		digits: 0,                 // sin decimales
		digitsOptional: false,
		allowMinus: false,
		prefix: '',
		suffix: '',
		rightAlign: true,
		unmaskAsNumber: true,
		min: 1,
		max: 24
	});
}

function getMaskForIntegerMax1000(selector) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: '.',       // separador de miles
		digits: 0,                 // sin decimales
		digitsOptional: false,
		allowMinus: false,
		prefix: '',
		suffix: '',
		rightAlign: true,
		unmaskAsNumber: true,
		min: 0,
		max: 1000
	});
}