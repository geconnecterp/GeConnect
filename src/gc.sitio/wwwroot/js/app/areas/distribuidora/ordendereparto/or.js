let _pedidoLoading = false;
let orCompteSeleccionado = null;
let oreCompteSeleccionado = null;
let pcCompteSeleccionado = null;
let pceCompteSeleccionado = null;
let pcCompteSeleccionadoEnConsolidar = null;
let modoEdicionConteo = false;

$(function () {
	InicializaPantallaOrdenDeReparto();
	InicializaEventosOrdenDeReparto();
});

function InicializaPantallaOrdenDeReparto() {
	// INICIALIZAMOS PANELES
	if ($("#divDetalle").is(":visible")) {
		$("#divDetalle").collapse("hide");
	}
	$("#divFiltro").collapse("show");

	initPeriodoFechas();

	$("#chkDesdeHasta")
		.prop("checked", true)
		.prop("disabled", true);

	$("#Desde").prop("disabled", false);
	$("#Hasta").prop("disabled", false);

	// Etiquetas de filtros
	$("#lbChkDesdeHasta").text("Periodo");
	$("#lbEstados").text("Estado"); // Estados
	$("#lbRepartidores").text("Repartidores"); // Repartidores

	$("#chkEstados").on("click", function () {
		if ($("#chkEstados").is(":checked")) {
			$("#listaEstados").prop("disabled", false);
			$("#EstadosList").prop("disabled", false);
			$("#listaEstados").trigger("focus");
		}
		else {
			$("#listaEstados").prop("disabled", true).val("");
			$("#EstadosList").prop("disabled", true).empty();
		}
	});

	$("#chkRepartidores").on("click", function () {
		if ($("#chkRepartidores").is(":checked")) {
			$("#listaRepartidores").prop("disabled", false);
			$("#RepartidoresList").prop("disabled", false);
			$("#listaRepartidores").trigger("focus");
		}
		else {
			$("#listaRepartidores").prop("disabled", true).val("");
			$("#RepartidoresList").prop("disabled", true).empty();
		}
	});

	$("#EstadosList").on("dblclick", 'option', function () { $(this).remove(); })
	$("#RepartidoresList").on("dblclick", 'option', function () { $(this).remove(); })

	$(document).on("change", "#listaEstados", ControlalistaEstadosSelected);
	$(document).on("change", "#listaRepartidores", ControlalistaRepartidoresSelected);
}

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function initPeriodoFechas() {
	// Último lunes pasado
	const desde = obtenerUltimoLunes();

	// Hoy
	const hasta = new Date();

	// Formatear YYYY-MM-DD
	const fmt = d => d.toISOString().split("T")[0];

	$("#Desde").val(fmt(desde));
	$("#Hasta").val(fmt(hasta));

	// Siempre habilitadas
	$("#Desde").prop("disabled", false);
	$("#Hasta").prop("disabled", false);

	// Checkbox siempre marcado y deshabilitado
	$("#chkDesdeHasta")
		.prop("checked", true)
		.prop("disabled", true);
}

function obtenerUltimoLunes() {
	const hoy = new Date();
	const diaSemana = hoy.getDay(); // 0=Domingo ... 1=Lunes

	// Si hoy es lunes → retroceder 7 días
	const diferencia = diaSemana === 1 ? 7 : (diaSemana + 6) % 7;

	const ultimoLunes = new Date(hoy);
	ultimoLunes.setDate(hoy.getDate() - diferencia);

	return ultimoLunes;
}

function ControlalistaEstadosSelected() {
	var item = $("#listaEstados").val();
	var desc = $("#listaEstados option:selected").text();
	if ($("#EstadosList").has('option:contains("' + item + '")').length === 0 && $("#EstadosList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#EstadosList").append(opc);
	}
}

function ControlalistaRepartidoresSelected() {
	var item = $("#listaRepartidores").val();
	var desc = $("#listaRepartidores option:selected").text();
	if ($("#RepartidoresList").has('option:contains("' + item + '")').length === 0 && $("#RepartidoresList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#RepartidoresList").append(opc);
	}
}

function InicializaEventosOrdenDeReparto() {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacion(div);
	});

	// Buscar
	$("#btnBuscar").on("click", function () {
		buscarOrdenesDeReparto(1);
	});
	funcCallBack = buscarOrdenesDeReparto;
}

//async function buscarOrdenesDeReparto(btn, pag = 1) {
async function buscarOrdenesDeReparto(pag = 1) {
	if (_pedidoLoading) return;
	_pedidoLoading = true;
	pagina = pag;

	//const $btn = $(btn);
	//const originalHtml = $btn.html();
	//setBtnLoading($btn, true);
	AbrirWaiting("Inicializando búsqueda...")
	try {
		const filtros = buildQueryFilters(pag);
		const url = buscarOrdenesDeRepartoUrl;
		const urlInitView = inicializarViewUrl;

		PostGenHtml({}, urlInitView, function (html) {
			$("#divDetalle").html(html).collapse("show");
			$("#divFiltro").collapse("hide");
			CerrarWaiting();
			CargarOrdenesDeReparto(filtros, url);
		});


	} catch (e) {
		console.error("Error al buscar pedidos de clientes:", e);
		$("#divDetalle").html('<div class="alert alert-danger py-2 mb-0">No se pudo obtener la información.</div>').collapse("show");
	} finally {
		//setBtnLoading($btn, false, originalHtml);
		_pedidoLoading = false;
	}
}

function CargarOrdenesDeReparto(filtros, url) {
	AbrirWaiting("Cargando ordenes de reparto...");
	PostGenHtml(filtros, url, function (html) {
		CerrarWaiting();
		$("#divListaOrdenesDeReparto").html(html);

		configurarEventosSeleccionListaOR();

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

$(document).on("click", "#btnAgregarOR", function () {
	CargarVistaNuevaOrdenDeReparto("A", "");
});

$(document).on("click", "#btnModificarOR", function () {
	CargarVistaNuevaOrdenDeReparto("M", orCompteSeleccionado);
});

$(document).on("click", "#btnEnCurso", function () {
	CargarVistaAnalizaAutEnOrdenDeReparto(orCompteSeleccionado);
});

$(document).on("click", "#btnConsolidar", function () {
	CargarVistConsolidarOrdenDeReparto(orCompteSeleccionado);
});

$(document).on("click", "#btnCambioPrecio", function () {
	CargarVistCambioPrecioOrdenDeReparto(orCompteSeleccionado);
});

$(document).on("click", "#btnAFacturar", function () {
	PonerAFacturarOrdenDeReparto(orCompteSeleccionado);
});

$(document).on("click", "#btnVolverCurso", function () {
	VolverAEnCursoOrdenDeReparto(orCompteSeleccionado);
});

$(document).on("click", "#btnHojaRuta", function () {
	ControlaImprimirHojaDeRutaDeOrdenDeReparto();
});

$(document).on("click", "#btnHojaProd", function () {
	ControlaImprimirHojaDeProductoDeOrdenDeReparto();
});

$(document).on("click", "#btnCF", function () {
	PonerCFPedidoDeCliente(pcCompteSeleccionado);
});

$(document).on("click", "#btnPedido", function () {
	ControlaImprimirPedidoDeLaOrdenDeReparto();
});

$(document).on("click", "#btnDividir", function () {
	DividirPedidoDeCliente(pcCompteSeleccionado);
});

function DividirPedidoDeCliente(pcCompteSeleccionado) {
	var dividir = $("#txtDividir").val();
	if (!pcCompteSeleccionado || pcCompteSeleccionado == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un pedido de cliente.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else if (dividir <= 0) {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un valor válido para dividir el pedido del cliente.", function () {
			$("#msjModal").modal("hide");
			$("#txtDividir").trigger("focus");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirMensaje(
			'CONFIRMAR DIVISIÓN',
			"¿Desea dividir el pedido de cliente seleccionado en " + dividir + " pedidos?",
			function (resp) {
				if (resp === 'SI') {
					var data = { pc_compte: pcCompteSeleccionado, divide: dividir };
					PostGen(data, dividirPedidoDeClienteURL, function (obj) {
						CerrarWaiting();
						if (obj.error === true || obj.warn === true) {
							console.error('❌ Response:', obj.mensaje);
							AbrirMensaje("ATENCIÓN", 'Error al intentar dividir el pedido de cliente: ' + (obj.mensaje || 'Error desconocido'), function () {
								$("#msjModal").modal("hide");
								return true;
							}, false, ["Aceptar"], "error!", null);
						}
						else {
							setTimeout(() => {
								AbrirMensaje(
									'CONFIRMACIÓN EXITOSA',
									'Se ha dividido el pedido de cliente en ' + dividir + ' pedidos.',
									function () {
										$('#msjModal').modal('hide');
										//Actualizar tabla de Ordenes de Reparto
										CargarPedidosDeLaOrdenesDeReparto(orCompteSeleccionado);
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
				$('#msjModal').modal('hide');
			},
			true,
			['Confirmar', 'Cancelar'],
			'info!',
			null
		);
	}
}

function ControlaImprimirPedidoDeLaOrdenDeReparto() {
	if (!pcCompteSeleccionado || pcCompteSeleccionado == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un pedido de cliente.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Imprimiendo ...");
		var tipoReporte = 4;
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
				ImprimirPedidoDeLaOrdenDeReparto();
			}
		});
	}
}

function ImprimirPedidoDeLaOrdenDeReparto() {
	ReseteoDeReportes();
	setTimeout(() => {
		var pc_compte = pcCompteSeleccionado;
		var data = { pc_compte };
		cargarReporteEnArre(62, data, "PEDIDO DE CLIENTE", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function PonerCFPedidoDeCliente(pcCompteSeleccionado) {
	if (!pcCompteSeleccionado || pcCompteSeleccionado == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un pedido de cliente.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirMensaje(
			'CONFIRMAR CAMBIO DE ESTADO',
			"¿Desea poner CF el pedido de cliente?",
			function (resp) {
				if (resp === 'SI') {
					var data = { pc_compte: pcCompteSeleccionado };
					PostGen(data, pasarPedidoDeClienteACFURL, function (obj) {
						CerrarWaiting();
						if (obj.error === true || obj.warn === true) {
							console.error('❌ Response:', obj.mensaje);
							AbrirMensaje("ATENCIÓN", 'Error al intentar poner CF el pedido de cliente: ' + (obj.mensaje || 'Error desconocido'), function () {
								$("#msjModal").modal("hide");
								return true;
							}, false, ["Aceptar"], "error!", null);
						}
						else {
							setTimeout(() => {
								AbrirMensaje(
									'CONFIRMACIÓN EXITOSA',
									'Se ha puesto CF el pedido de cliente',
									function () {
										$('#msjModal').modal('hide');
										//Actualizar tabla de Ordenes de Reparto
										CargarPedidosDeLaOrdenesDeReparto(orCompteSeleccionado);
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
				$('#msjModal').modal('hide');
			},
			true,
			['Confirmar', 'Cancelar'],
			'info!',
			null
		);
	}
}

function CargarPedidosDeLaOrdenesDeReparto(orCompte) {
	AbrirWaiting("Cargando pedidos de cliente de la orden de reparto...");
	PostGenHtml({ orCompte }, cargarPedidosDeLaOrdenDeRepartoUrl, function (html) {
		CerrarWaiting();
		$("#divPedidosDeLaOrdenDeReparto").html(html).collapse("show");
	});
}


function VolverAEnCursoOrdenDeReparto(orCompteSeleccionado) {
	if (!orCompteSeleccionado || orCompteSeleccionado == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar una orden de reparto.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirMensaje(
			'CONFIRMAR CAMBIO DE ESTADO',
			"¿Desea volver a En Curso la orden de reparto?",
			function (resp) {
				if (resp === 'SI') {
					var data = { or_compte: orCompteSeleccionado, ore_id: "O" };
					PostGen(data, cambiarEstadoOrdenDeRepartoUrl, function (obj) {
						CerrarWaiting();
						if (obj.error === true || obj.warn === true) {
							console.error('❌ Response:', obj.mensaje);
							AbrirMensaje("ATENCIÓN", 'Error al intentar volver a en curso la O.R.: ' + (obj.mensaje || 'Error desconocido'), function () {
								$("#msjModal").modal("hide");
								return true;
							}, false, ["Aceptar"], "error!", null);
						}
						else {
							setTimeout(() => {
								AbrirMensaje(
									'CONFIRMACIÓN EXITOSA',
									'Se ha cambiado el estado a En Curso de la orden de reparto',
									function () {
										$('#msjModal').modal('hide');

										//Actualizar tabla de Ordenes de Reparto
										const filtros = buildQueryFilters(pagina);
										const url = buscarOrdenesDeRepartoUrl;
										CargarOrdenesDeReparto(filtros, url);
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
				$('#msjModal').modal('hide');
			},
			true,
			['Confirmar', 'Cancelar'],
			'info!',
			null
		);

	}
}

function PonerAFacturarOrdenDeReparto(orCompteSeleccionado) {
	if (!orCompteSeleccionado || orCompteSeleccionado == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar una orden de reparto.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirMensaje(
			'CONFIRMAR CAMBIO DE ESTADO',
			"¿Desea poner A Facturar la orden de reparto?",
			function (resp) {
				if (resp === 'SI') {
					var data = { or_compte: orCompteSeleccionado, ore_id: "T" };
					PostGen(data, cambiarEstadoOrdenDeRepartoUrl, function (obj) {
						CerrarWaiting();
						if (obj.error === true || obj.warn === true) {
							console.error('❌ Response:', obj.mensaje);
							AbrirMensaje("ATENCIÓN", 'Error al intentar poner a facturar la O.R.: ' + (obj.mensaje || 'Error desconocido'), function () {
								$("#msjModal").modal("hide");
								return true;
							}, false, ["Aceptar"], "error!", null);
						}
						else {
							setTimeout(() => {
								AbrirMensaje(
									'CONFIRMACIÓN EXITOSA',
									'Se ha cambiado el estado A Facturar de la orden de reparto',
									function () {
										$('#msjModal').modal('hide');

										//Actualizar tabla de Ordenes de Reparto
										const filtros = buildQueryFilters(pagina);
										const url = buscarOrdenesDeRepartoUrl;
										CargarOrdenesDeReparto(filtros, url);
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
				$('#msjModal').modal('hide');
			},
			true,
			['Confirmar', 'Cancelar'],
			'info!',
			null
		);
		
	}
}

function ControlaImprimirHojaDeProductoDeOrdenDeReparto() {
	if (!orCompteSeleccionado || orCompteSeleccionado == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar una orden de reparto.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Imprimiendo ...");
		var tipoReporte = 2;
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
				ImprimirHojaDeProductoDeOrdenDeReparto();
			}
		});
	}
}

function ImprimirHojaDeProductoDeOrdenDeReparto() {
	ReseteoDeReportes();
	setTimeout(() => {
		var orCompte = orCompteSeleccionado;
		var data = { orCompte };
		cargarReporteEnArre(64, data, "Orden de Reparto - Hoja de Producto", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ControlaImprimirHojaDeRutaDeOrdenDeReparto() {
	if (!orCompteSeleccionado || orCompteSeleccionado == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar una orden de reparto.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Imprimiendo ...");
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
				ImprimirHojaDeRutaDeOrdenDeReparto();
			}
		});
	}
}

function ImprimirHojaDeRutaDeOrdenDeReparto() {
	ReseteoDeReportes();
	setTimeout(() => {
		var orCompte = orCompteSeleccionado;
		var data = { orCompte };
		cargarReporteEnArre(63, data, "Orden de Reparto - Hoja de Ruta", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function CargarVistCambioPrecioOrdenDeReparto(orCompteSeleccionado) {
	AbrirWaiting("Cargando vista para cambio de precio en Orden de Reparto...");
	PostGenHtml({ or_compte: orCompteSeleccionado, lp_id: '003' }, cargarVistCambioPrecioOrdenDeRepartoUrl, function (html) {
		CerrarWaiting();
		$("#vistaCambioPrecioOR").html(html);
		$("#vistaListaOR").addClass("d-none");
		$("#vistaCambioPrecioOR").removeClass("d-none");

		// ================================
		// VALIDAR SI LA TABLA TIENE DATOS
		// ================================
		let hayDatos = $("#tbCambioDePrecio tbody tr").not(".fila-vacia").length > 0;

		if (!hayDatos) {
			$("#btnAnalizarCambioPrecio").prop("disabled", true);
		} else {
			$("#btnAnalizarCambioPrecio").prop("disabled", false);
		}

		ConfigurarEventosEnCambioPrecio();
	});
}

function ConfigurarEventosEnCambioPrecio() {
	$(document).off("click", "#tbCambioDePrecio tbody tr");
	$(document).on("click", "#tbCambioDePrecio tbody tr", function (e) {
		if (!$(e.target).is("button, a, .btn, i")) {
			var $this = $(this);
			var fueSeleccionado = $this.hasClass("selected-row");

			$("#tbCambioDePrecio tbody tr").removeClass("selected-row");

			if (!fueSeleccionado) {
				$this.addClass("selected-row");
				let pId = $this.data("p-id");
				//Do something
			}
		}
	});

	$(document).off("mouseenter", "#tbCambioDePrecio th, #tbCambioDePrecio td");
	$(document).on("mouseenter", "#tbCambioDePrecio th, #tbCambioDePrecio td", function () {

		const el = this;
		const isOverflowing = el.scrollWidth > el.clientWidth;

		if (isOverflowing) {
			$(el).attr("title", $(el).text().trim());
		} else {
			$(el).removeAttr("title");
		}
	});

	//btnAnalizarCambioPrecio
	$(document).off("click", "#btnAnalizarCambioPrecio");
	$(document).on("click", "#btnAnalizarCambioPrecio", function () {
		// 1) Obtener todos los checkboxes seleccionados
		let $seleccionados = $("#tbCambioDePrecio tbody .chk-actualizar-precio:checked");

		// 2) Validar si hay al menos uno
		if ($seleccionados.length === 0) {
			alert("Debe seleccionar al menos un producto para actualizar el precio.");
			AbrirMensaje("ATENCIÓN", "Debe seleccionar al menos un producto para actualizar el precio.", function () {
				$("#msjModal").modal("hide");
				return;
			}, false, ["Aceptar"], "error!", null);
			return;
		}
		else {
			AbrirMensaje(
				'CONFIRMAR CAMBIO DE PRECIOS',
				"¿Desea confirmar lo cambios de precioes en la orden de reparto?",
				function (resp) {
					if (resp === 'SI') {
						confirmarCambiosDePrecioEnOrdenDeReparto();
					}
					$('#msjModal').modal('hide');
				},
				true,
				['Confirmar', 'Cancelar'],
				'info!',
				null
			);
		}
	});

	//btnCancelarCambioPrecio
	$(document).off("click", "#btnCancelarCambioPrecio");
	$(document).on("click", "#btnCancelarCambioPrecio", function () {
		// 1) Obtener todos los checkboxes seleccionados
		let $seleccionados = $("#tbCambioDePrecio tbody .chk-actualizar-precio:checked");
		// 2) Validar si hay al menos uno
		if ($seleccionados.length === 0) {
			// Ocultar vista de edición
			document.querySelector("#vistaCambioPrecioOR").classList.add("d-none");
			// Mostrar vista de lista
			document.querySelector("#vistaListaOR").classList.remove("d-none");
			// Opcional: limpiar contenido de edición
			document.querySelector("#vistaCambioPrecioOR").innerHTML = "";
			habilitarTabPedidos();
		}
		else {
			AbrirMensaje(
				'CANCELAR CAMBIO DE PRECIOS',
				"¿Desea cancelar los cambios de precio en la orden de reparto?",
				function (resp) {
					if (resp === 'SI') {
						// Ocultar vista de edición
						document.querySelector("#vistaCambioPrecioOR").classList.add("d-none");
						// Mostrar vista de lista
						document.querySelector("#vistaListaOR").classList.remove("d-none");
						// Opcional: limpiar contenido de edición
						document.querySelector("#vistaCambioPrecioOR").innerHTML = "";
						habilitarTabPedidos();
					}
					$('#msjModal').modal('hide');
				},
				true,
				['Confirmar', 'Cancelar'],
				'info!',
				null
			);
		}
	});

	// Selección global
	$(document).off("change", "#chkSeleccionGlobal");
	$(document).on("change", "#chkSeleccionGlobal", function () {

		const marcado = $(this).is(":checked");

		$("#tbCambioDePrecio tbody .chk-actualizar-precio:not(:disabled)")
			.prop("checked", marcado);
	});

	// Si el usuario marca/desmarca manualmente, actualizar el checkbox global
	$(document).off("change", "#tbCambioDePrecio .chk-actualizar-precio");
	$(document).on("change", "#tbCambioDePrecio .chk-actualizar-precio", function () {

		const totalHabilitados = $("#tbCambioDePrecio .chk-actualizar-precio:not(:disabled)").length;
		const marcados = $("#tbCambioDePrecio .chk-actualizar-precio:not(:disabled):checked").length;

		$("#chkSeleccionGlobal").prop("checked", totalHabilitados > 0 && totalHabilitados === marcados);
	});

	deshabilitarTabPedidos();
}

function confirmarCambiosDePrecioEnOrdenDeReparto() {
	// Armar lista de productos seleccionados
	let $seleccionados = $("#tbCambioDePrecio tbody .chk-actualizar-precio:checked");
	let productos = [];

	$seleccionados.each(function () {

		let $chk = $(this);
		let p_id = $chk.data("p-id");

		// Buscar la fila completa
		let $fila = $chk.closest("tr");

		// Extraer precios desde las celdas
		let pcd_pvta = $fila.find("td").eq(3).text().trim(); // Precio Pedido
		let p_vta_ctl = $fila.find("td").eq(4).text().trim(); // Precio Distrib.

		// Normalizar decimales (quita separadores de miles)
		pcd_pvta = pcd_pvta.replace(/,/g, "");   // quita separador de miles
		p_vta_ctl = p_vta_ctl.replace(/,/g, ""); // quita separador de miles

		productos.push({
			p_id: p_id,
			pcd_pvta: parseFloat(pcd_pvta),
			p_vta_ctl: parseFloat(p_vta_ctl)
		});
	});

	// Armar request final
	let data = {
		orCompte: orCompteSeleccionado,
		prods: productos
	};

	console.log("Request a enviar:", data);
	PostGen(data, confirmarCambioDePreciosEnOrdenDeRepartoUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true || obj.warn === true) {
			console.error('❌ Response:', obj.mensaje);
			AbrirMensaje("ATENCIÓN", 'Error al intentar cambiar precios en la O.R.: ' + (obj.mensaje || 'Error desconocido'), function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			setTimeout(() => {
				AbrirMensaje(
					'CONFIRMACIÓN EXITOSA',
					'Se han modificado los precios de la orden de reparto',
					function () {
						$('#msjModal').modal('hide');
						document.querySelector("#vistaCambioPrecioOR").classList.add("d-none");
						document.querySelector("#vistaListaOR").classList.remove("d-none");
						document.querySelector("#vistaCambioPrecioOR").innerHTML = "";
						habilitarTabPedidos();
						//Actualizar tabla de Ordenes de Reparto
						const filtros = buildQueryFilters(pagina);
						const url = buscarOrdenesDeRepartoUrl;
						CargarOrdenesDeReparto(filtros, url);
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

function CargarVistConsolidarOrdenDeReparto(orCompte) {
	AbrirWaiting("Cargando vista para consolidar Orden de Reparto...");
	PostGenHtml({ orCompte: orCompte }, cargarVistaConsolidarOrdenDeRepartoUrl, function (html) {
		CerrarWaiting();
		$("#vistaConsolidarOR").html(html);
		$("#vistaListaOR").addClass("d-none");
		$("#vistaConsolidarOR").removeClass("d-none");
		ConfigurarEventosEnPonerEnConsolidar();
		CargarConteosEnConsolidar(orCompteSeleccionado);

		// Seleccionar automáticamente el primer pedido
		SeleccionarPrimerPedidoEnConsolidar();
	});
}

function SeleccionarPrimerPedidoEnConsolidar() {

	const $primerRow = $("#tbConsolidarPedidos tbody tr.row-pedido").first();

	if ($primerRow.length === 0) return; // no hay datos

	// Marcar visualmente
	$("#tbConsolidarPedidos tbody tr").removeClass("selected-row");
	$primerRow.addClass("selected-row");

	// Obtener pc_compte
	const pcCompte = $primerRow.data("pc-compte");
	pcCompteSeleccionadoEnConsolidar = pcCompte;

	// Cargar detalle
	CargarDetalleDelPedidoDeLaOrdenEnConsolidar(orCompteSeleccionado, pcCompteSeleccionadoEnConsolidar);
}

function ConfigurarEventosEnPonerEnConsolidar() {
	$(document).off("click", "#tbConsolidarPedidos tbody tr");
	$(document).on("click", "#tbConsolidarPedidos tbody tr", function (e) {
		if (!$(e.target).is("button, a, .btn, i")) {
			var $this = $(this);
			var fueSeleccionado = $this.hasClass("selected-row");

			$("#tbConsolidarPedidos tbody tr").removeClass("selected-row");

			if (!fueSeleccionado) {
				$this.addClass("selected-row");
				let pcCompte = $this.data("pc-compte");
				pcCompteSeleccionadoEnConsolidar = pcCompte;
				CargarDetalleDelPedidoDeLaOrdenEnConsolidar(orCompteSeleccionado, pcCompteSeleccionadoEnConsolidar);
				//CargarConteosEnConsolidar(orCompteSeleccionado);
			}
		}
	});

	$(document).off("click", "#tbConsolidarConteos tbody tr");
	$(document).on("click", "#tbConsolidarConteos tbody tr", function (e) {
		if (!$(e.target).is("button, a, .btn, i")) {
			var $this = $(this);
			var fueSeleccionado = $this.hasClass("selected-row");

			$("#tbConsolidarConteos tbody tr").removeClass("selected-row");

			if (!fueSeleccionado) {
				$this.addClass("selected-row");
				let orCompte = $this.data("or-compte");
				let pId = $this.data("p-id");
				CargarDetalleDelProductoSeleccionadoEnConteo(orCompte, pId);
			}
		}
	});

	//btnConfirmarConciliacion
	$(document).off("click", "#btnConfirmarConciliacion");
	$(document).on("click", "#btnConfirmarConciliacion", function () {
		AbrirMensaje(
			'CONFIRMAR CONSOLIDAR',
			"¿Desea confirmar la consolidación de la orden de reparto?",
			function (resp) {
				if (resp === 'SI') {
					confirmarConsolidarOrdenDeReparto();
				}
				$('#msjModal').modal('hide');
			},
			true,
			['Confirmar', 'Cancelar'],
			'info!',
			null
		);
	});

	//btnCancelarConciliacion
	$(document).off("click", "#btnCancelarConciliacion");
	$(document).on("click", "#btnCancelarConciliacion", function () {
		AbrirMensaje(
			'CONFIRMAR CANCELACIÓN',
			"¿Desea cancelar la consolidación?",
			function (resp) {
				if (resp === 'SI') {
					// Ocultar vista de edición
					document.querySelector("#vistaConsolidarOR").classList.add("d-none");
					// Mostrar vista de lista
					document.querySelector("#vistaListaOR").classList.remove("d-none");
					// Opcional: limpiar contenido de edición
					document.querySelector("#vistaConsolidarOR").innerHTML = "";
					habilitarTabPedidos();
				}
				$('#msjModal').modal('hide');
			},
			true,
			['Confirmar', 'Cancelar'],
			'info!',
			null
		);
	});

	//btnReasignar
	$(document).off("click", "#btnReasignar");
	$(document).on("click", "#btnReasignar", function () {
		modoEdicionConteo = true;

		// 🔥 Deshabilitar selección de tablas
		$("#tbConsolidarConteos").addClass("tabla-bloqueada");
		$("#tbConsolidarDetallesPedido").addClass("tabla-bloqueada");
		$("#tbConsolidarPedidos").addClass("tabla-bloqueada");

		// 🔥 Deshabilitar botones
		$("#btnReasignar").prop("disabled", true);
		$("#btnConfirmarReasignacion").prop("disabled", false);
		$("#btnCancelarReasignacion").prop("disabled", false);


		HabilitarEdicionEnDetalleConteo();
	});

	//btnConfirmarReasignacion
	$(document).off("click", "#btnConfirmarReasignacion");
	$(document).on("click", "#btnConfirmarReasignacion", function () {
		// 🔥 Asegurar que los cambios del input se apliquen
		ConsolidarEdicionDetalleConteo();

		if (!ValidarAEnviarIgualAPedido()) {
			AbrirMensaje(
				'VALIDACIÓN',
				'Los valores de "A Enviar" deben coincidir con los valores de "Pedido".',
				function () { $('#msjModal').modal('hide'); },
				false,
				['Aceptar'],
				'error!',
				null
			);
			return;
		}

		if (HayCambiosEnDetalleConteo()) {
			AbrirMensaje(
				'CONFIRMAR REASIGNACIÓN',
				"¿Desea confirmar las modificaciones realizadas?",
				function (resp) {
					if (resp === 'SI') {
						GuardarReasignacionEnDatosDeSesion();

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
		}
	});

	//btnCancelarReasignacion
	$(document).off("click", "#btnCancelarReasignacion");
	$(document).on("click", "#btnCancelarReasignacion", function () {
		// 1) Ver si hubo cambios
		if (HayCambiosEnDetalleConteo()) {
			AbrirMensaje(
				'CANCELAR REASIGNACIÓN',
				"¿Desea cancelar las modificaciones realizadas?",
				function (resp) {
					CancelarModificacionesEnReasginacion();
					$('#msjModal').modal('hide');
				},
				true,
				['Confirmar', 'Cancelar'],
				'info!',
				null
			);

			// 2) Pedir confirmación
			//if (!confirm("Hay cambios sin guardar. ¿Desea descartar las modificaciones?")) {
			//	return; // ❌ No salir del modo edición
			//}
		}
		else {
			CancelarModificacionesEnReasginacion();
		}
	});

	deshabilitarTabPedidos();
}

function confirmarConsolidarOrdenDeReparto() {
	AbrirWaiting("Consolidando orden de reparto...");
	let data = {
		orCompte: orCompteSeleccionado
	};
	console.log("Payload a enviar:", data);
	PostGen(data, confirmarConsolidarOrdenDeRepartoUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true || obj.warn === true) {
			console.error('❌ Response:', obj.mensaje);
			AbrirMensaje("ATENCIÓN", obj.mensaje || "Error desconocido", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			setTimeout(() => {
				AbrirMensaje(
					'CONFIRMACIÓN EXITOSA',
					'Se ha consolidado la orden de reparto',
					function () {
						$('#msjModal').modal('hide');
						document.querySelector("#vistaConsolidarOR").classList.add("d-none");
						document.querySelector("#vistaListaOR").classList.remove("d-none");
						document.querySelector("#vistaConsolidarOR").innerHTML = "";
						habilitarTabPedidos();
						//Actualizar tabla de Ordenes de Reparto
						const filtros = buildQueryFilters(pagina);
						const url = buscarOrdenesDeRepartoUrl;
						CargarOrdenesDeReparto(filtros, url);
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

function GuardarReasignacionEnDatosDeSesion() {

	let orId = orCompteSeleccionado;
	let pedidoClienteId = pcCompteSeleccionado;
	let detalle = [];

	$("#tbConsolidarDetalleConteo tbody tr").each(function () {

		let $fila = $(this);
		// Obtener ID del producto (p_id)
		let productoId = $fila.data("p-id");
		let orCompte = $fila.data("or-compte");
		let pcCompte = $fila.data("pc-compte");
		// Obtener cantidad desde el input
		let $input = $fila.find("td.celda-a-enviar input");
		let cantidad = $input.val().trim();

		if (cantidad === "") cantidad = "0";

		// Quitar separadores de miles
		cantidad = cantidad.replace(/,/g, "");
		detalle.push({
			pId: productoId,
			cantidad: parseFloat(cantidad),
			orCompte: orCompte,
			pcCompte: pcCompte
		});
	});

	let data = {
		detalle: detalle
	};

	console.log("Payload a enviar:", data);
	AbrirWaiting("Actualizando cantidades de productos...")
	$.ajax({
		url: guardarReasignacionEnDatosDeSesionUrl,
		type: "POST",
		contentType: "application/json",
		data: JSON.stringify(data),
		success: function (resp) {
			CerrarWaiting();
			if (resp.error || resp.warn) {
				console.error('❌ Response:', resp.mensaje);
				AbrirMensaje("ATENCIÓN", 'Error al intentar reasignar cantidades de productos: ' + (resp.mensaje || 'Error desconocido'), function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				setTimeout(() =>
					EvaluarHabilitarReasignar(),
					EstadoInicialBotonesOKCancelEnDetalleDeConteos(),
					500);
				$("#tbConsolidarDetalleConteo .editor-celda").each(function () {
					let valor = $(this).val().trim().replace(/,/g, "");
					$(this).attr("data-original", valor);
				});
				ConfirmarModificacionesEnReasginacion();
			}
		},
		error: function (err) {
			CerrarWaiting();
			console.error("Error al enviar reasignación:", err);
		}
	});
}

function ConfirmarModificacionesEnReasginacion() {
	AbrirWaiting("Finalizando actualización...");
	// 2) Guardar valores definitivos
	$("#tbConsolidarDetalleConteo tbody tr").each(function () {

		let $input = $(this).find("td.celda-a-enviar input");

		if ($input.length > 0) {

			let valor = $input.val().trim();
			if (valor === "") valor = "0";

			// Quitar separadores de miles
			valor = valor.replace(/,/g, "");

			// Guardar valor final en el input
			$input.val(valor);

			// Actualizar el valor original para futuras ediciones
			$input.data("original", valor);
		}
	});

	// 3) Recalcular diferencias
	RecalcularDiferenciasEnDetalleConteo();

	// 4) Salir de modo edición
	modoEdicionConteo = false;

	// 5) Deshabilitar inputs
	$("#tbConsolidarDetalleConteo tbody tr td.celda-a-enviar input")
		.prop("disabled", true);

	// 6) Habilitar tablas y botones
	$("#tbConsolidarConteos, #tbConsolidarDetallesPedido, #tbConsolidarPedidos")
		.removeClass("tabla-bloqueada");

	$("#btnReasignar").prop("disabled", false);
	$("#btnConfirmarConciliacion").prop("disabled", false);
	$("#btnCancelarConciliacion").prop("disabled", false);
	CerrarWaiting();
}

function CancelarModificacionesEnReasginacion() {
	$("#tbConsolidarDetalleConteo tbody tr").each(function () {

		let $input = $(this).find("td.celda-a-enviar input");
		let original = $input.data("original");

		$input.val(original);
	});

	RecalcularDiferenciasEnDetalleConteo();

	modoEdicionConteo = false;

	// Deshabilitar inputs
	$("#tbConsolidarDetalleConteo tbody tr td.celda-a-enviar input")
		.prop("disabled", true);

	// Habilitar tablas y botones
	$("#tbConsolidarConteos, #tbConsolidarDetallesPedido, #tbConsolidarPedidos")
		.removeClass("tabla-bloqueada");

	$("#btnReasignar").prop("disabled", false);
	$("#btnConfirmarConciliacion").prop("disabled", false);
	$("#btnCancelarConciliacion").prop("disabled", false);
}

function RecalcularColoresEnDetallesPedido() {

	$("#tbConsolidarDetalleConteo tbody tr").each(function () {

		let $row = $(this);

		// Obtener Pedido (columna 3 → índice 3)
		let pedidoTexto = $row.find("td").eq(3).text().trim();

		// Obtener A Enviar (columna 4 → índice 4)
		let enviarTexto = $row.find("td").eq(4).find("input").val().trim();

		let pedido = parseFloat(pedidoTexto.replace(",", "."));
		let enviar = parseFloat(enviarTexto.replace(",", "."));

		if (isNaN(pedido) || isNaN(enviar)) return true;

		let dif = pedido - enviar;

		// Actualizar celda DIF (columna 5 → índice 5)
		let $celdaDif = $row.find("td").eq(5);
		$celdaDif.text(dif);

		// Quitar clases previas
		$celdaDif.removeClass("cell-up cell-down cell-zero");

		// Aplicar clase nueva
		if (dif > 0) {
			$celdaDif.addClass("cell-up");
		} else if (dif < 0) {
			$celdaDif.addClass("cell-down");
		} else {
			$celdaDif.addClass("cell-zero");
		}
	});
}


function CargarDetalleDelProductoSeleccionadoEnConteo(orCompte, pId) {
	AbrirWaiting("Cargando detalle de conteos en Pedidos...");
	PostGenHtml({ orCompte: orCompte, pId: pId }, cargarDetalleDelProductoEnConteoEnConsolidarUrl, function (html) {
		CerrarWaiting();
		$("#divConsolidarDetalleProductoSeleccionado").html(html);
		ConfigurarEventosEnProductoSeleccionadoEnDetalleDeConteo();

		setTimeout(() =>
			EvaluarHabilitarReasignar(),
			EstadoInicialBotonesOKCancelEnDetalleDeConteos(),
			500);
	});
}

function ValidarAEnviarIgualAPedido() {

	let valido = true;

	$("#tbConsolidarDetalleConteo tbody tr").each(function () {

		let $row = $(this);

		// Obtener Pedido (columna 4 → índice 3)
		let pedidoTexto = $row.find("td").eq(3).text().trim();
		let pedido = parseFloat(pedidoTexto.replace(",", "."));

		// Obtener A Enviar
		let valorActual = $row.find("td.celda-a-enviar input").val().trim();
		valorActual = parseFloat(valorActual.replace(",", "."));

		// Comparación
		if (pedido !== valorActual) {
			valido = false;
			return false; // cortar el each
		}
	});

	return valido;
}

function HayCambiosEnDetalleConteo() {

	let huboCambios = false;

	$("#tbConsolidarDetalleConteo tbody tr").each(function () {

		let $celda = $(this).find("td.celda-a-enviar");
		let $input = $celda.find("input");

		if ($input.length === 0) return true; // continuar

		let original = $input.attr("data-original");
		let valorActual = $input.val().trim().replace(/,/g, "");

		if (original !== valorActual) {
			huboCambios = true;
			return false; // cortar el each
		}
	});

	return huboCambios;
}

function ConfigurarEventosEnProductoSeleccionadoEnDetalleDeConteo() {
}

function RecalcularDiferenciasEnDetalleConteo() {

	$("#tbConsolidarDetalleConteo tbody tr").each(function () {

		let $fila = $(this);

		let pedido = parseFloat($fila.find("td").eq(3).text().trim().replace(",", "."));
		let enviar = parseFloat(
			$fila.find("td.celda-a-enviar input").val().trim().replace(/,/g, "").replace(",", ".")
		);


		if (isNaN(pedido)) pedido = 0;
		if (isNaN(enviar)) enviar = 0;

		let dif = enviar - pedido;

		let $celdaDif = $fila.find("td.celda-dif");

		// Actualizar valor
		$celdaDif.text(dif);
	});
}


function HabilitarEdicionEnDetalleConteo() {

	$("#tbConsolidarDetalleConteo tbody tr").each(function () {

		let $fila = $(this);
		let $input = $fila.find("td.celda-a-enviar input");

		let permiteDecimales = $input.data("permite-decimales") === true ||
			$input.data("permite-decimales") === "true";

		// Habilitar input
		$input.prop("disabled", false);

		// Aplicar máscara
		if (permiteDecimales) {
			$input.inputmask(maskConfigDecimales);
		} else {
			$input.inputmask(maskConfigEnteros);
		}
	});

	ConfigurarEventosDeEdicionEnDetalleConteo();

}

function ConfigurarEventosDeEdicionEnDetalleConteo() {

	$("#tbConsolidarDetalleConteo .editor-celda").off();
	$("#tbConsolidarDetalleConteo .editor-celda").on("keypress", function (e) {
		let permiteDecimales = $(this).data("permite-decimales");

		// Solo números
		if (e.which < 48 || e.which > 57) {
			// Permitir punto decimal si corresponde
			if (permiteDecimales && e.which === 46)
				return;

			e.preventDefault();
		}
	});

	$("#tbConsolidarDetalleConteo .editor-celda").on("blur", function () {
		if (modoEdicionConteo) {
			return;
		}

		// Modo edición desactivado → sí se aplica el blur normal
		let valor = $(this).val().trim();
		if (valor === "") valor = "0";

		// Quitar máscara para obtener el número real
		valor = valor.replace(/,/g, "");

		$(this).val(valor);
		RecalcularDiferenciasEnDetalleConteo();
		// 🔥 Recalcular colores en la tabla de detalles del pedido
		RecalcularColoresEnDetallesPedido();
	});

	$("#tbConsolidarDetalleConteo").on("keydown", ".editor-celda", function (e) {

		let $input = $(this);
		let $fila = $input.closest("tr");
		let $todasLasFilas = $("#tbConsolidarDetalleConteo tbody tr").not(".fila-vacia");
		let index = $todasLasFilas.index($fila);

		// ENTER o TAB → guardar y pasar a la siguiente fila
		if (e.key === "Enter" || e.key === "Tab") {
			e.preventDefault();
			GuardarValorYRecalcular($input);

			// Ir a la siguiente fila
			let nextIndex = (index + 1) % $todasLasFilas.length;
			ActivarEdicionEnFila($todasLasFilas.eq(nextIndex));
			return;
		}

		// FLECHA ARRIBA
		if (e.key === "ArrowUp") {
			e.preventDefault();
			GuardarValorYRecalcular($input);

			let prevIndex = index - 1;
			if (prevIndex < 0) prevIndex = $todasLasFilas.length - 1;

			ActivarEdicionEnFila($todasLasFilas.eq(prevIndex));
			return;
		}

		// FLECHA ABAJO
		if (e.key === "ArrowDown") {
			e.preventDefault();
			GuardarValorYRecalcular($input);

			let nextIndex = (index + 1) % $todasLasFilas.length;

			ActivarEdicionEnFila($todasLasFilas.eq(nextIndex));
			return;
		}
	});

	$("#tbConsolidarDetalleConteo").on("change", ".editor-celda", function () {
		if (!modoEdicionConteo) return;

		let $input = $(this);
		GuardarValorYRecalcular($input);
	});
}

function ConsolidarEdicionDetalleConteo() {
	$("#tbConsolidarDetalleConteo .editor-celda").each(function () {

		let $input = $(this);
		let valor = $input.val().trim();

		if (valor === "") valor = "0";

		// Normalizar número
		valor = valor.replace(/,/g, "");

		// 🔥 Mantener el input, solo actualizar su valor
		$input.val(valor);

		// 🔥 Actualizar data-original en el TD contenedor
		let $td = $input.closest("td");
		$td.attr("data-original", valor);
	});
}

function GuardarValorYRecalcular($input) {

	let valor = $input.val().trim();
	if (valor === "") valor = "0";

	valor = valor.replace(/,/g, "");

	// Guardar en el input (no reemplazar el td)
	$input.val(valor);

	RecalcularDiferenciasEnDetalleConteo();
	// 🔥 Recalcular colores en la tabla de detalles del pedido
	RecalcularColoresEnDetallesPedido();
}

function EvaluarHabilitarReasignar() {

	// 1) Fila seleccionada
	let filaSeleccionada = $("#tbConsolidarConteos tbody tr.selected-row");

	if (filaSeleccionada.length === 0) {
		$("#btnReasignar").prop("disabled", true);
		return;
	}

	// 2) Obtener Dif (columna 6 → índice 5)
	let difTexto = filaSeleccionada.find("td").eq(5).text().trim();
	let dif = parseFloat(difTexto.replace(",", "."));

	// 3) Obtener Cantidad (columna 4 → índice 3)
	let cantidadTexto = filaSeleccionada.find("td").eq(3).text().trim();
	let cantidad = parseFloat(cantidadTexto.replace(",", "."));

	// 4) Filas reales en la grilla inferior
	let filasInferiores = $("#tbConsolidarDetalleConteo tbody tr")
		.not(".fila-vacia")
		.length;

	// 5) Nueva regla: si cantidad == 0 → NO habilitar
	if (cantidad === 0) {
		$("#btnReasignar").prop("disabled", true);
		return;
	}

	// 6) Reglas originales
	if (dif !== 0 && filasInferiores > 1) {
		$("#btnReasignar").prop("disabled", false);
	} else {
		$("#btnReasignar").prop("disabled", true);
	}
}

function ActivarEdicionEnFila($fila) {

	let $input = $fila.find("td.celda-a-enviar input");

	let permiteDecimales = $input.data("permite-decimales") === true ||
		$input.data("permite-decimales") === "true";

	// Habilitar input
	$input.prop("disabled", false);

	// Aplicar máscara
	if (permiteDecimales) {
		$input.inputmask(maskConfigDecimales);
	} else {
		$input.inputmask(maskConfigEnteros);
	}

	// Foco automático
	setTimeout(() => $input.focus().select(), 10);
}


function EstadoInicialBotonesOKCancelEnDetalleDeConteos() {
	$("#btnConfirmarReasignacion").prop("disabled", true);
	$("#btnCancelarReasignacion").prop("disabled", true);
}

function CargarDetalleDelPedidoDeLaOrdenEnConsolidar(orCompte, pcCompte) {
	AbrirWaiting("Cargando productos del pedido...");
	PostGenHtml({ orCompte: orCompte, pcCompte: pcCompte }, cargarDetalleDelPedidoDeLaOrdenEnConsolidarUrl, function (html) {
		CerrarWaiting();
		$("#divConsolidarDetallesPedido").html(html);
		ConfigurarEventosEnPedidosDeLaOrdenEnConsolidar();
	});
}

function CargarConteosEnConsolidar(orCompte) {
	AbrirWaiting("Cargando conteos...");
	PostGenHtml({ orCompte: orCompte }, cargarConteosEnConsolidarUrl, function (html) {
		CerrarWaiting();
		$("#divConsolidarConteos").html(html);
		ConfigurarEventosEnConteosEnConsolidar();
	});
}

function ConfigurarEventosEnConteosEnConsolidar() {
	$(document).off("click", "#tbConsolidarConteos tbody tr");
	$(document).on("click", "#tbConsolidarConteos tbody tr", function (e) {
		if (!$(e.target).is("button, a, .btn, i")) {
			var $this = $(this);
			var fueSeleccionado = $this.hasClass("selected-row");

			$("#tbConsolidarConteos tbody tr").removeClass("selected-row");

			if (!fueSeleccionado) {
				$this.addClass("selected-row");
				let pId = $this.data("p-id");
				let orCompte = $this.data("or-compte");
				CargarDetalleDelProductoSeleccionadoEnConteo(orCompte, pId);
			}
		}
	});
}

function ConfigurarEventosEnPedidosDeLaOrdenEnConsolidar() {
	$(document).off("click", "#tbConsolidarDetallesPedido tbody tr");
	$(document).on("click", "#tbConsolidarDetallesPedido tbody tr", function (e) {
		if (!$(e.target).is("button, a, .btn, i")) {
			var $this = $(this);
			var fueSeleccionado = $this.hasClass("selected-row");

			$("#tbConsolidarDetallesPedido tbody tr").removeClass("selected-row");

			if (!fueSeleccionado) {
				$this.addClass("selected-row");
				let pId = $this.data("p-id");
				//Hacer algo?
			}
		}
	});
}

function CargarVistaAnalizaAutEnOrdenDeReparto(orCompte) {
	AbrirWaiting("Cargando vista de análisis de autorización de Orden de Reparto...");
	PostGenHtml({ orCompte: orCompte }, cargarVistaAnalizaAutEnOrdenDeRepartoUrl, function (html) {
		CerrarWaiting();
		$("#vistaPonerEnCursoOR").html(html);
		$("#vistaListaOR").addClass("d-none");
		$("#vistaPonerEnCursoOR").removeClass("d-none");
		ConfigurarEventosEnPonerEnCurso();
	});
}

function ConfigurarEventosEnPonerEnCurso() {
	$(document).off("click", "#btnAnalizarPonerEnCurso");
	$(document).on("click", "#btnAnalizarPonerEnCurso", function () {
		// Obtener depósitos seleccionados
		//const depositosSeleccionados = [...document.querySelectorAll(".chk-depo:checked")]
		//	.map(chk => chk.closest("tr").dataset.depoId);
		const depositosSeleccionados = $("#tbDepositos tbody .chk-depo:checked")
			.map(function () {
				return $(this).closest("tr").data("depoId");
			}).get();

		// Validar selección
		if (depositosSeleccionados.length === 0) {
			AbrirMensaje("ATENCIÓN", "Debe seleccionar al menos un depósito.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
			return;
		}

		// Armar string con @
		const cadenaDepositos = depositosSeleccionados.join("@");

		console.log("Depósitos seleccionados:", cadenaDepositos);

		// Llamada al backend
		AbrirWaiting("Ejecutando análisis");
		var data = { orCompte: orCompteSeleccionado, listaDepo: cadenaDepositos }
		PostGenHtml(data, actualizarGrillaAnalizaAutoEnOrdenDeRepartoUrl, function (html) {
			CerrarWaiting();
			$("#tbGrillaAnalizaAut").html(html);
			configurarEventosSeleccionListaAnalisisAutOR();
			AgregarHanlderColumnaDescripcion();
		});
	});

	$(document).off("click", "#btnConfirmarPonerEnCurso");
	$(document).on("click", "#btnConfirmarPonerEnCurso", function () {

		// Validar que existan filas con datos
		const filas = $("#tbGrillaAnalizaAut tbody tr.row-analisis");

		if (filas.length === 0) {
			AbrirMensaje("ATENCIÓN", "Debe analizar la orden de reparto antes de ponerla en curso.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
			return;
		}

		// Si hay filas → abrir confirmación
		AbrirMensaje(
			'CONFIRMAR',
			"¿Desea poner en curso la orden de reparto?",
			function (resp) {
				ConfirmarPonerEnCursoOrdenDeReparto(orCompteSeleccionado);
				$('#msjModal').modal('hide');
			},
			true,
			['Confirmar', 'Cancelar'],
			'info!',
			null
		);
	});


	$(document).off("click", "#btnCancelarPonerEnCurso");
	$(document).on("click", "#btnCancelarPonerEnCurso", function () {
		AbrirMensaje(
			'CONFIRMAR CANCELACIÓN',
			"¿Desea cancelar el análisis?",
			function (resp) {
				if (resp === 'SI') {
					// Ocultar vista de edición
					document.querySelector("#vistaPonerEnCursoOR").classList.add("d-none");
					// Mostrar vista de lista
					document.querySelector("#vistaListaOR").classList.remove("d-none");
					// Opcional: limpiar contenido de edición
					document.querySelector("#vistaPonerEnCursoOR").innerHTML = "";
					habilitarTabPedidos();
				}
				$('#msjModal').modal('hide');
			},
			true,
			['Confirmar', 'Cancelar'],
			'info!',
			null
		);
	});

	$(document).off("change", "#chkDepoMaster");
	$(document).on("change", "#chkDepoMaster", function () {
		const checked = $(this).is(":checked");
		$("#tbDepositos tbody .chk-depo").prop("checked", checked);
	});

	$(document).off("change", "#tbDepositos tbody .chk-depo");
	$(document).on("change", "#tbDepositos tbody .chk-depo", function () {

		const total = $("#tbDepositos tbody .chk-depo").length;
		const marcados = $("#tbDepositos tbody .chk-depo:checked").length;

		$("#chkDepoMaster").prop("checked", total > 0 && total === marcados);
	});

	deshabilitarTabPedidos();
}

function ConfirmarPonerEnCursoOrdenDeReparto(orCompteSeleccionado) {
	AbrirWaiting("Poniendo en curso la orden de reparto...");
	$.ajax({
		url: confirmarPonerEnCursoOrdenDeRepartoUrl,
		type: 'POST',
		contentType: 'application/json; charset=utf-8',
		data: JSON.stringify({ or_compte: orCompteSeleccionado }),
		dataType: 'json',
		success: function (response) {
			CerrarWaiting();
			if (response.error || response.warn) {
				console.error('❌ Response:', response.mensaje);
				AbrirMensaje("ATENCIÓN", 'Error al poner en curso la orden de reparto: ' + (response.mensaje || 'Error desconocido'), function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				AbrirMensaje(
					'CONFIRMACIÓN EXITOSA',
					'Se ha puesto en curso la orden de reparto',
					function () {
						$('#msjModal').modal('hide');
						document.querySelector("#vistaPonerEnCursoOR").classList.add("d-none");
						document.querySelector("#vistaListaOR").classList.remove("d-none");
						document.querySelector("#vistaPonerEnCursoOR").innerHTML = "";

						//Actualizar tabla de Ordenes de Reparto
						const filtros = buildQueryFilters(pagina);
						const url = buscarOrdenesDeRepartoUrl;
						CargarOrdenesDeReparto(filtros, url);
					},
					false,
					['Aceptar'],
					'success!',
					null
				);
			}
		},
		error: function (xhr, status, error) {
			CerrarWaiting();
			console.error('❌ Error al poner en curso la orden de reparto:', error);
			console.error('❌ Response:', xhr.responseText);
			ControlaMensajeError(
				'Error al poner en curso la orden de reparto: ' +
				(xhr.responseJSON?.mensaje || xhr.statusText || 'Error desconocido')
			);
		}
	});
}

function CargarVistaNuevaOrdenDeReparto(abm, orCompte) {
	if (orCompte == null || orCompte == undefined || orCompte == "") {
		AbrirMensaje("ATENCIÓN", 'Debe seleccionar una Orden de Reparto', function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Cargando ABM de Orden de Reparto");
		PostGenHtml({ accion: abm, orCompte: orCompte }, cargarVistaABMOrdenDeRepartoUrl, function (html) {
			CerrarWaiting();
			$("#vistaEditarOR").html(html);
			$("#vistaListaOR").addClass("d-none");
			$("#vistaEditarOR").removeClass("d-none");
			activarSeleccionDeFilas("#tbPedidosOR tbody");
			activarSeleccionDeFilas("#tbPedidosPendientes tbody");
			deshabilitarTabPedidos();
		});
	}
}

$(document).on("click", "#btnConfirmarORenABM", function () {

	// ============================
	// 1) Validar repartidor
	// ============================
	const repartidor = $("#RepartidorSeleccionado").val();
	if (!repartidor || repartidor === "" || repartidor === "-- Seleccione --") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un repartidor.", function () {
			$("#msjModal").modal("hide");
			$("#RepartidorSeleccionado").trigger("focus");
			return true;
		}, false, ["Aceptar"], "error!", null);
		return;
	}

	// ============================
	// 2) Validar observaciones
	// ============================
	const observaciones = $("#OrdenDeReparto_or_obs").val()?.trim();
	if (!observaciones || observaciones.length === 0) {
		AbrirMensaje("ATENCIÓN", "Debe ingresar observaciones.", function () {
			$("#msjModal").modal("hide");
			$("#OrdenDeReparto_or_obs").trigger("focus");
			return true;
		}, false, ["Aceptar"], "error!", null);
		return;
	}

	// ============================
	// 3) Validar que haya pedidos en la tabla izquierda
	// ============================
	const filas = $("#tbPedidosOR tbody tr").not(".fila-vacia");
	if (filas.length === 0) {
		AbrirMensaje("ATENCIÓN", "Debe agregar al menos un pedido a la Orden de Reparto.", function () {
			$("#msjModal").modal("hide");
			$("#OrdenDeReparto_or_obs").trigger("focus");
			return true;
		}, false, ["Aceptar"], "error!", null);
		return;
	}

	// ============================
	// 4) Determinar mensaje según acción
	// ============================
	const accion = $("#accion").val(); // "A" = Alta, "M" = Modificación
	let mensaje = "";

	if (accion === "A") {
		mensaje = "¿Confirma la creación de la nueva Orden de Reparto?";
	} else if (accion === "M") {
		mensaje = "¿Confirma la modificación de la Orden de Reparto?";
	} else {
		mensaje = "¿Confirma la operación?";
	}

	// ============================
	// 5) Abrir mensaje de confirmación
	// ============================
	AbrirMensaje(
		"CONFIRMAR CARGA/MODIFICACIÓN",
		mensaje,
		function (resp) {
			if (resp === "SI") {
				// ============================
				// 6) Llamar al backend
				// ============================
				confirmarOrdenDeReparto(); // <-- método que implementás vos
			}

			$("#msjModal").modal("hide");
		},
		true,
		["Confirmar", "Cancelar"],
		"info",
		null
	);
});

function confirmarOrdenDeReparto() {
	// Aquí hacés tu PostGenHtml o AJAX
	let accion = $("#accion").val(); // "A" o "M"
	let or_compte = $("#or_compte").val(); // Solo para modificación
	let or_obs = $("#OrdenDeReparto_or_obs").val().trim();
	let rp_id = $("#RepartidorSeleccionado").val();
	let pc = obtenerListaPedidosOR();
	let json = {
		abm: accion,
		or_compte: or_compte,
		or_obs: or_obs,
		rp_id: rp_id,
		pc: pc
	};
	$.ajax({
		url: confirmarOrdenDeRepartoUrl,
		type: 'POST',
		contentType: 'application/json; charset=utf-8', // ⚠️ CRUCIAL
		data: JSON.stringify(json), // ⚠️ SERIALIZAR EXPLÍCITAMENTE
		dataType: 'json',
		success: function (response) {
			CerrarWaiting();
			procesarRespuestaConfirmacion(response, accion);
			habilitarTabPedidos();
		},
		error: function (xhr, status, error) {
			CerrarWaiting();
			console.error('❌ Error al confirmar orden de reparto:', error);
			console.error('❌ Response:', xhr.responseText);
			ControlaMensajeError(
				'Error al confirmar orden de reparto: ' +
				(xhr.responseJSON?.mensaje || xhr.statusText || 'Error desconocido')
			);
		}
	});

}

function procesarRespuestaConfirmacion(response, abm) {
	console.log('📥 Respuesta del servidor:', response);

	if (response.error || response.warn) {
		if (response.error) {
			ControlaMensajeError(response.mensaje || 'Error al confirmar la orden de reparto');
			return;
		}
		else //warn
		{
			ControlaMensajeWarning(response.mensaje || 'Atención al confirmar la orden de reparto');
			return;
		}
	}

	// Mensaje de éxito según el tipo de operación
	let mensajeExito = '';
	switch (abm) {
		case 'A':
			mensajeExito = 'Orden de Reparto creada exitosamente';
			break;
		case 'M':
			mensajeExito = 'Orden de Reparto modificada exitosamente';
			break;
		case 'B':
			mensajeExito = 'Orden de Reparto eliminada exitosamente';
			break;
		default:
			mensajeExito = 'Operación completada exitosamente';
	}

	// Mostrar mensaje y redirigir
	AbrirMensaje(
		'CONFIRMACIÓN EXITOSA',
		mensajeExito,
		function () {
			$('#msjModal').modal('hide');

			// Resetear formulario y volver al inicio
			cancelarOperacion();
			//Actualizar tabla de Ordenes de Reparto
			const filtros = buildQueryFilters(pagina);
			const url = buscarOrdenesDeRepartoUrl;
			CargarOrdenesDeReparto(filtros, url);

			if (response.id) {
				console.log('✅ OR ID:', response.id);
			}
		},
		false,
		['Aceptar'],
		'success!',
		null
	);
}

function cancelarOperacion() {
	// Ocultar vista de edición
	document.querySelector("#vistaEditarOR").classList.add("d-none");
	// Mostrar vista de lista
	document.querySelector("#vistaListaOR").classList.remove("d-none");
	// Opcional: limpiar contenido de edición
	document.querySelector("#vistaEditarOR").innerHTML = "";
}

//$(document).on("click", "#btnConsolidar", function () {
//	$("#vistaListaOR").addClass("d-none");
//	$("#vistaConsolidarOR").removeClass("d-none");
//});

//$(document).on("click", "#btnConfirmarReasignacion, #btnCancelarReasignacion", function () {
//	$("#vistaConsolidarOR").addClass("d-none");
//	$("#vistaListaOR").removeClass("d-none");
//});

function obtenerListaPedidosOR() {
	const filas = document.querySelectorAll("#tbPedidosOR tbody tr:not(.fila-vacia)");
	const lista = [];

	filas.forEach(fila => {
		const btn = fila.querySelector(".btnQuitarPedido");
		if (btn) {
			lista.push(btn.dataset.id);
		}
	});

	return lista;
}


document.addEventListener("click", function (e) {

	const btn = e.target.closest(".btnAgregarPedido");
	if (!btn) return;

	bloquearTablas();

	const filaDerecha = btn.closest("tr");

	const pedido = {
		id: btn.dataset.id,
		cliente: btn.dataset.cliente,
		fecha: btn.dataset.fecha,
		vendedor: btn.dataset.vendedor,
		repartidor: btn.dataset.repartidor,
		importe: parseFloat(btn.dataset.importe).toFixed(2)
	};

	const tablaIzquierda = document.querySelector("#tbPedidosOR tbody");
	if (!tablaIzquierda) {
		console.error("No se encontró la tabla izquierda");
		return;
	}

	// 🔥 Verificar si ya existe
	const existe = [...tablaIzquierda.querySelectorAll("tr")]
		.some(tr => tr.querySelector("td")?.textContent.trim() === pedido.id);

	if (existe) {
		desbloquearTablas();
		ControlaMensajeError('El pedido ya fue agregado.');
		return;
	}

	// Crear fila nueva
	const tr = document.createElement("tr");
	tr.classList.add("fade-in-row");

	tr.innerHTML = `
        <td class="text-center">${pedido.id}</td>
        <td>${pedido.cliente}</td>
        <td class="text-center">${formatearFechaDDMMYY(pedido.fecha)}</td>
        <td>${pedido.vendedor}</td>
        <td>${pedido.repartidor}</td>
        <td class="text-end">${pedido.importe}</td>
        <td class="text-center">
            <div class="d-flex justify-content-center gap-1">
                <button class="btn btn-danger btn-table btn-sm btnQuitarPedido"
                        data-id="${pedido.id}">
                    <i class="bx bx-minus"></i>
                </button>

                <button class="btn btn-secondary btn-table btn-sm btnEditarPedido"
                        data-id="${pedido.id}">
                    <i class="bx bx-edit"></i>
                </button>
            </div>
        </td>
    `;

	// Eliminar fila vacía si existe
	const filaVacia = tablaIzquierda.querySelector(".fila-vacia");
	if (filaVacia) filaVacia.remove();

	tablaIzquierda.appendChild(tr);

	// Fade-out en la fila derecha
	filaDerecha.classList.add("fade-out-row");

	setTimeout(() => {
		filaDerecha.remove();
		desbloquearTablas();
	}, 300);
});


document.addEventListener("click", function (e) {

	const btn = e.target.closest(".btnQuitarPedido");
	if (!btn) return;

	bloquearTablas(); // 🔥 Bloqueo inmediato

	const filaIzquierda = btn.closest("tr");

	const pedido = {
		id: btn.dataset.id,
		cliente: btn.dataset.cliente,
		fecha: btn.dataset.fecha,
		vendedor: btn.dataset.vendedor,
		repartidor: btn.dataset.repartidor,
		importe: parseFloat(btn.dataset.importe).toFixed(2)
	};

	const tablaDerecha = document.querySelector("#tbPedidosPendientes tbody");
	const tablaIzquierda = document.querySelector("#tbPedidosOR tbody");

	// Crear fila nueva para la tabla derecha
	const tr = document.createElement("tr");
	tr.classList.add("fade-in-row");
	tr.innerHTML = `
        <td class="text-center">${pedido.id}</td>
        <td>${pedido.cliente}</td>
        <td class="text-center">${formatearFechaDDMMYY(pedido.fecha)}</td>
        <td>${pedido.vendedor}</td>
        <td>${pedido.repartidor}</td>
        <td class="text-end">${pedido.importe}</td>
        <td class="text-center">
            <button class="btn btn-success btn-table btn-sm btnAgregarPedido"
                    data-id="${pedido.id}"
                    data-cliente="${pedido.cliente}"
                    data-fecha="${pedido.fecha}"
                    data-vendedor="${pedido.vendedor}"
                    data-repartidor="${pedido.repartidor}"
                    data-importe="${pedido.importe}">
                <i class="bx bx-plus"></i>
            </button>
        </td>
    `;

	tablaDerecha.appendChild(tr);

	// Fade-out en la izquierda
	filaIzquierda.classList.add("fade-out-row");

	setTimeout(() => {
		filaIzquierda.remove();

		// 🔥 Si la tabla quedó vacía, agregamos la fila "No hay pedidos"
		if (tablaIzquierda.children.length === 0) {
			const filaVacia = document.createElement("tr");
			filaVacia.classList.add("fila-vacia");
			filaVacia.innerHTML = `
                <td colspan="7" class="text-center text-muted py-2">
                    <i class="bx bx-info-circle me-1"></i>No hay pedidos
                </td>
            `;
			tablaIzquierda.appendChild(filaVacia);
		}
		desbloquearTablas();
	}, 300);
});

document.addEventListener("click", function (e) {

	const btn = e.target.closest("#btnCargarPCDelRepartidor");
	if (!btn) return;

	const repartidorSeleccionado = document.querySelector("#RepartidorSeleccionado").value;

	// Validación 1: debe haber repartidor seleccionado
	if (!repartidorSeleccionado || repartidorSeleccionado === "-- Seleccione --") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un repartidor.", function () {
			$("#msjModal").modal("hide");
			$("#RepartidorSeleccionado").trigger("focus");
			return true;
		}, false, ["Aceptar"], "error!", null);
		return;
	}

	const tablaDerecha = document.querySelector("#tbPedidosPendientes tbody");
	const tablaIzquierda = document.querySelector("#tbPedidosOR tbody");

	// Buscar filas cuyo rp_id coincida
	const filas = tablaDerecha.querySelectorAll("tr");
	const filasAMover = [];

	filas.forEach(fila => {
		const btn = fila.querySelector(".btnAgregarPedido");
		if (!btn) return;

		if (btn.dataset.rpId === repartidorSeleccionado) {
			filasAMover.push(fila);
		}
	});

	// Validación 2: no hay pedidos del repartidor
	if (filasAMover.length === 0) {
		AbrirMensaje("ATENCIÓN", "No hay pedidos del repartidor seleccionado.", function () {
			$("#msjModal").modal("hide");
			$("#RepartidorSeleccionado").trigger("focus");
			return true;
		}, false, ["Aceptar"], "error!", null);
		return;
	}

	bloquearTablas();

	// Eliminar fila vacía de la izquierda si existe
	const filaVaciaIzq = tablaIzquierda.querySelector(".fila-vacia");
	if (filaVaciaIzq) filaVaciaIzq.remove();

	// Mover filas
	filasAMover.forEach(fila => {

		const btn = fila.querySelector(".btnAgregarPedido");

		const pedido = {
			id: btn.dataset.id,
			cliente: btn.dataset.cliente,
			fecha: btn.dataset.fecha,
			vendedor: btn.dataset.vendedor,
			repartidor: btn.dataset.repartidor,
			rpId: btn.dataset.rpId,
			importe: parseFloat(btn.dataset.importe).toFixed(2)
		};

		// Crear nueva fila en la izquierda
		const tr = document.createElement("tr");
		tr.classList.add("fade-in-row");
		tr.innerHTML = `
            <td class="text-center">${pedido.id}</td>
            <td>${pedido.cliente}</td>
            <td class="text-center">${formatearFechaDDMMYY(pedido.fecha)}</td>
            <td>${pedido.vendedor}</td>
            <td>${pedido.repartidor}</td>
            <td class="text-end">${pedido.importe}</td>
            <td class="text-center">
                <button class="btn btn-danger btn-table btn-sm btnQuitarPedido"
                        data-id="${pedido.id}"
                        data-cliente="${pedido.cliente}"
                        data-fecha="${pedido.fecha}"
                        data-vendedor="${pedido.vendedor}"
                        data-repartidor="${pedido.repartidor}"
                        data-rp-id="${pedido.rpId}"
                        data-importe="${pedido.importe}">
                    <i class="bx bx-minus"></i>
                </button>
            </td>
        `;

		tablaIzquierda.appendChild(tr);

		// Fade-out y eliminación de la fila derecha
		fila.classList.add("fade-out-row");
		setTimeout(() => {
			fila.remove();

			// 🔥 Si la tabla derecha quedó vacía, agregar fila "No hay pedidos"
			if (tablaDerecha.children.length === 0) {
				const filaVacia = document.createElement("tr");
				filaVacia.classList.add("fila-vacia");
				filaVacia.innerHTML = `
					<td colspan="7" class="text-center text-muted py-2">
						<i class="bx bx-info-circle me-1"></i>No hay pedidos
					</td>
				`;
				tablaDerecha.appendChild(filaVacia);
			}

		}, 300);

	});

	setTimeout(() => {
		desbloquearTablas();
	}, 350);
});

function CerrarTabEdicionPedido() {

	// 1) Ocultar el tab de edición
	const tabEditar = document.querySelector("#tabEditarPedido");
	tabEditar.classList.add("d-none");

	// 2) Activar el primer tab (Ordenes de Reparto)
	const tabButton = document.querySelector("#btnTabOrdenesDeReparto");
	const bsTab = new bootstrap.Tab(tabButton);
	bsTab.show();

	// 3) Rehabilitar los otros tabs
	document.querySelector("#tabOrdenesDeReparto").classList.remove("tab-disabled");
	document.querySelector("#tabPedidosDeCliente").classList.remove("tab-disabled");

	// 4) Limpiar contenido del tab de edición (opcional)
	document.querySelector("#divEditarPedido").innerHTML = "";

	ActualizarListaPedidosDeLaOrdenDeReparto();
}

function ActualizarListaPedidosDeLaOrdenDeReparto() {
	var data = { orCompte: orCompteSeleccionado }
	PostGenHtml(data, actualizarListaPedidosDeLaOrdenDeReparto, function (html) {
		$("#divListaPedidosDeLaOrden").html(html);
		activarSeleccionDeFilas("#tbPedidosOR tbody");
	});
}

document.addEventListener("click", function (e) {

	const btn = e.target.closest(".btnEditarPedido");
	if (!btn) return;

	//alert("Servicio no implementado...");
	//return;

	const pedidoId = btn.dataset.id;
	const pedidoEstadoId = btn.dataset.eid;

	// 1) Mostrar el TAB
	const tabLi = document.querySelector("#tabEditarPedido");
	tabLi.classList.remove("d-none");

	// 2) Activar el TAB con Bootstrap
	const tabButton = document.querySelector("#btnTabEditarPedido");
	const bsTab = new bootstrap.Tab(tabButton);
	bsTab.show();

	// 3) Si querés evitar que el usuario cambie de tab mientras edita (Opcional)
	document.querySelector("#tabOrdenesDeReparto").classList.add("tab-disabled");
	document.querySelector("#tabPedidosDeCliente").classList.add("tab-disabled");

	// 3.A) Y para habilitarlos de nuevo
	//document.querySelector("#tabOrdenesDeReparto").classList.remove("tab-disabled");
	//document.querySelector("#tabPedidosDeCliente").classList.remove("tab-disabled");

	// 4) Cargar contenido (AJAX o lo que necesites)
	CargarFormularioEdicionDePedidoDeCliente(pedidoId, pedidoEstadoId);
});

function CargarFormularioEdicionDePedidoDeCliente(pedidoId, pedidoEstadoId) {
	///TODO : Editar esta parte para abrir la edicion de Pedido de Cliente
	AbrirWaiting("Cargar datos del pedido...");
	const url = cargarVistaEdicionPedidoDeClienteURL;
	PostGenHtml({ pcCompte: pedidoId, orCompte: orCompteSeleccionado }, url, function (html) {
		$("#divEditarPedido").html(html);

		HabilitarCamposFormularioPedido(true, pedidoEstadoId);
		$('#btnAgregarCProducto').prop('disabled', false);
		AplicarReadonlyCamposPedido();
		ConfigurarEventosEnEdicionDePedidoDeCliente();

		setTimeout(() => {
			ActualizarTotalGeneralPedido();
			// Agregar inicialización del drag & drop aquí
			InicializarDragAndDropProductos();
			FinalizarInicializacion();
			ConfigurarEventosEliminacionProducto();
		}, 100);

		const $primer = $('#divPedidoDatos').find('input:not([type=hidden]):not([readonly]), textarea:not([readonly]), select:not([disabled])').filter(':visible').first();
		if ($primer.length) {
			setTimeout(() => $primer.trigger("focus"), 50);
		}

		CerrarWaiting();
	});
}

function estaEnModoEdicionPedido() {
	return false;
}

function InicializarDragAndDropProductos() {
	// Solo inicializar si hay filas y estamos en modo edición
	//if (!estaEnModoEdicionPedido()) {
	//	console.log('❌ Drag & Drop no inicializado - No está en modo edición');
	//	return;
	//}

	console.log('🔄 Inicializando Drag & Drop...');

	const $tbody = $('#tbGridPedidoProds tbody');

	// Destruir instancia previa si existe
	if ($tbody.hasClass('ui-sortable')) {
		$tbody.sortable('destroy');
	}

	// Usar Sortable de jQuery UI que ya está incluido en el proyecto
	$tbody.sortable({
		handle: 'td:first', // Usar primera columna como handle
		helper: function (e, ui) {
			// Mantener ancho de columnas durante el drag
			ui.children().each(function () {
				$(this).width($(this).width());
			});
			return ui;
		},
		axis: 'y',
		cursor: 'move',
		opacity: 0.7,
		stop: function (event, ui) {
			console.log('🔄 Reordenando filas...');
			// Reordenar items y actualizar numeración
			reordenarFilasPedidoProds();

			// Recalcular totales por si acaso
			setTimeout(() => {
				ActualizarTotalGeneralPedido();
				//calcularUtilidadMargen();
			}, 50);
		}
	}).disableSelection();

	// Agregar indicador visual mejorado
	$tbody.find('tr').each(function () {
		const $firstCell = $(this).find('td:first');
		if ($firstCell.length && !$firstCell.hasClass('drag-handle')) {
			$firstCell
				.addClass('drag-handle')
				.css({
					'cursor': 'move',
					'position': 'relative'
				})
				.append('<i class="bx bx-move-vertical position-absolute" style="right: 5px; top: 50%; transform: translateY(-50%);"></i>');
		}
	});

	console.log('✅ Drag & Drop inicializado');
}

function reordenarFilasPedidoProds() {
	console.log('🔄 Iniciando reordenamiento de filas');

	const $tbody = $('#tbGridPedidoProds tbody');
	let contador = 1;

	$tbody.find('tr').each(function () {
		const $fila = $(this);

		// Ignorar filas de mensaje
		if ($fila.find('td[colspan]').length > 0) {
			console.log('⏭️ Saltando fila de mensaje');
			return;
		}

		// Actualizar número de ítem
		$fila.attr('data-pre-item', contador);
		$fila.find('td:first').text(contador);

		// Actualizar clases alternadas
		$fila.removeClass('alt');
		if (contador % 2 === 0) {
			$fila.addClass('alt');
		}

		contador++;
	});

	console.log(`✅ Reordenamiento completado - ${contador - 1} filas procesadas`);
}

function ActualizarTotalGeneralPedido() {
	let totalGeneral = 0;

	$('#tbGridPedidoProds tbody tr').each(function () {
		const $fila = $(this);
		if ($fila.find('td[colspan]').length > 0) return;

		const total = parseFloat($fila.find('.input-pcd_pvta_total').text().replace(/,/g, '')) || 0;
		totalGeneral += total;
	});

	// 🔥 Formato con miles y decimales
	const totalFormateado = totalGeneral.toLocaleString("en-US", {
		minimumFractionDigits: 2,
		maximumFractionDigits: 2
	});

	$('#tbGridPedidoProds tfoot .fw-bold:last').text(totalFormateado);
}

function AplicarReadonlyCamposPedido() {
	const campos = $('.input-pcd_pedida');
	const tooltipMsg = 'Active el modo edición (Editar) para modificar este campo';

	requestAnimationFrame(() => {
		if (!estaEnModoEdicionPedido()) {
			// Modo NO edición - Deshabilitar todos los campos
			//campos.each(function () {
			//	const $c = $(this);
			//	$c.prop('readonly', true)
			//		.addClass('campo-readonly');
			//	if (!$c.attr('title')) {
			//		$c.attr('title', tooltipMsg);
			//	}
			//});

			// Ocultar botones de eliminación
			//$('.btn-eliminar-producto').hide();

		} else {
			const $filas = $('#tbGridPedidoProds tbody tr');
			if (modoNuevoPedido) {
				return;
			}
		}
	});
}

function ConfigurarEventosEnEdicionDePedidoDeCliente() {
	$(document).off("click", "#btnConfirmarEdicionPC");
	$(document).on("click", "#btnConfirmarEdicionPC", function () {
		let abm = 'M';
		const validacion = validarPedido(abm);

		if (validacion == null || validacion == undefined)
			return;
		if (!validacion.esValido) {
			//ControlaMensajeWarning(validacion.mensaje);
			AbrirMensaje("ATENCIÓN", validacion.mensaje, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
			return;
		}
		else {
			AbrirMensaje(
				'CONFIRMAR PEDIDO',
				'¿Desea confirmar las modificaciones del pedido?',
				function (resp) {
					if (resp === 'SI') {
						confirmarPedido(abm);
					}
					else {
						$("#msjModal").modal("hide");
					}
				},
				true,
				['Confirmar', 'Cancelar'],
				'info!',
				null
			);
		}
	});

	$(document).off("click", "#btnCancelarEdicionPC");
	$(document).on("click", "#btnCancelarEdicionPC", function () {
		AbrirMensaje(
			'CONFIRMAR CANCELACIÓN',
			"¿Desea cancelar la edición del pedido?",
			function (resp) {
				if (resp === 'SI') {
					CerrarTabEdicionPedido();
				}
				$('#msjModal').modal('hide');
			},
			true,
			['Confirmar', 'Cancelar'],
			'info!',
			null
		);
	});

	// Evento delegado para el botón de agregar producto
	$(document).on("click", "#btnAgregarCProducto", function () {
		if ($("#busquedaModal").length === 0) {
			cargarModalBusquedaAvanzada(function () {
				if (typeof configurarDestinoBusquedaProductos === 'function') {
					configurarDestinoBusquedaProductos("pedidos", "003", agregarProductosAlGrid, obtenerProductosExistentesIds);
				}
				$("#busquedaModal").modal("show");
			});
		} else {
			if (typeof configurarDestinoBusquedaProductos === 'function') {
				configurarDestinoBusquedaProductos("pedidos", "003", agregarProductosAlGrid, obtenerProductosExistentesIds);
			}
			$("#busquedaModal").modal("show");
		}
	});
	deshabilitarTabPedidos();
}

function obtenerProductosExistentesIds() {
	const productosIds = [];

	$('#tbGridPedidoProds tbody tr').each(function () {
		const $fila = $(this);
		if ($fila.find('td[colspan]').length > 0) return;

		const pId = $fila.data('p-id');
		if (pId) {
			productosIds.push(pId);
		}
	});

	return productosIds;
}

function agregarProductosAlGrid(productos) {
	if (!Array.isArray(productos) || productos.length === 0) return;

	const $tbody = $('#tbGridPedidoProds tbody');

	const $filaVacia = $tbody.find('tr td[colspan]');
	if ($filaVacia.length > 0) {
		$filaVacia.closest('tr').remove();
	}

	let $tfoot = $('#tbGridPedidoProds tfoot');
	if ($tfoot.length === 0) {
		$('#tbGridPedidoProds').append(`
            <tfoot class="table-golden-footer">
                <tr>
                    <td colspan="7" class="text-end fw-bold">Total General:</td>
                    <td class="text-end fw-bold">0.00</td>
                </tr>
            </tfoot>
        `);
		$tfoot = $('#tbGridPedidoProds tfoot');
	}

	let esAlternado = $tbody.find('tr').length % 2 !== 0;
	let ultimoItem = obtenerUltimoPcdItem();

	productos.forEach(function (producto, index) {
		ultimoItem++; // autoincremental real
		const fila = crearFilaProductoPedido(producto, esAlternado, ultimoItem);
		$tbody.append(fila);
		esAlternado = !esAlternado;
	});

	//aplicarInputMaskPresupuesto();
	AplicarReadonlyCamposPedido();
	ActualizarTotalGeneralPedido();
	ConfigurarEventosEliminacionProducto();
	setTimeout(() => {
		FinalizarInicializacion();
		//calcularUtilidadMargen();
		// Reinicializar drag & drop con las nuevas filas
		InicializarDragAndDropProductos();
	}, 100);
}

function obtenerUltimoPcdItem() {
	const $filas = $('#tbGridPedidoProds tbody tr');

	if ($filas.length === 0) return 0;

	// La columna # está en la primera celda (td:eq(0))
	let ultimoValor = 0;

	$filas.each(function () {
		const valor = parseInt($(this).find("td").eq(0).text().trim(), 10);
		if (!isNaN(valor) && valor > ultimoValor) {
			ultimoValor = valor;
		}
	});

	return ultimoValor;
}

function FinalizarInicializacion() {
	setTimeout(function () {
		ConfiguracionInputMaskOptimizadaPedido();
	}, 10);
}

function ConfiguracionInputMaskOptimizadaPedido() {
	console.log("Aplicando configuración InputMask optimizada...");
	console.log('ConfiguracionInputMaskOptimizadaPedido', ConfiguracionInputMaskOptimizadaPedido);
	// Establecer todos los campos como readonly de una sola vez
	$('.input-pcd_pedida')
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

	// Aplicar máscaras de forma eficiente con selección optimizada
	//Inputmask(maskConfig1Decimal).mask('.input-tp_dto1, .input-tp_dto2, .input-tp_dto3, .input-tp_dto4, .input-tp_dto_pa, .input-tp_porc_flete');
	Inputmask(maskConfig2Decimales).mask('.input-pcd_pedida');
	//Inputmask(maskConfigBoni).mask('.input-tp_boni');

	// Configurar eventos de edición
	configurarEventosEdicionOptimizado();

	console.log("Configuración InputMask aplicada");
}

// ✅ SIMPLIFICADO: Eventos de edición más eficientes
function configurarEventosEdicionOptimizado() {
	const camposEditables = '.input-pcd_pedida';
	const camposSecuencia01 = '.input-pcd_pedida';

	// Limpiar eventos previos
	$(document).off('click.camposEditables keydown.camposEditables blur.camposSecuencia01');

	// Evento click unificado
	$(document).on('click.camposEditables', camposEditables, function (e) {
		e.stopPropagation();

		const $this = $(this);
		const pIdDetalle = $this.closest('tr').data('p-id');

		//// Cambio de producto si es necesario
		//if (pIdDetalle !== productoActualEnLista) {
		//    productoActualEnLista = pIdDetalle;
		//    $("#divProdLista").attr('data-producto-actual', pIdDetalle);
		//    destacarFilaSeleccionada(pIdDetalle);
		//    buscarProductoListaOptimizado(pIdDetalle);
		//}

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

			marcarCampoModificadoPedido(this);
			actualizarEstadoCarga(row);
			activarSiguienteCampo(this);

			// Aplicar cálculos según tipo
			if (esSecuencia01) calcularTotalAPIDebounced(row);
			//else if (esMargen) calcularPrecioVentaAPIDebounced(row);
			//else if (esPrecioVenta) calcularPrecioVentaMargenAPIDebounced(row);
		}
	});

	// Eventos blur simplificados con delegación
	const eventosBlur = {
		[camposSecuencia01]: () => calcularTotalAPIDebounced
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
const calcularTotalAPIDebounced = debounce(function (row) {
	calcularProductoCompleto(row);
}, 300);

// ✅ UNIFICADA: Función principal que detecta contexto y aplica la lógica correcta
function calcularProductoCompleto(row, callback = null) {
	const productId = row.data('p-id');

	console.log(`🔄 Cálculo MASIVO para producto ${productId}`);
	calcularProductoCompletoSincrono(row);
	ActualizarTotalGeneralPedido();
}

// ✅ CORREGIDA: Versión síncrona con resguardo completo de producto y listas
function calcularProductoCompletoSincrono(row) {
	const productId = row.data('p-id');

	// ✅ EVITAR: Cálculos duplicados
	if (row.data('processing') === true) {
		console.log(`⏭️ Producto ${productId} ya en procesamiento`);
		return { success: false, skip: true };
	}

	row.data('processing', true);

	try {
		console.log(`🔄 Calculando COMPLETO SÍNCRONO para producto ${productId}`);

		// ✅ PASO 1: Calcular precio de Venta Total
		const resultadoPrecioVenta = calcularPrecioDeVentaSincronoRapido(row);
		if (!resultadoPrecioVenta) {
			console.error(`❌ Error en cálculo de precio de venta para producto ${productId}`);
			row.data('processing', false);
			return { success: false, error: "Error en cálculo de precio de venta" };
		}

		console.log(`✅ Secuencia completa finalizada para producto ${productId} `);

		return {
			success: true,
			precio: resultadoPrecioVenta
		};

	} catch (error) {
		console.error(`💥 Error general en cálculo síncrono ${productId}:`, error);
		return { success: false, error: error.message };
	} finally {
		row.data('processing', false);
	}
}

// ✅ MEJORADA: Función cálculo de costo con mejor retorno de información
function calcularPrecioDeVentaSincronoRapido(row) {
	const productId = row.data('p-id');

	console.log(`💰 Calculando precio de venta total para producto ${productId}`);

	const $pvta = row.find('.input-pcd_pvta').text().trim();
	const pcdPVta = parseFloat($pvta.replace(/,/g, '')) || 0;
	// Recopilar datos
	const pcdPedidaRaw = row.find('.input-pcd_pedida').val();
	const pcd_pedida = parseFloat((pcdPedidaRaw || '').toString().replace(/,/g, '')) || 0;

	const datos = {
		p_id: productId,
		pcd_pedida: pcd_pedida,
		pcd_pvta: pcdPVta || 0
	};

	try {
		// ✅ ACTUALIZAR: Campo de costo sin efectos visuales
		const nuevoPrecioNum = pcd_pedida * pcdPVta;
		const nuevoPrecioDeVenta = nuevoPrecioNum; // número sin formato

		const campoTotal = row.find('.input-pcd_pvta_total');
		if (!campoTotal || campoTotal.length === 0) {
			console.warn('No se encontró elemento .input-pcd_pvta_total en la fila', productId);
			return false;
		}

		// Determinar tipo de precio si está disponible en la fila (data-tipo-precio) o usar 'Venta'
		const tipoPrecio = (row.data('tipo-precio') || 'Venta').toString();

		// Formatear con la misma lógica que GridHelper.FormatearPrecio
		const formatted = formatPrecio(nuevoPrecioDeVenta, tipoPrecio);

		// Si es un input usamos val(), si es un td usamos text()
		if (campoTotal.is('input, textarea, :input')) {
			campoTotal.val(formatted);
		} else {
			campoTotal.text(formatted);
		}

		// Actualizar total general de la tabla
		if (typeof actualizarTotalGeneralPedido === 'function') {
			actualizarTotalGeneralPedido();
		}

		console.log(`✅ Precio calculado rápidamente: ${nuevoPrecioDeVenta}`);

		// ✅ RETORNAR: Información del cálculo
		return {
			success: true,
			precio: nuevoPrecioDeVenta,
			datos: datos
		};

	} catch (error) {
		console.error(`💥 Error calculando precio rápido para ${productId}:`, error.message);
		return false;
	}
}

// Helper: formatea número igual que GridHelper.FormatearPrecio (separador decimal ".", miles con ",")
function formatPrecio(valor, tipoPrecio = 'Venta') {
	if (valor == null || isNaN(Number(valor))) return '';

	const decimales = (tipoPrecio === 'Lista' || tipoPrecio === 'Costo' || tipoPrecio === 'Neto') ? 3 : 2;
	// Usamos 'en-US' para obtener separador decimal "." y miles con ","
	const nf = new Intl.NumberFormat('en-US', { minimumFractionDigits: decimales, maximumFractionDigits: decimales });
	return nf.format(Number(valor));
}

function activarSiguienteCampo(campoActual) {
	const $campoActual = $(campoActual);
	const $fila = $campoActual.closest('tr');
	const camposEditables = '.input-pcd_pedida';
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

function marcarCampoModificadoPedido($campo) {
	if (!$campo || !$campo.length) return;
	$campo.addClass('campo-modificado');
	setTimeout(() => $campo.removeClass('campo-modificado'), 1500);
}

/**
* ✅ NUEVO: Configura eventos de eliminación de productos
* Usa delegación de eventos para botones dinámicos
*/
function ConfigurarEventosEliminacionProducto() {
	// ✅ REMOVER LISTENER PREVIO para evitar duplicados
	$(document).off('click', '.btn-eliminar-producto');

	// ✅ DELEGACIÓN DE EVENTOS (más performante para elementos dinámicos)
	$(document).on('click', '.btn-eliminar-producto', function (e) {
		e.preventDefault();
		e.stopPropagation();

		const $btn = $(this);
		const pId = $btn.data('p-id');
		const $fila = $btn.closest('tr');
		const pDesc = $fila.find('td:nth-child(2)').text().trim();

		confirmarEliminacionProducto(pId, pDesc, $fila);
	});
}

/**
* ✅ NUEVO: Confirma y ejecuta eliminación de producto del grid
* @param {string} pId - ID del producto
* @param {string} pDesc - Descripción del producto
* @param {jQuery} $fila - Fila a eliminar
*/
function confirmarEliminacionProducto(pId, pDesc, $fila) {
	AbrirMensaje(
		'ELIMINAR PRODUCTO',
		`¿Está seguro que desea eliminar el producto "${pDesc}" del pedido?`,
		function (resp) {
			if (resp === 'SI') {
				eliminarProductoDelGrid($fila);
			}
			$('#msjModal').modal('hide');
		},
		true,
		['Eliminar', 'Cancelar'],
		'warn!',
		null
	);
}

/**
 * ✅ NUEVO: Elimina producto del grid y actualiza totales
 * @param {jQuery} $fila - Fila a eliminar
 */
function eliminarProductoDelGrid($fila) {
	const pDesc = $fila.find('td:nth-child(2)').text().trim();

	// ✅ ANIMACIÓN SUAVE (mejor UX)
	$fila.fadeOut(300, function () {
		$(this).remove();

		// ✅ VERIFICAR SI QUEDARON PRODUCTOS
		const $tbody = $('#tbGridPedidoProds tbody');
		if ($tbody.find('tr[data-p-id]').length === 0) {
			$tbody.html(`
                <tr>
                    <td colspan="9" class="text-center text-muted py-2">
                        <i class="bx bx-info-circle me-1"></i>No hay productos en este pedido
                    </td>
                </tr>
            `);

			// ✅ REMOVER FOOTER si no hay productos
			$('#tbGridPedidoProds tfoot').remove();
		} else {
			// ✅ REAJUSTAR CLASES ALTERNADAS
			ReajustarClasesAlternadas();
		}

		// ✅ ACTUALIZAR TOTAL
		ActualizarTotalGeneralPedido();

		ControlaMensajeSuccess(`Producto "${pDesc}" eliminado correctamente`);
	});
}

/**
* ✅ NUEVO: Reajusta clases 'alt' después de eliminar filas
* Mantiene consistencia visual
*/
function ReajustarClasesAlternadas() {
	$('#tbGridPedidoProds tbody tr[data-p-id]').each(function (index) {
		const $fila = $(this);

		if (index % 2 === 0) {
			$fila.removeClass('alt');
		} else {
			$fila.addClass('alt');
		}
	});
}

function crearSelectReemplazo(p_id_actual, p_id_remplazo) {
	let html = `<select class="form-select form-select-sm input-pcd_reemplazo">
                    <option value="">-- Seleccionar --</option>`;

	window.productosReemplazables.forEach(prod => {
		if (prod.p_id !== p_id_actual) {
			const selected = (prod.p_id === p_id_remplazo) ? "selected" : "";
			html += `<option value="${prod.p_id}" ${selected}>${prod.p_id} - ${prod.p_desc}</option>`;
		}
	});

	html += `</select>`;
	return html;
}

/**
 * ✅ OPTIMIZADO: Crea HTML de fila de producto con TODOS los nuevos campos
 * Unifica lógica de cálculo y evita duplicación de código
 * @param {object} producto - ProductoListaDto
 * @param {boolean} esAlternado - Alternar clase CSS
 * @returns {string} HTML de la fila
 */
function crearFilaProductoPedido(producto, esAlternado, pcd_item) {
	// ✅ VALIDACIÓN Y NORMALIZACIÓN DE DATOS
	const datosProducto = normalizarDatosProducto(producto);

	// ✅ FORMATEO
	const claseAlt = esAlternado ? 'alt' : '';

	const selectReemplazoHTML = datosProducto.pcd_origen_bool
		? crearSelectReemplazo(datosProducto.p_id, datosProducto.p_id_remplazo)
		: '<span class="text-muted">—</span>';

	// ✅ CONSTRUCCIÓN HTML CON TEMPLATE LITERALS (más legible y performante)
	return `
        <tr class="${claseAlt}"
            data-pcd-item="${pcd_item}"
            data-p-id="${datosProducto.p_id}">

            <td class="text-center">${pcd_item}</td>
            <td class="text-center">${datosProducto.p_id}</td>
            <td class="input-p_desc">${escaparHTML(datosProducto.p_desc)}</td>

            <td class="text-end">
                <div class="input-container">
                    <input type="text"
                           class="form-control form-control-sm input-pcd_pedida input-numeric"
                           value="${datosProducto.pcd_pedida}"
                           data-original-value="${datosProducto.pcd_pedida}"
                           title="Doble click para editar" />
                </div>
            </td>
            <td class="text-end input-pcd_enviada">${datosProducto.pcd_enviada.toFixed(0)}</td>
            <td class="text-end input-pcd_pvta">${datosProducto.p_pvta.toFixed(2)}</td>
            <td class="text-end input-pcd_pvta_total">${(datosProducto.p_pvta * datosProducto.pcd_pedida).toFixed(2)}</td>

            <td class="text-center align-middle">
                <input type="checkbox"
                       class="form-check-input m-0 p-0 input-pcd_origen_bool"
                       disabled
                       ${datosProducto.pcd_origen_bool ? "checked" : ""} />
            </td>

            <td class="text-center">
                ${selectReemplazoHTML}
            </td>

            <td class="text-center">
                <button type="button"
                        class="btn btn-sm btn-danger btn-eliminar-producto"
                        data-p-id="${datosProducto.p_id}"
                        title="Eliminar producto">
                    <i class="bx bx-trash"></i>
                </button>
            </td>
        </tr>
    `;
}

/**
 * ✅ NUEVO: Escapa HTML para prevenir XSS
 * @param {string} texto - Texto a escapar
 * @returns {string} Texto escapado
 */
function escaparHTML(texto) {
	const div = document.createElement('div');
	div.textContent = texto;
	return div.innerHTML;
}

/**
 * ✅ NUEVO: Normaliza y valida datos del ProductoListaDto
 * Centraliza validación y conversión de tipos
 * @param {object} producto - ProductoListaDto
 * @returns {object} Datos normalizados y validados
 */
function normalizarDatosProducto(producto) {
	// ✅ HELPER: Parsear decimal con fallback seguro
	const parseDecimalSeguro = (valor, defecto = 0) => {
		const num = parseFloat(valor);
		return isNaN(num) ? defecto : num;
	};

	return {
		// Identificadores
		p_id: String(producto.p_id || producto.P_id || '').trim(),
		p_desc: String(producto.p_desc || producto.P_desc || 'Sin descripción').trim(),
		// Precios y costos
		p_pcosto: parseDecimalSeguro(producto.p_pcosto || producto.P_pcosto, 0),
		p_pvta: parseDecimalSeguro(producto.p_pvta || producto.P_pvta, 0),
		p_pneto: parseDecimalSeguro(producto.p_pneto, 0), // ✅ NUEVO CAMPO

		// Márgenes
		p_margen: parseDecimalSeguro(producto.p_margen, 0), // ✅ USA p_margen DEL DTO
		//margenActual: parseDecimalSeguro(producto.p_margen, 0), // ✅ NUEVO CAMPO

		// Cantidad (siempre 1 para nuevos productos)
		cantidad: 1,
		pcd_pedida: 1,
		pcd_enviada: 0,
		pcd_origen_bool: true,
		// Impuestos
		//ivaSituacion: String(producto.iva_situacion || 'E').trim(),
		iva_situacion: producto.iva_situacion,
		iva_alicuota: parseDecimalSeguro(producto.iva_alicuota, 21),
		in_alicuota: parseDecimalSeguro(producto.in_alicuota, 0),

		// ✅ NUEVOS CAMPOS: Previsiones
		lp_prevision_tot: parseDecimalSeguro(producto.lp_prevision_tot, 0),
		lp_prevision_pin: parseDecimalSeguro(producto.lp_prevision_pin, 0),
	};
}

/**
 * ✅ Confirma el pedido enviándolo al servidor
 * @param {string} abm - Tipo de operación: 'A', 'M', 'B'
 */
function confirmarPedido(abm) {
	console.log(`📤 Confirmando pedido (Modo: ${abm})...`);

	try {
		// Construir objeto de confirmación
		const confirmacionDto = construirPedidoConfirmaReqDto(abm);

		// 🔥 Si hubo error en la construcción del DTO, detener todo
		if (!confirmacionDto) {
			return;
		}

		AbrirWaiting('Confirmando pedido...');
		// Debug: Ver estructura completa
		console.log('📦 DTO de confirmación:', confirmacionDto);

		$.ajax({
			url: confirmarPedidoUrl,
			type: 'POST',
			contentType: 'application/json; charset=utf-8', // ⚠️ CRUCIAL
			data: JSON.stringify(confirmacionDto), // ⚠️ SERIALIZAR EXPLÍCITAMENTE
			dataType: 'json',
			success: function (response) {
				CerrarWaiting();
				if (response.error === true || response.warn === true) {
					console.error('❌ Response:', response.msg);
					AbrirMensaje("ATENCIÓN", 'Error al intentar confirmar el pedido: ' + (response.msg || 'Error desconocido'), function () {
						$("#msjModal").modal("hide");
						return true;
					}, false, ["Aceptar"], "error!", null);
				}
				else {
					ProcesarRespuestaConfirmacionDePedido(response, abm);
					//if (abm == 'A' || abm == 'M')
					//	ImprimirPedido_Generado(response.id);
				}
			},
			error: function (xhr, status, error) {
				CerrarWaiting();
				console.error('❌ Error al confirmar pedido:', error);
				console.error('❌ Response:', xhr.responseText);
				ControlaMensajeError(
					'Error al confirmar el pedido: ' +
					(xhr.responseJSON?.mensaje || xhr.statusText || 'Error desconocido')
				);
			}
		});

	} catch (error) {
		CerrarWaiting();
		console.error('❌ Error al construir DTO:', error);
		ControlaMensajeError('Error al procesar los datos del pedido: ' + error.message);
	}
}

function ProcesarRespuestaConfirmacionDePedido(response, abm) {
	console.log('📥 Respuesta del servidor:', response);

	if (response.error || response.warn) {
		if (response.error) {
			AbrirMensaje("ATENCIÓN", response.mensaje || 'Error al confirmar el pedido', function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
			return;
		}
		else //warn
		{
			AbrirMensaje("ATENCIÓN", response.mensaje || 'Atención al confirmar el pedido', function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
			return;
		}
	}

	// Mensaje de éxito según el tipo de operación
	let mensajeExito = '';
	switch (abm) {
		case 'A':
			mensajeExito = 'Pedido creado exitosamente';
			break;
		case 'M':
			mensajeExito = 'Pedido modificado exitosamente';
			break;
		case 'B':
			mensajeExito = 'Pedido eliminado exitosamente';
			break;
		default:
			mensajeExito = 'Operación completada exitosamente';
	}

	// Mostrar mensaje y redirigir
	AbrirMensaje(
		'CONFIRMACIÓN EXITOSA',
		mensajeExito,
		function () {
			$('#msjModal').modal('hide');

			CerrarTabEdicionPedido();

			// Si hay ID de pedido en la respuesta, imprimir el pedido
			if (response.id) {
				// Opcional: Recargar el pedido recién creado/modificado
				console.log('✅ Pedido ID:', response.id);
			}
		},
		false,
		['Aceptar'],
		'success!',
		null
	);
}

/**
 * ✅ Construye el DTO PedidoConfirmaReqDto
 * @param {string} abm - Tipo de operación
 * @returns {object} PedidoConfirmaReqDto
 */
function construirPedidoConfirmaReqDto(abm) {
	const productos = obtenerProductosDelGrid();

	if (!productos) return null; // 🔥 Evita continuar si hubo error

	return {
		Abm: abm,
		Datos: obtenerDatosFormularioPedido(),
		Productos: productos
	};
}

function obtenerProductosDelGrid() {
	const productos = [];
	const $filas = $('#tbGridPedidoProds tbody tr');
	let cont = 0;

	let errorReemplazo = null; // 🔥 Para capturar el primer error

	$filas.each(function () {
		const $fila = $(this);
		cont++;

		if ($fila.find('td[colspan]').length > 0) return;

		const pId = $fila.data('p-id');
		if (!pId) return;

		const pDes = $fila.find('.input-p_desc').text().trim() || "";
		const pcdCantidad = parseFloat($fila.find('.input-pcd_pedida').val().replace(/,/g, '')) || 0;
		const pcdEnviada = parseFloat($fila.find('.input-pcd_enviada').text().replace(/,/g, '')) || 0;
		const pcdPVta = parseFloat($fila.find('.input-pcd_pvta').text().replace(/,/g, '')) || 0;

		const pcdOrigenBool = $fila.find('.input-pcd_origen_bool').prop('checked');
		const pcdOrigen = pcdOrigenBool ? 'S' : 'N';

		const $selectReemplazo = $fila.find('.input-pcd_reemplazo');
		const remplazoId = $selectReemplazo.val() || "";
		const remplazoDesc = $selectReemplazo.find("option:selected").text().trim();

		// 🔥 VALIDACIÓN: si es origen y no eligió reemplazo → ERROR
		if (pcdOrigenBool && remplazoId === "") {
			errorReemplazo = `Debe seleccionar un reemplazo para el producto ${pId} - ${pDes}`;
			return false; // cortar el each
		}

		productos.push({
			p_id: pId,
			p_desc: pDes,
			pcd_item: cont,
			pcd_pedida: pcdCantidad,
			pcd_enviada: pcdEnviada,
			lp_id: '003',
			pcd_pvta: pcdPVta,
			pcd_origen: pcdOrigen,
			pcd_oferta: 'N',
			p_id_remplazo: remplazoId,
			ve_comi_base: 0,
			ve_comi_porc: 0,
			rp_comi_base: 0,
			rp_comi_porc: 0,
		});
	});

	// 🔥 Si hubo error → mostrar aviso y devolver null
	if (errorReemplazo) {
		AbrirMensaje("VALIDACIÓN", errorReemplazo, function () {
			$('#msjModal').modal('hide');
		}, false, ["Aceptar"], "error!", null);

		return null;
	}

	return productos;
}

//function obtenerProductosDelGrid() {
//	const productos = [];
//	const $filas = $('#tbGridPedidoProds tbody tr');
//	let cont = 0;
//	$filas.each(function () {
//		const $fila = $(this);
//		cont++;
//		// ✅ OPTIMIZACIÓN: Saltar filas vacías o de mensaje en una sola verificación
//		if ($fila.find('td[colspan]').length > 0) return;

//		// ✅ OPTIMIZACIÓN: Extraer datos del DOM usando data attributes (más eficiente)
//		const pId = $fila.data('p-id');
//		if (!pId) return; // Si no hay ID, saltar esta fila

//		// ✅ OPTIMIZACIÓN: Parsear valores numéricos una sola vez
//		const pDes = $fila.find('.input-p_desc').text().trim() || "";
//		const $inputCantidad = $fila.find('.input-pcd_pedida');
//		const pcdCantidad = parseFloat($inputCantidad.val().replace(/,/g, '')) || 0;

//		const $inputEnviada = $fila.find('.input-pcd_enviada');
//		const pcdEnviada = parseFloat($inputEnviada.val().replace(/,/g, '')) || 0;

//		const $pvta = $fila.find('.input-pcd_pvta').text().trim();
//		const pcdPVta = parseFloat($pvta.replace(/,/g, '')) || 0;

//		const $origenBool = $fila.find('.input-pcd_origen_bool').prop('checked');
//		const pcdOrigen = $origenBool ? 'S' : 'N';

//		const remplazoId = $fila.find('.input-pcd_reemplazo').val() || "";
//		const remplazoDesc = $fila.find('.input-pcd_reemplazo option:selected').text().trim();

//		// ✅ Construir objeto Dto (coincide exactamente con el DTO de C#)
//		productos.push({
//			// Propiedades de productos
//			p_id: pId,
//			p_desc: pDes,
//			pcd_item: cont,
//			pcd_pedida: pcdCantidad,
//			pcd_enviada: pcdEnviada,
//			lp_id: '003',
//			pcd_pvta: pcdPVta,
//			pcd_origen: pcdOrigen,
//			pcd_oferta: 'N',
//			p_id_remplazo: remplazoId,
//			ve_comi_base: 0,
//			ve_comi_porc: 0,
//			rp_comi_base: 0,
//			rp_comi_porc: 0,
//		});
//	});

//	console.log(`📦 ${productos.length} productos capturados del grid`);
//	return productos;
//}

function validarPedido(abm) {
	console.log(`🔍 Validando pedido (Modo: ${abm})...`);

	// ✅ VALIDACIÓN 1: Cliente obligatorio
	const ctaValidar = validarCliente();
	if (!ctaValidar) {
		return {
			esValido: false,
			mensaje: 'Debe seleccionar un cliente para el pedido.'
		};
	}

	// ✅ VALIDACIÓN 6: Debe haber al menos un producto
	const productos = obtenerProductosDelGrid();
	if (productos == null || productos == undefined)
		return;

	if (productos.length === 0) {
		return {
			esValido: false,
			mensaje: 'Debe agregar al menos un producto al pedido'
		};
	}

	// ✅ VALIDACIÓN 7: Todos los productos deben tener cantidad > 0
	const productosConCantidadInvalida = productos.filter(p => p.pcd_pedida <= 0);
	if (productosConCantidadInvalida.length > 0) {
		return {
			esValido: false,
			mensaje: 'Todos los productos deben tener una cantidad mayor a 0'
		};
	}

	console.log('✅ Validación exitosa');
	return { esValido: true, mensaje: '' };
}

function validarCliente() {
	// Caso 1: Pedido nuevo → se usa Rel01B
	const rel01 = $("#Rel01B");
	if (rel01.length && !rel01.prop("readonly")) {
		const valor = rel01.val()?.trim();
		const item = $("#Rel01BItem").val();

		if (!valor || !item) {
			//alert("Debe seleccionar un cliente válido.");
			return false;
		}

		return true;
	}

	// Caso 2: Pedido existente → se usa cta_denominacion
	const cta = $("#cta_id");
	if (cta.length) {
		const valor = cta.val()?.trim();

		if (!valor) {
			//alert("El cliente del pedido no es válido.");
			return false;
		}

		return true;
	}

	// Si no existe ninguno, es un error de estructura
	//alert("No se encontró un campo de cliente para validar.");
	return false;
}

function obtenerDatosFormularioPedido() {
	const pcCompte = $('#pc_compte').val();
	var ctaId = "";
	var pcFc = "";
	if (pcCompte == "0" || pcCompte == "") {
		ctaId = $('#Rel01BItem').val() || '';
	}
	else {
		ctaId = $('#cta_id').val() || '';
	}
	if ($("#pc_cons_final").is(":checked")) {
		pcFc = "S";
	}
	else {
		pcFc = "N";
	}
	const datos = {
		pc_compte: $('#pc_compte').val() || '',
		pc_fecha: $('#pc_fecha').val() || '',
		pc_entrega: $('#pc_entrega').val() || '',
		cta_id: ctaId,
		pc_obs: $('#pc_obs').val() || '',
		pc_cf: pcFc
	};

	console.log('📋 Datos del formulario capturados:', datos);
	return datos;
}

function HabilitarCamposFormularioPedido(habilitar, pceId) {
	// Normalizamos por si viene en minúscula
	pceId = (pceId || "").toUpperCase();

	// Grupos de estados
	const estadosModificables = ["P", "O"];   // Pendiente, En Curso
	const estadosParciales = ["C", "T"];      // Consolidado, A Facturar
	const estadosBloqueados = ["A", "E", "F"]; // Anulado, Entregado, Facturado

	if (estadosModificables.includes(pceId)) {
		habilitarObservacion(true);
		habilitarCF(true);
		habilitarDetalleProductos(true);
		console.log("Modo edición completa (P/O)");
		return;
	}
	if (estadosParciales.includes(pceId)) {
		habilitarObservacion(true);
		habilitarCF(true);
		habilitarDetalleProductos(false);
		console.log("Modo edición completa (C/T)");
		return;
	}
}

function habilitarObservacion(habilitar) {
	$("#pc_obs").prop("readonly", !habilitar);
}

function habilitarCF(habilitar) {
	$("#pc_cons_final").prop("disabled", !habilitar);
}

function habilitarDetalleProductos(habilitar) {
	// Inputs de cantidad
	$(".input-pcd_pedida").prop("readonly", !habilitar);

	// Botón eliminar producto
	$(".btn-eliminar-producto").toggle(habilitar);

	// Select de reemplazo
	$(".input-pcd_reemplazo").prop("disabled", !habilitar);
}

document.addEventListener("click", function (e) {

	const btn = e.target.closest("#btnCancelarORenABM");
	if (!btn) return;

	// Confirmación opcional
	const accion = document.querySelector("#accion")?.value;
	const esAlta = accion === "A";
	const esModificacion = accion === "M";

	let mensaje = "";

	if (esAlta) {
		mensaje = "¿Desea cancelar el alta de la Orden de Reparto?";
	} else if (esModificacion) {
		mensaje = "¿Desea cancelar la modificación de la Orden de Reparto?";
	}

	if (mensaje == "") return;

	AbrirMensaje(
		'CONFIRMAR CANCELACIÓN',
		mensaje,
		function (resp) {
			if (resp === 'SI') {
				// Ocultar vista de edición
				document.querySelector("#vistaEditarOR").classList.add("d-none");
				// Mostrar vista de lista
				document.querySelector("#vistaListaOR").classList.remove("d-none");
				// Opcional: limpiar contenido de edición
				document.querySelector("#vistaEditarOR").innerHTML = "";

				habilitarTabPedidos();
			}
			$('#msjModal').modal('hide');
		},
		true,
		['Confirmar', 'Cancelar'],
		'info!',
		null
	);
});

function formatearFechaDDMMYY(fechaStr) {
	const fecha = new Date(fechaStr);

	const dia = String(fecha.getDate()).padStart(2, "0");
	const mes = String(fecha.getMonth() + 1).padStart(2, "0");
	const anio = String(fecha.getFullYear()).slice(-2); // ← solo últimos 2 dígitos

	return `${dia}/${mes}/${anio}`;
}


function formatearFechaDDMMYYYY(fechaStr) {
	const fecha = new Date(fechaStr);

	const dia = String(fecha.getDate()).padStart(2, "0");
	const mes = String(fecha.getMonth() + 1).padStart(2, "0");
	const anio = fecha.getFullYear();

	return `${dia}/${mes}/${anio}`;
}

function bloquearTablas() {
	document.querySelector("#tbPedidosOR").classList.add("tabla-bloqueada");
	document.querySelector("#tbPedidosPendientes").classList.add("tabla-bloqueada");
}

function desbloquearTablas() {
	document.querySelector("#tbPedidosOR").classList.remove("tabla-bloqueada");
	document.querySelector("#tbPedidosPendientes").classList.remove("tabla-bloqueada");
}

function buildQueryFilters(pag) {
	const usaPeriodo = $("#chkDesdeHasta").is(":checked");
	const fechaD = usaPeriodo ? $("#Desde").val() : null;
	const fechaH = usaPeriodo ? $("#Hasta").val() : null;

	var rel01 = [];
	$("#EstadosList").children().each(function (i, item) { rel01.push($(item).val()) });

	var rel02 = [];
	$("#RepartidoresList").children().each(function (i, item) { rel02.push($(item).val()) });

	return {
		Registros: 200,
		Pagina: pag,
		FechaD: fechaD || null,
		FechaH: fechaH || null,
		Rel01: rel01.length ? rel01 : null,
		Rel02: rel02.length ? rel02 : null,
	};
}

function setBtnLoading($btn, loading, originalHtml) {
	if (!$btn || !$btn.length) return;
	if (loading) {
		$btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span>Buscando...');
	} else {
		$btn.prop("disabled", false).html(originalHtml ?? "Buscar");
	}
}

function configurarEventosSeleccionListaAnalisisAutOR() {
	$(document).off("click", "#tbGrillaAnalizaAut tbody tr");
	$(document).on("click", "#tbGrillaAnalizaAut tbody tr", function (e) {
		if (!$(e.target).is("button, a, .btn, i")) {
			var $this = $(this);
			var fueSeleccionado = $this.hasClass("selected-row");

			$("#tbGrillaAnalizaAut tbody tr").removeClass("selected-row");

			if (!fueSeleccionado) {
				$this.addClass("selected-row");
				let pId = $this.data("p-id");
				pIdSeleccionadoEnAnalisisAut = pId;
				//Poder hacer algo, como por ejemplo, habilitar o no botones dependiendo del estado de la OR
			}
		}
	});

	$(document).off("mouseenter", "#tbGrillaAnalizaAut th, #tbGrillaAnalizaAut td");
	$(document).on("mouseenter", "#tbGrillaAnalizaAut th, #tbGrillaAnalizaAut td", function () {

		const el = this;

		// Detectar si el contenido está truncado
		const isOverflowing = el.scrollWidth > el.clientWidth;

		if (isOverflowing) {
			// Agregar tooltip con el contenido completo
			$(el).attr("title", $(el).text().trim());
		} else {
			// Evitar tooltips innecesarios
			$(el).removeAttr("title");
		}
	});

}

function configurarEventosSeleccionListaOR() {
	$(document).off("click", "#tbGridOrdenDeReparto tbody tr");
	$(document).on("click", "#tbGridOrdenDeReparto tbody tr", function (e) {
		if (!$(e.target).is("button, a, .btn, i")) {
			var $this = $(this);
			var fueSeleccionado = $this.hasClass("selected-row");

			$("#tbGridOrdenDeReparto tbody tr").removeClass("selected-row");

			if (!fueSeleccionado) {
				$this.addClass("selected-row");
				let orCompte = $this.data("or-compte");
				let oreId = $this.data("ore-id");
				orCompteSeleccionado = orCompte;
				oreCompteSeleccionado = oreId;
				if (orCompte) {
					//Poder hacer algo, como por ejemplo, habilitar o no botones dependiendo del estado de la OR
					CargarPedidosDelReparto(orCompte);
					ConfigurarEstadoDeBotonesEnTabOrdenDeReparto(orCompte, oreId);
				}
			}
		}
	});
}

function configurarEventosSeleccionListaPedidosDeOR() {
	$(document).off("click", "#tbGridPedidosEnOrdenDeReparto tbody tr");
	$(document).on("click", "#tbGridPedidosEnOrdenDeReparto tbody tr", function (e) {
		if (!$(e.target).is("button, a, .btn, i")) {
			var $this = $(this);
			var fueSeleccionado = $this.hasClass("selected-row");

			$("#tbGridPedidosEnOrdenDeReparto tbody tr").removeClass("selected-row");

			if (!fueSeleccionado) {
				$this.addClass("selected-row");
				let pcCompte = $this.data("pc-compte");
				let pceId = $this.data("pce-id");
				pcCompteSeleccionado = pcCompte;
				pceCompteSeleccionado = pceId;
				if (pcCompte) {
					ConfigurarEstadoDeBotonesEnTabPedidosDeLaOrdenDeReparto(pcCompte, pceId);
				}
			}
		}
	});
}

function CargarPedidosDelReparto(orCompte) {
	AbrirWaiting("Cargar pedidos de la orden de reparto...");
	const url = obtenerPedidosDeLaOrdenDeRepartoUrl;
	PostGenHtml({ orCompte: orCompte }, url, function (html) {
		const header = `
            <div class="card mb-2">
				<div class="card-body py-2 d-flex align-items-center gap-4">
					<div>
						<i class="bx bx-file me-1"></i>
						<strong>Orden de Reparto N°:</strong> ${orCompte}
					</div>
				</div>
			</div>
        `;
		$("#divListaPedidosDeCliente").html(header + html);
		CerrarWaiting();
		configurarEventosSeleccionListaPedidosDeOR();
		ConfigurarEstadoDeBotonesEnTabPedidosDeLaOrdenDeReparto("","")
	});
}

function ConfigurarEstadoDeBotonesEnTabPedidosDeLaOrdenDeReparto(pcCompte, pceCompte) {

	const controls = {
		btnCF: document.getElementById("btnCF"),
		btnAsociarNC: document.getElementById("btnAsociarNC"),
		btnPedido: document.getElementById("btnPedido"),
		btnDividir: document.getElementById("btnDividir"),
		inputDividir: document.querySelector(".input-dividir")
	};

	const setState = (el, enabled) => {
		if (!el) return;
		el.disabled = !enabled;
		el.classList.toggle("disabled", !enabled);
	};

	// Si no hay pedido seleccionado → todo deshabilitado
	if (!pcCompte) {
		Object.values(controls).forEach(ctrl => setState(ctrl, false));
		return;
	}

	// Siempre habilitado si hay pedido
	setState(controls.btnPedido, true);

	// CF habilitado solo si el estado está permitido
	const estadosPermitidosCF = ["C", "O", "T"];
	setState(controls.btnCF, estadosPermitidosCF.includes(pceCompte));

	// Asociar NC solo si estado = F
	setState(controls.btnAsociarNC, pceCompte === "F");

	// Dividir solo si estado = T
	const dividirHabilitado = pceCompte === "T";
	setState(controls.btnDividir, dividirHabilitado);
	setState(controls.inputDividir, dividirHabilitado);
}

function ConfigurarEstadoDeBotonesEnTabOrdenDeReparto(orCompte, oreId) {
	// Estados permitidos para modificar
	const estadosPermitidosModificar = ["C", "O", "S"];

	// Botón Modificar OR
	const btnModificar = document.getElementById("btnModificarOR");
	if (btnModificar) {
		if (estadosPermitidosModificar.includes(oreId)) {
			btnModificar.disabled = false;
			btnModificar.classList.remove("disabled");
		} else {
			btnModificar.disabled = true;
			btnModificar.classList.add("disabled");
		}
	}

	// Botón En Curso → solo habilitado si oreId === 'S'
	const btnEnCurso = document.getElementById("btnEnCurso");
	if (btnEnCurso) {
		if (oreId === "S") {
			btnEnCurso.disabled = false;
			btnEnCurso.classList.remove("disabled");
		} else {
			btnEnCurso.disabled = true;
			btnEnCurso.classList.add("disabled");
		}
	}

	// Botón A Consolidar → solo habilitado si oreId === 'O'
	const btnConsolidar = document.getElementById("btnConsolidar");
	if (btnConsolidar) {
		if (oreId === "O") {
			btnConsolidar.disabled = false;
			btnConsolidar.classList.remove("disabled");
		} else {
			btnConsolidar.disabled = true;
			btnConsolidar.classList.add("disabled");
		}
	}

	// Botón A Facturar → solo habilitado si oreId === 'C'
	const btnAFacturar = document.getElementById("btnAFacturar");
	if (btnAFacturar) {
		if (oreId === "C") {
			btnAFacturar.disabled = false;
			btnAFacturar.classList.remove("disabled");
		} else {
			btnAFacturar.disabled = true;
			btnAFacturar.classList.add("disabled");
		}
	}

	const estadosPermitidosCambioPrecio = ["C", "O", "T"];
	const btnCambioPrecio = document.getElementById("btnCambioPrecio");
	if (btnCambioPrecio) {
		if (estadosPermitidosCambioPrecio.includes(oreId)) {
			btnCambioPrecio.disabled = false;
			btnCambioPrecio.classList.remove("disabled");
		} else {
			btnCambioPrecio.disabled = true;
			btnCambioPrecio.classList.add("disabled");
		}
	}

	const estadosPermitidosRegresarEnCurso = ["C", "T"];
	const btnVolverCurso = document.getElementById("btnVolverCurso");
	if (btnVolverCurso) {
		if (estadosPermitidosRegresarEnCurso.includes(oreId)) {
			btnVolverCurso.disabled = false;
			btnVolverCurso.classList.remove("disabled");
		} else {
			btnVolverCurso.disabled = true;
			btnVolverCurso.classList.add("disabled");
		}
	}

	const estadosPermitidosHojaDeRuta = ["C", "E", "F", "S", "T"];
	const btnHojaRuta = document.getElementById("btnHojaRuta");
	if (btnHojaRuta) {
		if (estadosPermitidosHojaDeRuta.includes(oreId)) {
			btnHojaRuta.disabled = false;
			btnHojaRuta.classList.remove("disabled");
		} else {
			btnHojaRuta.disabled = true;
			btnHojaRuta.classList.add("disabled");
		}
	}

	const estadosPermitidosHojaDeProductos = ["C", "E", "F", "S", "T"];
	const btnHojaProd = document.getElementById("btnHojaRuta");
	if (btnHojaProd) {
		if (estadosPermitidosHojaDeProductos.includes(oreId)) {
			btnHojaProd.disabled = false;
			btnHojaProd.classList.remove("disabled");
		} else {
			btnHojaProd.disabled = true;
			btnHojaProd.classList.add("disabled");
		}
	}

}

function activarSeleccionDeFilas(selectorTabla) {
	const tabla = document.querySelector(selectorTabla);
	if (!tabla) return;

	tabla.addEventListener("click", function (e) {
		const fila = e.target.closest("tr");
		if (!fila) return;

		// Evitar seleccionar filas vacías o mensajes
		if (fila.classList.contains("fila-vacia")) return;

		// Quitar selección previa
		tabla.querySelectorAll(".selected-row").forEach(f => f.classList.remove("selected-row"));

		// Agregar selección a la fila actual
		fila.classList.add("selected-row");
	});
}



$(document).on("click", "#btnAnalizar", function () {

	// Obtener depósitos seleccionados
	const seleccionados = [...document.querySelectorAll(".chk-depo:checked")]
		.map(chk => chk.closest("tr").dataset.depoId);

	if (seleccionados.length === 0) {
		AlertaWarning("Debe seleccionar al menos un depósito.");
		return;
	}

	bloquearPantalla();

	$.ajax({
		url: "/OrdenDeReparto/AnalizarOR",
		type: "POST",
		data: {
			orCompte: "@Model.OrdenDeReparto.or_compte",
			depositos: seleccionados
		},
		success: function (html) {
			$("#divAnalisis").html(html);
		},
		complete: function () {
			desbloquearPantalla();
		}
	});
});


mostrarInfoProd = true;
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

/* *************************************************************************************** */
///Hanlder para manejar la apertura de Info de Producto desde la columna Descripción
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

	if (pIdSeleccionadoEnAnalisisAut && pIdSeleccionadoEnAnalisisAut !== "") {
		$("#divInfoAdicionaDeProducto").collapse("toggle");

		setTimeout(() => {
			invocarComponenteDeInfoAdicionalDeProd({
				p_id: pIdSeleccionadoEnAnalisisAut,
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
///////////////////////////////////////////////////////////////////////////////////////////////////
/* ######	FIN Componente de info adicional de producto ###### */


const maskConfigDecimales = {
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

const maskConfigEnteros = {
	alias: "numeric",
	groupSeparator: ",",
	radixPoint: ".",
	autoGroup: true,
	digits: 0,
	digitsOptional: true,
	rightAlign: true,
	prefix: '',
	placeholder: "0",
	clearMaskOnLostFocus: false,
	showMaskOnHover: false,
	showMaskOnFocus: false
};

function deshabilitarTabPedidos() {
	$("#tabPedidosDeCliente").addClass("tab-disabled");
}

function habilitarTabPedidos() {
	$("#tabPedidosDeCliente").removeClass("tab-disabled");
}


// ============================================================================
// INTEGRACIÓN CON BÚSQUEDA AVANZADA V02
// ============================================================================

function cargarModalBusquedaAvanzada(callback) {
	if ($("#busquedaModal").length > 0) {
		if (typeof callback === 'function') callback();
		return;
	}

	const urlModal = typeof busquedaAvanzadaModalUrl !== 'undefined'
		? busquedaAvanzadaModalUrl
		: '/ControlComun/Producto/BusquedaAdvanceV02';

	$.ajax({
		url: urlModal,
		type: 'GET',
		success: function (html) {
			if ($("#busquedaModal").length === 0) {
				$('body').append(html);
			}
			if (typeof callback === 'function') {
				callback();
			}
		},
		error: function (xhr, status, error) {
			console.error("Error al cargar modal de búsqueda:", error);
			ControlaMensajeError("No se pudo cargar el módulo de búsqueda de productos");
		}
	});
}