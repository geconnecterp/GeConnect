var invNroSeleccionado = "";
var inveIdSeleccionado = "";
var invtIdSeleccionado = "";
$(function () {
	InicializarCamposEnFiltros(false);

	$("#Date1, #Date2").on("blur", ValidarFechasClick);

	$("#btnBuscar").on("click", function () {
		if (validarFechas()) {
			BuscarInventarios();
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
});

function BuscarInventarios() {
	var data = {};
	AbrirWaiting("Inicializando presentación de vista de reporte de inventario...");
	PostGenHtml(data, inicializarPantallPrincipalURL, function (obj) {
		$("#divDetalle").html(obj);
		$("#divFiltros").collapse("hide");
		$("#divDetalle").collapse("show");
		$("#btnTabInventarioReporte").addClass("tab-disabled");
		AgregarHandlerEnTabs();
		CerrarWaiting();
		CargarInventarioLista();
		setTimeout(() => {
			CargarEventosABotonesEnDivPrincipal();
		}, 500);
		return true
	});
}

function AgregarHandlerEnTabs() {
	// Cuando el usuario hace click en "Inventarios"
	$('#btnTabInventarioLista').on('shown.bs.tab', function () {
		$('#btnImprimir').hide();
	});

	// Cuando el usuario hace click en "Reporte"
	$('#btnTabInventarioReporte').on('shown.bs.tab', function () {
		$('#btnImprimir').show();
	});

}

function usuarioSeleccionado() {
	const ddl = $("#listaUsuarios");
	// 1) Debe estar habilitado
	if (ddl.prop("disabled")) {
		return "";
	}
	// 2) Debe tener más opciones que la opción por defecto
	if (ddl.find("option").length <= 1) {
		return "";
	}
	// 3) Debe tener un valor seleccionado distinto de ""
	const valor = ddl.val();
	if (!valor || valor === "") {
		return "";
	}
	return valor;
}


function CargarEventosABotonesEnDivPrincipal() {
	$(document).on("click", "#btnRepoStkVsConteo", CargarTabRepoStkVsConteo);
	$(document).on("click", "#btnRepoValorPorSec", CargarTabRepoValorPorSec);
	$(document).on("click", "#btnRepoValorPorRub", CargarTabRepoValorPorRub);
	$(document).on("click", "#btnRepoValorDetalle", CargarTabRepoValorDetalle);
	$(document).on("click", "#btnRepoConteoPorUsu", CargarTabRepoConteoPorUsu);
}

function CargarTabRepoStkVsConteo() {
	var inv_nro = invNroSeleccionado;
	if (inv_nro == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un inventario.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Cargando datos...");
		var data = { inv_nro };
		PostGenHtml(data, inicializarTabRepoStkVsConteoURL, function (obj) {
			$("#divReporte").html(obj);
			let tab = new bootstrap.Tab(document.querySelector("#btnTabInventarioReporte"));
			setTimeout(() => {
				$(document).off("click", "#btnImprimir").on("click", "#btnImprimir", ControlaImprRepoStkVsConteo);
			}, 300);
			tab.show();
			$("#btnImprimir").show();
			CerrarWaiting();
			return true
		});
	}
}

function CargarTabRepoValorDetalle() {
	var inv_nro = invNroSeleccionado;
	if (inv_nro == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un inventario.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Cargando datos...");
		var data = { inv_nro };
		PostGenHtml(data, inicializarTabRepoValorDetalleURL, function (obj) {
			$("#divReporte").html(obj);
			let tab = new bootstrap.Tab(document.querySelector("#btnTabInventarioReporte"));
			setTimeout(() => {
				$(document).off("click", "#btnImprimir").on("click", "#btnImprimir", ControlaImprRepoValorDetalle);
			}, 300);
			tab.show();
			$("#btnImprimir").show();
			CerrarWaiting();
			return true
		});
	}
}

function CargarTabRepoConteoPorUsu() {
	if (usuarioSeleccionado() == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un usario.", function () {
			$("#msjModal").modal("hide");
			$("#listaUsuarios").trigger("focus");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		var inv_nro = invNroSeleccionado;
		var usu_id = $("#listaUsuarios").val();
		var data = { inv_nro, usu_id };
		PostGenHtml(data, inicializarTabRepoConteosPorUsuURL, function (obj) {
			$("#divReporte").html(obj);
			let tab = new bootstrap.Tab(document.querySelector("#btnTabInventarioReporte"));
			setTimeout(() => {
				$(document).off("click", "#btnImprimir").on("click", "#btnImprimir", ControlaImprRepoConteosPorUsu);
			}, 300);
			tab.show();
			$("#btnImprimir").show();
			CerrarWaiting();
			return true
		});
	}
}

function CargarTabRepoValorPorRub() {
	var inv_nro = invNroSeleccionado;
	if (inv_nro == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un inventario.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Cargando datos...");
		var data = { inv_nro };
		PostGenHtml(data, inicializarTabRepoValorPorRubURL, function (obj) {
			$("#divReporte").html(obj);
			let tab = new bootstrap.Tab(document.querySelector("#btnTabInventarioReporte"));
			setTimeout(() => {
				$(document).off("click", "#btnImprimir").on("click", "#btnImprimir", ControlaImprRepoValorPorRub);
			}, 300);
			tab.show();
			$("#btnImprimir").show();
			CerrarWaiting();
			return true
		});
	}
}

function CargarTabRepoValorPorSec() {
	var inv_nro = invNroSeleccionado;
	if (inv_nro == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un inventario.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Cargando datos...");
		var data = { inv_nro };
		PostGenHtml(data, inicializarTabRepoValorPorSecURL, function (obj) {
			$("#divReporte").html(obj);
			let tab = new bootstrap.Tab(document.querySelector("#btnTabInventarioReporte"));
			setTimeout(() => {
				$(document).off("click", "#btnImprimir").on("click", "#btnImprimir", ControlaImprRepoValorPorSec);
			}, 300);
			tab.show();
			$("#btnImprimir").show();
			CerrarWaiting();
			return true
		});
	}
}

function ControlaImprRepoStkVsConteo() {
	var filas = $("#tbGridInvStkVsConteo tbody tr[data-inv-nro]").length;
	if (filas == 0) {
		AbrirMensaje("ATENCIÓN", "No hay datos para imprimir.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Imprimiendo listado...");
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
				ImpimirRepoStkVsConteo();
			}
		});
	}
}

function ControlaImprRepoValorDetalle() {
	var filas = $("#tbGridInvValorDetalle tbody tr[data-registro]").length;
	if (filas == 0) {
		AbrirMensaje("ATENCIÓN", "No hay datos para imprimir.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Imprimiendo listado...");
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
				ImpimirRepoValorDetalle();
			}
		});
	}
}

function ControlaImprRepoConteosPorUsu() {
	var filas = $("#tbGridInvConteosPorUsu tbody tr[data-inv-nro]").length;
	if (filas == 0) {
		AbrirMensaje("ATENCIÓN", "No hay datos para imprimir.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Imprimiendo listado...");
		var tipoReporte = 5;
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
				ImpimirRepoConteosPorUsu();
			}
		});
	}
}

function ControlaImprRepoValorPorRub() {
	var filas = $("#tbGridInvValorPorRub tbody tr[data-inv-nro]").length;
	if (filas == 0) {
		AbrirMensaje("ATENCIÓN", "No hay datos para imprimir.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Imprimiendo listado...");
		var tipoReporte = 3;
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
				ImpimirRepoValorPorRub();
			}
		});
	}
}

function ControlaImprRepoValorPorSec() {
	var filas = $("#tbGridInvValorPorSec tbody tr[data-inv-nro]").length;
	if (filas == 0) {
		AbrirMensaje("ATENCIÓN", "No hay datos para imprimir.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting("Imprimiendo listado...");
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
				ImpimirRepoValorPorSec();
			}
		});
	}
}

function ImpimirRepoStkVsConteo() { 
	ReseteoDeReportes();
	setTimeout(() => {
		var inv_nro = invNroSeleccionado;
		var data = { inv_nro };
		cargarReporteEnArre(57, data, "REGISTRO DE STOCK VS CONTEO", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ImpimirRepoValorDetalle() {
	ReseteoDeReportes();
	setTimeout(() => {
		var inv_nro = invNroSeleccionado;
		var data = { inv_nro };
		cargarReporteEnArre(60, data, "VALORIZADO DETALLE", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ImpimirRepoConteosPorUsu() {
	ReseteoDeReportes();
	setTimeout(() => {
		var inv_nro = invNroSeleccionado;
		var usu_id = $("#listaUsuarios").val();
		var usu_nombre = $("#listaUsuarios option:selected").text();
		var data = { inv_nro, usu_id, usu_nombre };
		cargarReporteEnArre(61, data, "PLANILLA POR USUARIOS", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ImpimirRepoValorPorRub() {
	ReseteoDeReportes();
	setTimeout(() => {
		var inv_nro = invNroSeleccionado;
		var data = { inv_nro };
		cargarReporteEnArre(59, data, "VALORIZADO POR RUBROS", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ImpimirRepoValorPorSec() {
	ReseteoDeReportes();
	setTimeout(() => {
		var inv_nro = invNroSeleccionado;
		var data = { inv_nro };
		cargarReporteEnArre(58, data, "VALORIZADO POR SECTORES", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function selectReg(x, gridId) {
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
	if (gridId == 'tbGridInventario') {
		invNroSeleccionado = x.getAttribute("data-inv-nro");
		inveIdSeleccionado = x.getAttribute("data-inve-id");
		invtIdSeleccionado = x.getAttribute("data-invt-id");
		ActualizarListaDeUsuariosDelInventario();
	}
}

function ActualizarListaDeUsuariosDelInventario() {
	if (invNroSeleccionado != "") {
		var data = { inv_nro: invNroSeleccionado };
		PostGen(data, obtenerUsuariosDelInventarioURL, function (obj) {
			CerrarWaiting();
			const ddl = $("#listaUsuarios");
			if (obj.error === true) {
				CerrarWaiting();
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else if (obj.error === false && obj.usrs.length === 0) {
				ddl.append('<option value="">Seleccionar</option>');
				ddl.empty();
				ddl.prop("disabled", true);
				return;
			}
			else {
				ddl.empty();
				ddl.append('<option value="">Seleccionar</option>');

				// Si viene como string → parsearlo
				let usuarios = obj.usrs;
				if (typeof usuarios === "string") {
					try {
						usuarios = JSON.parse(usuarios);
					} catch (e) {
						console.error("Error al parsear JSON:", e);
						ddl.prop("disabled", true);
						return;
					}
				}

				usuarios.forEach(function (usr) {
					ddl.append($('<option></option>').val(usr.usu_id).html(usr.usu_apellidoynombre));
				});
				ddl.prop("disabled", false);
				return;
			}
			return true
		});
	}
}

function CargarInventarioLista() {
	TaskManager.start();
	var desde = $("#Desde").val();
	var hasta = $("#Hasta").val();
	const estadoBool = $("#chkEstados").is(":checked");
	let inve_id = "%";
	if (estadoBool) {
		inve_id = $("#listaEstados").val();
	}
	
	const sucursalBool = $("#chkSucursales").is(":checked");
	let adm_id = "%";
	if (sucursalBool) {
		adm_id = $("#listaSucursales").val();
	}
	else {
		if (!sucursalBool && $("#listaSucursales").val() != "") {
			adm_id = $("#listaSucursales").val();
		}
	}
	
	var data = { desde, hasta, adm_id, inve_id };
	PostGenHtml(data, buscarInventarioListaURL, function (obj) {
		$("#divGrillaInventarios").html(obj);
		TaskManager.end();
		return true
	});
}

function InicializarCamposEnFiltros(vieneDeCancelar) {
	if (!vieneDeCancelar) {
	}

	$("#chkDesdeHasta").prop('checked', true);
	$("#chkDesdeHasta").trigger("change");
	$("#chkDesdeHasta").prop("disabled", true);

	$("#btnImprimir").hide();
	$("#lbChkDesdeHasta").text("Fecha");
	$("#lbSucursales").text("Sucursal");
	$("#lbEstados").text("Estado");

	$("#chkSucursales").prop('checked', false);
	$("#chkSucursales").trigger("change");
	$("#chkEstados").prop('checked', false);
	$("#chkEstados").trigger("change");

	$("#listaSucursales").val("");
	$("#listaEstados").val("");

	$("#divFiltros").collapse("show");
	$("#divDetalle").collapse("hide");

	let sucSele = $("#SucursalSeleccionada").val();
	$("#listaSucursales").val(sucSele);

	setTimeout(() => {
		let habilitado = $("#HabilitarCambioDeSucursalSeleccionada").val();
		if ($("#HabilitarCambioDeSucursalSeleccionada").val() == "False")
			$("#divListaSucursales").find("input, select, textarea, button").prop("disabled", true);
		else
			$("#divListaSucursales").find("input, select, textarea, button").prop("disabled", false);
	}, 500);

	if (!vieneDeCancelar) {
		HandlerCheckBox();
	}
}

function HandlerCheckBox() {
	$("#chkSucursales").on("click", function () {
		if ($("#chkSucursales").is(":checked")) {
			$("#listaSucursales").prop("disabled", false);
			$("#listaSucursales").trigger("focus");
		}
		else {
			$("#listaSucursales").prop("disabled", true);
			$("#listaSucursales").val("");
		}
	});
	$("#chkEstados").on("click", function () {
		if ($("#chkEstados").is(":checked")) {
			$("#listaEstados").prop("disabled", false);
			$("#listaEstados").trigger("focus");
		}
		else {
			$("#listaEstados").prop("disabled", true);
			$("#listaEstados").val("");
		}
	});
}

function validarFechas() {
	let desde = $("#Desde").val();
	let hasta = $("#Hasta").val();

	if (!desde || !hasta) return false;

	let fechaDesde = new Date(desde);
	let fechaHasta = new Date(hasta);

	return !(fechaDesde > fechaHasta);
}

function ValidarFechasClick() {
	const desde = $("#Desde").val();
	const hasta = $("#Hasta").val();

	if (desde && hasta && desde > hasta) {
		AbrirMensaje("ATENCIÓN", "El valor de Fecha Desde no puede ser mayor a Fecha Hasta, revise.", function () {
			$("#msjModal").modal("hide");
			$("#Desde").val($("#Hasta").val());
			return true;
		}, false, ["Aceptar"], "error!", null);
	} else {
	}
}

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}