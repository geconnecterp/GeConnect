$(function () {
	$(document).on("click", "#btnSiguiente1", btnSiguiente1Validar);
	$(document).on("click", "#btnSiguiente2", btnSiguiente2Validar);
	$(document).on("click", "#btnAbmCancelar1", btnAbmCancelar1Validar);
	$(document).on("click", "#btnAnterior2", btnAnterior2Validar);
	$(document).on("click", "#btnAnterior3", btnAnterior3Validar);
	$(document).on('change', 'tbValores input[type="checkbox"]', function () {
		ActualizarTotalSeleccionado();
	});

});



function btnAbmCancelar1Validar() {

}

function btnSiguiente1Validar() {
	var tipo = $("#listaTipoMedioPago").val();
	if (tipo && tipo != "") {
		var tcf_id = tipo;
		tipoMedioPagoSelected = tipo;
		var data = { tcf_id };
		PostGenHtml(data, seleccionCuentaFinUrl, function (obj) {
			$("#divPrincipal").html(obj);
		});
	}
	else {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un Tipo de Medio de Pago.", function () {
			$("#msjModal").modal("hide");
			$("#listaTipoMedioPago").trigger("focus");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}

function selectReg(x, gridId) {
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
	if (gridId == "tbCtaFin") {
		ctafIdSelected = x.childNodes[1].innerText;
		ctafDescSelected = x.childNodes[3].innerText;
	}
}

function btnSiguiente2Validar() {
	if (ctafIdSelected == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar una cuenta financiera.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		var ctaf_id = ctafIdSelected;
		var ctaf_desc = ctafDescSelected;
		var data = { ctaf_id, ctaf_desc };
		PostGenHtml(data, seleccionValoresAPresentarUrl, function (obj) {
			$("#divPrincipal").html(obj);
			AgregarHandlerAGrillaPresDeValores();
		});
	}
}

const formatter = new Intl.NumberFormat('de-DE', {
	minimumFractionDigits: 2,
	maximumFractionDigits: 2
});

function ActualizarTotalSeleccionado() {
	var count = 0;
	var dataTable = document.getElementById('tbValores');
	var checkItems = dataTable.querySelectorAll('input[type="checkbox"]');
	if (checkItems) {
		checkItems.forEach(function (input) {
			if (input.checked) {
				count++;
			}
		});
		console.log(count);
	}
	var listaProd = "";
	var total = 0;
	$("#tbValores").find('tr').each(function (i, el) {
		var td = $(this).find('td');
		if (td.eq(0)[0]) {
			if (td.eq(6)[0].children[0].checked) {
				total = total + Number(td.eq(5).text().replace(',', ''));
			}
		}
	});
	$("#total").val(formatter.format(total));
}

function AgregarHandlerAGrillaPresDeValores() {
	var dataTable = document.getElementById('tbValores');
	var checkItAll = dataTable.querySelector('input[name="select_all"]');
	var inputs = dataTable.querySelectorAll('tbody>tr>td>input');
	checkItAll.addEventListener('change', function () {
		if (checkItAll.checked) {
			inputs.forEach(function (input) {
				input.checked = true;
			});
		}
		else {
			inputs.forEach(function (input) {
				input.checked = false;
			});
		}
	});
	//dataTable.addEventListener(
	//$(document).on('change', 'tbValores input[type="checkbox"]', function () {
	//	ActualizarTotalSeleccionado();
	//});
	var checkItems = dataTable.querySelectorAll('input[type="checkbox"]');
	if (checkItems) {
		checkItems.forEach(function (input) {
			input.addEventListener('change', function () {
				ActualizarTotalSeleccionado();
			});
		});
		//checkItems.addEventListener('change', function () {
		//	ActualizarTotalSeleccionado();
		//});
	}
		/*
		$(document).on('change', 'tbValores input[type="checkbox"]', function () {
		ActualizarTotalSeleccionado();
	});
		*/
}

function btnAnterior2Validar() {
	var data = {};
	PostGenHtml(data, paso1Url, function (obj) {
		$("#divPrincipal").html(obj);
	});
}

function btnAnterior3Validar() {
	var tcf_id = tipoMedioPagoSelected;
	var data = { tcf_id };
	PostGenHtml(data, seleccionCuentaFinUrl, function (obj) {
		$("#divPrincipal").html(obj);
		ctafIdSelected = "";
		ctafDescSelected = "";
	});
}