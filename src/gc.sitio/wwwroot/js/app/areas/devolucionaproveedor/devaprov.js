$(function () {
	$("#btnDelProd").off("click", DelProd);
	$("#btnDelProd").on("click", DelProd);
	$("#btnradioManual").off("click", BtnRadioManual);
	$("#btnradioManual").on("click", BtnRadioManual);
	$("#btnCargaPreviaDP").off("click", AbrirCargaPrevia);
	$("#btnCargaPreviaDP").on("click", AbrirCargaPrevia);
	$("#btnradioRevertirDevolucion").off("click", BtnRadioRevertirDevolucion);
	$("#btnradioRevertirDevolucion").on("click", BtnRadioRevertirDevolucion);
	$("#btnradioCargaPrevia").off("click", BtnRadioCargaPrevia);
	$("#btnradioCargaPrevia").on("click", BtnRadioCargaPrevia);
	$("#btnRevertirDevolucion").off("click", ValidarDevolucion);
	$("#btnRevertirDevolucion").on("click", ValidarDevolucion);
	$("#listaDeposito").off("change", listaDepositoChange);
	$("#listaDeposito").on("change", listaDepositoChange);
	$("#listaBox").off("change", listaBoxesChange);
	$("#listaBox").on("change", listaBoxesChange);
	$("#txtUP").off("keyup", analizaInputUP);
	$("#txtUP").on("keyup", analizaInputUP);
	$("#txtBto").off("keyup", analizaInputBto);
	$("#txtBto").on("keyup", analizaInputBto);
	$("#txtUnid").off("keyup", analizaInputUnid);
	$("#txtUnid").on("keyup", analizaInputUnid);
	$("#btnAddProd").off("click", AgregarProdManual);
	$("#btnAddProd").on("click", AgregarProdManual);
	$("#btnCancelar").off("click", CancelarDevolucion);
	$("#btnCancelar").on("click", CancelarDevolucion);
	$("#btnConfirmar").off("click", ConfirmarDevolucion);
	$("#btnConfirmar").on("click", ConfirmarDevolucion);
	$("#btnBusquedaBase").prop("disabled", false);

	$("#divRevertirDevolucion").find('input').each(function () {
		$(this).attr('disabled', 'disabled');
	});
	$("#divRevertirDevolucion").find('button').each(function () {
		$(this).attr('disabled', 'disabled');
	});
	$("#divCargaPrevia").find('button').each(function () {
		$(this).attr('disabled', 'disabled');
	});
	document.addEventListener('cc:cuentaChanged', function (e) {
		console.log('Razón social detectada desde evento externo:', e.detail.cta_denominacion, e.detail.cta_id);
		if (e.detail.cta_id != undefined) {
			provUnico = true;
			provId = e.detail.cta_id;
			provDesc = e.detail.cta_denominacion;
		}
		else {
			provUnico = false;
			provId = "";
			provDesc = "";
		}
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

function LimpiarCamposDeProducto() {
	$("#txtIdProd").val("");
	$("#txtProDescripcion").val("");
	$("#txtUP").val("");
	$("#txtBto").val("");
	$("#txtUnid").val("");
}

function quitarProducto(p_id) {
	console.log(p_id);
	if (p_id === "") {
		AbrirMensaje("Atención", "Debe seleccionar un producto.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "warn!", null);
	}
	else {
		var pId = p_id;
		var datos = { pId };
		PostGenHtml(datos, QuitarProductoDeListaURL, function (obj) {
			$('#modalCargaPrevia').modal('hide')
			$("#divDetalleDeProductosADevolver").html(obj);
			AddEventListenerToGrid("tbDetalleDeProductosAAjustar");
			CerrarWaiting();
			return true
		});
	}
}

function ConfirmarDevolucion() {
	if ($("#txtNota").val() == "") {
		AbrirMensaje("Atención", "Debe ingresar una 'Nota' antes de confirmar la devolución.", function () {
			$("#msjModal").modal("hide");
			$("#txtNota").trigger('focus');
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

									$("#Rel05").val("");
									$("#Rel05Item").val("");
									$("#razonsocial").val("");
									$("#txtNroDevolucion").val("");
									$("#tbDetalleDeProductosADevolver tbody tr").remove();
									$('#listaDeposito').val("");
									$('#listaBox').val("");
									$("#txtNota").val("");
									LimpiarCamposDeProducto();
									HabilitarDeshabilitarControlesParaDevolucionRevertido(false);
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
									console.log(o.jsonstring);
									AbrirMensaje("Atención", "La devolución se ha registrado con éxito.", function () {
										$("#msjModal").modal("hide");
										return true;
									}, false, ["Aceptar"], "succ!", null);
									$("#Rel05").val("");
									$("#listaDeposito").val("");
									$("#listaBox").val("");
									$("#tbDetalleDeProductosADevolver tbody tr").remove();
									$("#txtNota").val("");
									$("#txtNroDevolucion").val("");
									ImprimirDV_Generado(o.id);
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

function LimpiarCamposDeProducto() {
	$("#txtIdProd").val("");
	$("#txtProDescripcion").val("");
	$("#txtUP").val("");
	$("#txtBto").val("");
	$("#txtUnid").val("");
	$("#txtUP_ID").val("");
	$("#txtBARRADO_ID").val("");
	$("#txtID_PROV").val("");
	$("#btnBusquedaBase").prop("disabled", false);
	$("#Busqueda").val("");
	$("#Busqueda").trigger('focus');
}

function ImprimirDV_Generado(id) {
	ReseteoDeReportes();
	setTimeout(() => {
		let data = { id: id, sm_tipo: "DV" };
		cargarReporteEnArre(101, data, "DEVOLUCIÓN A PROVEEDOR", "", "");
		cargarReporteEnArre(102, data, "REMITO NO FISCAL", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
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

function listaBoxesChange() {
}

function BlanquearComboBoxes() {
	var depoId = "0";
	var datos = { depoId };
	PostGenHtml(datos, BuscarBoxesDesdeDepositoURL, function (obj) {
		$("#divComboBoxes").html(obj);
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
	var hayError = false;
	var dpId = $("#txtNroDevolucion").val();
	if (dpId === "") {
		hayError = true;
		AbrirMensaje("Atención", "Debe ingresar un ID de Devolución.", function () {
			$("#msjModal").modal("hide");
			$("#txtNroDevolucion").trigger('focus');
			return true;
		}, false, ["Aceptar"], "warn!", null);
	}
	//var ctaId = $("#Rel05Item").val();
	//if (ctaId == "") {
	//	hayError = true;
	//	AbrirMensaje("Atención", "Debe seleccionar una cuenta válida.", function () {
	//		$("#msjModal").modal("hide");
	//		$("#Rel05").trigger('focus');
	//		return true;
	//	}, false, ["Aceptar"], "warn!", null);
	//}
	if (!hayError) {
		AbrirWaiting();
		var datos = { dpId }
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
}

function RevertirDevolucion(dpId) {
	AbrirWaiting();
	var nota = $("#txtNota").val();
	var datos = { dpId }
	PostGenHtml(datos, ObtenerProductosDesdeDPRevertidoURL, function (obj) {
		$("#divDetalleDeProductosADevolver").html(obj);
		AddEventListenerToGrid("tbDetalleDeProductosADevolver");
		if ($("#tbDetalleDeProductosADevolver tbody tr").length > 0) {
			const $fila = $("#tbDetalleDeProductosADevolver tbody tr").first();
			const ctaId = $fila.data("cta-id");
			const motivo = $fila.data("dv-motivo");
			const depoId = $fila.data("depo-id");
			const boxId = $fila.data("box-id");

			// Seleccionar depósito (esto dispara la carga de boxes)
			$("#listaDeposito").val(depoId).trigger("change");

			// Esperar a que los boxes se carguen y luego seleccionar el correcto
			seleccionarBoxCuandoEsteListo(boxId, true);

			cargarAutocompleteRel05ConValor(ctaId);

			$("#txtNota").val("Revertido " + motivo);
		}

		if ($("#tbDetalleDeProductosADevolver tr")[1] !== undefined && $("#tbDetalleDeProductosADevolver tr")[1] !== null)
			ObtenerNotaDesdeProductosRevertidos($("#tbDetalleDeProductosADevolver tr")[1].children[7].innerText);

		HabilitarDeshabilitarControlesParaDevolucionRevertido(true);
		CerrarWaiting();
		return true
	});
	CerrarWaiting();
}

function seleccionarBoxCuandoEsteListo(boxId, deshabilitado = false) {
	const interval = setInterval(() => {
		const $box = $("#listaBox");
		if ($box.length > 0 && $box.find(`option[value='${boxId}']`).length > 0) {
			$box.val(boxId).trigger("change");
			clearInterval(interval);
		}
		$("#listaBox").prop("disabled", deshabilitado);
	}, 150);
}

function cargarAutocompleteRel05ConValor(valor) {

	// Setear el valor en el input
	$("#Rel05").val(valor);

	// Disparar la búsqueda del autocomplete
	$("#Rel05").autocomplete("search", valor);

	// Esperar a que el autocomplete reciba el resultado
	$("#Rel05").on("autocompleteresponse", function (event, ui) {

		// Si devuelve un solo registro → seleccionarlo automáticamente
		if (ui.content && ui.content.length === 1) {

			const item = ui.content[0];

			// Ejecutar manualmente el select del autocomplete
			$("#Rel05").data("autocomplete-selected", true);

			provIdSeleccionado = item.id;
			provDescSeleccionado = item.value;

			$("#Rel05Item").val(item.id);
			$("#Rel05").val(item.value);

			// Cerrar el menú del autocomplete
			$("#Rel05").autocomplete("close");
		}
	});
}


function HabilitarDeshabilitarControlesParaDevolucionRevertido(value) {
	$("#listaDeposito").prop("disabled", value);
	$("#listaBox").prop("disabled", value);
	$("#Rel05").prop("disabled", value);
	$("#txtNota").prop("disabled", value);
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
		$("#txtBto").mask('#,##0', {
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

		$("#txtUP").val(prod.p_unidad_pres);
		$("#txtBto").val(prod.bulto);

		if (prod.up_id === "07") {  //unidades enteras
			getMaskForInteger("#txtUnid");
			$("#txtUnid").val(0).prop("disabled", false);
			$("#txtUP").prop('disabled', false);
			$("#txtBto").prop('disabled', false);
			$("#txtBto").trigger('focus');
		}
		else { //unidades decimales
			$("#txtUP").prop('disabled', true);
			$("#txtBto").prop('disabled', true);
			getMaskForTwoDecimals("#txtUnid");
			$("#txtUnid").trigger('focus');
		}
		$("#Busqueda").val("");
		if (prod.p_con_vto !== "N") {
		} else {
		}
	}
	return true;
}

//function analizaEnterInput(e) {
//	if (e.which == "13") {
//		tope = 99999;
//		index = -1;
//		//obtengo los inputs dentro del div
//		var inputss = $("#divInputs :input:not(:disabled)");
//		tope = inputss.length;
//		//le el id del input en el que he dado enter
//		var cual = $(this).prop("id");
//		inputss.each(function (i, item) {
//			if ($(item).prop("id") === cual) {
//				index = i;
//				return false;
//			}
//		});
//		if (index > -1 && tope > index + 1) {
//			inputss[index + 1].focus();
//		}
//	}
//	return true;
//}

function cargarProductos() {
}

function EliminarProducto(id) {
}

function InicializaPantalla() {
}

function analizaInputUP(x) {
	if (x.key === "Enter" || x.which == "13") {
		x.preventDefault();
		x.stopPropagation();
		$("#txtBto").trigger('focus');
	}
}

function analizaInputBto(x) {
	if (x.key === "Enter" || x.which == "13") {
		x.preventDefault();
		x.stopPropagation();
		if ($("#txtUnid").prop('disabled')) {
			$("#btnAddProd").trigger('focus');
		}
		else {
			$("#txtUnid").trigger('focus');
		}
	}
}

function analizaInputUnid(x) {
	if (x.key === "Enter" || x.which == "13") {
		x.preventDefault();
		x.stopPropagation();
		$("#btnAddProd").trigger('focus');
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
				LimpiarCamposDeProducto();
				AddEventListenerToGrid("tbDetalleDeProductosADevolver");
				CerrarWaiting();
				return true
			}, function (xhr) {

				if (xhr.status === 422) {

					// 🔥 Mensaje Golden
					AbrirMensaje(
						"ATENCIÓN",
						xhr.responseText,
						function () {
							$("#msjModal").modal("hide");
							return true;
						},
						false,
						["Aceptar"],
						"error!",
						null
					);

					CerrarWaiting();
					return;
				}

				// Otros errores
				AbrirMensaje(
					"ERROR",
					"Se produjo un error inesperado al intentar agregar el producto.",
					function () {
						$("#msjModal").modal("hide");
						return true;
					},
					false,
					["Aceptar"],
					"error!",
					null
				);

				CerrarWaiting();
			});
			CerrarWaiting();
		}
	});
}

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
			$("#listaDeposito").trigger('focus');
			return true;
		}, false, ["Aceptar"], "warn!", null);
	}
	else if ($("#listaBox").val() == "") {
		AbrirMensaje("Atención", "Debe especificar un valor para box.", function () {
			$("#msjModal").modal("hide");
			$("#listaBox").trigger('focus');
			return true;
		}, false, ["Aceptar"], "warn!", null);
	}
	else if ($("#Rel05").val() == "") {
		AbrirMensaje("Atención", "Debe especificar una cuenta válida.", function () {
			$("#msjModal").modal("hide");
			$("#Rel05").trigger('focus');
			return true;
		}, false, ["Aceptar"], "warn!", null);
	}
	else {
		if ($("#txtUnid").prop('disabled')) {
			if ($("#txtUP").val() === "") {
				AbrirMensaje("Atención", "Debe especificar un valor para UP.", function () {
					$("#msjModal").modal("hide");
					$("#txtUP").trigger('focus');
					return true;
				}, false, ["Aceptar"], "warn!", null);
			}
			if ($("#txtBto").val() === "") {
				AbrirMensaje("Atención", "Debe especificar un valor para Bto.", function () {
					$("#msjModal").modal("hide");
					$("#txtBto").trigger('focus');
					return true;
				}, false, ["Aceptar"], "warn!", null);
			}
			ajuste = $("#txtUP").val() * $("#txtBto").val();
		}
		else {
			if ($("#txtUnid").val() === "") {
				AbrirMensaje("Atención", "Debe especificar un valor para Unid.", function () {
					$("#msjModal").modal("hide");
					$("#txtUnid").trigger('focus');
					return true;
				}, false, ["Aceptar"], "warn!", null);
			}
			ajuste = $("#txtUnid").val();
		}
		if (ajuste !== 0) {
			var pId = $("#txtIdProd").val();
			var ctaId = $("#Rel05Item").val();
			AbrirWaiting();
			ValidarPertenenciaDeProductoAProveedor(pId, ctaId)
		}
	}
}

$("#Rel05").on("focus", function () {
	if (!$(this).data("autocomplete-selected")) {
		$(this).val("");
	}
});

$("#Rel05").on("keyup", function () {
	$(this).data("autocomplete-selected", false);
});

$("#Rel05").autocomplete({
	source: function (request, response) {

		data = { prefix: request.term }; /*Rel05*/

		$.ajax({
			url: autoComRel011Url,
			type: "POST",
			dataType: "json",
			data: data,
			success: function (obj) {
				response($.map(obj, function (item) {
					return normalizarClienteAutocomplete(item);
				}));
			}
		})
	},
	minLength: 3,
	select: function (event, ui) {
		$("#Rel05").data("autocomplete-selected", true);

		provIdSeleccionado = ui.item.id;
		provDescSeleccionado = ui.item.value;

		$("#Rel05Item").val(ui.item.id);
		$("#Rel05").val(ui.item.value);

		return true;
	}
});
aplicarRenderClienteAutocomplete($("#Rel05"));

let provIdSeleccionado = "";
let provDescSeleccionado = "";

function getMaskForInteger(selector) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: ',',       // separador de miles
		digits: 0,                 // sin decimales
		digitsOptional: false,
		allowMinus: false,
		prefix: '',
		suffix: '',
		rightAlign: true,
		unmaskAsNumber: true,
		min: 0
	});
}

function getMaskForTwoDecimals(selector) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: ',',
		radixPoint: '.',
		digits: 2,
		digitsOptional: false,
		allowMinus: false,
		prefix: '',
		suffix: '',
		rightAlign: true,
		unmaskAsNumber: true
	});
}

function buscarProducto() {
	AbrirWaiting();
	var _post = busquedaProdBaseUrl;
	var valor = $("#Busqueda").val();
	var validarEstado = false;

	var datos = {};
	if (typeof validarEstado !== 'undefined') {
		datos = { busqueda: valor, validarEstado };
	}
	else {
		datos = { busqueda: valor };
	}
	$("#tbGridProd").empty();
	PostGen(datos, _post, function (obj) {
		if (obj.error === true) {
			CerrarWaiting();
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				productoBase = null;
				$("#estadoFuncion").val(false);
				$("#btnBusquedaBase").prop("disabled", false);
				$("#msjModal").modal("hide");
				$("#Busqueda").focus();
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else if (obj.warn === true) {
			CerrarWaiting();
			if (obj.producto.p_id === "0000-0000") {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					productoBase = null;
					$("#estadoFuncion").val(false);
					$("#btnBusquedaBase").prop("disabled", false);
					$("#msjModal").modal("hide");
					$("#Busqueda").focus();
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else if (obj.producto.p_id === "NO" && valor != "") {
				if (funcionBusquedaAvanzada === true) {
					AbrirMensaje("ATENCIÓN", "NO SE ENCONTRO EL PRODUCTO QUE INTENTO BUSCAR. SE ABRIRÁ LA BUSQUEDA AVANZADA.", function () {
						$("#msjModal").modal("hide");
						productoBase = null;
						$("#estadoFuncion").val(false);
						inicializaBusquedaAvanzada();
						$("#busquedaModal").modal("toggle");
						return true;
					}, false, ["Aceptar"], "error!", null);

					return true;
				}
				else {
					AbrirMensaje("ATENCIÓN", "NO SE ENCONTRO EL PRODUCTO QUE INTENTO BUSCAR.", function () {
						$("#msjModal").modal("hide");
						$("#Busqueda").focus();
						return true;
					}, false, ["Aceptar"], "error!", null);

				}
			} else if (obj.producto.p_id === "NO" && valor == "") {
				if (funcionBusquedaAvanzada === true) {
					productoBase = null;
					$("#estadoFuncion").val(false);
					inicializaBusquedaAvanzada();
					$("#busquedaModal").modal("toggle");
				}
			} else {
				//encontro producto pero hay warning
				AbrirMensaje("ATENCIÓN!", obj.msg, function (resp) {
					if (resp === "SI") {
						productoBase = obj.producto;
						$("#estadoFuncion").val(true);
						$("#estadoFuncion").trigger("change");
						$("#msjModal").modal("hide");
						var up = $("#txtUPEnComprobanteRP");
						if (up) {
							up.focus();
						}
						return true;
					}
					else {
						//se deniega
						productoBase = null;
						$("#estadoFuncion").val(false);
						$("#btnBusquedaBase").prop("disabled", false);
						$("#msjModal").modal("hide");
						$("#Busqueda").focus();
						return true;
					}
				},
					true, ["Aceptar", "Denegar"], "Warning!", null);
			}
		}
		else {
			//encontro y se presenta
			productoBase = obj.producto;
			$("#estadoFuncion").val(true);
			$("#estadoFuncion").trigger("change");
			return true;
		}
	});
	return true;
}

