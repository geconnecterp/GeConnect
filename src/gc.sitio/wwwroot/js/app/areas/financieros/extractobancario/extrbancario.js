var itemSeleccionadoOrden = 0;
var fecha_extracto;
$(function () {
	InicializarCamposEnFiltros();
	$(document).on("change", "#listaCuentaBanco", ControlalistaCuentaBancoSelected);
	$(document).on("click", "#btnAgregarItem", abrirModalAgregarItemExtracto);
	$(document).on("click", "#btnModificarItem", abrirModalModificarItemExtracto);
	$(document).on("click", "#btnCancelarCarga", cancelarCargaDeExtracto);
	$(document).on("click", "#btnConfirmarAgregarExtracto", confirmarAgregarExtracto);
	$(document).on("click", "#btnImportar", abrirModalImportarExtracto);
	$(document).on("click", "#btnImportarArchivo", importarArchivoExtracto);
	$("#btnReiniciarImportacion").on("click", function () {
		reiniciarImportacionDeExtracto("default"); // o el valor dinámico de uploadId
	});

	//btnReiniciarImportacion
	$("#FechaDesde, #FechaHasta").on("blur", validarFechas);

	$("#btnBuscar").on("click", function () {
		ctafIdSelected = $("#listaCuentaBanco").val();
		if (ctafIdSelected == "") {
			AbrirMensaje("ATENCIÓN", "Debe seleccionar una cuenta banco.", function () {
				$("#msjModal").modal("hide");
				$("#listaCuentaBanco").trigger("focus");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			ctafDenominacionSelected = $("#listaCuentaBanco option:selected").text();
			ControlaCargarExtractoBancarioClick();
		}
	});
	$("#btnFiltro").on("click", function () {
		if ($("#divFiltros").hasClass("show")) {
			$("#divFiltros").collapse("hide");
		}
		else {
			$("#divFiltros").collapse("show");
		}
	});
	$("#btnCancel").on("click", function () {
		btnCancelarClick();
	});
	$("#archivoImportar").on("change", function () {
		const archivo = this.files[0];
		const extensionesValidas = [".xlsx", ".txt"];

		if (archivo && extensionesValidas.some(ext => archivo.name.toLowerCase().endsWith(ext))) {
			$("#btnImportarArchivo").prop("disabled", false);
		} else {
			$("#btnImportarArchivo").prop("disabled", true);
			alert("Formato de archivo no válido. Solo se permite .xlsx o .txt tabulado.");
			$(this).val(""); // limpiar input
		}
	});

	initializeUploadControls();
});

function initializeUploadControls() {
	$('[id^="uploadContainer"]').each(function () {
		const uploadId = $(this).attr('id').replace('uploadContainer', '');
		setupUploadControl(uploadId);
	});
}

// Configurar un control de upload específico
function setupUploadControl(uploadId) {
	const $dropZone = $(`#dropZone${uploadId}`);
	const $fileInput = $(`#fileInput${uploadId}`);
	const $uploadInfo = $(`#uploadInfo${uploadId}`);
	const $fileName = $(`#fileName${uploadId}`);
	const $fileSize = $(`#fileSize${uploadId}`);
	const $removeBtn = $(`#removeFile${uploadId}`);

	// Eventos de drag and drop
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

	// ✅ CORREGIDO: Click en zona de drop - Usar trigger() en lugar de click()
	$dropZone.on('click', function () {
		$fileInput.trigger('click');
	});

	// Selección de archivo
	$fileInput.on('change', function () {
		if (this.files.length > 0) {
			//importarArchivoExtracto();
			handleFileSelection(this.files[0], uploadId);
		}
	});

	// Botón remover archivo
	$removeBtn.on('click', function () {
		//removeFile(uploadId);
		reiniciarImportacionDeExtracto(uploadId)
	});
}

function removeFile(uploadId) {
	const $dropZone = $(`#dropZone${uploadId}`);
	const $uploadInfo = $(`#uploadInfo${uploadId}`);
	const $fileInput = $(`#fileInput${uploadId}`);
	const $progressContainer = $(`#uploadProgress${uploadId}`);

	// Limpiar input
	$fileInput.val('');

	// Limpiar referencia
	delete window[`selectedFile${uploadId}`];

	// Mostrar drop zone y ocultar info
	$uploadInfo.hide();
	if ($progressContainer.length) {
		$progressContainer.hide();
	}
	$dropZone.show();

	// Disparar evento personalizado
	$(document).trigger('fileRemoved', [uploadId]);

	console.log(`🗑️ Archivo removido (${uploadId})`);
}

function reiniciarImportacionDeExtracto(uploadId) {
	// Limpiar input file
	$("#fileInput" + uploadId).val("");

	// Limpiar nombre y tamaño del archivo
	$("#fileName" + uploadId).text("");
	$("#fileSize" + uploadId).text("");

	// Ocultar solo la barra de progreso visual custom
	$("#uploadProgress" + uploadId).hide();
	$("#progressFill" + uploadId).css("width", "0%");
	$("#progressText" + uploadId).text("0%");

	// Ocultar errores
	$("#erroresImportacion").hide();
	$("#listaErroresImportacion").empty();

	// Resetear barra de progreso tradicional
	$("#barraProgresoContainer").hide();
	$("#barraProgreso")
		.removeClass("bg-success bg-danger")
		.addClass("progress-bar-animated")
		.css("width", "0%")
		.text("0%");

	// Desactivar botones
	$("#btnImportarArchivo").prop("disabled", true);
	$("#btnReiniciarImportacion").prop("disabled", true);

	// Mostrar nuevamente la sección de selección si estaba oculta
	$("#uploadInfo" + uploadId).hide(); // opcional si querés mantener visible

	// Volver a mostrar el área de selección
	$("#dropZone" + uploadId).show();
}

/*
function abrirModalAgregarItemExtracto() {
	AbrirWaiting();
	var datos = {};
	PostGenHtml(datos, abrirModalImportarExtractoUrl, function (obj) {
		$("#divAgregarItemExtracto").html(obj);
		const $modal = $("#modalAgregarItemExtracto");

		$modal.modal({
			backdrop: 'static',
		});
		//$modal.modal('show');

		inicializarCamposEnModal();
		$("#Fecha").trigger("focus");

		$modal.modal('show');
		CerrarWaiting();
		return true
	});
}
*/

function abrirModalImportarExtracto() {
	AbrirWaiting();
	var datos = {};
	PostGenHtml(datos, abrirModalImportarExtractoUrl, function (obj) {
		$("#divModalImportarExtracto").html(obj);
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

		$modal.modal('show');
		CerrarWaiting();
		return true
	});
}

function handleFileSelection(file, uploadId) {
	if (!validateFile(file)) {
		return;
	}

	const $dropZone = $(`#dropZone${uploadId}`);
	const $uploadInfo = $(`#uploadInfo${uploadId}`);
	const $fileName = $(`#fileName${uploadId}`);
	const $fileSize = $(`#fileSize${uploadId}`);

	// Mostrar información del archivo
	$fileName.text(file.name);
	$fileSize.text(formatFileSize(file.size));

	// Ocultar drop zone y mostrar info
	$dropZone.hide();
	$uploadInfo.show();

	// Guardar referencia del archivo
	window[`selectedFile${uploadId}`] = file;

	// Disparar evento personalizado
	$(document).trigger('fileSelected', [file, uploadId]);

	console.log(`✅ Archivo seleccionado (${uploadId}):`, file.name, formatFileSize(file.size));
}

// Formatear tamaño de archivo
function formatFileSize(bytes) {
	if (bytes === 0) return '0 Bytes';

	const k = 1024;
	const sizes = ['Bytes', 'KB', 'MB', 'GB'];
	const i = Math.floor(Math.log(bytes) / Math.log(k));

	return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

function validateFile(file) {
	const allowedTypes = [
		'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', // .xlsx
		'application/vnd.ms-excel', // .xls
		'text/csv' // .csv (opcional)
	];

	const maxSize = 10 * 1024 * 1024; // 10MB

	if (!allowedTypes.includes(file.type)) {
		showUploadError('Tipo de archivo no permitido. Solo se aceptan archivos Excel (.xlsx, .xls).');
		return false;
	}

	if (file.size > maxSize) {
		showUploadError('El archivo es demasiado grande. El tamaño máximo permitido es 10MB.');
		return false;
	}

	return true;
}

// ✅ MEJORADO: Eventos personalizados para el upload
$(document).on('fileSelected', function (event, file, uploadId) {
	console.log('✅ Archivo seleccionado:', file.name);
	importarArchivoExtracto(file);
});

function importarArchivoExtracto(file) {
	const formData = new FormData();
	formData.append("archivoImportar", file); // nombre debe coincidir con el parámetro del backend

	//const archivo = $("#archivoImportar")[0].files[0];
	const archivo = file;

	if (!archivo) {
		alert("Debe seleccionar un archivo.");
		return;
	}

	// 🔒 Desactivar botones durante la importación
	$("#btnImportarArchivo").prop("disabled", true);
	$("#btnReiniciarImportacion").prop("disabled", true);

	$("#barraProgresoContainer").show();
	$("#barraProgreso").css("width", "0%").text("0%");
	$("#erroresImportacion").hide();
	$("#listaErroresImportacion").empty();

	$.ajax({
		url: procesarArchivoUrl, // adaptá esta URL
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
			// ✅ Rehabilitar botón si querés permitir nueva carga
			$("#btnImportarArchivo").prop("disabled", false);
		},
		error: function (xhr) {
			$("#barraProgreso").removeClass("progress-bar-animated").addClass("bg-danger").text("Error en la importación");

			const response = xhr.responseJSON;
			if (response && response.errores) {
				response.errores.forEach(error => {
					$("#listaErroresImportacion").append(`<li>${error}</li>`);
				});
				$("#erroresImportacion").show();
				$("#btnReiniciarImportacion").prop("disabled", false); // ✅ Activar botón
			} else {
				$("#listaErroresImportacion").append(`<li>Error inesperado al procesar el archivo.</li>`);
				$("#erroresImportacion").show();
				$("#btnReiniciarImportacion").prop("disabled", false); // ✅ Activar botón
			}
		}
	});
}

function cancelarCargaDeExtracto() {
	AbrirMensaje("ATENCIÓN", "¿Esta seguro que desea cancelar la carga del extracto?", function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI": 
				btnCancelarClick();
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;

	}, true, ["Aceptar", "Cancelar"], "question!", null);
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

function ControlalistaCuentaBancoSelected() {
	var ctaf_id = $("#listaCuentaBanco").val();
	var data = { ctaf_id };
	PostGen(data, obtenerCuentaBancoUrl, function (obj) {
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
			EstableceValoresDeFechas(obj.ext_fecha);
		}
	});
}

function ControlaCargarExtractoBancarioClick() {
	AbrirWaiting();
	var ctaf_id = $("#listaCuentaBanco").val();
	var FechaDesde = $("#FechaDesde").val();
	var FechaHasta = $("#FechaHasta").val();
	var data = { ctaf_id, FechaDesde, FechaHasta };
	PostGenHtml(data, cargarExtractoBancarioURL, function (obj) {
		CerrarWaiting();
		$("#divExtractoBco").html(obj);
		$("#divFiltros").removeClass("show").addClass("collapse");
		$("#divDetalle").collapse("show");
		return true
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function EstableceValoresDeFechas(fecha) {
	console.log(fecha);
	fecha_extracto = fecha;
	if (fecha && typeof fecha === "string" && moment(fecha, moment.ISO_8601, true).isValid()) {
		const fechaMoment = moment(fecha);
		console.log("Fecha válida:", fechaMoment.format("YYYY-MM-DD HH:mm:ss"));

		const fechaMinima = moment(fecha).format("YYYY-MM-DD");
		$("#FechaDesde").attr("min", fechaMinima);
		$("#FechaHasta").attr("min", fechaMinima)

	} else {
		console.warn("Fecha inválida o no definida");
	}
}

function validarFechas() {
	const $desde = $("#FechaDesde");
	const $hasta = $("#FechaHasta");

	const desde = $desde.val();
	const hasta = $hasta.val();

	if (!desde || !hasta) return;

	const mDesde = moment(desde, "YYYY-MM-DD");
	const mHasta = moment(hasta, "YYYY-MM-DD");
	const mMinima = moment(fecha_extracto, "YYYY-MM-DD");
	var now = moment().format('yyyy-MM-DD');

	// Si alguna fecha es menor a la mínima → setear ambas
	if (mDesde.isBefore(mMinima) || mHasta.isBefore(mMinima)) {
		$desde.val(now);
		$hasta.val(now);
		AbrirMensaje("ATENCIÓN", `Las fechas no pueden ser menor a la fecha del extracto (${mMinima})`, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
		return;
	}
}

function eliminarItem(ctaf_id, extr_id, orden) {
	console.log("Ver detalle de cheque:", ctaf_id, extr_id, orden);
	var data = {
		orden
	};
	AbrirWaiting();
	PostGen(data, quitarItemExtractoUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			CerrarWaiting();
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			setTimeout(() => {
				$('#modalAgregarItemExtracto').modal('hide');
				obtenerListaExtractoBancario();
			}, 200);
		}
	});
}


function InicializarCamposEnFiltros() {
	var now = moment().format('yyyy-MM-DD');
	$("#FechaDesde").val(now);
	$("#FechaHasta").val(now);
	$("#divFiltros").removeClass("collapse").addClass("show");
	$("#lbCuentaBanco").text("Cuenta Banco");
	$("#lbFecha").text("Fecha");
	$("#lbCargar").text("Cargar");
	$("#chkFecha").on("click", function () {
		if ($("#chkFecha").is(":checked")) {
			$("#FechaDesde").prop("disabled", false);
			$("#FechaHasta").prop("disabled", false);
			$("#FechaDesde").trigger("focus");
		}
		else {
			$("#FechaDesde").prop("disabled", true);
			$("#FechaHasta").prop("disabled", true);
		}
	});
	$("#chkCuentaBanco").on("click", function () {
		if ($("#chkCuentaBanco").is(":checked")) {
			$("#listaCuentaBanco").prop("disabled", false);
			$("#listaCuentaBanco").trigger("focus");
		}
		else {
			$("#listaCuentaBanco").prop("disabled", true);
		}
	});
	$("#btnFiltro").on("click", function () {
		if ($("#divFiltros").hasClass("show")) {
			$("#divFiltros").collapse("hide");
		}
		else {
			$("#divFiltros").collapse("show");
		}
	});
	$("#FechaDesde, #FechaHasta, #listaCuentaBanco").prop("disabled", false);
	$("#chkCuentaBanco").prop('checked', true);
	$("#chkCuentaBanco").trigger("change");
	$("#chkCuentaBanco").prop("disabled", true);
	$("#chkFecha").prop('checked', true);
	$("#chkFecha").trigger("change");
	$("#chkFecha").prop("disabled", true);
	//$("#btnCancel").on("click", function () {
	//	btnCancelarClick();
	//});
}

function btnCancelarClick() {
	$("#divFiltros").removeClass("collapse").addClass("show");
	$("#divDetalle").collapse("hide");
	$("#listaCuentaBanco").val("");
	EstableceValoresDeFechas(fecha_extracto);
	InicializarDatosEnSesion();
}

//function abrirModalModificarItemExtracto() {
//	if (abrirModalModificarItemExtracto != 0) {
//		AbrirWaiting();
//		var orden = itemSeleccionadoOrden;
//		var abm = "M";
//		var datos = { abm, orden };
//		PostGenHtml(datos, abrirModalAgregarItemExtractoUrl, function (obj) {
//			$("#divAgregarItemExtracto").html(obj);
//			const $modal = $("#modalAgregarItemExtracto");

//			$modal.modal({
//				backdrop: 'static',
//			});

//			inicializarCamposEnModal();
//			$("#Fecha").trigger("focus");

//			$modal.modal('show');
//			CerrarWaiting();
//			return true
//		});
//	}
//	else {
//		AbrirMensaje("ATENCIÓN", "Debe seleccionar un ítem extracto para modificar.", function () {
//			$("#msjModal").modal("hide");
//			return true;
//		}, false, ["Aceptar"], "error!", null);
//	}
//}

function abrirModalModificarItemExtracto() {
	if (abrirModalModificarItemExtracto != 0) {
		AbrirWaiting();
		var orden = itemSeleccionadoOrden;
		var abm = "M";
		var datos = { abm, orden };
		PostGenHtml(datos, abrirModalAgregarItemExtractoUrl, function (obj) {
			$("#divAgregarItemExtracto").html(obj);
			const $modal = $("#modalAgregarItemExtracto");

			$modal.modal({
				backdrop: 'static',
			});

			inicializarCamposEnModal();
			$("#Fecha").trigger("focus");

			$modal.modal('show');
			CerrarWaiting();
			return true
		});
	}
	else {
		AbrirMensaje("ATENCIÓN", "Debe seleccionar un ítem extracto para modificar.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}

function inicializarCamposEnModal() {
	const $modal = $("#modalAgregarItemExtracto");

	// ✅ Corrección: usar el modal correcto y evitar document.ready redundante
	$modal.find("input, select, checkbox").on("keydown", function (e) {
		if (e.key === "Enter") {
			e.preventDefault();

			const $campos = $modal.find("input, select, checkbox")
				.filter(":visible:enabled");

			const index = $campos.index(this);

			if (index !== -1) {
				if (index < $campos.length - 1) {
					$campos.eq(index + 1).focus();
				} else {
					// Último campo → foco al botón Confirmar
					$modal.find("#btnConfirmarAgregarExtracto").focus();
				}
			}
		}
	});

	["#Debe", "#Haber"].forEach(selector => {
		const $campo = $modal.find(selector);
		let valor = $campo.val();

		// Si el valor tiene punto decimal, lo transformamos
		if (valor && valor.includes(".")) {
			valor = valor.replace(".", ",");
			$campo.val(valor);
		}
	});

	getMaskForMoneyType("#Debe");
	getMaskForMoneyType("#Haber");

	// Sincronizar Debe/Haber al tipear
	const $debe = $modal.find('input[name="Debe"]');
	const $haber = $modal.find('input[name="Haber"]');

	let bloqueado = false;

	$debe.off('input').on('input', function () {
		if (bloqueado) return;
		bloqueado = true;

		const valor = $(this).val().replace(",", ".").trim();
		if (valor !== '' && parseFloat(valor) !== 0) {
			$haber.val('0');
		}

		bloqueado = false;
	});

	$haber.off('input').on('input', function () {
		if (bloqueado) return;
		bloqueado = true;

		const valor = $(this).val().replace(",", ".").trim();
		if (valor !== '' && parseFloat(valor) !== 0) {
			$debe.val('0');
		}

		bloqueado = false;
	});
}

function abrirModalAgregarItemExtracto() {
	AbrirWaiting();
	var datos = { abm: "A", orden: itemSeleccionadoOrden };
	PostGenHtml(datos, abrirModalAgregarItemExtractoUrl, function (obj) {
		$("#divAgregarItemExtracto").html(obj);
		const $modal = $("#modalAgregarItemExtracto");

		$modal.modal({
			backdrop: 'static',
		});
		//$modal.modal('show');

		inicializarCamposEnModal();
		$("#Fecha").trigger("focus");

		$modal.modal('show');
		CerrarWaiting();
		return true
	});
}

function confirmarAgregarExtracto() {
	var abm = $("#abm").val();
	if (abm == "A") {
		var fecha = $("#Fecha").val();
		var insertar = $("#chkInsertar")[0].checked;
		var movimiento = $("#listaMovimientos").val();
		var movimiento_desc = $("#listaMovimientos option:selected").text();
		var comprobante = $("#Comprobante").val();
		var debe = $("#Debe").inputmask('unmaskedvalue');
		var haber = $("#Haber").inputmask('unmaskedvalue');
		var orden = $("#orden").val();
		//Actualizar lista en backend
		var data = {
			ctaf_id: ctafIdSelected,
			ext_fecha: fecha,
			extr_id: movimiento,
			extr_desc: movimiento_desc,
			ext_concepto: comprobante,
			ext_debe: debe,
			ext_haber: haber,
			abm: "A",
			insertar,
			orden
		};
		AbrirWaiting();
		PostGen(data, agregarItemExtractoUrl, function (obj) {
			CerrarWaiting();
			if (obj.error === true) {
				CerrarWaiting();
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				setTimeout(() => {
					$('#modalAgregarItemExtracto').modal('hide');
					obtenerListaExtractoBancario();
				}, 200);
			}
		});
	}
	else if (abm == "M") {
		var orden = $("#orden").val();
		var movimiento = $("#listaMovimientos").val();
		var movimiento_desc = $("#listaMovimientos option:selected").text();
		var comprobante = $("#Comprobante").val();
		var debe = $("#Debe").inputmask('unmaskedvalue');
		var haber = $("#Haber").inputmask('unmaskedvalue');
		
		var data = {
			orden,
			extr_id: movimiento,
			extr_desc: movimiento_desc,
			ext_concepto: comprobante,
			ext_debe: debe,
			ext_haber: haber,
			abm: "M"
		};
		AbrirWaiting();
		PostGen(data, modificarItemExtractoUrl, function (obj) {
			CerrarWaiting();
			if (obj.error === true) {
				CerrarWaiting();
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				setTimeout(() => {
					$('#modalAgregarItemExtracto').modal('hide');
					obtenerListaExtractoBancario();
				}, 200);
			}
		});
	}
}

function obtenerListaExtractoBancario() {
	CerrarWaiting();
	var data = {};
	PostGenHtml(data, obtenerListaExtractoBancarioUrl, function (obj) {
		$("#divGridCrudExtracto").html(obj);
		itemSeleccionadoOrden = 0;
		CerrarWaiting();
		return true
	});
}

function selectItemExtracto(x) {
	console.log($(x).data("orden"))
	itemSeleccionadoOrden = $(x).data("orden");
	$("#tbListaCrudExtracto tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
}

function getMaskForMoneyType(selector) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: '.',
		radixPoint: ',',
		digits: 2,
		digitsOptional: false,
		allowMinus: false,
		prefix: '',
		suffix: '',
		rightAlign: true,
		unmaskAsNumber: true
	});
}