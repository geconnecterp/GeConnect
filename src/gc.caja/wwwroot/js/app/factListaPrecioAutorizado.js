(function (window, document, $) {
    'use strict';

    const S = Object.freeze({
        modal: '#modalCambiarListaPrecios', abrir: '#btnListaPrecios', select: '#selListaPreciosModal',
        lista: '#listboxListasPrecios', confirmar: '#btnConfirmarCambiarLP', cancelar: '#btnCancelarCambiarLP',
        seguir: '#btnSeguirCliente', listaActual: '#txtListaPrecioActual'
    });
    let catalogo = [];
    let seleccion = null;
    let solicitudEnCurso = false;

    $(function () {
        $(S.abrir).on('click', abrirModal);
        $(S.select).on('change', function () { seleccionar(String($(this).val() || '')); });
        $(document).on('click', `${S.lista} .list-group-item:not(:disabled)`, function () {
            seleccionar(String($(this).data('lp-id') || ''));
        });
        $(S.cancelar).on('click', cerrarModal);
        $(S.confirmar).on('click', confirmarCambio);
    });

    function urlGlobal(nombre, alternativa) {
        return typeof window[nombre] === 'string' && window[nombre] ? window[nombre] : alternativa;
    }

    function listaActivaId() {
        return String(window.obtenerListaPrecioActivaId?.() || '').trim();
    }

    function abrirModal() {
        if (solicitudEnCurso || window.AutorizacionRemota?.estaEsperando()) {
            mostrarError('Ya existe una autorización en curso.');
            return;
        }
        if (!listaActivaId()) {
            mostrarError('No hay una lista de precios activa para el cliente.');
            return;
        }
        seleccion = null;
        restaurarBotonesModal();
        $(S.modal).modal('show');
        cargarCatalogo();
    }

    function cargarCatalogo() {
        $(S.select).empty().prop('disabled', true).append($('<option>').text('Cargando listas...'));
        $(S.lista).empty().append(
            $('<button>', { type: 'button', disabled: true }).addClass('list-group-item list-group-item-action')
                .append($('<i>').addClass('bx bx-loader-alt bx-spin me-2'))
                .append(document.createTextNode('Cargando listas de precios...')));

        $.ajax({
            url: urlGlobal('ObtenerListasPreciosUrl', '/Facturacion/ProductoFact/ObtenerListasPrecios'),
            type: 'GET', dataType: 'json', timeout: 15000
        }).done(function (respuesta) {
            if (!respuesta?.ok || !Array.isArray(respuesta.listas) || respuesta.listas.length === 0) {
                mostrarErrorCatalogo(respuesta?.mensaje || 'No hay listas de precios disponibles.');
                return;
            }
            catalogo = respuesta.listas;
            window.establecerListaPrecioActiva(respuesta.lp_actual, respuesta.lp_actual_descripcion);
            actualizarIndicadorListaActual();
            renderizarCatalogo();
        }).fail(function (xhr) {
            if (typeof window.esSesionExpirada === 'function' && window.esSesionExpirada(xhr.status)) return;
            mostrarErrorCatalogo(obtenerMensaje(xhr, 'No se pudieron cargar las listas de precios.'));
        });
    }

    function renderizarCatalogo() {
        const activa = listaActivaId();
        const $select = $(S.select).empty().prop('disabled', false)
            .append($('<option>', { value: '' }).text('-- Seleccione una lista --'));
        const $lista = $(S.lista).empty();

        catalogo.forEach(function (item) {
            const id = String(item.lp_id || '').trim();
            const descripcion = String(item.lp_desc || 'Sin descripción').trim();
            if (!id) return;
            $select.append($('<option>', { value: id }).text(`${id} - ${descripcion}`));

            const esActiva = id.toUpperCase() === activa.toUpperCase();
            const $boton = $('<button>', { type: 'button' })
                .addClass('list-group-item list-group-item-action d-flex justify-content-between align-items-center')
                .toggleClass('active', esActiva).attr('data-lp-id', id);
            const $texto = $('<span>')
                .append($('<i>').addClass(`bx ${esActiva ? 'bx-check-circle' : 'bx-purchase-tag'} me-2`))
                .append(document.createTextNode(descripcion));
            $boton.append($texto, $('<span>').addClass('badge-lp-codigo').text(id));
            $lista.append($boton);
        });
    }

    function seleccionar(id) {
        const item = catalogo.find(x => String(x.lp_id || '').trim().toUpperCase() === id.toUpperCase());
        seleccion = item ? { id: String(item.lp_id || '').trim(), descripcion: String(item.lp_desc || '').trim() } : null;
        $(S.lista).find('.list-group-item').removeClass('active');
        if (seleccion) {
            $(S.lista).find('.list-group-item').filter(function () {
                return String($(this).data('lp-id') || '').toUpperCase() === seleccion.id.toUpperCase();
            }).addClass('active');
            $(S.select).val(seleccion.id);
        }
        const esActual = seleccion && seleccion.id.toUpperCase() === listaActivaId().toUpperCase();
        $(S.confirmar).prop('disabled', !seleccion || esActual);
    }

    function confirmarCambio() {
        if (!seleccion || seleccion.id.toUpperCase() === listaActivaId().toUpperCase()) {
            mostrarError('Seleccione una lista distinta de la actual.');
            return;
        }
        AbrirMensaje('Solicitar autorización',
            `Se solicitará autorización para cambiar a:<br><br><strong>${escaparHtml(seleccion.descripcion)}</strong> (${escaparHtml(seleccion.id)}).`,
            function () { $('#msjModal').modal('hide'); solicitarAutorizacion(); }, true,
            ['Solicitar', 'Cancelar'], 'warn!', null);
    }

    async function solicitarAutorizacion() {
        if (solicitudEnCurso || !seleccion) return;
        solicitudEnCurso = true;
        bloquearInterfaz();
        try {
            const solicitud = await ajaxJson(
                urlGlobal('SolicitarCambioListaPrecioUrl', '/Facturacion/ListaPrecio/SolicitarCambio'),
                { lpId: seleccion.id });
            if (!solicitud?.ok) throw new Error(solicitud?.mensaje || 'No se pudo crear la solicitud.');
            $(S.modal).modal('hide');

            const resultado = await window.AutorizacionRemota.esperar({
                idSolicitud: solicitud.idSolicitud, claveOperacion: solicitud.claveOperacion,
                urlEstado: solicitud.urlEstado, timeoutSegundos: solicitud.timeoutSegundos,
                titulo: 'Cambio de lista de precios', subtitulo: 'Esperando autorización del administrador',
                mensaje: `Solicitud enviada para la lista ${seleccion.id} - ${seleccion.descripcion}.`,
                detalle: 'No cierre ni actualice esta pantalla.',
                onAprobada: async function (evento) {
                    const aplicada = await ajaxJson(
                        urlGlobal('AplicarCambioListaPrecioUrl', '/Facturacion/ListaPrecio/AplicarCambio'),
                        { idSolicitud: evento.idSolicitud });
                    if (!aplicada?.ok) throw new Error(aplicada?.mensaje || 'La autorización aprobada no pudo aplicarse.');
                    aplicarEnPantalla(aplicada.lista);
                },
                onRechazada: finalizarNoAprobada,
                onExpirada: finalizarNoAprobada
            });

            if (resultado.tipo === window.AutorizacionRemota.estados.APROBADA) mostrarExito('Lista de precios actualizada correctamente.');
            else if (resultado.tipo === window.AutorizacionRemota.estados.RECHAZADA) mostrarError(resultado.respuesta?.mensaje || 'El administrador rechazó el cambio.');
            else if (resultado.tipo === window.AutorizacionRemota.estados.EXPIRADA) mostrarError('La solicitud expiró. Puede solicitarla nuevamente.');
            else if (resultado.tipo === window.AutorizacionRemota.estados.REEMPLAZADA) mostrarError('La solicitud fue reemplazada por una más reciente.');
            else mostrarError(resultado.respuesta?.mensaje || 'No se pudo completar la autorización.');
        } catch (error) {
            mostrarError(error?.message || 'No se pudo solicitar el cambio de lista de precios.');
        } finally {
            solicitudEnCurso = false;
            seleccion = null;
            restaurarBotonesModal();
            window.actualizarEstadoBotonesAccion?.();
        }
    }

    async function finalizarNoAprobada(evento) {
        try {
            await ajaxJson(urlGlobal('FinalizarCambioListaPrecioUrl', '/Facturacion/ListaPrecio/FinalizarNoAprobada'),
                { idSolicitud: evento.idSolicitud });
        } catch (error) { console.warn('No se pudo liberar la solicitud finalizada.', error); }
    }

    function aplicarEnPantalla(lista) {
        const id = String(lista?.id || '').trim();
        const descripcion = String(lista?.descripcion || '').trim();
        window.establecerListaPrecioActiva(id, descripcion);
        window.aplicarListaPrecioAutorizadaUI?.(id, descripcion);
        if (window.clienteSeleccionado) {
            window.clienteSeleccionado.lp_id = id;
            window.clienteSeleccionado.listaPrecio = id;
            window.clienteSeleccionado.listaPrecioDescripcion = descripcion;
        }
        if (window.clienteActualFactura) {
            window.clienteActualFactura.lp_id = id;
            window.clienteActualFactura.listaPrecio = id;
            window.clienteActualFactura.listaPrecioDescripcion = descripcion;
        }
        actualizarIndicadorListaActual();
        $(document).trigger('listaPrecioAutorizada', [{ id, descripcion }]);
    }

    function actualizarIndicadorListaActual() {
        const lista = window.listaPrecioActiva || { id: '', descripcion: '' };
        $(S.listaActual).val(lista.id ? `${lista.id}${lista.descripcion ? ` - ${lista.descripcion}` : ''}` : 'Sin lista de precios activa');
    }

    function bloquearInterfaz() {
        $(S.confirmar).prop('disabled', true).html('<i class="bx bx-loader-alt bx-spin"></i> Solicitando...');
        $(S.cancelar).prop('disabled', true); $(S.abrir).prop('disabled', true); $(S.seguir).prop('disabled', true);
    }
    function restaurarBotonesModal() {
        $(S.confirmar).prop('disabled', true).html('<i class="bx bx-check-circle"></i> Cambiar LP');
        $(S.cancelar).prop('disabled', false);
    }
    function cerrarModal() {
        if (solicitudEnCurso) return;
        $(S.modal).modal('hide'); seleccion = null; restaurarBotonesModal();
    }
    function mostrarErrorCatalogo(mensaje) {
        $(S.select).empty().prop('disabled', true).append($('<option>').text('Sin listas disponibles'));
        $(S.lista).empty().append($('<div>').addClass('alert alert-danger m-2').text(mensaje));
    }
    function mostrarError(mensaje) {
        if (typeof window.mostrarMensajeError === 'function') window.mostrarMensajeError(mensaje);
        else AbrirMensaje('Error', escaparHtml(mensaje), null, false, ['Aceptar'], 'error!', null);
    }
    function mostrarExito(mensaje) {
        if (typeof window.mostrarMensajeExito === 'function') window.mostrarMensajeExito(mensaje);
        else AbrirMensaje('Operación completada', escaparHtml(mensaje), null, false, ['Aceptar'], 'success!', null);
    }
    function ajaxJson(url, datos) {
        return new Promise(function (resolve, reject) {
            $.ajax({ url, type: 'POST', contentType: 'application/json; charset=utf-8', dataType: 'json', data: JSON.stringify(datos) })
                .done(resolve).fail(function (xhr) {
                    if (typeof window.esSesionExpirada === 'function' && window.esSesionExpirada(xhr.status)) {
                        reject(new Error('La sesión ha expirado.')); return;
                    }
                    reject(new Error(obtenerMensaje(xhr, 'La operación no pudo completarse.')));
                });
        });
    }
    function obtenerMensaje(xhr, alternativa) { return xhr?.responseJSON?.mensaje || xhr?.responseJSON?.message || alternativa; }
    function escaparHtml(valor) {
        return String(valor || '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;').replace(/'/g, '&#039;');
    }
})(window, document, window.jQuery);
