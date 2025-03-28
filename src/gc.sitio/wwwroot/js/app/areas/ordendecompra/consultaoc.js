$(function () {
	InicializaPantalla();
	$("#Date1").on("change", function () { ValidarFechas(); });
	$("#Date2").on("change", function () { ValidarFechas(); });
	$("#tabDetalleDeOC").on("click", function () { BuscarDetalleDeOCTabClick(); });
	$("#tabRprAsociadas").on("click", function () { BuscarRprAsociadasDeOCTabClick(); });
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacionConsultaOC(div);
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
		BuscarOrdenesDeCompra(pagina);
	});
});

const AccionesOC = {
	ACTIVAR: 'ACTIVAR',
	CERRAR: 'CERRAR',
	ANULAR: 'ANULAR',
	LEVANTAR: 'LEVANTAR',
	MODIFICAR_ADM: 'MODIFICAR_ADM'
}

function ModificarOC(opt) {
	var puedoModificar = true;
	var adm_id_entrega = $("#listaAdm").val();
	if (opt == AccionesOC.MODIFICAR_ADM) {
		if (adm_id_entrega == "") {
			AbrirMensaje("ATENCIÓN", "Debe seleccionar una administración para modificar.", function () {
				$("#msjModal").modal("hide");
				puedoModificar = false;
				return true;
			}, false, ["Aceptar"], "error!", null);
			return;
		}
		if (admIdSeleccionado == "") {
			AbrirMensaje("ATENCIÓN", "Debe seleccionar una orden de compra para modificar la administración.", function () {
				$("#msjModal").modal("hide");
				puedoModificar = false;
				return true;
			}, false, ["Aceptar"], "error!", null);
			return;
		}
		if (admIdSeleccionado == adm_id_entrega) {
			AbrirMensaje("ATENCIÓN", "La administración seleccionada es la misma que la actual, por favor seleccione una diferente.", function () {
				$("#msjModal").modal("hide");
				puedoModificar = false;
				return true;
			}, false, ["Aceptar"], "error!", null);
			return;
		}
	}
	if (puedoModificar) {
		AbrirMensaje("Confirmación", "Esta a punto de modificar una Orden de Compra (" + ocIdSeleccionado +"), esta acción no puede deshacerse. Desea continuar?", function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI": //Confirmar comprobante RPR
					AbrirWaiting("Actualizando datos...");
					console.log(opt);
					var oc_compte = ocIdSeleccionado;
					var accion = opt;
					var data = { oc_compte, adm_id_entrega, accion };
					PostGen(data, modificarOCURL, function (obj) {
						CerrarWaiting();
						if (obj.error === true) {
							AbrirMensaje("ATENCIÓN", obj.msg, function () {
								$("#msjModal").modal("hide");
								return true;
							}, false, ["Aceptar"], "error!", null);
						}
						else {
							$("#tbListaOC").find('tr').each(function (i, el) {
								var td = $(this).find('td');
								if (td.length > 0 && td[0].innerText !== undefined && td[0].innerText === ocIdSeleccionado) {
									td[7].innerText = obj.data.oce_id
									td[4].innerText = obj.data.oce_desc
									td[3].innerText = obj.data.adm_desc
									td[7].innerText = obj.data.adm_id
								}
							});
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
}

function ValidarFechas() {
	if ($("#Date1").val() <= $("#Date2").val()) {
		let d1 = moment($("#Date1").val());
		let d2 = moment($("#Date2").val());
		let diffInDays = d2.diff(d1, 'days');
		if (diffInDays > 370) {
			AbrirMensaje("ATENCIÓN", "La diferencia entre las fechas no puede ser mayor a 370 días, revise.", function () {
				$("#msjModal").modal("hide");
				var fecha = moment().format('yyyy-MM-DD');
				$("#Date2").val(fecha)
				fecha = moment($("#FechaEntrega").val()).add(-30, 'day').format('yyyy-MM-DD');
				$("#Date1").val(fecha)
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		return;
	}
	if ($("#Date1").val() > $("#Date2").val()) {
		AbrirMensaje("ATENCIÓN", "El valor de Fecha Desde no puede ser mayor a Fecha Hasta, revise.", function () {
			$("#msjModal").modal("hide");
			$("#Date1").val($("#Date2").val())
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	console.log($("#Date2").val() - $("#Date1").val());
}

function BuscarRprAsociadasDeOCTabClick() {
	if (ocIdSeleccionado == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar una orden de comprar para visualizar las rpr asociadas.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Cargando datos...");
		var ocCompte = ocIdSeleccionado;
		var data = { ocCompte };
		PostGenHtml(data, buscarRprAsociadaDeOrdenDeCompraURL, function (obj) {
			$("#divRprAsociadas").html(obj);
			AddEventListenerToGrid("tbListaRprAsociadaDeOC");
			CerrarWaiting();
			return true
		});
	}
}

function BuscarDetalleDeOCTabClick() {
	if (ocIdSeleccionado == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar una orden de comprar para visualizar el detalle.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Cargando datos...");
		var ocCompte = ocIdSeleccionado;
		var data = { ocCompte };
		PostGenHtml(data, buscarDetalleDeOrdenDeCompraURL, function (obj) {
			$("#divDetalleDeOC").html(obj);
			AddEventListenerToGrid("tbListaDetalleOC");
			FormatearValores("#tbGridConcepto", 1)
			CerrarWaiting();
			return true
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

// Create our number formatter.
const formatter = new Intl.NumberFormat('en-US', {
	style: 'currency',
	currency: 'USD',

	// These options can be used to round to whole numbers.
	trailingZeroDisplay: 'stripIfInteger'   // This is probably what most people
	// want. It will only stop printing
	// the fraction when the input
	// amount is a round number (int)
	// already. If that's not what you
	// need, have a look at the options
	// below.
	//minimumFractionDigits: 0, // This suffices for whole numbers, but will
	// print 2500.10 as $2,500.1
	//maximumFractionDigits: 0, // Causes 2500.99 to be printed as $2,501
});

function selectListaOCDetalleRow(x) {
}

function selectListaOCRow(x) {
	if (x) {
		oceIdSeleccionado = x.cells[7].innerText.trim();
		ocIdSeleccionado = x.cells[0].innerText.trim();
		admIdSeleccionado = x.cells[8].innerText.trim();
	}
	else {
		oceIdSeleccionado = "";
		ocIdSeleccionado = "";
		admIdSeleccionado = "";
	}
	ActivarBotonesTabPrincipal(oceIdSeleccionado);
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

function presentaPaginacionConsultaOC(div) {
	div.pagination({
		items: totalRegs,
		itemsOnPage: pagRegs,
		cssStyle: "dark-theme",
		currentPage: pagina,
		onPageClick: function (num) {
			BuscarOrdenesDeCompra(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltro").collapse("hide")
	return true;
}

function InicializaPantalla() {
	$("#chkDesdeHasta").prop('checked', true);
	$("#chkDesdeHasta").trigger("change");
	$("#chkDesdeHasta").prop("disabled", true);
	$("#Date1").prop("disabled", false);
	$("#Date2").prop("disabled", false);
	$("#lbChkDesdeHasta").text("Fechas");
	$("#lbRel01").text("Proveedor");
	$("#lbRel03").text("Sucursal");
	$("#lbRel02").text("Estado");
	$(".activable").prop("disabled", true);
	$("#btnDetalle").prop("disabled", true);
	$("#divFiltro").collapse("show")
	var fecha = moment().format('yyyy-MM-DD');
	$("#Date2").val(fecha)
	fecha = moment($("#FechaEntrega").val()).add(-30, 'day').format('yyyy-MM-DD');
	$("#Date1").val(fecha)
	funcCallBack = BuscarOrdenesDeCompra;
	ActivarBotonesTabPrincipal('');
	CerrarWaiting();
	return true;
}

function ActivarBotonesTabPrincipal(estado) {
	$("#btnActivarOC").prop("disabled", true);
	$("#btnCerrarOC").prop("disabled", true);
	$("#btnAnularOC").prop("disabled", true);
	$("#btnModiAdm").prop("disabled", true);
	$("#btnImprimir").prop("disabled", true);
	$("#btnServicioExt").prop("disabled", true);
	$("#btnLevantarOC").prop("disabled", true);
	$("#listaAdm").prop("disabled", true);

	if (estado == 'A') {
		$("#btnLevantarOC").prop("disabled", false);
	}
	if (estado == 'P') {
		$("#btnActivarOC").prop("disabled", false);
		$("#btnAnularOC").prop("disabled", false);
		$("#btnImprimir").prop("disabled", false);
	}
	if (estado == "C") {
		$("#btnAnularOC").prop("disabled", false);
		$("#btnModiAdm").prop("disabled", false);
		$("#listaAdm").prop("disabled", false);
		$("#btnServicioExt").prop("disabled", false);
		$("#btnImprimir").prop("disabled", false);
	}
	if (estado == "R") {
		$("#btnCerrarOC").prop("disabled", false);
		$("#btnModiAdm").prop("disabled", false);
		$("#listaAdm").prop("disabled", false);
		$("#btnServicioExt").prop("disabled", false);
		$("#btnImprimir").prop("disabled", false);
	}
}

$("#Rel01List").on("dblclick", 'option', function () {
	$(this).remove();
})
$("#Rel02List").on("dblclick", 'option', function () {
	$(this).remove();
})
$("#Rel03List").on("dblclick", 'option', function () {
	$(this).remove();
})

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

function BuscarOrdenesDeCompra(pag = 1) {
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
	var sort = "oc_id";
	var sortDir = "DESC"
	var data2 = { sort, sortDir, Pagina, buscaNew }
	var data1 = { Buscar, Id, Id2, Date1, Date2, Rel01, Rel02, Rel03 };
	var data = $.extend({}, data1, data2);
	PostGenHtml(data, buscarOrdenesDeCompraURL, function (obj) {
		$("#divOrdenesDeCompra").html(obj);
		AddEventListenerToGrid("tbListaOC");
		ActivarBotonesTabPrincipal("");
		$("#btnActivarOC").on("click", function () { ModificarOC(AccionesOC.ACTIVAR); });
		$("#btnCerrarOC").on("click", function () { ModificarOC(AccionesOC.CERRAR); });
		$("#btnAnularOC").on("click", function () { ModificarOC(AccionesOC.ANULAR); });
		$("#btnLevantarOC").on("click", function () { ModificarOC(AccionesOC.LEVANTAR); });
		$("#btnModiAdm").on("click", function () { ModificarOC(AccionesOC.MODIFICAR_ADM); });
		FormatearValores("#tbListaOC", 6)
		$("#Importe").val(formatter.format($("#Importe").val()));
		//formatter.format(td[idx].innerText);
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