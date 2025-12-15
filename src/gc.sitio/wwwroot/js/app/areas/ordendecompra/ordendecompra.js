let productoActualEnLista = null;

const mostrarInfoProd = true;
const mostrarInfoProdStkA = true;
const mostrarInfoProdStkD = true;
const mostrarInfoProdStkBox = true;
const mostrarInfoProdStkMovM = true;
const mostrarInfoProdStkMovS = true;
const mostrarInfoProdStkMovD = true;
const mostrarInfoProdSustituto = true;

if (!customElements.get('box-icon')) {
	customElements.define('box-icon', window.BoxIconElement.default);
}

$(function () {
	const pId = $("#hdnPid").val();
	const ctaId = $("#hdnCtaId").val();
	const ctaDenominacion = $("#hdnCtaDeno").val();

	// Validación: que no sean null, undefined ni string vacío
	if (pId && pId.trim() !== "" && ctaId && ctaId.trim() !== "") {
		console.log("Vista abierta desde Index con:", pId, ctaId, ctaDenominacion);
		// acá podés disparar lógica adicional
		// cargarVistaParcial(pId, ctaId);
		BuscarProductosDesdeNCPI(1, pId, ctaId, ctaDenominacion);
	} else {
		console.warn("pId o ctaId no son válidos:", pId, ctaId);
	}

	$("#btnCollapseSectionInfoProd").on("click", function (e) {
		e.preventDefault();

		if (pIdSeleccionado && pIdSeleccionado !== "") {
			// toggle manual
			$("#divInfoAdicionaDeProducto").collapse("toggle");

			// opcional: refrescar contenido si querés al abrir
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
			});
		} else {
			AbrirMensaje("ATENCIÓN", "Debe seleccionar un producto.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	});

	$("#tituloLegend").text("Productos a cargar");
	$("#chkRel01").prop('checked', true);
	$("#chkRel01").trigger("change");
	$("#chkRel01").prop("disabled", true);
	$("#Rel01").prop("disabled", false);
	$("input#Rel01").on("click", function () {
		$("input#Rel01").val("");
		$("#Rel01Item").val("");
		$("input#Rel03").val("");
		$("#Rel03Item").val("");
		$("#Rel03List").empty();
		$("input#Rel04").val("");
		$("#Rel04Item").val("");
		$("#chkRel03").prop('checked', false);
		$("#chkRel03").trigger("change");
		$("#chkRel04").prop('checked', false);
		$("#chkRel04").trigger("change");
		$("input#Rel03").prop("disabled", true);
		$("input#Rel04").prop("disabled", true);
	});
	//elimina item de la lista
	$("#Rel02List").on("dblclick", 'option', function () { $(this).remove(); })
	$("#Rel03List").on("dblclick", 'option', function () { $(this).remove(); })
	$("input#Rel03").on("click", function () {
		$("input#Rel03").val("");
		$("#Rel03Item").val("");
	});
	$("input#Rel04").on("click", function () {
		$("input#Rel04").val("");
		$("#Rel04Item").val("");
	});
	$("#btnBuscar").on("click", function () {
		if (ctaIdSelected == "") {
			AbrirMensaje("ATENCIÓN", "Debe seleccionar una cuenta.", function () {
				$("#msjModal").modal("hide");
				$("input#Rel01").focus();
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			dataBak = "";
			pagina = 1;
			BuscarProductos(pagina);
		}
	});
	$("#btnAbmAceptar").on("click", function () {
		ConfirmarOrdenDeCompra();
	});
	$("#btnCancel").on("click", function () {
		LimpiarDatosDelFiltroInicial();
		$("#btnFiltro").trigger("click");
	});
	$("#btnAbmCancelar").on("click", function () {
		InicializarDatosEnSesion();
		InicializaPantalla();
		LimpiarDatosDelFiltroInicial();
		$("#btnFiltro").trigger("click");
		$("#btnDetalle").trigger("click");
		$("#divDetalle").collapse("hide");
	});
	funcCallBack = BuscarProductos;
	InicializaPantalla();
	$("#Rel01").focus();
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacionOC(div);
	});
	$(document).on("change", "#listaOCPend", ControlaListaOcSelected);
	$(document).on("change", "#listaSucursales", ControlaSucursalSeleccionada);
	$(document).on("keypress", ".inputEditable", analizaEnterInput);

	$("#btnCollapseSection").on("click", btnCollapseSectionClicked);
	$("#tabResumen").on("click", function () {
		$("#btnAbmAceptar").prop("disabled", false);
		CargarResumenDeOc();
	});
	$("#tabNuevaOC").on("click", function () {
		$("#btnAbmAceptar").prop("disabled", true);
	});
	$("#tabProductos").on("click", function () {
		$("#btnAbmAceptar").prop("disabled", true);
	});
	$("#btnImprimirTemp").on("click", function () {
		ImprimirOC_Generada("07-00000121", "C0017180");
	});
	return true;
});

// Create our number formatter.
const formatter = new Intl.NumberFormat('en-US', {
	style: 'currency',
	currency: 'USD',
	trailingZeroDisplay: 'stripIfInteger'
});

const EstadoColor = {
	Activo: '#34dc22', //≈ Lima
	NoActivo: '#f74146', //≈ Sunset Orange
	Discontinuo: '#4180f7' //≈ Dodger Blue
}

function ControlaSucursalSeleccionada() {
	BuscarInfoAdicional();
}

function ControlaListaOcSelected() {
	if ($("#listaOCPend").val() != "")
		ocIdSelected = $("#listaOCPend").val();
	else
		ocIdSelected = "";
}

function addHandlerOnChkRel04_Click() {
	$("#chkRel04").on("click", function () {
		if ($("#chkRel04").is(":checked")) {
			$("#listaOCPend").prop("disabled", false);
			$("#listaOCPend").trigger("focus");

		}
		else {
			$("#listaOCPend").prop("disabled", true).val("");
			ocIdSelected = "";
		}
	});
}

function LimpiarDatosDelFiltroInicial() {
	$("input#Rel01").val("");
	$("#Rel01Item").val("");
	$("#Rel01List").empty();

	$("#chkRel04").prop('checked', false);
	$("#chkRel04").trigger("change");
	$("input#Rel04").val("");
	$("#Rel04Item").val("");
	$("input#Rel04").prop('disabled', true);

	$("input#Rel03").val("");
	$("#Rel03Item").val("");
	$("#Rel03List").empty();
	$("#chkRel03").prop('checked', false);
	$("#chkRel03").trigger("change");
	$("input#Rel03").prop('disabled', true);
	$("#Rel03List").prop('disabled', true);
	$("#chkRel03").prop('disabled', true);

	$("input#Rel02").val("");
	$("#Rel02Item").val("");
	$("#Rel02List").empty();
	$("#chkRel02").prop('checked', false);
	$("#chkRel02").trigger("change");
	$("input#Rel02").prop('disabled', true);
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
	$("#listaOCPend").empty();
	$("#divLstOcPendiente").html("");
}

function ConfirmarOrdenDeCompra() {
	AbrirMensaje("ATENCIÓN", "¿Confirma la generación de la Orden de Compra?", function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI": //Confirmar 
				var Oc_Compte = ocIdSelected;
				var Entrega_Fecha = $("#FechaEntrega").val();
				var Entrega_Adm = $("#listaSucEntrega").val()
				var Pago_Anticipado = 'N';
				if ($("#chkPagoAnticipado")[0].checked)
					Pago_Anticipado = 'S';
				var Pago_Fecha = $("#PagoPlazo").val();
				var Observaciones = $("#Obs").val();
				var Oce_Id = 'P';
				if ($("#chkDejarOCActiva")[0].checked)
					Oce_Id = 'C';
				var data = { Oc_Compte, Entrega_Fecha, Entrega_Adm, Pago_Anticipado, Pago_Fecha, Observaciones, Oce_Id };
				PostGen(data, "/Compras/ordendecompra/ConfirmarOrdenDeCompra", function (obj) {
					if (obj.error === true) {
						AbrirMensaje("ATENCIÓN", obj.msg, function () {
							$("#msjModal").modal("hide");
							return true;
						}, false, ["Aceptar"], "error!", null);
					}
					else {
						AbrirMensaje("ATENCIÓN", obj.msg, function () {
							$("#msjModal").modal("hide");
							if (obj.id != "") {
								console.log(obj.id); //Tomar este valor para imprimir.
								ImprimirOC_Generada(obj.id, ctaIdSelected);
							}
							InicializarDatosEnSesion();
							InicializaPantalla();
							LimpiarDatosDelFiltroInicial();
							$("#btnFiltro").trigger("click");
							$("#btnDetalle").trigger("click");
							$("#divDetalle").collapse("hide");
							return true;
						}, false, ["Aceptar"], "info!", null);
					}
				});
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;

	}, true, ["Aceptar", "Cancelar"], "question!", null);
}

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function ImprimirOC_Generada(ocCompte, ctaId) {
	ReseteoDeReportes();
	setTimeout(() => {
		let data = { oc_compte: ocCompte, ctaId: ctaId };
		cargarReporteEnArre(21, data, "ORDEN DE COMPRA", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function InicializarDatosEnSesion() {
	PostGen({}, inicializarDatosEnSesionURL, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			console.log(obj.msg);
		}
	});
}

function ActualizarGrillaConceptos() {
	var Oc_Compte = ocIdSelected;
	var Entrega_Fecha = $("#FechaEntrega").val();
	var Entrega_Adm = $("#listaSucEntrega").val()
	var Pago_Anticipado = 'N';
	if ($("#chkPagoAnticipado")[0].checked)
		Pago_Anticipado = 'S';
	var Pago_Fecha = $("#PagoPlazo").val();
	var Observaciones = $("#Obs").val();
	var Oce_Id = 'P';
	if ($("#chkDejarOCActiva")[0].checked)
		Oce_Id = 'C';
	var data = { Oc_Compte, Entrega_Fecha, Entrega_Adm, Pago_Anticipado, Pago_Fecha, Observaciones, Oce_Id };
	PostGenHtml(data, ObtenerConceptoURL, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			$("#divGridConcepto").html(obj);
			FormatearValores("#tbGridConcepto", 1);
		}
	});
}

function onChangeFechaEntrega(e) {
	var validDate = moment($("#FechaEntrega").val()).isValid();
	if (!validDate) {
		var fecha = moment().format('yyyy-MM-DD');
		$("#FechaEntrega").val(fecha)
		fecha = moment($("#FechaEntrega").val()).add(1, 'day').format('yyyy-MM-DD');
		$("#PagoPlazo").val(fecha);
	}
	if ($("#FechaEntrega").val() > $("#PagoPlazo").val()) {
		var fecha = moment($("#FechaEntrega").val()).add(1, 'day').format('yyyy-MM-DD');
		$("#PagoPlazo").val(fecha);
	}
	ActualizarGrillaConceptos();
}

function onChangePagoPlazo(e) {
	var validDate = moment($("#PagoPlazo").val()).isValid();
	if (!validDate) {
		fecha = moment($("#FechaEntrega").val()).add(1, 'day').format('yyyy-MM-DD');
		$("#PagoPlazo").val(fecha);
	}
	if ($("#FechaEntrega").val() > $("#PagoPlazo").val()) {
		var fecha = moment($("#FechaEntrega").val()).add(1, 'day').format('yyyy-MM-DD');
		$("#PagoPlazo").val(fecha);

	}
	ActualizarGrillaConceptos();
}

function onChangeListaSucEntrega(e) {
	ActualizarGrillaConceptos();
}

function AplicarSeteoMasivo() {
	var alMenosUno = false;
	var pIds = [];
	$('#tbListaProductoOC tbody tr').each(function () {
		const $checkbox = $(this).find('.check-producto');
		if ($checkbox.length && $checkbox.is(':checked')) {
			alMenosUno = true;
			const pId = $checkbox.data('p-id'); 
			pIds.push(pId);
		}
	});

	if (alMenosUno) {
		var dto1 = $("#Dto1").inputmask('unmaskedvalue');
		var dto2 = $("#Dto2").inputmask('unmaskedvalue');
		var dto3 = $("#Dto3").inputmask('unmaskedvalue');
		var dto4 = $("#Dto4").inputmask('unmaskedvalue');
		var dpa = $("#Dpa").inputmask('unmaskedvalue');
		var boolFlete = $("#chkFleteAPagar")[0].checked
		var flete = $("#Flete").inputmask('unmaskedvalue');
		var data = { pIds, dto1, dto2, dto3, dto4, dpa, boolFlete, flete };
		PostGenHtml(data, UpdateMasivoEnOcURL, function (obj) {
			if (obj.error === true) {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				$("#divListaProductoNuevaOC").html(obj);
				finalizarInicializacion();
				formatearTotalesEnTabDetalleOC();
			}
		});
	}
	else {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar al menos un producto para la edición masiva.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}

function formatearTotalesEnTabDetalleOC() {
	$("#Total_Costo").val(formatearValorConFormatoNumerico($("#Total_Costo").val(), 2));
	$("#Total_Pallet").val(formatearValorConFormatoNumerico($("#Total_Pallet").val(), 1));
	$("#Dto1").val("0");
	$("#Dto2").val("0");
	$("#Dto3").val("0");
	$("#Dto4").val("0");
	$("#Dpa").val("0");
	$("#Flete").val("0");
	getMaskForDiscountType("#Dto1");
	getMaskForDiscountType("#Dto2");
	getMaskForDiscountType("#Dto3");
	getMaskForDiscountType("#Dto4");
	getMaskForDiscountType("#Dpa");
	getMaskForDiscountType("#Flete");
}

function ActualizarProductoEnOc(row, campoActual) {
	if (campoActual == undefined) return false;
	else {
		var pId = row.data('p-id');
		var field = $(campoActual).data('field');
		var val = $(campoActual).val();
		var data = { pId, field, val };
		PostGen(data, ActualizarProductoEnOcURL, function (obj) {
			if (obj.error === true) {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				//Actualizar valores en la grilla
				$("#tbListaProductoOC").find('tr').each(function (i, el) {
					var td = $(this).find('td');
					if (td.length > 0 && td[1].innerText !== undefined && td[1].innerText === pId) {
						//GRILLA
						td[8].innerText = obj.data.pedidoCantidad.toFixed(3);//
						td[16].innerText = obj.data.pedido_Mas_Boni.toFixed(1);//PEDIDO +BONI -> obj.data.pedido_Mas_Boni
						td[17].innerText = formatearValorConFormatoNumerico(obj.data.p_Pcosto.toFixed(2),2);//PRECIO COSTO -> obj.data.p_Pcosto
						td[18].innerText = formatearValorConFormatoNumerico(obj.data.p_Pcosto_Total.toFixed(2),2);//TOTAL COSTO -> obj.data.p_Pcosto_Total
						td[19].innerText = obj.data.paletizado;//TOTAL PALLET -> obj.data.paletizado

						//TOTALES
						$("#Total_Costo").val(formatter.format(obj.data.total_Costo));//TOTAL_COSTO -> obj.data.total_Costo
					}
				});
			}

		});
	}
}

function formatearValorConFormatoNumerico(valor, decimales) {
	var retValue = "";
	// Validar si es string y convertirlo a número
	let numero = typeof valor === 'string'
		? parseFloat(valor.replace(/,/g, '').trim())
		: valor;

	// Validar que sea un número válido
	if (isNaN(numero)) return '';

	// Formatear con separador de miles y dos decimales
	retValue = numero.toLocaleString('en-US', {
		minimumFractionDigits: decimales,
		maximumFractionDigits: decimales
	});

	return retValue;
}

function CargarResumenDeOc() {
	if (ExitensItemsEnOC()) {
		var data = { ocIdSelected };
		PostGenHtml(data, CargarResumenDeOcURL, function (obj) {
			if (obj.error === true) {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				$("#divResumen").html(obj);
				FormatearValores("#tbGridConcepto", 1);
				$("#Obs").on('focusout', function (e) {
					ActualizarGrillaConceptos();
				});
				$("#chkPagoAnticipado").on("click", function () {
					ActualizarGrillaConceptos();
				});
				$("#chkDejarOCActiva").on("click", function () {
					ActualizarGrillaConceptos();
				});
				const dateControl2 = $('input[type="date"]');
				var now = moment().format('yyyy-MM-DD');
				var min = now;
				var max = moment().add(4, 'months');
				for (var i = 0; i < dateControl2.length; i++) {
					if (dateControl2[i].id == "FechaEntrega") {
						dateControl2[i].setAttribute('min', min);
						dateControl2[i].setAttribute('max', max.format('yyyy-MM-DD'));
					}
				}
			}
		});
	}
}

///Da formato monetario a los campos de tipo "money"
function FormatearValores(grilla, idx) {
	$(grilla).find('tr').each(function (i, el) {
		var td = $(this).find('td');
		if (td.length > 0 && td[idx].innerText !== undefined) {
			td[idx].innerText = formatter.format(td[idx].innerText);
		}
	});
}

function presentaPaginacionOC(div) {
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

/// Funcion que restaura el estado del producto en la grilla del primer Tab, luego de quitarlo de la lista de OC (segundo Tab)
function ActualizarInfoDeProductoEnGrilla(pId) {
	$("#tbListaProducto").find('tr').each(function (i, el) {
		var td = $(this).find('td');
		if (td.length > 0 && td[0].innerText !== undefined) {
			var p_Id = td[0].innerText;
			if (p_Id === pId) {
				var id = "a" + pId;
				$("#" + id).addClass('btn-success').removeClass('btn-danger');
				$("#" + id).prop('title', '');
			}
		}
	});
}

/// Funcion que evalúa si el producto de la grilla del primer Tab ya esta cargado en la grilla de OC (segundo Tab), si es así cambiar el estilo del icono.
function ActualizarInfoDeProductosEnGrilla() {
	if ($("#tbListaProductoOC").length != 0) {
		var idArrayOC = [];
		$("#tbListaProductoOC").find('tr').each(function (i, el) {
			var td = $(this).find('td');
			if (td.length > 0 && td[1].innerText !== undefined) {
				idArrayOC.push(td[1].innerText);
			}
		});

		if (idArrayOC.length > 0 && $("#tbListaProducto").length != 0) {
			$("#tbListaProducto").find('tr').each(function (i, el) {
				var td = $(this).find('td');
				if (td.length > 0 && td[0].innerText !== undefined) {
					var pId = td[0].innerText;
					if (idArrayOC.find(x => x === pId)) {
						var id = "a" + pId;
						$("#" + id).addClass('btn-danger').removeClass('btn-success');
						$("#" + id).prop('title', 'Producto existente en OC.');
					}
				}
			});
		}
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

function quitarProductoEnOC(e) {
	var pId = $(e).attr("data-interaction");
	var data = { pId };
	PostGenHtml(data, QuitarProductoEnOcURL, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			$("#divListaProductoNuevaOC").html(obj);
			finalizarInicializacion();
			formatearTotalesEnTabDetalleOC();
		}
	});
}

//Funcion que agrega el producto seleccionado en la grilla del primer, en la grilla de OC (Segundo Tab)
function actualizarProducto(e) {
	if ($(e).hasClass("btn-success")) {
		//event.stopPropagation(); // Evita que el clic se propague a la fila
		AbrirWaiting("Actualizando información de Orden de Compra.");
		var pId = $(e).attr("data-interaction");
		var data = { pId };
		PostGenHtml(data, AgregarProductoEnOcURL, function (obj) {
			if (obj.error === true) {
				CerrarWaiting();
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				$("#divListaProductoNuevaOC").html(obj);
				finalizarInicializacion();
				formatearTotalesEnTabDetalleOC();
				ActualizarInfoDeProductosEnGrilla();
				CerrarWaiting();
			}
		});
	}
	else if ($(e).hasClass("btn-secondary")) {
		AbrirMensaje("ATENCIÓN", "El producto seleccionado esta discontínuo.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else if ($(e).hasClass("btn-danger")) {
		AbrirMensaje("ATENCIÓN", "El producto seleccionado ya esta incluído en la OC.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		console.log("chan!");
	}
}

function InicializaPantalla() {
	var tb = $("#tbListaProducto tbody tr");
	if (tb.length === 0) {
		$("#divFiltro").collapse("show")
	}

	$("#lbRel01").text("Proveedor");
	$("#lbRel02").text("Rubro");
	$("#lbRel03").text("Familia");
	$("#lbRel04").text("OC Pendiente");
	$("#lbChkDescr").text("Descripción Producto");
	$("#lbDescr").html("Desc");

	$("#lbchk01").text("Alta Rotación");
	$("#lbchk02").text("Con PI");
	$("#lbchk03").text("Con OC");
	$("#lbchk04").text("Sin Stk");
	$("#lbchk05").text("Con Stk a Vencer");

	$("#lbChkDesdeHasta").text("ID Producto");

	//$("#IdSelected").val("");
	$(".activable").prop("disabled", true);
	$("#chkRel03").prop("disabled", true);
	$("#btnAbmAceptar").hide();
	$("#btnAbmCancelar").hide();
	$("#btnDetalle").prop("disabled", true);
	activarBotones(false);
	ocIdSelected = "";
	ctaIdSelected = "";
	MostrarDatosDeCuenta(false);
	$("#btnAbmAceptar").prop("disabled", true);
	CerrarWaiting();
	return true;
}

function ExitensItemsEnOC() {
	if ($("#tbListaProductoOC").length != 0) {
		return true;
	}
	else {
		return false;
	}
}

function activarBotones(activar) {
	if (activar === true && ExitensItemsEnOC()) {
		$("#btnAbmAceptar").show();
		$("#btnAbmCancelar").show();
	}
	else {
		$("#btnAbmAceptar").hide();
		$("#btnAbmCancelar").hide();
	}
}

function addTxtMesesKeyUpHandler() {
	$("#txtMeses").on('keyup', function (e) {
		if (e.keyCode == 13) {
			BuscarInfoAdicional();
		}
	});
}

function addTxtSemanasKeyUpHandler() {
	$("#txtSemanas").on('keyup', function (e) {
		if (e.keyCode == 13) {
			BuscarInfoAdicional();
		}
	});
}

function selectListaProductoRow(x, event) {
	if (event.target.closest('.no-propagar')) {
		return; // El clic fue en un botón, no seleccionar la fila
	}
	if (x) {
		pIdSeleccionado = x.cells[0].innerText.trim();
		/* ######	INICIO Componente de info adicional de producto ###### */
		// disparar evento custom con datos del producto
		$(document).trigger("productoSeleccionadoParaInfoAdicional", {
			p_id: pIdSeleccionado,
			ctaId: "",
			ctaDeno: ""
		});
		/* ######	FIN Componente de info adicional de producto ###### */
		//setTimeout(function () {
		//	BuscarInfoAdicional();
		//}, 500);
	}
	else {
		pIdSeleccionado = "";
	}
}

function NoHayProdSeleccionado() {
	if (pIdSeleccionado == undefined || pIdSeleccionado == "") {
		return true;
	}
	return false;
}

function BuscarInfoAdicional() {
	if (NoHayProdSeleccionado()) {
		AbrirMensaje("Atención", "Debe seleccionar un producto.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	AbrirWaiting();
	var admId = $("#listaSucursales").val();
	var meses = $("#txtMeses").val();
	var semanas = $("#txtSemanas").val();
	var pId = pIdSeleccionado;
	var datos = { pId, admId, meses };
	PostGenHtml(datos, BuscarInfoProdIExMesesURL, function (obj) {
		$("#divInfoProdIExMeses").html(obj);
		AddEventListenerToGrid("tbInfoProdIExMes");
		CerrarWaiting();
		return true
	});
	datos = { pId, admId, semanas };
	PostGenHtml(datos, BuscarInfoProdIExSemanasURL, function (obj) {
		$("#divInfoProdIExSemanas").html(obj);
		AddEventListenerToGrid("tbInfoProdIExSemana");
		CerrarWaiting();
		return true
	});
	datos = { pId, admId };
	PostGenHtml(datos, BuscarInfoProdStkDepositoURL, function (obj) {
		$("#divInfoProductoStkD").html(obj);
		AddEventListenerToGrid("tbInfoProdStkD");
		CerrarWaiting();
		return true
	});
	PostGenHtml(datos, BuscarInfoProdStkSucursalURL, function (obj) {
		$("#divInfoProductoStkA").html(obj);
		AddEventListenerToGrid("tbInfoProdStkA");
		CerrarWaiting();
		return true
	});
	var tipo = tipoDeOperacion;
	var soloProv = true; //Valor por default
	datos = { pId, tipo, soloProv }
	PostGenHtml(datos, BuscarInfoProdSustitutoURL, function (obj) {
		$("#divInfoProdSustituto").html(obj);
		AddEventListenerToGrid("tbListaProductoSust");
		CerrarWaiting();
		return true
	});
	datos = { pId }
	PostGenHtml(datos, BuscarInfoProdURL, function (obj) {
		$("#divInfoProducto").html(obj);
		AddEventListenerToGrid("tbInfoProducto");
		CerrarWaiting();
		return true
	});
}

function selectListaProductoRowOC(x) {
	if (x) {
		pIdEnOcSeleccionado = x.cells[1].innerText.trim();
	}
	else {
		pIdSeleccionado = "";
	}
}

function BuscarProductosTabOC() {

	if (ocIdSelected && ocIdSelected != "") {
		$("#btnTabNuevaOC").text(ocIdSelected);
	}
	else {
		$("#btnTabNuevaOC").text("Nueva OC");
	}
	var ocCompte = ocIdSelected;
	var ctaId = ctaIdSelected;
	data = { ctaId, ocCompte }
	PostGenHtml(data, BuscarProductosTabOCURL, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			$("#divListaProductoNuevaOC").html(obj);
			//$("#Total_Costo").val(formatter.format($("#Total_Costo").val()));
			finalizarInicializacion();
			formatearTotalesEnTabDetalleOC();
			//$("#Total_Pallet").val(formatter.format($("#Total_Pallet").val()));
			//AgregarHandlerAGrillaProdOC();
			ActualizarInfoDeProductosEnGrilla();
			activarBotones(true);
			CargarResumenDeOc();
			//setTimeout(function () {
			//	pingARegistro();
			//}, 100);
		}
	});
}

function getMaskForDiscountType(selector) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: '.',
		radixPoint: ',',
		digits: 1,
		digitsOptional: false,
		allowMinus: false,
		prefix: '',
		suffix: '',
		min: 0,
		max: 50,
		unmaskAsNumber: true
	});
}

function getMaskForMoneyType(selector) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: '.',
		radixPoint: ',',
		digits: 2,
		digitsOptional: false,
		allowMinus: false,
		prefix: '',
		suffix: '',
		rightAlign: true,
		unmaskAsNumber: true
	});
}

function BuscarProductos(pag = 1) {
	viendeDesdeBusquedaDeProducto = true;
	AbrirWaiting();
	var Tipo = tipoDeOperacion;
	var Buscar = $("#Buscar").val();
	var Id = $("#Id").val();
	var Id2 = $("#Id2").val();
	var Rel01 = [];
	var Rel02 = [];
	var Rel03 = [];
	Rel01.push(ctaIdSelected);
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

	var buscaNew = true;
	pagina = pag;
	Pagina = pag;
	var sort = null;
	var sortDir = null
	var data2 = { sort, sortDir, Pagina, buscaNew }
	var data1 = { Tipo, Buscar, Id, Id2, Rel01, Rel02, Rel03, Opt1, Opt2, Opt3, Opt4, Opt5 };
	var data = $.extend({}, data1, data2);
	PostGenHtml(data, BuscarProductosURL, function (obj) {
		$("#divListaProducto").html(obj);
		$("#divDetalle").collapse("show");
		AddEventListenerToGrid("tbListaProducto");
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

		BuscarProductosTabOC();

		$("#btnDetalle").prop("disabled", false);
		$("#btnAbmCancelar").prop("disabled", false);
		MostrarDatosDeCuenta(true);
		CargarTopesDeOC();
		CargarSucursalesParInfoAdicional();
		LimpiarDatosDelFiltroInicial();
		$("#btnCollapseSectionInfoProd").on("click", function (e) {
			e.preventDefault();

			if (pIdSeleccionado && pIdSeleccionado !== "") {
				// toggle manual
				$("#divInfoAdicionaDeProducto").collapse("toggle");

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
				});
			} else {
				AbrirMensaje("ATENCIÓN", "Debe seleccionar un producto.", function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
		});
		CerrarWaiting();
		viendeDesdeBusquedaDeProducto = false;
		return true
	});
}

function BuscarProductosDesdeNCPI(pag = 1, pId, ctaId, ctaDeno) {
	viendeDesdeBusquedaDeProducto = true;
	AbrirWaiting();
	var Tipo = tipoDeOperacion;
	var Buscar = ""
	var Id = pId;
	var Id2 = pId;
	var Rel01 = [];
	var Rel02 = [];
	var Rel03 = [];
	Rel01.push(ctaId);
	var Opt1 = $("#chk01")[0].checked
	var Opt2 = $("#chk02")[0].checked
	var Opt3 = $("#chk03")[0].checked
	var Opt4 = $("#chk04")[0].checked
	var Opt5 = $("#chk05")[0].checked

	var buscaNew = true;
	pagina = pag;
	Pagina = pag;
	var sort = null;
	var sortDir = null
	var data2 = { sort, sortDir, Pagina, buscaNew }
	var data1 = { Tipo, Buscar, Id, Id2, Rel01, Rel02, Rel03, Opt1, Opt2, Opt3, Opt4, Opt5 };
	var data = $.extend({}, data1, data2);
	PostGenHtml(data, BuscarProductosURL, function (obj) {
		$("#divListaProducto").html(obj);
		$("#divDetalle").collapse("show");
		AddEventListenerToGrid("tbListaProducto");
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
		ctaIdSelected = ctaId;
		ctaDescSelected = ctaDeno;
		BuscarProductosTabOC();

		$("#btnDetalle").prop("disabled", false);
		$("#btnAbmCancelar").prop("disabled", false);
		MostrarDatosDeCuenta(true);
		CargarTopesDeOC();
		CargarSucursalesParInfoAdicional();
		LimpiarDatosDelFiltroInicial();
		CerrarWaiting();
		viendeDesdeBusquedaDeProducto = false;
		return true
	});
}

function CargarSucursalesParInfoAdicional() {
	datos = {}
	PostGenHtml(datos, CargarSucursalesParInfoAdicionalURL, function (obj) {
		$("#divSucursales").html(obj);
		return true
	});
}

function CargarTopesDeOC() {
	data = {};
	PostGen(data, ObtenerTopesDeOcURL, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else if (obj.warn === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			//formatter.format(e.target.value);
			$("#Lim_Mensual").val(formatter.format(obj.data.oc_limite_semanal));
			$("#OC_Emitidas").val(formatter.format(obj.data.oc_emitidas));
			$("#Tope_Emision").val(formatter.format(obj.data.oc_tope));
		}
	});
}

function MostrarDatosDeCuenta(mostrar) {
	if (mostrar) {
		$("#CtaID").val(ctaIdSelected);
		$("#CtaDesc").val(ctaDescSelected);
		$("#divProveedorSeleccionado").collapse("show");
	}
	else {
		$("#CtaID").val("");
		$("#CtaDesc").val("");
		$("#divProveedorSeleccionado").collapse("hide");
	}
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
					return { label: texto, value: item.descripcion, id: item.id, prov: item.provId };
				}));
			}
		})
	},
	minLength: 3,
	select: function (event, ui) {
		ctaIdSelected = ui.item.id;
		ctaDescSelected = ui.item.value;
		$("#Rel01List").empty();
		$("#Rel01Item").val(ui.item.id);
		var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
		$("#Rel01List").append(opc);
		$("#chkRel03").prop("disabled", false);
		CargarFamiliaLista(ui.item.id);
		CargarOCLista(ui.item.id);

		return true;
	}
});

//codigo generico para autocomplete 03
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

//codigo generico para autocomplete 03
$("#Rel04").autocomplete({
	source: function (request, response) {

		data = { prefix: request.term }; Rel03

		$.ajax({
			url: autoComRel04Url,
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
		if ($("#Rel04List").has('option:contains("' + ui.item.id + '")').length === 0) {
			$("#Rel04Item").val(ui.item.id);
			var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
			$("#Rel04List").append(opc);
		}
		ocIdSelected = ui.item.id;
		return true;
	}
});

function CargarFamiliaLista(id) {
	var ctaId = id;
	data = { ctaId };
	PostGen(data, buscarFamiliaDesdeProveedorSeleccionadoUrl, function (obj) {
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

function CargarOCLista(id) {
	var ctaId = id;
	data = { ctaId };
	PostGen(data, buscarOCDesdeCtaIdSeleccionadoUrl, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			$("#divLstOcPendiente").html(obj);
			$("#lbRel04").text("OC Pendiente");
			addHandlerOnChkRel04_Click();
		}
	});
}

function btnCollapseSectionClicked() {
	if ($("#containerListaProducto").hasClass('table-wrapper-400-full-width')) {
		$("#containerListaProducto").removeClass('table-wrapper-400-full-width');
		$("#containerListaProducto").addClass('table-wrapper-300-full-width');
	} else {
		$("#containerListaProducto").removeClass('table-wrapper-300-full-width');
		$("#containerListaProducto").addClass('table-wrapper-400-full-width');
	}
}



/****************************************************************************************
################################ ADD-ON --  tbListaProductoOC  ##########################
*****************************************************************************************/

// Función de utilidad para destacar la fila seleccionada
// ✅ MEJORADA: Función destacar fila con verificación adicional
function destacarFilaSeleccionada(productoId) {
	console.log(`🎯 Destacando fila para producto ID: ${productoId}`);

	// Remover el destacado de todas las filas
	$("#tbListaProductoOC tbody tr").removeClass("selected");

	// Verificar que existe una fila con ese ID
	const $fila = $("#tbListaProductoOC tbody tr[data-p-id='" + productoId + "']");

	if ($fila.length === 0) {
		console.warn(`⚠️ No se encontró ninguna fila con data-p-id="${productoId}"`);
		return false;
	}

	// Añadir el destacado solo a la fila del producto seleccionado
	$fila.addClass("selected");
	console.log(`✅ Fila destacada correctamente para producto ${productoId}`);

	// Hacer scroll a la fila si está fuera de vista
	scrollAFilaSeleccionada($fila);

	return true;
}

// ✅ NUEVA: Función separada para scroll optimizado
function scrollAFilaSeleccionada($fila) {
	const $tableContainer = $("#tbListaProductoOC").closest('.table-responsive');

	if ($tableContainer.length > 0) {
		const containerTop = $tableContainer.offset().top;
		const containerHeight = $tableContainer.height();
		const rowTop = $fila.offset().top;

		// Solo hacer scroll si la fila está fuera del área visible
		if (rowTop < containerTop || rowTop > containerTop + containerHeight) {
			$tableContainer.animate({
				scrollTop: $tableContainer.scrollTop() + (rowTop - containerTop - containerHeight / 2)
			}, 300);
			console.log(`📜 Realizando scroll a la fila seleccionada`);
		}
	}
}

function finalizarInicializacion() {
	setTimeout(function () {
		configuracionInputMaskOptimizada();
		optimizarVisualizacionTabla();
	}, 10);
}

function optimizarVisualizacionTabla() {
	// Asegurarnos de que la tabla existe
	if ($("#tbListaProductoOC").length === 0) {
		return;
	}

	// Ajustar columnas con texto para que no sean demasiado anchas
	$("#tbListaProductoOC th:nth-child(2)").css('max-width', '180px'); // Descripción
	$("#tbListaProductoOC td:nth-child(2)").css({
		'max-width': '180px',
		'white-space': 'nowrap',
		'overflow': 'hidden',
		'text-overflow': 'ellipsis'
	});

	// Asegurarnos que la tabla tenga scroll horizontal si es necesario
	$("#tbListaProductoOC").closest('.table-responsive').css('overflow-x', 'auto');

	console.log("Tabla optimizada para mejor visualización");
}

function configuracionInputMaskOptimizada() {
	console.log("Aplicando configuración InputMask optimizada...");

	// Establecer todos los campos como readonly de una sola vez
	$('.input-bultos, .input-p_plista, .input-p_dto1, .input-p_dto2, .input-p_dto3, .input-p_dto4, .input-p_dto_pa, .input-p_boni')
		.prop('readonly', true)
		.addClass('campo-readonly');

	// Definir configuraciones de máscara fuera de los bucles
	const maskConfig3Decimales = {
		alias: "numeric",
		groupSeparator: ",",
		radixPoint: ".",
		autoGroup: true,
		digits: 3,
		digitsOptional: false,
		rightAlign: true,
		prefix: '',
		placeholder: "0",
		clearMaskOnLostFocus: false,
		showMaskOnHover: false,
		showMaskOnFocus: false,
		min: 0, // Explícitamente permitir 0 como valor mínimo
		allowMinus: false, // No permitir valores negativos
		onBeforeMask: function (value) {
			// Si es null, undefined o cadena vacía, retornar '0'
			if (value === null || value === undefined || value === '') {
				return '0';
			}

			// Para otros valores, formatear correctamente
			try {
				let numValue = parseFloat(value.toString().replace(/,/g, ''));
				return isNaN(numValue) ? '0' : numValue.toFixed(3);
			} catch (e) {
				console.error('Error al formatear valor:', e);
				return '0';
			}
		}
	};

	const maskConfig1Decimal = {
		alias: "numeric",
		groupSeparator: ",",
		radixPoint: ".",
		autoGroup: true,
		digits: 1,
		digitsOptional: false,
		rightAlign: true,
		integerDigits: 2,
		min: 0,
		max: 99.9,
		prefix: '',
		placeholder: "0",
		clearMaskOnLostFocus: false,
		showMaskOnHover: false,
		showMaskOnFocus: false,
		onBeforeMask: function (value) {
			if (value) {
				let numValue = parseFloat(value.toString().replace(/,/g, ''));
				if (numValue > 99.9) numValue = 99.9;
				return isNaN(numValue) ? value : numValue.toFixed(1);
			}
			return value;
		}
	};

	const maskConfigEntero = {
		alias: "numeric",
		groupSeparator: ",",
		radixPoint: ".", // no se usa en enteros, pero puede quedar por consistencia
		autoGroup: true,
		digits: 0,
		digitsOptional: false,
		rightAlign: true,
		integerDigits: 2,
		min: 0,
		max: 99999,
		prefix: '',
		placeholder: "0",
		clearMaskOnLostFocus: false,
		showMaskOnHover: false,
		showMaskOnFocus: false,
		onBeforeMask: function (value) {
			if (value) {
				let numValue = parseInt(value.toString().replace(/,/g, ''));
				if (numValue > 99999) numValue = 99999;
				return isNaN(numValue) ? value : numValue.toString();
			}
			return value;
		}
	};


	const maskConfig2Decimales = {
		alias: "numeric",
		groupSeparator: ",",
		radixPoint: ".",
		autoGroup: true,
		digits: 2,
		digitsOptional: false,
		rightAlign: true,
		prefix: '',
		placeholder: "0",
		clearMaskOnLostFocus: false,
		showMaskOnHover: false,
		showMaskOnFocus: false,
		onBeforeMask: function (value) {
			if (value) {
				let numValue = parseFloat(value.toString().replace(/,/g, ''));
				return isNaN(numValue) ? value : numValue.toFixed(2);
			}
			return value;
		}
	};

	const maskConfigBoni = {
		mask: "999/999",
		placeholder: "",
		showMaskOnHover: false,
		showMaskOnFocus: false
	};

	// Aplicar máscaras de forma eficiente con selección optimizada
	Inputmask(maskConfig3Decimales).mask('.input-p_plista');
	Inputmask(maskConfig1Decimal).mask('.input-p_dto1, .input-p_dto2, .input-p_dto3, .input-p_dto4, .input-p_dto_pa');
	Inputmask(maskConfigBoni).mask('.input-p_boni');
	Inputmask(maskConfigEntero).mask('.input-bultos');

	// Configurar eventos de edición
	configurarEventosEdicionOptimizado();

	console.log("Configuración InputMask aplicada");
}

function configurarEventosEdicionOptimizado() {
	const camposEditables = '.input-bultos, .input-p_plista, .input-p_dto1, .input-p_dto2, .input-p_dto3, .input-p_dto4, .input-p_dto_pa, .input-p_boni';
	const camposSecuencia01 = '.input-bultos, .input-p_plista, .input-p_dto1, .input-p_dto2, .input-p_dto3, .input-p_dto4, .input-p_dto_pa, .input-p_boni';

	// Limpiar eventos previos
	$(document).off('click.camposEditables keydown.camposEditables blur.camposSecuencia01');

	// Evento click unificado
	$(document).on('click.camposEditables', camposEditables, function (e) {
		e.stopPropagation();

		const $this = $(this);
		const pIdDetalle = $this.closest('tr').data('p-id');

		// Cambio de producto si es necesario
		if (pIdDetalle !== productoActualEnLista) {
			productoActualEnLista = pIdDetalle;
			//$("#divProdLista").attr('data-producto-actual', pIdDetalle);
			destacarFilaSeleccionada(pIdDetalle);
			//buscarProductoListaOptimizado(pIdDetalle);
		}

		// Habilitar campo
		$this.prop('readonly', false).removeClass('campo-readonly');
		setTimeout(() => { $this[0].focus(); $this[0].select(); }, 0);
	});

	// Evento keydown unificado
	$(document).on('keydown.camposEditables', camposEditables, function (e) {
		if (e.key === 'Enter' || e.key === 'Tab') {
			e.preventDefault();

			const row = $(this).closest('tr');
			const esSecuencia01 = $(this).is(camposSecuencia01);
			//const esMargen = $(this).hasClass('input-tp_margen');
			//const esPrecioVenta = $(this).hasClass('input-tp_pvta');

			var fueModificado = marcarCampoModificado(this);
			//actualizarEstadoCarga(row);
			activarSiguienteCampo(this);
			
			// Aplicar cálculos según tipo
			if (esSecuencia01 && fueModificado) ActualizarProductoEnOcDebounced(row, this);
			//else if (esMargen) calcularPrecioVentaAPIDebounced(row);
			//else if (esPrecioVenta) calcularPrecioVentaMargenAPIDebounced(row);
		}
	});

	// Eventos blur simplificados con delegación
	const eventosBlur = {
		[camposSecuencia01]: () => ActualizarProductoEnOcDebounced
	};

	Object.entries(eventosBlur).forEach(([selector, getCallback]) => {
		$(document).on(`blur.${selector.replace(/[^a-zA-Z]/g, '')}`, selector, function () {
			if ($(this).prop('readonly')) return;

			const row = $(this).closest('tr');
			const value = $(this).val().replace(/,/g, '');
			const numValue = parseFloat(value);

			if (!isNaN(numValue)) {
				const decimals = $(this).hasClass('input-tp_plista') || $(this).hasClass('input-tp_pcosto') || $(this).hasClass('input-tp_pneto') ? 3 :
					$(this).hasClass('input-tp_dto1') || $(this).hasClass('input-tp_dto2') || $(this).hasClass('input-tp_dto3') || $(this).hasClass('input-tp_dto4') || $(this).hasClass('input-tp_dto_pa') || $(this).hasClass('input-tp_porc_flete') ? 1 : 2;
				$(this).val(numValue.toFixed(decimals));
			}

			$(this).prop('readonly', true).addClass('campo-readonly');
			getCallback()(row);
		});
	});
}

// Función de debounce para evitar llamadas repetidas
function debounce(func, wait) {
	let timeout;
	return function () {
		const context = this, args = arguments;
		clearTimeout(timeout);
		timeout = setTimeout(function () {
			func.apply(context, args);
		}, wait);
	};
}

// Aplicar debounce a funciones de cálculo intensivas
const ActualizarProductoEnOcDebounced = debounce(function (row, campoActual) {
	ActualizarProductoEnOc(row, campoActual);
}, 300);

function marcarCampoModificado(input) {
	// Usar el parámetro input en lugar de this
	const $input = $(input);

	// Validar que el input existe
	if (!$input.length) {
		console.warn('marcarCampoModificado: Input no válido', input);
		return false;
	}

	const valorOriginal = $input.data('original-value');

	// Obtener valor actual con manejo de errores
	let valorActual = '';
	try {
		valorActual = $input.val() ? $input.val().replace(/,/g, '') : '';
	} catch (e) {
		console.error('Error al obtener valor del campo:', e);
		return false;
	}

	// Si no hay valor original definido, no podemos comparar
	if (valorOriginal === undefined) {
		return false;
	}

	// Determinar si el campo está modificado
	let esModificado = false;

	// Para el campo de bonificación (caso especial)
	if ($input.hasClass('input-p_boni')) {
		const originalTrim = (valorOriginal || '').toString().trim();
		const actualTrim = (valorActual || '').toString().trim();

		// Casos especiales: "0" y "" se consideran iguales
		if ((originalTrim === "0" && actualTrim === "") ||
			(originalTrim === "" && actualTrim === "0")) {
			esModificado = false;
		} else {
			esModificado = originalTrim !== actualTrim;
		}
	} else {
		// Para campos numéricos - manejar correctamente el caso del valor 0
		try {
			// Convertir valores a números, manejando cadenas vacías como 0
			let numOriginal = valorOriginal === '' || valorOriginal === null ? 0 : parseFloat(valorOriginal);
			let numActual = valorActual === '' ? 0 : parseFloat(valorActual);

			// Si ambos valores son realmente cero (o equivalentes a cero), no están modificados
			if ((numOriginal === 0 || isNaN(numOriginal)) &&
				(numActual === 0 || isNaN(numActual))) {
				esModificado = false;
			} else if (!isNaN(numOriginal) && !isNaN(numActual)) {
				// Ambos son números válidos, usar tolerancias específicas según el campo
				let tolerancia = 0.009; // Base para campos con 2 decimales

				if ($input.hasClass('input-p_dto1') ||
					$input.hasClass('input-p_dto2') ||
					$input.hasClass('input-p_dto3') ||
					$input.hasClass('input-p_dto4') ||
					$input.hasClass('input-p_dto_pa')) {
					tolerancia = 0.09; // Para campos con 1 decimal
				} else if ($input.hasClass('input-p_plista')) {
					tolerancia = 0.0009; // Para campos con 3 decimales
				}

				// Si la diferencia supera la tolerancia, está modificado
				esModificado = Math.abs(numOriginal - numActual) > tolerancia;
			} else if (isNaN(numOriginal) !== isNaN(numActual)) {
				// Si uno es NaN y el otro no, están diferentes
				esModificado = true;
			}
		} catch (e) {
			console.error("Error al comparar valores:", e);
			esModificado = false; // En caso de error, no marcar como modificado
		}
	}

	// Aplicar o quitar la clase según corresponda
	if (esModificado) {
		$input.addClass('campo-modificado');
	} else {
		$input.removeClass('campo-modificado');
	}

	// Manejar el indicador visual
	const container = $input.closest('.input-container');
	if (esModificado) {
		if (container.find('.indicador-cambio').length === 0) {
			container.append('<div class="indicador-cambio"></div>');
		}
	} else {
		container.find('.indicador-cambio').remove();
	}

	return esModificado;
}

/**
* Actualiza el atributo data-carga de una fila según las reglas:
* - Si hay cambios y carga=0, establecer carga=1
* - Si no hay cambios y carga=1, establecer carga=0
* - En otros casos, mantener valor actual
* @param {jQuery} row - La fila (tr) a verificar
* @returns {boolean} - Indica si la fila tiene algún campo modificado
*/
function actualizarEstadoCarga(row) {
	// Obtener el estado actual de carga
	const estadoCargaActual = row.data('carga') === 1;

	// Verificación rápida: si ya hay campos con la clase 'campo-modificado', hay cambios
	const camposModificados = row.find('.campo-modificado').length;

	if (camposModificados > 0) {
		// Hay campos modificados, asegurar que carga=1
		if (!estadoCargaActual) {
			row.data('carga', 1);
			row.attr('data-carga', '1');
			console.log(`Fila ${row.data('p-id')}: Cambiando data-carga a 1 (detectados ${camposModificados} campos modificados)`);
		}
		return true; // Hay campos modificados
	} else {
		// No hay campos con la clase, verificar si realmente hay diferencias
		// (esta es una verificación más profunda y costosa)
		let hayAlgunCampoModificado = false;

		row.find('input[data-original-value]').each(function () {
			const $input = $(this);
			const valorOriginal = $input.data('original-value');
			const valorActual = $input.val().replace(/,/g, '');

			// Verificar si está modificado según el tipo de campo
			if ($input.hasClass('input-tp_boni')) {
				// Lógica para bonificación
				const originalTrim = (valorOriginal || '').toString().trim();
				const actualTrim = (valorActual || '').toString().trim();

				if (!((originalTrim === actualTrim) ||
					(originalTrim === "0" && actualTrim === "") ||
					(originalTrim === "" && actualTrim === "0"))) {
					hayAlgunCampoModificado = true;
					return false; // Salir del bucle
				}
			} else {
				// Lógica para campos numéricos (simplificada para rendimiento)
				try {
					const numOriginal = parseFloat(valorOriginal);
					const numActual = parseFloat(valorActual);

					if (!isNaN(numOriginal) && !isNaN(numActual) &&
						Math.abs(numOriginal - numActual) > 0.0001) {
						hayAlgunCampoModificado = true;
						return false; // Salir del bucle
					}
				} catch (e) { }
			}
		});

		// Actualizar según resultado
		if (hayAlgunCampoModificado && !estadoCargaActual) {
			row.data('carga', 1);
			row.attr('data-carga', '1');
			console.log(`Fila ${row.data('p-id')}: Cambiando data-carga a 1 (hay campos modificados no marcados)`);
		} else if (!hayAlgunCampoModificado && estadoCargaActual) {
			row.data('carga', 0);
			row.attr('data-carga', '0');
			console.log(`Fila ${row.data('p-id')}: Cambiando data-carga a 0 (no hay campos modificados)`);
		}

		return hayAlgunCampoModificado;
	}
}

function activarSiguienteCampo(campoActual) {
	const $campoActual = $(campoActual);
	const $fila = $campoActual.closest('tr');
	const camposEditables = '.input-bultos, .input-p_plista, .input-p_dto1, .input-p_dto2, .input-p_dto3, .input-p_dto4, .input-p_dto_pa, .input-p_boni';
	const $camposEnFila = $fila.find(camposEditables);
	const indiceActual = $camposEnFila.index($campoActual);

	let $siguienteCampo = null;
	if (indiceActual < $camposEnFila.length - 1) {
		$siguienteCampo = $camposEnFila.eq(indiceActual + 1);
	} else if ($fila.next('tr').length) {
		$siguienteCampo = $fila.next('tr').find(camposEditables).first();
	}

	$campoActual.prop('readonly', true).addClass('campo-readonly');

	if ($siguienteCampo && $siguienteCampo.length) {
		$siguienteCampo.prop('readonly', false).removeClass('campo-readonly');
		setTimeout(() => { $siguienteCampo[0].focus(); $siguienteCampo[0].select(); }, 0);
	}
}