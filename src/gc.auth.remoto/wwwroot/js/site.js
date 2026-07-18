// =============================================================================
// site.js — Lógica del Dashboard del Agente Autorizador
// Escrito en jQuery para mayor legibilidad y mantenibilidad.
// =============================================================================

// ─── Configuración inyectada por el servidor ──────────────────────────────────
const HUB_URL = $('meta[name="api-hub-url"]').attr('content') || '';
const CSRF_TOKEN = $('meta[name="csrf-token"]').attr('content') || '';

function getCurrentUser() {
    return $('meta[name="authenticated-user-id"]').attr('content') || '';
}

function handleSessionError(xhr) {
    if (xhr.status === 401 || xhr.status === 440) {
        const returnUrl = window.location.pathname + window.location.search;
        window.location.assign('/Seguridad/Token/Login?returnUrl=' + encodeURIComponent(returnUrl));
        return true;
    }

    return false;
}

// ─── Estado de la aplicación ───────────────────────────────────────────────────
let requests = [];       // Array de solicitudes pendientes en memoria
let selectedRequestId = null; // ID de la solicitud actualmente seleccionada

// =============================================================================
// Punto de entrada: cuando el DOM esté listo
// =============================================================================
$(document).ready(function () {
    // Configurar fechas por defecto para histórico (Hoy)
    const hoy = new Date().toISOString().split('T')[0];
    $('#historico-desde').val(hoy);
    $('#historico-hasta').val(hoy);

    cargarSolicitudes();
    iniciarSignalR();
});

// =============================================================================
// SignalR: conexión en tiempo real con la API
// =============================================================================
function iniciarSignalR() {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl(HUB_URL)
        .withAutomaticReconnect()
        .build();

    // Evento recibido desde la API vía SignalR
    connection.on('EventoAutorizacionRecibido', function (eventoStr) {
        let evento = typeof eventoStr === 'string' ? JSON.parse(eventoStr) : eventoStr;
        console.log('SignalR - Evento recibido:', evento);

        if (evento.EventType === 'SolicitudAutorizacionCreada') {
            // Nueva solicitud creada: refrescar la bandeja si estamos en pendientes
            if (currentTab === 'pendientes') {
                cargarSolicitudes();
            }

        } else if (evento.EventType === 'SolicitudAutorizacionResuelta') {
            if (currentTab === 'pendientes') {
                // Solicitud resuelta: quitarla de la lista local
                requests = $.grep(requests, function (r) {
                    return r.id !== evento.IdSolicitud;
                });
                renderizarTarjetas();

                // Si era la que estaba seleccionada, limpiar el panel de detalle
                if (selectedRequestId === evento.IdSolicitud) {
                    limpiarSeleccion();
                }
            } else if (currentTab === 'historico') {
                // Si justo resolvieron una mientras veo el histórico del día actual, podríamos recargar, pero por ahora no es crítico
            }
        }
    });

    // Intentar conectar; reintentar en 5 segundos si falla
    connection.start()
        .then(function () {
            console.log('SignalR conectado correctamente.');
        })
        .catch(function (err) {
            console.error('SignalR - Error de conexión:', err.toString());
            setTimeout(iniciarSignalR, 5000);
        });
}

// =============================================================================
// TABS Y NAVEGACIÓN
// =============================================================================
let currentTab = 'pendientes';

function switchTab(tabName) {
    if (currentTab === tabName) return;
    currentTab = tabName;

    $('.tab-btn').removeClass('active');
    $('#tab-' + tabName).addClass('active');

    limpiarSeleccion();

    if (tabName === 'pendientes') {
        $('#panel-title').html('Bandeja de Entrada <span class="badge" id="pending-count">0</span>');
        $('#historico-filters').hide();
        cargarSolicitudes();
    } else {
        $('#panel-title').html('Consultas Históricas <span class="badge" id="pending-count">0</span>');
        $('#historico-filters').css('display', 'flex');
        cargarHistorico();
    }
}

// =============================================================================
// API: cargar solicitudes pendientes al iniciar o al recibir un evento nuevo
// =============================================================================
function cargarSolicitudes() {
    $.ajax({
        url: AUTH_SOL_RUTA_API + 'pendientes',
        method: 'GET',
        success: function (data) {
            requests = data;
            renderizarTarjetas();
        },
        error: function (xhr, status, error) {
            if (!handleSessionError(xhr)) {
                console.error('Error al cargar solicitudes:', status, error);
            }
        }
    });
}

function cargarHistorico() {
    const desde = $('#historico-desde').val();
    const hasta = $('#historico-hasta').val();

    $.ajax({
        url: AUTH_SOL_RUTA_API + 'historico?fechaDesde=' + encodeURIComponent(desde)
            + '&fechaHasta=' + encodeURIComponent(hasta),
        method: 'GET',
        success: function (data) {
            requests = data;
            renderizarTarjetas();
        },
        error: function (xhr, status, error) {
            if (!handleSessionError(xhr)) {
                console.error('Error al cargar histórico:', status, error);
            }
        }
    });
}

// =============================================================================
// Render: dibujar las tarjetas en la bandeja de entrada
// =============================================================================
function renderizarTarjetas() {
    const $contenedor = $('#requests-container');
    const cantidadPendientes = requests.length;

    // Actualizar badge con la cantidad
    $('#pending-count').text(cantidadPendientes);

    if (cantidadPendientes === 0) {
        $contenedor.html('<div class="empty-state"><p>No hay solicitudes pendientes 🎉</p></div>');
        return;
    }

    $contenedor.empty();

    $.each(requests, function (i, req) {
        const esBloqueadaPorOtro = req.idUsuarioBloqueo && req.idUsuarioBloqueo !== getCurrentUser();
        const estaSeleccionada = req.id === selectedRequestId;

        // Formatear hora
        const fecha = new Date(req.fechaSolicitud);
        const horaStr = fecha.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

        // Indicador de estado/bloqueo
        let htmlBloqueo = '';
        if (currentTab === 'historico' && req.resolucion) {
            const decision = req.resolucion.decision || 'RESUELTO';
            const usuarioResolucion = req.resolucion.idUsuarioResolucion || 'Sistema';
            const colorDecision = decision.toUpperCase().includes('RECHAZAD')
                ? 'var(--danger-color)'
                : 'var(--success-color)';

            htmlBloqueo = `
                <div class="lock-indicator" style="color: ${colorDecision}">
                    ${decision} por ${usuarioResolucion}
                </div>`;
        } else if (req.idUsuarioBloqueo) {
            if (esBloqueadaPorOtro) {
                htmlBloqueo = `
                    <div class="lock-indicator">
                        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                            <rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect>
                            <path d="M7 11V7a5 5 0 0 1 10 0v4"></path>
                        </svg>
                        Atendido por ${req.idUsuarioBloqueo}
                    </div>`;
            } else {
                htmlBloqueo = `
                    <div class="lock-indicator" style="color: var(--success-color)">
                        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                            <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path>
                            <polyline points="22 4 12 14.01 9 11.01"></polyline>
                        </svg>
                        Tomado por mí
                    </div>`;
            }
        }

        // Construir la tarjeta
        const $tarjeta = $('<div>')
            .addClass('request-card')
            .toggleClass('selected-golden', estaSeleccionada)
            .toggleClass('locked', esBloqueadaPorOtro)
            .attr('data-id', req.id)
            .html(`
                <div class="card-header">
                    <span class="module-badge">${req.codigoModuloOrigen}</span>
                    <span class="time-ago">${horaStr}</span>
                </div>
                <div class="request-title">${req.derechoDescripcion || ('Derecho #' + req.derCodigo)}</div>
                <div class="request-meta">Cajero: ${req.idUsuarioSolicitante}</div>
                ${htmlBloqueo}
            `)
            .on('click', function () {
                seleccionarSolicitud(req);
            });

        $contenedor.append($tarjeta);
    });
}

// =============================================================================
// Seleccionar: el supervisor hace clic en una tarjeta
// =============================================================================
function seleccionarSolicitud(req) {
    // Si estamos en histórico, solo mostramos el detalle (no tomamos el bloqueo)
    if (currentTab === 'historico') {
        selectedRequestId = req.id;
        mostrarDetalle(req);
        return;
    }

    // Si ya está siendo atendida por otro, bloquear
    if (req.idUsuarioBloqueo && req.idUsuarioBloqueo !== getCurrentUser()) {
        alert('Esta solicitud ya está siendo atendida por ' + req.idUsuarioBloqueo + '.');
        return;
    }

    selectedRequestId = req.id;

    // Si el usuario no tiene derechos para esta categoría, solo se muestra el detalle
    if (!req.puedeAutorizar) {
        mostrarDetalle(req);
        return;
    }

    // Si aún no está bloqueada, intentar bloquearla (tomarla)
    if (!req.idUsuarioBloqueo) {
        $.ajax({
            url: AUTH_SOL_RUTA_API + req.id + '/bloqueo',
            method: 'POST',
            headers: {
                'X-CSRF-TOKEN': CSRF_TOKEN
            },
            success: function () {
                req.idUsuarioBloqueo = getCurrentUser();
                req.estado = 'EN_PROCESO';
                renderizarTarjetas();
                mostrarDetalle(req);
            },
            error: function (xhr) {
                if (!handleSessionError(xhr)) {
                    alert('No se pudo tomar la solicitud. Es posible que alguien más la haya tomado justo ahora.');
                    cargarSolicitudes(); // Refrescar para ver el estado real
                }
            }
        });
    } else {
        // Ya la tengo yo: solo mostrar el detalle
        renderizarTarjetas();
        mostrarDetalle(req);
    }
}

// =============================================================================
// Mostrar: panel de detalle y formulario de resolución
// =============================================================================
function mostrarDetalle(req) {
    $('#empty-detail-panel').hide();
    $('#detail-panel').show();

    $('#detail-module').text(req.codigoModuloOrigen);
    $('#detail-id').text(req.id);

    // Básicos
    $('#lbl-tipo').text(req.derechoDescripcion || ('Derecho #' + req.derCodigo));
    $('#lbl-solicitante').text(req.idUsuarioSolicitante);
    $('#lbl-fecha').text(new Date(req.fechaSolicitud).toLocaleString());
    $('#lbl-estado').text(req.estado);

    // Contexto Dinámico
    const $grid = $('#detail-context-grid');
    $grid.empty();

    try {
        let contextoObj = JSON.parse(req.contextoJson);
        $grid.html(generarHtmlContexto(contextoObj));
    } catch (e) {
        $grid.html('<div class="golden-message golden-message-error">Error al leer los detalles específicos.</div>');
    }

    // Resolución
    if (req.resolucion) {
        // Mostrar datos de resolución y ocultar form
        $('#resolution-form').hide();
        $('#title-resolucion').show();
        $('#resolucion-info-grid').show();
        $('#detail-resolucion-grid').show();

        const decisionText = req.resolucion.decision;
        $('#lbl-res-decision').text(decisionText);
        if (decisionText.toUpperCase().includes('RECHAZAD')) {
            $('#lbl-res-decision').css('color', 'var(--danger-color)');
        } else {
            $('#lbl-res-decision').css('color', ''); // Restores default golden from CSS
        }
        $('#lbl-res-usuario').text(req.resolucion.idUsuarioResolucion || 'Sistema');
        $('#lbl-res-fecha').text(new Date(req.resolucion.fechaResolucion).toLocaleString());

        if (req.resolucion.mensaje) {
            $('#detail-resolucion-grid').html(`<div class="context-item"><span class="context-label">Mensaje:</span><span class="context-value">${req.resolucion.mensaje}</span></div>`);
        } else {
            $('#detail-resolucion-grid').html(`<div class="context-item"><span class="context-value" style="opacity:0.6;">Sin observaciones adicionales.</span></div>`);
        }
    } else {
        // Mostrar form de resolución
        $('#title-resolucion').hide();
        $('#resolucion-info-grid').hide();
        $('#detail-resolucion-grid').hide();

        // Si estamos en histórico o no tiene derechos, ocultar igual el form
        if (currentTab === 'historico' || !req.puedeAutorizar) {
            $('#resolution-form').hide();
        } else {
            $('#resolution-form').show();
            $('#resolution-comment').val('');
        }
    }
}

function generarHtmlContexto(obj, level = 0) {
    let html = '';
    for (const key in obj) {
        if (obj.hasOwnProperty(key)) {
            const val = obj[key];
            const label = formatKey(key);

            if (val !== null && typeof val === 'object') {
                html += `<div class="context-group level-${level}">
                            <div class="context-group-title">${label}</div>
                            ${generarHtmlContexto(val, level + 1)}
                         </div>`;
            } else {
                html += `<div class="context-item">
                            <span class="context-label">${label}:</span>
                            <span class="context-value">${val}</span>
                         </div>`;
            }
        }
    }
    return html;
}

function formatKey(key) {
    let result = key.replace(/([A-Z])/g, ' $1');
    return result.charAt(0).toUpperCase() + result.slice(1);
}

// =============================================================================
// Limpiar: volver al estado de "nada seleccionado"
// =============================================================================
function limpiarSeleccion() {
    selectedRequestId = null;
    $('#detail-panel').hide();
    $('#empty-detail-panel').show();
}

// =============================================================================
// Resolver: el supervisor aprueba o rechaza la solicitud
// =============================================================================
function resolverSolicitud(decision) {
    if (!selectedRequestId) return;

    const observacion = $('#resolution-comment').val();

    $.ajax({
        url: AUTH_SOL_RUTA_API + selectedRequestId + '/resolucion',
        method: 'POST',
        contentType: 'application/json',
        headers: {
            'Idempotency-Key': crypto.randomUUID(),
            'X-CSRF-TOKEN': CSRF_TOKEN
        },
        data: JSON.stringify({
            decision: decision, // "APROBADO" o "RECHAZADO"
            codigoResolucion: 'RES-001',
            mensaje: observacion
        }),
        success: function () {
            limpiarSeleccion();
            // SignalR avisará para quitar la tarjeta de la bandeja automáticamente
        },
        error: function (xhr) {
            if (!handleSessionError(xhr)) {
                alert('Error al resolver la solicitud. Código: ' + xhr.status);
            }
        }
    });
}
