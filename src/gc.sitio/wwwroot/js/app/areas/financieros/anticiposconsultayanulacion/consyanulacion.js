$(function () {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		cargaPaginacion();
	});

	$("#chkDesdeHasta").prop('checked', true);
	$("#chkDesdeHasta").trigger("change");
	$("#chkDesdeHasta").prop("disabled", true);

	InicializarCamposEnFiltros();

	$("#btnFiltro").on("click", function () {
		if ($("#divFiltros").hasClass("show")) {
			$("#divFiltros").collapse("hide");
			$("#divDetalle").collapse("show");
		}
		else {
			$("#divFiltros").collapse("show");
			$("#divDetalle").collapse("hide");
		}
	});
	$("input#Rel01").on("click", function () {
		$("input#Rel01").val("");
		$("#Rel01Item").val("");
	});

	$("#btnBuscar").on("click", function () {
		dataBak = "";
		pagina = 1;
		BuscarAnticiposDeEmpleados(pagina);
	});

	$("#Rel01List").on("dblclick", 'option', function () { $(this).remove(); })
	$("#UsuarioList").on("dblclick", 'option', function () { $(this).remove(); })

	funcCallBack = BuscarAnticiposDeEmpleados;
});

function cargaPaginacion() {
	$("#divPaginacion").pagination({
		items: totalRegs,
		itemsOnPage: pagRegs,
		cssStyle: "dark-theme",
		currentPage: pagina,
		onPageClick: function (num) {
			BuscarAnticiposDeEmpleados(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltro").collapse("hide")
	return true;
}

function BuscarAnticiposDeEmpleados(pag) {
	AbrirWaiting();
	var desde = $("#Date1").val();
	var hasta = $("#Date2").val();
	var cta_list = [];
	var tipo_list = [];
	var usu_list = [];
	if ($("#chkRel01").is(":checked")) {
		$("#Rel01List").children().each(function (i, item) { cta_list.push($(item).val()) });
	}
	if ($("#chkTipo").is(":checked")) {
		var tipoVal = $("#listaTipo").val();
		tipo_list.push(tipoVal);
	}
	if ($("#chkUsuario").is(":checked")) {
		$("#UsuarioList").children().each(function (i, item) { usu_list.push($(item).val()) });
	}
	var cta = $("#chkRel01")[0].checked;
	var tipo = $("#chkTipo")[0].checked;
	var usu = $("#chkUsuario")[0].checked;
	var data1 = { desde, hasta, cta_list, cta, tipo_list, tipo, usu_list, usu };
	var buscaNew = true;
	var sort = null;
	var sortDir = null
	pagina = pag;
	var data2 = { sort, sortDir, pag, buscaNew }
	var data = $.extend({}, data1, data2);
	PostGenHtml(data, buscarAnticiposDeEmpleadosURL, function (obj) {
		CerrarWaiting();
		$("#divAntFinanEmp").html(obj);
		$("#divAntFinanEmpDetalle").empty();
		$("#divFiltros").removeClass("show").addClass("collapse");
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
				$("#divPaginacion").removeClass("collapse");
			}

		});
		an_compte_selected = "";
		CerrarWaiting();
		return true
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function selectReg(x, gridId) {
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
	if (gridId === "tbGridAnticipoFinEmp") {
		let anCompte = $(x).data("an-compte");

		CargarDetalleDeAnticipo(anCompte);
	}
}

function CargarDetalleDeAnticipo(anCompte) {
	AbrirWaiting(`Cargando detalle de Anticipo N° ${anCompte}`)
	var data = { anCompte };
	PostGenHtml(data, cargarDetalleDeAnticipoUrl, function (obj) {
		CerrarWaiting();
		$("#divAntFinanEmpDetalle").html(obj);
		return true
	}, function (obj) {
		CerrarWaiting();
		console.log(obj);
		ControlaMensajeError(obj.responseText);
	});
}

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function InicializarCamposEnFiltros() {
	$("#Date1, #Date2").on("blur", ValidarFechasClick);
	$("#chkRel01").on("click", function () {
		if ($("#chkRel01").is(":checked")) {
			$("#Rel01").prop("disabled", false);
			$("#Rel01List").prop("disabled", false);
			$("#Rel01").trigger("focus");
		}
		else {
			$("#Rel01").prop("disabled", true);
			$("#Rel01List").prop("disabled", true);
			$("#Rel01").val("");
			$("#Rel01List").empty();
		}
	});
	$("#chkTipo").on("click", function () {
		if ($("#chkTipo").is(":checked")) {
			$("#listaTipo").prop("disabled", false);
			$("#listaTipo").trigger("focus");
		}
		else {
			$("#listaTipo").val("");
			$("#listaTipo").prop("disabled", true);
		}
	});
	$("#chkUsuario").on("click", function () {
		if ($("#chkUsuario").is(":checked")) {
			$("#listaUsuario").prop("disabled", false);
			$("#UsuarioList").prop("disabled", false);
			$("#listaUsuario").trigger("focus");
		}
		else {
			$("#listaUsuario").prop("disabled", true);
			$("#UsuarioList").prop("disabled", true);
			$("#listaUsuario").val("");
			$("#UsuarioList").empty();
		}
	});

	$("#lbChkDesdeHasta").text("Desde / Hasta");
	$("#lbRel01").text("Cliente");
	$("#lbTipo").text("Tipo");
	$("#lbUsuario").text("Usuario");

	$("#Date1").prop("disabled", false);
	$("#Date2").prop("disabled", false);
	$("#divFiltros").collapse("show");
	$("#divDetalle").collapse("hide");
}

$("#Rel01").autocomplete({
	source: function (request, response) {

		data = { prefix: request.term }; /*Rel01*/

		$.ajax({
			url: autoComRel01Url,
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
		ctaIdSelected = ui.item.id;
		ctaDescSelected = ui.item.value;
		if ($("#Rel01List").has('option:contains("' + ui.item.id + '")').length === 0) {
			$("#Rel01Item").val(ui.item.id);
			var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
			$("#Rel01List").append(opc);
		}
		return true;
	}
});

function ValidarFechasClick() {
	const desde = $("#Date1").val();
	const hasta = $("#Date2").val();

	if (desde && hasta && desde > hasta) {
		AbrirMensaje("ATENCIÓN", "El valor de Fecha Desde no puede ser mayor a Fecha Hasta, revise.", function () {
			$("#msjModal").modal("hide");
			$("#Date1").val($("#Date2").val());
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}