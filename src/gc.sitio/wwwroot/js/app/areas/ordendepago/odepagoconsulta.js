$(function () {
	InicializaPantalla();
	// Validar al salir del campo
	$("#Date1, #Date2").on("blur", ValidarFechasClick);
	// Validar al seleccionar con el mouse
	//$("#Date1, #Date2").on("change", ValidarFechasClick);

	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacionConsultaOP(div);
	});
	$("input#Rel01").on("click", function () {
		$("input#Rel01").val("");
		$("#Rel01Item").val("");
	});
	$("input#Rel02").on("click", function () {
		$("input#Rel02").val("");
		$("#Rel02Item").val("");
	});
	$("input#Rel03").on("click", function () {
		$("input#Rel03").val("");
		$("#Rel03Item").val("");
	});
	$("#btnBuscar").on("click", function () {
		dataBak = "";
		pagina = 1;
		BuscarOrdenesDePago(pagina);
	});
	$("#btnCancel").on("click", function () {
		InicializarDatosEnSesion();
		InicializaPantalla();
		LimpiarDatosDelFiltroInicial();
		$("#btnDetalle").trigger("click");
		$("#divDetalle").collapse("hide");
	});
	$("#chkRel02").on("click", function () {
		if ($("#chkRel02").is(":checked")) {
			$("#listaLs02").prop("disabled", false);
			$("#Rel02List").prop("disabled", false);
			$("#listaLs02").trigger("focus");

		}
		else {
			$("#listaLs02").prop("disabled", true).val("");
			$("#Rel02List").prop("disabled", true).empty();
		}
	});
	//check generico REL03 activando componentes disables
	$("#chkRel03").on("click", function () {
		if ($("#chkRel03").is(":checked")) {
			$("#listaLs03").prop("disabled", false);
			$("#Rel03List").prop("disabled", false);
			$("#listaLs03").trigger("focus");

		}
		else {
			$("#listaLs03").prop("disabled", true).val("");
			$("#Rel03List").prop("disabled", true).empty();
		}
	});

	$(document).on("click", "#btnAnularOP", btnAnularOP);
	$(document).on("click", "#btnAnularCertRet", btnAnularCertRet);
	$(document).on("click", "#btnImprimirLista", btnImprimirLista);
	$(document).on("click", "#btnImprimirOP", btnImprimirOP);
	$(document).on("change", "#listaTipoCert", ControlalistaTipoCertSelected);
	$(document).on("change", "#listaLs03", ControlalistaTipoSelected);
	$(document).on("change", "#listaLs02", ControlalistaUsuarioSelected);

	$("#Rel02List").on("dblclick", 'option', function () { $(this).remove(); })
	$("#Rel03List").on("dblclick", 'option', function () { $(this).remove(); })
});

const formatter = new Intl.NumberFormat('de-DE', {
	minimumFractionDigits: 2,
	maximumFractionDigits: 2
});

function ControlalistaTipoSelected() {
	var item = $("#listaLs03").val();
	var desc = $("#listaLs03 option:selected").text();
	if ($("#Rel03List").has('option:contains("' + item + '")').length === 0) {
		$("#Rel03Item").val(item);
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#Rel03List").append(opc);
	}
}

function ControlalistaUsuarioSelected() {
	var item = $("#listaLs02").val();
	var desc = $("#listaLs02 option:selected").text();
	if ($("#Rel02List").has('option:contains("' + item + '")').length === 0 && $("#Rel02List").has('option:contains("' + desc + '")').length === 0) {
		$("#Rel02Item").val(item);
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#Rel02List").append(opc);
	}
}

function AgregarHandlerAHeaderEnGrillaListaOP() {
	var dataTable = document.getElementById('tbListaOP');
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
}

function LimpiarDatosDelFiltroInicial() {
	$("#chkDesdeHasta").prop('checked', true);
	$("#chkDesdeHasta").trigger("change");
	$("#chkDesdeHasta").prop("disabled", true);
	$("#chkRel01").prop('checked', false);
	$("#chkRel01").trigger("change");
	$("#chkRel02").prop('checked', false);
	$("#chkRel02").trigger("change");
	$("#chkRel03").prop('checked', false);
	$("#chkRel03").trigger("change");
	$("#Rel01").val("");
	$("#Rel02").val("");
	$("#Rel03").val("");
	$("#Rel01List").empty();
	$("#Rel03List").empty();
	$("#Rel02List").empty();
	$("#Rel01").prop("disabled", true);
	$("#Rel02").prop("disabled", true);
	$("#Rel03").prop("disabled", true);
	$("#Rel01List").prop("disabled", true);
	$("#Rel02List").prop("disabled", true);
	$("#Rel03List").prop("disabled", true);
	$("#chkRel03").prop("disabled", false);
}

function InicializarDatosEnSesion() {
	PostGen({}, inicializarDatosEnSesionURL, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	});
}

function ControlalistaTipoCertSelected() {
	if ($("#listaTipoCert").val() != "") {
		impIdSeleccionado = $("#listaTipoCert").val();
	}
	else {
		impIdSeleccionado = "";
	}
}

function btnImprimirLista() {
	if ($("#tbListaOP > tbody > tr").length === 0) {
		AbrirMensaje("ATENCIÓN", "No hay datos generar el reporte.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		var tipoReporte = 1;
		var data = { tipoReporte };
		PostGen(data, setearTipoDeReporteUrl, function (obj) {
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
				ImprimirListadoDeOP();
			}
		});
	}
}

function btnImprimirOP() {
	if ($("#tbListaOP > tbody > tr").length === 0) {
		AbrirMensaje("ATENCIÓN", "No hay datos generar el reporte.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		var tipoReporte = 2;
		var data = { tipoReporte };
		PostGen(data, setearTipoDeReporteUrl, function (obj) {
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
				ImprimirOPSeleccionadas();
			}
		});
	}
}

function ImprimirListadoDeOP() {
	var Buscar = "";
	var Date1 = $("#Date1").val();
	var Date2 = $("#Date2").val();
	var Date1Print = moment($("#Date1").val()).format('DD/MM/yyyy')
	var Date2Print = moment($("#Date2").val()).format('DD/MM/yyyy')
	var Id = "";
	var Id2 = "";
	var Rel01 = [];
	var Rel02 = [];
	var Rel03 = [];
	$("#Rel01List").children().each(function (i, item) { Rel01.push($(item).val()) });
	$("#Rel02List").children().each(function (i, item) { Rel02.push($(item).val()) });
	$("#Rel03List").children().each(function (i, item) {
		var aux = { Id: $(item).val(), Descripcion: $(item).text() };
		Rel03.push(aux);
	});
	var rel01 = Rel01.toString();
	var rel02 = Rel02.toString();
	var rel03 = Rel03.toString();
	var data = { Buscar, Id, Id2, Date1, Date2, rel01, rel02, rel03, Date1Print, Date2Print };
	cargarReporteEnArre(24, data, "CONSULTA DE ORDENES DE PAGO", "", "");
	invocacionGestorDoc({});
}

function ImprimirOPSeleccionadas() {
	var IdsComptes = [];
	var Ids = [];
	$("#tbListaOP").find('tr').each(function (i, el) {
		var td = $(this).find('td');
		if (td.length > 0 && td[0].innerText !== undefined) {
			if (td.eq(0)[0]) {
				if (td.eq(7)[0].children[0].checked) {
					Ids.push({ Id1: td.eq(0).text(), Id2: td.eq(9).text() });
					IdsComptes.push(td.eq(0).text());
				}
			}
		}
	});
	AbrirWaiting();
	var opCompteLista = IdsComptes.join(',');
	var data = { opCompteLista };
	PostGen(data, validarExistenciaDeCertificadosParaImprimirURL, function (obj) {
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
			let data = { Ids };
			cargarReporteEnArre(17, data, "ORDEN DE PAGO A PROVEEDORES", "", "");
			if (obj.imprimeIIBB) {
				cargarReporteEnArre(18, data, "CERTIFICADO RETENCIÓN IIBB", "", "");
			}
			if (obj.imprimeIVA) {
				cargarReporteEnArre(20, data, "CERTIFICADO RETENCIÓN IVA", "", "");
			}
			if (obj.imprimeGAN) {
				cargarReporteEnArre(19, data, "CERTIFICADO RETENCIÓN GA", "", "");
			}
			//TODO MARCE: Modificar los reportes de IVA y GA para soportar la multiple generación de reportes.
			invocacionGestorDoc({});
		}
	});
}

function btnAnularCertRet() {
	if (impIdSeleccionado == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un Tipo de Certificado.", function () {
			$("#msjModal").modal("hide");
			$("#listaTipoCert").trigger("focus");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirMensaje("ATENCIÓN", "¿Desea anular el Certificado de Retención?", function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI": //Confirmar
					AbrirWaiting();
					var data = { op_compte: opIdSeleccionado, imp_id: impIdSeleccionado };
					PostGen(data, anularCertificadoDeOrdenDePagoURL, function (obj) {
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
							ConsultarExistenciaDeCertificados(opIdSeleccionado);
							AbrirMensaje("ATENCIÓN", obj.msg, function () {
								$("#msjModal").modal("hide");
								return true;
							}, false, ["Aceptar"], "succ!", null);
						}
					});
					break;
				case "NO":
					break;
				default:
					break;
			}
			return true;
		}, true, ["Aceptar", "Cancelar"], "warn!", null);
	}
}

function btnAnularOP() {
	if (opIdSeleccionado == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar una Orden de Pago.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirMensaje("ATENCIÓN", "¿Desea anular la Orden de Pago seleccionada?", function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI": //Confirmar
					AbrirWaiting();
					var data = { op_compte: opIdSeleccionado };
					PostGen(data, anularOrdenDePagoURL, function (obj) {
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
							ActualizarRegistroDeOrdenDePagoLuegoDeAnular(opIdSeleccionado);
							AbrirMensaje("ATENCIÓN", obj.msg, function () {
								$("#msjModal").modal("hide");
								return true;
							}, false, ["Aceptar"], "succ!", null);
						}
					});
					break;
				case "NO":
					break;
				default:
					break;
			}
			return true;
		}, true, ["Aceptar", "Cancelar"], "warn!", null);

	}
}

function ActualizarRegistroDeOrdenDePagoLuegoDeAnular(op_compte) {
	$("#tbListaOP").find('tr').each(function (i, el) {
		var td = $(this).find('td');
		if (td.length > 0 && td[1].innerText !== undefined && td[0].innerText === op_compte) {
			//GRILLA
			td[4].innerText = "SI";//
		}
	});
	$("#btnAnularOP").prop("disabled", true);
	CerrarWaiting();
}

function selectListaOPRow(x) {
	if (x) {
		opIdSeleccionado = x.cells[0].innerText.trim();
	}
	else {
		opIdSeleccionado = "";
	}
	ConsultarExistenciaDeCertificados(opIdSeleccionado);
	CargarDetalleDeOP(opIdSeleccionado);
}

function CargarDetalleDeOP(op_compte) {
	var data = { cmptId: op_compte };
	PostGenHtml(data, consultarOPProvDetUrl, function (obj) {
		$("#divDetalleDeOP").html(obj);
		CerrarWaiting();
		return true
	});
}

function ConsultarExistenciaDeCertificados(op_compte) {
	AbrirWaiting();
	var data = { op_compte };
	PostGen(data, consultarExistenciaDeCertificadosURL, function (obj) {
		if (obj.error === true) {
			CerrarWaiting();
			ControlaMensajeWarning(obj.msg);
		}
		else {
			$("#MostrarListaTipoCertificado").val(obj.tieneCertificados);
			ActivarBotonesTabPrincipal();
			if (obj.tieneCertificados) {
				CargarListaTiposCertificados(op_compte);
			}
			else {
				CerrarWaiting();
			}
		}
	});
}

function CargarListaTiposCertificados(op_compte) {
	var data = { op_compte };
	PostGenHtml(data, cargarListaTiposCertificadosURL, function (obj) {
		$("#divListaCert").html(obj);
		if ($("#listaTipoCert").val() != "") {
			impIdSeleccionado = $("#listaTipoCert").val();
		}
		else {
			impIdSeleccionado = "";
		}
		CerrarWaiting();
		return true
	});
}

function InicializaPantalla() {
	$("#chkDesdeHasta").prop('checked', true);
	$("#chkDesdeHasta").trigger("change");
	$("#chkDesdeHasta").prop("disabled", true);
	$("#Date1").prop("disabled", false);
	$("#Date2").prop("disabled", false);
	$("#lbChkDesdeHasta").text("Fechas");
	$("#lbRel01").text("Proveedor");
	$("#lbRel03").text("Tipo");
	$("#lbRel02").text("Usuario");
	$("#btnDetalle").prop("disabled", true)
	var fecha = moment().format('yyyy-MM-DD');
	$("#Date2").val(fecha)
	fecha = moment($("#FechaEntrega").val()).add(-30, 'day').format('yyyy-MM-DD');
	$("#Date1").val(fecha)
	$("#divFiltro").collapse("show")
	$("#chkRel03").prop("disabled", false);
	ActualizarListaDeUsuarios();
	ActualizarListaDeTipos();
	funcCallBack = BuscarOrdenesDePago;
}

function presentaPaginacionConsultaOP(div) {
	div.pagination({
		items: totalRegs,
		itemsOnPage: pagRegs,
		cssStyle: "dark-theme",
		currentPage: pagina,
		onPageClick: function (num) {
			BuscarOrdenesDePago(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltro").collapse("hide")
	return true;
}

function BuscarOrdenesDePago(pag = 1) {
	AbrirWaiting();
	var Buscar = "";
	var Date1 = $("#Date1").val();
	var Date2 = $("#Date2").val();
	var Id = "";
	var Id2 = "";
	var Rel01 = [];
	var Rel02 = [];
	var Rel03 = [];
	$("#Rel01List").children().each(function (i, item) { Rel01.push($(item).val()) });
	$("#Rel02List").children().each(function (i, item) { Rel02.push($(item).val()) });
	$("#Rel03List").children().each(function (i, item) {
		var aux = { Id: $(item).val(), Descripcion: $(item).text() };
		Rel03.push(aux);
	});

	var buscaNew = true;
	pagina = pag;
	Pagina = pag;
	var sort = "op_id";
	var sortDir = "DESC"
	var data2 = { sort, sortDir, Pagina, buscaNew }
	var data1 = { Buscar, Id, Id2, Date1, Date2, Rel01, Rel02, Rel03 };
	var data = $.extend({}, data1, data2);
	PostGenHtml(data, buscarOrdenesDePagoURL, function (obj) {
		$("#divOrdenesDePago").html(obj);
		AddEventListenerToGrid("tbListaOP");
		ActivarBotonesTabPrincipal();
		AgregarHandlerAHeaderEnGrillaListaOP();
		$('input[name="select_all"]').prop('checked', true);
		FormatearValores("#tbListaOC", 6)
		$("#Importe").val(formatter.format($("#Importe").val()));
		$("#divDetalle").collapse("show");

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
			}

		});

		$("#btnDetalle").prop("disabled", false);
		CerrarWaiting();
		return true
	});
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
function ValidarFechas() {
	console.log("ValidarFechas entró");
	if ($("#Date1").val() > $("#Date2").val()) {
		AbrirMensaje("ATENCIÓN", "El valor de Fecha Desde no puede ser mayor a Fecha Hasta, revise.", function () {
			$("#msjModal").modal("hide");
			$("#Date1").val($("#Date2").val());
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		ActualizarListaDeUsuarios();
	}
}

function ActualizarListaDeUsuarios() {
	var data = { f_desde: $("#Date1").val(), f_hasta: $("#Date2").val() };
	PostGenHtml(data, actualizarListaDeUsuariosURL, function (obj) {
		$("#divLs02").html(obj);
		$("#chkRel02").prop('checked', false);
		$("#chkRel02").trigger("change");
		$("#Rel02List").empty();
		CerrarWaiting();
		return true
	});
}

function ActualizarListaDeUsuariosOP() {
	var data = { f_desde: $("#Date1").val(), f_hasta: $("#Date2").val() };
	PostGen(data, actualizarListaDeUsuariosOPURL, function (obj) {
		if (obj.error === true) {
			ControlaMensajeWarning(obj.msg);
		}
		else {
			console.log("Lista de usuarios actualizada correctamente.");
		}
	});
}

function ActualizarListaDeTipos() {
	var data = {};
	PostGenHtml(data, actualizarListaDeTiposURL, function (obj) {
		$("#divLs03").html(obj);
		$("#chkRel03").prop('checked', false);
		$("#chkRel03").trigger("change");
		CerrarWaiting();
		return true
	});
}

$("#Rel03").autocomplete({
	source: function (request, response) {

		data = { prefix: request.term }; Rel03

		$.ajax({
			url: autoComRel03Url,
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
		if ($("#Rel03List").has('option:contains("' + ui.item.id + '")').length === 0) {
			$("#Rel03Item").val(ui.item.id);
			var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
			$("#Rel03List").append(opc);
		}
		return true;
	}
});

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

function FormatearValores(grilla, idx) {
	//grilla = "#tbListaProductoOC"
	$(grilla).find('tr').each(function (i, el) {
		var td = $(this).find('td');
		if (td.length > 0 && td[idx].innerText !== undefined) {
			td[idx].innerText = formatter.format(td[idx].innerText);
		}
	});
}

function ActivarBotonesTabPrincipal() {
	var mostrarListaTipoCertificado = $("#MostrarListaTipoCertificado").val();
	if (mostrarListaTipoCertificado === "true") {
		$("#divListaCert").collapse("show");
		$("#btnAnularCertRet").prop("disabled", false);
	}
	else {
		$("#divListaCert").collapse("hide");
		$("#btnAnularCertRet").prop("disabled", true);
	}
	$("#tbListaOP").find('tr').each(function (i, el) {
		var td = $(this).find('td');
		if (td.length > 0 && td[1].innerText !== undefined && td[0].innerText === opIdSeleccionado) {
			if (td[4].innerText == "SI") {
				$("#btnAnularOP").prop("disabled", true);
			}
			else {
				$("#btnAnularOP").prop("disabled", false);
			}
			return false;
		}
	});
}