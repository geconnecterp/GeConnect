$(function () {
    let productos = [];
    let archivoSeleccionado = null;
    let uploadAreaVisible = false;
    let opcionAnterior = '1'; // ✅ NUEVA: Bandera para rastrear la opción anterior

    // Inicializar eventos
    inicializarEventos();

    function inicializarEventos() {
        $('#btnCargar').on('click', cargarProductos);
        $('#btnConfirmar').on('click', confirmarCambios);
        $('#btnCancelar').on('click', cancelar);
        $('#chkSelectAll').on('change', seleccionarTodos);
        $(document).on('change', '.chk-producto', actualizarCheckGeneral);
        $(document).on('click', '.btn-eliminar-producto', eliminarProducto);
        
        // ✅ NUEVO: Eventos para radio buttons
        $('input[name="opcionDiscontinuos"]').on('click', manejarCambioOpcion);
        
        // ✅ NUEVO: Eventos para upload
        $('#fileInputDiscon').on('change', manejarSeleccionArchivo);
        $('#btnCerrarUpload').on('click', ocultarUploadArea);
    }

    // ✅ OPTIMIZADA: Función para manejar cambio de opción con bandera
    function manejarCambioOpcion() {
        const $radio = $(this);
        const valorActual = $radio.val();

        // Si se hace click en la distinta opción que la que estaba seleccionada
        if (valorActual !== opcionAnterior) {
            // Lanzar trigger del botón cancelar
            $('#btnCancelar').trigger('click');
        }

        // Actualizar la bandera con la nueva opción seleccionada
        opcionAnterior = valorActual;

        // Si se selecciona opción 2 o 3, mostrar área de upload
        if (valorActual === '2' || valorActual === '3') {
            mostrarUploadArea();
        } else {
            ocultarUploadArea();
        }
    }

    // ✅ NUEVA: Mostrar área de upload
    function mostrarUploadArea() {
        if (!uploadAreaVisible) {
            $('#uploadArea').slideDown(300);
            uploadAreaVisible = true;
        }
    }

    // ✅ NUEVA: Ocultar área de upload
    function ocultarUploadArea() {
        if (uploadAreaVisible) {
            $('#uploadArea').slideUp(300);
            uploadAreaVisible = false;
            limpiarArchivoSeleccionado();
        }
    }

    // ✅ NUEVA: Manejar selección de archivo
    function manejarSeleccionArchivo(event) {
        const file = event.target.files[0];
        
        if (!file) {
            limpiarArchivoSeleccionado();
            return;
        }

        // Validar extensión
        const extension = file.name.split('.').pop().toLowerCase();
        if (!['xls', 'xlsx', 'txt'].includes(extension)) {
            AbrirMensaje(
                "ATENCIÓN",
                "El archivo debe ser .xls, .xlsx o .txt",
                function () {
                    $("#msjModal").modal("hide");
                },
                false,
                ["Entendido"],
                "warn!",
                null
            );
            limpiarArchivoSeleccionado();
            return;
        }

        archivoSeleccionado = file;
        
        // Mostrar información del archivo
        $('#selectedFileName').text(file.name);
        $('#selectedFileSize').text(`(${formatearTamanioArchivo(file.size)})`);
        $('#fileSelectedInfo').fadeIn(300);
    }

    // ✅ NUEVA: Limpiar archivo seleccionado
    function limpiarArchivoSeleccionado() {
        archivoSeleccionado = null;
        $('#fileInputDiscon').val('');
        $('#fileSelectedInfo').fadeOut(300);
    }

    // ✅ CORREGIDA: Formatear tamaño de archivo (sin espacio en el nombre)
    function formatearTamanioArchivo(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
    }

    // ✅ MODIFICADA: Función cargarProductos con validaciones adicionales
    function cargarProductos() {
        const opcion = $('input[name="opcionDiscontinuos"]:checked').val();
        
        if (!opcion) {
            AbrirMensaje(
                "ATENCIÓN",
                "Debe seleccionar una opción",
                function () {
                    $("#msjModal").modal("hide");
                },
                false,
                ["Entendido"],
                "warn!",
                null
            );
            return;
        }

        // ✅ VALIDACIÓN MEJORADA: Solo para opciones 2 y 3 se requiere archivo
        if (opcion === '2' || opcion === '3') {
            if (!archivoSeleccionado) {
                AbrirMensaje(
                    "ATENCIÓN",
                    "Debe seleccionar un archivo para esta opción",
                    function () {
                        $("#msjModal").modal("hide");
                    },
                    false,
                    ["Entendido"],
                    "warn!",
                    null
                );
                return;
            }
            
            // Mostrar spinner
            mostrarCargando('#tbodyDiscontinuos');
            AbrirWaiting();
            
            // Procesar archivo
            procesarArchivoYCargar(opcion);
        } else {
            // Opción 1: Sin archivo
            mostrarCargando('#tbodyDiscontinuos');
            AbrirWaiting();
            cargarSinArchivo(opcion);
        }
    }

    // ✅ NUEVA: Procesar archivo y cargar datos
    function procesarArchivoYCargar(opcion) {
        const reader = new FileReader();
        
        reader.onload = function(e) {
            try {
                const contenido = e.target.result;
                const extension = archivoSeleccionado.name.split('.').pop().toLowerCase();
                let listaProductos = [];

                if (extension === 'txt') {
                    // Procesar archivo TXT
                    listaProductos = procesarArchivoTXT(contenido);
                } else if (extension === 'xls' || extension === 'xlsx') {
                    // Procesar archivo Excel
                    listaProductos = procesarArchivoExcel(contenido);
                }

                if (listaProductos.length === 0) {
                    CerrarWaiting();
                    mostrarTablaVacia();
                    AbrirMensaje(
                        "ATENCIÓN",
                        "El archivo no contiene datos válidos (valores numéricos)",
                        function () {
                            $("#msjModal").modal("hide");
                        },
                        false,
                        ["Entendido"],
                        "warn!",
                        null
                    );
                    return;
                }

                // Enviar datos al servidor
                enviarDatosAlServidor(opcion, listaProductos);

            } catch (error) {
                CerrarWaiting();
                mostrarTablaVacia();
                console.error('Error al procesar archivo:', error);
                AbrirMensaje(
                    "ERROR",
                    "Error al procesar el archivo: " + error.message,
                    function () {
                        $("#msjModal").modal("hide");
                    },
                    false,
                    ["Entendido"],
                    "error!",
                    null
                );
            }
        };

        reader.onerror = function() {
            CerrarWaiting();
            mostrarTablaVacia();
            AbrirMensaje(
                "ERROR",
                "Error al leer el archivo",
                function () {
                    $("#msjModal").modal("hide");
                },
                false,
                ["Entendido"],
                "error!",
                null
            );
        };

        // Leer archivo según extensión
        const extension = archivoSeleccionado.name.split('.').pop().toLowerCase();
        if (extension === 'txt') {
            reader.readAsText(archivoSeleccionado);
        } else {
            reader.readAsBinaryString(archivoSeleccionado);
        }
    }

    // ✅ NUEVA: Procesar archivo TXT
    function procesarArchivoTXT(contenido) {
        const lineas = contenido.split(/\r?\n/);
        const listaProductos = [];

        lineas.forEach(linea => {
            const valor = linea.trim();
            // Validar que sea numérico (solo dígitos)
            if (valor && /^\d+$/.test(valor)) {
                listaProductos.push(valor);
            }
        });

        return listaProductos;
    }

    // ✅ NUEVA: Procesar archivo Excel (requiere librería XLSX)
    function procesarArchivoExcel(data) {
        // Nota: Requiere la librería xlsx.js incluida en la página
        if (typeof XLSX === 'undefined') {
            throw new Error('Librería XLSX no está cargada. Incluir <script src="https://cdn.sheetjs.com/xlsx-latest/package/dist/xlsx.full.min.js"></script>');
        }

        const workbook = XLSX.read(data, { type: 'binary' });
        const firstSheetName = workbook.SheetNames[0];
        const worksheet = workbook.Sheets[firstSheetName];
        const jsonData = XLSX.utils.sheet_to_json(worksheet, { header: 1 });

        const listaProductos = [];

        jsonData.forEach(row => {
            if (row.length > 0) {
                const valor = String(row[0]).trim();
                // Validar que sea numérico (solo dígitos)
                if (valor && /^\d+$/.test(valor)) {
                    listaProductos.push(valor);
                }
            }
        });

        return listaProductos;
    }

    // ✅ NUEVA: Enviar datos al servidor
    function enviarDatosAlServidor(opcion, listaProductos) {
        const filtros = {
            opcion: opcion,
            lista: listaProductos
        };

        $.ajax({
            url: obtenerDatosUrl,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(filtros),
            success: function (response) {
                CerrarWaiting();
                
                if (response.error === true) {
                    mostrarTablaVacia();
                    AbrirMensaje("ATENCIÓN", response.msg, function () {
                        $("#msjModal").modal("hide");
                    }, false, ["Entendido"], "error!", null);
                }
                else if (response.warn === true) {
                    mostrarTablaVacia();
                    AbrirMensaje("ATENCIÓN", response.msg, function () {
                        $("#msjModal").modal("hide");
                    }, false, ["Entendido"], "warn!", null);
                }
                else {
                    productos = response.lista.map(item => ({
                        p_id: item.p_id,
                        p_id_ok: item.p_id_ok,
                        codigo: item.p_id_ok,
                        descripcion: item.p_desc,
                        estado: item.p_activo_desc,
                        proveedor: item.cta_denominacion,
                        stock: item.stk,
                        procesado: item.procesado,
                        procesado_desc: item.procesado_desc
                    }));

                    renderizarTabla();

                    $("#btnConfirmar").prop("disabled", false);
                    // Ocultar área de upload después de cargar
                    ocultarUploadArea();
                }
            },
            error: function (xhr, status, error) {
                CerrarWaiting();
                productos = [];
                $('#tbodyDiscontinuos').html(`
                    <tr>
                        <td colspan="7" class="text-center py-4">
                            <i class="bx bx-error-circle bx-md text-danger"></i>
                            <p class="text-danger mb-0">Error al cargar productos: ${error}</p>
                        </td>
                    </tr>
                `);
                console.error('Error:', error);
            }
        });
    }

    // ✅ FUNCIÓN ORIGINAL: Cargar sin archivo (opción 1)
    function cargarSinArchivo(opcion) {
        const filtros = {
            opcion,
            lista: []
        };

        $.ajax({
            url: obtenerDatosUrl,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(filtros),
            success: function (response) {
                CerrarWaiting();
                
                if (response.error === true) {
                    mostrarTablaVacia();
                    AbrirMensaje("ATENCIÓN", response.msg, function () {
                        $("#msjModal").modal("hide");
                    }, false, ["Entendido"], "error!", null);
                }
                else if (response.warn === true) {
                    mostrarTablaVacia();
                    AbrirMensaje("ATENCIÓN", response.msg, function () {
                        $("#msjModal").modal("hide");
                    }, false, ["Entendido"], "warn!", null);
                }
                else {
                    productos = response.lista.map(item => ({
                        p_id: item.p_id,
                        p_id_ok: item.p_id_ok,
                        codigo: item.p_id_ok,
                        descripcion: item.p_desc,
                        estado: item.p_activo_desc,
                        proveedor: item.cta_denominacion,
                        stock: item.stk,
                        procesado: item.procesado,
                        procesado_desc: item.procesado_desc
                    }));

                    renderizarTabla();
                }
            },
            error: function (xhr, status, error) {
                CerrarWaiting();
                productos = [];
                $('#tbodyDiscontinuos').html(`
                    <tr>
                        <td colspan="7" class="text-center py-4">
                            <i class="bx bx-error-circle bx-md text-danger"></i>
                            <p class="text-danger mb-0">Error al cargar productos: ${error}</p>
                        </td>
                    </tr>
                `);
                console.error('Error:', error);
            }
        });
    }

    // ✅ NUEVA: Mostrar tabla vacía (para mantener consistencia)
    function mostrarTablaVacia() {
        productos = [];
        $('#tbodyDiscontinuos').html(`
            <tr>
                <td colspan="7" class="text-center py-4">
                    <i class="bx bx-info-circle bx-md text-muted"></i>
                    <p class="text-muted mb-0">No hay productos para mostrar</p>
                </td>
            </tr>
        `);
    }

    function renderizarTabla() {
        const tbody = $('#tbodyDiscontinuos');
        tbody.empty();

        if (productos.length === 0) {
            tbody.append(`
                <tr>
                    <td colspan="7" class="text-center py-4">
                        <i class="bx bx-info-circle bx-md text-muted"></i>
                        <p class="text-muted mb-0">No hay productos para mostrar</p>
                    </td>
                </tr>
            `);
            return;
        }

        productos.forEach((p, index) => {
            const badgeClass = p.estado === 'Activo' ? 'bg-success' : 'bg-secondary';
            const rowClass = index % 2 === 0 ? '' : 'alt';
            const iconoProcesado = p.procesado
                ? '<i class="bx bx-check-circle bx-sm text-success ms-1" title="Procesado"></i>'
                : '<i class="bx bx-x-circle bx-sm text-danger ms-1" title="No procesado"></i>';
            
            tbody.append(`
                <tr class="${rowClass}" data-index="${index}">
                    <td class="text-center">
                        <input type="checkbox" class="form-check-input chk-producto" data-p-id="${p.p_id}" data-p-id-ok="${p.p_id_ok}">
                    </td>
                    <td>${p.codigo}</td>
                    <td class="text-truncate" style="max-width: 300px;" title="${p.descripcion}">${p.descripcion}</td>
                    <td>
                        <span class="badge ${badgeClass}">${p.estado}</span>
                        ${iconoProcesado}
                    </td>
                    <td class="text-truncate" style="max-width: 300px;" title="${p.proveedor}">${p.proveedor}</td>
                    <td class="text-end">${formatearDecimal(p.stock)}</td>
                    <td class="text-center">
                        <button type="button" class="btn btn-sm btn-danger btn-eliminar-producto" data-index="${index}" title="Eliminar">
                            <i class="bx bx-trash"></i>
                        </button>
                    </td>
                </tr>
            `);
        });
    }

    function eliminarProducto() {
        const index = $(this).data('index');
        const producto = productos[index];

        AbrirMensaje(
            "¿Eliminar producto?",
            `<p>¿Está seguro de eliminar el producto?</p><p><strong>${producto.codigo} - ${producto.descripcion}</strong></p>`,
            function () {
                productos.splice(index, 1);
                renderizarTabla();
                actualizarCheckGeneral();
                $("#msjModal").modal("hide");
                
                AbrirMensaje(
                    "Eliminado",
                    "El producto ha sido eliminado del grid",
                    function () {
                        $("#msjModal").modal("hide");
                    },
                    false,
                    ["Aceptar"],
                    "success!",
                    null
                );
            },
            true,
            ["Sí, eliminar", "Cancelar"],
            "warn!",
            null
        );
    }

    function seleccionarTodos() {
        const isChecked = $(this).is(':checked');
        $('.chk-producto').prop('checked', isChecked);
    }

    function actualizarCheckGeneral() {
        const total = $('.chk-producto').length;
        const seleccionados = $('.chk-producto:checked').length;
        $('#chkSelectAll').prop('checked', total > 0 && total === seleccionados);
    }

    function confirmarCambios() {
        const opcion = $('input[name="opcionDiscontinuos"]:checked').val();
        const seleccionados = $('.chk-producto:checked').map(function() {
            return $(this).data('p-id-ok');
        }).get();

        if (seleccionados.length === 0) {
            AbrirMensaje(
                "ATENCIÓN",
                "Debe seleccionar al menos un producto",
                function () {
                    $("#msjModal").modal("hide");
                },
                false,
                ["Entendido"],
                "warn!",
                null
            );
            return;
        }

        AbrirMensaje(
            "Confirmar",
            `¿Confirmar cambios en ${seleccionados.length} producto(s)?`,
            function () {
                $("#msjModal").modal("hide");
                AbrirWaiting();
                
                const request = {
                    opcion: opcion,
                    lista: seleccionados
                };

                $.ajax({
                    url: confirmarDiscontinuosUrl,
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify(request),
                    success: function (response) {
                        CerrarWaiting();
                        
                        if (response.error === true) {
                            AbrirMensaje(
                                "ERROR",
                                response.msg,
                                function () {
                                    $("#msjModal").modal("hide");
                                },
                                false,
                                ["Entendido"],
                                "error!",
                                null
                            );
                        }
                        else if (response.warn === true) {
                            AbrirMensaje(
                                "ATENCIÓN",
                                response.msg,
                                function () {
                                    $("#msjModal").modal("hide");
                                },
                                false,
                                ["Entendido"],
                                "warn!",
                                null
                            );
                        }
                        else {
                            AbrirMensaje(
                                "Éxito",
                                response.msg || "Cambios confirmados correctamente",
                                function () {
                                    $("#msjModal").modal("hide");
                                    cancelar();
                                },
                                false,
                                ["Aceptar"],
                                "success!",
                                null
                            );
                        }
                    },
                    error: function (xhr, status, error) {
                        CerrarWaiting();
                        console.error('Error al confirmar:', error);
                        
                        AbrirMensaje(
                            "ERROR",
                            "Hubo un problema al confirmar los cambios. Si el problema persiste informe al administrador del sistema",
                            function () {
                                $("#msjModal").modal("hide");
                            },
                            false,
                            ["Entendido"],
                            "error!",
                            null
                        );
                    }
                });
            },
            true,
            ["Sí, confirmar", "Cancelar"],
            "question!",
            null
        );
    }

    // ✅ MODIFICADA: Resetear bandera al cancelar
    function cancelar() {
        productos = [];
        $('#tbodyDiscontinuos').empty();
        $('.chk-producto').prop('checked', false);
        $('#chkSelectAll').prop('checked', false);
        $("#btnConfirmar").prop("disabled", true);
        ocultarUploadArea();
        
        // Resetear la bandera a la opción por defecto
        opcionAnterior = $('input[name="opcionDiscontinuos"]:checked').val() || '1';
    }

    function formatearDecimal(valor) {
        return parseFloat(valor).toLocaleString('es-AR', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    function mostrarCargando(cuerpo) {
        $(cuerpo).html(`
            <tr>
                <td colspan="7" class="text-center py-4">
                    <div class="spinner-border spinner-border-golden" role="status">
                        <span class="visually-hidden">Cargando...</span>
                    </div>
                    <p class="loading-text-golden mt-2">Cargando productos...</p>
                </td>
            </tr>
        `);
    }

    function ocultarCargando() {
        // La tabla se renderiza automáticamente en renderizarTabla()
    }
});