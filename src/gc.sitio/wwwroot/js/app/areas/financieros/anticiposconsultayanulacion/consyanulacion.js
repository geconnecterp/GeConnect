$(function () {
	// patch: placeholder no-op to update file for next edits
	$(document).on("click", "#btnImprimirDetalle", ImprimirDetalle);
	$(document).on("click", "#btnImprimirVales", ImprimirVales);
	$(document).on("click", "#btnCancelar", ControlaCancelar);
	$(document).on("click", "#btnAnularAntic", ControlaAnularAnticipo);

	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		cargaPaginacion();
	});

	$("#chkDesdeHasta").prop('checked', true);
	$("#chkDesdeHasta").trigger("change");
	$("#chkDesdeHasta").prop("disabled", true);

	$(document).on("change", "#Date1, #Date2", function () {
		const d1 = $("#Date1").val();
		const d2 = $("#Date2").val();
		console.log(d1);
		console.log(d2);

		// Solo sigo si ambas fechas son “válidas de negocio”
		if (!esFechaValidaFiltro(d1) || !esFechaValidaFiltro(d2)) {
			return;
		}

		validarRangoFechas();
	});

	InicializarCamposEnFiltros();

	$(document).on("change", "#listaUsuario", ControlalistaUsuSelected);
	$("#UsuarioList").on("dblclick", 'option', function () { $(this).remove(); })

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
	$("input#Rel01").on("click", function () {
		$("input#Rel01").val("");
		$("#Rel01Item").val("");
	});

	$("#btnBuscar").on("click", function () {
		// Actualiza visualización de filtros antes de buscar
		MostrarFiltrosAplicados();
		dataBak = "";
		pagina = 1;
		BuscarAnticiposDeEmpleados(pagina);
	});

	$("#Rel01List").on("dblclick", 'option', function () { $(this).remove(); })
	$("#UsuarioList").on("dblclick", 'option', function () { $(this).remove(); })

	funcCallBack = BuscarAnticiposDeEmpleados;
	// Mostrar filtros iniciales cargados desde el servidor
	MostrarFiltrosAplicados();
});


function MostrarFiltrosAplicados() {
	try {
		// Buscar el contenedor flotante primero; si no existe, usar el container normal
		const cont = $("#filtrosAplicadosFloating");
		const fallback = $("#filtrosAplicadosContainer");
		const target = cont.length ? cont : fallback;
		if (!target.length) return;
		if (!cont.length) return;

		const desde = $("#Date1").val();
		const hasta = $("#Date2").val();
		const tipoText = $("#listaTipo option:selected").text() || "Todos";

		const clientes = listFrom("Rel01List");
		const usuarios = listFrom("UsuarioList");

		let html = '<div class="d-inline-flex align-items-center" style="gap:8px;white-space:nowrap;">';
		if (desde) html += `<span class="badge bg-secondary">Desde: ${desde}</span>`;
		if (hasta) html += `<span class="badge bg-secondary">Hasta: ${hasta}</span>`;
		if (tipoText) html += `<span class=\"badge bg-secondary me-1\">Tipo: ${tipoText}</span>`;

		html += renderGroup('CLI', clientes);
		html += renderGroup('USU', usuarios);
		html += '</div>';

		target.html(html);
	} catch (e) {
		console.error('MostrarFiltrosAplicados error', e);
	}
}

function esFechaValidaFiltro(valor) {
	if (!valor || valor.length !== 10) return false; // formato esperado yyyy-MM-dd

	const partes = valor.split("-");
	if (partes.length !== 3) return false;

	const anio = parseInt(partes[0], 10);
	const mes = parseInt(partes[1], 10);
	const dia = parseInt(partes[2], 10);

	// rango de años aceptable para tu negocio
	if (isNaN(anio) || anio < 1900 || anio > 2500) return false;
	if (isNaN(mes) || mes < 1 || mes > 12) return false;
	if (isNaN(dia) || dia < 1 || dia > 31) return false;

	return true;
	//const f = new Date(valor);
	//if (isNaN(f.getTime())) return false;

	//// chequeo cruzado por seguridad
	//return f.getFullYear() === anio &&
	//	(f.getMonth() + 1) === mes &&
	//	f.getDate() === dia;
}


function ControlalistaUsuSelected() {
	var item = $("#listaUsuario").val();
	var desc = $("#listaUsuario option:selected").text();
	if ($("#UsuarioList").has('option:contains("' + item + '")').length === 0 && $("#UsuarioList").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#UsuarioList").append(opc);
	}
}

function validarRangoFechas() {
	const desde = $("#Date1").val();
	const hasta = $("#Date2").val();

	if (desde && hasta) {
		const fDesde = new Date(desde);
		const fHasta = new Date(hasta);

		if (fDesde > fHasta) {
			/*alert("La fecha 'Desde' no puede ser mayor que la fecha 'Hasta'.");*/
			$("#Date1").trigger("focus");
		}
		else {
			ActualizarListaDeUsuarios(desde, hasta);
		}
	}
}

function ActualizarListaDeUsuarios(desde, hasta) {
	console.log("ActualizarListaDeUsuarios");
	var data = { desde, hasta };
	PostGenHtml(data, cargarUsuariosUrl, function (obj) {
		$("#divUsuarios").html(obj);
		if ($("#chkUsuario").is(":checked")) {
			console.log("Está chequeado");
			$("#listaUsuario").prop("disabled", false);
			$("#UsuarioList").prop("disabled", false);
		} else {
			console.log("NO está chequeado");
			$("#listaUsuario").prop("disabled", true);
			$("#UsuarioList").prop("disabled", true);
		}
		CerrarWaiting();
		return true
	});
}

function ControlaAnularAnticipo() {
	if (an_compte_selected == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un comprobante para imprimir.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirMensaje("ATENCIÓN", `¿Esta seguro que desea anular el anticipo seleccionado N° ${an_compte_selected}?`, function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI":
					handlerAnularAnticipo(an_compte_selected);
					break;
				case "NO":
					break;
				default: //NO
					break;
			}
			return true;

		}, true, ["Aceptar", "Cancelar"], "question!", null);
	}
}

function handlerAnularAnticipo(anCompte) {
	var data = { anCompte };
	PostGen(data, financieroAnticipoAnularUrl, function (obj) {
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
			AbrirMensaje("ÉXITO", obj.msg, function () {
				$("#msjModal").modal("hide");
				var pagina = 1;
				BuscarAnticiposDeEmpleados(pagina);
				return true;
			}, false, ["Aceptar"], "success!", null);
		}
	});
}

function ImprimirDetalle() {
	var filas = $("#tbGridAnticipoFinEmpDetalle tbody tr").length;
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
				HandlerImprimirDetalle();
			}
		});
	}
}

function HandlerImprimirDetalle() {
	ReseteoDeReportes();
	setTimeout(() => {
		var id = an_compte_selected;
		let data = { id };
		cargarReporteEnArre(40, data, "DETALLE DE ANTICIPO", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ImprimirVales() {
	var filas = $("#tbGridAnticipoFinEmp tbody tr").length;
	if (filas == 0) {
		AbrirMensaje("ATENCIÓN", "No hay datos para imprimir.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else if (an_compte_selected == "") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un comprobante para imprimir.", function () {
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
				HandlerImprimirVales();
			}
		});
	}
}

function HandlerImprimirVales() {
	ReseteoDeReportes();
	setTimeout(() => {
		var id = an_compte_selected;
		let data = { id };
		cargarReporteEnArre(39, data, "ANTICIPO DE EMPLEADO", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ControlaCancelar() {
	$("#divFiltros").removeClass("collapse").addClass("show");
	$("#divDetalle").collapse("hide");
	$("#tbGridAnticipoFinEmpDetalle tbody").empty();
	$("#tbGridAnticipoFinEmp tbody").empty();
	$(".leyenda-titulo").hide();
	InicializarDatosEnSesion();
	ResetDeFiltros();
}

function ResetDeFiltros() {
	$("#Rel01List").empty();
	$("#Rel01").val("");
	$("#listaTipo").val("");
	$("#listaUsuario").val("");
	$("#UsuarioList").empty();
	$("#chkUsuario").prop('checked', false);
	$("#chkUsuario").trigger("change");
	$("#chkTipo").prop('checked', false);
	$("#chkTipo").trigger("change");
	$("#chkRel01").prop('checked', false);
	$("#chkRel01").trigger("change");
	$("#listaTipo").prop("disabled", true);
	$("#listaUsuario").prop("disabled", true);
	$("#Rel01List").prop("disabled", true);
	$("#UsuarioList").prop("disabled", true);
}

function InicializarDatosEnSesion() {
	var data = {};
	PostGen(data, inicializarDatosEnSesionUrl, function (obj) {
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
			BuscarAnticiposDeEmpleados(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltro").collapse("hide")
	return true;
}

function BuscarAnticiposDeEmpleados(pag) {
	AbrirWaiting("");
	var desde = $("#Date1").val();
	var hasta = $("#Date2").val();
	var cta_list = [];
	var tipo_list = [];
	var usu_list = [];
	if ($("#chkRel01").is(":checked")) {
		$("#Rel01List").children().each(function (i, item) { cta_list.push($(item).val()) });
	}
	if ($("#chkTipo").is(":checked")) {
		var tipoVal = $("#listaTipo").val();
		tipo_list.push(tipoVal);
	}
	if ($("#chkUsuario").is(":checked")) {
		$("#UsuarioList").children().each(function (i, item) { usu_list.push($(item).val()) });
	}
	var cta = $("#chkRel01")[0].checked;
	var tipo = $("#chkTipo")[0].checked;
	var usu = $("#chkUsuario")[0].checked;
	var data1 = { desde, hasta, cta_list, cta, tipo_list, tipo, usu_list, usu };
	var buscaNew = true;
	var sort = null;
	var sortDir = null
	pagina = pag;
	var data2 = { sort, sortDir, pag, buscaNew }
	var data = $.extend({}, data1, data2);
	PostGenHtml(data, buscarAnticiposDeEmpleadosURL, function (obj) {
		CerrarWaiting();
		$("#divAntFinanEmp").html(obj);
		inicializarEventosTablaAnticipoFinEmp();
		$("#divAntFinanEmpDetalle").empty();
		$("#divFiltros").removeClass("show").addClass("collapse");
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
				$("#divPaginacion").removeClass("collapse");
			}

		});
		an_compte_selected = "";
		CerrarWaiting();
		return true
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function selectReg(x, gridId) {
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
	if (gridId === "tbGridAnticipoFinEmp") {
		let anCompte = x.childNodes[1].innerText;
		an_compte_selected = anCompte;
		CargarDetalleDeAnticipo(anCompte);
	}
}

function selectRegDbl(x, gridId) {
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
	});
	$(x).addClass("selected-row");
	let anCompte = x.childNodes[1].innerText;
	an_compte_selected = anCompte;
	CargarDetalleDeAnticipo(anCompte);
	AbrirWaiting();
	setTimeout(function () {
		CerrarWaiting();
		irAlTabDetalle();
	}, 500);
}

function irAlTabDetalle() {
	var tabDetalle = document.getElementById("btnTabAntFinanEmpDetalle");
	var tab = new bootstrap.Tab(tabDetalle);
	tab.show();
}

function CargarDetalleDeAnticipo(anCompte) {
	AbrirWaiting(`Cargando detalle de Anticipo N° ${anCompte}`)
	var data = { anCompte };
	PostGenHtml(data, cargarDetalleDeAnticipoUrl, function (obj) {
		CerrarWaiting();
		const header = `
				<div class="card mb-2">
					<div class="card-body py-2 d-flex align-items-center gap-4">
						<div>
							<i class="bx bx-file me-1"></i>
							<strong>Detalle de Cuentas del Anticipo N°:</strong> ${anCompte}
						</div>
					</div>
				</div>
			`;
		$("#divAntFinanEmpDetalle").html(header + obj);
		return true
	}, function (obj) {
		CerrarWaiting();
		console.log(obj);
		ControlaMensajeError(obj.responseText);
	});
}

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function InicializarCamposEnFiltros() {
	$("#Date1, #Date2").on("blur", ValidarFechasClick);
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
	$("#chkTipo").on("click", function () {
		if ($("#chkTipo").is(":checked")) {
			$("#listaTipo").prop("disabled", false);
			$("#listaTipo").trigger("focus");
		}
		else {
			$("#listaTipo").val("");
			$("#listaTipo").prop("disabled", true);
		}
	});
	$("#chkUsuario").on("click", function () {
		if ($("#chkUsuario").is(":checked")) {
			$("#listaUsuario").prop("disabled", false);
			$("#UsuarioList").prop("disabled", false);
			$("#listaUsuario").trigger("focus");
		}
		else {
			$("#listaUsuario").prop("disabled", true);
			$("#UsuarioList").prop("disabled", true);
			$("#listaUsuario").val("");
			$("#UsuarioList").empty();
		}
	});

	$("#lbChkDesdeHasta").text("Desde / Hasta");
	$("#lbRel01").text("Cliente");
	$("#lbTipo").text("Tipo");
	$("#lbUsuario").text("Usuario");

	$("#Date1").prop("disabled", false);
	$("#Date2").prop("disabled", false);
	$("#divFiltros").collapse("show");
	$("#divDetalle").collapse("hide");
}

$("#Rel01").autocomplete({
	source: function (request, response) {

		data = { prefix: request.term }; /*Rel01*/

		$.ajax({
			url: autoComRel01Url,
			type: "POST",
			dataType: "json",
			data: data,
			success: function (obj) {
				response($.map(obj, function (item) {
					return normalizarClienteAutocomplete(item);
				}));
			}
		})
	},
	minLength: 3,
	select: function (event, ui) {
		ctaIdSelected = ui.item.id;
		ctaDescSelected = ui.item.value;
		if ($("#Rel01List").has('option:contains("' + ui.item.id + '")').length === 0) {
			$("#Rel01Item").val(ui.item.id);
			var opc = "<option value=" + ui.item.id + ">" + ui.item.value + "</option>"
			$("#Rel01List").append(opc);
		}
		return true;
	}
});
aplicarRenderClienteAutocomplete($("#Rel01"));

function ValidarFechasClick() {
	const desde = $("#Date1").val();
	const hasta = $("#Date2").val();

	if (desde && hasta && desde > hasta) {
		AbrirMensaje("ATENCIÓN", "El valor de Fecha Desde no puede ser mayor a Fecha Hasta, revise.", function () {
			$("#msjModal").modal("hide");
			$("#Date1").val($("#Date2").val());
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}

function inicializarEventosTablaAnticipoFinEmp() {
	let clickTimer = null;

	// CLICK
	$(document)
		.off("click", "#tbGridAnticipoFinEmp tbody tr")
		.on("click", "#tbGridAnticipoFinEmp tbody tr", function () {

			const row = this;

			if (clickTimer) {
				clearTimeout(clickTimer);
				clickTimer = null;
				return;
			}

			clickTimer = setTimeout(function () {
				clickTimer = null;
				selectReg(row, "tbGridAnticipoFinEmp");   // CLICK NORMAL
			}, 200);
		});

	// DOUBLE CLICK
	$(document)
		.off("dblclick", "#tbGridAnticipoFinEmp tbody tr")
		.on("dblclick", "#tbGridAnticipoFinEmp tbody tr", function () {

			if (clickTimer) {
				clearTimeout(clickTimer);
				clickTimer = null;
			}

			selectRegDbl(this, "tbGridAnticipoFinEmp");  // DOBLE CLICK
		});
}
