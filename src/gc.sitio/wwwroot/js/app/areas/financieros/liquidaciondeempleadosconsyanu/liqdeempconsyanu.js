$(function () {

	$(document).on("click", "#btnImprimirDetalle", ImprimirDetalle);
	$(document).on("click", "#btnAnularLiqDeEmp", AnularLiquidacion);
	$(document).on("click", "#btnCancelar", ControlaCancelar);
	$(document).on("click", "#btnFileBco", AbrirModalArchivoBanco);
	$(document).on("click", "#btnConfirmar", ConfirmarArchivoBanco);
	//

	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		cargaPaginacion();
	});

	InicializarCamposEnFiltros();

	$("#chkDesdeHasta").prop('checked', true);
	$("#chkDesdeHasta").trigger("change");
	$("#chkDesdeHasta").prop("disabled", true);

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

	$("#btnBuscar").on("click", function () {
		// Actualizar visualización de filtros antes de buscar
		try { MostrarFiltrosAplicados(); } catch (e) { console.warn('MostrarFiltrosAplicados no disponible:', e); }
		dataBak = "";
		pagina = 1;
		BuscarLiquidacionDeEmpleados(pagina);
	});

	funcCallBack = BuscarLiquidacionDeEmpleados;
});
try { MostrarFiltrosAplicados(); } catch (e) { console.warn('MostrarFiltrosAplicados no disponible:', e); }

function AbrirModalArchivoBanco() {
	if (le_compte_selected == "" || le_compte_selected == null || le_compte_selected == undefined) {
		AbrirMensaje("ATENCIÓN", "Debe establecer un valor para Intereses, mayor o igual a 0.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting();
		var le_compte = le_compte_selected;
		var data = { le_compte };
		PostGenHtml(data, abrirModalArchivoBancoUrl, function (obj) {
			$("#divArchivoParaBanco").empty();
			$("#divArchivoParaBanco").html(obj);
			const $modal = $("#modalArchivoParaBanco");

			$modal.modal({
				backdrop: 'static',
			});

			CerrarWaiting();
			$modal.modal('show');

			setTimeout(() => {
				const $nroArchivo = $("#nro_archivo");
				if ($nroArchivo.length > 0) {
					$nroArchivo.trigger("focus");
					console.log("Foco aplicado a #nro_archivo");
				} else {
					console.warn("No se encontró el input #nro_archivo");
				}
			}, 500);

			return true
		});
	}
}

function ConfirmarArchivoBanco() {
	var nroArchivo = $("#nro_archivo").val();
	if (nroArchivo == "" || nroArchivo == null || nroArchivo == undefined) {
		AbrirMensaje("ATENCIÓN", "Debe indicar un Número de Archivo válido.", function () {
			$("#msjModal").modal("hide");
			$("#nro_archivo").trigger("focus");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirMensaje("ATENCIÓN", `¿Está seguro que desea generar el Archivo Banco?`, function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI":
					handlerGenerarArchivoBanco();
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

function handlerGenerarArchivoBanco() {
	var le_compte = $("#le_compte").val();
	var nro_file = $("#nro_archivo").val();
	var ctaf_id = obtenerTipoFiltroSeleccionado();
	AbrirWaiting(`Generando Archivo Banco para Liquidación N° ${le_compte}`);
	let data = { le_compte, nro_file, ctaf_id };
	PostGen(data, generarArchivoParaBancoUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			const formato = (obj.formato ?? "").toUpperCase();

			try {
				switch (formato) {
					case "CSV":
						descargarCSVDesdeJSON(obj.json, generarNombreArchivo(obj.formato), ";", obj.encabezado);
						break;
					case "TXT":
						descargarTXTDesdeJSON(obj.json, generarNombreArchivo(obj.formato), obj.encabezado);
						break;
					case "XLS":
						descargarXLSPlanoDesdeJSON(obj.json, generarNombreArchivo(obj.formato), obj.encabezado);
						break;
					default:
				}
				AbrirMensaje("ATENCIÓN", "El archivo se ha generado con éxito!", function () {
					$("#msjModal").modal("hide");
					$('#modalArchivoParaBanco').modal('hide');
					return true;
				}, false, ["Aceptar"], "succ!", null);
			} catch (e) {
				console.error("Error al generar el archivo:", e);
				AbrirMensaje("ERROR", "Ocurrió un problema al generar el archivo. Intente nuevamente.", function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);

			} finally {
				CerrarWaiting();
			}
		}
	});
}

function generarNombreArchivo(extension = "csv") {
	const fecha = new Date();
	const timestamp = fecha.toISOString().replace(/[-:.TZ]/g, "").slice(0, 14);
	const random = Math.random().toString(36).substring(2, 10);
	return `archivo_${timestamp}_${random}.${extension}`;
}

function descargarCSVDesdeJSON(jsonData, nombreArchivo = "datos.csv", separador = ",", incluirEncabezados = false) {
	if (typeof jsonData === "string") {
		jsonData = JSON.parse(jsonData);
	}

	if (!Array.isArray(jsonData) || jsonData.length === 0) {
		console.warn("El JSON está vacío o no es un array.");
		return;
	}

	const headers = Object.keys(jsonData[0]);
	const filas = jsonData.map(obj =>
		headers.map(header => `"${(obj[header] ?? "").toString().replace(/"/g, '""')}"`).join(separador)
	);

	const contenidoCSV = incluirEncabezados
		? [headers.join(separador), ...filas].join("\r\n")
		: filas.join("\r\n");

	const blob = new Blob([contenidoCSV], { type: "text/csv;charset=utf-8;" });
	const link = document.createElement("a");
	link.href = URL.createObjectURL(blob);
	link.download = nombreArchivo;
	document.body.appendChild(link);
	link.click();
	document.body.removeChild(link);

	console.log(`Archivo CSV generado (${incluirEncabezados ? "con" : "sin"} encabezados):`, nombreArchivo);
}

function descargarTXTDesdeJSON(jsonData, nombreArchivo = "datos.txt", incluirEncabezados = false) {
	if (!jsonData || !jsonData.length) return;

	jsonData = JSON.parse(jsonData);

	const headers = Object.keys(jsonData[0]);

	const filas = jsonData.map(obj =>
		headers.map(header => (obj[header] ?? "").toString()).join(" | ")
	);

	if (incluirEncabezados) {
		filas.unshift(headers.join(" | "));
	}

	const contenidoTXT = filas.join("\r\n");

	const blob = new Blob([contenidoTXT], { type: "text/plain;charset=utf-8;" });
	const link = document.createElement("a");
	link.href = URL.createObjectURL(blob);
	link.download = nombreArchivo;
	document.body.appendChild(link);
	link.click();
	document.body.removeChild(link);

}

function descargarXLSPlanoDesdeJSON(jsonData, nombreArchivo = "datos.xls", incluirEncabezados = false) {
	if (!jsonData || !jsonData.length) return;

	jsonData = JSON.parse(jsonData);
	const headers = Object.keys(jsonData[0]);

	const filas = jsonData.map(obj =>
		headers.map(header => obj[header] ?? "").join("\t")
	);

	if (incluirEncabezados) {
		filas.unshift(headers.join("\t"));
	}

	const contenidoXLS = filas.join("\r\n");

	const blob = new Blob([contenidoXLS], { type: "application/vnd.ms-excel;charset=utf-8;" });
	const link = document.createElement("a");
	link.href = URL.createObjectURL(blob);
	link.download = nombreArchivo;
	document.body.appendChild(link);
	link.click();
	document.body.removeChild(link);

}

function obtenerTipoFiltroSeleccionado() {
	const seleccionado = document.querySelector('input[name="tipoFiltro"]:checked');
	return seleccionado ? seleccionado.value : null;
}

function ControlaCancelar() {
	$("#divFiltros").removeClass("collapse").addClass("show");
	$("#divDetalle").collapse("hide");
	$("#tbGridLiqDeEmpDetalle tbody").empty();
	$("#tbGridLiqDeEmp tbody").empty();
	$(".leyenda-titulo").hide();
	InicializarDatosEnSesion();
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

function AnularLiquidacion() {
	if (le_compte_selected == "" || le_compte_selected == null || le_compte_selected == undefined) {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar una Liquidación.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else if (le_compte_anulada.toUpperCase() == "S") {
		AbrirMensaje("ATENCIÓN", "La Liquidación seleccionada ya se encuentra anulada.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirMensaje("ATENCIÓN", `¿Está seguro que desea anular la Liquidación N° ${le_compte_selected}?`, function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI":
					handlerAnularLiquidacion(le_compte_selected);
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

function handlerAnularLiquidacion(le_compte_selected) {
	AbrirWaiting(`Anulando Liquidación N° ${le_compte_selected}`);
	var id = le_compte_selected;
	let data = { id };
	PostGen(data, anularLiquidacionDeEmpleadoURL, function (obj) {
		CerrarWaiting();
		AbrirMensaje("ÉXITO", `La Liquidación N° ${le_compte_selected} ha sido anulada correctamente.`, function () {
			$("#msjModal").modal("hide");
			BuscarLiquidacionDeEmpleados(pagina);
			return true;
		}, false, ["Aceptar"], "success!", null);
		return true;
	}, function (obj) {
		CerrarWaiting();
		ControlaMensajeError(obj.responseText);
	});
}

function ImprimirDetalle() {
	var filas = $("#tbGridLiqDeEmpDetalle tbody tr").length;
	if (filas == 0) {
		AbrirMensaje("ATENCIÓN", "No hay datos para imprimir.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		ReseteoDeReportes();
		setTimeout(() => {
			var id = le_compte_selected;
			let data = { id };
			cargarReporteEnArre(41, data, "DETALLE DE LIQUIDACIÓN DE HABERES", "", "");
			invocacionGestorDoc({});
		}, 500);
	}
}

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function InicializarCamposEnFiltros() {
	$("#Date1, #Date2").on("blur", ValidarFechasClick);
	$("#lbChkDesdeHasta").text("Desde / Hasta");

	$("#Date1").prop("disabled", false);
	$("#Date2").prop("disabled", false);
	$("#divFiltros").collapse("show");
	$("#divDetalle").collapse("hide");
}

function MostrarFiltrosAplicados() {
	try {
		const cont = $("#filtrosAplicadosFloating");
		if (!cont || cont.length === 0) return;

		const desde = $("#Date1").val();
		const hasta = $("#Date2").val();

		let html = "";
		html += '<span class="badge bg-secondary me-1">DESDE: ' + (desde || '-') + ' </span>';
		html += '<span class="badge bg-secondary me-1">HASTA: ' + (hasta || '-') + ' </span>';

		cont.html(html);
	} catch (e) {
		console.error('MostrarFiltrosAplicados error', e);
	}
}

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

function cargaPaginacion() {
	$("#divPaginacion").pagination({
		items: totalRegs,
		itemsOnPage: pagRegs,
		cssStyle: "dark-theme",
		currentPage: pagina,
		onPageClick: function (num) {
			BuscarLiquidacionDeEmpleados(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltro").collapse("hide")
	return true;
}

function BuscarLiquidacionDeEmpleados(pag) {
	AbrirWaiting();
	var desde = $("#Date1").val();
	var hasta = $("#Date2").val();
	var data1 = { desde, hasta };
	var buscaNew = true;
	var sort = null;
	var sortDir = null
	pagina = pag;
	var data2 = { sort, sortDir, pag, buscaNew }
	var data = $.extend({}, data1, data2);
	PostGenHtml(data, buscarLiquidacionesDeEmpleadosURL, function (obj) {
		CerrarWaiting();
		$("#divLiqDeEmp").html(obj);
		// Actualizar filtros aplicados después de renderizar los resultados
		try { MostrarFiltrosAplicados(); } catch (e) { console.warn('MostrarFiltrosAplicados no disponible:', e); }
		$("#divLiqDeEmpDetalle").empty();
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
		le_compte_selected = "";
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
	if (gridId === "tbGridLiqDeEmp") {
		let leCompte = $(x).data("le-compte");
		let leCompteAnulada = $(x).data("le-anulada");
		le_compte_selected = leCompte;
		le_compte_anulada = leCompteAnulada;
		CargarDetalleDeLiquidacion(leCompte);
	}
}

function CargarDetalleDeLiquidacion(leCompte) {
	AbrirWaiting(`Cargando detalle de Liquidación N° ${leCompte}`)
	var data = { leCompte };
	PostGenHtml(data, cargarDetalleDeLiquidacionUrl, function (obj) {
		CerrarWaiting();
		const header = `
            <div class="card mb-2">
				<div class="card-body py-2 d-flex align-items-center gap-4">
					<div>
						<i class="bx bx-file me-1"></i>
						<strong>Detalle de Liquidación N°:</strong> ${leCompte}
					</div>
				</div>
			</div>
        `;
		$("#divLiqDeEmpDetalle").html(header + obj);
		return true
	}, function (obj) {
		CerrarWaiting();
		console.log(obj);
		ControlaMensajeError(obj.responseText);
	});
}