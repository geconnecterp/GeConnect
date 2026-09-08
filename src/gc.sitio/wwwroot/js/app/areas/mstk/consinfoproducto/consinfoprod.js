/* ######	INICIO Componente de info adicional de producto ###### */
const mostrarInfoProd = true;
const mostrarInfoProdStkA = true;
const mostrarInfoProdStkD = true;
const mostrarInfoProdStkBox = true;
const mostrarInfoProdStkMovM = true;
const mostrarInfoProdStkMovS = true;
const mostrarInfoProdStkMovD = true;
const mostrarInfoProdSustituto = true;
const pasarAdmLogueo = false;

$(function () {
	$("#btnCancel").on("click", function () {
		AbrirWaiting();
		LimpiarDatosDelFiltroInicial();
		InicializarDatosEnSesion();
		CerrarWaiting();
		setTimeout(() => {
			$('#divDetalle').collapse('hide');
		}, 200);
	});

	// Cuando se muestra el filtro → ocultar detalle y opciones
	$('#divFiltro').on('shown.bs.collapse', function (e) {
		if (e.target.id === 'divFiltro') { // aseguramos que sea el filtro
			$('#divDetalle').collapse('hide');
		}
	});

	// Cuando se oculta el filtro → mostrar detalle
	$('#divFiltro').on('hidden.bs.collapse', function (e) {
		setTimeout(() => {
			if (e.target.id === 'divFiltro') {
				const $tabla = $('#tbListaInfoProducto');
				if ($tabla.length > 0) { // existe en el DOM
					const filas = $tabla.find('tbody tr').length;

					if (filas > 0) {
						//$('#divDetalle').collapse('show');
						console.log("divDetalle > show");
					} else {
						//$('#divDetalle').collapse('hide');
						console.log("divDetalle > hide");
					}
				} else {
					console.warn("La tabla #tbListaInfoProducto no existe en el DOM");
				}
			}
		}, 1000);
	});

	$('#divFiltro').on('shown.bs.collapse', function () {
		console.log("divFiltro > show");
	});

	// Controlar directamente el estado de divDetalle
	$('#divDetalle').on('shown.bs.collapse', function (e) {
		if (e.target.id === 'divDetalle') {
			const $tabla = $('#tbListaInfoProducto');
			if ($tabla.length > 0) { // existe en el DOM
				const filas = $tabla.find('tbody tr').length;
				if (filas > 0) {
				} else {
				}
			}
			else {
			}
		}
	});

	$('#divDetalle').on('hidden.bs.collapse', function (e) {
		if (e.target.id === 'divDetalle') {
		}
	});

	$("input#Rel03").on("click", function () {
		$("input#Rel03").val("");
		$("#Rel03Item").val("");
	});
	AddEventListenerToGrid("tbListaInfoProducto");

	$("#btnBuscar").on("click", function () {
		dataBak = "";
		pagina = 1;
		BuscarProductos(pagina);
	});

	$(document).on("change", "#listaLs02", ControlalistaRubroSelected);
	$(document).on("change", "#listaLs03", ControlalistaFamiliaSelected);
	$("#Rel03List").on("dblclick", 'option', function () { $(this).remove(); })
	$("#Rel02List").on("dblclick", 'option', function () { $(this).remove(); })

	$("#chkRel02").on("click", function () {
		if ($("#chkRel02").is(":checked")) {
			$("#listaLs02").prop("disabled", false);
			$("#Rel02List").prop("disabled", false);
		}
		else {
			$("#listaLs02").prop("disabled", true);
			$("#Rel02List").prop("disabled", true);
		}
	})

	$("#chkRel03").on("click", function () {
		if ($("#chkRel03").is(":checked")) {
			$("#listaLs03").prop("disabled", false);
			$("#Rel03List").prop("disabled", false);
		}
		else {
			$("#listaLs03").prop("disabled", true);
			$("#Rel03List").prop("disabled", true);
		}
	})

	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacionPI(div);
	});
	funcCallBack = BuscarProductos;
	CargarRubros();
	InicializaPantalla();
	return true;
});


function BuscarProductos(pag = 1) {
	AbrirWaiting();
	var Tipo = tipoDeOperacion;
	var Buscar = $("#Buscar").val();
	var Id = $("#Id").val();
	var Id2 = $("#Id2").val();
	var Rel01 = [];
	var Rel02 = [];
	var Rel03 = [];
	$("#Rel01List").children().each(function (i, item) { Rel01.push($(item).val()) });
	$("#Rel02List").children().each(function (i, item) { Rel02.push($(item).val()) });
	$("#Rel03List").children().each(function (i, item) {
		var aux = { Id: $(item).val(), Descripcion: $(item).text() };
		Rel03.push(aux);
	});

	var Opt1 = $("#chk01")[0].checked
	var Opt2 = $("#chk02")[0].checked
	var Opt3 = $("#chk03")[0].checked
	var Opt4 = $("#chk04")[0].checked
	var Opt5 = $("#chk05")[0].checked
	var Opt6 = $("#chk06")[0].checked

	var buscaNew = true;
	pagina = pag;
	Pagina = pag;
	var sort = null;
	var sortDir = null
	var data2 = { sort, sortDir, Pagina, buscaNew }
	var data1 = { Tipo, Buscar, Id, Id2, Rel01, Rel02, Rel03, Opt1, Opt2, Opt3, Opt4, Opt5, Opt6 };
	var data = $.extend({}, data1, data2);
	PostGenHtml(data, BuscarInfoProductosURL, function (obj) {
		$("#divListaProducto").html(obj);
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
		AgregarHanlderColumnaDescripcion();
		CerrarWaiting();
		return true
	});
}

function selectListaProductoRow(x) {
	$("#tbListaInfoProducto tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
	const id = x.getAttribute("data-id");
	const ctaId = x.getAttribute("data-cta-id");
	const ctaDeno = x.getAttribute("data-cta-denominacion");
	console.log("Producto ID:", id);
	console.log("Cuenta ID:", ctaId);
	if (id) {
		pIdSeleccionado = id;
		ctaIdDeProdSeleccionado = ctaId;
		ctaDenoProdSeleccionado = ctaDeno;

		const el = document.getElementById("divInfo");

		if (!el || el.style.display === "none") {
			return;
		}
		else {
			/* ######	INICIO Componente de info adicional de producto ###### */
			//BuscarInfoAdicional();
			// disparar evento custom con datos del producto
			$(document).trigger("productoSeleccionadoParaInfoAdicional", {
				p_id: id,
				ctaId: ctaId,
				ctaDeno: ctaDeno
			});
			/* ######	FIN Componente de info adicional de producto ###### */
		}
	}
	else {
		pIdSeleccionado = "";
	}
}

function ControlalistaFamiliaSelected() {
	var item = $("#listaLs03").val();
	var desc = $("#listaLs03 option:selected").text();
	if ($("#Rel03List").has('option:contains("' + item + '")').length === 0 && $("#Rel03List").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#Rel03List").append(opc);
	}
}

function ControlalistaRubroSelected() {
	var item = $("#listaLs02").val();
	var desc = $("#listaLs02 option:selected").text();
	if ($("#Rel02List").has('option:contains("' + item + '")').length === 0 && $("#Rel02List").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#Rel02List").append(opc);
	}
}

function AgregarHanlderColumnaDescripcion() {
	$(document)
		.off("click", "[data-action='info-producto']")
		.on("click", "[data-action='info-producto']", function (e) {

			e.stopPropagation();
			e.preventDefault();
			AbrirInfoProducto();
		});
}

function AbrirInfoProducto() {
	//e.preventDefault();

	if (pIdSeleccionado && pIdSeleccionado !== "") {
		$("#divInfoAdicionaDeProducto").collapse("toggle");

		setTimeout(() => {
			invocarComponenteDeInfoAdicionalDeProd({
				p_id: pIdSeleccionado,
				mostrarInfoProd,
				mostrarInfoProdStkA,
				mostrarInfoProdStkD,
				mostrarInfoProdStkBox,
				mostrarInfoProdStkMovM,
				mostrarInfoProdStkMovD,
				mostrarInfoProdStkMovS,
				mostrarInfoProdSustituto,
				pasarAdmLogueo,
			});
		}, 500);
	} else {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un producto.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
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

function InicializarDatosEnSesion() {
	var data = {};
	PostGen(data, inicializarDatosEnSesionUrl, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
		}
	});
}


function LimpiarDatosDelFiltroInicial() {
	$("input#Rel01").val("");
	$("#Rel01Item").val("");
	$("#Rel01List").empty();
	$("#chkRel01").prop('checked', false);
	$("#chkRel01").trigger("change");
	$("input#Rel01").prop('disabled', true);
	$("#Rel01List").prop('disabled', true);

	limpiarListaLs03();
	$("#Rel03List").empty();
	$("#chkRel03").prop('checked', false);
	$("#chkRel03").trigger("change");
	$("#listaLs03").prop('disabled', true);
	$("#Rel03List").prop('disabled', true);
	$("#chkRel03").prop('disabled', true);

	$("#listaLs02").val("");
	$("#Rel02Item").val("");
	$("#Rel02List").empty();
	$("#chkRel02").prop('checked', false);
	$("#chkRel02").trigger("change");
	$("#listaLs02").prop('disabled', true);
	$("#Rel02List").prop('disabled', true);

	$("#chk01").prop('checked', false);
	$("#chk01").trigger("change");
	$("#chk02").prop('checked', false);
	$("#chk02").trigger("change");
	$("#chk03").prop('checked', false);
	$("#chk03").trigger("change");
	$("#chk04").prop('checked', false);
	$("#chk04").trigger("change");
	$("#chk05").prop('checked', false);
	$("#chk05").trigger("change");

	$("#chkDescr").prop('checked', false);
	$("#chkDescr").trigger("change");
	$("input#Buscar").val("");
	$("input#Buscar").prop('disabled', true);

	$("#chkDesdeHasta").prop('checked', false);
	$("#chkDesdeHasta").trigger("change");
	$("input#Id").val("");
	$("input#Id").prop('disabled', true);
	$("input#Id2").val("");
	$("input#Id2").prop('disabled', true);
}

function InicializaPantalla() {
	var tb = $("#tbListaInfoProducto tbody tr");
	if (tb.length === 0) {
		$("#divFiltro").collapse("show")
	}

	$("#lbRel01").text("Proveedor");
	$("#lbRel02").text("Rubro");
	$("#lbRel03").text("Familia");
	$("#lbChkDescr").text("Descripción Producto");
	$("#lbDescr").html("Desc");

	$("#lbchk01").text("Alta Rotación");
	$("#lbchk02").text("Con PI");
	$("#lbchk03").text("Con OC");
	$("#lbchk04").text("Sin Stk");
	$("#lbchk05").text("Con Stk a Vencer");
	$("#lbchk06").text("Ofe./Pro");

	$("#lbChkDesdeHasta").text("ID Producto");

	$(".activable").prop("disabled", true);
	$("#chkRel03").prop("disabled", true);
	$("#listaLs02").prop("disabled", true);

	CerrarWaiting();
	return true;
}

$("#Rel01List").off("dblclick");
$("#Rel01List").on("dblclick", 'option', function () {
	$(this).remove();
	if ($("#Rel01List")[0].length === 1) {
		$("#chkRel03").prop("disabled", false);
		CargarFamiliaLista($("#Rel01List")[0][0].value);
	}
	else {
		$("#chkRel03").prop("disabled", true);
	}
})

$("#Rel01").on("click", function () { $(this).val(""); });

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
		if ($("#Rel01List")[0].length === 1) {
			$("#chkRel03").prop("disabled", false);
			CargarFamiliaLista(ui.item.id);
		}
		else {
			$("#chkRel03").prop("disabled", true);
			$("#listaLs03").prop("disabled", true).val("");
			$("#Rel03List").prop("disabled", true).empty();
			$("#chkRel03")[0].checked = false;
		}

		// *** CLAVE ***
		// Evita que jQuery UI vuelva a poner el value original con "#"
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

function CargarFamiliaLista(id) {
	var ctaId = id;
	data = { ctaId };
	PostGenHtml(data, BuscarProveedoresFamiliaURL, function (obj) {
		$("#divLs03").html(obj);
		CerrarWaiting();
		return true
	});
}

function CargarRubros() {
	data = {};
	PostGenHtml(data, BuscarRubrosURL, function (obj) {
		$("#divLs02").html(obj);
		$("#divLs02").attr("class", "col-md-6 col-sm-6");
		$("#listaLs02").prop("disabled", true);
		CerrarWaiting();
		return true
	});
}

function presentaPaginacionPI(div) {
	div.pagination({
		items: totalRegs,
		itemsOnPage: pagRegs,
		cssStyle: "dark-theme",
		currentPage: pagina,
		onPageClick: function (num) {
			BuscarProductos(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltro").collapse("hide")
	return true;
}