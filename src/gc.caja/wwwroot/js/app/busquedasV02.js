// ═══════════════════════════════════════════════════════════════════
// MÓDULO DE BÚSQUEDA AVANZADA DE PRODUCTOS SIMPLIFICADO - CAJA V2.0
// ═══════════════════════════════════════════════════════════════════


// ═══════════════════════════════════════════════════════════════════
// SECCIÓN 1: INICIALIZACIÓN Y EVENTOS
// ═══════════════════════════════════════════════════════════════════

$(function () {
    console.log('🚀 Módulo de Búsqueda Avanzada Simplificado - Caja v2.0');
    console.log(`   Lista de precios inicial: ${window.obtenerListaPrecioActivaId?.() || ''}`);

    // Botón cerrar modal
    $("button[type='button'].close.buscAdv").on("click", function () {
        $("#busquedaModal").modal("toggle");
    });

    // Botón de búsqueda
    $("#btnBuscarProd").off("click").on("click", function () {
        buscarAvUIStart();
        busquedaAvanzadaProductosV02(pagina);
    });

    // Paginación
    $("#pagEstado").on("change", function () {
        var div = $("#divPaginacionAdv");
        presentaPaginacion(div);
    });
    
    // ✅ NUEVO: Enter en campo de búsqueda
    $("#Search").off("keydown").on("keydown", function (e) {
        if (e.key === "Enter") {
            e.preventDefault();
            const valor = $(this).val().trim();
            if (valor) {
                $("#btnBuscarProd").trigger("click");
            }
        }
    });

    // ✅ CRÍTICO: Limpiar al mostrar/ocultar modal
    $("#busquedaModal").on("show.bs.modal", function () {
        console.log("📂 Modal de búsqueda abierto");
        inicializarControlesBusquedaAvanzada();
    });

    $("#busquedaModal").on('shown.bs.modal', function () {
        setTimeout(function () {
            $('#Search').trigger('focus');
        }, 150); // Un pequeño retraso para asegurar que el modal esté completamente visible
    });

    $("#busquedaModal").on("hidden.bs.modal", function () {
        console.log("📕 Modal de búsqueda cerrado");
        inicializarControlesBusquedaAvanzada();
    });

    // ✅ CRÍTICO: Callback para paginación
    funcCallBack = busquedaAvanzadaProductosV02;

    return true;
});

// ═══════════════════════════════════════════════════════════════════
// SECCIÓN 2: BÚSQUEDA PRINCIPAL
// ═══════════════════════════════════════════════════════════════════

/**
 * ✅ ACTUALIZADO v3.0: Búsqueda avanzada con detección de sesión expirada
 */
function busquedaAvanzadaProductosV02(pag) {
    console.log('🔍 BÚSQUEDA AVANZADA DE PRODUCTOS - CAJA V3.0');
    
    // ✅ Parámetros con valores por defecto correctos
    const ri01 = $("#Rel01B2Item").val() || "";
    const ri02 = $("#Rel02B2Item").val() || $("#Rel02B2").val() || "";
    const ri03 = $("#Rel03B2 option:selected").val() || "%";
    const act = $("#chkActivos").val();
    const dis = $("#chkDisc").val();
    const ina = $("#chkInact").val();
    
    let cstk = true;
    let sstk = false;

    const buscar = $("#Search").val() || "";
    
    // ✅ CRÍTICO: Incluir lp_id
    const data1 = { 
        ri01,
        ri02,
        ri03,
        act, 
        dis, 
        ina, 
        cstk, 
        sstk, 
        buscar, 
        lp_id: window.obtenerListaPrecioActivaId?.() || ''
    };

    const buscaNew = JSON.stringify(dataBakV02) !== JSON.stringify(data1);
    
    if (!buscaNew) {
        pagina = pag;
    } else {
        dataBakV02 = data1;
        pagina = 1;
        pag = 1;
    }

    const sort = "p_desc";
    const sortDir = "asc";
    const data2 = { sort, sortDir, pag, buscaNew };
    const data = $.extend({}, data1, data2);

    const urlBusqueda = busquedaAvanzadaUrl;
    
    console.log('📡 Enviando petición a:', urlBusqueda);
    console.log('📦 Datos enviados:', data);
    
    PostGen(data, urlBusqueda, function (response) {
        try { buscarAvUIStop(); } catch (e) { }

        console.log('📦 Respuesta recibida:', response);

        // ═══════════════════════════════════════════════════════════════════
        // ✅ NUEVO: DETECCIÓN DE SESIÓN EXPIRADA
        // ═══════════════════════════════════════════════════════════════════
        if (response.error) {
            console.log('═══════════════════════════════════════════════════');
            console.log('⚠️ RESPUESTA CON ERROR DETECTADA');
            console.log('═══════════════════════════════════════════════════');
            console.log('   response.error:', response.error);
            console.log('   response.msg:', response.msg);
            console.log('   response.redirect:', response.redirect);
            console.log('   response.redirectUrl:', response.redirectUrl);
            
            // ❶ CRÍTICO: Detectar sesión expirada mediante validarRespuestaSesion()
            if (!validarRespuestaSesion(response)) {
                console.log('🚪 Sesión expirada detectada - Redirigiendo...');
                return; // validarRespuestaSesion() ya maneja la redirección
            }
            
            // ❷ ALTERNATIVO: Detección explícita de redirect
            if (response.redirect && response.redirectUrl) {
                console.log('🚪 Redirección solicitada por el servidor');
                console.log(`   URL destino: ${response.redirectUrl}`);
                
                manejarSesionExpirada(response.msg);
                return;
            }
            
            // ❸ Si es otro tipo de error, mostrar mensaje
            console.log('❌ Error de búsqueda (no es sesión expirada)');
            ControlaMensajeError(response.msg || "Error en búsqueda");
            
            console.log('═══════════════════════════════════════════════════');
            return;
        }

        // ✅ Procesamiento normal de resultados exitosos
        const metadata = response.metadata || {
            totalCount: response.productos ? response.productos.length : 0,
            totalPages: 1,
            currentPage: pag,
            pageSize: response.productos ? response.productos.length : 0
        };

        console.log('📊 Metadata procesada:', metadata);

        totalRegs = metadata.totalCount;
        pags = metadata.totalPages;
        pagRegs = metadata.pageSize;
        pagina = metadata.currentPage;

        console.log('📄 Variables de paginación actualizadas:');
        console.log(`   - totalRegs: ${totalRegs}`);
        console.log(`   - pags: ${pags}`);
        console.log(`   - pagRegs: ${pagRegs}`);
        console.log(`   - pagina: ${pagina}`);

        const htmlGrid = generarGridSimplificadoCaja(response.productos, metadata);
        $("#divBusquedaAvanzada").html(htmlGrid);

        if (metadata.totalCount > 0) {
            $("#pagEstado").val(true).trigger("change");
        } else {
            $("#pagEstado").val(false);
            $("#divPaginacionAdv").empty();
        }

        console.log('✅ Búsqueda completada exitosamente');
        console.log('═══════════════════════════════════════════════════');
    }, function (error) {
        try { buscarAvUIStop(); } catch (e) { }
        
        console.error('═══════════════════════════════════════════════════');
        console.error('❌ ERROR EN CALLBACK DE ERROR AJAX');
        console.error('═══════════════════════════════════════════════════');
        console.error('   Error completo:', error);
        
        // ✅ CRÍTICO: El interceptor global de siteGen.js ya maneja sesiones expiradas
        // Si llegamos aquí y no es sesión expirada, mostrar error genérico
        if (error && !esSesionExpirada(error.status)) {
            console.error('❌ Error de comunicación (no es sesión expirada)');
            ControlaMensajeError("Error en búsqueda avanzada: " + (error.message || "Error desconocido"));
        }
        
        console.error('═══════════════════════════════════════════════════');
    });

    return true;
}

// ═══════════════════════════════════════════════════════════════════
// SECCIÓN 3: GENERACIÓN DE GRID SIMPLIFICADO
// ═══════════════════════════════════════════════════════════════════

/**
 * ✅ NUEVA: Genera grid simplificado para Caja (sin checkboxes)
 */
function generarGridSimplificadoCaja(productos, metadata) {
    const lista = Array.isArray(productos) ? productos : [];

    if (lista.length === 0) {
        return `<div class="text-center text-muted py-4">
            <i class="bx bx-info-circle me-2"></i>
            No se encontraron productos con los criterios especificados
        </div>`;
    }

    const filas = lista.map((item, index) => {
        const claseAlternada = index % 2 === 0 ? "table-row-alt" : "";
        const estadoActivo = item.p_activo === "S";
        const estadoBadge = estadoActivo ? "bg-success" : "bg-warning";
        const estadoTexto = estadoActivo ? "Activo" : "Inactivo";

        const precioFormateado = formatearNumeroConCultura(item.p_pvta_001 || 0, 2);

        // ✅ CRÍTICO: Manejar código de barras vacío o null
        const codigoBarras = item.p_id_barrado || '-';

        return `
            <tr class="${claseAlternada} fila-producto-caja" 
                data-producto-id="${item.p_id}"
                ondblclick="selectRegDbl(this)"
                style="cursor: pointer;">
                
                <td class="text-center fw-bold">${item.p_id}</td>                          <!-- [1] ID -->
                <td class="text-left" title="${item.p_desc || ''}">${item.p_desc || ''}</td> <!-- [2] DESC -->
                <td class="text-center" style="user-select: none;">${codigoBarras}</td> <!-- [3] EAN ⬅️ CRÍTICO -->
                <td class="text-right fw-semibold text-success">$ ${precioFormateado}</td> <!-- [4] PRECIO -->
                <td class="text-center">
                    <span class="badge ${estadoBadge}">${estadoTexto}</span>
                </td>
                <td class="text-center">
                    <button type="button" 
                            class="btn btn-success btn-sm"
                            onclick="selectRegDbl(this.closest('tr'))"
                            title="Seleccionar este producto">
                        <i class="bx bx-check-circle"></i> Seleccionar
                    </button>
                </td>
            </tr>
        `;
    }).join("");

    return `
        <div class="table-responsive text-nowrap table-wrapper-400">
            <table class="table table-sm mb-0 table-hover table-golden" id="tbGridBusquedaProductos">
                <thead class="table-golden-header">
                    <tr class="header">
                        <th class="text-center">ID</th>
                        <th class="text-left">DESCRIPCIÓN</th>
                        <th class="text-center">CÓDIGO EAN</th> <!-- ⬅️ IMPORTANTE -->
                        <th class="text-right">PRECIO</th>
                        <th class="text-center">ESTADO</th>
                        <th class="text-center">ACCIÓN</th>
                    </tr>
                </thead>
                <tbody>${filas}</tbody>
            </table>
        </div>
        <div class="d-flex justify-content-between align-items-center mt-3 px-2">
            <div class="text-muted small">
                <i class="bx bx-package me-1"></i>
                Total: ${metadata.totalCount} productos encontrados
            </div>
            <div class="text-muted small">
                Página ${metadata.currentPage} de ${metadata.totalPages}
            </div>
        </div>
    `;
}

// ═══════════════════════════════════════════════════════════════════
// SECCIÓN 4: SELECCIÓN DE PRODUCTO
// ═══════════════════════════════════════════════════════════════════

/**
 * ✅ NUEVA: Selecciona UN producto y lo envía a facturación
 */
function seleccionarProductoCaja(fila) {
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ PRODUCTO SELECCIONADO EN BÚSQUEDA AVANZADA');
    console.log('═══════════════════════════════════════════════════');
    
    // Limpiar selección visual previa
    $("#tbGridBusquedaProductos tbody tr").removeClass("selected-row");
    
    // Marcar fila actual
    $(fila).addClass("selected-row");
    
    // Obtener ID del producto
    const id = $(fila).find("td:first").text().trim();
    
    console.log(`   Producto seleccionado: ${id}`);
    console.log('═══════════════════════════════════════════════════');
    
    // Cerrar modal
    $("#busquedaModal").modal("hide");
    
    // ✅ CRÍTICO: Enviar a búsqueda base para agregar a factura
    $("#Busqueda").val(id);
    $("#btnBusquedaBase").trigger("click");
}

/**
 * ✅ ACTUALIZADO v4.0: Agrega producto automáticamente si NO tiene código de barras
 * Si tiene código de barras, lo coloca en el campo para confirmación manual
 * 
 * @param {HTMLTableRowElement} x - Fila de la tabla seleccionada
 */
function selectRegDbl(x) {
    console.log('═══════════════════════════════════════════════════');
    console.log('✅ PRODUCTO SELECCIONADO VIA BÚSQUEDA AVANZADA v4.0');
    console.log('═══════════════════════════════════════════════════');

    // ❶ Limpiar selección visual previa
    $("#tbGridBusquedaProductos tbody tr").removeClass("selected-row");

    // ❷ Seleccionar fila actual
    $(x).addClass("selected-row");

    // ❸ Obtener datos del producto
    const idProducto = x.cells[0].innerText.trim();     // Columna [0]: ID
    const descripcion = x.cells[1].innerText.trim();    // Columna [1]: DESCRIPCIÓN
    const codigoBarras = x.cells[2].innerText.trim();   // Columna [2]: CÓDIGO EAN

    console.log(`   Producto seleccionado:`);
    console.log(`   - ID: ${idProducto}`);
    console.log(`   - Descripción: ${descripcion}`);
    console.log(`   - Código de Barras: "${codigoBarras}"`);

    // ❹ CRÍTICO: Detectar si tiene código de barras válido
    const tieneCodigoBarras = codigoBarras &&
        codigoBarras !== '' &&
        codigoBarras !== '-' &&
        codigoBarras !== 'N/A' &&
        codigoBarras.toLowerCase() !== 'sin código';

    console.log(`   ✅ Tiene código de barras válido: ${tieneCodigoBarras ? 'SÍ' : 'NO'}`);

    // ❺ Cerrar modal de búsqueda avanzada
    $("#busquedaModal").modal("hide");
    console.log('   → Modal cerrado');

    // ═══════════════════════════════════════════════════════════════════
    // ❻ LÓGICA DIFERENCIAL SEGÚN CÓDIGO DE BARRAS
    // ═══════════════════════════════════════════════════════════════════

    if (!tieneCodigoBarras) {
        // ✅ CASO 1: SIN CÓDIGO DE BARRAS → Agregar automáticamente
        console.log('═══════════════════════════════════════════════════');
        console.log('🚀 MODO AUTOMÁTICO: Producto SIN código de barras');
        console.log('   → Agregando directamente a la grilla...');
        console.log('═══════════════════════════════════════════════════');

        // ❼ CRÍTICO: Marcar origen como 'busquedaAvanzada' (para logs)
        // Esta variable debe estar definida en prodfact.js
        if (typeof origenCargaActual !== 'undefined') {
            origenCargaActual = 'busquedaAvanzada';
        }

        // ❽ Colocar código en el input (para referencia visual)
        setTimeout(() => {
            $("#txtCodigoProducto").val(idProducto);

            // ❾ CRÍTICO: Disparar búsqueda automática
            // Llamar a la función que procesa la entrada de código
            if (typeof procesarEntradaCodigo === 'function') {
                procesarEntradaCodigo();
            } else {
                console.error('❌ Función procesarEntradaCodigo no encontrada');
                // Fallback: disparar Enter manualmente
                $("#txtCodigoProducto").trigger($.Event('keypress', { which: 13 }));
            }

            console.log('✅ Búsqueda automática disparada');
            console.log('   → El producto se agregará a la grilla automáticamente');
        }, 300); // Delay para asegurar que el modal se cerró

    } else {
        // ⚠️ CASO 2: CON CÓDIGO DE BARRAS → Colocar en campo (sin agregar)
        console.log('═══════════════════════════════════════════════════');
        console.log('⚠️ MODO MANUAL: Producto CON código de barras');
        console.log('   → Código colocado en campo para confirmación');
        console.log('   → Operador debe presionar ENTER o BUSCAR');
        console.log('═══════════════════════════════════════════════════');

        setTimeout(() => {
            $("#txtCodigoProducto").val(idProducto);
            $("#txtCodigoProducto").trigger("focus").trigger("select");
            console.log(`   → Código "${idProducto}" listo para confirmar`);
        }, 300);
    }

    console.log('═══════════════════════════════════════════════════');
}

// ═══════════════════════════════════════════════════════════════════
// SECCIÓN 5: UTILIDADES
// ═══════════════════════════════════════════════════════════════════

/**
 * ✅ SIMPLIFICADA: Inicializa controles sin filtros complejos
 */
function inicializarControlesBusquedaAvanzada() {
    console.log("🔄 Inicializando controles del modal de búsqueda avanzada...");

    $("#Search").val("").prop("disabled", false);

    $("#chkActivos").prop("checked", true).prop("disabled", false);
    $("#chkDisc, #chkInact").prop("checked", false).prop("disabled", false);

    $("#divBusquedaAvanzada").html(`
        <div class="text-center text-muted py-4">
            <i class="bx bx-info-circle me-2"></i>
            Ingrese un criterio de búsqueda y presione el botón Buscar
        </div>
    `);

    // ✅ CRÍTICO: Resetear paginación
    $("#pagEstado").val(false);
    $("#divPaginacionAdv").empty();
    
    // ✅ CRÍTICO: Resetear variables de paginación
    totalRegs = 0;
    pags = 1;
    pagRegs = 0;
    pagina = 1;

    try { buscarAvUIStop(); } catch (e) { }

    console.log("✅ Controles inicializados correctamente.");
}

// Helpers UI de búsqueda avanzada (spinner y botón)
function buscarAvUIStart() {
    $("#btnBuscarProd").prop("disabled", true);
    $("#spnBuscarProd").removeClass("d-none");
}

function buscarAvUIStop() {
    $("#spnBuscarProd").addClass("d-none");
    $("#btnBuscarProd").prop("disabled", false);
}

// ✅ MANTENER: Función de formateo
function formatearNumeroConCultura(numero, decimales = 2) {
    if (numero === null || numero === undefined || isNaN(numero)) {
        return "0" + ",".repeat(decimales > 0 ? 1 : 0) + "0".repeat(decimales);
    }
    
    const num = parseFloat(numero);
    return num.toLocaleString('es-AR', {
        minimumFractionDigits: decimales,
        maximumFractionDigits: decimales,
        useGrouping: true
    });
}

// ═══════════════════════════════════════════════════════════════════
// FIN DEL MÓDULO
// ═══════════════════════════════════════════════════════════════════
