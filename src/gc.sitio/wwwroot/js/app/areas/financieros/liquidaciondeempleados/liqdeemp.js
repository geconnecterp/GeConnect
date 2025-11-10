const campos = [
	'#listaAnio',
	'#listaMes',
	'#btnCargar'
	//'input[name="PorcTope"]',
	//'input[name="Concepto"]',
	//'#chkActualizaTope'
];

let liqEmpDetalleActualEnLista = null;
$(function () {

	$(document).on("click", "#btnCargar", abrirModalImportarArchivo);
	$(document).on("click", "#btnProcesarArchivo", handleProcesarArchivo);
	$(document).on("click", "#btnCancelar", handleCancelar);
	$(document).on("click", "#btnConfirmar", ValidarAntesDeConfirmarCargaDeLiquidacion);

	getMaskForIntegerMin50Max100($("#PorcTope"));
});

function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function ImprimirLiquidacion_Generada(id) {
	ReseteoDeReportes();
	setTimeout(() => {
		let data = { id };
		cargarReporteEnArre(41, data, "DETALLE DE LIQUIDACIÓN DE HABERES", "", "");
		invocacionGestorDoc({});
	}, 500);
}

//############ COMENTAR AL FINALIZAR ############
// Botón de imprimir
//$(document).on("click", ".btnImprimir", function () {
//	imprimirOPP();
//});

$("#btnImprimirTemp").on("click", function () {
	ImprimirLiquidacion_Generada("00-00");
});

//function imprimirOPP() {
//	// Invocar gestor documental
//	invocacionGestorDoc({});
//}
//############ COMENTAR AL FINALIZAR ############

function DeshabilitarCampos(valor) {
	campos.forEach(selector => {
		$(selector).prop('disabled', valor);
	});

}

function ValidarAntesDeConfirmarCargaDeLiquidacion() {
	var concepto = $("#Concepto").val();
	var porc = $("#PorcTope").inputmask('unmaskedvalue');
	var resultadoDeValidarFechas = validarPeriodoDentroDeRango(true);
	var filas = $("#tbListaLiqEmpEncabezado tbody tr").length;
	if (!resultadoDeValidarFechas) {
		return false;
	}
	else if (concepto == "") {
		AbrirMensaje("ATENCIÓN", "Debe indicar un valor para 'Concepto'.", function () {
			$("#msjModal").modal("hide");
			$("#Concepto").trigger('focus');
			//return true;
		}, false, ["Aceptar"], "error!", null);
		return false;
	}
	else if (porc < 50 || porc > 100) {
		AbrirMensaje("ATENCIÓN", "Debe indicar un valor válido para 'Porc. Tope'.", function () {
			$("#msjModal").modal("hide");
			$("#PorcTope").trigger('focus');
			//return true;
		}, false, ["Aceptar"], "error!", null);
		return false;
	}
	else if (filas <= 0) {
		AbrirMensaje("ATENCIÓN", "No existen datos de liquidación para confirmar.", function () {
			$("#msjModal").modal("hide");
			$("#PorcTope").trigger('focus');
			//return true;
		}, false, ["Aceptar"], "error!", null);
		return false;
	}
	else {
		handleConfirmarCargaDeLiquidacion();
	}
}

function handleConfirmarCargaDeLiquidacion() {
	AbrirWaiting();
	DeshabilitarCampos(false);
	var periodo = $("#listaAnio").val();
	var mes = $("#listaMes").val();
	var concepto = $("#Concepto").val();
	var actualiza_tope = $("#chkActualizaTope").is(":checked");
	var porc_tope = $("#PorcTope").inputmask('unmaskedvalue');
	var data = { periodo, mes, concepto, actualiza_tope, porc_tope };
	PostGen(data, financieroLiqEmpleadoConfirmarUrl, function (obj) {
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
				return true;
			}, false, ["Aceptar"], "succ!", null);

			ImprimirLiquidacion_Generada(obj.id); // TODO MARCE: Descomentar cuando este el store_procedure que lo llena
			LimpiarCampos();
			DeshabilitarCampos(false);
		}
	});
}

function validarPeriodoDentroDeRango(mostrarMensaje) {
	const anioSeleccionado = parseInt($('#listaAnio').val(), 10);
	const mesSeleccionado = parseInt($('#listaMes').val(), 10); // formato MM

	if (isNaN(anioSeleccionado) || isNaN(mesSeleccionado)) {
		if (mostrarMensaje) {
			AbrirMensaje("ATENCIÓN", "Periodo incompleto: año o mes no seleccionados.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		return false;
	}

	const fechaSeleccionada = new Date(anioSeleccionado, mesSeleccionado - 1, 1);
	const hoy = new Date();
	const fechaActual = new Date(hoy.getFullYear(), hoy.getMonth(), 1);

	// Fecha mínima permitida: hace 6 meses
	const fechaMinima = new Date(fechaActual);
	fechaMinima.setMonth(fechaMinima.getMonth() - 6);

	console.log('fechaSeleccionada:', fechaSeleccionada);
	console.log('fechaMinima:', fechaMinima);
	console.log('fechaActual:', fechaActual);

	if (fechaSeleccionada > fechaActual) {
		if (mostrarMensaje) {
			AbrirMensaje("ATENCIÓN", "La combinación de año y mes no puede superar el mes actual.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		return false;
	}

	if (fechaSeleccionada < fechaMinima) {
		if (mostrarMensaje) {
			AbrirMensaje("ATENCIÓN", "La combinación de año y mes no puede ser anterior a 6 meses respecto al mes actual.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		return false;
	}

	return true;
}


function handleCancelar() {
	var filas = $("#tbListaLiqEmpEncabezado tbody tr").length;
	if (filas > 0) {
		AbrirMensaje("ATENCIÓN", "¿Esta seguro que desea cancelar la operación actual? Se perderán los datos no guardados.", function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI":
					CancelarCargaLiqEmp();
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

function CancelarCargaLiqEmp() {
	AbrirWaiting();
	var data = {};
	PostGen(data, cancelarCargaLiqEmpUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			LimpiarCampos();
			DeshabilitarCampos(false);
		}
	});
}

function LimpiarCampos() {
	$("#tbListaLiqEmpDetalle tbody").empty();
	$("#tbListaLiqEmpEncabezado tbody").empty();
	$("#listaAnio").val($("#SelectedValueAnio").val());
	$("#listaMes").val($("#SelectedValueMes").val());
	$("#PorcTope").val("50");
	$("#Concepto").val("");
}

function abrirModalImportarArchivo() {
	if (!validarPeriodoDentroDeRango(true)) {
		return; // aborta si la validación falla
	}
	AbrirWaiting();
	var datos = {};
	PostGenHtml(datos, abrirModalImportarArchivoUrl, function (obj) {
		$("#divModalImportarArchivo").html(obj);
		const $modal = $("#modalImportarArchivo");

		$modal.modal({
			backdrop: 'static',
		});

		// Reiniciar estado visual del modal
		$("#archivoImportar").val("");
		$("#erroresImportacion").hide();
		$("#listaErroresImportacion").empty();
		$("#barraProgresoContainer").hide();
		$("#barraProgreso")
			.removeClass("bg-success bg-danger")
			.addClass("progress-bar-animated")
			.css("width", "0%")
			.text("0%");

		initializeUploadControls();
		$modal.modal('show');
		CerrarWaiting();
		return true
	});
}

function initializeUploadControls() {
	$('[id^="uploadContainer"]').each(function () {
		const uploadId = $(this).attr('id').replace('uploadContainer', '');
		setupUploadControl(uploadId);
	});
}

function setupUploadControl(uploadId) {
	const $dropZone = $(`#dropZone${uploadId}`);
	const $fileInput = $(`#fileInput${uploadId}`);
	const $uploadInfo = $(`#uploadInfo${uploadId}`);
	const $fileName = $(`#fileName${uploadId}`);
	const $fileSize = $(`#fileSize${uploadId}`);
	const $removeBtn = $(`#removeFile${uploadId}`);

	$dropZone.on('dragover dragenter', function (e) {
		e.preventDefault();
		e.stopPropagation();
		$(this).addClass('dragover');
	});

	$dropZone.on('dragleave dragend', function (e) {
		e.preventDefault();
		e.stopPropagation();
		$(this).removeClass('dragover');
	});

	$dropZone.on('drop', function (e) {
		e.preventDefault();
		e.stopPropagation();
		$(this).removeClass('dragover');

		const files = e.originalEvent.dataTransfer.files;
		if (files.length > 0) {
			handleFileSelection(files[0], uploadId);
		}
	});

	$fileInput.on('change', function () {
		if (this.files.length > 0) {
			handleFileSelection(this.files[0], uploadId);
		}
	});

	$removeBtn.on('click', function () {
		reiniciarImportacionDeExtracto(uploadId)
	});
}

function handleFileSelection(file, uploadId) {
	const origenSeleccionado = $("#listaOrigenDeDatos").val();

	if (!origenSeleccionado || origenSeleccionado === "Seleccionar") {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un origen de datos antes de subir un archivo.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "warn!", null);

		return;
	}

	if (!validateFile(file)) {
		return;
	}

	const $dropZone = $(`#dropZone${uploadId}`);
	const $uploadInfo = $(`#uploadInfo${uploadId}`);
	const $fileName = $(`#fileName${uploadId}`);
	const $fileSize = $(`#fileSize${uploadId}`);

	$fileName.text(file.name);
	$fileSize.text(formatFileSize(file.size));

	$dropZone.hide();
	$uploadInfo.show();

	window[`selectedFile${uploadId}`] = file;

	$(document).trigger('fileSelected', [file, uploadId]);

	console.log(`✅ Archivo seleccionado (${uploadId}):`, file.name, formatFileSize(file.size));
}

function reiniciarImportacionDeExtracto(uploadId) {
	$("#fileInput" + uploadId).val("");

	$("#fileName" + uploadId).text("");
	$("#fileSize" + uploadId).text("");

	$("#uploadProgress" + uploadId).hide();
	$("#progressFill" + uploadId).css("width", "0%");
	$("#progressText" + uploadId).text("0%");

	$("#erroresImportacion").hide();
	$("#listaErroresImportacion").empty();

	$("#barraProgresoContainer").hide();
	$("#barraProgreso")
		.removeClass("bg-success bg-danger")
		.addClass("progress-bar-animated")
		.css("width", "0%")
		.text("0%");

	$("#btnProcesarArchivo").prop("disabled", true);
	$("#btnReiniciarImportacion").prop("disabled", true);

	$("#uploadInfo" + uploadId).hide();
	$("#dropZone" + uploadId).show();
}

function removeFile(uploadId) {
	const $dropZone = $(`#dropZone${uploadId}`);
	const $uploadInfo = $(`#uploadInfo${uploadId}`);
	const $fileInput = $(`#fileInput${uploadId}`);
	const $progressContainer = $(`#uploadProgress${uploadId}`);

	$fileInput.val('');
	delete window[`selectedFile${uploadId}`];

	$uploadInfo.hide();
	if ($progressContainer.length) {
		$progressContainer.hide();
	}
	$dropZone.show();

	$(document).trigger('fileRemoved', [uploadId]);
	console.log(`🗑️ Archivo removido (${uploadId})`);
}

function formatFileSize(bytes) {
	if (bytes === 0) return '0 Bytes';

	const k = 1024;
	const sizes = ['Bytes', 'KB', 'MB', 'GB'];
	const i = Math.floor(Math.log(bytes) / Math.log(k));

	return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

function showUploadError(message) {
	if (typeof AbrirMensaje === 'function') {
		AbrirMensaje("ERROR", message, () => $("#msjModal").modal("hide"), false, ["Aceptar"], "error!", null);
	} else if (typeof showNotification === 'function') {
		showNotification('error', message);
	} else {
		console.error('Upload Error:', message);
		alert(message);
	}
}

function validateFile(file) {
	const allowedTypes = [
		'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', // .xlsx
		'application/vnd.ms-excel', // .xls
		'text/csv', // .csv (opcional)
		'text/plain' // .txt
	];
	const allowedExtensions = ['.xlsx', '.xls', '.csv', '.txt'];
	const maxSize = 10 * 1024 * 1024; // 10MB
	const fileExtension = file.name.toLowerCase().split('.').pop();

	if (!allowedTypes.includes(file.type) && !allowedExtensions.includes(`.${fileExtension}`)) {
		showUploadError('Tipo de archivo no permitido. Solo se aceptan archivos Excel (.xlsx, .xls).');
		return false;
	}

	if (file.size > maxSize) {
		showUploadError('El archivo es demasiado grande. El tamaño máximo permitido es 10MB.');
		return false;
	}

	return true;
}

$(document).on('fileSelected', function (event, file, uploadId) {
	console.log('✅ Archivo seleccionado:', file.name);
	importarArchivo(file);
});

function importarArchivo(file) {
	const formData = new FormData();
	formData.append("archivoImportar", file);
	formData.append("origenId", $("#listaOrigenDeDatos").val());
	const archivo = file;

	if (!archivo) {
		alert("Debe seleccionar un archivo.");
		return;
	}

	$("#btnProcesarArchivo").prop("disabled", true);
	$("#btnReiniciarImportacion").prop("disabled", true);

	$("#barraProgresoContainer").show();
	$("#barraProgreso").css("width", "0%").text("0%");
	$("#erroresImportacion").hide();
	$("#listaErroresImportacion").empty();

	$.ajax({
		url: importarArchivoUrl,
		type: "POST",
		data: formData,
		contentType: false,
		processData: false,
		xhr: function () {
			let xhr = new window.XMLHttpRequest();
			xhr.upload.addEventListener("progress", function (evt) {
				if (evt.lengthComputable) {
					let porcentaje = Math.round((evt.loaded / evt.total) * 100);
					$("#barraProgreso").css("width", porcentaje + "%").text(porcentaje + "%");
				}
			}, false);
			return xhr;
		},
		success: function (data) {
			$("#barraProgreso").removeClass("progress-bar-animated").addClass("bg-success").text("Importación completa");
			$("#btnProcesarArchivo").prop("disabled", false);
		},
		error: function (xhr) {
			$("#barraProgreso").removeClass("progress-bar-animated").addClass("bg-danger").text("Error en la importación");

			const response = xhr.responseJSON;
			if (response && response.errores) {
				response.errores.forEach(error => {
					$("#listaErroresImportacion").append(`<li>${error}</li>`);
				});
				$("#erroresImportacion").show();
				$("#btnReiniciarImportacion").prop("disabled", false);
			} else {
				$("#listaErroresImportacion").append(`<li>Error inesperado al procesar el archivo.</li>`);
				$("#erroresImportacion").show();
				$("#btnReiniciarImportacion").prop("disabled", false);
			}
		}
	});
}

function handleProcesarArchivo() {
	AbrirWaiting();
	var data = {};
	PostGen(data, validarSiExistenRegistrosDeArchivoParaImportarUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			CerrarWaiting();
			AbrirMensaje("ATENCIÓN", "No existen registros para importar.", function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			if (obj.existenRegistros) {
				AbrirMensaje("ATENCIÓN", "¿Esta seguro que desea procesar el archivo importado?", function (e) {
					$("#msjModal").modal("hide");
					switch (e) {
						case "SI":
							ProcesarArchivoImportado();
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
	});
}

function ProcesarArchivoImportado() {
	AbrirWaiting();
	var periodo = $("#listaAnio").val();;
	var mes = $("#listaMes").val();
	var porcTope = $("#PorcTope").inputmask('unmaskedvalue');
	var data = { periodo, mes, porcTope };
	PostGen(data, procesarArchivoImportadoUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			//Cerrar Modal y Actualizar la lista
			$("#modalImportarArchivo").modal("hide");
			ObtenerGrillaEncabezado();
			DeshabilitarCampos(true);
		}
	});
}

function ObtenerGrillaEncabezado() {
	AbrirWaiting();
	var data = {};
	PostGenHtml(data, obtenerGrillaEncabezadoUrl, function (obj) {
		$("#divGrillaEncabezado").html(obj);
		CerrarWaiting();
		return true;
	});
}

function getMaskForIntegerMin50Max100(selector) {
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
		min: 50,
		max: 100
	});
}


const formatearNumero = (valor, opciones = {}) => {
	const formato = new Intl.NumberFormat('en-US', {
		minimumFractionDigits: 2,
		maximumFractionDigits: 2,
		useGrouping: true,
		...opciones
	});
	return formato.format(parseFloat(valor) || 0);
};



/****************************************************************************************
################################ ADD-ON --  tbListaLiqEmpDetalle  #########################
*****************************************************************************************/
function selectItemDetalle(x) {
	$("#tbListaLiqEmpDetalle tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
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
const ActualizarLiqEmpDetalleDebounced = debounce(function (row, campoActual) {
	if (campoActual != undefined) {
		ActualizarLiqEmpDetalle(row, campoActual);
	}
}, 300);

function selectItemEncabezado(x) {
	$("#tbListaLiqEmpEncabezado tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
	let row = $(x).closest("tr");
	let cta_id = $(x).attr("data-cta-id");
	var data = { cta_id: cta_id };
	PostGenHtml(data, obtenerGrillaDetalleUrl, function (obj) {
		$("#divGrillaDetalle").html(obj);
		finalizarInicializacionGridLiqEmpDetalle();
		return true;
	});
}

function finalizarInicializacionGridLiqEmpDetalle() {
	setTimeout(function () {
		configuracionInputMaskOptimizadaGridLiqEmpDetalle();
		optimizarVisualizacionTablaGridLiqEmpDetalle();
	}, 10);
}

function ActualizarLiqEmpDetalle(row, campoActual) {
	AbrirWaiting();
	var idSeleccionado = row.data('id');
	var cta_id = row.data('cta-id');
	var dia_movi = row.data('dia-movi');
	var cm_compte = row.data('cm-compte');
	var tco_id = row.data('tco-id');
	var cm_compte_cuota = row.data('cm-compte-cuota');
	var id = $(campoActual).data('field');
	var val = $(campoActual).val();
	var data = { cta_id, dia_movi, cm_compte, tco_id, cm_compte_cuota, id, val, idSeleccionado };
	PostGen(data, editarItemEnLiqEmpDetalleUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			//Actualizar valores en la grilla
			$(`.input-importe[data-id="${idSeleccionado}"]`)[0].inputmask.setValue(obj.data.importe); // ✅ mantiene la máscara

			// Buscar la fila en la tabla de encabezado con el mismo cta_id
			const filaEncabezado = $(`#tbListaLiqEmpEncabezado tbody tr[data-cta-id="${cta_id}"]`);

			if (filaEncabezado.length) {
				// Actualizar columnas específicas
				filaEncabezado.find('.columna-pendiente').text(formatearNumero(obj.data.pendiente));
				filaEncabezado.find('.columna-stsueldo').text(formatearNumero(obj.data.dtoSueldo));
				filaEncabezado.find('.columna-porc').text(formatearNumero(obj.data.porc, { minimumFractionDigits: 2, maximumFractionDigits: 2 }));
			}

		}
	});
}

function destacarFilaSeleccionadaGridLiqEmpDetalle(id) {
	console.log(`🎯 Destacando fila para ID: ${id}`);

	// Remover el destacado de todas las filas
	$("#tbListaLiqEmpDetalle tbody tr").removeClass("selected");

	// Verificar que existe una fila con ese ID
	const $fila = $("#tbListaLiqEmpDetalle tbody tr[data-id='" + id + "']");

	if ($fila.length === 0) {
		console.warn(`⚠️ No se encontró ninguna fila con data-id="${id}"`);
		return false;
	}

	// Añadir el destacado solo a la fila del producto seleccionado
	$fila.addClass("selected");
	console.log(`✅ Fila destacada correctamente para producto ${id}`);

	// Hacer scroll a la fila si está fuera de vista
	scrollAFilaSeleccionadaGridLiqEmpDetalle($fila);

	return true;
}

function scrollAFilaSeleccionadaGridLiqEmpDetalle($fila) {
	const $tableContainer = $("#tbListaLiqEmpDetalle").closest('.table-responsive');

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

function configuracionInputMaskOptimizadaGridLiqEmpDetalle() {
	console.log("Aplicando configuración InputMask optimizada...");

	// Establecer todos los campos como readonly de una sola vez
	$('.input-importe').prop('readonly', true).addClass('campo-readonly');

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
	Inputmask(maskConfig2Decimales).mask('.input-importe');

	// Configurar eventos de edición
	configurarEventosEdicionOptimizadoGridLiqEmpDetalle();

	console.log("Configuración InputMask aplicada");
}

function configurarEventosEdicionOptimizadoGridLiqEmpDetalle() {
	const camposEditables = '.input-importe';
	const camposSecuencia01 = '.input-importe';

	// Limpiar eventos previos
	$(document).off('click.camposEditables keydown.camposEditables blur.camposSecuencia01');

	// Evento click unificado
	$(document).on('click.camposEditables', camposEditables, function (e) {
		e.stopPropagation();

		const $this = $(this);
		const idDetalle = $this.closest('tr').data('id');

		// Cambio de producto si es necesario
		if (idDetalle !== liqEmpDetalleActualEnLista) {
			liqEmpDetalleActualEnLista = idDetalle;
			destacarFilaSeleccionadaGridLiqEmpDetalle(idDetalle);
		}

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

			var fueModificado = marcarCampoModificadoGridLiqEmpDetalle(this);
			activarSiguienteCampoGridLiqEmpDetalle(this);

			// Aplicar cálculos según tipo
			if (esSecuencia01 && fueModificado) ActualizarLiqEmpDetalleDebounced(row, this);
			//else if (esMargen) calcularPrecioVentaAPIDebounced(row);
			//else if (esPrecioVenta) calcularPrecioVentaMargenAPIDebounced(row);
		}
	});

	// Eventos blur simplificados con delegación
	const eventosBlur = {
		[camposSecuencia01]: () => ActualizarLiqEmpDetalleDebounced
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

			$(this).prop('readonly', true).addClass('campo-readonly');
			getCallback()(row);
		});
	});
}

function activarSiguienteCampoGridLiqEmpDetalle(campoActual) {
	const $campoActual = $(campoActual);
	const $fila = $campoActual.closest('tr');
	const camposEditables = '.input-importe';
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

function marcarCampoModificadoGridLiqEmpDetalle(input) {
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

function optimizarVisualizacionTablaGridLiqEmpDetalle() {
	// Asegurarnos de que la tabla existe
	if ($("#tbListaLiqEmpDetalle").length === 0) {
		return;
	}

	// Ajustar columnas con texto para que no sean demasiado anchas
	$("#tbListaLiqEmpDetalle th:nth-child(0)").css('max-width', '180px'); // Descripción
	$("#tbListaLiqEmpDetalle td:nth-child(0)").css({
		'max-width': '180px',
		'white-space': 'nowrap',
		'overflow': 'hidden',
		'text-overflow': 'ellipsis'
	});

	// Asegurarnos que la tabla tenga scroll horizontal si es necesario
	$("#tbListaLiqEmpDetalle").closest('.table-responsive').css('overflow-x', 'auto');

	console.log("Tabla optimizada para mejor visualización");
}
/****************************************************************************************
################################ FIN ADD-ON --  tbListaLiqEmpDetalle  #####################
*****************************************************************************************/