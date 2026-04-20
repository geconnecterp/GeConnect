// ═══════════════════════════════════════════════════════════════════
// MÓDULO DE BÚSQUEDA AVANZADA DE PRODUCTOS SIMPLIFICADO - CAJA V2.0
// ═══════════════════════════════════════════════════════════════════


// ═══════════════════════════════════════════════════════════════════
// SECCIÓN 1: INICIALIZACIÓN Y EVENTOS
// ═══════════════════════════════════════════════════════════════════

$(function () {
    console.log('🚀 Módulo de Búsqueda Avanzada Simplificado - Caja v2.0');
    console.log(`   Lista de precios inicial: ${admLp_id}`);

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
 * ✅ COMPLETAMENTE SIMPLIFICADA: Sin filtros de proveedor/rubro/familia
 */
function busquedaAvanzadaProductosV02(pag) {
    console.log('═══════════════════════════════════════════════════');
    console.log('🔍 BÚSQUEDA AVANZADA DE PRODUCTOS - CAJA V2.0');
    console.log(`   Página solicitada: ${pag}`);
    console.log('═══════════════════════════════════════════════════');
    
    // ✅ SIMPLIFICADO: Solo estados y búsqueda de texto
    const act = $("#chkActivos").is(":checked");
    const dis = $("#chkDisc").is(":checked");
    const ina = $("#chkInact").is(":checked");
    const buscar = $("#Search").val() || "";
    
    const data1 = { act, dis, ina, buscar, lp_id: admLp_id };

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
    
    console.log('📡 Enviando petición AJAX:', urlBusqueda);
    console.log('   Datos:', data);
    
    PostGen(data, urlBusqueda, function (response) {
        try { buscarAvUIStop(); } catch (e) { }

        console.log('📦 Respuesta recibida:', response);

        if (response.error) {
            ControlaMensajeError(response.msg || "Error en búsqueda");
            return;
        }

        // ✅ CRÍTICO: Validación y asignación de metadata
        const metadata = response.metadata || {
            totalCount: response.productos ? response.productos.length : 0,
            totalPages: 1,
            currentPage: pag,
            pageSize: response.productos ? response.productos.length : 0
        };

        console.log('📊 Metadata procesada:', metadata);

        // ✅ CRÍTICO: Actualizar variables globales de paginación
        totalRegs = metadata.totalCount;
        pags = metadata.totalPages;
        pagRegs = metadata.pageSize;
        pagina = metadata.currentPage;

        console.log('📄 Variables de paginación actualizadas:');
        console.log(`   - totalRegs: ${totalRegs}`);
        console.log(`   - pags: ${pags}`);
        console.log(`   - pagRegs: ${pagRegs}`);
        console.log(`   - pagina: ${pagina}`);

        // ✅ SIMPLIFICADO: Generar grid sin selección múltiple
        const htmlGrid = generarGridSimplificadoCaja(response.productos, metadata);
        $("#divBusquedaAvanzada").html(htmlGrid);

        // ✅ CRÍTICO: Activar paginación solo si hay resultados
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
        console.error('❌ Error en búsqueda avanzada:', error);
        ControlaMensajeError("Error en búsqueda avanzada: " + (error.message || "Error desconocido"));
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
    const meta = metadata || { totalCount: lista.length, totalPages: 1, currentPage: 1, pageSize: lista.length };

    if (lista.length === 0) {
        return `
            <div class="text-center text-muted py-4">
                <i class="bx bx-info-circle me-2"></i>
                No se encontraron productos con los criterios especificados
            </div>
        `;
    }

    // ✅ SIMPLIFICADO: Solo 6 columnas esenciales
    const filas = lista.map((item, index) => {
        const claseAlternada = index % 2 === 0 ? "table-row-alt" : "";
        const estadoActivo = item.p_activo === "S";
        const estadoBadge = estadoActivo ? "bg-success" : "bg-warning";
        const estadoTexto = estadoActivo ? "Activo" : "Inactivo";

        const precioFormateado = formatearNumeroConCultura(item.p_pvta_001 || 0, 2);

        return `
            <tr class="${claseAlternada} fila-producto-caja" 
                data-producto-id="${item.p_id}"
                ondblclick="seleccionarProductoCaja(this)"
                style="cursor: pointer;">
                
                <td class="text-center fw-bold">${item.p_id}</td>
                <td class="text-left" title="${item.p_desc || ''}">${item.p_desc || ''}</td>
                <td class="text-center">${item.p_id_barrado || ''}</td>
                <td class="text-right fw-semibold text-success">$ ${precioFormateado}</td>
                <td class="text-center">
                    <span class="badge ${estadoBadge}">${estadoTexto}</span>
                </td>
                <td class="text-center">
                    <button type="button" 
                            class="btn btn-success btn-sm"
                            onclick="seleccionarProductoCaja(this.closest('tr'))"
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
                        <th class="text-center">CÓDIGO EAN</th>
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
                Total: ${meta.totalCount} productos encontrados
            </div>
            <div class="text-muted small">
                Página ${meta.currentPage} de ${meta.totalPages}
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