$(function () {

	InicializarControles();
	$(document).on("click", "#btnAgregar", AbrirModalAgregarAnticipo);
	$(document).on("click", "#btnConfirmar", AgregarAnticipo);
	$(document).on("click", "#btnSalir", CerrarModal);
	//

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
});

function AgregarAnticipo() {
	AbrirWaiting();
	var cta_id = clienteIdSelected;
	var cta_desc = clienteDescSelected;
	var cuotas = $("#cuotas").inputmask('unmaskedvalue');
	var importe = $("#importe").inputmask('unmaskedvalue');
	var intereses = $("#porc_interes").val();
	var data = { cta_id, cta_desc, cuotas, importe, intereses };
	PostGen(data, agregarAnticipoUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			CerrarWaiting();
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);

		}
		else {
			setTimeout(() => {
				$('#modalCargaDeAnticipo').modal('hide');
				ActualizarListaDeAnticipos();
			}, 500);
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
	AbrirWaiting();
	var intereses = $("#porc_interes").inputmask('unmaskedvalue');
	var cta_id = ctaIdSelected;
	var cta_desc = ctaDescSelected;
	var data = { intereses, cta_id, cta_desc };
	PostGenHtml(data, abrirModalAgregarAnticipoUrl, function (obj) {
		$("#divCargaDeAnticipo").html(obj);
		const $modal = $("#modalCargaDeAnticipo");

		$modal.modal({
			backdrop: 'static',
		});

		inicializarCamposEnModal();

		$modal.modal('show');
		CerrarWaiting();

		setTimeout(() => {
			$("#Rel02").trigger('focus');
		}, 100);

		return true
	});
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

	//$ordenFoco.forEach((el, index) => {
	//	el.off("keydown.enterNav").on("keydown.enterNav", function (e) {
	//		if (e.key === "Enter") {
	//			e.preventDefault();
	//			e.stopImmediatePropagation();

	//			const siguiente = $ordenFoco[index + 1];

	//			if (siguiente && siguiente.length && siguiente.is(":visible") && !siguiente.is(":disabled")) {
	//				setTimeout(() => {
	//					siguiente.focus();
	//				}, 10); // ⏱️ Pequeño delay para asegurar render
	//			} else {
	//				// Foco forzado al botón si no se detecta siguiente
	//				setTimeout(() => {
	//					$("#btnConfirmar").focus();
	//				}, 10);
	//			}
	//		}
	//	});
	//});

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