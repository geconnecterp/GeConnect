$(function () {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		cargaPaginacion();
	});

	InicializarCamposEnFiltros(false);

	$(document).off("click", "#btnImprimir");
	$(document).on("click", "#btnImprimir", ControlaImprimirSelected);
	$(document).off("click", "#btnCancel");
	$(document).on("click", "#btnCancel", ControlaCancelar);
	$(document).off("change", "#listaTipoMovimientos");
	$(document).on("change", "#listaTipoMovimientos", ControlalistaTipoMovSelected);
	$(document).off("change", "#listaDepositos");
	$(document).on("change", "#listaDepositos", ControlalistaDepositosSelected);
	$(document).off("change", "#listaBoxs");
	$(document).on("change", "#listaBoxs", ControlalistaBoxsSelected);

	$("#TipoMovimientosList").off("dblclick");
	$("#TipoMovimientosList").on("dblclick", 'option', function () { $(this).remove(); })
	$("#DepositosList").off("dblclick");
	$("#DepositosList").on("dblclick", 'option', function () {
		$(this).remove();
		ControlarDepositosYBoxes();
	})
	$("#BoxsList").off("dblclick");
	$("#BoxsList").on("dblclick", 'option', function () { $(this).remove(); })
	$("#Rel01List").off("dblclick");
	$("#Rel01List").on("dblclick", 'option', function () { $(this).remove(); })

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

	$("#Rel01").off("click");
	$("#Rel01").on("click", function () { $(this).val(""); });

	$("#btnBuscar").off('click');
	$("#btnBuscar").on("click", function () {
		if (!ValidarRangoFechas()) {
			return; // ← NO ejecutar la búsqueda si falla
		}

		dataBak = "";
		pagina = 1;
		BuscarMovStock(pagina);
	});

	funcCallBack = BuscarMovStock;
});

function BuscarMovStock(pag) {
	AbrirWaiting();
	var lTipoMov = [];
	var lTipoMovTextos = "";
	var lDep = [];
	var lDepTextos = "";
	var lBox = [];
	var lBoxTextos = "";
	var lProv = [];
	var lProvTextos = "";
	var temp = ObtenerFiltroLista("#chkTipo", "#TipoMovimientosList");
	lMovTipo = temp.ids;
	lMovTipoTextos = temp.textos;

	temp = ObtenerFiltroLista("#chkDepositos", "#DepositosList");
	lDep = temp.ids;
	lDepTextos = temp.textos;

	temp = ObtenerFiltroLista("#chkBox", "#BoxsList");
	lBox = temp.ids;
	lBoxTextos = temp.textos;

	temp = ObtenerFiltroLista("#chkRel01", "#Rel01List");
	lProv = temp.ids;
	lProvTextos = temp.textos;

	var pId = $("#Texto").val();
	pId = (pId && pId.trim() !== "" ) ? pId.trim() : "%";

	temp = [];

	var desde = $("#FechaDesde").val();
	var hasta = $("#FechaHasta").val();

	var buscaNew = true;
	pagina = pag;
	Pagina = pag;
	var sort = null;
	var sortDir = null
	var data2 = { sort, sortDir, Pagina, buscaNew }
	var data1 = {
		desde,
		hasta,
		pId,
		lMovTipo,
		lDep,
		lBox,
		lProv,
		lMovTipoTextos,
		lDepTextos,
		lBoxTextos,
		lProvTextos,
		pIdTextos: (pId != "" && pId != "%" && pId != '%') ? pId.trim() : ""
	};
	var data = $.extend({}, data1, data2);

	PostGenHtml(data, buscarMovStockProductosURL, function (obj) {
		$("#divDetalle").html(obj);
		$("#divFiltros").collapse("hide");
		$("#divDetalle").collapse("show");
		$("#btnImprimir").show();
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
		return true
	});
}

function ImprimirListaMovStk_Generada() {
	ReseteoDeReportes();
	setTimeout(() => {
		var lTipoMov = [];
		var lTipoMovTextos = "";
		var lDep = [];
		var lDepTextos = "";
		var lBox = [];
		var lBoxTextos = "";
		var lProv = [];
		var lProvTextos = "";
		var temp = ObtenerFiltroLista("#chkTipo", "#TipoMovimientosList");
		lTipoMov = temp.ids;
		lTipoMovTextos = temp.textos;

		temp = ObtenerFiltroLista("#chkDepositos", "#DepositosList");
		lDep = temp.ids;
		lDepTextos = temp.textos;

		temp = ObtenerFiltroLista("#chkBox", "#BoxsList");
		lBox = temp.ids;
		lBoxTextos = temp.textos;

		temp = ObtenerFiltroLista("#chkRel01", "#Rel01List");
		lProv = temp.ids;
		lProvTextos = temp.textos;

		var pId = $("#Texto").val();
		pId = (pId && pId.trim() !== "") ? pId.trim() : "%";

		temp = [];

		var desde = $("#FechaDesde").val();
		var hasta = $("#FechaHasta").val();

		// 🔥 Construcción del string de filtros
		var filtrosDesc = [];

		filtrosDesc.push(ConstruirDescripcionFiltro("Tipo Mov.", "#chkTipo", "#TipoMovimientosList"));
		filtrosDesc.push(ConstruirDescripcionFiltro("Proveedores", "#chkRel01", "#Rel01List"));
		filtrosDesc.push(ConstruirDescripcionFiltro("Depósitos", "#chkDepositos", "#DepositosList"));
		filtrosDesc.push(ConstruirDescripcionFiltro("Box", "#chkBox", "#BoxsList"));
		if (pId != "" && pId != "%" && pId != '%') {
			filtrosDesc.push(`Producto: ${pId}`);
		}

		// Limpieza: eliminar vacíos
		filtrosDesc = filtrosDesc.filter(x => x !== "");

		// String final
		var filtrosString = filtrosDesc.join(" | ");

		var data = { lTipoMov: lTipoMov.join(","), lProv: lProv.join(","), lDep: lDep.join(","), lBox: lBox.join(","), desde, hasta, pId, filtrosString };

		cargarReporteEnArre(96, data, "CONSULTA MOVIMIENTO DE STOCK", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function ConstruirDescripcionFiltro(nombre, idCheckbox, idListBox) {

	const activo = $(idCheckbox).is(":checked");

	// Si NO está activo → devolver "Todos"
	if (!activo) {
		return `${nombre}: Todos`;
	}

	// Si está activo → obtener valores
	const valores = [];
	$(idListBox + " option").each(function () {
		const txt = $(this).text().trim();

		// Si viene "%" → reemplazar por "Todos"
		if (txt === "%" || txt === "") {
			valores.push("Todos");
		} else {
			valores.push(txt);
		}
	});

	// Si no hay valores → "Todos"
	if (valores.length === 0) {
		return `${nombre}: Todos`;
	}

	return `${nombre}: ${valores.join(", ")}`;
}



function ValidarRangoFechas() {

	const desdeStr = $("#FechaDesde").val();
	const hastaStr = $("#FechaHasta").val();

	if (!desdeStr || !hastaStr) {
		AbrirMensaje("ATENCIÓN", "Debe especificar ambas fechas (Desde y Hasta).", null, false, ["Aceptar"], "error!", null);
		return false;
	}

	const desde = new Date(desdeStr);
	const hasta = new Date(hastaStr);

	if (isNaN(desde.getTime()) || isNaN(hasta.getTime())) {
		AbrirMensaje("ATENCIÓN", "Las fechas ingresadas no son válidas.", null, false, ["Aceptar"], "error!", null);
		return false;
	}

	if (hasta < desde) {
		AbrirMensaje("ATENCIÓN", "La fecha Hasta no puede ser menor que la fecha Desde.", null, false, ["Aceptar"], "error!", null);
		return false;
	}

	// Diferencia en días
	const diffMs = hasta - desde;
	const diffDias = diffMs / (1000 * 60 * 60 * 24);

	// Si hay texto → rango permitido = 65 días
	// Si NO hay texto → rango permitido = 35 días
	const hayTexto = $("#Texto").val().trim().length > 0;
	const maxDias = hayTexto ? 65 : 35;

	if (diffDias > maxDias) {
		const msg = hayTexto
			? `El rango de fechas no puede superar los ${maxDias} días cuando se especifica un Id de Producto.`
			: `El rango de fechas no puede superar los ${maxDias} días.`;

		AbrirMensaje("ATENCIÓN", msg, null, false, ["Aceptar"], "error!", null);
		return false;
	}

	return true;
}


function ObtenerFiltroLista(idCheckbox, idListBox) {

	const estaChequeado = $(idCheckbox).is(":checked");
	const ids = [];
	const textos = [];

	// Tomar valores y textos del ListBox
	$(idListBox + " option").each(function () {
		ids.push($(this).val());
		textos.push($(this).text());
	});

	// NUEVA REGLA:
	// Si el checkbox está deschequeado → devolver "%" y "Todos"
	if (!estaChequeado) {
		return {
			ids: ["%"],
			textos: "Todos"
		};
	}

	// Si está chequeado y no hay valores → devolver "%"
	if (estaChequeado && ids.length === 0) {
		return {
			ids: ["%"],
			textos: "Todos"
		};
	}

	// Caso normal: checkbox chequeado y con items
	return {
		ids: ids,
		textos: textos.join(", ")
	};
}


function ControlarDepositosYBoxes() {

	const cantDep = $("#DepositosList option").length;

	// Caso especial: usuario deschequea chkDepositos
	if (!$("#chkDepositos").is(":checked")) {

		// Desmarcar y bloquear checkbox Box
		$("#chkBox").prop("checked", false);
		$("#chkBox").prop("disabled", true).addClass("checkbox-readonly");

		// Limpiar selects
		$("#listaBoxs").val("");
		$("#BoxsList").empty();

		// Deshabilitar selects
		$("#listaBoxs").prop("disabled", true);
		$("#BoxsList").prop("disabled", true);

		return;
	}

	// Caso 1: EXACTAMENTE 1 depósito seleccionado
	if (cantDep === 1) {

		// Habilitar checkbox Box
		$("#chkBox").prop("checked", true);
		$("#chkBox").prop("disabled", false).removeClass("checkbox-readonly");

		// Habilitar selects
		$("#listaBoxs").prop("disabled", false);
		$("#BoxsList").prop("disabled", false);

		// Obtener el ID del depósito seleccionado
		const depId = $("#DepositosList option").first().val();

		// Recargar los boxes del depósito
		CargarBoxesDelDepositoSeleccionado(depId);

		return;
	}

	// Caso 2: 2 o más depósitos → bloquear Boxs
	if (cantDep >= 2) {

		// Desmarcar y bloquear checkbox Box
		$("#chkBox").prop("checked", false);
		$("#chkBox").prop("disabled", true).addClass("checkbox-readonly");

		// Limpiar selects
		$("#listaBoxs").val("");
		$("#BoxsList").empty();

		// Deshabilitar selects
		$("#listaBoxs").prop("disabled", true);
		$("#BoxsList").prop("disabled", true);

		return;
	}

	// Caso 3: 0 depósitos → limpiar todo
	if (cantDep === 0) {

		$("#chkBox").prop("checked", false);
		$("#chkBox").prop("disabled", true).addClass("checkbox-readonly");

		$("#listaBoxs").val("");
		$("#BoxsList").empty();

		$("#listaBoxs").prop("disabled", true);
		$("#BoxsList").prop("disabled", true);

		return;
	}
}



function CargarBoxesDelDepositoSeleccionado(depId) {
	var data = { depId };
	PostGenHtml(data, cargarBoxesDesdeDepositoUrl, function (obj) {
		$("#divBoxes").html(obj);
		return true;
	});
}

function ControlalistaTipoMovSelected() {
	var item = $("#listaTipoMovimientos").val();
	var desc = $("#listaTipoMovimientos option:selected").text();
	if ($("#TipoMovimientosList").has('option:contains("' + item + '")').length === 0 && $("#TipoMovimientosList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#TipoMovimientosList").append(opc);
	}
}

function ControlalistaBoxsSelected() {
	var item = $("#listaBoxs").val();
	var desc = $("#listaBoxs option:selected").text();
	if ($("#BoxsList").has('option:contains("' + item + '")').length === 0 && $("#BoxsList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#BoxsList").append(opc);
	}
}

function ControlalistaDepositosSelected() {
	var item = $("#listaDepositos").val();
	var desc = $("#listaDepositos option:selected").text();
	if ($("#DepositosList").has('option:contains("' + item + '")').length === 0 && $("#DepositosList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#DepositosList").append(opc);
	}
	ControlarDepositosYBoxes();
}

function ControlaCancelar() { }
function ControlaImprimirSelected() {
	const filasReales = $("#tbGridProductos tbody tr")
		.filter(function () {
			// Fila informativa tiene un único TD con colspan
			return $(this).find("td[colspan]").length === 0;
		});

	if (filasReales.length === 0) {
		AbrirMensaje("ATENCIÓN", "No hay datos generar el reporte.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		ImprimirListaMovStk_Generada();
	}
}

function InicializarCamposEnFiltros(vieneDeCancelar) {
	if (!vieneDeCancelar) {
	}
	$("#btnImprimir").hide();
	$("#lbChkDesdeHasta").text("Fechas");
	$("#lbChkDescr").text("ID Producto o Descripción");
	$("#lbTipo").text("Tipo de Movimiento");
	$("#lbDepositos").text("Depósitos");
	$("#lbBox").text("Box");
	$("#lbRel01").text("Proveedor");


	$("#chkDesdeHasta").prop('checked', true);
	$("#chkDesdeHasta").trigger("change");
	$("#chkDesdeHasta").prop('disabled', true);

	$("#chkDescr").prop('checked', false);
	$("#chkDescr").trigger("change");

	$("#chkTipo").prop('checked', false);
	$("#chkTipo").trigger("change");

	$("#chkDepositos").prop('checked', false);
	$("#chkDepositos").trigger("change");
	$("#chkBox").prop('checked', false);
	$("#chkBox").trigger("change");

	$("#chkRel01").prop('checked', false);
	$("#chkRel01").trigger("change");

	$("#TipoMovimientosList").empty();
	$("#DepositosList").empty();
	$("#BoxsList").empty();
	$("#Rel01List").empty();

	$("#listaTipoMovimientos").val("");
	$("#listaDepositos").val("");
	$("#listaBoxs").val("");
	$("#Rel01Item").val("");

	$("#divFiltros").collapse("show");
	$("#divDetalle").collapse("hide");
	if (!vieneDeCancelar) {
		HandlerCheckBox();
	}
}

function HandlerCheckBox() {
	$("#chkDescr").on("click", function () {
		if ($("#chkDescr").is(":checked")) {
			$("#Texto").prop("disabled", false);
			$("#Texto").trigger("focus");
		}
		else {
			$("#Texto").prop("disabled", true);
			$("#Texto").val("");
		}
	});
	$("#chkTipo").on("click", function () {
		if ($("#chkTipo").is(":checked")) {
			$("#listaTipoMovimientos").prop("disabled", false);
			$("#TipoMovimientosList").prop("disabled", false);
			$("#listaTipoMovimientos").trigger("focus");
		}
		else {
			$("#listaTipoMovimientos").prop("disabled", true);
			$("#TipoMovimientosList").prop("disabled", true);
			$("#listaTipoMovimientos").val("");
			$("#TipoMovimientosList").empty();
		}
	});
	$("#chkDepositos").on("click", function () {
		if ($("#chkDepositos").is(":checked")) {
			$("#listaDepositos").prop("disabled", false);
			$("#DepositosList").prop("disabled", false);
			$("#listaDepositos").trigger("focus");
		}
		else {
			$("#listaDepositos").prop("disabled", true);
			$("#DepositosList").prop("disabled", true);
			$("#listaDepositos").val("");
			$("#DepositosList").empty();
			ControlarDepositosYBoxes();
		}
	});
	$("#chkBox").on("click", function () {
		if ($("#chkBox").is(":checked")) {
			$("#listaBoxs").prop("disabled", false);
			$("#BoxsList").prop("disabled", false);
			$("#listaBoxs").trigger("focus");
		}
		else {
			$("#listaBoxs").prop("disabled", true);
			$("#BoxsList").prop("disabled", true);
			$("#listaBoxs").val("");
			$("#BoxsList").empty();
		}
	});
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

}

function cargaPaginacion() {
	$("#divPaginacion").pagination({
		items: totalRegs,
		itemsOnPage: pagRegs,
		cssStyle: "dark-theme",
		currentPage: pagina,
		onPageClick: function (num) {
			BuscarMovStock(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltro").collapse("hide")
	return true;
}

$("#Rel01").autocomplete({
	source: function (request, response) {

		data = { prefix: request.term }; Rel01

		$.ajax({
			url: autoComRel01Url,
			type: "POST",
			dataType: "json",
			data: data,
			success: function (obj) {
				response($.map(obj, function (item) {
					var texto = item.descripcion;
					return {
						label: texto,
						value: item.descripcion,
						id: item.id,
						prov: item.provId
					};
				}));
			}
		})
	},
	minLength: 3,

	focus: function (event, ui) {
		// evita que el # aparezca mientras navegas con flechas
		const partes = ui.item.value.split("#");
		$("#Rel01").val(partes.join(" "));
		return false;
	},

	select: function (event, ui) {
		const partes = ui.item.value.split("#");
		const textoSinSeparador = partes.join(" ");

		// Mostrar SIN el "#"
		$("#Rel01").val(textoSinSeparador);

		if ($("#Rel01List").has('option:contains("' + ui.item.id + '")').length === 0) {
			$("#Rel01Item").val(ui.item.id);
			var opc = "<option value=" + ui.item.id + ">" + textoSinSeparador + "</option>"
			$("#Rel01List").append(opc);
		}

		event.preventDefault();
		return true;
	}
}).autocomplete("instance")._renderItem = function (ul, item) {

	const partes = item.label.split("#");

	const ctaLista = partes[0];
	const tipoDesc = partes[1];

	return $("<li>")
		.append(
			`<div>
                <span style="font-weight:bold; font-size:14px;">
                    ${ctaLista}
                </span>
                <span style="font-size:13px; color:#555;">
                    ${tipoDesc}
                </span>
            </div>`
		)
		.appendTo(ul);
};