// Modulo: Nota de Debito, Nota de Credito y Factura de Servicio
// Lote 1: busqueda y seleccion de cuenta con app=ND.

(function () {
    'use strict';

    const APP_BUSQUEDA = 'ND';
    const LOG_PREFIX = '[ND/NC/FS]';
    const SELECTORES = {
        modal: '#modalIdentificarCliente',
        inputBusqueda: '#txtBuscarCliente',
        btnBuscar: '#btnBuscarCliente',
        btnCancelar: '#btnCancelarCliente',
        btnSalir: '#btnSalirFacturacion',
        btnSeguir: '#btnSeguirCliente',
        cardDatos: '#cardDatosCliente',
        alertSinCliente: '#alertSinCliente',
        txtNombre: '#txtNombre',
        txtClienteId: '#txtClienteId',
        txtDomicilio: '#txtDomicilio',
        txtCondicionAfip: '#txtCondicionAfip',
        txtTipoNumero: '#txtTipoNumero',
        txtEmite: '#txtEmite',
        txtEmail: '#txtEmail',
        txtMovil: '#txtMovil',
        resumenCuenta: '#ndcfsResumenCuenta',
        modalConceptos: '#modalNdcfsConceptos',
        modalCalculo: '#modalNdcfsCalculo',
        operacion: '#cmbNdcfsOperacion',
        tcoOri: '#txtNdcfsTcoOri',
        comboTcoOri: '#cmbNdcfsTcoOri',
        puntoVentaOri: '#txtNdcfsPuntoVentaOri',
        numeroCompteOri: '#txtNdcfsNumeroCompteOri',
        compteOri: '#txtNdcfsCompteOri',
        repetidoOri: '#txtNdcfsRepetidoOri',
        cantidad: '#txtNdcfsCantidad',
        neto: '#txtNdcfsNeto',
        alicuota: '#txtNdcfsAlicuota',
        iva: '#cmbNdcfsIva',
        perIb: '#txtNdcfsPerIb',
        perIva: '#txtNdcfsPerIva',
        concepto: '#txtNdcfsConcepto',
        mensajeConcepto: '#ndcfsMensajeConcepto',
        btnAgregarConcepto: '#btnNdcfsAgregarConcepto',
        btnCancelarConceptos: '#btnNdcfsCancelarConceptos',
        btnSeguirConceptos: '#btnNdcfsSeguirConceptos',
        tbodyConceptos: '#tbodyNdcfsConceptos',
        totalConceptos: '#lblNdcfsTotalConceptos',
        tbodySubtotales: '#tbodyNdcfsSubtotales',
        totalCalculo: '#tdNdcfsTotalCalculo',
        btnVolverCalculo: '#btnNdcfsVolverCalculo',
        btnFinalizar: '#btnNdcfsFinalizar'
    };

    let cuentaSeleccionada = null;
    let busquedaEnCurso = false;
    let conceptos = [];
    let calculoActual = null;
    let calculoEnCurso = false;
    let confirmacionEnCurso = false;
    let ivaAlicuotasCargadas = false;
    let tiposComprobanteOrigenCargados = false;

    $(function () {
        logPaso('Inicializando modulo', {
            buscarClienteUrl: window.BuscarClienteUrl,
            registrarCuentaUrl: window.ndcfsRegistrarCuentaUrl,
            calcularUrl: window.ndcfsCalcularConceptosUrl,
            confirmarUrl: window.ndcfsConfirmarOperacionUrl,
            tiposComprobanteUrl: window.ndcfsObtenerTiposComprobanteUrl
        });
        reiniciarEstadoModulo();
        inicializarVista();
        registrarEventos();

        setTimeout(function () {
            logPaso('Abriendo modal inicial de busqueda de cuenta');
            $(SELECTORES.modal).modal('show');
        }, 250);
    });

    function reiniciarEstadoModulo() {
        cuentaSeleccionada = null;
        busquedaEnCurso = false;
        conceptos = [];
        calculoActual = null;
        actualizarEstadoOperacionPorConceptos();
        calculoEnCurso = false;
        confirmacionEnCurso = false;
    }

    function inicializarVista() {
        $(SELECTORES.cardDatos).hide();
        $(SELECTORES.alertSinCliente).show();
        $(SELECTORES.btnSeguir).prop('disabled', true);
        $('#seccionAutoConfirmar, #seccionAccionRapida').addClass('d-none');
        $('#txtListaPrecioActual').closest('.row').addClass('d-none');
    }

    function registrarEventos() {
        $(document).on('click', SELECTORES.btnBuscar, buscarCuenta);

        $(document).on('keydown', SELECTORES.inputBusqueda, function (event) {
            if (event.key !== 'Enter') {
                return;
            }

            event.preventDefault();
            buscarCuenta();
        });

        $(document).on('click', SELECTORES.btnCancelar, limpiarBusqueda);

        $(document).on('click', SELECTORES.btnSalir, function () {
            cancelarOperacion(function () {
                window.location.href = window.ndcfsMenuCajaUrl || '/';
            });
        });

        $(document).on('click', SELECTORES.btnSeguir, function () {
            if (!cuentaSeleccionada) {
                mostrarMensaje('Atencion', 'Debe seleccionar una cuenta habilitada para continuar.', 'warn!');
                return;
            }

            registrarCuentaSeleccionada();
        });

        $(document).on('dblclick', '.cliente-row', function () {
            seleccionarCuentaDesdeFila($(this));
        });

        $(document).on('click', '.btn-seleccionar-cliente', function (event) {
            event.preventDefault();
            event.stopPropagation();
            seleccionarCuentaDesdeFila($(this).closest('.cliente-row'));
        });

        $(document).on('click', '#btnCerrarGrilla', function () {
            $('#cardGrillaClientes').remove();
        });

        $(document).on('change', SELECTORES.operacion, actualizarVisibilidadOrigenNc);
        $(document).on('change', SELECTORES.comboTcoOri, sincronizarTipoComprobanteOrigen);
        $(document).on('input', `${SELECTORES.puntoVentaOri}, ${SELECTORES.numeroCompteOri}`, function () {
            limitarSoloDigitos($(this), this.id === 'txtNdcfsPuntoVentaOri' ? 4 : 8);
            sincronizarComprobanteOrigen();
        });
        $(document).on('input', SELECTORES.neto, function () {
            limitarInputDecimal($(this));
            recalcularAlicuota();
        });
        $(document).on('change', SELECTORES.iva, recalcularAlicuota);
        $(document).on('focus', SELECTORES.neto, function () {
            normalizarInputParaEdicion($(this));
            this.select();
        });
        $(document).on('keydown', SELECTORES.neto, permitirSoloDecimal);
        $(document).on('paste', SELECTORES.neto, function () {
            const $input = $(this);
            setTimeout(function () {
                limitarInputDecimal($input);
                recalcularAlicuota();
            }, 0);
        });
        $(document).on('blur', SELECTORES.neto, function () {
            formatearInputNumerico($(this), 2);
            recalcularAlicuota();
        });
        $(document).on('blur', SELECTORES.cantidad, function () {
            formatearInputNumerico($(this), 0);
        });
        $(document).on('focus', SELECTORES.cantidad, function () {
            normalizarInputParaEdicion($(this));
        });
        $(document).on('click', SELECTORES.btnAgregarConcepto, agregarConcepto);
        $(document).on('click', '.btn-ndcfs-eliminar-concepto', function () {
            eliminarConcepto($(this).data('index'));
        });
        $(document).on('click', SELECTORES.btnCancelarConceptos, function () {
            cancelarOperacion(function () {
                window.location.href = window.ndcfsMenuCajaUrl || '/';
            });
        });
        $(document).on('click', SELECTORES.btnSeguirConceptos, calcularConceptos);
        $(document).on('click', SELECTORES.btnVolverCalculo, function () {
            $(SELECTORES.modalCalculo).modal('hide');
            $(SELECTORES.modalConceptos).modal('show');
        });
        $(document).on('click', SELECTORES.btnFinalizar, confirmarOperacion);
    }

    function actualizarEstadoOperacionPorConceptos() {
        const tieneConceptos = conceptos.length > 0;
        $(SELECTORES.operacion).prop('disabled', tieneConceptos);
    }

    function buscarCuenta() {
        if (busquedaEnCurso) {
            return;
        }

        const criterio = String($(SELECTORES.inputBusqueda).val() || '').trim();
        logPaso('Inicio busqueda de cuenta', { criterio: criterio, app: APP_BUSQUEDA });

        if (!criterio) {
            logWarn('Busqueda cancelada: criterio vacio');
            mostrarMensaje('Atencion', 'Ingrese CUIT, DNI, ID o nombre para buscar la cuenta.', 'warn!');
            return;
        }

        const url = String(window.BuscarClienteUrl || '').trim();

        if (!url) {
            logError('Busqueda cancelada: URL no encontrada');
            mostrarMensaje('Error', 'No se encontro la URL de busqueda de cuentas.', 'error!');
            return;
        }

        let grillaPendiente = false;
        bloquearBusqueda(true);
        limpiarSeleccionVisual(false);
        mostrarLoaderNdcfs('Buscando cuenta...');

        $.ajax({
            url: url,
            type: 'POST',
            dataType: 'json',
            data: {
                criterio: criterio,
                app: APP_BUSQUEDA
            },
            timeout: 30000
        })
            .done(function (response) {
                logPaso('Respuesta busqueda de cuenta', response);
                if (!response || response.ok !== true) {
                    limpiarSeleccionVisual(true);
                    mostrarMensaje(
                        'Atencion',
                        response?.mensaje || 'No se encontraron cuentas habilitadas para operar.',
                        'warn!'
                    );
                    return;
                }

                if ((response.cantidadResultados || 0) === 1 && response.cliente) {
                    logPaso('Busqueda resolvio una cuenta unica', response.cliente);
                    procesarCuentaSeleccionada(response.cliente);
                    return;
                }

                logPaso('Busqueda requiere seleccion en grilla', { cantidadResultados: response.cantidadResultados });
                grillaPendiente = true;
                cargarGrillaCuentas();
            })
            .fail(function (xhr) {
                logError('Error AJAX buscando cuenta', {
                    status: xhr?.status,
                    response: xhr?.responseJSON || xhr?.responseText
                });
                limpiarSeleccionVisual(true);
                mostrarMensaje(
                    'Error de Comunicacion',
                    xhr?.responseJSON?.mensaje || 'Ocurrio un error al buscar la cuenta.',
                    'error!'
                );
            })
            .always(function () {
                bloquearBusqueda(false);
                if (!grillaPendiente) {
                    ocultarLoaderNdcfs();
                }
                logPaso('Fin busqueda de cuenta');
            });
    }

    function cargarGrillaCuentas() {
        const url = String(window.TraerGrillaClientesUrl || '').trim();

        if (!url) {
            logError('Carga de grilla cancelada: URL no encontrada');
            mostrarMensaje('Error', 'No se encontro la URL para mostrar las cuentas encontradas.', 'error!');
            return;
        }

        logPaso('Solicitando grilla de cuentas', { url: url });
        mostrarLoaderNdcfs('Cargando cuentas encontradas...');

        $.ajax({
            url: url,
            type: 'POST',
            dataType: 'html',
            timeout: 30000
        })
            .done(function (html) {
                logPaso('Grilla de cuentas recibida', { bytes: html ? html.length : 0 });
                $('#cardGrillaClientes').remove();
                $(SELECTORES.cardDatos).after(
                    `<div id="cardGrillaClientes" class="mt-3">${html}</div>`
                );
                $(SELECTORES.alertSinCliente).hide();
            })
            .fail(function () {
                logError('No se pudo cargar la grilla de cuentas');
                limpiarSeleccionVisual(true);
                mostrarMensaje('Error', 'No se pudo cargar la grilla de cuentas encontradas.', 'error!');
            })
            .always(function () {
                ocultarLoaderNdcfs();
            });
    }

    function seleccionarCuentaDesdeFila($fila) {
        logPaso('Seleccion de cuenta desde grilla');
        if (!$fila || $fila.length === 0) {
            logWarn('Seleccion de cuenta invalida: fila vacia');
            mostrarMensaje('Atencion', 'No se pudo identificar la cuenta seleccionada.', 'warn!');
            return;
        }

        const origen = String($fila.data('cta-origen') || '').trim().toUpperCase();
        logPaso('Datos fila seleccionada', {
            origen: origen,
            ctaId: $fila.data('cta-id'),
            documento: $fila.data('cta-documento')
        });

        if (origen === 'N' || origen === 'Q') {
            logWarn('Cuenta bloqueada por origen', { origen: origen });
            mostrarMensaje(
                'Cuenta no habilitada',
                origen === 'Q' ? 'El proveedor seleccionado no esta habilitado.' : 'El cliente seleccionado no esta habilitado.',
                'warn!'
            );
            return;
        }

        const criterio = origen === 'F'
            ? String($fila.data('cta-documento') || '').trim()
            : String($fila.data('cta-id') || '').trim();

        if (!criterio) {
            logWarn('Cuenta seleccionada sin identificador valido', { origen: origen });
            mostrarMensaje('Atencion', 'La cuenta seleccionada no posee identificador valido.', 'warn!');
            return;
        }

        $(SELECTORES.inputBusqueda).val(criterio);
        buscarCuenta();
    }

    function procesarCuentaSeleccionada(cuenta) {
        const origen = String(cuenta?.origen || '').trim().toUpperCase();
        logPaso('Procesando cuenta seleccionada', cuenta);

        if (origen === 'N' || origen === 'Q') {
            logWarn('Cuenta bloqueada luego de buscar datos completos', { origen: origen });
            mostrarMensaje(
                'Cuenta no habilitada',
                origen === 'Q' ? 'El proveedor seleccionado no esta habilitado.' : 'El cliente seleccionado no esta habilitado.',
                'warn!'
            );
            return;
        }

        cuentaSeleccionada = cuenta;
        $('#cardGrillaClientes').remove();
        hidratarDatosCuenta(cuenta);
        $(SELECTORES.cardDatos).show();
        $(SELECTORES.alertSinCliente).hide();
        $(SELECTORES.btnSeguir).prop('disabled', false);
    }

    function hidratarDatosCuenta(cuenta) {
        const origen = String(cuenta?.origen || '').trim().toUpperCase();
        const idVisible = origen === 'F'
            ? 'N/A'
            : cuenta?.id || '';

        $(SELECTORES.txtNombre).val(cuenta?.denominacion || cuenta?.nombre || '');
        $(SELECTORES.txtClienteId).val(idVisible);
        $(SELECTORES.txtDomicilio).val(cuenta?.domicilio || '');
        $(SELECTORES.txtCondicionAfip).val(cuenta?.condicionAfip || '');
        $(SELECTORES.txtTipoNumero).val(cuenta?.tipoNumero || cuenta?.documento || '');
        $(SELECTORES.txtEmite).val(cuenta?.emite || '');
        $(SELECTORES.txtEmail).val(cuenta?.email || '');
        $(SELECTORES.txtMovil).val(cuenta?.movil || '');
    }

    function limpiarBusqueda() {
        logPaso('Limpiando busqueda de cuenta');
        cuentaSeleccionada = null;
        $(SELECTORES.inputBusqueda).val('').focus();
        $('#cardGrillaClientes').remove();
        limpiarSeleccionVisual();

        const url = String(window.LimpiarSesionClientesUrl || '').trim();
        if (url) {
            logPaso('Limpiando sesion de clientes');
            $.post(url);
        }
    }

    function registrarCuentaSeleccionada() {
        const url = String(window.ndcfsRegistrarCuentaUrl || '').trim();

        if (!url) {
            logError('Registro de cuenta cancelado: URL no encontrada');
            mostrarMensaje('Error', 'No se encontro la URL para registrar la cuenta seleccionada.', 'error!');
            return;
        }

        logPaso('Registrando cuenta seleccionada');
        mostrarLoaderNdcfs('Preparando operacion...');
        $(SELECTORES.btnSeguir).prop('disabled', true);

        $.ajax({
            url: url,
            type: 'POST',
            dataType: 'json',
            timeout: 30000
        })
            .done(function (response) {
                logPaso('Respuesta registro de cuenta', response);
                if (!response || response.ok !== true) {
                    mostrarMensaje(
                        'Atencion',
                        response?.mensaje || 'No se pudo registrar la cuenta seleccionada.',
                        'warn!'
                    );
                    $(SELECTORES.btnSeguir).prop('disabled', false);
                    return;
                }

                cuentaSeleccionada = {
                    ...(cuentaSeleccionada || {}),
                    ...(response.cuenta || {}),
                    operacionesPermitidas: response.operacionesPermitidas || []
                };

                mostrarResumenCuenta(response);
                $(SELECTORES.modal).modal('hide');
                abrirModalConceptos(response);
            })
            .fail(function (xhr) {
                logError('Error AJAX registrando cuenta', {
                    status: xhr?.status,
                    response: xhr?.responseJSON || xhr?.responseText
                });
                mostrarMensaje(
                    'Error de Comunicacion',
                    xhr?.responseJSON?.mensaje || 'Ocurrio un error al registrar la cuenta seleccionada.',
                    'error!'
                );
                $(SELECTORES.btnSeguir).prop('disabled', false);
            })
            .always(function () {
                ocultarLoaderNdcfs();
            });
    }

    function abrirModalConceptos(response) {
        logPaso('Abriendo modal de conceptos', {
            cuenta: response?.cuenta || cuentaSeleccionada,
            operacionesPermitidas: response?.operacionesPermitidas || cuentaSeleccionada?.operacionesPermitidas || []
        });
        conceptos = [];
        calculoActual = null;
        cargarOperaciones(response?.operacionesPermitidas || cuentaSeleccionada?.operacionesPermitidas || []);
        cargarIvaAlicuotas();
        cargarTiposComprobanteOrigen();
        hidratarDatosCuentaModulo(cuentaSeleccionada || response?.cuenta || {}, 'Conceptos');
        limpiarInputsConcepto();
        renderConceptos();
        actualizarVisibilidadOrigenNc();
        $(SELECTORES.modalConceptos).modal('show');
        setTimeout(function () {
            $(SELECTORES.neto).trigger('focus');
        }, 350);
    }

    function cargarOperaciones(operaciones) {
        logPaso('Cargando operaciones permitidas', operaciones);
        const $select = $(SELECTORES.operacion);
        $select.empty();

        normalizarOperacionesObjetos(operaciones).forEach(function (operacion) {
            $select.append(
                `<option value="${escaparHtml(operacion.codigo)}">${escaparHtml(operacion.descripcion)}</option>`
            );
        });

        if (!$select.val()) {
            $select.append('<option value="">Sin operaciones disponibles</option>');
        }
    }

    function cargarTiposComprobanteOrigen() {
        if (tiposComprobanteOrigenCargados) {
            return;
        }

        const url = String(window.ndcfsObtenerTiposComprobanteUrl || '').trim();
        const $select = $(SELECTORES.comboTcoOri);

        if (!url || $select.length === 0) {
            logWarn('No se encontro URL o combo para tipos de comprobante origen');
            $select.html('<option value="">Sin tipos disponibles</option>');
            return;
        }

        logPaso('Solicitando tipos de comprobante origen', { url: url });
        $select.prop('disabled', true).html('<option value="">Cargando...</option>');

        $.ajax({
            url: url,
            type: 'GET',
            dataType: 'json',
            timeout: 30000
        })
            .done(function (response) {
                logPaso('Respuesta tipos de comprobante origen', response);
                const datos = Array.isArray(response?.datos) ? response.datos : [];
                if (!response || response.ok !== true || datos.length === 0) {
                    const mensaje = response?.mensaje || 'No se encontraron tipos de comprobante origen.';
                    logWarn('Tipos de comprobante origen no disponibles', response);
                    $select.html(`<option value="">${escaparHtml(mensaje)}</option>`);
                    return;
                }

                hidratarComboTiposComprobanteOrigen(datos);
                tiposComprobanteOrigenCargados = true;
            })
            .fail(function (xhr) {
                logError('Error AJAX obteniendo tipos de comprobante origen', {
                    status: xhr?.status,
                    response: xhr?.responseJSON || xhr?.responseText
                });
                $select.html('<option value="">No disponible</option>');
            })
            .always(function () {
                $select.prop('disabled', false);
                sincronizarTipoComprobanteOrigen();
            });
    }

    function hidratarComboTiposComprobanteOrigen(tipos) {
        const $select = $(SELECTORES.comboTcoOri);
        $select.empty().append('<option value="">Seleccione...</option>');

        tipos.forEach(function (tipo) {
            const id = String(tipo.tco_id || '').trim();
            if (!id) {
                return;
            }

            const descripcion = String(tipo.tco_desc || '').trim();
            const letra = String(tipo.tco_letra || '').trim();
            const texto = `${id}${letra ? ` ${letra}` : ''}${descripcion ? ` - ${descripcion}` : ''}`;
            $select.append(
                `<option value="${escaparHtml(id)}" data-letra="${escaparHtml(letra)}">${escaparHtml(texto)}</option>`
            );
        });
    }

    function sincronizarTipoComprobanteOrigen() {
        $(SELECTORES.tcoOri).val(obtenerTipoComprobanteOrigen());
    }

    function sincronizarComprobanteOrigen() {
        $(SELECTORES.compteOri).val(obtenerComprobanteOrigenFormateado());
    }

    function obtenerTipoComprobanteOrigen() {
        return String($(SELECTORES.comboTcoOri).val() || $(SELECTORES.tcoOri).val() || '').trim();
    }

    function obtenerComprobanteOrigenFormateado() {
        const puntoVenta = String($(SELECTORES.puntoVentaOri).val() || '').trim();
        const numero = String($(SELECTORES.numeroCompteOri).val() || '').trim();

        if (!puntoVenta || !numero) {
            return '';
        }

        return `${puntoVenta.padStart(4, '0')}-${numero.padStart(8, '0')}`;
    }

    function validarComprobanteOrigenNc() {
        const tco = obtenerTipoComprobanteOrigen();
        const puntoVenta = String($(SELECTORES.puntoVentaOri).val() || '').trim();
        const numero = String($(SELECTORES.numeroCompteOri).val() || '').trim();

        if (!tco) {
            logWarn('Calculo cancelado: NC sin tipo de comprobante origen');
            mostrarMensajeConcepto('Para Nota de Credito debe seleccionar el tipo de comprobante origen.');
            $(SELECTORES.comboTcoOri).trigger('focus');
            return false;
        }

        if (!/^\d{1,4}$/.test(puntoVenta)) {
            logWarn('Calculo cancelado: PV origen invalido', { puntoVenta: puntoVenta });
            mostrarMensajeConcepto('El punto de venta origen debe contener entre 1 y 4 digitos numericos.');
            $(SELECTORES.puntoVentaOri).trigger('focus');
            return false;
        }

        if (!/^\d{1,8}$/.test(numero)) {
            logWarn('Calculo cancelado: numero origen invalido', { numero: numero });
            mostrarMensajeConcepto('El numero de comprobante origen debe contener entre 1 y 8 digitos numericos.');
            $(SELECTORES.numeroCompteOri).trigger('focus');
            return false;
        }

        sincronizarTipoComprobanteOrigen();
        sincronizarComprobanteOrigen();
        return true;
    }

    function limpiarComprobanteOrigenNc() {
        $(SELECTORES.comboTcoOri).val('');
        $(SELECTORES.tcoOri).val('');
        $(SELECTORES.puntoVentaOri).val('');
        $(SELECTORES.numeroCompteOri).val('');
        $(SELECTORES.compteOri).val('');
        $(SELECTORES.repetidoOri).val('0');
    }

    function limitarSoloDigitos($input, maxLength) {
        const valor = String($input.val() || '');
        const limpio = valor.replace(/\D/g, '').slice(0, maxLength);
        if (valor !== limpio) {
            $input.val(limpio);
        }
    }
    function permitirSoloDecimal(event) {
        const teclasPermitidas = ['Backspace', 'Delete', 'Tab', 'Enter', 'Escape', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End'];
        if (event.ctrlKey || event.metaKey || teclasPermitidas.includes(event.key)) {
            return;
        }
        if (/^[0-9.,]$/.test(event.key)) {
            return;
        }
        event.preventDefault();
    }

    function limitarInputDecimal($input) {
        const valor = String($input.val() || '');
        const limpio = valor.replace(/[^0-9.,]/g, '');
        if (valor !== limpio) {
            $input.val(limpio);
        }
    }

    function cargarIvaAlicuotas() {
        if (ivaAlicuotasCargadas) {
            logPaso('Alicuotas IVA ya cargadas');
            recalcularAlicuota();
            return;
        }

        const url = String(window.ndcfsObtenerIvaAlicuotasUrl || '').trim();
        const $select = $(SELECTORES.iva);

        if (!url) {
            logError('No se encontro URL para obtener alicuotas IVA');
            cargarIvaAlicuotasFallback();
            return;
        }

        logPaso('Solicitando alicuotas IVA', { url: url });
        $select.prop('disabled', true).html('<option value="">Cargando...</option>');

        $.ajax({
            url: url,
            type: 'GET',
            dataType: 'json',
            timeout: 30000
        })
            .done(function (response) {
                logPaso('Respuesta alicuotas IVA', response);
                if (!response || response.ok !== true || !Array.isArray(response.lista) || response.lista.length === 0) {
                    logWarn('Alicuotas IVA no disponibles desde servicio', response);
                    cargarIvaAlicuotasFallback();
                    return;
                }

                hidratarComboIva(response.lista);
                ivaAlicuotasCargadas = true;
            })
            .fail(function (xhr) {
                logError('Error AJAX obteniendo alicuotas IVA', {
                    status: xhr?.status,
                    response: xhr?.responseJSON || xhr?.responseText
                });
                cargarIvaAlicuotasFallback();
            })
            .always(function () {
                $select.prop('disabled', false);
                recalcularAlicuota();
            });
    }

    function hidratarComboIva(lista) {
        const $select = $(SELECTORES.iva);
        $select.empty();

        lista
            .map(function (item) {
                return {
                    valor: parsearNumero(item.ivaAlicuota ?? item.IVA_Alicuota, NaN),
                    grl: item.ivaGrl ?? item.IVA_Grl ?? '',
                    extra: item.ivaExtra ?? item.IVA_Extra ?? '',
                    afip: item.ivaAfip ?? item.IVA_Afip ?? ''
                };
            })
            .filter(function (item) {
                return Number.isFinite(item.valor) && item.valor >= 0;
            })
            .sort(function (a, b) {
                return a.valor - b.valor;
            })
            .forEach(function (item) {
                const textoBase = formatearPorcentaje(item.valor);
                
                $select.append(
                    `<option class="text-end" value="${item.valor}" data-iva-grl="${escaparHtml(item.grl)}" data-iva-extra="${escaparHtml(item.extra)}" data-iva-afip="${escaparHtml(item.afip)}">${escaparHtml(textoBase)}</option>`
                );
            });

        if ($select.find('option[value="21"]').length > 0) {
            $select.val('21');
        }

        if (!$select.val()) {
            $select.prop('selectedIndex', 0);
        }
    }

    function cargarIvaAlicuotasFallback() {
        logWarn('Usando fallback local de alicuotas IVA');
        const $select = $(SELECTORES.iva);
        $select.html(`
            <option class="text-end" value="0">0.00</option>
            <option class="text-end" value="10.5">10.50</option>
            <option class="text-end" value="21" selected>21.00</option>
            <option class="text-end" value="27">27.00</option>
        `);
        ivaAlicuotasCargadas = true;
    }

    function agregarConcepto() {
        const concepto = {
            concepto: String($(SELECTORES.concepto).val() || '').trim(),
            cantidad: parsearNumero($(SELECTORES.cantidad).val(), 1),
            netoGravado: parsearNumero($(SELECTORES.neto).val(), 0),
            alicuotaIva: parsearNumero($(SELECTORES.iva).val(), 0),
            alicuotaMonto: parsearNumero($(SELECTORES.alicuota).val(), 0),
            percepcionIb: parsearNumero($(SELECTORES.perIb).val(), 0),
            percepcionIva: parsearNumero($(SELECTORES.perIva).val(), 0)
        };
        logPaso('Intentando agregar concepto', concepto);

        const validacion = validarConcepto(concepto);
        if (!validacion.ok) {
            logWarn('Concepto invalido', validacion);
            mostrarMensajeConcepto(validacion.mensaje);
            return;
        }

        conceptos.push(concepto);
        logPaso('Concepto agregado', { cantidadConceptos: conceptos.length, total: calcularTotalConceptos() });
        limpiarInputsConcepto();
        renderConceptos();
        $(SELECTORES.neto).trigger('focus');
    }

    function validarConcepto(concepto) {
        if (!concepto.concepto) {
            return { ok: false, mensaje: 'Ingrese la descripcion del concepto.' };
        }

        if (!Number.isFinite(concepto.netoGravado) || concepto.netoGravado <= 0) {
            return { ok: false, mensaje: 'Ingrese un neto mayor a cero.' };
        }

        if (!Number.isFinite(concepto.cantidad) || concepto.cantidad <= 0) {
            return { ok: false, mensaje: 'Ingrese una cantidad mayor a cero.' };
        }

        if (!Number.isFinite(concepto.alicuotaIva) || concepto.alicuotaIva < 0) {
            return { ok: false, mensaje: 'Seleccione una alicuota de IVA valida.' };
        }

        if (concepto.percepcionIb < 0 || concepto.percepcionIva < 0) {
            return { ok: false, mensaje: 'Las percepciones no pueden ser negativas.' };
        }

        return { ok: true, mensaje: 'OK' };
    }

    function eliminarConcepto(index) {
        const indice = Number(index);
        if (!Number.isInteger(indice) || indice < 0 || indice >= conceptos.length) {
            return;
        }

        conceptos.splice(indice, 1);
        logPaso('Concepto eliminado', { index: indice, cantidadConceptos: conceptos.length });
        renderConceptos();
    }

    function renderConceptos() {
        const $tbody = $(SELECTORES.tbodyConceptos);
        $tbody.empty();

        if (conceptos.length === 0) {
            $tbody.html(`
                <tr id="rowNdcfsSinConceptos">
                    <td colspan="5" class="text-center text-muted py-4">
                        <i class="bx bx-info-circle"></i>
                        Sin conceptos cargados
                    </td>
                </tr>
            `);
            $(SELECTORES.btnSeguirConceptos).prop('disabled', true);
            $(SELECTORES.totalConceptos).text(formatearMoneda(0));
            actualizarEstadoOperacionPorConceptos();
            return;
        }

        let total = 0;
        conceptos.forEach(function (item, index) {
            const totalItem = item.netoGravado * item.cantidad;
            total += totalItem;

            $tbody.append(`
                <tr>
                    <td>${escaparHtml(item.concepto)}</td>
                    <td class="text-end">${formatearNumero(item.cantidad, 0)}</td>
                    <td class="text-end">${formatearMoneda(item.netoGravado)}</td>
                    <td class="text-end fw-bold">${formatearMoneda(totalItem)}</td>
                    <td class="text-center">
                        <button type="button"
                                class="btn btn-sm btn-danger btn-ndcfs-eliminar-concepto"
                                data-index="${index}"
                                title="Eliminar concepto">
                            <i class="bx bx-trash"></i>
                        </button>
                    </td>
                </tr>
            `);
        });

        $(SELECTORES.totalConceptos).text(formatearMoneda(total));
        $(SELECTORES.btnSeguirConceptos).prop('disabled', false);
        actualizarEstadoOperacionPorConceptos();
    }

    function calcularConceptos() {
        if (calculoEnCurso) {
            logWarn('Calculo ignorado: ya existe uno en curso');
            return;
        }

        if (conceptos.length === 0) {
            logWarn('Calculo cancelado: no hay conceptos');
            mostrarMensajeConcepto('Debe cargar al menos un concepto.');
            return;
        }

        const coTipo = String($(SELECTORES.operacion).val() || '').trim().toUpperCase();
        if (!coTipo) {
            logWarn('Calculo cancelado: operacion no seleccionada');
            mostrarMensajeConcepto('Seleccione el tipo de comprobante a emitir.');
            return;
        }

        if (coTipo === 'NC' && !validarComprobanteOrigenNc()) {
            return;
        }

        const url = String(window.ndcfsCalcularConceptosUrl || '').trim();
        if (!url) {
            logError('Calculo cancelado: URL no encontrada');
            mostrarMensaje('Error', 'No se encontro la URL de calculo.', 'error!');
            return;
        }

        const request = {
            coTipo: coTipo,
            tcoIdOri: obtenerTipoComprobanteOrigen(),
            cmCompteOri: obtenerComprobanteOrigenFormateado(),
            cmRepetidoOri: String($(SELECTORES.repetidoOri).val() || '').trim(),
            conceptos: conceptos.map(function (item) {
                return {
                    concepto: item.concepto,
                    cantidad: item.cantidad,
                    netoGravado: item.netoGravado,
                    alicuotaIva: item.alicuotaIva,
                    percepcionIb: item.percepcionIb,
                    percepcionIva: item.percepcionIva
                };
            })
        };

        logPaso('Request calculo de conceptos', request);
        calculoEnCurso = true;
        mostrarLoaderNdcfs('Calculando operacion...');
        $(SELECTORES.btnSeguirConceptos).prop('disabled', true).html('<i class="bx bx-loader-alt bx-spin"></i> CALCULANDO');

        $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json',
            dataType: 'json',
            data: JSON.stringify(request),
            timeout: 45000
        })
            .done(function (response) {
                logPaso('Response calculo de conceptos', response);
                if (!response || response.ok !== true) {
                    mostrarMensajeConcepto(response?.mensaje || 'No se pudo calcular la operacion.');
                    return;
                }

                calculoActual = response;
                abrirModalCalculo(response);
            })
            .fail(function (xhr) {
                logError('Error AJAX calculando conceptos', {
                    status: xhr?.status,
                    response: xhr?.responseJSON || xhr?.responseText
                });
                mostrarMensaje('Error de Comunicacion', xhr?.responseJSON?.mensaje || 'Ocurrio un error al calcular.', 'error!');
            })
            .always(function () {
                calculoEnCurso = false;
                ocultarLoaderNdcfs();
                $(SELECTORES.btnSeguirConceptos).prop('disabled', conceptos.length === 0).html('<i class="bx bx-right-arrow-circle"></i> SEGUIR');
            });
    }

    function abrirModalCalculo(response) {
        logPaso('Abriendo modal de calculo', response);
        hidratarDatosCuentaModulo(cuentaSeleccionada || response?.cuenta || {}, 'Calculo');
        renderSubtotales(response);

        const operacion = response?.operacion || {};
        $('#lblNdcfsOperacionCalculo').text(`${operacion.codigo || ''} - ${operacion.descripcion || ''}`.trim());
        $('#lblNdcfsCuentaCalculo').text(cuentaSeleccionada?.cta_denominacion || cuentaSeleccionada?.denominacion || cuentaSeleccionada?.nombre || '-');
        $('#lblNdcfsOrigenComprobanteCalculo').text(obtenerTextoComprobanteOrigen());
        $('#lblNdcfsCantidadConceptosCalculo').text(String(conceptos.length));

        $(SELECTORES.modalConceptos).modal('hide');
        $(SELECTORES.modalCalculo).modal('show');
    }

    function renderSubtotales(response) {
        const $tbody = $(SELECTORES.tbodySubtotales);
        const subtotales = obtenerSubtotalesCalculo(response);
        $tbody.empty();

        if (subtotales.length === 0) {
            $tbody.html(`
                <tr>
                    <td colspan="2" class="text-center text-muted py-4">
                        <i class="bx bx-info-circle"></i>
                        Sin subtotales calculados
                    </td>
                </tr>
            `);
        } else {
            subtotales.forEach(function (item) {
                const importe = parsearNumero(item.importe, 0);
                $tbody.append(`
                    <tr>
                        <td>${escaparHtml(item.concepto || item.tipo || '-')}</td>
                        <td class="text-end ${esTotal(item) ? 'fw-bold text-success' : ''}">
                            ${formatearMoneda(importe)}
                        </td>
                    </tr>
                `);
            });
        }

        const total = calcularTotalSubtotales(subtotales);
        $(SELECTORES.totalCalculo).text(formatearMoneda(total));
    }

    function obtenerSubtotalesCalculo(response) {
        if (Array.isArray(response?.subtotales) && response.subtotales.length > 0) {
            return response.subtotales;
        }

        const jsonSubtotal =
            response?.calculo?.json_subtotal ||
            response?.calculo?.jsonSubtotal ||
            response?.json_subtotal ||
            response?.jsonSubtotal ||
            '';

        return normalizarSubtotalesCalculo(parsearJsonSeguro(jsonSubtotal));
    }

    function normalizarSubtotalesCalculo(origen) {
        const filas = Array.isArray(origen)
            ? origen
            : Array.isArray(origen?.subtotales)
                ? origen.subtotales
                : Array.isArray(origen?.Subtotales)
                    ? origen.Subtotales
                    : Array.isArray(origen?.data)
                        ? origen.data
                        : Array.isArray(origen?.Data)
                            ? origen.Data
                            : [];

        return filas.map(function (item) {
            const fila = item || {};
            return {
                concepto: fila.concepto || fila.Concepto || fila.descripcion || fila.Descripcion || fila.tipo || fila.Tipo || '',
                tipo: fila.tipo || fila.Tipo || '',
                importe: parsearNumero(fila.importe ?? fila.Importe ?? fila.total ?? fila.Total ?? fila.monto ?? fila.Monto, 0)
            };
        }).filter(function (item) {
            return item.concepto || item.tipo || item.importe !== 0;
        });
    }

    function parsearJsonSeguro(valor) {
        if (!valor) {
            return [];
        }

        if (Array.isArray(valor) || typeof valor === 'object') {
            return valor;
        }

        if (typeof valor !== 'string') {
            return [];
        }

        try {
            const parseado = JSON.parse(valor);
            return typeof parseado === 'string'
                ? parsearJsonSeguro(parseado)
                : parseado;
        } catch (error) {
            logWarn('No se pudo parsear json_subtotal', {
                error: String(error || ''),
                jsonSubtotal: valor
            });
            return [];
        }
    }

    function calcularTotalSubtotales(subtotales) {
        if (!Array.isArray(subtotales)) {
            return 0;
        }

        return subtotales.reduce(function (total, item) {
            return total + parsearNumero(item?.importe, 0);
        }, 0);
    }

    function confirmarOperacion() {
        if (confirmacionEnCurso) {
            logWarn('Confirmacion ignorada: ya existe una en curso');
            return;
        }

        if (!calculoActual) {
            logWarn('Confirmacion cancelada: no hay calculo actual');
            mostrarMensaje('Atencion', 'Debe calcular la operacion antes de finalizar.', 'warn!');
            return;
        }

        logPaso('Solicitando confirmacion final al operador', calculoActual);
        AbrirMensaje(
            'Confirmar Operacion',
            `
                <div class="text-start">
                    <p class="mb-2"><strong>Â¿Desea finalizar la operacion?</strong></p>
                    <p class="mb-0">Se emitira el comprobante y se registrara en cuenta corriente.</p>
                </div>
            `,
            function (respuesta) {
                $('#msjModal').modal('hide');
                if (respuesta !== 'SI') {
                    return;
                }

                ejecutarConfirmacion();
            },
            true,
            ['Confirmar', 'Cancelar'],
            'quest!',
            null
        );
    }

    function ejecutarConfirmacion() {
        const url = String(window.ndcfsConfirmarOperacionUrl || '').trim();
        if (!url) {
            logError('Confirmacion cancelada: URL no encontrada');
            mostrarMensaje('Error', 'No se encontro la URL de confirmacion.', 'error!');
            return;
        }

        logPaso('Ejecutando confirmacion de operacion', { url: url, calculoActual: calculoActual });
        confirmacionEnCurso = true;
        mostrarLoaderNdcfs('Finalizando operacion. Aguarde, no toque nada hasta que el proceso termine...');
        mostrarEsperaFinalizacion();
        $(SELECTORES.btnFinalizar).prop('disabled', true).html('<i class="bx bx-loader-alt bx-spin"></i> FINALIZANDO');

        $.ajax({
            url: url,
            type: 'POST',
            dataType: 'json',
            timeout: 45000
        })
            .done(function (response) {
                logPaso('Response confirmacion de operacion', response);
                if (!response || response.ok !== true) {
                    mostrarMensaje('Atencion', response?.mensaje || 'No se pudo confirmar la operacion.', 'warn!');
                    return;
                }

                procesarConfirmacionExitosa(response);
            })
            .fail(function (xhr) {
                logError('Error AJAX confirmando operacion', {
                    status: xhr?.status,
                    response: xhr?.responseJSON || xhr?.responseText
                });
                mostrarMensaje('Error de Comunicacion', xhr?.responseJSON?.mensaje || 'Ocurrio un error al confirmar.', 'error!');
            })
            .always(function () {
                confirmacionEnCurso = false;
                ocultarLoaderNdcfs();
                ocultarEsperaFinalizacion();
                $(SELECTORES.btnFinalizar).prop('disabled', false).html('<i class="bx bx-check-circle"></i> FINALIZAR');
            });
    }

    function mostrarEsperaFinalizacion() {
        const $modal = $(SELECTORES.modalCalculo);
        if ($modal.find('#ndcfsEsperaFinalizacion').length > 0) {
            return;
        }
        $modal.find('.modal-body').prepend(`
            <div id="ndcfsEsperaFinalizacion" class="alert alert-warning border-golden d-flex align-items-center mb-3" role="alert">
                <i class="bx bx-loader-alt bx-spin fs-3 me-2"></i>
                <div>
                    <strong>Finalizando operacion.</strong>
                    <div class="small">Aguarde sin tocar la pantalla hasta que el proceso termine.</div>
                </div>
            </div>
        `);
    }

    function ocultarEsperaFinalizacion() {
        $('#ndcfsEsperaFinalizacion').remove();
    }

    function obtenerUrlReinicioModulo() {
        return window.ndcfsModuloIndexUrl || window.location.pathname || '/';
    }

    function procesarConfirmacionExitosa(response) {
        const comprobante = Array.isArray(response?.data)
            ? response.data[0]
            : null;
        const debeImprimir = response?.debe_imprimir === true;
        const modoReporte = String(response?.reporte_modo || 'PANTALLA')
            .trim()
            .toUpperCase();
        const mensajeOperacion = construirMensajeConfirmacion(response, comprobante);

        const mostrarResultadoYVolver = function (titulo, mensaje, tipo) {
            AbrirMensaje(
                titulo,
                mensaje,
                function () {
                    $('#msjModal').modal('hide');
                    window.location.href = obtenerUrlReinicioModulo();
                },
                false,
                ['Aceptar'],
                tipo || 'success!',
                null
            );
        };

        const cerrarConExito = function () {
            mostrarResultadoYVolver(
                'Operacion Confirmada',
                mensajeOperacion,
                'success!'
            );
        };

        if (!debeImprimir) {
            cerrarConExito();
            return;
        }

        if (!comprobante) {
            mostrarResultadoYVolver(
                'Operacion Confirmada',
                `${mensajeOperacion}<br>` +
                '<span class="text-warning">No se recibieron datos suficientes para presentar el comprobante.</span>',
                'warn!'
            );
            return;
        }

        if (typeof ModuloReportes === 'undefined') {
            mostrarResultadoYVolver(
                'Operacion Confirmada',
                `${mensajeOperacion}<br>` +
                '<span class="text-warning">No se encontro el modulo de reportes para presentar el comprobante.</span>',
                'warn!'
            );
            return;
        }

        mostrarLoaderNdcfs('Generando comprobante...');
        ModuloReportes.generarYVisualizarReporte(
            {
                tco_letra: comprobante.tco_letra,
                tco_id: comprobante.tco_id,
                cm_compte: comprobante.cm_compte,
                cm_repetido: comprobante.cm_repetido
            },
            {
                modo: modoReporte,
                titulo: 'Operacion Confirmada'
            }
        ).then(function (exitoso) {
            logPaso('Generacion de reporte de comprobante finalizada', { exitoso: exitoso });
            cerrarConExito();
        }).catch(function (error) {
            logError('Error generando reporte de comprobante', error);
            cerrarConExito();
        }).finally(function () {
            ocultarLoaderNdcfs();
        });
    }
    function construirMensajeConfirmacion(response, comprobante) {
        const mensajePrincipal = response?.mensaje || 'La operacion fue confirmada correctamente.';
        const mensajeDetalle = response?.resultado_completo || response?.resultado_msj || '';
        const numero = comprobante?.cm_compte || '';
        const letra = comprobante?.tco_letra || '';
        const tipo = comprobante?.tco_id || '';

        let html = `<div class="text-center">
            <div class="mb-3">
                <i class='bx bx-check-circle text-golden' style="font-size: 4rem;"></i>
            </div>
            <h4 class="text-golden mb-3">${escaparHtml(mensajePrincipal)}</h4>`;

        if (numero || letra || tipo) {
            html += `<div class="alert alert-success mb-3">
                <div class="mb-2">
                    <strong class="d-block text-uppercase">Comprobante emitido</strong>
                    ${letra ? `<span class="badge bg-primary fs-6">${escaparHtml(letra)}</span>` : ''}
                </div>
                <div class="mt-2">
                    <small class="text-muted">Numero:</small><br>
                    <strong class="fs-5">${escaparHtml(numero || '-')}</strong>
                </div>
                ${tipo ? `<div class="mt-2"><small class="text-muted">Tipo: ${escaparHtml(tipo)}</small></div>` : ''}
            </div>`;
        }

        if (mensajeDetalle && mensajeDetalle !== mensajePrincipal) {
            html += `<div class="alert alert-info text-start mb-0">
                <i class='bx bx-info-circle'></i> ${escaparHtml(mensajeDetalle)}
            </div>`;
        }

        html += '</div>';
        return html;
    }
    function actualizarVisibilidadOrigenNc() {
        const coTipo = String($(SELECTORES.operacion).val() || '').trim().toUpperCase();
        logPaso('Cambio de operacion', { coTipo: coTipo });
        $('.ndcfs-origen-nc').toggleClass('d-none', coTipo !== 'NC');

        if (coTipo === 'NC') {
            cargarTiposComprobanteOrigen();
            sincronizarTipoComprobanteOrigen();
            sincronizarComprobanteOrigen();
        } else {
            limpiarComprobanteOrigenNc();
        }
    }
    function hidratarDatosCuentaModulo(cuenta, sufijo) {
        const nombre = cuenta.cta_denominacion || cuenta.denominacion || cuenta.nombre || '';
        const id = cuenta.cta_id || cuenta.id || '';
        const domicilio = cuenta.cta_domicilio || cuenta.domicilio || '';
        const tipoNumero = cuenta.cta_documento || cuenta.tipoNumero || cuenta.documento || '';
        const afip = cuenta.afip_desc || cuenta.condicionAfip || '';
        const email = cuenta.cta_email || cuenta.email || '';
        const movil = cuenta.cta_celu || cuenta.movil || '';
        const emite = obtenerDescripcionOperacion(String($(SELECTORES.operacion).val() || ''));
        const origen = cuenta.origen_desc || cuenta.origenDesc || cuenta.origen || '';

        $(`#txtNdcfsNombre${sufijo}`).val(nombre);
        $(`#txtNdcfsId${sufijo}`).val(id);
        $(`#txtNdcfsDomicilio${sufijo}`).val(domicilio);
        $(`#txtNdcfsTipoNumero${sufijo}`).val(tipoNumero);
        $(`#txtNdcfsAfip${sufijo}`).val(afip);
        $(`#txtNdcfsEmail${sufijo}`).val(email);
        $(`#txtNdcfsMovil${sufijo}`).val(movil);
        $(`#txtNdcfsEmite${sufijo}`).val(emite);
        $(`#txtNdcfsOrigen${sufijo}`).val(origen);
    }

    function limpiarInputsConcepto() {
        $(SELECTORES.cantidad).val('1');
        $(SELECTORES.neto).val('');
        $(SELECTORES.alicuota).val('0.00');
        $(SELECTORES.perIb).val('0.00');
        $(SELECTORES.perIva).val('0.00');
        $(SELECTORES.concepto).val('');
        $(SELECTORES.mensajeConcepto).addClass('d-none').empty();
    }

    function recalcularAlicuota() {
        const neto = parsearNumero($(SELECTORES.neto).val(), 0);
        const porcentajeIva = parsearNumero($(SELECTORES.iva).val(), 0);
        const alicuota = calcularIva(neto, porcentajeIva);

        $(SELECTORES.alicuota).val(formatearNumero(alicuota, 2));
    }

    function formatearInputNumerico($input, decimales) {
        const valor = parsearNumero($input.val(), 0);
        $input.val(formatearNumero(valor, decimales));
    }

    function normalizarInputParaEdicion($input) {
        const valorActual = String($input.val() || '').trim();
        if (!valorActual) {
            return;
        }

        const valor = parsearNumero(valorActual, 0);
        $input.val(Number.isFinite(valor) ? String(valor) : '');
    }

    function mostrarMensajeConcepto(mensaje) {
        $(SELECTORES.mensajeConcepto)
            .removeClass('d-none')
            .html(`<i class="bx bx-info-circle"></i> ${escaparHtml(mensaje)}`);
    }

    function cancelarOperacion(callback) {
        const url = String(window.ndcfsCancelarOperacionUrl || '').trim();
        logPaso('Cancelando operacion', { url: url });

        if (!url) {
            callback?.();
            return;
        }

        $.ajax({
            url: url,
            type: 'POST',
            dataType: 'json',
            timeout: 10000
        })
            .always(function () {
                callback?.();
            });
    }

    function limpiarSeleccionVisual() {
        const mostrarSinCliente = arguments.length === 0 ? true : arguments[0] === true;
        $(SELECTORES.txtNombre).val('');
        $(SELECTORES.txtClienteId).val('');
        $(SELECTORES.txtDomicilio).val('');
        $(SELECTORES.txtCondicionAfip).val('');
        $(SELECTORES.txtTipoNumero).val('');
        $(SELECTORES.txtEmite).val('');
        $(SELECTORES.txtEmail).val('');
        $(SELECTORES.txtMovil).val('');
        $(SELECTORES.cardDatos).hide();
        $(SELECTORES.alertSinCliente).toggle(mostrarSinCliente);
        $(SELECTORES.btnSeguir).prop('disabled', true);
        $(SELECTORES.resumenCuenta).addClass('d-none').empty();
    }

    function mostrarResumenCuenta(response) {
        const cuenta = response?.cuenta || cuentaSeleccionada || {};
        const operaciones = normalizarOperaciones(
            response?.operacionesPermitidas || cuenta.operacionesPermitidas || []
        );
        const origen = String(cuenta.origen_desc || cuenta.origenDesc || cuenta.origen || '').trim();
        const nombre = cuenta.cta_denominacion || cuenta.denominacion || cuenta.nombre || '';

        $(SELECTORES.resumenCuenta)
            .removeClass('d-none')
            .html(`
                <div class="d-flex align-items-start">
                    <i class="bx bx-check-circle fs-3 me-3 text-golden-dark"></i>
                    <div>
                        <strong>Cuenta seleccionada.</strong>
                        <div class="small mt-1">
                            ${escaparHtml(nombre)}
                            ${origen ? ` - ${escaparHtml(origen)}` : ''}
                        </div>
                        ${operaciones.length > 0
                            ? `<div class="small mt-2">
                                Operaciones habilitadas: ${operaciones.map(escaparHtml).join(', ')}
                               </div>`
                            : ''}
                    </div>
                </div>
            `);
    }

    function mostrarLoaderNdcfs(mensaje) {
        if (typeof mostrarLoader === 'function') {
            mostrarLoader(mensaje || 'Procesando...');
        }
    }

    function ocultarLoaderNdcfs() {
        if (typeof ocultarLoader === 'function') {
            ocultarLoader();
        }
    }

    function logPaso(paso, datos) {
        if (datos === undefined) {
            console.log(LOG_PREFIX, paso);
            return;
        }

        console.log(LOG_PREFIX, paso, datos);
    }

    function logWarn(paso, datos) {
        if (datos === undefined) {
            console.warn(LOG_PREFIX, paso);
            return;
        }

        console.warn(LOG_PREFIX, paso, datos);
    }

    function logError(paso, datos) {
        if (datos === undefined) {
            console.error(LOG_PREFIX, paso);
            return;
        }

        console.error(LOG_PREFIX, paso, datos);
    }

    function normalizarOperaciones(operaciones) {
        if (!Array.isArray(operaciones)) {
            return [];
        }

        return operaciones
            .map(function (operacion) {
                if (typeof operacion === 'string') {
                    return operacion;
                }

                const codigo = operacion?.codigo || '';
                const descripcion = operacion?.descripcion || '';
                return descripcion ? `${codigo} - ${descripcion}` : codigo;
            })
            .filter(Boolean);
    }

    function normalizarOperacionesObjetos(operaciones) {
        if (!Array.isArray(operaciones)) {
            return [];
        }

        return operaciones
            .map(function (operacion) {
                if (typeof operacion === 'string') {
                    return {
                        codigo: operacion,
                        descripcion: obtenerDescripcionOperacion(operacion)
                    };
                }

                return {
                    codigo: String(operacion?.codigo || '').trim(),
                    descripcion: String(operacion?.descripcion || operacion?.codigo || '').trim()
                };
            })
            .filter(function (operacion) {
                return operacion.codigo;
            });
    }

    function obtenerDescripcionOperacion(codigo) {
        switch (String(codigo || '').trim().toUpperCase()) {
            case 'ND':
                return 'Nota de Debito';
            case 'NC':
                return 'Nota de Credito';
            case 'FS':
                return 'Factura de Servicio';
            default:
                return '';
        }
    }

    function obtenerTextoComprobanteOrigen() {
        const coTipo = String($(SELECTORES.operacion).val() || '').trim().toUpperCase();
        if (coTipo !== 'NC') {
            return 'No requiere';
        }

        const tco = obtenerTipoComprobanteOrigen();
        const compte = obtenerComprobanteOrigenFormateado();
        const repetido = String($(SELECTORES.repetidoOri).val() || '').trim();
        return `${tco} ${compte}${repetido ? ` (${repetido})` : ''}`.trim();
    }

    function parsearNumero(valor, defecto) {
        if (typeof valor === 'number') {
            return Number.isFinite(valor) ? valor : defecto;
        }

        let normalizado = String(valor ?? '')
            .replace(/\$/g, '')
            .replace(/\s/g, '')
            .trim();

        if (!normalizado) {
            return defecto;
        }

        const ultimoPunto = normalizado.lastIndexOf('.');
        const ultimaComa = normalizado.lastIndexOf(',');

        if (ultimoPunto >= 0 && ultimaComa >= 0) {
            const separadorDecimal = ultimoPunto > ultimaComa ? '.' : ',';
            const separadorMiles = separadorDecimal === '.' ? ',' : '.';
            normalizado = normalizado
                .replaceAll(separadorMiles, '')
                .replace(separadorDecimal, '.');
        } else if (ultimaComa >= 0) {
            const decimales = normalizado.length - ultimaComa - 1;
            normalizado = decimales === 3
                ? normalizado.replaceAll(',', '')
                : normalizado.replace(',', '.');
        } else if (ultimoPunto >= 0) {
            const decimales = normalizado.length - ultimoPunto - 1;
            normalizado = decimales === 3
                ? normalizado.replaceAll('.', '')
                : normalizado;
        }

        const numero = parseFloat(normalizado);
        return Number.isFinite(numero) ? numero : defecto;
    }

    function calcularIva(neto, alicuota) {
        return Math.round((Number(neto || 0) * Number(alicuota || 0) / 100) * 100) / 100;
    }

    function calcularTotalConceptos() {
        return conceptos.reduce(function (total, item) {
            return total + (item.netoGravado * item.cantidad);
        }, 0);
    }

    function formatearMoneda(valor) {
        return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'ARS',
            minimumFractionDigits: 2
        }).format(Number(valor || 0));
    }

    function formatearNumero(valor, decimales) {
        const digitos = Number.isInteger(decimales) ? decimales : null;
        return new Intl.NumberFormat('en-US', {
            minimumFractionDigits: digitos ?? 0,
            maximumFractionDigits: digitos ?? 3
        }).format(Number(valor || 0));
    }

    function formatearPorcentaje(valor) {
        return new Intl.NumberFormat('en-US', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        }).format(Number(valor || 0));
    }

    function esTotal(item) {
        const concepto = String(item?.concepto || item?.tipo || '').trim().toUpperCase();
        return concepto === 'TOTAL';
    }

    function bloquearBusqueda(bloquear) {
        busquedaEnCurso = bloquear;
        $(SELECTORES.inputBusqueda).prop('disabled', bloquear);
        $(SELECTORES.btnBuscar).prop('disabled', bloquear);
    }

    function mostrarMensaje(titulo, mensaje, tipo) {
        AbrirMensaje(
            titulo,
            mensaje,
            function () {
                $('#msjModal').modal('hide');
            },
            false,
            ['Aceptar'],
            tipo || 'info!'
        );
    }

    function escaparHtml(valor) {
        return String(valor ?? '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }
})();







