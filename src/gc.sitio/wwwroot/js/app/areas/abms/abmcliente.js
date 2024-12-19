$(function () {
	$("#btnCancel").on("click", function () {
		$("#btnFiltro").trigger("click");
	});
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacion(div);
	});
	$("#btnBuscar").on("click", function () { buscarClientes(pagina); });

	$("#tabFormaDePago").on("click", function () { BuscarFormaDePago(); });
	$("#tabOtrosContactos").on("click", function () { BuscarOtrosContactos(); });
	$("#tabNotas").on("click", function () { BuscarNotas(); });
	$("#tabObservaciones").on("click", function () { BuscarObservaciones(); });
	//
	//codigo trasladado a siteGen

	InicializaPantallaAbmProd();
	/*    AbrirWaiting();*/
	funcCallBack = buscarClientes;
	return true;
});

function BuscarObservaciones() {
	if (ctaId != "") {
		var data = { ctaId };
		AbrirWaiting();
		PostGenHtml(data, buscarObservacionesUrl, function (obj) {
			$("#divObservaciones").html(obj);
			AgregarHandlerSelectedRow("tbClienteObservaciones");
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function BuscarNotas() {
	if (ctaId != "") {
		var data = { ctaId };
		AbrirWaiting();
		PostGenHtml(data, buscarNotasUrl, function (obj) {
			$("#divNotas").html(obj);
			AgregarHandlerSelectedRow("tbClienteNotas");
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function BuscarOtrosContactos() {
	if (ctaId != "") {
		var data = { ctaId };
		AbrirWaiting();
		PostGenHtml(data, buscarOtrosContactosUrl, function (obj) {
			$("#divOtrosContactos").html(obj);
			AgregarHandlerSelectedRow("tbClienteOtroContacto");
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function BuscarFormaDePago() {
	if (ctaId != "") {
		var data = { ctaId };
		AbrirWaiting();
		PostGenHtml(data, buscarFormaDePagoUrl, function (obj) {
			$("#divFormasDePago").html(obj);
			AgregarHandlerSelectedRow("tbClienteFormaPagoEnTab");
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function AgregarHandlerSelectedRow(grilla) {
	document.getElementById(grilla).addEventListener('click', function (e) {
		if (e.target.nodeName === 'TD') {
			var selectedRow = this.querySelector('.selected-row');
			if (selectedRow) {
				selectedRow.classList.remove('selected-row');
			}
			e.target.closest('tr').classList.add('selected-row');
		}
	});
}

function selectOCenTab(x) {
	var tcId = x.cells[5].innerText.trim();
	var data = { ctaId, tcId };
	AbrirWaiting();
	PostGenHtml(data, buscarDatosOtrosContactosUrl, function (obj) {
		$("#divDatosDeOCSelected").html(obj);
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function selectFPenTab(x) {
	var fpId = x.cells[2].innerText.trim();
	var data = { ctaId, fpId };
	AbrirWaiting();
	PostGenHtml(data, buscarDatosFormasDePagoUrl, function (obj) {
		$("#divDatosDeFPSelected").html(obj);
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function selectNotaenTab(x) {
	var usuId = x.cells[3].innerText.trim();
	var data = { ctaId, usuId };
	AbrirWaiting();
	PostGenHtml(data, buscarDatosNotaUrl, function (obj) {
		$("#divDatosDeNotaSelected").html(obj);
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function selectObsEnTab(x) {
	var toId = x.cells[2].innerText.trim();
	var data = { ctaId, toId };
	AbrirWaiting();
	PostGenHtml(data, buscarDatosObservacionesUrl, function (obj) {
		$("#divDatosDeObsSelected").html(obj);
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function InicializaPantallaAbmProd() {
	var tb = $("#tbGridProd tbody tr");
	if (tb.length === 0) {
		$("#divFiltro").collapse("show")
	}

	$("#lbRel01").text("TIPO");
	$("#lbRel02").text("ZONA");

	CerrarWaiting();
	return true;
}

function cargaPaginacion() {
	$("#divPaginacion").pagination({
		items: totalRegs,
		itemsOnPage: pagRegs,
		cssStyle: "dark-theme",
		currentPage: pagina,
		onPageClick: function (num) {
			buscarClientes(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltro").collapse("hide")
	return true;
}

function buscarClientes(pag) {
	AbrirWaiting();
	var buscar = $("#Buscar").val();
	var id = $("#Id").val();
	var id2 = $("#Id2").val();
	var r01 = [];
	var r02 = [];
	$("#Rel01List").children().each(function (i, item) { r01.push($(item).val()) });
	$("#Rel02List").children().each(function (i, item) { r02.push($(item).val()) });

	var data1 = {
		id, id2,
		rel01: r01,
		rel02: r02,
		rel03: [],
		"fechaD": null, //"0001-01-01T00:00:00",
		"fechaH": null, //"0001-01-01T00:00:00",
		buscar
	};

	var buscaNew = JSON.stringify(dataBak) != JSON.stringify(data1)

	if (buscaNew === false) {
		//son iguales las condiciones cambia de pagina
		pagina = pag;
	}
	else {
		dataBak = data1;
		pagina = 1;
		pag = 1;
	}

	var sort = null;
	var sortDir = null

	var data2 = { sort, sortDir, pag, buscaNew }

	var data = $.extend({}, data1, data2);

	PostGenHtml(data, buscarUrl, function (obj) {
		$("#divGrilla").html(obj);
		$("#divFiltro").collapse("hide")
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
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});

}

function selectReg(e) {
	$("#tbGridProd tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
	});
	$(e).addClass("selected-row");

	var cta_id = e.cells[0].innerText.trim();
	if (cta_id !== "") {
		ctaId = cta_id;
		BuscarCliente(cta_id);
		BuscarFormaDePago();
		BuscarOtrosContactos();
		BuscarNotas();
		BuscarObservaciones();
	}
}

function BuscarCliente(ctaId) {
	var data = { ctaId };
	AbrirWaiting();
	PostGenHtml(data, buscarClienteUrl, function (obj) {
		$("#divDatosCliente").html(obj);
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}