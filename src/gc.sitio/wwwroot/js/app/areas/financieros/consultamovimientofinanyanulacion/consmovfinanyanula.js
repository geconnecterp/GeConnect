$(function () {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		cargaPaginacion();
	});
	$("#chkDesdeHasta").prop('checked', true);
	$("#chkDesdeHasta").trigger("change");
	$("#chkDesdeHasta").prop("disabled", true);

	$("#Date1, #Date2").on("blur", ValidarFechasClick);

	$(document).on("change", "#listaCFO", ControlalistaCFOSelected);
	$(document).on("change", "#listaCFD", ControlalistaCFDSelected);
	$(document).on("change", "#listaTT", ControlalistaTTSelected);
	$(document).on("change", "#listaUsu", ControlalistaUsuSelected);
	//$(document).on("click", "#btnBuscar", btnBuscarClick);
	$(document).on("change", "#btnCancelar", btnCancelarClick);
	//btnCancelar


	$("#CFOList").on("dblclick", 'option', function () { $(this).remove(); })
	$("#CFDList").on("dblclick", 'option', function () { $(this).remove(); })
	$("#TTList").on("dblclick", 'option', function () { $(this).remove(); })
	$("#UsuList").on("dblclick", 'option', function () { $(this).remove(); })

	$("#btnBuscar").on("click", function () {
		dataBak = "";
		pagina = 1;
		BuscarMovimientosFinancieros(pagina);
	});

	InicializarCamposEnFiltros();
	funcCallBack = BuscarMovimientosFinancieros;
});



function cargaPaginacion() {
	$("#divPaginacion").pagination({
		items: totalRegs,
		itemsOnPage: pagRegs,
		cssStyle: "dark-theme",
		currentPage: pagina,
		onPageClick: function (num) {
			BuscarMovimientosFinancieros(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltro").collapse("hide")
	return true;
}

function BuscarMovimientosFinancieros(pag) {
	AbrirWaiting();
	var desde = $("#Date1").val();
	var hasta = $("#Date2").val();
	var ctaf_ori_list = [];
	var ctaf_des_list = [];
	var tipo_list = [];
	var usu_list = [];
	if ($("#chkCFO").is(":checked")) {
		$("#CFOList").children().each(function (i, item) { ctaf_ori_list.push($(item).val()) });
	}
	if ($("#chkCFD").is(":checked")) {
		$("#CFDList").children().each(function (i, item) { ctaf_des_list.push($(item).val()) });
	}
	if ($("#chkTT").is(":checked")) {
		$("#TTList").children().each(function (i, item) { tipo_list.push($(item).val()) });
	}
	if ($("#chkUsu").is(":checked")) {
		$("#UsuList").children().each(function (i, item) { usu_list.push($(item).val()) });
	}
	var ctaf_ori = $("#chkCFO")[0].checked;
	var ctaf_des = $("#chkCFD")[0].checked;
	var tipo = $("#chkTT")[0].checked;
	var usu = $("#chkUsu")[0].checked;
	var data1 = { desde, hasta, ctaf_ori_list, ctaf_ori, ctaf_des_list, ctaf_des, tipo_list, tipo, usu_list, usu };
	var buscaNew = true;
	var sort = null;
	var sortDir = null
	pagina = pag;
	var data2 = { sort, sortDir, pag, buscaNew }
	var data = $.extend({}, data1, data2);
	PostGenHtml(data, buscarMovimientosFinancieros2URL, function (obj) {
		CerrarWaiting();
		$("#divDatosMovimientoFinanciero").html(obj);

		$("#divFiltros").removeClass("show").addClass("collapse");
		$("#divDetalle").collapse("show");
		$("#btnCancelar").on("click", function () {
			btnCancelarClick();
		});
		$("#btnImprimirMovSele").on("click", function () {
			btnImprimirMovSele();
		});
		$("#btnImprimirLista").on("click", function () {
			ControlaMensajeWarning("Método no implementado.");
		});
		$("#btnAnularMovi").on("click", function () {
			ControlaMensajeWarning("Método no implementado.");
		});

		PostGen({}, buscarMetadataURL, function (obj) {
			if (obj.error === true) {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				totalRegs = obj.metadata.totalCount;
				pags = obj.metadata.totalPages;
				pagRegs = obj.metadata.pageSize;

				$("#pagEstado").val(true).trigger("change");
				$("#divPaginacion").removeClass("collapse");
			}

		});
		PostGen({}, actualizarTotalUrl, function (obj) {
			if (obj.error === true) {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				$("#txtTotales").val(formatter.format(obj.totales));
			}
		});

		CerrarWaiting();
		return true
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function btnAnularMovimiento() {
	if (ttraSelected == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar el comprobante a anular.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirMensaje("ATENCIÓN", "¿Confirma que desea anular el comprobante seleccionado?", function () {
			$("#msjModal").modal("hide");
			AbrirWaiting("Anulando comprobante...");
			var traCompte = ttraSelected;
			var data = { tra_compte: traCompte };
			PostGen(data, anularMovimientoFinancieroUrl, function (obj) {
				CerrarWaiting();
				if (obj.error === true) {
					AbrirMensaje("ATENCIÓN", obj.msg, function () {
						$("#msjModal").modal("hide");
						return true;
					}, false, ["Aceptar"], "error!", null);
				}
				else {
					AbrirMensaje("ATENCIÓN", obj.msg, function () {
						$("#msjModal").modal("hide");
						BuscarMovimientosFinancieros(pagina);
						return true;
					}, false, ["Aceptar"], "info!", null);
				}
			});
			return true;
		}, true, ["Cancelar", "Aceptar"], "question", null);
	}
}

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function btnImprimirLista() {
	var filas = $("#tbGridMovFin tbody tr").length;
	if (filas == 0) {
		AbrirMensaje("ATENCIÓN", "No hay datos para imprimir.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Imprimiendo listado...");
		var tipoReporte = 2;
		var data = { tipoReporte };
		PostGen(data, setearTipoDeReporteUrl, function (obj) {
			CerrarWaiting();
			if (obj.error === true) {
				CerrarWaiting();
				//ControlaMensajeWarning(obj.msg);
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				CerrarWaiting();
				ImprimirListadoDeMovimientos();
			}
		});
	}
}

function ImprimirListadoDeMovimientos() {
	//TODO Marce: Esperar a que Carlos pase el modelo de reporte de listado de movimientos financieros
}

function btnImprimirMovSele() {
	if (ttraSelected == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar el comprobante a imprimir.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Imprimiendo comprobante seleccionado...");
		var tipoReporte = 1;
		var data = { tipoReporte };
		PostGen(data, setearTipoDeReporteUrl, function (obj) {
			CerrarWaiting();
			if (obj.error === true) {
				CerrarWaiting();
				//ControlaMensajeWarning(obj.msg);
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				CerrarWaiting();
				ImprimirComprobanteSeleccionadas();
			}
		});
	}
}

function ImprimirComprobanteSeleccionadas() {
	var traCompte = ttraSelected;
	ImprimirTRA_Generada(traCompte);
}

function ImprimirTRA_Generada(traCompte) {
	ReseteoDeReportes();
	setTimeout(() => {
		let data = { tra_compte: traCompte };
		cargarReporteEnArre(25, data, "TRANSFERENCIA ENTRE CUENTAS", "", "");
		invocacionGestorDoc({});
	}, 500);
}

const formatter = new Intl.NumberFormat('de-DE', {
	minimumFractionDigits: 2,
	maximumFractionDigits: 2
});

function btnBuscarClick(pag) {
	
}

function ImprimirTRA_Generada(traCompte) {
	ReseteoDeReportes();
	setTimeout(() => {
		let data = { tra_compte: traCompte };
		cargarReporteEnArre(25, data, "TRANSFERENCIA ENTRE CUENTAS", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function selectReg(x, gridId) {
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		//$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
	ttraSelected = x.childNodes[1].innerText;
}

function ValidarFechasClick() {
	const desde = $("#Date1").val();
	const hasta = $("#Date2").val();

	if (desde && hasta && desde > hasta) {
		AbrirMensaje("ATENCIÓN", "El valor de Fecha Desde no puede ser mayor a Fecha Hasta, revise.", function () {
			$("#msjModal").modal("hide");
			$("#Date1").val($("#Date2").val());
			return true;
		}, false, ["Aceptar"], "error!", null);
	} else {
		ActualizarListaDeUsuarios();
	}
}

function ActualizarListaDeUsuarios() {
	var data = { desde: $("#Date1").val(), hasta: $("#Date2").val() };
	PostGenHtml(data, actualizarListaDeUsuariosURL, function (obj) {
		$("#divUsuarios").html(obj);
		$("#chkUsu").on("click", function () {
			if ($("#chkUsu").is(":checked")) {
				$("#listaUsu").prop("disabled", false);
				$("#UsuList").prop("disabled", false);
				$("#listaUsu").trigger("focus");
			}
			else {
				$("#listaUsu").prop("disabled", true);
				$("#UsuList").prop("disabled", true);
			}
		});
		CerrarWaiting();
		return true
	});
}

function ControlalistaUsuSelected() {
	var item = $("#listaUsu").val();
	var desc = $("#listaUsu option:selected").text();
	if ($("#UsuList").has('option:contains("' + item + '")').length === 0 && $("#UsuList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#UsuList").append(opc);
	}
}

function ControlalistaCFOSelected() {
	var item = $("#listaCFO").val();
	var desc = $("#listaCFO option:selected").text();
	if ($("#CFOList").has('option:contains("' + item + '")').length === 0 && $("#CFOList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#CFOList").append(opc);
	}
}

function ControlalistaCFDSelected() {
	var item = $("#listaCFD").val();
	var desc = $("#listaCFD option:selected").text();
	if ($("#CFDList").has('option:contains("' + item + '")').length === 0 && $("#CFDList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#CFDList").append(opc);
	}
}

function ControlalistaTTSelected() {
	var item = $("#listaTT").val();
	var desc = $("#listaTT option:selected").text();
	if ($("#TTList").has('option:contains("' + item + '")').length === 0 && $("#TTList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#TTList").append(opc);
	}
}

function btnCancelarClick() {
	$("#divFiltros").removeClass("collapse").addClass("show");
	$("#divDetalle").collapse("hide");
	$("#chkCFO").prop('checked', false);
	$("#chkCFO").trigger("change");
	$("#chkCFD").prop('checked', false);
	$("#chkCFD").trigger("change");
	$("#chkTT").prop('checked', false);
	$("#chkTT").trigger("change");
	$("#chkUsu").prop('checked', false);
	$("#chkUsu").trigger("change");
	$("#CFOList").empty();
	$("#CFDList").empty();
	$("#TTList").empty();
	$("#UsuList").empty();
	$("#listaCFO").val("");
	$("#listaCFD").val();
	$("#listaTT").val();
	$("#listaUsu").val();
	$("#listaCFO").prop("disabled", true);
	$("#listaCFD").prop("disabled", true);
	$("#listaTT").prop("disabled", true);
	$("#listaUsu").prop("disabled", true);
	$("#CFOList").prop("disabled", true);
	$("#CFDList").prop("disabled", true);
	$("#TTList").prop("disabled", true);
	$("#UsuList").prop("disabled", true);
	$("#btnCancel").on("click", function () {
		btnCancelarClick();
	});
	InicializarDatosEnSesion();
}

function InicializarDatosEnSesion() {
	ttraSelected = "";
	PostGen({}, inicializarDatosEnSesionURL, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	});
}

function InicializarCamposEnFiltros() {
	$("#chkDesdeHasta").on("click", function () {
		if ($("#chkDesdeHasta").is(":checked")) {
			$("#Date1").prop("disabled", false);
			$("#Date2").prop("disabled", false);
			$("#Date1").trigger("focus");
		}
		else {
			$("#Date1").prop("disabled", true);
			$("#Date2").prop("disabled", true);
		}
	});
	$("#chkCFO").on("click", function () {
		if ($("#chkCFO").is(":checked")) {
			$("#listaCFO").prop("disabled", false);
			$("#CFOList").prop("disabled", false);
			$("#listaCFO").trigger("focus");
		}
		else {
			$("#listaCFO").prop("disabled", true);
			$("#CFOList").prop("disabled", true);
		}
	});
	$("#chkCFD").on("click", function () {
		if ($("#chkCFD").is(":checked")) {
			$("#listaCFD").prop("disabled", false);
			$("#CFDList").prop("disabled", false);
			$("#listaCFD").trigger("focus");
		}
		else {
			$("#listaCFD").prop("disabled", true);
			$("#CFDList").prop("disabled", true);
		}
	});
	$("#chkTT").on("click", function () {
		if ($("#chkTT").is(":checked")) {
			$("#listaTT").prop("disabled", false);
			$("#TTList").prop("disabled", false);
			$("#listaTT").trigger("focus");
		}
		else {
			$("#listaTT").prop("disabled", true);
			$("#TTList").prop("disabled", true);
		}
	});
	$("#chkUsu").on("click", function () {
		if ($("#chkUsu").is(":checked")) {
			$("#listaUsu").prop("disabled", false);
			$("#UsuList").prop("disabled", false);
			$("#listaUsu").trigger("focus");
		}
		else {
			$("#listaUsu").prop("disabled", true);
			$("#UsuList").prop("disabled", true);
		}
	});
	$("#Date1, #Date2").prop("disabled", false);
	$("#btnCancel").on("click", function () {
		btnCancelarClick();
	});
}