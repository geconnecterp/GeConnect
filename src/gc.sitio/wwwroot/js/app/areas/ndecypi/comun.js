
class Origen {
	static PedidoInterno = 'PI';
	static NecesidadesDeCompra = 'NC';
}

$(function () {
	//$("#btnCollapseSection").on("click", btnCollapseSectionClicked);
	/*$(document).on("click", "#btnCollapseSectionInfoProd", btnCollapseSectionValidar);*/

	/* ######	INICIO Componente de info adicional de producto ###### */
	$("#btnCollapseSectionInfoProd").on("click", function (e) {
		e.preventDefault();

		if (pIdSeleccionado && pIdSeleccionado !== "") {
			// toggle manual
			//$("#divInfoAdicionaDeProducto").collapse("toggle");

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
				pasarAdmLogueo,
			});
		} else {
			AbrirMensaje("ATENCIÓN", "Debe seleccionar un producto.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	});

	$("#btnCancel").on("click", function () {
		AbrirWaiting();
		LimpiarDatosDelFiltroInicial();
		InicializarDatosEnSesion();
		CerrarWaiting();
		setTimeout(() => {
			HandlerActualizarTablaPostOCAuto();
			$('#divDetalle').collapse('hide');
		}, 200);
	});

	// Cuando se muestra el filtro → ocultar detalle y opciones
	$('#divFiltro').on('shown.bs.collapse', function (e) {
		if (e.target.id === 'divFiltro') { // aseguramos que sea el filtro
			$('#divDetalle').collapse('hide');
			$('#divBtnOpciones').collapse('hide');
		}
	});

	// Cuando se oculta el filtro → mostrar detalle
	$('#divFiltro').on('hidden.bs.collapse', function (e) {
		if (e.target.id === 'divFiltro') {
			const $tabla = $('#tbListaProducto');
			if ($tabla.length > 0) { // existe en el DOM
				const filas = $tabla.find('tbody tr').length;

				if (filas > 0) {
					$('#divDetalle').collapse('show');
					console.log("divDetalle > show");
				} else {
					$('#divDetalle').collapse('hide');
					console.log("divDetalle > hide");
				}
			} else {
				console.warn("La tabla #tbListaProducto no existe en el DOM");
			}
		}
	});

	$('#divFiltro').on('shown.bs.collapse', function () {
		console.log("divFiltro > show");
		$('#divBtnOpciones').hide();
		console.log("divBtnOpciones > hide");
	});

	// Controlar directamente el estado de divDetalle
	$('#divDetalle').on('shown.bs.collapse', function (e) {
		if (e.target.id === 'divDetalle') {
			const $tabla = $('#tbListaProducto');
			if ($tabla.length > 0) { // existe en el DOM
				const filas = $tabla.find('tbody tr').length;
				if (filas > 0) {
					$('#divBtnOpciones').show();
					console.log("divDetalle > show");
				} else {
					$('#divBtnOpciones').hide();
					console.log("divDetalle > hide");
				}
			}
			else {
				$('#divBtnOpciones').hide();
				console.log("divBtnOpciones > hide");
			}
		}
	});

	$('#divDetalle').on('hidden.bs.collapse', function (e) {
		if (e.target.id === 'divDetalle') {
			$('#divBtnOpciones').hide();
			console.log("divBtnOpciones > hide");
		}
	});

	$("input#Rel03").on("click", function () {
		$("input#Rel03").val("");
		$("#Rel03Item").val("");
	});
	AddEventListenerToGrid("tbListaProducto");

	$("#btnBuscar").on("click", function () {
		dataBak = "";
		pagina = 1;
		BuscarProductos(pagina);
	});
	$(document).on("change", "#listaSucursales", ControlaSucursalSeleccionada);
	$("#btnOCAuto").on("click", function () {
		AbrirlModalAuto(Origen.NecesidadesDeCompra);
	});
	$("#btnPasarAOC").on("click", function () {
		AbrirOrdenDeCompra();
	});
	$(document).on("change", "#listaSucursalesModal", ControlalistaSucursalesModalSelected);
	$(document).on("change", "#listaDepositosModal", ControlalistaDepositosModalSelected);
	$(document).on("click", "#btnCompraAutoBuscar", ControlaCompraAutoBuscar);

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

	CargarRubros();
	InicializaPantallaNC();
	funcCallBack = BuscarProductos;
	return true;
});

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

function btnCollapseSectionValidar() {
	if (pIdSeleccionado != "") {
		var p_id = pIdSeleccionado;
		var data = {
			p_id,
			mostrarInfoProd,
			mostrarInfoProdStkA,
			mostrarInfoProdStkD,
			mostrarInfoProdStkBox,
			mostrarInfoProdStkMovM,
			mostrarInfoProdStkMovS,
			mostrarInfoProdStkMovD,
			mostrarInfoProdSustituto,
			pasarAdmLogueo
		};
		invocarComponenteDeInfoAdicionalDeProd(data);
	}
	else {
		$("#divInfoAdicionaDeProducto").html("").collapse("hide");
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un producto.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}

/* ######	FIN Componente de info adicional de producto ###### */

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

function AbrirOrdenDeCompra() {
	if (pIdSeleccionado && ctaIdDeProdSeleccionado && pIdSeleccionado != undefined && ctaIdDeProdSeleccionado != undefined && pIdSeleccionado != "" && ctaIdDeProdSeleccionado != "") {
		AbrirMensaje("ATENCIÓN", "Va a ser redirigido a la aplicación Carga de Orden de Compra. ¿Confirma?", function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI": //Confirmar
					(function submitPostNavigation() {
						var form = document.createElement('form');
						form.method = 'POST';
						form.action = '/Compras/OrdenDeCompra/IndexConParametros';
						form.style.display = 'none';

						var addField = function (name, value) {
							var input = document.createElement('input');
							input.type = 'hidden';
							input.name = name;
							input.value = value !== undefined && value !== null ? value : '';
							form.appendChild(input);
						};

						addField('pId', pIdSeleccionado);
						addField('ctaId', ctaIdDeProdSeleccionado);
						addField('ctaDeno', ctaDenoProdSeleccionado);

						// Intentar agregar token antiforgery si está disponible en la página
						var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
						if (tokenInput && tokenInput.value) {
							addField('__RequestVerificationToken', tokenInput.value);
						} else {
							var tokenMeta = document.querySelector('meta[name="__RequestVerificationToken"]');
							if (tokenMeta && tokenMeta.content) {
								addField('__RequestVerificationToken', tokenMeta.content);
							} else {
								// Intento adicional: leer cookies comunes de XSRF (si la app las usa)
								function readCookie(name) {
									var match = document.cookie.split('; ').find(function (c) { return c.indexOf(name + '=') === 0; });
									return match ? decodeURIComponent(match.split('=')[1]) : null;
								}
								var cookieNames = ['XSRF-TOKEN', 'RequestVerificationToken', 'X-XSRF-TOKEN'];
								for (var i = 0; i < cookieNames.length; i++) {
									var cval = readCookie(cookieNames[i]);
									if (cval) {
										addField('__RequestVerificationToken', cval);
										break;
									}
								}
							}
						}

						document.body.appendChild(form);
						form.submit();
					})();
					break;
				case "NO":
					break;
				default: //NO
					break;
			}
			return true;

		}, true, ["Aceptar", "Cancelar"], "question!", null);

	}
	else {
		AbrirMensaje("Atención", "Debe seleccionar un producto.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
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

function ControlaCompraAutoBuscar() {
	var resultado = ValidarDatosObligAnalizarCompraAuto(); //ValidarDatosObligatoriosAntesDeAnalizarCompraAuto
	if (resultado.msj != "") {
		AbrirMensaje("Atención", resultado.msj, function () {
			$("#msjModal").modal("hide");
			$(resultado.objeto).trigger("focus");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Actualizando datos...");
		let tipo = "OC";
		let dias_prevision = $("#DiasAprov").inputmask('unmaskedvalue');
		let vta_ana_desde = $("#VentaDiariaDesde").val();
		let vta_ana_hasta = $("#VentaDiariaHasta").val();
		let limite_max = $("#LimitarPedidoACompletar")[0].checked;
		let limite_min = $("#LimitarPedidoParaCumplir")[0].checked;
		let ultimo_ped = $("#TomarUltimoPedido")[0].checked;
		if ($("#ExcluirOCPendientes").length > 0) {
			var excluir_pend = $("#ExcluirOCPendientes")[0].checked;
			var es_pedido_interno = true;
			console.log("El checkbox ExcluirOCPendientes está renderizado");
		} else {
			var es_pedido_interno = false;
			var excluir_pend = null;
			console.log("El checkbox ExcluirOCPendientes NO está en el DOM");
		}
		let adm_list = [];
		$("#SucursalesListModal").children().each(function (i, item) { adm_list.push($(item).val()) });
		let depo_list = [];
		$("#DepositosListModal").children().each(function (i, item) { depo_list.push($(item).val()) });
		var data = { tipo, adm_list, dias_prevision, vta_ana_desde, vta_ana_hasta, limite_max, limite_min, ultimo_ped, depo_list, excluir_pend, es_pedido_interno };
		PostGen(data, confirmarCambiosPedidoAutoUrl, function (obj) {
			CerrarWaiting();
			if (obj.error === true) {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				HandlerActualizarTablaPostOCAuto();
			}
		});
	}
}

function HandlerActualizarTablaPostOCAuto() {
	AbrirWaiting("Actualizando vista de la tabla...")
	var datos = {};
	PostGenHtml(datos, recargarGrillaUrl, function (obj) {
		$('#modalFiltroCompraAuto').modal('hide');
		$("#divListaProducto").html(obj);
		finalizarInicializacionGridListaProductos();
		const tabla = document.getElementById("tbListaProducto");
		AplicarEstilosTabla(tabla);
		CerrarWaiting();
		return true
	});
}

function AplicarEstilosTabla(tabla) {
	if (!tabla || !tabla.rows) {
		console.warn("Tabla no válida");
		return;
	}

	const $tbody = $("#tbListaProducto tbody");
	const $filas = $tbody.find("tr").not(".table-secondary"); // excluye agrupadoras

	if ($filas.length === 1) {
		const textoFila = $filas.text().trim();
		if (textoFila.includes("No se encontraron productos con los criterios especificados")) {
			console.log("⚠️ La tabla está vacía y muestra el mensaje de 'No se encontraron productos'.");
			return;
		}
	}

	$filas.each(function () {
		const fila = this;
		const valorCantidad = parseFloat(fila.cells[colCantidad].innerText) || 0;
		const pedidoTipo = fila.getAttribute("data-pedido-tipo") || "";

		let color = "";
		if (pedidoTipo === "M") {
			color = "lightgreen";
		} else if (pedidoTipo === "A") {
			color = "#6cc6f3";
		}

		if (valorCantidad !== 0) {
			fila.cells[colBulto].style.backgroundColor = color;
			fila.cells[colCantidad].style.backgroundColor = color;
			fila.cells[colCosto].style.backgroundColor = color;
			fila.cells[colCostoTotal].style.backgroundColor = color;
		} else {
			fila.cells[colBulto].style.backgroundColor = "";
			fila.cells[colCantidad].style.backgroundColor = "";
			fila.cells[colCosto].style.backgroundColor = "";
			fila.cells[colCostoTotal].style.backgroundColor = "";
		}
	});

}

function ValidarDatosObligAnalizarCompraAuto() {
	var ret = { msj: "", objeto: "" };
	let registrosEnDepositos = $("#DepositosListModal option").length;
	let registrosEnSucursal = $("#SucursalesListModal option").length;
	if (registrosEnSucursal > 0 && registrosEnDepositos <= 0) {
		ret.msj = "Debe seleccionar al menos un Depósito.";
		ret.objeto = "#listaDepositosModal";
	}
	//if ($("#DepositosListModal option").length <= 0) {
	//	ret.msj = "Debe seleccionar al menos un Depósito.";
	//	ret.objeto = "#listaDepositosModal";
	//}
	//else if ($("#SucursalesListModal option").length <= 0) {
	//	ret.msj = "Debe seleccionar al menos una Sucursal.";
	//	ret.objeto = "#listaSucursalesModal";
	//}
	return ret;
}

function AbrirlModalAuto(abrirComo) {
	var data = { abrirComo };
	PostGenHtml(data, abrirModalAutoUrl, function (obj) {
		$("#divFiltroCompraAuto").empty();
		$("#divFiltroCompraAuto").html(obj);
		const $modal = $("#modalFiltroCompraAuto");

		$modal.modal({
			backdrop: 'static',
		});

		inicializarCamposEnModal();

		CerrarWaiting();
		$modal.modal('show');

		setTimeout(() => {
			const $item = $("#listaSucursalesModal");
			if ($item.length > 0) {
				$item.trigger("focus");
				console.log("Foco aplicado a #listaSucursalesModal");
			} else {
				console.warn("No se encontró el input #listaSucursalesModal");
			}
		}, 500);

		return true
	});
}

function inicializarCamposEnModal() {
	$("#lbSucursales").text("Sucursales");
	$("#chkSucursales").prop('checked', true);
	$("#chkSucursales").trigger("change");
	$("#chkSucursales").prop("disabled", true);
	$("#listaSucursalesModal").prop("disabled", false);
	$("#SucursalesListModal").prop("disabled", false);

	$("#lbDepositos").text("Depósitos");
	$("#chkDepositos").prop('checked', true);
	$("#chkDepositos").trigger("change");
	$("#chkDepositos").prop("disabled", true);
	$("#listaDepositosModal").prop("disabled", false);
	$("#DepositosListModal").prop("disabled", false);

	getMaskForIntegerMax1000("#DiasAprov");
	$("#SucursalesListModal").on("dblclick", 'option', function () {
		$(this).remove();
		//Si hay depositos, limpio la lista de depositos
		var tieneItems = $("#DepositosListModal option").length > 0;
		if (tieneItems) {
			$("#DepositosListModal").empty();
		}
	})
	$("#DepositosListModal").on("dblclick", 'option', function () {
		$(this).remove();
	})

	const chkTomarUltimoPedido = document.getElementById("TomarUltimoPedido");
	const chkLimitarCompletar = document.getElementById("LimitarPedidoACompletar");
	const chkLimitarCumplir = document.getElementById("LimitarPedidoParaCumplir");

	if (chkTomarUltimoPedido) {
		chkTomarUltimoPedido.addEventListener("change", function () {
			if (this.checked) {
				chkLimitarCompletar.checked = false;
				chkLimitarCumplir.checked = false;
			}
		});
	}
}

function ControlalistaSucursalesModalSelected() {
	var item = $("#listaSucursalesModal").val();
	var desc = $("#listaSucursalesModal option:selected").text();
	if ($("#SucursalesListModal").has('option:contains("' + item + '")').length === 0 && $("#SucursalesListModal").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#SucursalesListModal").append(opc);
	}
}

function ControlalistaDepositosModalSelected() {
	var tieneItems = $("#SucursalesListModal option").length > 0;
	if (!tieneItems) {
		var item = $("#listaDepositosModal").val();
		var desc = $("#listaDepositosModal option:selected").text();
		if ($("#DepositosListModal").has('option:contains("' + item + '")').length === 0 && $("#DepositosListModal").has('option:contains("' + desc + '")').length === 0) {
			var opc = "<option value=" + item + ">" + desc + "</option>"
			$("#DepositosListModal").append(opc);
		}
	}
	else {
		AbrirWaiting("Validando Deósito seleccionado...");
		let sucuId = $("#SucursalesListModal option").map(function () {
			return this.value;
		}).get();
		var depoId = $("#listaDepositosModal").val();
		var data = { depoId, sucuId };
		PostGen(data, validarPertenenciaDeDepositoEnSucursalUrl, function (obj) {
			CerrarWaiting();
			if (obj.error === true) {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				var item = $("#listaDepositosModal").val();
				var desc = $("#listaDepositosModal option:selected").text();
				if ($("#DepositosListModal").has('option:contains("' + item + '")').length === 0 && $("#DepositosListModal").has('option:contains("' + desc + '")').length === 0) {
					var opc = "<option value=" + item + ">" + desc + "</option>"
					$("#DepositosListModal").append(opc);
				}
			}
		});
	}
}

function getMaskForIntegerMax1000(selector) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: '.',       // separador de miles
		digits: 0,                 // sin decimales
		digitsOptional: false,
		allowMinus: false,
		prefix: '',
		suffix: '',
		rightAlign: true,
		unmaskAsNumber: true,
		min: 0,
		max: 1000
	});
}

function ControlaSucursalSeleccionada() {
	BuscarInfoAdicional();
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

function limpiarListaLs03() {
	const $select = $("#listaLs03");
	if ($select.length) {
		$select.empty(); // vacía todo
		$select.append($("<option>", { value: "", text: "Seleccionar" }));
		$select.prop("selectedIndex", 0); // deja seleccionado "Seleccionar"
	}
}

function NoHayProdSeleccionado() {
	if (pIdSeleccionado == undefined || pIdSeleccionado == "") {
		return true;
	}
	return false;
}

function changeProductosDelMismoProveedor(x) {
	if (NoHayProdSeleccionado()) {
		AbrirMensaje("Atención", "Debe seleccionar un producto.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	AbrirWaiting();
	var pId = pIdSeleccionado;
	var tipo = tipoDeOperacion;
	var soloProv = $("#chkProductosDelMismoProveedor")[0].checked;
	datos = { pId, tipo, soloProv }
	PostGenHtml(datos, BuscarInfoProdSustitutoURL, function (obj) {
		$("#divInfoProdSustituto").html(obj);
		AddEventListenerToGrid("tbListaProductoSust");
		CerrarWaiting();
		return true
	});
}

function BuscarInfoAdicional() {
	const el = document.getElementById("divInfo");

	if (!el || el.style.display === "none") {
		return;
	}
	else {
		/* ######	INICIO Componente de info adicional de producto ###### */
		//BuscarInfoAdicional();
		// disparar evento custom con datos del producto
		$(document).trigger("productoSeleccionadoParaInfoAdicional", {
			p_id: pIdSeleccionado,
			ctaId: "",
			ctaDeno: ""
		});
		/* ######	FIN Componente de info adicional de producto ###### */
	}
}

function InicializaPantallaNC() {
	var tb = $("#tbListaProducto tbody tr");
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

	$("#lbChkDesdeHasta").text("ID Producto");

	$(".activable").prop("disabled", true);
	$("#chkRel03").prop("disabled", true);
	$("#listaLs02").prop("disabled", true);

	CerrarWaiting();
	return true;
}

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
					return { label: texto, value: item.descripcion, id: item.id, prov: item.provId };
				}));
			}
		})
	},
	minLength: 3,
	select: function (event, ui) {
		if ($("#Rel01List").has('option:contains("' + ui.item.id + '")').length === 0) {
			$("#Rel01Item").val(ui.item.id);
			var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
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

		return true;
	}
});

function CargarFamiliaLista(id) {
	var ctaId = id;
	data = { ctaId };
	PostGenHtml(data, BuscarProveedoresFamiliaURL, function (obj) {
		$("#divLs03").html(obj);
		CerrarWaiting();
		return true
	});
}

var tipoBusqueda = "";
var viendeDesdeBusquedaDeProducto = false;
const FuncionSobreBusquedaDeProductos = {
	PROVEEDORES: 'PROVEEDORES',
	PROVEEDORESYFAMILIA: 'PROVEEDORESYFAMILIA',
	RUBROS: 'RUBROS',
	SINSTOCK: 'SINSTOCK',
	CONSTOCKAVENCER: 'CONSTOCKAVENCER',
	ALTAROTACION: 'ALTAROTACION',
	CONPI: 'CONPI',
	CONOC: 'CONOC'
}

const colBulto = 11;
const colCantidad = 12;
const colCosto = 13;
const colCostoTotal = 14;
const colPallet = 15;
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

	var buscaNew = true;
	pagina = pag;
	Pagina = pag;
	var sort = null;
	var sortDir = null
	var data2 = { sort, sortDir, Pagina, buscaNew }
	var data1 = { Tipo, Buscar, Id, Id2, Rel01, Rel02, Rel03, Opt1, Opt2, Opt3, Opt4, Opt5 };
	var data = $.extend({}, data1, data2);
	PostGenHtml(data, BuscarProductosOCPI2URL, function (obj) {
		$("#divListaProducto").html(obj);
		$("#divDetalle").collapse("show");
		finalizarInicializacionGridListaProductos();
		$("#divBtnOpciones").show();
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
		viendeDesdeBusquedaDeProducto = false;
		return true
	});
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

function selectListaProductoRow(x) {
	$("#tbListaProducto tbody tr").each(function (index) {
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

function SeleccionarDesdeFila(index, ctrol) {
	if (index === "")
		return;
	if (ctrol === undefined)
		return;
	for (var i = 0; i < ctrol.options.length; i++) {
		if (ctrol.options[i].value == index) {
			ctrol.options[i].selected = true;
			return;
		}
	}
}

function verificaEstado(e) {
	FunctionCallback = null; //inicializo funcion por si tiene alguna funcionalidad asignada.
	var res = $("#estadoFuncion").val();
	CerrarWaiting();
	if (res === "true") {
		var prod = productoBase;
		if (prod) { //Producto existe
		}
	}
	return true;
}

function SelecccionarSucursal() {
	$("#listaSucursales").val()
	if ($("#listaSucursales").val() == "") {
		$("#listaSucursales").prop("selectedIndex", 1).change();
	}
}

function btnCollapseSectionClicked() {
	if ($("#containerListaProducto").hasClass('table-wrapper-full-width')) {
		$("#containerListaProducto").removeClass('table-wrapper-full-width');
		$("#containerListaProducto").addClass('table-wrapper-300-full-width');
	} else {
		$("#containerListaProducto").removeClass('table-wrapper-300-full-width');
		$("#containerListaProducto").addClass('table-wrapper-full-width');
	}
}

function selectListaInfoProdIExMesRow(x) {
}

function selectListaInfoProdIExSemanaRow(x) {
}

function selectListaProductoSustitutoRow(x) {
}

function selectListaInfoProductoRow() {
}


/****************************************************************************************
################################ ADD-ON --  tbListaProducto  #########################
*****************************************************************************************/
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
const ActualizarListaProductosDebounced = debounce(function (row, campoActual) {
	if (campoActual != undefined) {
		ActualizarListaProductos(row, campoActual);
	}
}, 300);

function finalizarInicializacionGridListaProductos() {
	setTimeout(function () {
		configuracionInputMaskOptimizadaGridListaProductos();
		optimizarVisualizacionTablaGridListaProductos();
	}, 10);
}

function ActualizarListaProductos(row, campoActual) {
	AbrirWaiting();
	var tipo = tipoDeOperacion;
	var pId = row.data('id');
	var tipoCarga = "M";
	var bultos = $(campoActual).val();
	var datos = { tipo, pId, tipoCarga, bultos }
	PostGen(datos, CargaPedidoOCPIURL, function (o) {
		if (o.error === true) {
			CerrarWaiting();
			AbrirMensaje("Atención", o.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		} else if (o.warn === true) {
			CerrarWaiting();
			AbrirMensaje("Atención", o.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "warn!", null);
		} else if (o.msg !== "") {
			CerrarWaiting();
			AbrirMensaje("Atención", o.msg, function (e) {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "info!", null);
		} else {
			CerrarWaiting();
			tabla = myTable = document.querySelector('#tbListaProducto tbody');
			let fila = tabla.querySelector(`tr[data-id='${pId}']`);

			if (fila) {
				// Actualizar
				fila.setAttribute("data-pedido-tipo", "M"); // ahora la fila tiene data-pedido-tipo="M"

				const pedidoTipo = fila.getAttribute("data-pedido-tipo"); // "A" o "M"

				// Buscar el input dentro de la fila
				const inputBulto = fila.querySelector("input.input-bulto");
				// Si existe, actualizar su valor original
				if (inputBulto) {
					// Actualizar DOM
					inputBulto.setAttribute("data-original-value", bultos);

					// Actualizar caché jQuery
					$(inputBulto).data("original-value", bultos);

					// Sincronizar visualmente el input (opcional pero recomendable)
					inputBulto.value = bultos;
				}

				var color = "";
				if (o.cantidad != 0) {
					if (pedidoTipo === "M") {
						color = "lightgreen"; // Manual
					} else if (pedidoTipo === "A") {
						color = "#6cc6f3"; // celeste pastel (Automático)
					}

					fila.cells[colCantidad].innerText = o.cantidad;
					fila.cells[colCantidad].style.backgroundColor = color;

					fila.cells[colCosto].innerText = (Math.round(o.pCosto * 100) / 100).toFixed(3);
					fila.cells[colCosto].style.backgroundColor = color;

					fila.cells[colCostoTotal].innerText = (Math.round(o.pCostoTotal * 100) / 100).toFixed(3);
					fila.cells[colCostoTotal].style.backgroundColor = color;

					fila.cells[colPallet].innerText = (Math.round(o.pallet * 100) / 100).toFixed(2);

					fila.cells[colBulto].style.backgroundColor = color;
				} else {
					fila.cells[colCantidad].innerText = o.cantidad;
					fila.cells[colCantidad].style.backgroundColor = "";

					fila.cells[colCosto].innerText = (Math.round(o.pCosto * 100) / 100).toFixed(3);
					fila.cells[colCosto].style.backgroundColor = "";

					fila.cells[colCostoTotal].innerText = (Math.round(o.pCostoTotal * 100) / 100).toFixed(3);
					fila.cells[colCostoTotal].style.backgroundColor = "";

					fila.cells[colPallet].innerText = (Math.round(o.pallet * 100) / 100).toFixed(2);

					fila.cells[colBulto].style.backgroundColor = "";
				}
			}
			return false;
		}
	});
}

function destacarFilaSeleccionadaGridListaProductos(id) {
	console.log(`🎯 Destacando fila para ID: ${id}`);

	// Remover el destacado de todas las filas
	$("#tbListaProducto tbody tr").removeClass("selected");

	// Verificar que existe una fila con ese ID
	const $fila = $("#tbListaProducto tbody tr[data-id='" + id + "']");

	if ($fila.length === 0) {
		console.warn(`⚠️ No se encontró ninguna fila con data-id="${id}"`);
		return false;
	}

	// Añadir el destacado solo a la fila del producto seleccionado
	$fila.addClass("selected");
	console.log(`✅ Fila destacada correctamente para producto ${id}`);

	// Hacer scroll a la fila si está fuera de vista
	scrollAFilaSeleccionadaGridListaProductos($fila);

	return true;
}

function scrollAFilaSeleccionadaGridListaProductos($fila) {
	const $tableContainer = $("#tbListaProducto").closest('.table-responsive');

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

function configuracionInputMaskOptimizadaGridListaProductos() {
	console.log("Aplicando configuración InputMask optimizada...");

	// Establecer todos los campos como readonly de una sola vez
	$('.input-bulto').prop('readonly', true).addClass('campo-readonly-ncpi');

	const maskConfigInt = {
		alias: "numeric",
		groupSeparator: ",",
		autoGroup: true,
		digits: 0,              // 🔹 sin decimales
		digitsOptional: false,  // 🔹 no permite decimales
		rightAlign: true,
		prefix: '',
		placeholder: "0",
		clearMaskOnLostFocus: false,
		showMaskOnHover: false,
		showMaskOnFocus: false,
		onBeforeMask: function (value) {
			if (value) {
				let numValue = parseInt(value.toString().replace(/,/g, ''), 10);
				return isNaN(numValue) ? value : numValue.toString();
			}
			return value;
		}
	};

	// Aplicar máscaras de forma eficiente con selección optimizada
	Inputmask(maskConfigInt).mask('.input-bulto');

	// Configurar eventos de edición
	configurarEventosEdicionOptimizadoGridListaProductos();

	console.log("Configuración InputMask aplicada");
}

let listProdActualEnLista = null;

function configurarEventosEdicionOptimizadoGridListaProductos() {
	let campoEditando = null;
	let campoEditandoPrevio = null;

	const camposEditables = '.input-bulto';
	const camposSecuencia01 = '.input-bulto';

	// Limpiar eventos previos
	$(document).off('click.camposEditables keydown.camposEditables blur.camposSecuencia01');

	$(document).on("mousedown.camposEditables", ".input-bulto", function (e) {
		campoEditandoPrevio = campoEditando; // guardamos el que se estaba editando
		campoEditando = this; // ahora sí, actualizamos

		const $input = $(this);
		if ($input.prop("readonly")) {
			e.preventDefault(); // evita que el primer click sea "inútil"
			$input.prop("readonly", false).removeClass("campo-readonly-ncpi");
			setTimeout(() => {
				$input.focus();
				$input.select();
			}, 0);
		}
	});

	// Evento click unificado
	$(document).on('click.camposEditables', camposEditables, function (e) {
		campoEditando = this;
		e.stopPropagation();

		const $this = $(this);
		const id = $this.closest('tr').data('id');

		// Cambio de producto si es necesario
		if (id !== listProdActualEnLista) {
			listProdActualEnLista = id;
			destacarFilaSeleccionadaGridListaProductos(id);
		}

		// Habilitar campo
		$this.prop('readonly', false).removeClass('campo-readonly-ncpi');
		setTimeout(() => { $this[0].focus(); $this[0].select(); }, 0);
	});

	$(document).on("mousedown.cambioCelda", function (e) {

		// Si no hay campo en edición, no hacemos nada
		if (!campoEditandoPrevio) return;

		// Si el clic fue dentro del mismo campo que se estaba editando → no hacemos nada
		if (e.target === campoEditandoPrevio) return;

		const $campo = $(campoEditandoPrevio);

		if ($campo.prop("readonly")) {
			campoEditandoPrevio = null;
			return;
		}

		const fueModificado = marcarCampoModificadoGridListaProductos(campoEditandoPrevio);

		if (fueModificado) {
			const row = $campo.closest("tr");
			ActualizarListaProductosDebounced(row, campoEditandoPrevio);
		}

		$campo.prop("readonly", true).addClass("campo-readonly-ncpi");

		campoEditandoPrevio = null;

	});

	// Evento keydown unificado
	$(document).on('keydown.camposEditables', camposEditables, function (e) {
		const $this = $(this);
		const row = $this.closest('tr');

		if (e.key === 'Enter') {
			e.preventDefault();

			const row = $(this).closest('tr');
			const esSecuencia01 = $(this).is(camposSecuencia01);

			var fueModificado = marcarCampoModificadoGridListaProductos(this);
			activarSiguienteCampoGridListaProductos(this);

			// Aplicar cálculos según tipo
			if (esSecuencia01 && fueModificado) ActualizarListaProductosDebounced(row, this);
		}

		if (e.key === 'Tab') {
			e.preventDefault();

			const esSecuencia01 = $this.is(camposSecuencia01);
			var fueModificado = marcarCampoModificadoGridListaProductos(this);

			if (e.shiftKey) {
				// 🔹 Navegar hacia el campo anterior
				activarCampoAnteriorGridListaProductos(this);
			} else {
				// 🔹 Navegar hacia el siguiente campo
				activarSiguienteCampoGridListaProductos(this);
			}

			if (esSecuencia01 && fueModificado) ActualizarListaProductosDebounced(row, this);
		}

		// 🔹 Nueva lógica para navegación con flechas
		if (e.key === 'ArrowUp' || e.key === 'ArrowDown') {
			e.preventDefault();

			const esSecuencia01 = $(this).is(camposSecuencia01);
			var fueModificado = marcarCampoModificadoGridListaProductos(this);
			// Aplicar cálculos según tipo
			if (esSecuencia01 && fueModificado) ActualizarListaProductosDebounced(row, this);

			const $filaActual = $this.closest('tr');
			let $filaDestino;

			if (e.key === 'ArrowUp') {
				$filaDestino = $filaActual.prev('tr');
			} else if (e.key === 'ArrowDown') {
				$filaDestino = $filaActual.next('tr');
			}

			if ($filaDestino && $filaDestino.length) {
				const $campoDestino = $filaDestino.find(camposEditables).first();

				// cerrar el campo actual
				$this.prop('readonly', true).addClass('campo-readonly-ncpi');

				// abrir el campo destino
				$campoDestino.prop('readonly', false).removeClass('campo-readonly-ncpi');
				setTimeout(() => {
					$campoDestino[0].focus();
					$campoDestino[0].select();
				}, 0);

				// destacar la fila destino
				const idDestino = $filaDestino.data('id');
				if (idDestino) {
					listProdActualEnLista = idDestino;
					destacarFilaSeleccionadaGridListaProductos(idDestino);
				}
			}
		}
	});

	$(document).on('focusin.camposEditables', camposEditables, function (e) {
		const $this = $(this);
		const id = $this.closest('tr').data('id');
		pIdSeleccionado = id;

		if (!$this.prop('readonly')) {
			setTimeout(() => {
				this.select();
			}, 0);
		}

		// Quitar readonly en el primer focus
		$this.prop("readonly", false).removeClass("campo-readonly-ncpi");

		// Llamar a tu función personalizada
		BuscarInfoAdicional();

		console.log(`ℹ️ Disparado BuscarInfoAdicional para producto ${id}`);
	});

	// Eventos blur simplificados con delegación
	const eventosBlur = {
		[camposSecuencia01]: () => ActualizarListaProductosDebounced
	};

	Object.entries(eventosBlur).forEach(([selector, getCallback]) => {
		$(document).on(`blur.${selector.replace(/[^a-zA-Z]/g, '')}`, selector, function () {
			if ($(this).prop('readonly')) return;

			const row = $(this).closest('tr');
			const value = $(this).val().replace(/,/g, '');
			const numValue = parseFloat(value);

			if (!isNaN(numValue)) {
				const decimals = 2;
				$(this).val(numValue.toFixed(decimals));
			}

			$(this).prop('readonly', true).addClass('campo-readonly-ncpi');
			getCallback()(row);
		});
	});
}

function activarCampoAnteriorGridListaProductos(campoActual) {
	const $campoActual = $(campoActual);
	const $fila = $campoActual.closest('tr');
	const camposEditables = '.input-bulto';
	const $camposEnFila = $fila.find(camposEditables);
	const indiceActual = $camposEnFila.index($campoActual);

	let $campoDestino = null;
	if (indiceActual > 0) {
		$campoDestino = $camposEnFila.eq(indiceActual - 1);
	} else if ($fila.prev('tr').length) {
		$campoDestino = $fila.prev('tr').find(camposEditables).last();
	}

	$campoActual.prop('readonly', true).addClass('campo-readonly-ncpi');

	if ($campoDestino && $campoDestino.length) {
		$campoDestino.prop('readonly', false).removeClass('campo-readonly-ncpi');
		setTimeout(() => { $campoDestino[0].focus(); $campoDestino[0].select(); }, 0);
	}
}

function activarSiguienteCampoGridListaProductos(campoActual) {
	const $campoActual = $(campoActual);
	const $fila = $campoActual.closest('tr');
	const camposEditables = '.input-bulto';
	const $camposEnFila = $fila.find(camposEditables);
	const indiceActual = $camposEnFila.index($campoActual);

	let $siguienteCampo = null;
	if (indiceActual < $camposEnFila.length - 1) {
		$siguienteCampo = $camposEnFila.eq(indiceActual + 1);
	} else if ($fila.next('tr').length) {
		$siguienteCampo = $fila.next('tr').find(camposEditables).first();
	}

	$campoActual.prop('readonly', true).addClass('campo-readonly-ncpi');

	if ($siguienteCampo && $siguienteCampo.length) {
		$siguienteCampo.prop('readonly', false).removeClass('campo-readonly-ncpi');
		setTimeout(() => { $siguienteCampo[0].focus(); $siguienteCampo[0].select(); }, 0);
	}
}

function marcarCampoModificadoGridListaProductos(input) {
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

			//if ($input.hasClass('input-importe')) {
			//	tolerancia = 0.0009; // Para campos con 3 decimales
			//}

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

function optimizarVisualizacionTablaGridListaProductos() {
	// Asegurarnos de que la tabla existe
	if ($("#tbListaProducto").length === 0) {
		return;
	}

	// Ajustar columnas con texto para que no sean demasiado anchas
	$("#tbListaProducto th:nth-child(0)").css('max-width', '180px'); // Descripción
	$("#tbListaProducto td:nth-child(0)").css({
		'max-width': '180px',
		'white-space': 'nowrap',
		'overflow': 'hidden',
		'text-overflow': 'ellipsis'
	});

	// Asegurarnos que la tabla tenga scroll horizontal si es necesario
	$("#tbListaProducto").closest('.table-responsive').css('overflow-x', 'auto');

	console.log("Tabla optimizada para mejor visualización");
}
/****************************************************************************************
################################ FIN ADD-ON --  tbListaProducto  #####################
*****************************************************************************************/