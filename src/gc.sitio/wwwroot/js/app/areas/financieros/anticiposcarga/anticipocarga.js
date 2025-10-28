$(function () {

	InicializarControles();
	$(document).on("click", "#btnAgregar", AbrirModalAgregarAnticipo);
	$(document).on("click", "#btnConfirmar", AgregarAnticipo);
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
}

function eliminarItem(cta_id, cuotas) {
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