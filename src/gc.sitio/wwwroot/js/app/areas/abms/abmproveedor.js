$(function () {
	$("#btnCancel").on("click", function () {
		$("#btnFiltro").trigger("click");
	});
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacion(div);
	});
	$("#btnBuscar").on("click", function () { buscarProveedores(pagina); });

	//tabProveedor
	$("#tabProveedor").on("click", function () { SeteaIDProvSelected(); });
	$("#tabFormaDePago").on("click", function () { BuscarFormaDePago(); });
	$("#tabOtrosContactos").on("click", function () { BuscarOtrosContactos(); });
	$("#tabNotas").on("click", function () { BuscarNotas(); });
	$("#tabObservaciones").on("click", function () { BuscarObservaciones(); });

	/*ABM Botones*/
	$("#btnAbmNuevo").on("click", function () { btnNuevoClick(); });
	$("#btnAbmModif").on("click", function () { btnModiClick(); });
	$("#btnAbmElimi").on("click", function () { btnBajaClick(); });
	$("#btnAbmAceptar").on("click", function () { btnSubmitClick(); });
	$("#btnAbmCancelar").on("click", function () { btnCancelClick(); });

	InicializaPantallaAbmProveedor();
	funcCallBack = buscarProveedores;
	return true;
});

const Tabs = {
	TabCliente: 'btnTabProveedor',
	TabFormasDePago: 'btnTabFormasDePago',
	TabOtrosContactos: 'btnTabOtrosContactos',
	TabNotas: 'btnTabNotas',
	TabObservaciones: 'btnTabObservaciones'
}

function btnNuevoClick() { }

function btnModiClick() { }

function btnBajaClick() { }

function btnSubmitClick() { }

function btnCancelClick() { }

function SeteaIDProvSelected() {
	$("#IdSelected").val($("#Proveedor_Cta_Id").val());
}

function InicializaPantallaAbmProveedor() {
	var tb = $("#tbGridProveedor tbody tr");
	if (tb.length === 0) {
		$("#divFiltro").collapse("show")
	}

	$("#lbRel01").text("TIPO OPE");

	$("#chkRel02").hide();
	$("#lbRel02").hide();
	$("#lbNombreRel02").hide();
	$("#Rel02").hide();
	$("#Rel02List").hide();

	$("#lbChkDescr").text("Denominación");
	$("#lbDescr").html("Desc");

	$("#lbChkDesdeHasta").text("ID Cuenta");

	$("#IdSelected").val("");
	$(".activable").prop("disabled", true);
	CerrarWaiting();
	return true;
}

function buscarProveedores(pag) {
	AbrirWaiting();
	var buscar = "";
	var id = "";
	var id2 = "";
	var r01 = [];

	if ($("#chkDescr").is(":checked")) {
		buscar = $("#Buscar").val();
	}
	if ($("#chkDesdeHasta").is(":checked")) {
		id = $("#Id").val();
		id2 = $("#Id2").val();
	}
	if ($("#chkRel01").is(":checked")) {
		$("#Rel01List").children().each(function (i, item) { r01.push($(item).val()) });
	}

	var data1 = {
		id, id2,
		rel01: r01,
		rel02: [],
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
				$(".activable").prop("disabled", true);
			}

		});
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});

}

function selectReg(e) {
	$("#tbGridProveedor tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
	});
	$(e).addClass("selected-row");
}

function selectRegDbl(x) {
	AbrirWaiting("Espere mientras se busca el proveedor seleccionado...");
	$("#tbGridProveedor tbody tr").each(function (index) {
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selectedEdit-row");

	var cta_id = x.cells[0].innerText.trim();
	if (cta_id !== "") {
		ctaId = cta_id;
		BuscarProveedor(cta_id);
		BuscarFormaDePago();
		BuscarOtrosContactos();
		BuscarNotas();
		BuscarObservaciones();
		HabilitarBotones(false, false, false, true, true);
		$(".activable").prop("disabled", true);
	}
}

function BuscarProveedor(ctaId) {
	var data = { ctaId };
	AbrirWaiting();
	PostGenHtml(data, buscarProveedorUrl, function (obj) {
		$("#divDatosProveedor").html(obj);
		$("#IdSelected").val($("#Proveedor_Cta_Id").val());
		$(".activable").prop("disabled", true);
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}