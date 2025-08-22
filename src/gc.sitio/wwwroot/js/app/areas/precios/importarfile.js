// Configuración simplificada
const CONFIGURACION_FORMATEO = {
    p_plista: { decimales: 3, tipo: 'moneda' },
    p_dto1: { decimales: 1, tipo: 'porcentaje' },
    p_dto2: { decimales: 1, tipo: 'porcentaje' },
    p_dto3: { decimales: 1, tipo: 'porcentaje' },
    p_dto4: { decimales: 1, tipo: 'porcentaje' },
    p_dto_pa: { decimales: 1, tipo: 'porcentaje' },
    p_porc_flete: { decimales: 1, tipo: 'porcentaje' },
    in_alicuota: { decimales: 1, tipo: 'porcentaje' },
    p_boni: { decimales: 0, tipo: 'string' },
    p_pcosto: { decimales: 2, tipo: 'moneda' }
};

// ✅ CONFIGURACIÓN: URLs centralizadas
const IMPORTAR_URLS = {
    analizarColumnas: '/Productos/Importar/AnalizarColumnas',
    procesarExcel: typeof procesarExcelUrl !== 'undefined' ? procesarExcelUrl : '/Productos/Importar/ProcesarExcel',
    diagnosticarCeldas: '/Productos/Importar/DiagnosticarCeldasCombinadas'
};

$(function () {
    // Configurar URLs si están definidas
    if (typeof procesarExcelUrl !== 'undefined') {
        IMPORTAR_URLS.procesarExcel = procesarExcelUrl;
    }
    if (typeof analizarColumnasUrl !== 'undefined') {
        IMPORTAR_URLS.analizarColumnas = analizarColumnasUrl;
    }

    initializeUploadControls();
    agregarBotonesDiagnostico();
});

// ✅ MANTENER: Solo funciones de formateo esenciales
function FormatearPorcentaje(valor) {
    if (!valor || valor === '0' || valor === 0) return '0%';
    const num = parseFloat(valor);
    return isNaN(num) ? '0%' : `${num.toFixed(1)}%`;
}

function FormatearMoneda(valor, decimales = 2) {
    if (!valor || valor === '0' || valor === 0) return `$ 0.${'0'.repeat(decimales)}`;
    const num = parseFloat(valor);
    return isNaN(num) ? `$ 0.${'0'.repeat(decimales)}` : `$ ${num.toFixed(decimales)}`;
}

function FormatearTexto(valor) {
    return (!valor || valor === '0' || valor === 0) ? '-' : valor.toString();
}

// ✅ SIMPLIFICAR: Función de formateo más directa
function formatearValor(valor, nombreCampo) {
    // Valores vacíos o nulos
    if (valor === null || valor === undefined || valor === '' || valor === '0.00' || valor === '0') {
        if (nombreCampo.includes('dto') || nombreCampo.includes('alicuota') || nombreCampo.includes('flete')) {
            return '0%';
        }
        if (nombreCampo === 'p_boni') {
            return '-';
        }
        if (nombreCampo === 'p_plista' || nombreCampo === 'p_pcosto') {
            return '$ 0.00';
        }
        return '-';
    }

    const config = CONFIGURACION_FORMATEO[nombreCampo];
    if (!config) {
        return valor.toString();
    }

    const valorNum = parseFloat(valor);
    if (isNaN(valorNum)) {
        return valor.toString();
    }

    switch (config.tipo) {
        case 'moneda':
            return `$ ${valorNum.toFixed(config.decimales)}`;
        case 'porcentaje':
            return `${valorNum.toFixed(config.decimales)}%`;
        case 'string':
            return valor.toString();
        default:
            return valorNum.toFixed(config.decimales);
    }
}



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

    // ✅ CORREGIDO: Click en zona de drop - Usar trigger() en lugar de click()
    $dropZone.on('click', function () {
        $fileInput.trigger('click');
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

// ✅ OPTIMIZAR: Función para diagnosticar celdas combinadas
function diagnosticarCeldasCombinadas() {
    const file = getSelectedFile('Importar');
    if (!file) {
        showUploadError('Primero seleccione un archivo Excel para diagnosticar');
        return;
    }

    const formData = new FormData();
    formData.append('archivo', file);

    // ✅ USAR: URL de configuración
    $.ajax({
        url: IMPORTAR_URLS.diagnosticarCeldas,
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        beforeSend: function () {
            // Mostrar indicador de carga
            if (typeof mostrarCargando === 'function') {
                mostrarCargando(true, 'Analizando celdas combinadas...');
            }
        },
        success: function (response) {
            if (response.error) {
                showUploadError('Error: ' + response.mensaje);
                return;
            }
            mostrarDiagnosticoCeldasCombinadas(response.diagnostico);
        },
        error: function (xhr, status, error) {
            console.error('Error diagnosticando celdas:', error);
            showUploadError('Error de comunicación con el servidor');
        },
        complete: function () {
            if (typeof mostrarCargando === 'function') {
                mostrarCargando(false);
            }
        }
    });
}

// ✅ OPTIMIZAR: Modal de diagnóstico mejorado
function mostrarDiagnosticoCeldasCombinadas(diagnostico) {
    // Verificar si ya existe un modal y eliminarlo
    $('#modalDiagnosticoCeldas').remove();

    const html = `
        <div class="modal fade" id="modalDiagnosticoCeldas" tabindex="-1" aria-labelledby="modalDiagnosticoCeldasLabel">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="modalDiagnosticoCeldasLabel">
                            <i class="bx bx-merge-cells me-2"></i>Diagnóstico de Celdas Combinadas
                        </h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <div class="row mb-3">
                            <div class="col-md-6">
                                <h6><i class="bx bx-info-circle text-info me-1"></i>Información General:</h6>
                                <ul class="list-unstyled ms-3">
                                    <li><strong>Archivo:</strong> ${diagnostico.nombreArchivo}</li>
                                    <li><strong>Hoja:</strong> ${diagnostico.nombreHoja}</li>
                                    <li><strong>Dimensiones:</strong> ${diagnostico.totalFilas} × ${diagnostico.totalColumnas}</li>
                                    <li><strong>Celdas combinadas:</strong> 
                                        <span class="badge bg-${diagnostico.cantidadCeldasCombinadas > 0 ? 'warning' : 'success'}">
                                            ${diagnostico.cantidadCeldasCombinadas}
                                        </span>
                                    </li>
                                </ul>
                            </div>
                            <div class="col-md-6">
                                <h6><i class="bx bx-target text-warning me-1"></i>Impacto en Encabezados:</h6>
                                <ul class="list-unstyled ms-3">
                                    <li><strong>Filas afectadas:</strong> 
                                        ${diagnostico.impactoEncabezados.filasAfectadas.length > 0 ?
            diagnostico.impactoEncabezados.filasAfectadas.join(', ') : 'Ninguna'}
                                    </li>
                                    <li><strong>Posibles encabezados perdidos:</strong> 
                                        <span class="badge bg-${diagnostico.impactoEncabezados.posiblesEncabezadosPerdidos.length > 0 ? 'danger' : 'success'}">
                                            ${diagnostico.impactoEncabezados.posiblesEncabezadosPerdidos.length}
                                        </span>
                                    </li>
                                </ul>
                            </div>
                        </div>
                        
                        ${diagnostico.cantidadCeldasCombinadas > 0 ? `
                            <h6><i class="bx bx-table text-primary me-1"></i>Detalle de Celdas Combinadas:</h6>
                            <div class="table-responsive" style="max-height: 300px;">
                                <table class="table table-sm table-striped">
                                    <thead class="table-dark">
                                        <tr><th>Rango</th><th>Valor</th><th>Filas</th><th>Columnas</th></tr>
                                    </thead>
                                    <tbody>
                                        ${diagnostico.celdasCombinadas.map(cc => `
                                            <tr>
                                                <td><code class="text-primary">${cc.rango}</code></td>
                                                <td><strong>${cc.valor || '<span class="text-muted">(vacío)</span>'}</strong></td>
                                                <td><span class="badge bg-secondary">${cc.filas}</span></td>
                                                <td><span class="badge bg-secondary">${cc.columnas}</span></td>
                                            </tr>
                                        `).join('')}
                                    </tbody>
                                </table>
                            </div>
                        ` : '<div class="alert alert-success"><i class="bx bx-check-circle me-2"></i>No se encontraron celdas combinadas</div>'}
                        
                        ${diagnostico.impactoEncabezados.posiblesEncabezadosPerdidos.length > 0 ? `
                            <div class="alert alert-warning mt-3">
                                <h6><i class="bx bx-warning"></i> Posibles Encabezados Perdidos:</h6>
                                <ul class="mb-0">
                                    ${diagnostico.impactoEncabezados.posiblesEncabezadosPerdidos.map(enc => `
                                        <li><strong>"${enc.valor}"</strong> en fila ${enc.filaConValor} 
                                        (afecta filas: <span class="badge bg-warning text-dark">${enc.filasVacias.join(', ')}</span>)</li>
                                    `).join('')}
                                </ul>
                            </div>
                        ` : ''}
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                            <i class="bx bx-x me-1"></i>Cerrar
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;

    $('body').append(html);
    $('#modalDiagnosticoCeldas').modal('show').on('hidden.bs.modal', function () {
        $(this).remove();
    });
}

// ✅ SIMPLIFICAR: Solo agregar botón de diagnóstico de celdas combinadas
function agregarBotonesDiagnostico() {
    if ($('#btnDiagnosticoCeldas').length === 0) {
        const boton = `
            <button type="button" id="btnDiagnosticoCeldas" class="btn btn-outline-warning btn-sm ms-2" onclick="diagnosticarCeldasCombinadas()" title="Diagnosticar celdas combinadas">
                <i class="bx bx-merge-cells"></i> Celdas Combinadas
            </button>
        `;
        $('#btnToggleUpload').parent().append(boton);
    }
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

    console.log(`✅ Archivo seleccionado (${uploadId}):`, file.name, formatFileSize(file.size));
}

// Validar archivo
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
    if ($progressContainer.length) {
        $progressContainer.hide();
    }
    $dropZone.show();

    // Disparar evento personalizado
    $(document).trigger('fileRemoved', [uploadId]);

    console.log(`🗑️ Archivo removido (${uploadId})`);
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
    // ✅ MEJORAR: Usar sistema de notificaciones más robusto
    if (typeof AbrirMensaje === 'function') {
        AbrirMensaje("ERROR", message, () => $("#msjModal").modal("hide"), false, ["Aceptar"], "error!", null);
    } else if (typeof showNotification === 'function') {
        showNotification('error', message);
    } else {
        console.error('Upload Error:', message);
        alert(message);
    }
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

// ✅ OPTIMIZAR: Función principal de procesamiento
function procesarImportacion() {
    if (!archivoSeleccionado) {
        AbrirMensaje("ATENCIÓN", "No hay ningún archivo seleccionado para procesar.",
            () => $("#msjModal").modal("hide"), false, ["Aceptar"], "warn!", null);
        return;
    }

    $('#importResults').slideDown(300);
    $('#importProgress').css('width', '20%').text('Analizando estructura...');

    const formData = new FormData();
    formData.append('archivo', archivoSeleccionado);

    $.ajax({
        url: IMPORTAR_URLS.analizarColumnas,
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

// ✅ MANTENER: Función de mostrar análisis (ACTIVA)
function mostrarAnalisisColumnas(analisis) {
    const htmlAnalisis = `
        <div class="row mt-3">
            <div class="col-12">
                <!-- Información general del archivo -->
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

                <!-- Tabla de análisis de columnas -->
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
                        <div class="table-responsive text-nowrap table-wrapper-500">
                            <table class="table table-hover mb-0 table-wrapper-fixed-head">
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

                <!-- Botones de acción -->
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
                        <button type="button" class="btn btn-success" onclick="cargarEIniciarImportacion()">
                            <i class="bx bx-check-double me-1"></i> Cargar archivo
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;

    $('#mainContent').html(htmlAnalisis).hide().slideDown(400);
    window.analisisActual = analisis;
}

// ✅ MANTENER: Funciones de soporte para mapeo (ACTIVAS)
function generarOpcionesMapeo(camposDisponibles, campoSeleccionado) {
    if (!camposDisponibles || !Array.isArray(camposDisponibles)) {
        return '<option value="">No hay campos disponibles</option>';
    }

    return camposDisponibles.map(campo =>
        `<option value="${campo.campo || campo.Campo}" ${(campo.campo || campo.Campo) === campoSeleccionado ? 'selected' : ''}>
            ${campo.dato || campo.Dato} (${campo.campo || campo.Campo})
        </option>`
    ).join('');
}

function contarColumnasMapepadas(columnas) {
    if (!columnas || !Array.isArray(columnas)) return 0;
    return columnas.filter(col => col.campoMapeado && col.campoMapeado !== '').length;
}

function actualizarMapeo(columnaIndice, nuevoCampo) {
    if (!window.analisisActual) return;

    const columna = window.analisisActual.columnas.find(col => col.indice === columnaIndice);
    if (columna) {
        columna.campoMapeado = nuevoCampo;
        columna.mapeadoAutomatico = false;

        // Buscar descripción del campo
        const campoInfo = window.analisisActual.camposDisponibles.find(c =>
            (c.dato || c.Dato) === nuevoCampo);
        columna.descripcionMapeado = campoInfo ? (campoInfo.campo || campoInfo.Campo) : '';

        // Actualizar contador
        $('#contadorMapeados').text(contarColumnasMapepadas(window.analisisActual.columnas));

        console.log(`✅ Mapeo actualizado: Columna ${columna.letra} → ${nuevoCampo}`);
    }
}

function autoMapearTodas() {
    AbrirMensaje("CONFIRMACIÓN",
        "¿Desea aplicar el mapeo automático a todas las columnas?",
        function (respuesta) {
            $("#msjModal").modal("hide");
            if (respuesta === "SI") {
                procesarImportacion();
            }
        },
        true, ["Continuar", "Cancelar"], "info!", null);
}

function validarMapeo() {
    if (!window.analisisActual) return;

    const columnasMapeadas = window.analisisActual.columnas.filter(col => col.campoMapeado);
    const columnasRequeridas = ['p_ean', 'p_plista'];

    let mensajeValidacion = `<div class="mb-3">
        <strong>Resumen del Mapeo:</strong><br>
        • ${columnasMapeadas.length} de ${window.analisisActual.columnas.length} columnas mapeadas<br>
        • Campos detectados: ${columnasMapeadas.map(c => c.descripcionMapeado).join(', ')}
    </div>`;

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

// ✅ MANTENER: Funciones auxiliares para presentación (ACTIVAS)
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
    if (!text || text.length <= maxLength) return text;
    return text.substring(0, maxLength - 3) + '...';
}

// ✅ MANTENER: Funciones de acción (ACTIVAS)
function cancelarAnalisis() {
    $('#mainContent').slideUp(400, function () {
        $(this).html('');
    });
    // Limpiar referencia global
    window.analisisActual = null;
}

function cargarEIniciarImportacion() {
    if (!archivoSeleccionado) {
        showUploadError('No hay archivo seleccionado para importar');
        return;
    }

    AbrirMensaje("CONFIRMACIÓN",
        `¿Desea proceder con la importación del archivo "${archivoSeleccionado.name}"?`,
        function (respuesta) {
            $("#msjModal").modal("hide");
            if (respuesta === "SI") {
                ejecutarImportacionReal();
            }
        },
        true, ["Continuar", "Cancelar"], "info!", null);
}

// ✅ SIMPLIFICAR: Función de importación sin validaciones innecesarias
function ejecutarImportacionReal() {
    if (!archivoSeleccionado) {
        showUploadError('No hay archivo seleccionado para procesar');
        return;
    }

    $('#importResults').slideDown(300);
    $('#importProgress').css('width', '0%').text('Iniciando importación...');

    const formData = new FormData();
    formData.append('archivo', archivoSeleccionado);
    formData.append('proveedorId', consCta);

    // ✅ ENVIAR: Mapeo de columnas del usuario
    if (window.analisisActual && window.analisisActual.columnas) {
        const mapeoColumnas = {};
        window.analisisActual.columnas
            .filter(col => col.campoMapeado)
            .forEach(col => {
                mapeoColumnas[col.indice] = col.campoMapeado;
            });

        if (Object.keys(mapeoColumnas).length > 0) {
            formData.append('mapeoColumnas', JSON.stringify(mapeoColumnas));
            console.log('✅ Enviando mapeo de columnas:', mapeoColumnas);
        }
    }

    $.ajax({
        url: IMPORTAR_URLS.procesarExcel,
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            $('#importProgress').css('width', '100%').text('Importación completada');
            window.ultimaRespuestaImportacion = response;

            console.log('✅ Respuesta recibida:', {
                error: response.error,
                tieneVista: !!response.vistaResultados
            });

            setTimeout(() => {
                $('#importResults').slideUp(300);

                if (response.error) {
                    AbrirMensaje("ERROR", `Error: ${response.mensaje}`,
                        () => $("#msjModal").modal("hide"), false, ["Aceptar"], "error!", null);
                } else {
                    mostrarResultadosImportacion(response);
                }
            }, 1000);
        },
        error: function (xhr, status, error) {
            $('#importResults').slideUp(300);
            console.error('❌ Error:', error);

            AbrirMensaje("ERROR", "Error de comunicación con el servidor.",
                () => $("#msjModal").modal("hide"), false, ["Aceptar"], "error!", null);
        }
    });
}

function mostrarResultadosBasicos(response) {
    const { datos, mensaje } = response;

    const htmlBasico = `
        <div class="alert alert-info">
            <h5><i class="bx bx-info-circle me-2"></i>Importación Completada</h5>
            <p>${mensaje || 'El proceso de importación ha finalizado.'}</p>
            
            ${datos ? `
                <div class="mt-3">
                    <strong>Resumen:</strong>
                    <ul class="mb-0">
                        <li>Registros procesados: <strong>${datos.registrosProcesados || 0}</strong></li>
                        <li>Registros exitosos: <strong class="text-success">${datos.registrosExitosos || 0}</strong></li>
                        <li>Registros con error: <strong class="text-warning">${datos.registrosConError || 0}</strong></li>
                        <li>Archivo: <strong>${datos.archivo || 'N/A'}</strong></li>
                    </ul>
                </div>
            ` : ''}
        </div>
        
        <div class="text-center mt-3">
            <button type="button" class="btn btn-primary" onclick="location.reload()">
                <i class="bx bx-refresh me-1"></i>Nueva Importación
            </button>
        </div>
    `;

    $('#mainContent').html(htmlBasico);

    setTimeout(() => {
        manejarArchivoRemovido();
    }, 2000);
}

// ✅ OPTIMIZAR: Mostrar resultados exitosos con vista parcial incluida
function mostrarResultadosImportacionExitosa(response) {
    console.warn('⚠️ mostrarResultadosImportacionExitosa está obsoleta. Usar mostrarResultadosImportacion');
    mostrarResultadosImportacion(response);
}

// ✅ MEJORAR: Función para exportar resultados con más detalles
function exportarResultados() {
    if (!window.ultimaRespuestaImportacion) {
        AbrirMensaje("INFORMACIÓN",
            "No hay datos de importación para exportar.",
            () => $("#msjModal").modal("hide"),
            false, ["Aceptar"], "info!", null);
        return;
    }

    try {
        const { datos } = window.ultimaRespuestaImportacion;
        const estado = analizarEstadoImportacion(datos);

        // ✅ CSV con más información
        let csvContent = "Estado,Total,Exitosos,Errores,Porcentaje_Exito,Archivo,Fecha\n";
        csvContent += `"${estado.esExitoso ? 'Exitoso' : estado.tieneMixto ? 'Mixto' : 'Con Errores'}",`;
        csvContent += `"${estado.total}","${estado.exitosos}","${estado.errores}","${estado.porcentajeExito}%",`;
        csvContent += `"${datos.archivo}","${datos.fechaProceso}"\n`;

        // Descargar archivo
        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement("a");
        link.href = URL.createObjectURL(blob);
        link.download = `resultados_importacion_${new Date().toISOString().slice(0, 19).replace(/:/g, '-')}.csv`;
        link.click();

        console.log('✅ Resultados exportados exitosamente');
    } catch (error) {
        console.error('❌ Error exportando resultados:', error);
        showUploadError('Error al exportar los resultados');
    }
}

// ✅ AGREGAR: Función auxiliar para manejar archivo removido (si no existe)
function manejarArchivoRemovido() {
    if (typeof archivoSeleccionado !== 'undefined') {
        archivoSeleccionado = null;
    }

    // Ocultar información del archivo si existe
    if ($('#fileSelectedInfo').length) {
        $('#fileSelectedInfo').fadeOut(300);
    }

    // Deshabilitar botón procesar si existe
    if ($('#btnProcesarArchivo').length) {
        $('#btnProcesarArchivo').prop('disabled', true).fadeOut(300);
    }

    console.log('🗑️ Estado de archivo removido');
}

// ✅ AGREGAR: Función auxiliar para toggle de área de upload (si no existe)
function toggleUploadArea() {
    const $uploadArea = $('#uploadArea');
    if ($uploadArea.length === 0) return;

    const isVisible = $uploadArea.is(':visible');

    if (isVisible) {
        $uploadArea.slideUp(300);
        if (typeof uploadAreaVisible !== 'undefined') {
            uploadAreaVisible = false;
        }
    } else {
        $uploadArea.slideDown(300);
        if (typeof uploadAreaVisible !== 'undefined') {
            uploadAreaVisible = true;
        }
    }
}

// ✅ NUEVA: Función para ver detalles de la importación
function verDetalleImportacion() {
    if (!window.ultimaRespuestaImportacion) {
        AbrirMensaje("INFORMACIÓN",
            "No hay detalles adicionales disponibles para esta importación.",
            () => $("#msjModal").modal("hide"),
            false, ["Aceptar"], "info!", null);
        return;
    }

    const { datos } = window.ultimaRespuestaImportacion;

    const detalleHtml = `
        <div class="modal fade" id="modalDetalleImportacion" tabindex="-1">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">
                            <i class="bx bx-detail me-2"></i>Detalle de la Importación
                        </h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <div class="row mb-3">
                            <div class="col-md-6">
                                <h6><i class="bx bx-file text-primary me-1"></i>Información del Archivo:</h6>
                                <ul class="list-unstyled ms-3">
                                    <li><strong>Archivo:</strong> ${datos.archivo}</li>
                                    <li><strong>Proveedor:</strong> ${datos.proveedor}</li>
                                    <li><strong>Fecha proceso:</strong> ${datos.fechaProceso}</li>
                                </ul>
                            </div>
                            <div class="col-md-6">
                                <h6><i class="bx bx-data text-success me-1"></i>Estadísticas:</h6>
                                <ul class="list-unstyled ms-3">
                                    <li><strong>Registros:</strong> ${datos.registrosProcesados}</li>
                                    <li><strong>Columnas:</strong> ${datos.columnasUtilizadas}</li>
                                </ul>
                            </div>
                        </div>

                        ${datos.detalleResultado ? `
                            <h6><i class="bx bx-info-circle text-info me-1"></i>Resultado del Proceso:</h6>
                            <div class="bg-light p-3 rounded">
                                <pre class="mb-0">${datos.detalleResultado}</pre>
                            </div>
                        ` : ''}

                        ${window.analisisActual && window.analisisActual.columnas ? `
                            <h6 class="mt-3"><i class="bx bx-table text-warning me-1"></i>Mapeo de Columnas Utilizado:</h6>
                            <div class="table-responsive" style="max-height: 300px;">
                                <table class="table table-sm table-striped">
                                    <thead class="table-dark">
                                        <tr>
                                            <th>Columna</th>
                                            <th>Encabezado Excel</th>
                                            <th>Campo BD</th>
                                            <th>Confianza</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        ${window.analisisActual.columnas
                .filter(col => col.campoMapeado)
                .map(col => `
                                                <tr>
                                                    <td><span class="badge bg-secondary">${col.letra}</span></td>
                                                    <td>${col.encabezado}</td>
                                                    <td><code>${col.campoMapeado}</code></td>
                                                    <td>
                                                        <span class="badge ${col.confianzaMapeo >= 80 ? 'bg-success' : col.confianzaMapeo >= 60 ? 'bg-warning' : 'bg-danger'}">
                                                            ${col.confianzaMapeo}%
                                                        </span>
                                                        ${col.mapeadoAutomatico ?
                        '<i class="bx bx-magic-wand ms-1" title="Automático"></i>' :
                        '<i class="bx bx-user ms-1" title="Manual"></i>'
                    }
                                                    </td>
                                                </tr>
                                            `).join('')}
                                    </tbody>
                                </table>
                            </div>
                        ` : ''}
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                            <i class="bx bx-x me-1"></i>Cerrar
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;

    $('body').append(detalleHtml);
    $('#modalDetalleImportacion').modal('show').on('hidden.bs.modal', function () {
        $(this).remove();
    });
}

// ✅ CORREGIR: Función principal para mostrar resultados según el estado real
function mostrarResultadosImportacion(response) {
    const { datos, mensaje, vistaResultados } = response;

    if (!datos) {
        // Sin datos estadísticos, mostrar básico
        mostrarResultadosBasicos(response);
        return;
    }

    // ✅ ANALIZAR: Estado real de la importación
    const estadoImportacion = analizarEstadoImportacion(datos);

    if (estadoImportacion.esExitoso) {
        mostrarResultadosExitosos(response, estadoImportacion);
    } else if (estadoImportacion.tieneMixto) {
        mostrarResultadosMixtos(response, estadoImportacion);
    } else {
        mostrarResultadosConErrores(response, estadoImportacion);
    }
}

// ✅ NUEVA: Analizar el estado real de la importación
function analizarEstadoImportacion(datos) {
    const total = datos.registrosProcesados || 0;
    const exitosos = datos.registrosExitosos || 0;
    const errores = datos.registrosConError || 0;

    const porcentajeExito = total > 0 ? Math.round((exitosos / total) * 100) : 0;

    return {
        total: total,
        exitosos: exitosos,
        errores: errores,
        porcentajeExito: porcentajeExito,
        esExitoso: errores === 0 && exitosos > 0,           // Solo éxitos
        tieneMixto: errores > 0 && exitosos > 0,            // Mixto: éxitos y errores
        soloErrores: errores > 0 && exitosos === 0,         // Solo errores
        sinProcesar: total === 0                            // No procesó nada
    };
}

// ✅ CORREGIR: Función para resultados completamente exitosos
function mostrarResultadosExitosos(response, estado) {
    const { datos, mensaje, vistaResultados } = response;

    const htmlResultados = `
        <div class="alert alert-success alert-dismissible fade show" role="alert">
            <div class="d-flex align-items-center">
                <i class="bx bx-check-circle bx-lg text-success me-3"></i>
                <div class="flex-grow-1">
                    <h5 class="alert-heading mb-2">¡Importación Completada Exitosamente!</h5>
                    <p class="mb-2">Todos los ${estado.exitosos} registros fueron procesados correctamente.</p>
                    
                    <div class="row text-center mt-3">
                        <div class="col-md-4">
                            <h6 class="text-success mb-0">${estado.total}</h6>
                            <small class="text-muted">Total Procesados</small>
                        </div>
                        <div class="col-md-4">
                            <h6 class="text-success mb-0">${estado.exitosos}</h6>
                            <small class="text-muted">Exitosos</small>
                        </div>
                        <div class="col-md-4">
                            <h6 class="text-info mb-0">${truncateText(datos.archivo || 'N/A', 20)}</h6>
                            <small class="text-muted">Archivo</small>
                        </div>
                    </div>
                </div>
            </div>
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
        
        ${generarBotonesAccion()}
        ${generarContenedorResultados(vistaResultados)}
    `;

    mostrarContenidoFinal(htmlResultados);
}

// ✅ NUEVA: Función para resultados mixtos (éxitos y errores)
function mostrarResultadosMixtos(response, estado) {
    const { datos, mensaje, vistaResultados } = response;

    const htmlResultados = `
        <div class="alert alert-warning alert-dismissible fade show" role="alert">
            <div class="d-flex align-items-center">
                <i class="bx bx-error-circle bx-lg text-warning me-3"></i>
                <div class="flex-grow-1">
                    <h5 class="alert-heading mb-2">Importación Completada con Advertencias</h5>
                    <p class="mb-2">
                        Se procesaron ${estado.exitosos} registros exitosamente, pero ${estado.errores} 
                        registros presentaron errores que requieren revisión.
                    </p>
                    
                    <div class="row text-center mt-3">
                        <div class="col-md-3">
                            <h6 class="text-info mb-0">${estado.total}</h6>
                            <small class="text-muted">Total</small>
                        </div>
                        <div class="col-md-3">
                            <h6 class="text-success mb-0">${estado.exitosos}</h6>
                            <small class="text-muted">Exitosos</small>
                        </div>
                        <div class="col-md-3">
                            <h6 class="text-warning mb-0">${estado.errores}</h6>
                            <small class="text-muted">Con Errores</small>
                        </div>
                        <div class="col-md-3">
                            <h6 class="text-success mb-0">${estado.porcentajeExito}%</h6>
                            <small class="text-muted">Éxito</small>
                        </div>
                    </div>
                </div>
            </div>
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
        
        ${generarBotonesAccion()}
        ${generarContenedorResultados(vistaResultados)}
    `;

    mostrarContenidoFinal(htmlResultados);
}

// ✅ NUEVA: Función para resultados con errores predominantes
function mostrarResultadosConErrores(response, estado) {
    const { datos, mensaje, vistaResultados } = response;

    const htmlResultados = `
        <div class="alert alert-danger alert-dismissible fade show" role="alert">
            <div class="d-flex align-items-center">
                <i class="bx bx-x-circle bx-lg text-danger me-3"></i>
                <div class="flex-grow-1">
                    <h5 class="alert-heading mb-2">Importación Completada con Errores</h5>
                    <p class="mb-2">
                        ${estado.soloErrores ?
            `Todos los ${estado.errores} registros presentaron errores y no pudieron ser procesados.` :
            `La mayoría de registros (${estado.errores}) presentaron errores. Solo ${estado.exitosos} fueron procesados exitosamente.`
        }
                    </p>
                    
                    <div class="row text-center mt-3">
                        <div class="col-md-4">
                            <h6 class="text-info mb-0">${estado.total}</h6>
                            <small class="text-muted">Total</small>
                        </div>
                        <div class="col-md-4">
                            <h6 class="text-danger mb-0">${estado.errores}</h6>
                            <small class="text-muted">Con Errores</small>
                        </div>
                        <div class="col-md-4">
                            <h6 class="text-success mb-0">${estado.exitosos}</h6>
                            <small class="text-muted">Exitosos</small>
                        </div>
                    </div>
                </div>
            </div>
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
        
        ${generarBotonesAccion()}
        ${generarContenedorResultados(vistaResultados)}
    `;

    mostrarContenidoFinal(htmlResultados);
}

// ✅ NUEVA: Generar botones de acción reutilizable
function generarBotonesAccion() {
    return `
        <div class="d-flex justify-content-between align-items-center mb-3">
            <h6 class="mb-0"><i class="bx bx-list-ul me-2"></i>Resultados Detallados de la Importación</h6>
            <div class="d-flex gap-2">
                <button type="button" class="btn btn-outline-secondary btn-sm" onclick="location.reload()">
                    <i class="bx bx-refresh me-1"></i>Nueva Importación
                </button>
                <button type="button" class="btn btn-outline-info btn-sm" onclick="exportarResultados()">
                    <i class="bx bx-download me-1"></i>Exportar Resultados
                </button>
                <button type="button" class="btn btn-outline-primary btn-sm" onclick="verDetalleImportacion()">
                    <i class="bx bx-detail me-1"></i>Ver Detalles
                </button>
            </div>
        </div>
    `;
}

// ✅ NUEVA: Generar contenedor de resultados reutilizable
function generarContenedorResultados(vistaResultados) {
    return `
        <!-- ✅ CONTENEDOR: Para la vista parcial de resultados -->
        <div id="contenedorResultadosDetallados" class="mt-3">
            ${vistaResultados && vistaResultados.trim().length > 0 ?
            vistaResultados :
            '<div class="alert alert-info"><i class="bx bx-info-circle me-2"></i>No hay resultados detallados disponibles</div>'
        }
        </div>
    `;
}

// ✅ SIMPLIFICAR: Función de mostrar contenido sin formateo innecesario
function mostrarContenidoFinal(html) {
    $('#mainContent').html(html);

    // ✅ SCROLL: Hacia los resultados
    setTimeout(() => {
        if ($('#contenedorResultadosDetallados').length > 0) {
            $('html, body').animate({
                scrollTop: $('#contenedorResultadosDetallados').offset().top - 100
            }, 800);
        }
    }, 500);

    // ✅ LIMPIAR: Estado después de mostrar resultados
    setTimeout(() => {
        manejarArchivoRemovido();
        if (typeof uploadAreaVisible !== 'undefined' && uploadAreaVisible) {
            toggleUploadArea();
        }
    }, 2000);
}