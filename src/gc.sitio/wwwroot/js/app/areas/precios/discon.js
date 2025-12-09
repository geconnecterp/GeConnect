$(function () {
    let productos = [];

    // Inicializar eventos
    inicializarEventos();

    function inicializarEventos() {
        $('#btnCargar').on('click', cargarProductos);
        $('#btnConfirmar').on('click', confirmarCambios);
        $('#btnCancelar').on('click', cancelar);
        $('#chkSelectAll').on('change', seleccionarTodos);
        $(document).on('change', '.chk-producto', actualizarCheckGeneral);
        $(document).on('click', '.btn-eliminar-producto', eliminarProducto);
    }

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

        // Mostrar spinner
        mostrarCargando('#tbodyDiscontinuos');

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
                if (response.error === true) {
                    CerrarWaiting();
                    AbrirMensaje("ATENCIÓN", response.msg, function () {
                        $("#msjModal").modal("hide");
                    }, false, ["Entendido"], "error!", null);
                }
                else if (response.warn === true) {
                    CerrarWaiting();
                    AbrirMensaje("ATENCIÓN", response.msg, function () {
                        $("#msjModal").modal("hide");
                    }, false, ["Entendido"], "warn!", null);
                }
                else {
                    CerrarWaiting();

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
            //const iconoProcesado = p.procesado
            //    ? '<i class="bx bx-check-circle text-success" title="Procesado"></i>'
            //    : '<i class="bx bx-x-circle text-danger" title="No procesado"></i>';
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

    function cancelar() {
        productos = [];
        $('#tbodyDiscontinuos').empty();
        $('.chk-producto').prop('checked', false);
        $('#chkSelectAll').prop('checked', false);
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