$(function () {
	$("#btnDelProd").on("click", DelProd);
	$("#btnradioManual").on("click", BtnRadioManual);
	$("#btnCargaPreviaDP").on("click", AbrirCargaPrevia);
	$("#btnradioRevertirDevolucion").on("click", BtnRadioRevertirDevolucion);
	$("#btnradioCargaPrevia").on("click", BtnRadioCargaPrevia);
	$("#btnRevertirDevolucion").on("click", ValidarDevolucion);
	$("#listaDeposito").on("change", listaDepositoChange);
	$("#listaBox").on("change", listaBoxesChange);
	$("#txtUP").on("keyup", analizaInputUP);
	$("#txtBto").on("keyup", analizaInputBto);
	$("#txtUnid").on("keyup", analizaInputUnid);
	$("#btnAddProd").on("click", AgregarProdManual);
	$("#btnCancelar").on("click", CancelarDevolucion);
	$("#btnConfirmar").on("click", ConfirmarDevolucion);
	//btnConfirmar

	$("#divRevertirDevolucion").find('input').each(function () {
		$(this).attr('disabled', 'disabled');
	});
	$("#divRevertirDevolucion").find('button').each(function () {
		$(this).attr('disabled', 'disabled');
	});
	$("#divCargaPrevia").find('button').each(function () {
		$(this).attr('disabled', 'disabled');
	});
});

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
		$("#divDetalleDeProductosADevolver").html(obj);
		AddEventListenerToGrid("tbDetalleDeProductosADevolver");
		CerrarWaiting();
		return true
	});
}

function ConfirmarDevolucion() {
	if ($("#txtNota").val() == "") {
		AbrirMensaje("Atención", "Debe ingresar una 'Nota' antes de confirmar la devolución.", function () {
			$("#msjModal").modal("hide");
			$("#txtNota").focus();
			return true;
		}, false, ["Aceptar"], "warn!", null);
	}
	else {
		ValidarExistenciaDeProductosCargadosParaDevolucion(true);
	}
}

function CancelarDevolucion() {
	ValidarExistenciaDeProductosCargadosParaDevolucion(false);
}

function ValidarExistenciaDeProductosCargadosParaDevolucion(confirma) {
	AbrirWaiting();
	var esConfirmación = confirma;
	var datos = { esConfirmación };
	PostGen(datos, ValidarExistenciaDeProductosCargadosParaDevolucionURL, function (o) {
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
			AbrirMensaje("Atención", o.msg, function (e) {
				$("#msjModal").modal("hide");
				switch (e) {
					case "SI":
						AbrirWaiting();
						if (!confirma) {
							var datos = {};
							PostGen(datos, LimpiarDatosCargadosParaDevolucionURL, function (o) {
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
									$("#Cuenta").val("");
									$("#razonsocial").val("");
									$("#tbDetalleDeProductosADevolver tbody tr").remove();
									$("#txtNota").val("");
								}
							});
						}
						else {
							var nota = $("#txtNota").val();
							var datos = { nota };
							PostGen(datos, ConfirmarDevolucionURL, function (o) {
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
									$("#Cuenta").val("");
									$("#razonsocial").val("");
									$("#tbDetalleDeProductosADevolver tbody tr").remove();
									$("#txtNota").val("");
								}
							});
						}
						break;
					case "NO":
						return true;
						break;
					default: //NO
						break;
				}
				return true;
			}, true, ["Aceptar", "Cancelar"], "info!", null);
		}
	});
}

function BtnRadioManual() {
	$("#divRevertirDevolucion").find('input').each(function () {
		$(this).attr('disabled', 'disabled');
	});
	$("#divRevertirDevolucion").find('button').each(function () {
		$(this).attr('disabled', 'disabled');
	});
	$("#divCargaPrevia").find('button').each(function () {
		$(this).attr('disabled', 'disabled');
	});
}

function BtnRadioRevertirDevolucion() {
	$("#divRevertirDevolucion").find('input').each(function () {
		$(this).removeAttr('disabled');
	});
	$("#divRevertirDevolucion").find('button').each(function () {
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
	$("#divRevertirDevolucion").find('button').each(function () {
		$(this).attr('disabled', 'disabled');
	});
	$("#divCargaPrevia").find('button').each(function () {
		$(this).removeAttr('disabled');
	});
}

function listaDepositoChange() {
	BuscarBoxDesdeDeposito();
}

function listaBoxesChange() {
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

function AbrirCargaPrevia() {
	var ctaId = $("#Cuenta").val();
	if (ctaId === "") {
		AbrirMensaje("Atención", "Debe ingresar una cuenta válida.", function () {
			$("#msjModal").modal("hide");
			$("#Cuenta").focus();
			return true;
		}, false, ["Aceptar"], "warn!", null);
	}
	else {
		AbrirWaiting();
		var datos = { ctaId };
		PostGenHtml(datos, ObtenerDatosModalCargaPreviaUrl, function (obj) {
			$("#divModalCargaPreviaDP").html(obj);
			AddEventListenerToGrid("tbListaProductosParaAgregar");
			$("#listaDepositoEnCargaPrevia").on("change", listaDepositoEnCargaPreviaChange);
			$('#modalCargaPreviaDP').modal('show')
			CerrarWaiting();
			return true
		});
		CerrarWaiting();
	}
}

function ValidarDevolucion() {
	var dpId = $("#txtNroDevolucion").val();
	if (dpId === "") {
		AbrirMensaje("Atención", "Debe ingresar un ID de Devolución.", function () {
			$("#msjModal").modal("hide");
			$("#txtNroDevolucion").focus();
			return true;
		}, false, ["Aceptar"], "warn!", null);
	}
	var ctaId = $("#Cuenta").val();
	if ($("#Cuenta").val() == "") {
		AbrirMensaje("Atención", "Debe seleccionar una cuenta válida.", function () {
			$("#msjModal").modal("hide");
			$("#Cuenta").focus();
			return true;
		}, false, ["Aceptar"], "warn!", null);
	}
	AbrirWaiting();
	var datos = { dpId, ctaId }
	PostGen(datos, ValidarNroDeDevARevertirURL, function (o) {
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
			RevertirDevolucion(dpId);
		}
	});
}

function RevertirDevolucion(dpId) {
	AbrirWaiting();
	var nota = $("#txtNota").val();
	var datos = { dpId }
	PostGenHtml(datos, ObtenerProductosDesdeDPRevertidoURL, function (obj) {
		$("#divDetalleDeProductosADevolver").html(obj);
		AddEventListenerToGrid("tbDetalleDeProductosADevolver");
		if ($("#tbDetalleDeProductosADevolver tr")[1] !== undefined && $("#tbDetalleDeProductosADevolver tr")[1] !== null)
			ObtenerNotaDesdeProductosRevertidos($("#tbDetalleDeProductosADevolver tr")[1].children[7].innerText);
		CerrarWaiting();
		return true
	});
	CerrarWaiting();
}

function ObtenerNotaDesdeProductosRevertidos(nota) {
	$("#txtNota").val("Revertido " + nota);
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

function cargarProductos() {
}

function EliminarProducto(id) {
}

function InicializaPantalla() {
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

function ValidarPertenenciaDeProductoAProveedor(pId, ctaId) {
	var datos = { pId, ctaId }
	PostGen(datos, ValidarPertenenciaDeProductoAProveedorURL, function (o) {
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
		} else {
			var boxId = $("#listaBox").val();
			var depoId = $("#listaDeposito").val();
			var us = $("#txtUnid").val();
			var bto = $("#txtBto").val();
			var unidadPres = $("#txtUP").val();
			var upId = $("#txtUP_ID").val();
			var datos = { pId, boxId, ctaId, depoId, us, bto, unidadPres, upId }
			PostGenHtml(datos, AgregarProductoAListaURL, function (obj) {
				$("#divDetalleDeProductosADevolver").html(obj);
				AddEventListenerToGrid("tbDetalleDeProductosADevolver");
				CerrarWaiting();
				return true
			});
			CerrarWaiting();
		}
	});
}

//function AbrirCargaPrevia() {
//	AbrirWaiting();
//	var datos = {};
//	PostGenHtml(datos, ObtenerDatosModalCargaPreviaUrl, function (obj) {
//		$("#divModalCargaPrevia").html(obj);
//		AddEventListenerToGrid("tbListaProductosParaAgregar");
//		$("#listaDepositoEnCargaPrevia").on("change", listaDepositoEnCargaPreviaChange);
//		$('#modalCargaPrevia').modal('show')
//		CerrarWaiting();
//		return true
//	});
//	CerrarWaiting();
//}

function AgregarHandlerAGrillaDetalleDeProductosEnModal() {
	var dataTable = document.getElementById('tbListaProductosParaAgregar');
	var checkItAll = dataTable.querySelector('input[name="select_all"]');
	var inputs = dataTable.querySelectorAll('tbody>tr>td>input');

	if (checkItAll != null) {
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
	}
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
		AgregarHandlerAGrillaDetalleDeProductosEnModal();
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
		AgregarHandlerAGrillaDetalleDeProductosEnModal();
		CerrarWaiting();
		return true
	});
}

function seleccionarProductosDesdeCargaPrevia() {
	var ids = ObtenerIdsDeProdSeleccionadosEnModal();
	if (ids.length == 0) {
		AbrirMensaje("Atención", "Debe seleccionar al menos un producto.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "warn!", null);
		return false;
	}
	var depoId = $("#listaDepositoEnCargaPrevia").val();
	var boxId = $("#listaBoxEnCargaPrevia").val();
	var nota = $("#txtNota").val();
	if (depoId == "" || boxId == "") {
		return false;
	}
	var datos = { depoId, boxId, ids };
	AbrirWaiting();
	PostGenHtml(datos, ActualizarListaProductosDesdeModalCargaPreviaURL, function (obj) {
		$('#modalCargaPreviaDP').modal('hide')
		$("#divDetalleDeProductosADevolver").html(obj);
		AddEventListenerToGrid("tbDetalleDeProductosADevolver");
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

function ObtenerIdsDeProdSeleccionadosEnModal() {
	var ids = [];
	$("#tbListaProductosParaAgregar").find('tr').each(function (i, el) {
		var td = $(this).find('td');
		if (td.eq(0)[0]) {
			if (td.eq(0)[0].children[0].checked)
				ids.push(td.eq(1).text());
		}
	});
	return ids;
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
	else if ($("#Cuenta").val() == "") {
		AbrirMensaje("Atención", "Debe especificar un valor para cuenta.", function () {
			$("#msjModal").modal("hide");
			$("#Cuenta").focus();
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
		}
		if (ajuste !== 0) {
			var pId = $("#txtIdProd").val();
			var ctaId = $("#Cuenta").val();
			AbrirWaiting();
			ValidarPertenenciaDeProductoAProveedor(pId, ctaId)
		}
	}
}