$(function () {
	InicializaPantallaConsultaFaltante();
});

function InicializaPantallaConsultaFaltante() {
	if ($("#divDetalle").is(":visible")) {
		$("#divDetalle").collapse("hide");
	}
	$("#divFiltro").collapse("show");

	$("#divFiltro").on("shown.bs.collapse hidden.bs.collapse", function () {
		const abierto = $(this).hasClass("show");

		if (abierto) {
			$("#divDetalle").collapse("hide");
		} else {
			$("#divDetalle").collapse("show");
		}
	});

	$("#btnCancel").on("click", function () {
		window.location.href = homePedido;
	});

	$("#btnImprimir").off("click");
	$("#btnImprimir").on("click", controlaImprimirFaltanteDeCaja);
	$("#SucursalesList").on("dblclick", 'option', function () { $(this).remove(); })
	$(document).off("change", "#listaSucursales");
	$(document).on("change", "#listaSucursales", ControlalistaSucursalesSelected);
	$("#btnImprimir").prop("disabled", true).hide();

	$(document).on("keydown", "#divFiltro input[type='date']", function (e) {
		if (e.key === "Enter") {
			e.preventDefault(); // evita submit o comportamientos raros
			$("#btnBuscar").trigger("click");
		}
	});

	initPeriodoFechas();

	$("#lbChkDesdeHasta").text("Periodo");
	$("#chkDesdeHasta")
		.prop("checked", true)
		.prop("disabled", true);
	$("#chkSucursales")
		.prop("checked", true)
		.prop("disabled", true);

	$("#Desde").prop("disabled", false);
	$("#Hasta").prop("disabled", false);

	$("#lbSucursales").text("Sucursales");

	$("#btnBuscar").on("click", function () {
		const [ok, msg] = ValidarFiltro();
		if (!ok) {
			AbrirMensaje("ATENCIÓN", msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			try { MostrarFiltrosAplicados(); } catch (e) { }
			InicializarPantallaPrincipal();
		}
	});
}

function controlaImprimirFaltanteDeCaja() {
	const tieneDatos = $("#tbGridFaltantes tbody tr[data-caja-nro_proceso]").length > 0;
	if (!tieneDatos) {
		AbrirMensaje("ATENCIÓN", "No hay faltantes de caja para imprimir", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		ImprimirFaltanteDeCaja();
	}
}

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function ImprimirFaltanteDeCaja() {
	ReseteoDeReportes();
	setTimeout(() => {
		let desde = $("#Desde").val();
		let hasta = $("#Hasta").val();
		let temp = ObtenerFiltroLista("#chkSucursales", "#SucursalesList");
		let lSuc = (temp.ids.length === 1 && temp.ids[0] === "%") ? "%" : temp.ids.join(",");
		lSucTextos = temp.textos;
		// 🔥 Construcción del string de filtros
		var filtrosDesc = [];
		filtrosDesc.push(ConstruirDescripcionFiltro("Sucursales", "#chkSucursales", "#SucursalesList"));
		filtrosDesc.push(`Desde el ${desde} al ${hasta}`);
		// Limpieza: eliminar vacíos
		filtrosDesc = filtrosDesc.filter(x => x !== "");
		// String final
		var filtrosString = filtrosDesc.join(" | ");
		let data = { desde, hasta, lSuc, lSucTextos, filtrosString };
		cargarReporteEnArre(106, data, "CONSULTA FALTANTE DE CAJA", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function InicializarPantallaPrincipal() {
	let desde = $("#Desde").val();
	let hasta = $("#Hasta").val();
	var temp = ObtenerFiltroLista("#chkSucursales", "#SucursalesList");
	lSuc = temp.ids;
	lSucTextos = temp.textos;

	AbrirWaiting("Cargando Faltantes de Caja.")
	var data = { desde, hasta, lSuc, lSucTextos };
	PostGenHtml(data, cargarPantallaPrincipalURL, function (obj) {
		$("#divDetalle").html(obj);
		$("#divFiltro").collapse("hide");
		$("#divDetalle").collapse("show");

		CargarFaltantesDeCajasListado(desde, hasta, lSuc);

	});
	console.log("BuscarRecepcionesDelProveedor");
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

function CargarFaltantesDeCajasListado(desde, hasta, lSuc) {
	var data = { desde, hasta, lSuc };
	PostGenHtml(data, cargarFaltantesDeCajasListadoURL, function (obj) {
		$("#divFaltanteDeCajaLista").html(obj);

		const tieneDatos =
			$("#tbGridFaltantes tbody tr[data-caja-nro_proceso]").length > 0;

		$("#btnImprimir")
			.prop("disabled", !tieneDatos)
			.toggle(tieneDatos);
		InicializarEventosTabFaltantes();
		CerrarWaiting();
	});
}

function InicializarEventosTabFaltantes() {
	$(document).off("click", "#tbGridFaltantes tbody tr");
	$(document).on("click", "#tbGridFaltantes tbody tr", function (e) {

		if ($(e.target).is("button, a, .btn, i")) return;

		const $nuevaFila = $(this);
		ProcesarSeleccionFilaEnTabFaltantes($nuevaFila);
	});
}

function ProcesarSeleccionFilaEnTabFaltantes($fila) {
	$("#tbGridFaltantes tbody tr").removeClass("selected-row");
	$fila.addClass("selected-row");
}

function ObtenerFiltroLista(idCheckbox, idListBox) {

	const estaChequeado = $(idCheckbox).is(":checked");
	const ids = [];
	const textos = [];

	$(idListBox + " option").each(function () {
		ids.push($(this).val());
		textos.push($(this).text());
	});

	if (!estaChequeado) {
		return {
			ids: ["%"],
			textos: "Todos"
		};
	}

	if (estaChequeado && ids.length === 0) {
		return {
			ids: ["%"],
			textos: "Todos"
		};
	}

	return {
		ids: ids,
		textos: textos.join(", ")
	};
}

function ValidarFiltro() {

	let fDesde = $("#Desde").val();
	let fHasta = $("#Hasta").val();

	if (!fDesde || !fHasta) {
		return [false, "Debe seleccionar ambas fechas."];
	}

	let dDesde = new Date(fDesde);
	let dHasta = new Date(fHasta);

	if (isNaN(dDesde.getTime()) || isNaN(dHasta.getTime())) {
		return [false, "Alguna de las fechas no es válida."];
	}

	if (dDesde > dHasta) {
		return [false, "La fecha 'Desde' no puede ser mayor que la fecha 'Hasta'."];
	}

	let diffMs = dHasta - dDesde;
	let diffDias = diffMs / (1000 * 60 * 60 * 24);

	if (diffDias > 31) {
		return [false, "El rango de fechas no puede superar los 31 días."];
	}

	return [true, ""];
}

function ControlalistaSucursalesSelected() {
	var item = $("#listaSucursales").val();
	var desc = $("#listaSucursales option:selected").text();
	if ($("#SucursalesList").has('option:contains("' + item + '")').length === 0 && $("#SucursalesList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#SucursalesList").append(opc);
	}
}

function initPeriodoFechas() {
	const desde = obtenerPrimerDiaMes();

	const hasta = new Date();

	const fmt = d => d.toISOString().split("T")[0];

	$("#Desde").val(fmt(desde));
	$("#Hasta").val(fmt(hasta));

	$("#Desde").prop("disabled", false);
	$("#Hasta").prop("disabled", false);

	$("#chkDesdeHasta")
		.prop("checked", true)
		.prop("disabled", true);
}

function obtenerPrimerDiaMes() {
	const hoy = new Date();
	const year = hoy.getFullYear();
	const month = hoy.getMonth(); // 0=enero ... 11=diciembre

	return new Date(year, month, 1);
}