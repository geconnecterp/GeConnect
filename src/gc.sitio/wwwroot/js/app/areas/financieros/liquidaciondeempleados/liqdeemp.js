$(function () {

	$(document).on("click", "#btnCargar", abrirModalImportarArchivo);
	$(document).on("click", "#btnProcesarArchivo", handleProcesarArchivo);

	getMaskForIntegerMin50Max100($("#PorcTope"));
});

function abrirModalImportarArchivo() {
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
			CerrarWaiting();
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			//Cerrar Modal y Actualizar la lista
			$("#modalImportarArchivo").modal("hide");
			obtenerListaExtractoBancario()
		}
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