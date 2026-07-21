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
        compteOri: '#txtNdcfsCompteOri',
        repetidoOri: '#txtNdcfsRepetidoOri',
        cantidad: '#txtNdcfsCantidad',
        neto: '#txtNdcfsNeto',
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

    $(function () {
        logPaso('Inicializando modulo', {
            buscarClienteUrl: window.BuscarClienteUrl,
            registrarCuentaUrl: window.ndcfsRegistrarCuentaUrl,
            calcularUrl: window.ndcfsCalcularConceptosUrl,
            confirmarUrl: window.ndcfsConfirmarOperacionUrl
        });
        inicializarVista();
        registrarEventos();

        setTimeout(function () {
            logPaso('Abriendo modal inicial de busqueda de cuenta');
            $(SELECTORES.modal).modal('show');
        }, 250);
    });

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
        hidratarDatosCuentaModulo(cuentaSeleccionada || response?.cuenta || {}, 'Conceptos');
        limpiarInputsConcepto();
        renderConceptos();
        actualizarVisibilidadOrigenNc();
        $(SELECTORES.modalConceptos).modal('show');
        setTimeout(function () {
            $(SELECTORES.concepto).trigger('focus');
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

    function agregarConcepto() {
        const concepto = {
            concepto: String($(SELECTORES.concepto).val() || '').trim(),
            cantidad: parsearNumero($(SELECTORES.cantidad).val(), 1),
            netoGravado: parsearNumero($(SELECTORES.neto).val(), 0),
            alicuotaIva: parsearNumero($(SELECTORES.iva).val(), 0),
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
        $(SELECTORES.concepto).trigger('focus');
    }

    function validarConcepto(concepto) {
        if (!concepto.concepto) {
            return { ok: false, mensaje: 'Ingrese la descripcion del concepto.' };
        }

        if (!Number.isFinite(concepto.netoGravado) || concepto.netoGravado <= 0) {
            return { ok: false, mensaje: 'Ingrese un neto gravado mayor a cero.' };
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
            return;
        }

        let total = 0;
        conceptos.forEach(function (item, index) {
            const iva = calcularIva(item.netoGravado, item.alicuotaIva);
            const totalItem = (item.netoGravado + iva + item.percepcionIb + item.percepcionIva) * item.cantidad;
            total += totalItem;

            $tbody.append(`
                <tr>
                    <td>${escaparHtml(item.concepto)}</td>
                    <td class="text-end">${formatearMoneda(item.netoGravado)}</td>
                    <td class="text-end">${formatearMoneda(iva)}</td>
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

        if (coTipo === 'NC' && (!String($(SELECTORES.tcoOri).val() || '').trim() || !String($(SELECTORES.compteOri).val() || '').trim())) {
            logWarn('Calculo cancelado: NC sin comprobante origen');
            mostrarMensajeConcepto('Para Nota de Credito debe informar el comprobante origen.');
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
            tcoIdOri: String($(SELECTORES.tcoOri).val() || '').trim(),
            cmCompteOri: String($(SELECTORES.compteOri).val() || '').trim(),
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
        const subtotales = Array.isArray(response?.subtotales) ? response.subtotales : [];
        $tbody.empty();

        let total = Number(response?.calculo?.total || 0);

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

        if (!Number.isFinite(total) || total <= 0) {
            total = calcularTotalConceptos();
        }

        $(SELECTORES.totalCalculo).text(formatearMoneda(total));
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
                    <p class="mb-2"><strong>¿Desea finalizar la operacion?</strong></p>
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
        mostrarLoaderNdcfs('Confirmando operacion...');
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

                AbrirMensaje(
                    'Operacion Confirmada',
                    response.mensaje || 'La operacion fue confirmada correctamente.',
                    function () {
                        $('#msjModal').modal('hide');
                        window.location.href = window.ndcfsMenuCajaUrl || '/';
                    },
                    false,
                    ['Aceptar'],
                    'success!',
                    null
                );
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
                $(SELECTORES.btnFinalizar).prop('disabled', false).html('<i class="bx bx-check-circle"></i> FINALIZAR');
            });
    }

    function actualizarVisibilidadOrigenNc() {
        const coTipo = String($(SELECTORES.operacion).val() || '').trim().toUpperCase();
        logPaso('Cambio de operacion', { coTipo: coTipo });
        $('.ndcfs-origen-nc').toggleClass('d-none', coTipo !== 'NC');
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
        $(SELECTORES.perIb).val('0');
        $(SELECTORES.perIva).val('0');
        $(SELECTORES.concepto).val('');
        $(SELECTORES.mensajeConcepto).addClass('d-none').empty();
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

        const tco = String($(SELECTORES.tcoOri).val() || '').trim();
        const compte = String($(SELECTORES.compteOri).val() || '').trim();
        const repetido = String($(SELECTORES.repetidoOri).val() || '').trim();
        return `${tco} ${compte}${repetido ? ` (${repetido})` : ''}`.trim();
    }

    function parsearNumero(valor, defecto) {
        if (typeof valor === 'number') {
            return Number.isFinite(valor) ? valor : defecto;
        }

        const normalizado = String(valor ?? '')
            .replace(/\$/g, '')
            .replace(/\s/g, '')
            .replace(/\./g, '')
            .replace(',', '.');

        const numero = parseFloat(normalizado);
        return Number.isFinite(numero) ? numero : defecto;
    }

    function calcularIva(neto, alicuota) {
        return Math.round((Number(neto || 0) * Number(alicuota || 0) / 100) * 100) / 100;
    }

    function calcularTotalConceptos() {
        return conceptos.reduce(function (total, item) {
            return total + ((item.netoGravado + calcularIva(item.netoGravado, item.alicuotaIva) + item.percepcionIb + item.percepcionIva) * item.cantidad);
        }, 0);
    }

    function formatearMoneda(valor) {
        return new Intl.NumberFormat('es-AR', {
            style: 'currency',
            currency: 'ARS',
            minimumFractionDigits: 2
        }).format(Number(valor || 0));
    }

    function formatearNumero(valor) {
        return new Intl.NumberFormat('es-AR', {
            minimumFractionDigits: 0,
            maximumFractionDigits: 3
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
