$(function () {
	$("#btnDelProd").on("click", DelProd);
	$("#btnCargaPrevia").on("click", AbrirCargaPrevia);
	$("#btnRevertirAjuste").on("click", ValidarAjuste);
	$("#btnAddProd").on("click", AgregarProdManual);
	$("#btnConfirmar").on("click", ConfirmarAjuste);
	$("#btnCancelar").on("click", VerificarAntesDeCancelarAjuste);
	$("#txtUP").on("keyup", analizaInputUP);
	$("#txtBto").on("keyup", analizaInputBto);
	$("#txtUnid").on("keyup", analizaInputUnid);
	$("#listaDeposito").on("change", listaDepositoChange);
	$("#listaBox").on("change", listaBoxesChange);
	$("#listaDepositoEnCargaPrevia").on("change", listaDepositoEnCargaPreviaChange);
	$("#listaMotivo").on("change", listalistaMotivoChange);
	$("#btnradioManual").on("click", BtnRadioManual);
	$("#btnradioRevertirAjuste").on("click", BtnRadioRevertirAjuste);
	$("#btnradioCargaPrevia").on("click", BtnRadioCargaPrevia);
	$("#divRevertirAjuste").find('input').each(function () {
		$(this).attr('disabled', 'disabled');
	});
	$("#divRevertirAjuste").find('button').each(function () {
		$(this).attr('disabled', 'disabled');
	});
	$("#divCargaPrevia").find('button').each(function () {
		$(this).attr('disabled', 'disabled');
	});
});

function VerificarAntesDeCancelarAjuste() {
	var datos = {};
	PostGen(datos, VerificaExistenciaDeAjusteDeStockURL, function (o) {
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
			CancelarAjuste();
		}
	});
}

function CancelarAjuste() {
	AbrirWaiting();
	var datos = {};
	PostGenHtml(datos, CancelarAjusteDeStockURL, function (obj) {
		$("#divDetalleDeProductosAAjustar").html(obj);
		AddEventListenerToGrid("tbDetalleDeProductosAAjustar");
		CerrarWaiting();
		return true
	});
}

function ConfirmarAjuste() {
	var hayError = false;
	var nota = $("#txtNota").val();
	if (nota === "") {
		AbrirMensaje("Atención", "Debe especificar una nota antes de confirmar.", function () {
			$("#msjModal").modal("hide");
			$("#txtNota").focus();
			hayError = true;
			return true;
		}, false, ["Aceptar"], "warn!", null);
	}
	var rows = $("#tbDetalleDeProductosAAjustar tr").length;
	if (rows <= 0) {
		AbrirMensaje("Atención", "Debe agregar al menos un producto en el Ajuste de Stock antes de confirmar.", function () {
			$("#msjModal").modal("hide");
			hayError = true;
			return true;
		}, false, ["Aceptar"], "warn!", null);
	}
	var motivo = $("#listaMotivo").val();
	if (motivo === "") {
		AbrirMensaje("Atención", "Debe especificar un 'Tipo' antes de confirmar.", function () {
			$("#msjModal").modal("hide");
			$("#listaMotivo").focus();
			hayError = true;
			return true;
		}, false, ["Aceptar"], "warn!", null);
	}
	motivoSplited = motivo.split('#');
	if (motivoSplited.length !== 2) {
		AbrirMensaje("Atención", "El tipo de ajuste no tiene la configuracion correcta, consulte con el Administrador. Tipo: " + motivoSplited, function () {
			$("#msjModal").modal("hide");
			$("#listaMotivo").focus();
			hayError = true;
			return true;
		}, false, ["Aceptar"], "warn!", null);
	}
	if (!hayError) {
		AbrirWaiting();
		var atId = motivoSplited[0];
		var atTipo = motivoSplited[1];
		var datos = { atId, nota, atTipo }
		PostGen(datos, ConfirmarAjusteDeStockURL, function (o) {
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
				console.log(o.msg);
			}
		});
	}
}

function DelProd() {
	if (pIdSeleccionado === "") {
		AbrirMensaje("Atención", "Debe seleccionar un producto.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "warn!", null);
	}
	var pId = pIdSeleccionado;
	var datos = { pId };
	PostGenHtml(datos, QuitarProductoDeListaURL, function (obj) {
		$('#modalCargaPrevia').modal('hide')
		$("#divDetalleDeProductosAAjustar").html(obj);
		AddEventListenerToGrid("tbDetalleDeProductosAAjustar");
		CerrarWaiting();
		return true
	});
}

function BtnRadioManual() {
	$("#divRevertirAjuste").find('input').each(function () {
		$(this).attr('disabled', 'disabled');
	});
	$("#divRevertirAjuste").find('button').each(function () {
		$(this).attr('disabled', 'disabled');
	});
	$("#divCargaPrevia").find('button').each(function () {
		$(this).attr('disabled', 'disabled');
	});
}

function BtnRadioRevertirAjuste() {
	$("#divRevertirAjuste").find('input').each(function () {
		$(this).removeAttr('disabled');
	});
	$("#divRevertirAjuste").find('button').each(function () {
		$(this).removeAttr('disabled');
	});
	$("#divCargaPrevia").find('button').each(function () {
		$(this).attr('disabled', 'disabled');
	});
}

function BtnRadioCargaPrevia() {
	$("#divRevertirAjuste").find('input').each(function () {
		$(this).attr('disabled', 'disabled');
	});
	$("#divRevertirAjuste").find('button').each(function () {
		$(this).attr('disabled', 'disabled');
	});
	$("#divCargaPrevia").find('button').each(function () {
		$(this).removeAttr('disabled');
	});
}

function analizaInputUP(x) {
	if (x.which == "13") {
		$("#txtBto").focus();
	}
}

function analizaInputBto(x) {
	if (x.which == "13") {
		if ($("#txtUnid").prop('disabled')) {
			$("#btnAddProd").focus();
		}
		else {
			$("#txtUnid").focus();
		}
	}
}

function analizaInputUnid(x) {
	if (x.which == "13") {
		$("#btnAddProd").focus();
	}
}

function seleccionarProductosDesdeCargaPrevia() {
	var depoId = $("#listaDepositoEnCargaPrevia").val();
	var boxId = $("#listaBoxEnCargaPrevia").val();
	var nota = $("#txtNota").val();
	if (depoId == "" || boxId == "") {
		return false;
	}
	var datos = { depoId, boxId };
	AbrirWaiting();
	PostGenHtml(datos, ActaulizarListaProductosDesdeModalCargaPreviaURL, function (obj) {
		$('#modalCargaPrevia').modal('hide')
		$("#divDetalleDeProductosAAjustar").html(obj);
		AddEventListenerToGrid("tbDetalleDeProductosAAjustar");
		CerrarWaiting();
		return true
	});
}

function selectListaProductoRow(x) {
	var pId = x.cells[0].innerText.trim();
	if (pId !== "") {
		pIdSeleccionado = pId;
	}
	else {
		pIdSeleccionado = "";
	}
}

function listaBoxesChange() {
}
function cargarProductos() {
}
function EliminarProducto(id) {
}
function InicializaPantalla() {
}

function analizaEnterInput(e) {
	if (e.which == "13") {
		tope = 99999;
		index = -1;
		//obtengo los inputs dentro del div
		var inputss = $("#divInputs :input:not(:disabled)");
		tope = inputss.length;
		//le el id del input en el que he dado enter
		var cual = $(this).prop("id");
		inputss.each(function (i, item) {
			if ($(item).prop("id") === cual) {
				index = i;
				return false;
			}
		});
		if (index > -1 && tope > index + 1) {
			inputss[index + 1].focus();
		}
	}
	return true;
}

function verificaEstado(e) {
	FunctionCallback = null; //inicializo funcion por si tiene alguna funcionalidad asignada.
	var res = $("#estadoFuncion").val();
	CerrarWaiting();
	if (res === "true") {
		//traigo la variable productoBase e hidrato componentes
		var prod = productoBase;

		$("#txtIdProd").val(prod.p_id);
		$("#txtProDescripcion").val(prod.p_desc);
		$("#estadoFuncion").val(false);
		$("#txtUP_ID").val(prod.up_id);
		$("#txtBARRADO_ID").val(prod.p_id_barrado);
		$("#txtID_PROV").val(prod.p_id_prov);
		$("#txtUP").mask("000.000.000.000", { reverse: true });
		$("txtBto").mask('#,##0', {
			reverse: true,
			translation: {
				'#': {
					pattern: /-|\d/,
					recursive: true
				}
			},
			onChange: function (value, e) {
				e.target.value = value.replace(/(?!^)-/g, '').replace(/^,/, '').replace(/^-,/, '-');
			}
		});
		//$("#txtBto").mask("000.000.000.000", {
		//	reverse: true,
		//	translation: {
		//		'#': {
		//			pattern: /-|\d/,
		//			recursive: true
		//		}
		//	},
		//	onChange: function (value, e) {
		//		e.target.value = value.replace(/(?!^)-/g, '').replace(/^,/, '').replace(/^-,/, '-');
		//	}
		//});
		$("#txtUP").val(prod.p_unidad_pres).prop("disabled", false);
		$("#txtBto").val(prod.bulto).prop("disabled", false);
		$("#txtUnid").mask("000.000.000.000", { reverse: true });

		if (prod.up_id !== "07") {  //unidades enteras
			// $("#box").mask("000.000.000.000,00", { reverse: true });
			$("#txtUnid").mask("000.000.000.000,00", { reverse: true });
			$("#txtUnid").val(0).prop("disabled", false);
		}
		else { //unidades decimales
			//$("#txtUnid").val(0).prop("disabled", true);
		}
		$("#Busqueda").val("");
		if (prod.p_con_vto !== "N") {
		} else {
		}
		$("#txtUP").focus();
	}
	return true;
}

function AgregarProdManual() {
	var ajuste = 0;
	if ($("#listaDeposito").val() == "") {
		AbrirMensaje("Atención", "Debe especificar un valor para depósito.", function () {
			$("#msjModal").modal("hide");
			$("#listaDeposito").focus();
			return true;
		}, false, ["Aceptar"], "warn!", null);
	}
	else if ($("#listaBox").val() == "") {
		AbrirMensaje("Atención", "Debe especificar un valor para box.", function () {
			$("#msjModal").modal("hide");
			$("#listaBox").focus();
			return true;
		}, false, ["Aceptar"], "warn!", null);
	}
	else {
		if ($("#txtUnid").prop('disabled')) {
			if ($("#txtUP").val() === "") {
				AbrirMensaje("Atención", "Debe especificar un valor para UP.", function () {
					$("#msjModal").modal("hide");
					$("#txtUP").focus();
					return true;
				}, false, ["Aceptar"], "warn!", null);
			}
			if ($("#txtBto").val() === "") {
				AbrirMensaje("Atención", "Debe especificar un valor para Bto.", function () {
					$("#msjModal").modal("hide");
					$("#txtBto").focus();
					return true;
				}, false, ["Aceptar"], "warn!", null);
			}
			ajuste = $("#txtUP").val() * $("#txtBto").val();
			if (tipoMotivoSeleccionado === "B" && ajuste > 0) {
				ajuste = ajuste * -1;
			}
		}
		else {
			if ($("#txtUnid").val() === "") {
				AbrirMensaje("Atención", "Debe especificar un valor para Unid.", function () {
					$("#msjModal").modal("hide");
					$("#txtUnid").focus();
					return true;
				}, false, ["Aceptar"], "warn!", null);
			}
			ajuste = $("#txtUnid").val();
			if (tipoMotivoSeleccionado === "B" && ajuste > 0) {
				ajuste = ajuste * -1;
			}
		}
		if (ajuste !== 0) {
			AbrirWaiting();
			var pId = $("#txtIdProd").val();
			var boxId = $("#listaBox").val();
			var depoId = $("#listaDeposito").val();
			var atId = $("#listaMotivo").val();
			var us = $("#txtUnid").val();
			var bto = $("#txtBto").val();
			var unidadPres = $("#txtUP").val();
			var upId = $("#txtUP_ID").val();
			var datos = { pId, boxId, depoId, atId, us, bto, unidadPres, upId }
			PostGenHtml(datos, AgregarProductoAListaURL, function (obj) {
				$("#divDetalleDeProductosAAjustar").html(obj);
				AddEventListenerToGrid("tbDetalleDeProductosAAjustar");
				CerrarWaiting();
				return true
			});
			CerrarWaiting();
		}
	}
}

function ValidarAjuste() {
	var ajId = $("#txtNroAjuste").val();
	if (ajId === "") {
		AbrirMensaje("Atención", "Debe ingresar un ID de Ajuste.", function () {
			$("#msjModal").modal("hide");
			$("#txtNroAjuste").focus();
			return true;
		}, false, ["Aceptar"], "warn!", null);
	}
	AbrirWaiting();
	var datos = { ajId }
	PostGen(datos, ValidarNroDeAjusteARevertirURL, function (o) {
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
			RevertirAjuste(ajId);
		}
	});
}

function RevertirAjuste(ajId) {
	AbrirWaiting();
	var nota = $("#txtNota").val();
	var datos = { ajId }
	PostGenHtml(datos, ObtenerProductosDesdeAJRevertidoURL, function (obj) {
		$("#divDetalleDeProductosAAjustar").html(obj);
		AddEventListenerToGrid("tbDetalleDeProductosAAjustar");
		if ($("#tbDetalleDeProductosAAjustar tr")[1] !== undefined && $("#tbDetalleDeProductosAAjustar tr")[1] !== null)
			ObtenerNotaDesdeProductosRevertidos($("#tbDetalleDeProductosAAjustar tr")[1].children[7].innerText);
		CerrarWaiting();
		return true
	});
	CerrarWaiting();
}

function ObtenerNotaDesdeProductosRevertidos(nota) {
	$("#txtNota").val("Revertido " + nota);
}

function listalistaMotivoChange() {
	var motivoSelected = $("#listaMotivo").val();
	if (motivoSelected != "") {
		motivoSelected = motivoSelected.split('#');
		if (motivoSelected.length == 2) {
			tipoMotivoSeleccionado = motivoSelected[1];
		}
		else {
			tipoMotivoSeleccionado = "";
		}
	}
	else {
		tipoMotivoSeleccionado = "";
	}
	//M -> Permite valores (+) y (-)
	//B -> Solo valores (-) 
	console.log(tipoMotivoSeleccionado);
}

function AbrirCargaPrevia() {
	AbrirWaiting();
	var datos = {};
	PostGenHtml(datos, ObtenerDatosModalCargaPreviaUrl, function (obj) {
		$("#divModalCargaPrevia").html(obj);
		AddEventListenerToGrid("tbListaProductosParaAgregar");
		$("#listaDepositoEnCargaPrevia").on("change", listaDepositoEnCargaPreviaChange);
		$('#modalCargaPrevia').modal('show')
		CerrarWaiting();
		return true
	});
	CerrarWaiting();
}

function listaDepositoChange() {
	BuscarBoxDesdeDeposito();
}

function listaBoxEnCargaPreviaChange() {
	var depoId = $("#listaDepositoEnCargaPrevia").val();
	var boxId = $("#listaBoxEnCargaPrevia").val();
	if (depoId == "" || boxId == "") {
		return false;
	}
	var datos = { depoId, boxId };
	PostGenHtml(datos, FiltrarProductosModalCargaPreviaURL, function (obj) {
		$("#divListaProductosParaAgregar").html(obj);
		$("#listaBox").on("change", listaBoxesChange);
		CerrarWaiting();
		return true
	});
}

function BuscarBoxDesdeDeposito() {
	AbrirWaiting();
	var depoId = $("#listaDeposito").val();
	var datos = { depoId };
	PostGenHtml(datos, BuscarBoxesDesdeDepositoURL, function (obj) {
		$("#divComboBoxes").html(obj);
		$("#listaBox").on("change", listaBoxesChange);
		CerrarWaiting();
		return true
	});
}

function listaDepositoEnCargaPreviaChange() {
	AbrirWaiting();
	var depoId = $("#listaDepositoEnCargaPrevia").val();
	var datos = { depoId };
	PostGenHtml(datos, ObtenerBoxesDesdeDepositoDesdeCargaPreviaURL, function (obj) {
		$("#divComboBoxesEnCargaPrevia").html(obj);
		$("#listaBoxEnCargaPrevia").on("change", listaBoxEnCargaPreviaChange);
		CerrarWaiting();
		return true
	});
	var boxId = "";
	var datos = { depoId, boxId };
	PostGenHtml(datos, FiltrarProductosModalCargaPreviaURL, function (obj) {
		$("#divListaProductosParaAgregar").html(obj);
		$("#listaBox").on("change", listaBoxesChange);
		CerrarWaiting();
		return true
	});
}

function AddEventListenerToGrid(tabla) {
	var grilla = document.getElementById(tabla);
	if (grilla) {
		document.getElementById(tabla).addEventListener('click', function (e) {
			if (e.target.nodeName === 'TD') {
				var selectedRow = this.querySelector('.selected-row');
				if (selectedRow) {
					selectedRow.classList.remove('selected-row');
				}
				e.target.closest('tr').classList.add('selected-row');
			}
		});
	}
}