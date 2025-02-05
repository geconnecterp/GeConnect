$(function () {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacion(div);
	});

	$(document).on("dblclick", "#" + Grids.GridMedioDePago + " tbody tr", function () {
		x = $(this);
		ejecutaDblClickGrid(x, Grids.GridMedioDePago);
	});

	$("#tabMedioDePago").on("click", function () { SeteaInsIdSelected(); });
	$("#tabOpcionesCuotas").on("click", function () { BuscarOpcionesCuotasTabClick(); });
	$("#tabCuentaFinContable").on("click", function () { BuscarCuentaFinContableTabClick(); });
	$("#tabPos").on("click", function () { BuscarPosTabClick(); });

	/*ABM Botones*/
	$("#btnAbmNuevo").on("click", function () { btnNuevoClick(); });
	$("#btnAbmModif").on("click", function () { btnModiClick(); });
	$("#btnAbmElimi").on("click", function () { btnBajaClick(); });
	$("#btnAbmAceptar").on("click", function () { btnSubmitClick(); });
	$("#btnAbmCancelar").on("click", function () { btnCancelClick(); });

	$("#btnDetalle").on("mousedown", analizaEstadoBtnDetalle); 

	$("#btnDetalle").prop("disabled", true);

	$("#btnCancel").on("click", function () {
		$("#btnFiltro").trigger("click");
	});

	$("#btnBuscar").on("click", function () {
		//es nueva la busqueda no resguardamos la busqueda anterior. es util para paginado
		dataBak = "";
		//es una busqueda por filtro. siempre sera pagina 1
		pagina = 1;
		buscarMediosDePago(pagina);
	});

	$(".inputEditable").on("keypress", analizaEnterInput);
	$("#btnAbmAceptar").hide();
	$("#btnAbmCancelar").hide();

	InicializaPantallaAbmMedioDePago();
	funcCallBack = buscarMediosDePago;
	return true;
});

function analizaEstadoBtnDetalle() {
	var res = $("#divDetalle").hasClass("show");
	if (res === true) {
		selectRegCli(regSelected, Grids.GridMedioDePago);
	}
	return true;

}

function SeteaInsIdSelected() {
	$("#IdSelected").val($("#MedioDePago_Ins_Id").val());
}

function BuscarOpcionesCuotasTabClick() {
	if ($(".nav-link").prop("disabled")) {
		return false;
	}
	BuscarOpcionesCuotas();
}

function BuscarOpcionesCuotas() {
	if (insId === "") {
		insId = $("#IdSelected").val();
	}
	if (insId != "") {
		var data = { insId };
		AbrirWaiting();
		PostGenHtml(data, buscarOpcionesCuotasUrl, function (obj) {
			$("#divOpcionesCuotas").html(obj);
			AgregarHandlerSelectedRow("tbOpcionesCuotas");
			$(".activable").prop("disabled", true);
			$("#IdSelected").val("");
			CerrarWaiting();
		}, function (obj) {
			ControlaMensajeError(obj.message);
			CerrarWaiting();
		});
	}
}

function BuscarCuentaFinContableTabClick() {
}

function BuscarPosTabClick() {
}

function NuevoMedioDePago() {
}

function NuevaOpcionCuota() {
}

function NuevaCuentaFinYContable() {
}

function NuevaPos() {
}

function ModificaMedioDePago() {

}

function ModificaOpcionesCuota() {
}

function ModificaCuentaFinYContable() {
}
function ModificaPos() {
}

function InicializaPantallaAbmMedioDePago() {
	var tb = $("#tbGridMedioDePago tbody tr");
	if (tb.length === 0) {
		$("#divFiltro").collapse("show")
	}

	$("#lbRel01").text("Medios de Pago");

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
	activarBotones(false);
	CerrarWaiting();
	return true;
}

function selectRegDbl(x, gridId) {
	AbrirWaiting("Espere mientras se busca el elemento seleccionado...");
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selectedEdit-row");
	regSelected = x;

	switch (gridId) {
		case Grids.GridMedioDePago:
			var ins_id = x[0].cells[0].innerText.trim();
			if (ins_id !== "") {
				insIdRow = x[0];
				insId = ins_id;
				BuscarMedioDePago(ins_id);
				//BuscarFormaDePago();
				//BuscarOtrosContactos();
				//BuscarNotas();
				//BuscarObservaciones();
				activarBotones(true);
				$(".activable").prop("disabled", true);
				$("#btnDetalle").prop("disabled", false);
				$("#divFiltro").collapse("hide");
				$("#divDetalle").collapse("show");
				$("#IdSelected").val(insId);
				posicionarRegOnTop(x);
			}
			break;
		case Grids.GridOpcionesCuotas:
			var insId = x.cells[2].innerText.trim();
			var cuota = x.cells[0].innerText.trim();
			var data = { insId, cuota };
			AbrirWaiting();
			PostGenHtml(data, buscarOpcionCuotaUrl, function (obj) {
				$("#divOpcionesCuotasSelected").html(obj);
				$("#IdSelected").val(cuota);
				$(".activable").prop("disabled", true);
				activarBotones(true);
				CerrarWaiting();
			}, function (obj) {
				ControlaMensajeError(obj.message);
				CerrarWaiting();
			});
			break;
		case Grids.GridCuentaFinYConta:
			
			break;
		default:
	}
}

function BuscarMedioDePago(insId) {
	var data = { insId };
	AbrirWaiting();
	PostGenHtml(data, buscarMedioDePagoUrl, function (obj) {
		$("#divDatosMedioDePago").html(obj);
		$("#IdSelected").val($("#MedioDePago_Ins_Id").val());
		$(".activable").prop("disabled", true);
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function buscarMediosDePago(pag, esBaja = false) {
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
	if (esBaja)
		buscaNew = true;

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