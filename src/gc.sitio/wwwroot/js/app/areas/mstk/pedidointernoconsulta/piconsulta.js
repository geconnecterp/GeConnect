let _pedidoLoading = false;
var orCompteSeleccionado = null;
var oreCompteSeleccionado = null;

$(function () {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		cargaPaginacion();
	});

	InicializarCamposEnFiltros(false);

	$("#FechaDesde, #FechaHasta").on("blur", ValidarFechasClick);
	$(document).on("change", "#listaSucursales", ControlalistaSucursalesSelected);
	$(document).on("change", "#listaEstados", ControlalistaEstadosSelected);

	$("#SucursalesList").on("dblclick", 'option', function () { $(this).remove(); })
	$("#EstadosList").on("dblclick", 'option', function () { $(this).remove(); })

	$("#btnBuscar").on("click", function () {
		if (validarFechas()) {
			dataBak = "";
			pagina = 1;
			BuscarPedidosInternos(pagina);
		} else {
			AbrirMensaje("ATENCIÓN", "Problemas con las fechas, por favor verifique.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	});

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

	funcCallBack = BuscarPedidosInternos;
});

function ControlalistaSucursalesSelected() {
	var item = $("#listaSucursales").val();
	var desc = $("#listaSucursales option:selected").text();
	if ($("#SucursalesList").has('option:contains("' + item + '")').length === 0 && $("#SucursalesList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#SucursalesList").append(opc);
	}
}

function ControlalistaEstadosSelected() {
	var item = $("#listaEstados").val();
	var desc = $("#listaEstados option:selected").text();
	if ($("#EstadosList").has('option:contains("' + item + '")').length === 0 && $("#EstadosList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#EstadosList").append(opc);
	}
}

function BuscarPedidosInternos(pag = 1) {
	AbrirWaiting("Inicializando búsqueda...")
	try {
		const filtros = buildQueryFilters(pag);
		const url = buscarPedidosInternosUrl;
		const urlInitView = inicializarViewUrl;

		PostGenHtml({}, urlInitView, function (html) {
			$("#divDetalle").html(html).collapse("show");
			$("#divFiltros").collapse("hide");
			CerrarWaiting();
			CargarPedidosInternos(filtros, url);
		});


	} catch (e) {
		console.error("Error al buscar pedidos internos:", e);
		$("#divDetalle").html('<div class="alert alert-danger py-2 mb-0">No se pudo obtener la información.</div>').collapse("show");
	} finally {
		_pedidoLoading = false;
	}
}

function CargarPedidosInternos(filtros, url) {
	AbrirWaiting("Cargando pedidos internos...");
	PostGenHtml(filtros, url, function (html) {
		CerrarWaiting();
		$("#divPedidosInternos").html(html);

		configurarEventosSeleccionListaPI();

		PostGen({}, buscarMetadataURL, function (obj) {
			if (obj.error === true) {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			} else {
				totalRegs = obj.metadata.totalCount;
				pags = obj.metadata.totalPages;
				pagRegs = obj.metadata.pageSize;
				$("#pagEstado").val(true).trigger("change");
			}
		});
	});

}



function configurarEventosSeleccionListaPI() {
	$(document).off("click", "#tbGridPedidosInternos tbody tr");
	$(document).on("click", "#tbGridPedidosInternos tbody tr", function (e) {
		if (!$(e.target).is("button, a, .btn, i")) {
			var $this = $(this);
			var fueSeleccionado = $this.hasClass("selected-row");

			$("#tbGridPedidosInternos tbody tr").removeClass("selected-row");

			if (!fueSeleccionado) {
				$this.addClass("selected-row");
				let piCompte = $this.data("pi-compte");
				let pieId = $this.data("pie-id");
				piCompteSeleccionado = piCompte;
				pieCompteSeleccionado = pieId;
				if (piCompte) {
					//Poder hacer algo, como por ejemplo, habilitar o no botones dependiendo del estado de la OR
					ActualizarEstadosDeBotonesEnPI();
				}
			}
		}
	});
	$(document).off("click", "#btnCerrarPI");
	$(document).on("click", "#btnCerrarPI", function () {
		if (piCompteSeleccionado != "") {
			AbrirMensaje(
				'CONFIRMAR CERRAR',
				"¿Desea confirmar el cierre del pedido interno seleccionado?",
				function (resp) {
					if (resp === 'SI') {
						ConfirmarCierreDePedidoInternoSeleccionado();
					}
					$('#msjModal').modal('hide');
				},
				true,
				['Confirmar', 'Cancelar'],
				'info!',
				null
			);
		}
		else {
			AbrirMensaje("ATENCIÓN", "Debe seleccionar un pedido interno para cerrar.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	});
	$(document).off("click", "#btnAnularPI");
	$(document).on("click", "#btnAnularPI", function () {
		if (piCompteSeleccionado != "") {
			AbrirMensaje(
				'CONFIRMAR ANULAR',
				"¿Desea confirmar la anulación del pedido interno seleccionado?",
				function (resp) {
					if (resp === 'SI') {
						ConfirmarAnulacionDePedidoInternoSeleccionado();
					}
					$('#msjModal').modal('hide');
				},
				true,
				['Confirmar', 'Cancelar'],
				'info!',
				null
			);
		}
		else {
			AbrirMensaje("ATENCIÓN", "Debe seleccionar un pedido interno para anular.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	});
	$(document).off("click", "#btnImprimirLista");
	$(document).on("click", "#btnImprimirLista", function () {

	});
	$(document).off("click", "#btnImprimirPI");
	$(document).on("click", "#btnImprimirPI", function () {
		if (piCompteSeleccionado != "") {
			var tipoReporte = 1;
			var data = { tipoReporte };
			PostGen(data, setearTipoDeReporteUrl, function (obj) {
				CerrarWaiting();
				if (obj.error === true) {
					CerrarWaiting();
					AbrirMensaje("ATENCIÓN", obj.msg, function () {
						$("#msjModal").modal("hide");
						return true;
					}, false, ["Aceptar"], "error!", null);
				}
				else {
					CerrarWaiting();
					ImprimirPedidoInterno(piCompteSeleccionado)
				}
			});
		}
		else {
			AbrirMensaje("ATENCIÓN", "Debe seleccionar un pedido interno para imprimir.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
	});
}

function ImprimirPedidoInterno(id) {
	ReseteoDeReportes();
	setTimeout(() => {
		let data = { id: id };
		cargarReporteEnArre(65, data, "PEDIDO INTERNO", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ActualizarEstadosDeBotonesEnPI() {
	const estadosPermitidosCerrar = ["R"];
	const btnCerrarPI = document.getElementById("btnCerrarPI");
	if (btnCerrarPI) {
		if (estadosPermitidosCerrar.includes(pieCompteSeleccionado)) {
			btnCerrarPI.disabled = false;
			btnCerrarPI.classList.remove("disabled");
		} else {
			btnCerrarPI.disabled = true;
			btnCerrarPI.classList.add("disabled");
		}
	}

	const estadosPermitidosAnular = ["P"];
	const btnAnularPI = document.getElementById("btnAnularPI");
	if (btnAnularPI) {
		if (estadosPermitidosAnular.includes(pieCompteSeleccionado)) {
			btnAnularPI.disabled = false;
			btnAnularPI.classList.remove("disabled");
		} else {
			btnAnularPI.disabled = true;
			btnAnularPI.classList.add("disabled");
		}
	}
}

function ConfirmarCierreDePedidoInternoSeleccionado() {
	var data = {
		PiCompte: piCompteSeleccionado,
		Cierra: true,
		Anula: false
	}
	AbrirWaiting("Cerrando pedido interno...");
	PostGen(data, cambiarEstadoPedidoInternoUrl, function (response) {
		CerrarWaiting();
		if (response.error === true || response.warn === true) {
			console.error('❌ Response:', response.mensaje);
			AbrirMensaje("ATENCIÓN", 'Error al intentar cerrar el PI.: ' + (response.mensaje || 'Error desconocido'), function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			setTimeout(() => {
				AbrirMensaje(
					'CONFIRMACIÓN EXITOSA',
					'Se ha cerrado el pedido interno',
					function () {
						$('#msjModal').modal('hide');

						//Actualizar tabla de Ordenes de Reparto
						const filtros = buildQueryFilters(pagina);
						const url = buscarPedidosInternosUrl;
						CargarPedidosInternos(filtros, url);
					},
					false,
					['Aceptar'],
					'success!',
					null
				);
			}, 200);
		}
	});
}

function ConfirmarAnulacionDePedidoInternoSeleccionado() {
	var data = {
		PiCompte: piCompteSeleccionado,
		Cierra: false,
		Anula: true
	}
	AbrirWaiting("Anulando pedido interno...");
	PostGen(data, cambiarEstadoPedidoInternoUrl, function (response) {
		CerrarWaiting();
		if (response.error === true || response.warn === true) {
			console.error('❌ Response:', response.mensaje);
			AbrirMensaje("ATENCIÓN", 'Error al intentar anular el PI.: ' + (response.mensaje || 'Error desconocido'), function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			setTimeout(() => {
				AbrirMensaje(
					'CONFIRMACIÓN EXITOSA',
					'Se ha anulado el pedido interno',
					function () {
						$('#msjModal').modal('hide');

						//Actualizar tabla de Ordenes de Reparto
						const filtros = buildQueryFilters(pagina);
						const url = buscarPedidosInternosUrl;
						CargarPedidosInternos(filtros, url);
					},
					false,
					['Aceptar'],
					'success!',
					null
				);
			}, 200);
		}
	});
}

function buildQueryFilters(pag) {
	const usaPeriodo = $("#chkDesdeHasta").is(":checked");
	const fechaD = usaPeriodo ? $("#FechaDesde").val() : null;
	const fechaH = usaPeriodo ? $("#FechaHasta").val() : null;

	var rel01 = [];
	$("#SucursalesList").children().each(function (i, item) { rel01.push($(item).val()) });

	var rel02 = [];
	$("#EstadosList").children().each(function (i, item) { rel02.push($(item).val()) });

	return {
		Registros: 200,
		Pagina: pag,
		FechaD: fechaD || null,
		FechaH: fechaH || null,
		Rel01: rel01.length ? rel01 : null,
		Rel02: rel02.length ? rel02 : null,
	};
}

function cargaPaginacion() {
	$("#divPaginacion").pagination({
		items: totalRegs,
		itemsOnPage: pagRegs,
		cssStyle: "dark-theme",
		currentPage: pagina,
		onPageClick: function (num) {
			BuscarPedidosInternos(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltro").collapse("hide")
	return true;
}

function InicializarCamposEnFiltros(vieneDeCancelar) {
	if (!vieneDeCancelar) {
	}

	$("#chkDesdeHasta").prop('checked', true);
	$("#chkDesdeHasta").trigger("change");
	$("#chkDesdeHasta").prop("disabled", true);

	$("#btnImprimir").hide();
	$("#lbChkDesdeHasta").text("Fechas");
	$("#lbSucursales").text("Sucursal que genera");
	$("#lbEstados").text("Estado");

	$("#chkSucursales").prop('checked', false);
	$("#chkSucursales").trigger("change");
	$("#chkEstados").prop('checked', false);
	$("#chkEstados").trigger("change");

	$("#listaSucursales").val("");
	$("#listaEstados").val("");

	$("#divFiltros").collapse("show");
	$("#divDetalle").collapse("hide");

	$("#SucursalesList").empty();
	$("#EstadosList").empty();

	$("#listaSucursales").val("");
	$("#listaEstados").val("");

	if (!vieneDeCancelar) {
		HandlerCheckBox();
	}
}

function HandlerCheckBox() {
	$("#chkSucursales").on("click", function () {
		if ($("#chkSucursales").is(":checked")) {
			$("#listaSucursales").prop("disabled", false);
			$("#SucursalesList").prop("disabled", false);
			$("#listaSucursales").trigger("focus");
		}
		else {
			$("#listaSucursales").prop("disabled", true);
			$("#SucursalesList").prop("disabled", true);
			$("#listaSucursales").val("");
			$("#SucursalesList").empty();
		}
	});
	$("#chkEstados").on("click", function () {
		if ($("#chkEstados").is(":checked")) {
			$("#listaEstados").prop("disabled", false);
			$("#EstadosList").prop("disabled", false);
			$("#listaEstados").trigger("focus");
		}
		else {
			$("#listaEstados").prop("disabled", true);
			$("#EstadosList").prop("disabled", true);
			$("#listaEstados").val("");
			$("#EstadosList").empty();
		}
	});
}

function validarFechas() {
	let desde = $("#FechaDesde").val();
	let hasta = $("#FechaHasta").val();

	if (!desde || !hasta) return false;

	let fechaDesde = new Date(desde);
	let fechaHasta = new Date(hasta);

	const diffMs = hasta - desde;
	const diffDias = diffMs / (1000 * 60 * 60 * 24);

	if (diffDias > 370) {
		return false;
	}

	return !(fechaDesde > fechaHasta);
}

function ValidarFechasClick() {
	const desdeStr = $("#FechaDesde").val();
	const hastaStr = $("#FechaHasta").val();

	if (!desdeStr || !hastaStr)
		return;

	const desde = new Date(desdeStr);
	const hasta = new Date(hastaStr);

	if (desde > hasta) {
		AbrirMensaje("ATENCIÓN", "El valor de Fecha Desde no puede ser mayor a Fecha Hasta, revise.", function () {
			$("#msjModal").modal("hide");
			$("#FechaDesde").val($("#FechaHasta").val());
			return true;
		}, false, ["Aceptar"], "error!", null);
		return;
	}

	const diffMs = hasta - desde;
	const diffDias = diffMs / (1000 * 60 * 60 * 24);

	if (diffDias > 370) {

		// Calcular fechas por defecto
		const hoy = new Date();
		const hace30 = new Date();
		hace30.setDate(hoy.getDate() - 30);

		// Formatear a yyyy-MM-dd para los inputs type="date"
		const fmt = d => d.toISOString().split("T")[0];

		AbrirMensaje("ATENCIÓN", "El rango entre fechas no puede superar los 370 días.", function () {
			$("#msjModal").modal("hide");

			$("#FechaDesde").val(fmt(hace30));
			$("#FechaHasta").val(fmt(hoy));

			$("#FechaDesde").trigger('focus');
			return true;
		}, false, ["Aceptar"], "error!", null);

		return;
	}

}

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}