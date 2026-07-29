$(function () {
	$("#chkDesdeHasta").prop('checked', true);
	$("#chkDesdeHasta").trigger("change");
	$("#chkDesdeHasta").prop("disabled", true);

	InicializarCamposEnFiltros();

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

	

	// intentar mostrar al cargar
	try { MostrarFiltrosAplicados(); } catch (e) { }
	$("input#Rel01").on("click", function () {
		$("input#Rel01").val("");
		$("#Rel01Item").val("");
	});

	$("#btnBuscar").on("click", function () {
		try { MostrarFiltrosAplicados(); } catch (e) { console.warn('MostrarFiltrosAplicados no disponible:', e); }
		BuscarChequesPropiosEmitidos();
	});

	$(document).on("change", "#listaCB", ControlalistaCuentaBancoSelected);
	$(document).on("change", "#listaUsu", ControlalistaUsuarioSelected);
	$(document).on("change", "#listaEst", ControlalistaEstadoSelected);
	$(document).on("click", "#btnArchivoECheq", ControlaPasoPrevioECheqSelected);
	$(document).on("click", "#btnImprimir", ControlaImprimirSelected);
	//
	$(document).on("click", "#btnChequeModificar", GuardarChequeModificar);

	$("#CBList").on("dblclick", 'option', function () { $(this).remove(); })
	$("#UsuList").on("dblclick", 'option', function () { $(this).remove(); })
	$("#EstList").on("dblclick", 'option', function () { $(this).remove(); })
});

function MostrarFiltrosAplicados() {
	try {
		// preferir un contenedor flotante si existe, si no usar el container dentro del collapse
		const floatCont = $("#filtrosAplicadosFloating");
		const fallback = $("#filtrosAplicadosContainer");
		const cont = floatCont.length ? floatCont : (fallback.length ? fallback : null);
		if (!cont) return;

		const desde = $("#Date1").val();
		const hasta = $("#Date2").val();
		const tipo_fecha = $("#radioSection input[name='opcion']:checked").parent().text().trim();

		// recoger listas seleccionadas
		const cbs = [];
		$("#CBList option").each(function () { cbs.push($(this).text()); });
		const clientes = [];
		$("#Rel01List option").each(function () { clientes.push($(this).text()); });
		const usuarios = [];
		$("#UsuList option").each(function () { usuarios.push($(this).text()); });
		const estados = [];
		$("#EstList option").each(function () { estados.push($(this).text()); });

		let html = "";
		html += `<span class=\"badge bg-secondary me-1\">DESDE: ${desde || '-'} </span>`;
		html += `<span class=\"badge bg-secondary me-1\">HASTA: ${hasta || '-'} </span>`;
		if (tipo_fecha) html += `<span class=\"badge bg-secondary me-1\">TIPO: ${tipo_fecha}</span>`;

		function makeListBadge(label, items, id) {
			if (!items || items.length === 0) return '';
			if (items.length === 1) return `<span class=\"badge bg-secondary me-1\">${label}: ${items[0]}</span>`;
			let s = `<div class=\"dropdown me-1\">`;
			s += `<button class=\"badge bg-secondary dropdown-toggle text-nowrap\" type=\"button\" id=\"${id}\" data-bs-toggle=\"dropdown\" aria-expanded=\"false\">${label}: ${items.length} seleccionados</button>`;
			s += `<ul class=\"dropdown-menu dropdown-menu-end\" aria-labelledby=\"${id}\" data-bs-boundary=\"viewport\">`;
			items.forEach(function (it) { s += `<li><a class=\"dropdown-item\" href=\"#\">${it}</a></li>`; });
			s += `</ul></div>`;
			return s;
		}

		html += makeListBadge('CUENTA', cbs, 'cbDrop');
		html += makeListBadge('CLIENTE', clientes, 'cliDrop');
		html += makeListBadge('USUARIO', usuarios, 'usuDrop');
		html += makeListBadge('ESTADO', estados, 'estDrop');

		cont.html(html);
	} catch (e) {
		console.error('MostrarFiltrosAplicados error', e);
	}
}

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function ControlaImprimirSelected() {
	if ($("#tbListaDetalleCheques > tbody > tr").length === 0) {
		AbrirMensaje("ATENCIÓN", "No hay datos generar el reporte.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		ImprimirListaCheque_Generada();
	}
}

function ImprimirListaCheque_Generada() {
	ReseteoDeReportes();
	setTimeout(() => {
		var id_f_bool = $("#chkCB").is(":checked");
		var id_c_bool = $("#chkRel01").is(":checked");
		var id_u_bool = $("#chkUsu").is(":checked");
		var id_e_bool = $("#chkEst").is(":checked");

		var $opcion = $("#CBList option").first();
		var id_f = $opcion.val() ?? "";
		var id_f_texto = $opcion.text()?.trim() ?? "";

		$opcion = $("#Rel01List option").first();
		var id_c = $opcion.val() ?? "";
		var id_c_texto = $opcion.text()?.trim() ?? "";

		$opcion = $("#UsuList option").first();
		var id_u = $opcion.val() ?? "";
		var id_u_texto = $opcion.text()?.trim() ?? "";

		$opcion = $("#EstList option").first();
		var id_e = $opcion.val() ?? "";
		var id_e_texto = $opcion.text()?.trim() ?? "";

		var desde = $("#Date1").val();
		var hasta = $("#Date2").val();
		var desde1Print = moment($("#Date1").val()).format('DD/MM/yyyy')
		var hasta2Print = moment($("#Date2").val()).format('DD/MM/yyyy')

		var tipo_fecha = $("#radioSection input[name='opcion']:checked").val();
		var tipo_fecha_texto = $("#radioSection input[name='opcion']:checked").parent().text().trim();
		let data = {
			id_f_bool, id_f, id_f_texto,
			id_e_bool, id_e, id_e_texto,
			id_c_bool, id_c, id_c_texto,
			id_u_bool, id_u, id_u_texto,
			tipo_fecha, tipo_fecha_texto,
			desde, desde1Print,
			hasta, hasta2Print
		};
		cargarReporteEnArre(33, data, "CHEQUE PROPIO EMITIDO", "", "");
		invocacionGestorDoc({});
	}, 500);
}

function ControlaPasoPrevioECheqSelected() {
	//Obtenemos los cheques seleccionados
	var esOK = true;
	const seleccionados = window.obtenerChequesSeleccionados();
	if (!ValidarSiAlMenosUnaFilaTieneUnCheckBox()) {
		AbrirMensaje("ATENCIÓN", "No existen Cheques habilitados para generar el archivo.", function () {
			$("#msjModal").modal("hide");
			esOK = false;
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else if (seleccionados.length <= 0) {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar al menos un cheque.", function () {
			$("#msjModal").modal("hide");
			esOK = false;
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirMensaje("ATENCIÓN", "¿Esta seguro que desea generar el archivo?", function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI": //Confirmar la entrega
					GenerarArchivo(seleccionados);
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

function GenerarArchivo(seleccionados) {
	//Obtenemos el json en formato string
	var json_che = ObtenerJSonDesdeECheqSeleccionados(seleccionados);
	var data = { json_che };
	PostGen(data, pasoPrevioECheqUrl, function (obj) {
		$("#modalAgregarProducto").modal("hide");
		if (obj.error === true) {
			CerrarWaiting();
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
						descargarTXTDesdeJSON(obj.json, generarNombreArchivo(obj.formato));
						break;
					case "XLS":
						descargarXLSPlanoDesdeJSON(obj.json, generarNombreArchivo(obj.formato));
						break;
					default:
				}
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

function ValidarSiAlMenosUnaFilaTieneUnCheckBox() {
	const $checkboxes = $("#tbListaDetalleCheques tbody input.check-cheque");
	const hayAlMenosUnoMarcado = $checkboxes.length > 0 /*&& $checkboxes.filter(":checked").length > 0*/;
	console.log("¿Hay al menos un checkbox renderizado y marcado?", hayAlMenosUnoMarcado);
	return hayAlMenosUnoMarcado;
}

function generarNombreArchivo(extension = "csv") {
	const fecha = new Date();
	const timestamp = fecha.toISOString().replace(/[-:.TZ]/g, "").slice(0, 14);
	const random = Math.random().toString(36).substring(2, 10);
	return `archivo_${timestamp}_${random}.${extension}`;
}

function descargarXLSPlanoDesdeJSON(jsonData, nombreArchivo = "datos.xls") {
	if (typeof jsonData === "string") {
		jsonData = JSON.parse(jsonData);
	}

	if (!Array.isArray(jsonData) || jsonData.length === 0) {
		console.warn("El JSON está vacío o no es un array.");
		return;
	}

	// Convertir JSON a hoja de cálculo
	const hoja = XLSX.utils.json_to_sheet(jsonData);

	// Crear libro
	const libro = XLSX.utils.book_new();
	XLSX.utils.book_append_sheet(libro, hoja, "Cheques");

	// Exportar
	XLSX.writeFile(libro, nombreArchivo);

	//if (typeof jsonData === "string") {
	//	jsonData = JSON.parse(jsonData);
	//}

	//if (!Array.isArray(jsonData) || jsonData.length === 0) {
	//	console.warn("El JSON está vacío o no es un array.");
	//	return;
	//}

	//const headers = Object.keys(jsonData[0]);
	//const filas = jsonData.map(obj =>
	//	headers.map(header => `"${(obj[header] ?? "").toString().replace(/"/g, '""')}"`).join("\t")
	//);

	//const BOM = "\uFEFF"; // UTF-8 BOM
	//const contenidoXLS = [headers.join("\t"), ...filas].join("\r\n");

	//const blob = new Blob([BOM + contenidoXLS], { type: "application/vnd.ms-excel;charset=utf-8;" });
	//const link = document.createElement("a");
	//link.href = URL.createObjectURL(blob);
	//link.download = nombreArchivo;
	//document.body.appendChild(link);
	//link.click();
	//document.body.removeChild(link);
}


function descargarTXTDesdeJSON(jsonData, nombreArchivo = "datos.txt") {
	if (!jsonData || !jsonData.length) return;

	const headers = Object.keys(jsonData[0]);
	const filas = jsonData.map(obj =>
		headers.map(header => `${header}: ${(obj[header] ?? "").toString()}`).join(" | ")
	);

	const contenidoTXT = filas.join("\r\n");

	const blob = new Blob([contenidoTXT], { type: "text/plain;charset=utf-8;" });
	const link = document.createElement("a");
	link.href = URL.createObjectURL(blob);
	link.download = nombreArchivo;
	document.body.appendChild(link);
	link.click();
	document.body.removeChild(link);
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


function ObtenerJSonDesdeECheqSeleccionados(lista) {
	const jsonArray = lista.map(item => {
		const [ctaf_id, che_emision] = item.split("|");
		return { ctaf_id, che_emision };
	});

	const jsonString = JSON.stringify(jsonArray);
	console.log(jsonString);
	return jsonString;
}

function ControlalistaCuentaBancoSelected() {
	var item = $("#listaCB").val();
	var desc = $("#listaCB option:selected").text();
	$("#CBList").empty();
	var opc = "<option value=" + item + ">" + desc + "</option>"
	$("#CBList").append(opc);
}

function ControlalistaUsuarioSelected() {
	var item = $("#listaUsu").val();
	var desc = $("#listaUsu option:selected").text();
	$("#UsuList").empty();
	var opc = "<option value=" + item + ">" + desc + "</option>"
	$("#UsuList").append(opc);
}

function ControlalistaEstadoSelected() {
	var item = $("#listaEst").val();
	var desc = $("#listaEst option:selected").text();
	$("#EstList").empty();
	var opc = "<option value=" + item + ">" + desc + "</option>"
	$("#EstList").append(opc);
}

function BuscarChequesPropiosEmitidos() {
	var tipo_fecha = $("#radioSection input[name='opcion']:checked").val();
	if (tipo_fecha == undefined) {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un Tipo de Fecha.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting();
		var id_f = $("#chkCB").is(":checked");
		var id_c = $("#chkRel01").is(":checked");
		var id_u = $("#chkUsu").is(":checked");
		var id_e = $("#chkEst").is(":checked");
		var ctaf_id = $("#listaCB").val();
		var cta_id = $("#Rel01Item").val();
		var usu_id = $("#listaUsu").val();
		var desde = $("#Date1").val();
		var hasta = $("#Date2").val();
		var estado = $("#listaEst").val();
		var data = { id_f, ctaf_id, id_c, cta_id, id_u, usu_id, tipo_fecha, desde, hasta, estado };
		PostGenHtml(data, buscarChequesPropiosEmitidosUrl, function (obj) {
			$("#divChequesPropiosEmitidos").html(obj);
			// actualizar filtros aplicados (si el partial reemplaza el DOM)
			try { MostrarFiltrosAplicados(); } catch (e) { console.warn('MostrarFiltrosAplicados no disponible:', e); }
			$("#divFiltros").collapse("hide");
			$("#divDetalle").collapse("show");
			CerrarWaiting();
			return true
		});
	}
}

function InicializarCamposEnFiltros() {
	$("#Date1, #Date2").on("blur", ValidarFechasClick);
	$("#chkCB").on("click", function () {
		if ($("#chkCB").is(":checked")) {
			$("#listaCB").prop("disabled", false);
			$("#CBList").prop("disabled", false);
			$("#listaCB").trigger("focus");
		}
		else {
			$("#listaCB").prop("disabled", true);
			$("#CBList").prop("disabled", true);
			$("#listaCB").val("");
			$("#CBList").empty();
		}
	});
	$("#chkUsu").on("click", function () {
		if ($("#chkUsu").is(":checked")) {
			$("#listaUsu").prop("disabled", false);
			$("#UsuList").prop("disabled", false);
			$("#listaUsu").trigger("focus");
		}
		else {
			$("#listaUsu").prop("disabled", true);
			$("#UsuList").prop("disabled", true);
			$("#listaUsu").val("");
			$("#UsuList").empty();
		}
	});
	$("#chkEst").on("click", function () {
		if ($("#chkEst").is(":checked")) {
			$("#listaEst").prop("disabled", false);
			$("#EstList").prop("disabled", false);
			$("#listaEst").trigger("focus");
		}
		else {
			$("#listaEst").prop("disabled", true);
			$("#EstList").prop("disabled", true);
			$("#listaEst").val("");
			$("#EstList").empty();
		}
	});
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

	$("#lbChkDesdeHasta").text("Desde / Hasta");
	$("#lbCB").text("Cuenta Banco");
	$("#lbRel01").text("Proveedor");
	$("#lbUsu").text("Usuario");
	$("#lbEst").text("Estado");

	$("#Date1").prop("disabled", false);
	$("#Date2").prop("disabled", false);
	$("#divFiltros").collapse("show");
	$("#divDetalle").collapse("hide");
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
		return true;
	}
});

function verDetalleModificado(ctaf_id, che_emision) {
	console.log("Ver detalle de cheque modificado:", ctaf_id, che_emision);
	AbrirWaiting();
	var datos = { ctaf_id, che_emision };
	PostGenHtml(datos, mostrarChequeModificadoUrl, function (obj) {
		$("#divChequeModificado").html(obj);
		$('#modalChequeModificado').modal({
			backdrop: 'static',
		});
		$('#modalChequeModificado').modal('show');

		CerrarWaiting();
		return true
	});
}

function verModalDetalleChequeModificar(ctaf_id, che_emision, che_nro, che_fecha, che_anombre) {
	console.log("Ver detalle de cheque modificar:", ctaf_id, che_emision, che_nro, che_fecha, che_anombre);
	// Lógica para mostrar modal o cargar datos
	AbrirWaiting();
	var datos = { ctaf_id, che_emision, che_nro, che_fecha, che_anombre };
	PostGenHtml(datos, verModalDetalleChequeModificarUrl, function (obj) {
		$("#divChequeModificar").html(obj);
		$('#modalChequeModificar').modal({
			backdrop: 'static',
		});

		const fechaFormateada = moment(che_fecha, 'DD/MM/YYYY HH:mm:ss').format('YYYY-MM-DD');
		$("#che_fecha").val(fechaFormateada);
		$('#modalChequeModificar').modal('show');

		CerrarWaiting();
		return true
	});
}

function GuardarChequeModificar() {
	var esValido = true;
	var ctaf_id = $("#ctaf_id").val();
	var che_emision = $("#che_emision").val();
	var che_nro = $("#che_nro").val();
	if (che_nro == "") {
		esValido = false;
		AbrirMensaje("ATENCIÓN", "Debe indicar un número de cheque válido.", function () {
			$("#msjModal").modal("hide");
			$("#che_nro").trigger("focus");
			return false;
		}, false, ["Aceptar"], "error!", null);
	}
	var che_fecha = $("#che_fecha").val();
	if (che_fecha == "") {
		esValido = false;
		AbrirMensaje("ATENCIÓN", "Debe indicar una fecha válida.", function () {
			$("#msjModal").modal("hide");
			$("#che_fecha").trigger("focus");
			return false;
		}, false, ["Aceptar"], "error!", null);
	}
	var che_anombre = $("#che_anombre").val();
	if (che_anombre == "") {
		esValido = false;
		AbrirMensaje("ATENCIÓN", "Debe indicar un valor válido para A Nombre de.", function () {
			$("#msjModal").modal("hide");
			$("#che_anombre").trigger("focus");
			return false;
		}, false, ["Aceptar"], "error!", null);
	}
	if (esValido) {
		AbrirWaiting();
		var data = { ctaf_id, che_emision, che_nro, che_fecha, che_anombre };
		PostGen(data, guardarChequeModificarUrl, function (obj) {
			$("#modalAgregarProducto").modal("hide");
			if (obj.error === true) {
				CerrarWaiting();
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				$("#modalChequeModificar").modal("hide");
				CerrarWaiting();
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					guardarSeleccionados();
					ActualizarListaCheques();
					return true;
				}, false, ["Aceptar"], "succ!", null);

			}
		});
	}

}

function ActualizarListaCheques() {
	AbrirWaiting();
	var id_f = $("#chkCB").is(":checked");
	var id_c = $("#chkRel01").is(":checked");
	var id_u = $("#chkUsu").is(":checked");
	var id_e = $("#chkEst").is(":checked");
	var ctaf_id = $("#listaCB").val();
	var cta_id = $("#Rel01Item").val();
	var usu_id = $("#listaUsu").val();
	var desde = $("#Date1").val();
	var hasta = $("#Date2").val();
	var estado = $("#listaEst").val();
	var tipo_fecha = $("#radioSection input[name='opcion']:checked").val();
	var data = { id_f, ctaf_id, id_c, cta_id, id_u, usu_id, tipo_fecha, desde, hasta, estado };
	PostGenHtml(data, buscarChequesPropiosEmitidosUrl, function (obj) {
		$("#divChequesPropiosEmitidos").html(obj);
		restaurarSeleccionados();
		CerrarWaiting();
		return true
	});
}

function verDetalleEntrega(ctaf_id, che_nro, che_emision) {
	console.log("Ver detalle de cheque:", ctaf_id);
	// Lógica para mostrar modal o cargar datos
	AbrirMensaje("ATENCIÓN", `¿Esta seguro que desea registrar la entrega el cheque seleccionado? N°: ${che_nro}`, function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI": //Confirmar la entrega
				SetFechaDeEntrega(ctaf_id, che_emision);
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;

	}, true, ["Aceptar", "Cancelar"], "question!", null);
}

function verDetalleRechazar(ctaf_id, che_nro, che_emision) {
	console.log("Ver detalle de cheque:", ctaf_id, che_nro, che_emision);
	// Lógica para mostrar modal o cargar datos
	AbrirMensaje("ATENCIÓN", `¿Esta seguro que desea rechazar el cheque seleccionado? N°: ${che_nro}`, function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI": //Confirmar la entrega
				SetRechazarCheque(ctaf_id, che_emision);
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;

	}, true, ["Aceptar", "Cancelar"], "question!", null);
}

function SetRechazarCheque(ctaf_id, che_emision) {
	AbrirWaiting();
	var data = { ctaf_id, che_emision };
	PostGen(data, registrarRechazoDeChequeUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			guardarSeleccionados();
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			AbrirMensaje("ATENCIÓN", "El rechazo se confirmó exitósamente.", function () {
				$("#msjModal").modal("hide");
				guardarSeleccionados();
				ActualizarListaCheques();
				return true;
			}, false, ["Aceptar"], "succ!", null);
		}
	});
}

function SetFechaDeEntrega(ctaf_id, che_emision) {
	AbrirWaiting();
	var data = { ctaf_id, che_emision };
	PostGen(data, registrarFechaDeEntrega2Url, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {

			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				guardarSeleccionados();
				ActualizarListaCheques();
				return true;
			}, false, ["Aceptar"], "succ!", null);

		}
	});
}

function guardarSeleccionados() {
	const seleccionados = window.obtenerChequesSeleccionados();
	chequesSeleccionados = JSON.stringify(seleccionados);
	console.log("chequesSeleccionados", chequesSeleccionados)
}

function restaurarSeleccionados() {
	const seleccionados = JSON.parse(chequesSeleccionados || "[]");

	$(".check-cheque").each(function () {
		const cheEmision = $(this).data("che-emision");
		const ctafId = $(this).data("ctaf-id");
		const clave = `${ctafId}|${cheEmision}`;

		if (seleccionados.includes(clave)) {
			$(this).prop("checked", true);
			$(this).closest("tr").addClass("selected");
		}
	});

	// Actualizar el checkbox principal si corresponde
	const totalCheckboxes = $(".check-cheque").length;
	const totalChecked = $(".check-cheque:checked").length;
	$("#checkAllCheque").prop("checked", (totalCheckboxes === totalChecked && (totalCheckboxes != 0 && totalChecked != 0)));
}