function normalizarDatosFacturaEmitida(tipo, pv, numero) {
    const datos = {
        tcoId: String(tipo || '').trim(),
        puntoVenta: String(pv || '').trim(),
        numero: String(numero || '').trim()
    };
    if (!/^[0-9]{3}$/.test(datos.tcoId))
        return { error: 'Seleccione un tipo de comprobante.', campo: 'ddlFacturaEmitidaTipo' };
    if (!/^[0-9]{1,4}$/.test(datos.puntoVenta))
        return { error: 'El PV debe tener entre 1 y 4 d\u00edgitos.', campo: 'txtFacturaEmitidaPv' };
    if (!/^[0-9]{1,8}$/.test(datos.numero))
        return { error: 'El n\u00famero debe tener entre 1 y 8 d\u00edgitos.', campo: 'txtFacturaEmitidaNumero' };
    datos.puntoVenta = datos.puntoVenta.padStart(4, '0');
    datos.numero = datos.numero.padStart(8, '0');
    return { datos };
}

function aplicarDetalleFacturaEmitida(response) {
    if (!response || response.ok !== true || !Array.isArray(response.productos)) return 0;
    if (modoBloqueoGrilla === 'cotizacion') return 0;
    if (typeof response.acumula === 'boolean') cajaAcumulaProductos = response.acumula;
    const origenAnterior = origenCargaActual;
    let cargados = 0;
    origenCargaActual = 'facturaEmitida';
    try {
        response.productos.forEach(producto => {
            if (producto.respuesta !== 0 || !producto.p_id || !(Number(producto.cantidad_tot) > 0)) return;
            const resultado = agregarProductoAGrilla(producto);
            if (resultado && resultado.accion !== 'error') cargados++;
        });
    } finally {
        origenCargaActual = origenAnterior;
    }
    return cargados;
}

(function () {
    'use strict';
    let abierto = false;
    let ocupado = false;
    let tiposDisponibles = false;
    let mensajeAlVolver = null;
    const selector = '#modalFacturaEmitida';
    const inputs = '#txtFacturaEmitidaPv, #txtFacturaEmitidaNumero';

    function estado(mensaje, nivel = 'warning') {
        $('#estadoFacturaEmitida').removeClass('text-muted text-warning text-danger text-success')
            .addClass('text-' + nivel).text(mensaje);
    }

    function cerrarTeclado() {
        if (typeof cerrarTecladoDigital === 'function') cerrarTecladoDigital();
    }

    function bloquear(valor) {
        ocupado = valor;
        $('#formFacturaEmitida').attr('aria-busy', String(valor));
        $('#ddlFacturaEmitidaTipo, ' + inputs + ', #btnCargarFacturaEmitida')
            .prop('disabled', valor || !tiposDisponibles);
        $('[data-factura-emitida-cancelar]').prop('disabled', valor);
        $('#spinnerFacturaEmitida').toggleClass('d-none', !valor);
        if (valor) cerrarTeclado();
    }

    function posicionar(input) {
        requestAnimationFrame(() => {
            if (!abierto || ocupado || document.activeElement !== input) return;
            if (typeof posicionarTecladoVirtual === 'function') {
                posicionarTecladoVirtual('#' + input.id, null, { preferredSide: 'left', verticalAlign: 'bottom' });
                const teclado = document.getElementById('virtual-keyboard');
                const cancelar = document.querySelector(selector + ' .modal-footer');
                if (teclado && cancelar) {
                    const t = teclado.getBoundingClientRect();
                    const c = cancelar.getBoundingClientRect();
                    if (t.left < c.right && t.right > c.left && t.top < c.bottom && t.bottom > c.top)
                        teclado.style.setProperty('top', Math.max(12, c.top - t.height - 12) + 'px', 'important');
                }
            }
        });
    }

    function avanzar(input) {
        if (ocupado) return;
        if (input.id === 'txtFacturaEmitidaPv') {
            document.getElementById('txtFacturaEmitidaNumero').focus();
        } else {
            cargar();
        }
    }

    async function cargarTipos() {
        bloquear(true);
        estado('Cargando tipos de comprobante...', 'muted');
        try {
            const response = await $.getJSON(window.ObtenerTiposFacturaEmitidaUrl);
            if (!response.ok || !Array.isArray(response.datos) || response.datos.length === 0) {
                estado(response.mensaje || 'No hay tipos de comprobante disponibles.');
                return;
            }
            const select = document.getElementById('ddlFacturaEmitidaTipo');
            response.datos.forEach(tipo => select.add(new Option(tipo.tco_id + ' - ' + tipo.tco_desc, tipo.tco_id)));
            tiposDisponibles = true;
            estado('', 'muted');
        } catch (error) {
            estado(error.status === 401 ? 'La sesi\u00f3n ha expirado.' : 'No se pudieron obtener los tipos de comprobante.');
        } finally {
            bloquear(false);
            document.getElementById('ddlFacturaEmitidaTipo').focus();
        }
    }

    async function cargar() {
        if (ocupado || !tiposDisponibles) return;
        const validacion = normalizarDatosFacturaEmitida(
            $('#ddlFacturaEmitidaTipo').val(), $('#txtFacturaEmitidaPv').val(), $('#txtFacturaEmitidaNumero').val());
        if (validacion.error) {
            estado(validacion.error);
            document.getElementById(validacion.campo).focus();
            return;
        }
        $('#txtFacturaEmitidaPv').val(validacion.datos.puntoVenta);
        $('#txtFacturaEmitidaNumero').val(validacion.datos.numero);
        bloquear(true);
        estado('Validando factura y cargando productos...', 'muted');
        let cargado = false;
        try {
            const response = await $.ajax({
                url: window.CargarFacturaEmitidaUrl,
                type: 'POST',
                contentType: 'application/json',
                headers: { RequestVerificationToken: $('#formFacturaEmitida input[name="__RequestVerificationToken"]').val() },
                data: JSON.stringify(validacion.datos)
            });
            const mensaje = [response.mensaje, ...(response.errores || [])].filter(Boolean).join('\n');
            if (!response.ok) {
                estado(mensaje || 'No fue posible cargar la factura.');
                return;
            }
            const cantidad = aplicarDetalleFacturaEmitida(response);
            if (cantidad === 0) {
                estado('No hay productos disponibles para agregar a la grilla.');
                return;
            }
            mensajeAlVolver = { texto: mensaje, nivel: response.errores?.length ? 'warning' : 'success' };
            cargado = true;
        } catch (error) {
            console.error('[Factura emitida] Error de carga:', error);
            estado(error.status === 401 ? 'La sesi\u00f3n ha expirado.' : 'No se pudo completar la carga de la factura.');
        } finally {
            bloquear(false);
            if (cargado) {
                bootstrap.Modal.getInstance(document.querySelector(selector)).hide();
            } else {
                document.getElementById('txtFacturaEmitidaNumero').focus();
            }
        }
    }

    window.cargarFacturaEmitida = function () {
        if (abierto || ocupado) return;
        if (modoBloqueoGrilla === 'cotizacion') {
            mostrarMensajeEstado('No se puede agregar una factura emitida a una cotizaci\u00f3n.', 'warning');
            return;
        }
        abierto = true;
        tiposDisponibles = false;
        mensajeAlVolver = null;
        document.getElementById('formFacturaEmitida').reset();
        $('#ddlFacturaEmitidaTipo').empty().append(new Option('Seleccione un tipo', ''));
        bloquear(true);
        cerrarTeclado();
        const mostrar = () => bootstrap.Modal.getOrCreateInstance(document.querySelector(selector), { focus: false }).show();
        const productos = document.getElementById('modalProductosFactura');
        if (productos.classList.contains('show')) {
            $('#modalProductosFactura').one('hidden.bs.modal.facturaEmitida', mostrar);
            document.activeElement?.blur();
            bootstrap.Modal.getOrCreateInstance(productos).hide();
        } else mostrar();
    };

    $(function () {
        $(selector).on('shown.bs.modal', cargarTipos).on('hide.bs.modal', function (event) {
            if (ocupado) {
                event.preventDefault();
                return;
            }
            document.activeElement?.blur();
            cerrarTeclado();
        }).on('hidden.bs.modal', function () {
            abierto = false;
            $('#modalProductosFactura').one('shown.bs.modal.facturaEmitida', function () {
                if (!mensajeAlVolver) return;
                // El mensaje puede incluir descripciones devueltas por el SP: renderizar como texto.
                mostrarMensajeEstado('', mensajeAlVolver.nivel, 0);
                $('#mensajeEstadoProducto').text(mensajeAlVolver.texto);
                mensajeAlVolver = null;
            });
            bootstrap.Modal.getOrCreateInstance(document.getElementById('modalProductosFactura')).show();
        });
        $('#formFacturaEmitida').on('submit', function (event) { event.preventDefault(); cargar(); });
        $('[data-factura-emitida-cancelar]').on('click', function () {
            if (!ocupado) bootstrap.Modal.getInstance(document.querySelector(selector)).hide();
        });
        $('#ddlFacturaEmitidaTipo').on('change', function () {
            if (this.value) document.getElementById('txtFacturaEmitidaPv').focus();
        });
        $(inputs).on('focus', function () { posicionar(this); }).on('input', function () {
            this.value = this.value.replace(/[^0-9]/g, '').slice(0, this.maxLength);
        }).on('keydown', function (event) {
            if (event.key === 'Enter') { event.preventDefault(); event.stopPropagation(); avanzar(this); }
        });
        // El teclado compartido navega por todos los inputs de la pagina. Aqui Enter es local al modal.
        const enterDigital = function (event) {
            if (!abierto || !event.target.closest('#virtual-keyboard [data-key="ENTER"]')) return;
            const input = document.activeElement;
            if (!input?.matches(inputs)) return;
            event.preventDefault();
            event.stopImmediatePropagation();
            avanzar(input);
        };
        document.addEventListener('mousedown', enterDigital, true);
        document.addEventListener('touchstart', enterDigital, { capture: true, passive: false });
        window.addEventListener('resize', function () {
            if (abierto && document.activeElement?.matches(inputs)) posicionar(document.activeElement);
        });
    });
})();
