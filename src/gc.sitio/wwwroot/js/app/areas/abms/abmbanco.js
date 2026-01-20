$(function () {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacion(div);
	});

	$(document).on("dblclick", "#" + Grids.GridBanco + " tbody tr", function () {
		x = $(this);
		ejecutaDblClickGrid(x, Grids.GridBanco);
	});

	$("#btnAbmNuevo").on("click", function () { btnNuevoClick(); });
	$("#btnAbmModif").on("click", function () { btnModiClick(); });
	$("#btnAbmElimi").on("click", function () { btnBajaClick(); });
	$("#btnAbmAceptar").on("click", function () { btnSubmitClick(); });
	$("#btnAbmCancelar").on("click", function () { btnCancelClick(); });

	$("#btnDetalle").on("mousedown", analizaEstadoBtnDetalle);
	$("#btnDetalle").prop("disabled", true);
	$("#btnAbmAceptar").hide();
	$("#btnAbmCancelar").hide();
	//$("#btnCancel").on("click", function () {
	//	$("#btnFiltro").trigger("click");
	//});
	$("#btnCancel").on("click", function () {
		OcultarDivs(true);
		$("#divFiltro").collapse('show');
		//InicializaPantallaAbmBancos();
	});
	$("#btnBuscar").on("click", function () {
		dataBak = "";
		pagina = 1;
		buscarBancos(pagina);
	});

	InicializaPantallaAbmBancos();
	funcCallBack = buscarBancos;
	return true;
});

function OcultarDivs(valor) {
	$("#divDetalle").collapse('hide');
	$("#divGrilla").collapse('hide');
	$("#divPaginacion").collapse('hide');
}

function InicializaPantallaAbmBancos() {
	var tb = $("#tbGridBanco tbody tr");
	if (tb.length === 0) {
		$("#divFiltro").collapse("show")
	}

	$("#divDetalle").collapse("hide");
	$("#lbChkDescr").text("Denominación");
	$("#lbDescr").html("Desc");

	$("#lbChkDesdeHasta").text("ID Cuenta");

	$("#IdSelected").val("");
	$(".activable").prop("disabled", true);
	activarBotones(false);
	CerrarWaiting();
	return true;
}

function analizaEstadoBtnDetalle() {
	var res = $("#divDetalle").hasClass("show");
	if (res === true) {
		selectRegCli(regSelected, Grids.GridBanco);
		activarGrilla(Grids.GridBanco);
	}
	return true;

}

function selectReg(x, gridId) {
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
	if (gridId == Grids.GridBanco) {
		$("#btnDetalle").prop("disabled", true);
		$("#divDetalle").collapse("hide");
	}
}

function selectRegDbl(x, gridId) {
	AbrirWaiting("Espere mientras se busca la información solicitada...");
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selectedEdit-row");
	regSelected = x;

	switch (gridId) {
		case Grids.GridBanco:
			var ctaf_id = x[0].cells[0].innerText.trim();
			if (ctaf_id !== "") {
				ctafId = ctaf_id;
				BuscarBanco(ctaf_id);
				activarBotones(true);
				$(".activable").prop("disabled", true);
				$("#btnDetalle").prop("disabled", false);
				$("#divFiltro").collapse("hide");
				$("#divDetalle").collapse("show");
				$("#IdSelected").val(ctafId);
				posicionarRegOnTop(x);
				desactivarGrilla('tbGridBanco');
				setTimeout(function () {
					// Inicializar el selector de cuentas
					inicializarSelectorCuentas();
				}, 1000);
			}
			break;
		default:
	}
}

function NuevoBanco() {
	var data = {};
	PostGenHtml(data, nuevoBancoUrl, function (obj) {
		$("#divDatosBanco").html(obj);
		$(".nav-link").prop("disabled", true);
		$(".activable").prop("disabled", false);
		$("#Banco_Ctaf_Id").prop("disabled", false);
		desactivarGrilla(Grids.GridBanco);
		accionBotones(AbmAction.ALTA, Tabs.TabBanco);
		$("#divFiltro").collapse("hide");
		$("#divDetalle").collapse("show");
		setTimeout(function () {
			// Inicializar el selector de cuentas
			inicializarSelectorCuentas();
		}, 500);
		$("#Banco_Ban_Razon_Social").focus();
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function ModificaBanco(tabAct, mainGrid) {
	var mensaje = PuedoModificar(tabAct);
	if (mensaje !== "") {
		AbrirMensaje("ATENCIÓN", mensaje, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		accionBotones(AbmAction.MODIFICACION, Tabs.TabBanco);
		tipoDeOperacion = AbmAction.MODIFICACION;
		SetearDestinoDeOperacion(tabAct);
		$(".nav-link").prop("disabled", true);
		$(".activable").prop("disabled", false);
		desactivarGrilla(mainGrid);
		$("#Banco_Ban_Razon_Social").focus();
	}
}

function cargaPaginacion() {
	$("#divPaginacion").pagination({
		items: totalRegs,
		itemsOnPage: pagRegs,
		cssStyle: "dark-theme",
		currentPage: pagina,
		onPageClick: function (num) {
			buscarBancos(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltro").collapse("hide")
	return true;
}

function ObtenerDatosDeBancoParaJson(destinoDeOperacion, tipoDeOperacion) {
	var ctaf_id = $("#Banco_Ctaf_Id").val();
	var ban_razon_social = $("#Banco_Ban_Razon_Social").val();
	var ban_cuit = $("#Banco_Ban_Cuit").val();
	var tcb_id = $("#listaTcb").val();
	var tcb_desc = $("#listaTcb option:selected").text();
	var ban_cuenta_nro = $("#Banco_Ban_Cuenta_Nro").val();
	var ban_cuenta_cbu = $("#Banco_Ban_Cuenta_Cbu").val();
	var mon_codigo = $("#listaMoneda").val();
	var mon_desc = $("#listaMoneda option:selected").text();
	var ban_che_nro = $("#Banco_Ban_Che_Nro").val();
	var ban_che_desde = $("#Banco_Ban_Che_Desde").val();
	var ban_che_hasta = $("#Banco_Ban_Che_Hasta").val();
	var ccb_id = $("#cuentaContableId").val();
	var ccb_desc = limpiarDescripcion($("#cuentaContable").val());
	var ccb_id_diferido = $("#cuentaContableDifId").val();
	var ccb_desc_diferido = limpiarDescripcion($("#cuentaContableDif").val());
	var ctag_id = $("#listaCtaGas").val();
	var ctag_denominacion = $("#listaCtaGas option:selected").text();
	var data = {
		ctaf_id, ban_razon_social, ban_cuit, tcb_id, tcb_desc, ban_cuenta_nro, ban_cuenta_cbu, mon_codigo, mon_desc, ban_che_nro, ban_che_desde,
		ban_che_hasta, ccb_id, ccb_desc, ccb_id_diferido, ccb_desc_diferido, ctag_id, ctag_denominacion, destinoDeOperacion, tipoDeOperacion
	}
	return data;
}

function limpiarDescripcion(desc) {
	return desc.replace(/^\(\d+\)\s*/, "");
}


function buscarBancos(pag, esBaja = false) {
	AbrirWaiting();
	var buscar = "";
	var id = "";
	var id2 = "";
	var r01 = [];
	var r02 = [];

	if ($("#chkDescr").is(":checked")) {
		buscar = $("#Buscar").val();
	}
	if ($("#chkDesdeHasta").is(":checked")) {
		id = $("#Id").val();
		id2 = $("#Id2").val();
	}
	if ($("#chkRel01").is(":checked")) {
		$("#Rel01List").children().each(function (i, item) { r01.push($(item).val()) });
	}
	if ($("#chkRel02").is(":checked")) {
		$("#Rel02List").children().each(function (i, item) { r02.push($(item).val()) });
	}

	var data1 = {
		id, id2,
		rel01: r01,
		rel02: r02,
		rel03: [],
		"fechaD": null, //"0001-01-01T00:00:00",
		"fechaH": null, //"0001-01-01T00:00:00",
		buscar
	};

	var buscaNew = JSON.stringify(dataBak) != JSON.stringify(data1)
	if (esBaja)
		buscaNew = true;

	if (buscaNew === false) {
		//son iguales las condiciones cambia de pagina
		pagina = pag;
	}
	else {
		dataBak = data1;
		pagina = 1;
		pag = 1;
	}

	var sort = null;
	var sortDir = null

	var data2 = { sort, sortDir, pag, buscaNew }

	var data = $.extend({}, data1, data2);

	PostGenHtml(data, buscarUrl, function (obj) {
		$("#divGrilla").html(obj);
		$("#divFiltro").collapse("hide");
		PostGen({}, buscarMetadataURL, function (obj) {
			if (obj.error === true) {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				totalRegs = obj.metadata.totalCount;
				pags = obj.metadata.totalPages;
				pagRegs = obj.metadata.pageSize;

				$("#pagEstado").val(true).trigger("change");
				$(".activable").prop("disabled", true);
			}

		});
		$("#divGrilla").collapse("show");
		$("#divPaginacion").collapse("show");
		CerrarWaiting();
		// Inicializar el selector de cuentas
		//inicializarSelectorCuentas();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});

}

function BuscarBanco(ctafId) {
	var data = { ctafId };
	AbrirWaiting();
	PostGenHtml(data, buscarBancoUrl, function (obj) {
		$("#divDatosBanco").html(obj);
		$("#IdSelected").val($("#Banco_Ctaf_Id").val());
		let ccb_desc = "";
		let id = $("#Banco_Ccb_Id").val();
		let nombre = $("#Banco_Ccb_Desc").val();
		if (id != undefined && id != "") {
			ccb_desc = `(${id}) ${nombre}`;
			$("#cuentaContable").val(ccb_desc);
			$("#cuentaContableId").val(id);
		}
		id = $("#Banco_Ccb_Id_Diferido").val();
		nombre = $("#Banco_Ccb_Desc_Diferido").val();
		if (id != undefined && id != "") {
			ccb_desc = `(${id}) ${nombre}`;
			$("#cuentaContableDif").val(ccb_desc);
			$("#cuentaContableDifId").val(id);
		}
		$(".activable").prop("disabled", true);
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

// Variables globales para el selector de cuentas
let cuentaSeleccionada = null;
let arbolCuentasInicializado = false;

/**
* Modifica el selector de cuentas para implementar la búsqueda en tiempo real
*/
function inicializarSelectorCuentas() {
	$("input#cuentaContable").off('click').on("click", function () {
		$("input#cuentaContable").val("");
		$("input#cuentaContableId").val("");
	});
	$("input#cuentaContableDif").off('click').on("click", function () {
		$("input#cuentaContableDif").val("");
		$("input#cuentaContableDifId").val("");
	});
	// Configurar evento para abrir el selector al hacer clic en el botón
	$('.btnBuscarCuenta').off('click').on('click', function () {
		// Tomar los destinos desde los data-attributes
		const campo = $(this).data("target");
		const campoId = $(this).data("target-id");

		// Guardar referencias para los campos destino
		$('#selectorPlanCuentasModal').data('campo-destino', campo);
		$('#selectorPlanCuentasModal').data('campo-destino-id', campoId);

		// Abrir el modal
		$('#selectorPlanCuentasModal').modal('show');

		let tree = $('#cuentasTree').jstree(true);
		let tieneNodos = false;
		if (tree && tree.get_json('#', { flat: true }).length > 0) {
			tieneNodos = true;
		}

		// Cargar el árbol si no está inicializado
		if (!arbolCuentasInicializado || !tieneNodos) {
			cargarArbolCuentas();
		}
	});

	// NUEVA IMPLEMENTACIÓN: Búsqueda en tiempo real al escribir
	$('#txtBuscarCuentaPlan').off('keyup').on('keyup', function () {
		const termino = $(this).val().trim();

		// Obtener instancia del árbol
		const tree = $("#cuentasTree").jstree(true);
		if (!tree) return;

		if (termino.length > 0) {
			// Si hay texto, realizar la búsqueda
			tree.search(termino, false, true);

			// Usar setTimeout para dar tiempo a jsTree a actualizar el DOM
			setTimeout(function () {
				// Contar los resultados usando jQuery
				const nodosEncontrados = $('.jstree-search');
				const totalResultados = nodosEncontrados.length;

				// Expandir los nodos padre de los resultados
				nodosEncontrados.each(function () {
					const nodeId = $(this).closest('.jstree-node').attr('id');
					if (nodeId) {
						// Obtener y expandir todos los nodos padres
						let parent = tree.get_parent(nodeId);
						while (parent && parent !== "#") {
							tree.open_node(parent);
							parent = tree.get_parent(parent);
						}
					}
				});

				// Mostrar mensaje con cantidad de resultados
				if (totalResultados > 0) {
					$("#resultadosBusqueda").html(`
                    <div class="alert alert-success py-1 small">
                        <i class="bx bx-check-circle me-1"></i>
                        Se encontraron <strong>${totalResultados}</strong> cuenta(s) que coinciden
                    </div>
                `).show();
				} else {
					$("#resultadosBusqueda").html(`
                    <div class="alert alert-warning py-1 small">
                        <i class="bx bx-error-circle me-1"></i>
                        No se encontraron cuentas que coincidan
                    </div>
                `).show();
				}

				// Ocultar después de 3 segundos
				setTimeout(function () {
					$("#resultadosBusqueda").fadeOut();
				}, 3000);
			}, 200); // Pequeño retraso para que jsTree termine de actualizar el DOM
		} else {
			// Si el campo está vacío, limpiar la búsqueda
			tree.clear_search();
			tree.close_all();
			$("#resultadosBusqueda").fadeOut();
		}
	});


	// Búsqueda al presionar Enter (para evitar envío de formulario)
	$('#txtBuscarCuentaPlan').off('keypress').on('keypress', function (e) {
		if (e.which === 13) {
			e.preventDefault(); // Evitar envío de formulario
			// La búsqueda ya se habrá hecho con el evento keyup
		}
	});

	// Evento para seleccionar cuenta
	$('#btnSeleccionarCuenta').off('click').on('click', function () {
		if (cuentaSeleccionada) {
			// Obtener los campos destino desde el modal
			const campoDestino = $('#selectorPlanCuentasModal').data('campo-destino');
			const campoDestinoId = $('#selectorPlanCuentasModal').data('campo-destino-id');

			// Actualizar los campos con la cuenta seleccionada
			$('#' + campoDestino).val(cuentaSeleccionada.text);
			$('#' + campoDestinoId).val(cuentaSeleccionada.id);

			// Cerrar el modal
			$('#selectorPlanCuentasModal').modal('hide');
		}
	});

	// Limpiar búsqueda y selección al abrir el modal
	$('#selectorPlanCuentasModal').off('shown.bs.modal').on('shown.bs.modal', function () {
		// Limpiar campo de búsqueda y darle el foco
		$('#txtBuscarCuentaPlan').val('').trigger("focus");

		// Limpiar búsqueda previa
		const tree = $("#cuentasTree").jstree(true);
		if (tree) {
			tree.clear_search();
			tree.close_all();
		}

		// Resetear selección
		cuentaSeleccionada = null;
		$('#btnSeleccionarCuenta').prop('disabled', true);
		$("#resultadosBusqueda").hide();
	});

	// Limpiar búsqueda y selección al cerrar el modal
	$('#selectorPlanCuentasModal').off('hidden.bs.modal').on('hidden.bs.modal', function () {
		$('#txtBuscarCuentaPlan').val('');
		cuentaSeleccionada = null;
		$('#btnSeleccionarCuenta').prop('disabled', true);

		// Devolver el foco al botón que abrió el modal (para accesibilidad)
		$('#btnBuscarCuenta').trigger("focus");
	});
}

/**
* Carga el árbol de cuentas desde el servidor
*/
function cargarArbolCuentas() {
	// Mostrar indicador de carga en el árbol
	$("#cuentasTree").html(`
        <div class="text-center p-3">
            <div class="spinner-border spinner-border-sm text-warning" role="status">
                <span class="visually-hidden">Cargando...</span>
            </div>
            <p class="mt-2 small">Cargando plan de cuentas...</p>
        </div>
    `);

	AbrirWaiting("Cargando plan de cuentas...");

	const data = {
		buscar: "",
		buscaNew: true
	};

	// Verificar que la URL esté configurada
	if (!buscarPlanCuentasUrl) {
		console.error("La URL para buscar el plan de cuentas no está configurada");
		AbrirMensaje(
			"Error",
			"No se pudo cargar el plan de cuentas. La URL no está configurada.",
			function () { $("#msjModal").modal("hide"); },
			false,
			["Aceptar"],
			"error!",
			null
		);
		CerrarWaiting();
		return;
	}

	// Realizar la petición AJAX
	$.ajax({
		url: buscarPlanCuentasUrl,
		type: "POST",
		contentType: "application/json",
		data: JSON.stringify(data),
		success: function (resultado) {
			CerrarWaiting();

			if (resultado.error) {
				console.error("Error al cargar el plan de cuentas:", resultado.msg);
				AbrirMensaje(
					"Error",
					"Error al cargar el plan de cuentas: " + resultado.msg,
					function () { $("#msjModal").modal("hide"); },
					false,
					["Aceptar"],
					"error!",
					null
				);
				return;
			}

			try {
				// Parsear el árbol
				const arbolCuentas = JSON.parse(resultado.arbol);

				// Procesar los nodos para añadir íconos y clases
				procesarNodosArbol(arbolCuentas);

				// Inicializar jsTree
				inicializarJsTree(arbolCuentas);

				arbolCuentasInicializado = true;
			} catch (error) {
				console.error("Error al procesar los datos del plan de cuentas:", error);
				AbrirMensaje(
					"Error",
					"Error al procesar los datos del plan de cuentas",
					function () { $("#msjModal").modal("hide"); },
					false,
					["Aceptar"],
					"error!",
					null
				);
			}
		},
		error: function (xhr, status, error) {
			CerrarWaiting();
			console.error("Error al cargar el plan de cuentas:", error);
			AbrirMensaje(
				"Error",
				"Error de comunicación al cargar el plan de cuentas",
				function () { $("#msjModal").modal("hide"); },
				false,
				["Aceptar"],
				"error!",
				null
			);
		}
	});
}

/**
* Procesa los nodos del árbol para añadir íconos y clases
* @param {Array} nodos - Lista de nodos del árbol
*/
function procesarNodosArbol(nodos) {
	nodos.forEach(nodo => {
		// Determinar tipo de cuenta para el ícono
		const tipo = nodo.data?.tipo;
		const cuentaTipo = nodo.data?.cuenta?.toLowerCase();

		// Asignar tipo para íconos
		nodo.type = cuentaTipo || "default";

		// Asignar clases CSS
		nodo.a_attr = nodo.a_attr || {};
		let clases = [];

		if (tipo === "M") clases.push("tipo-movimiento");
		if (cuentaTipo) clases.push("cuenta-" + cuentaTipo);

		nodo.a_attr.class = clases.join(" ");

		// Procesar nodos hijos recursivamente
		if (nodo.children && nodo.children.length > 0) {
			procesarNodosArbol(nodo.children);
		}
	});
}

/**
 * Inicializa el árbol jsTree con los datos procesados y configura la búsqueda
 * @param {Array} datos - Datos del árbol
 */
function inicializarJsTree(datos) {
	// Destruir instancia previa si existe
	if ($.jstree.reference("#cuentasTree")) {
		$("#cuentasTree").jstree("destroy");
	}

	// Inicializar nueva instancia con soporte para búsqueda
	$("#cuentasTree").jstree({
		core: {
			data: datos,
			themes: {
				responsive: true
			}
		},
		types: {
			activo: {
				icon: "bx bx-wallet"
			},
			pasivo: {
				icon: "bx bx-trending-down"
			},
			patrimonio: {
				icon: "bx bx-building-house"
			},
			ingresos: {
				icon: "bx bx-dollar-circle"
			},
			egresos: {
				icon: "bx bx-money-withdraw"
			},
			default: {
				icon: "bx bx-folder"
			}
		},
		search: {
			show_only_matches: true,
			show_only_matches_children: true,
			close_opened_onclear: true,
			search_leaves_only: false
		},
		plugins: ["types", "search"]
	});

	// Evento al seleccionar un nodo
	$("#cuentasTree").off('select_node.jstree').on("select_node.jstree", function (e, data) {
		const nodo = data.node;
		const nodoId = nodo.id;
		const nodoTexto = nodo.text;
		const nodoTipo = nodo.data?.tipo;

		// Solo permitir seleccionar cuentas de movimiento
		if (nodoTipo === "M") {
			// Guardar la cuenta seleccionada
			cuentaSeleccionada = {
				id: nodoId,
				text: nodoTexto
			};

			// Habilitar el botón de seleccionar
			$('#btnSeleccionarCuenta').prop('disabled', false);
		} else {
			// No es una cuenta de movimiento, mostrar mensaje
			AbrirMensaje(
				"Aviso",
				"Solo puede seleccionar cuentas de movimiento.",
				function () { $("#msjModal").modal("hide"); },
				false,
				["Aceptar"],
				"info!",
				null
			);

			// Desseleccionar el nodo
			$("#cuentasTree").jstree("deselect_node", nodoId);

			// Deshabilitar el botón de seleccionar
			$('#btnSeleccionarCuenta').prop('disabled', true);
			cuentaSeleccionada = null;
		}
	});

	// Cuando el árbol está listo, colapsarlo inicialmente
	$("#cuentasTree").on("ready.jstree", function () {
		$("#cuentasTree").jstree("close_all");
	});
}