let _pedidoLoading = false;
let orCompteSeleccionado = null;
let pcCompteSeleccionado = null;
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

	// Etiquetas de filtros
	$("#lbChkDesdeHasta").text("Periodo");
	$("#lbEstados").text("Estado"); // Estados
	$("#lbRepartidores").text("Repartidores"); // Repartidores

	$("#chkDesdeHasta").on("click", function () {
		if ($("#chkDesdeHasta").is(":checked")) {
			$("#Desde").prop("disabled", false);
			$("#Hasta").prop("disabled", false);
		} else {
			$("#Desde").prop("disabled", true);
			$("#Hasta").prop("disabled", true);
		}
	});

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
	const hoy = new Date();
	const base = new Date(hoy.getFullYear(), hoy.getMonth(), hoy.getDate());
	const hasta = new Date(base);
	hasta.setDate(hasta.getDate() + 30);

	const format = (d) => {
		const y = d.getFullYear();
		const m = String(d.getMonth() + 1).padStart(2, '0');
		const day = String(d.getDate()).padStart(2, '0');
		return `${y}-${m}-${day}`;
	};

	$("#Desde").val(format(base));
	$("#Hasta").val(format(hasta));

	const enabled = $("#chkDesdeHasta").is(":checked");
	$("#Desde").prop("disabled", !enabled);
	$("#Hasta").prop("disabled", !enabled);
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

$(document).on("click", "#btnHojaRuta", function () {
	ControlaImprimirHojaDeRutaDeOrdenDeReparto();
});

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

		let marcado = $(this).is(":checked");

		$("#tbCambioDePrecio tbody .chk-actualizar-precio")
			.prop("checked", marcado);
	});

	// Si el usuario marca/desmarca manualmente, actualizar el checkbox global
	$(document).off("change", ".chk-actualizar-precio");
	$(document).on("change", ".chk-actualizar-precio", function () {

		let total = $("#tbCambioDePrecio tbody .chk-actualizar-precio").length;
		let marcados = $("#tbCambioDePrecio tbody .chk-actualizar-precio:checked").length;

		$("#chkSeleccionGlobal").prop("checked", total === marcados);
	});

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
			ControlaMensajeError(
				'Error al intentar consolidar la O.R.: ' +
				(obj.mensaje || 'Error desconocido')
			);
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
	});
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
		// 1) Ver si hubo cambios
		if (HayCambiosEnDetalleConteo()) {
			AbrirMensaje(
				'CONFIRMAR REASIGNACIÓN',
				"¿Desea confirmar las modificaciones realizadas?",
				function (resp) {
					GuardarReasignacionEnDatosDeSesion();
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
			ControlaMensajeError(
				'Error al intentar consolidar la O.R.: ' +
				(obj.mensaje || 'Error desconocido')
			);
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
				ControlaMensajeError(
					'Error al intentar reasignar cantidades de productos: ' +
					(resp.mensaje || 'Error desconocido')
				);
			}
			else {
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


function HayCambiosEnDetalleConteo() {

	let huboCambios = false;

	$("#tbConsolidarDetalleConteo tbody tr").each(function () {

		let $celda = $(this).find("td.celda-a-enviar");
		let original = $celda.attr("data-original");

		let valorActual;

		if ($celda.find("input").length > 0) {
			valorActual = $celda.find("input").val().trim();
		} else {
			valorActual = $celda.text().trim();
		}

		valorActual = valorActual.replace(/,/g, "");

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
			// No salir de edición mientras el modo está activo
			return;
		}

		// Modo edición desactivado → sí se aplica el blur normal
		let valor = $(this).val().trim();
		if (valor === "") valor = "0";

		// Quitar máscara para obtener el número real
		valor = valor.replace(/,/g, "");

		$(this).parent().text(valor);

		// 🔥 Recalcular diferencias
		RecalcularDiferenciasEnDetalleConteo();

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
function GuardarValorYRecalcular($input) {

	let valor = $input.val().trim();
	if (valor === "") valor = "0";

	valor = valor.replace(/,/g, "");

	// Guardar en el input (no reemplazar el td)
	$input.val(valor);

	RecalcularDiferenciasEnDetalleConteo();
}


function EvaluarHabilitarReasignar() {
	// 1) Obtener la fila seleccionada en la grilla superior
	let filaSeleccionada = $("#tbConsolidarConteos tbody tr.selected-row");

	if (filaSeleccionada.length === 0) {
		$("#btnReasignar").prop("disabled", true);
		return;
	}

	// 2) Obtener el valor de Dif de la fila seleccionada
	let difTexto = filaSeleccionada.find("td").eq(5).text().trim(); // columna Dif es la 6ta
	let dif = parseFloat(difTexto.replace(",", "."));

	// 3) Contar filas reales en la grilla inferior
	let filasInferiores = $("#tbConsolidarDetalleConteo tbody tr")
		.not(".fila-vacia")
		.length;

	// 4) Aplicar la lógica
	if (dif != 0 && filasInferiores > 1) {
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
		const depositosSeleccionados = [...document.querySelectorAll(".chk-depo:checked")]
			.map(chk => chk.closest("tr").dataset.depoId);

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
				}
				$('#msjModal').modal('hide');
			},
			true,
			['Confirmar', 'Cancelar'],
			'info!',
			null
		);
	});
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
				ControlaMensajeError(
					'Error al poner en curso la orden de reparto: ' +
					(response.mensaje || 'Error desconocido')
				);
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
	AbrirWaiting("Cargando ABM de Orden de Reparto");
	PostGenHtml({ accion: abm, orCompte: orCompte }, cargarVistaABMOrdenDeRepartoUrl, function (html) {
		CerrarWaiting();
		$("#vistaEditarOR").html(html);
		$("#vistaListaOR").addClass("d-none");
		$("#vistaEditarOR").removeClass("d-none");
		activarSeleccionDeFilas("#tbPedidosOR tbody");
		activarSeleccionDeFilas("#tbPedidosPendientes tbody");
	});
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
			///TODO: Descomentar si corresponde
			//if (abm == 'A' || abm == 'M')
			//	ImprimirPedido_Generado(response.id);
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

document.addEventListener("click", function (e) {

	const btn = e.target.closest(".btnEditarPedido");
	if (!btn) return;

	alert("Servicio no implementado...");
	return;

	const pedidoId = btn.dataset.id;

	// 1) Mostrar el TAB
	const tabLi = document.querySelector("#tabEditarPedido");
	tabLi.classList.remove("d-none");

	// 2) Activar el TAB con Bootstrap
	const tabButton = document.querySelector("#btnTabEditarPedido");
	const bsTab = new bootstrap.Tab(tabButton);
	bsTab.show();

	// 3) Si querés evitar que el usuario cambie de tab mientras edita (Opcional)
	//document.querySelector("#tabOrdenesDeReparto").classList.add("tab-disabled");
	//document.querySelector("#tabPedidosDeCliente").classList.add("tab-disabled");

	// 3.A) Y para habilitarlos de nuevo
	//document.querySelector("#tabOrdenesDeReparto").classList.remove("tab-disabled");
	//document.querySelector("#tabPedidosDeCliente").classList.remove("tab-disabled");

	// 4) Cargar contenido (AJAX o lo que necesites)
	cargarFormularioEdicion(pedidoId);

});

function cargarFormularioEdicion(pedidoId) {


	///TODO : Editar esta parte para abrir la edicion de Pedido de Cliente
	alert("Servicio no implementado...");
	//AbrirWaiting("Cargando pedido...");


	//PostGenHtml(
	//	{ idPedido: pedidoId },
	//	urlCargarFormularioEdicion,
	//	function (html) {
	//		CerrarWaiting();
	//		document.querySelector("#divEditarPedido").innerHTML = html;
	//	}
	//);
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
		$("#divListaPedidosDeCliente").html(html);
		CerrarWaiting();
		//Evaluar estados de los botones
	});
}

function ConfigurarEstadoDeBotonesEnTabPedidosDeLaOrdenDeReparto(pcCompte, pceId) {
	const estadosPermitidosCF = ["C", "O", "T"];
	const btnCF = document.getElementById("btnCF");
	if (btnCF) {
		if (estadosPermitidosCF.includes(pceId)) {
			btnCF.disabled = false;
			btnCF.classList.remove("disabled");
		} else {
			btnCF.disabled = true;
			btnCF.classList.add("disabled");
		}
	}

	const btnAsociarNC = document.getElementById("btnAsociarNC");
	if (btnAsociarNC) {
		if (pceId === "F") {
			btnAsociarNC.disabled = false;
			btnAsociarNC.classList.remove("disabled");
		} else {
			btnAsociarNC.disabled = true;
			btnAsociarNC.classList.add("disabled");
		}
	}

	const btnDividir = document.getElementById("btnDividir");
	const inputDividir = document.querySelector(".input-dividir");
	if (btnDividir) {
		if (pceId === "T") {
			btnDividir.disabled = false;
			btnDividir.classList.remove("disabled");
			if (inputDividir) {
				inputDividir.disabled = false;
				inputDividir.classList.remove("disabled");
			}
		} else {
			btnDividir.disabled = true;
			btnDividir.classList.add("disabled");
			if (inputDividir) {
				inputDividir.disabled = true;
				inputDividir.classList.add("disabled");
			}

		}
	}
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

	// Botón A Facturar → solo habilitado si oreId === 'C'
	//const btnCambioPrecio = document.getElementById("btnCambioPrecio");
	//if (btnCambioPrecio) {
	//	if (oreId === "C") {
	//		btnCambioPrecio.disabled = false;
	//		btnCambioPrecio.classList.remove("disabled");
	//	} else {
	//		btnCambioPrecio.disabled = true;
	//		btnCambioPrecio.classList.add("disabled");
	//	}
	//}

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

	const estadosPermitidosRegresarEnCurso = ["O", "T"];
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
