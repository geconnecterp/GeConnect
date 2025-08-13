$(function () {
    // Inicialización automática de controles de upload
    initializeUploadControls();
});

function inicializaControlCuentaImp() {
    $("#controlConsultaCambio" + nnControlCta01).val(true);
    window["AsignaDatosCuenta" + nnControlCta01]();

    //muestro el control
    $("#controlCta" + nnControlCta01).show("fast");
}

// Inicializar todos los controles de upload en la página
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

    // Click en zona de drop
    $dropZone.on('click', function () {
        $fileInput.click();
    });

    // Selección de archivo
    $fileInput.on('change', function () {
        if (this.files.length > 0) {
            handleFileSelection(this.files[0], uploadId);
        }
    });

    // Botón remover archivo
    $removeBtn.on('click', function () {
        removeFile(uploadId);
    });
}

// Manejar selección de archivo
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

    console.log(`Archivo seleccionado (${uploadId}):`, file.name, formatFileSize(file.size));
}

// Validar archivo
function validateFile(file) {
    const allowedTypes = [
        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', // .xlsx
        'application/vnd.ms-excel', // .xls
        'text/csv' // .csv
    ];

    const maxSize = 10 * 1024 * 1024; // 10MB

    if (!allowedTypes.includes(file.type)) {
        showUploadError('Tipo de archivo no permitido. Solo se aceptan archivos Excel (.xlsx, .xls) y CSV.');
        return false;
    }

    if (file.size > maxSize) {
        showUploadError('El archivo es demasiado grande. El tamaño máximo permitido es 10MB.');
        return false;
    }

    return true;
}

// Remover archivo seleccionado
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
    $progressContainer.hide();
    $dropZone.show();

    // Disparar evento personalizado
    $(document).trigger('fileRemoved', [uploadId]);

    console.log(`Archivo removido (${uploadId})`);
}

// Formatear tamaño de archivo
function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';

    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));

    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

// Mostrar error de upload
function showUploadError(message) {
    // Usar el sistema de notificaciones existente o alert
    if (typeof showNotification === 'function') {
        showNotification('error', message);
    } else {
        alert(message);
    }
}

// Simular progreso de upload (para uso futuro)
function simulateUploadProgress(uploadId, callback) {
    const $progressContainer = $(`#uploadProgress${uploadId}`);
    const $progressFill = $(`#progressFill${uploadId}`);
    const $progressText = $(`#progressText${uploadId}`);

    $progressContainer.show();

    let progress = 0;
    const interval = setInterval(() => {
        progress += Math.random() * 15;
        if (progress >= 100) {
            progress = 100;
            clearInterval(interval);
            if (callback) callback();
        }

        $progressFill.css('width', progress + '%');
        $progressText.text(Math.round(progress) + '%');
    }, 200);
}

// Obtener archivo seleccionado
function getSelectedFile(uploadId) {
    return window[`selectedFile${uploadId}`] || null;
}

// Función pública para obtener información del archivo
function getFileInfo(uploadId) {
    const file = getSelectedFile(uploadId);
    if (!file) return null;

    return {
        name: file.name,
        size: file.size,
        type: file.type,
        formattedSize: formatFileSize(file.size)
    };
}

// ✅ REEMPLAZAR: La función procesarImportacion() con análisis previo
function procesarImportacion() {
    if (!archivoSeleccionado) {
        AbrirMensaje("ATENCIÓN", "No hay ningún archivo seleccionado para procesar.",
            () => $("#msjModal").modal("hide"), false, ["Aceptar"], "warn!", null);
        return;
    }

    // ✅ PASO 1: Mostrar progreso de análisis
    $('#importResults').slideDown(300);
    $('#importProgress').css('width', '20%').text('Analizando estructura...');

    // ✅ PASO 2: Crear FormData para análisis
    const formData = new FormData();
    formData.append('archivo', archivoSeleccionado);

    // ✅ PASO 3: Llamada AJAX para análisis de columnas
    $.ajax({
        url: analizarColumnasUrl,// '@Url.Action("AnalizarColumnas", "Importar", new { area = "Productos" })',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            $('#importProgress').css('width', '100%').text('Análisis completado');

            setTimeout(() => {
                $('#importResults').slideUp(300);

                if (response.error) {
                    AbrirMensaje("ERROR", response.mensaje,
                        () => $("#msjModal").modal("hide"), false, ["Aceptar"], "error!", null);
                } else {
                    mostrarAnalisisColumnas(response.analisis);
                }
            }, 1000);
        },
        error: function (xhr, status, error) {
            $('#importResults').slideUp(300);
            console.error('Error en análisis:', error);
            AbrirMensaje("ERROR", "Error de comunicación con el servidor.",
                () => $("#msjModal").modal("hide"), false, ["Aceptar"], "error!", null);
        }
    });
}

// ✅ ACTUALIZAR: Función mostrarAnalisisColumnas con combo de mapeo
function mostrarAnalisisColumnas(analisis) {
    const htmlAnalisis = `
        <div class="row mt-3">
            <div class="col-12">
                <!-- Información general del archivo (sin cambios) -->
                <div class="alert alert-info border-0 shadow-sm">
                    <div class="d-flex align-items-center mb-2">
                        <i class="bx bx-file-blank bx-lg text-info me-3"></i>
                        <div>
                            <h5 class="alert-heading mb-1">Análisis de Estructura con Mapeo Automático</h5>
                            <p class="mb-0">
                                <strong>${analisis.nombreArchivo}</strong> - Hoja: <em>${analisis.nombreHoja}</em>
                            </p>
                        </div>
                    </div>
                    <div class="row text-center">
                        <div class="col-md-3">
                            <div class="border-end">
                                <h4 class="text-info mb-0">${analisis.totalFilas.toLocaleString()}</h4>
                                <small class="text-muted">Filas Totales</small>
                            </div>
                        </div>
                        <div class="col-md-3">
                            <div class="border-end">
                                <h4 class="text-info mb-0">${analisis.totalColumnas}</h4>
                                <small class="text-muted">Columnas</small>
                            </div>
                        </div>
                        <div class="col-md-3">
                            <div class="border-end">
                                <h4 class="text-info mb-0">${(analisis.totalFilas - 1).toLocaleString()}</h4>
                                <small class="text-muted">Registros de Datos</small>
                            </div>
                        </div>
                        <div class="col-md-3">
                            <h4 class="text-success mb-0">${contarColumnasMapepadas(analisis.columnas)}</h4>
                            <small class="text-muted">Auto-mapeadas</small>
                        </div>
                    </div>
                </div>

                <!-- ✅ TABLA ACTUALIZADA CON COMBO DE MAPEO -->
                <div class="card shadow-sm">
                    <div class="card-header bg-primary text-white d-flex justify-content-between align-items-center">
                        <h6 class="mb-0">
                            <i class="bx bx-table me-2"></i>
                            Estructura de Columnas Detectadas y Mapeo
                        </h6>
                        <button type="button" class="btn btn-sm btn-outline-light" onclick="autoMapearTodas()">
                            <i class="bx bx-magic-wand me-1"></i>Re-mapear Todo
                        </button>
                    </div>
                    <div class="card-body p-0">
                        <div class="table-responsive">
                            <table class="table table-hover mb-0">
                                <thead class="table-light">
                                    <tr>
                                        <th class="text-center" style="width: 50px;">Col.</th>
                                        <th style="min-width: 180px;">Encabezado Excel</th>
                                        <th class="text-center" style="width: 80px;">Tipo</th>
                                        <th class="text-center" style="width: 70px;">Datos</th>
                                        <th class="text-center" style="width: 90px;">% Llenado</th>
                                        <th style="min-width: 250px;">Campo Mapeado</th>
                                        <th style="min-width: 200px;">Ejemplos</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    ${analisis.columnas.map(columna => `
                                        <tr>
                                            <td class="text-center">
                                                <span class="badge bg-secondary">${columna.letra}</span>
                                            </td>
                                            <td>
                                                <strong class="text-primary">${columna.encabezado}</strong>
                                            </td>
                                            <td class="text-center">
                                                <span class="badge ${getTipoBadgeClass(columna.tipoDetectado)}">
                                                    ${getTipoIcon(columna.tipoDetectado)} ${columna.tipoDetectado}
                                                </span>
                                            </td>
                                            <td class="text-center">
                                                <small>${columna.valoresNoVacios.toLocaleString()}</small>
                                            </td>
                                            <td class="text-center">
                                                <div class="d-flex align-items-center justify-content-center">
                                                    <div class="progress me-1" style="width: 30px; height: 6px;">
                                                        <div class="progress-bar ${getProgressBarClass(columna.porcentajeLlenado)}" 
                                                             style="width: ${columna.porcentajeLlenado}%"></div>
                                                    </div>
                                                    <small class="text-muted">${columna.porcentajeLlenado}%</small>
                                                </div>
                                            </td>
                                            <td>
                                                <!-- ✅ COMBO DE MAPEO PRINCIPAL -->
                                                <div class="d-flex align-items-center gap-2">
                                                    <select class="form-select form-select-sm mapeo-combo" 
                                                            data-columna="${columna.indice}"
                                                            onchange="actualizarMapeo(${columna.indice}, this.value)">
                                                        <option value="">Sin mapear</option>
                                                        ${generarOpcionesMapeo(analisis.camposDisponibles, columna.campoMapeado)}
                                                    </select>
                                                    ${columna.mapeadoAutomatico ?
            `<span class="badge bg-success ms-1" title="Mapeo automático con ${columna.confianzaMapeo}% confianza">
                                                            <i class="bx bx-magic-wand"></i> ${columna.confianzaMapeo}%
                                                        </span>` : ''
        }
                                                </div>
                                            </td>
                                            <td>
                                                <div class="d-flex flex-wrap gap-1">
                                                    ${columna.ejemplosValores.slice(0, 2).map(ejemplo =>
            `<small class="badge bg-light text-dark border">${truncateText(ejemplo, 15)}</small>`
        ).join('')}
                                                </div>
                                            </td>
                                        </tr>
                                    `).join('')}
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>

                <!-- Botones de acción actualizados -->
                <div class="d-flex justify-content-between align-items-center mt-4">
                    <div class="d-flex align-items-center gap-3">
                        <button type="button" class="btn btn-outline-secondary" onclick="cancelarAnalisis()">
                            <i class="bx bx-x me-1"></i>Cancelar
                        </button>
                        <div class="text-muted small">
                            <i class="bx bx-info-circle me-1"></i>
                            <span id="contadorMapeados">${contarColumnasMapepadas(analisis.columnas)}</span> 
                            de ${analisis.columnas.length} columnas mapeadas
                        </div>
                    </div>
                    
                    <div class="d-flex gap-2">
                        <button type="button" class="btn btn-outline-warning" onclick="validarMapeo()">
                            <i class="bx bx-check-shield me-1"></i>Validar Mapeo
                        </button>
                        <button type="button" class="btn btn-success" onclick="confirmarEIniciarImportacion()">
                            <i class="bx bx-check-double me-1"></i>Confirmar e Importar
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;

    $('#mainContent').html(htmlAnalisis).hide().slideDown(400);

    // ✅ GUARDAR: Referencia global para uso posterior
    window.analisisActual = analisis;
}

// ✅ NUEVAS: Funciones de soporte para mapeo
function generarOpcionesMapeo(camposDisponibles, campoSeleccionado) {
    return camposDisponibles.map(campo =>
        `<option value="${campo.dato}" ${campo.dato === campoSeleccionado ? 'selected' : ''}>
            ${campo.campo} (${campo.dato})
        </option>`
    ).join('');
}

function contarColumnasMapepadas(columnas) {
    return columnas.filter(col => col.campoMapeado && col.campoMapeado !== '').length;
}

function actualizarMapeo(columnaIndice, nuevoCampo) {
    if (!window.analisisActual) return;

    const columna = window.analisisActual.columnas.find(col => col.indice === columnaIndice);
    if (columna) {
        columna.campoMapeado = nuevoCampo;
        columna.mapeadoAutomatico = false; // Ya no es automático

        // Buscar descripción del campo
        const campoInfo = window.analisisActual.camposDisponibles.find(c => c.dato === nuevoCampo);
        columna.descripcionMapeado = campoInfo ? campoInfo.campo : '';

        // Actualizar contador
        $('#contadorMapeados').text(contarColumnasMapepadas(window.analisisActual.columnas));

        console.log(`✅ Mapeo actualizado: Columna ${columna.letra} → ${nuevoCampo}`);
    }
}

function autoMapearTodas() {
    AbrirMensaje("CONFIRMACIÓN",
        "¿Desea aplicar el mapeo automático a todas las columnas? Esto sobrescribirá los mapeos manuales.",
        function (respuesta) {
            $("#msjModal").modal("hide");
            if (respuesta === "SI") {
                // Re-ejecutar análisis con mapeo automático
                procesarImportacion();
            }
        },
        true, ["Continuar", "Cancelar"], "info!", null);
}

function validarMapeo() {
    if (!window.analisisActual) return;

    const columnasMapeadas = window.analisisActual.columnas.filter(col => col.campoMapeado);
    const columnasRequeridas = ['codigo', 'precio']; // Campos mínimos requeridos

    let mensajeValidacion = `<div class="mb-3">
        <strong>Resumen del Mapeo:</strong><br>
        • ${columnasMapeadas.length} de ${window.analisisActual.columnas.length} columnas mapeadas<br>
        • Campos detectados: ${columnasMapeadas.map(c => c.descripcionMapeado).join(', ')}
    </div>`;

    // Verificar campos requeridos
    const faltantes = columnasRequeridas.filter(req =>
        !columnasMapeadas.some(col => col.campoMapeado.includes(req))
    );

    if (faltantes.length > 0) {
        mensajeValidacion += `<div class="alert alert-warning">
            <strong>Advertencia:</strong> Faltan campos importantes: ${faltantes.join(', ')}
        </div>`;
    }

    AbrirMensaje("Validación de Mapeo", mensajeValidacion,
        () => $("#msjModal").modal("hide"), false, ["Aceptar"],
        faltantes.length > 0 ? "warn!" : "success!", null);
}

// ✅ FUNCIONES AUXILIARES PARA PRESENTACIÓN
function getTipoBadgeClass(tipo) {
    const clases = {
        'Número': 'bg-success',
        'Texto': 'bg-primary',
        'Fecha': 'bg-warning',
        'Vacío': 'bg-secondary'
    };
    return clases[tipo] || 'bg-secondary';
}

function getTipoIcon(tipo) {
    const iconos = {
        'Número': '<i class="bx bx-hash"></i>',
        'Texto': '<i class="bx bx-text"></i>',
        'Fecha': '<i class="bx bx-calendar"></i>',
        'Vacío': '<i class="bx bx-minus"></i>'
    };
    return iconos[tipo] || '<i class="bx bx-question-mark"></i>';
}

function getProgressBarClass(porcentaje) {
    if (porcentaje >= 80) return 'bg-success';
    if (porcentaje >= 50) return 'bg-warning';
    return 'bg-danger';
}

function truncateText(text, maxLength) {
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength - 3) + '...';
}

// ✅ FUNCIONES DE ACCIÓN
function cancelarAnalisis() {
    $('#mainContent').slideUp(400, function () {
        $(this).html('');
    });
}

function configurarMapeoColumnas() {
    AbrirMensaje("INFORMACIÓN",
        "La configuración de mapeo de columnas estará disponible en la próxima versión.",
        () => $("#msjModal").modal("hide"),
        false, ["Aceptar"], "info!", null);
}

function confirmarEIniciarImportacion() {
    AbrirMensaje("CONFIRMACIÓN",
        `¿Desea proceder con la importación del archivo "${archivoSeleccionado.name}"?<br><br>
         <small class="text-muted">Se procesarán todos los registros detectados.</small>`,
        function (respuesta) {
            $("#msjModal").modal("hide");
            if (respuesta === "SI") {
                ejecutarImportacionReal();
            }
        },
        true, ["Continuar", "Cancelar"], "info!", null);
}

// ✅ NUEVA: Función para ejecutar importación real (después del análisis)
function ejecutarImportacionReal() {
    // Mostrar progreso de importación real
    $('#importResults').slideDown(300);
    $('#importProgress').css('width', '0%').text('Iniciando importación...');

    // Crear FormData para importación completa
    const formData = new FormData();
    formData.append('archivo', archivoSeleccionado);
    formData.append('proveedorId', consCta);

    // Llamada AJAX para importación completa
    $.ajax({
        url: procesarExcelUrl,//'@Url.Action("ProcesarExcel", "Importar", new { area = "Productos" })',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        xhr: function () {
            const xhr = new window.XMLHttpRequest();
            xhr.upload.addEventListener("progress", function (evt) {
                if (evt.lengthComputable) {
                    const percentComplete = (evt.loaded / evt.total) * 100;
                    $('#importProgress').css('width', percentComplete + '%').text(`Procesando... ${Math.round(percentComplete)}%`);
                }
            }, false);
            return xhr;
        },
        success: function (response) {
            $('#importProgress').css('width', '100%').text('Importación completada');

            setTimeout(() => {
                $('#importResults').slideUp(300);

                if (response.error) {
                    AbrirMensaje("ERROR", response.mensaje,
                        () => $("#msjModal").modal("hide"), false, ["Aceptar"], "error!", null);
                } else {
                    mostrarResultadosImportacion(response);
                }
            }, 1500);
        },
        error: function (xhr, status, error) {
            $('#importResults').slideUp(300);
            console.error('Error en importación:', error);
            AbrirMensaje("ERROR", "Error de comunicación durante la importación.",
                () => $("#msjModal").modal("hide"), false, ["Aceptar"], "error!", null);
        }
    });
}