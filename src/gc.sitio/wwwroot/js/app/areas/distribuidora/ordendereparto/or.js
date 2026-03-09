let _pedidoLoading = false;
let orCompteSeleccionado = null;

$(function () {
	InicializaPantallaOrdenDeReparto();
	InicializaEventosPedido();
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

function InicializaEventosPedido() {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacion(div);
	});

	// Buscar
	$("#btnBuscar").on("click", function () {
		buscarOrdenesDeReparto(this);
	});
	funcCallBack = buscarOrdenesDeReparto;
}

async function buscarOrdenesDeReparto(btn, pag = 1) {
	if (_pedidoLoading) return;
	_pedidoLoading = true;

	const $btn = $(btn);
	const originalHtml = $btn.html();
	setBtnLoading($btn, true);

	try {
		const filtros = buildQueryFilters(pag);
		const url = buscarOrdenesDeRepartoUrl;
		const urlInitView = inicializarViewUrl;

		PostGenHtml({}, urlInitView, function (html) {
			$("#divDetalle").html(html).collapse("show");
			$("#divFiltro").collapse("hide");

			CargarOrdenesDeReparto(filtros, url);
		});


	} catch (e) {
		console.error("Error al buscar pedidos de clientes:", e);
		$("#divDetalle").html('<div class="alert alert-danger py-2 mb-0">No se pudo obtener la información.</div>').collapse("show");
	} finally {
		setBtnLoading($btn, false, originalHtml);
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

$(document).on("click", "#btnAgregarOR, #btnModificarOR", function () {
	CargarVistaNuevaOrdenDeReparto();
});

function CargarVistaNuevaOrdenDeReparto() {
	AbrirWaiting("Cargando ABM de Orden de Reparto");
	PostGenHtml({ accion: "A", orCompte: "" }, cargarVistaABMOrdenDeRepartoUrl, function (html) {
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

$(document).on("click", "#btnConsolidar", function () {
	$("#vistaListaOR").addClass("d-none");
	$("#vistaConsolidarOR").removeClass("d-none");
});

$(document).on("click", "#btnConsolidarOR, #btnCancelarConsolidar, #btnReasignar", function () {
	$("#vistaConsolidarOR").addClass("d-none");
	$("#vistaListaOR").removeClass("d-none");
});

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

	bloquearTablas(); // 🔥 Bloqueo inmediato

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

	// Crear fila nueva para la tabla izquierda
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
						data-id="${pedido.id}"
						data-cliente="${pedido.cliente}"
						data-fecha="${pedido.fecha}"
						data-vendedor="${pedido.vendedor}"
						data-repartidor="${pedido.repartidor}"
						data-rp-id="${pedido.rpId}"
						data-importe="${pedido.importe}">
					<i class="bx bx-minus"></i>
				</button>

				<button class="btn btn-secondary btn-table btn-sm btnEditarPedido"
						data-id="${pedido.id}">
					<i class="bx bx-edit"></i>
				</button>
			</div>
		</td>
    `;

	// Eliminar la fila "No hay pedidos" si existe
	const filaVacia = tablaIzquierda.querySelector(".fila-vacia");
	if (filaVacia) {
		filaVacia.remove();
	}

	tablaIzquierda.appendChild(tr);

	// Fade-out en la fila derecha
	filaDerecha.classList.add("fade-out-row");

	setTimeout(() => {
		filaDerecha.remove();
		desbloquearTablas(); // 🔥 Desbloqueo
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


function configurarEventosSeleccionListaOR() {
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

function CargarPedidosDelReparto(orCompte) {
	AbrirWaiting("Cargar pedidos de la orden de reparto...");
	const url = obtenerPedidosDeLaOrdenDeRepartoUrl;
	PostGenHtml({ orCompte: orCompte }, url, function (html) {
		$("#divListaPedidosDeCliente").html(html);
		CerrarWaiting();
		//Evaluar estados de los botones
	});
}

function ConfigurarEstadoDeBotonesEnTabOrdenDeReparto(orCompte, oreId) {
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
