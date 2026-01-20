$(function () {
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacion(div);
	});

	$(document).on("dblclick", "#" + Grids.GridCuentaDirecta + " tbody tr", function () {
		x = $(this);
		ejecutaDblClickGrid(x, Grids.GridCuentaDirecta);
	});

	/*ABM Botones*/
	$("#btnAbmNuevo").on("click", function () { btnNuevoClick(); });
	$("#btnAbmModif").on("click", function () { btnModiClick(); });
	$("#btnAbmElimi").on("click", function () { btnBajaClick(); });
	$("#btnAbmAceptar").on("click", function () { btnSubmitClick(); });
	$("#btnAbmCancelar").on("click", function () { btnCancelClick(); });

	$(document).on("change", "#listaLs03", controlalistaLs03Selected);
	$("#btnDetalle").on("mousedown", analizaEstadoBtnDetalle);
	$("#btnDetalle").prop("disabled", true);
	$("#btnAbmAceptar").hide();
	$("#btnAbmCancelar").hide();
	$("#btnCancel").on("click", function () {
		//$("#btnFiltro").trigger("click");
		OcultarDivs(true);
		$("#listaLs03").prop("disabled", false);
		$("#divFiltro").collapse('show');
	});

	$("#btnBuscar").on("click", function () {
		dataBak = "";
		pagina = 1;
		buscarCuentasDirectas(pagina);
	});

	InicializaPantallaAbmCuentaDirecta();
	funcCallBack = buscarCuentasDirectas;
	return true;
});

function OcultarDivs(valor) {
	$("#divDetalle").collapse('hide');
	$("#divGrilla").collapse('hide');
	$("#divPaginacion").collapse('hide');
}

function ModificaCuentaDirecta(tabAct) {
	accionBotones(AbmAction.MODIFICACION, tabAct);
	tipoDeOperacion = AbmAction.MODIFICACION;
	SetearDestinoDeOperacion(tabAct);
	$(".nav-link").prop("disabled", true);
	$(".activable").prop("disabled", false);
	desactivarGrilla(Grids.GridCuentaDirecta);
	$("#CuentaDirecta_Ctag_Id").prop("disabled", true);
	$("#CuentaDirecta_Ctag_Denominacion").focus();
}

function ObtenerDatosDeCuentaDirectaParaJson(destinoDeOperacion, tipoDeOperacion) {
	var ctag_id = $("#CuentaDirecta_Ctag_Id").val();
	var ctag_denominacion = $("#CuentaDirecta_Ctag_Denominacion").val();
	var tcg_id = $("#listaTcg").val();
	var tcg_desc = $("#listaTcg option:selected").text();
	var ctag_ingreso = $("#chkCtagIngreso")[0].checked;
	var ctag_valores_anombre = $("#CuentaDirecta_Ctag_Valores_Anombre").val();
	var ctag_activo = 'N';
	if ($("#chkCtagActiva")[0].checked)
		ctag_activo = "S";
	var ccb_id = $("#cuentaContableId").val();
	var ccb_desc = limpiarDescripcion($("#cuentaContable").val());
	var data = { ctag_id, ctag_denominacion, tcg_id, tcg_desc, ctag_ingreso, ctag_valores_anombre, ctag_activo, ccb_id, ccb_desc, destinoDeOperacion, tipoDeOperacion };
	return data;
}

function limpiarDescripcion(desc) {
	return desc.replace(/^\(\d+\)\s*/, "");
}

function BuscarElementoInsertadoCuentaDirecta(ctagId) {
	var data = { ctagId };
	var url = buscarCuentaDirectaCargadaUrl;
	PostGen(data, url, function (obj) {
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", "Se produjo un error al intentar obtener la entidad recientemente cargada.", function () {
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			if (obj.data) {
				$("#chkDescr").prop('checked', true);
				$("#chkDescr").trigger("change");
				$("#Buscar").val(obj.data);
				$("#chkDesdeHasta").prop('checked', false);
				$("#chkDesdeHasta").trigger("change");
				$("#chkRel01").prop('checked', false);
				$("#chkRel01").trigger("change");
				$("#chkRel02").prop('checked', false);
				$("#chkRel02").trigger("change");
				buscarCuentasDirectas(1);
			}
		}
	});
}

function InicializaPantallaAbmCuentaDirecta() {
	var tb = $("#tbGridCuentaDirecta tbody tr");
	if (tb.length === 0) {
		$("#divFiltro").collapse("show")
	}

	$("#lbRel01").text("Tipo");
	$("#lbChkDescr").text("Denominación");
	$("#lbDescr").html("Desc");
	$("#lbRel03").text("Tipo");

	$("#lbChkDesdeHasta").text("ID Cuenta");

	$("#chkRel03").prop('checked', false);
	$("#chkRel03").trigger("change");
	$("#listaLs03").val("");
	$("#listaLs03").prop("disabled", true);
	$("#Rel03List").empty();

	$("#IdSelected").val("");
	$(".activable").prop("disabled", true);
	activarBotones(false);
	CargarTiposDeCuentaDirecta()
	CerrarWaiting();
	return true;
}

$("#chkRel03").on("click", function () {
	if ($("#chkRel03").is(":checked")) {
		$("#listaLs03").prop("disabled", false);
		$("#Rel03List").prop("disabled", false);
		$("#listaLs03").trigger("focus");
	}
	else {
		$("#listaLs03").prop("disabled", true).val("");
		$("#Rel03List").prop("disabled", true).empty();
	}
});

function controlalistaLs03Selected() {
	var item = $("#listaLs03").val();
	var desc = $("#listaLs03 option:selected").text();
	if ($("#Rel03List").has('option:contains("' + item + '")').length === 0 && $("#Rel03List").has('option:contains("' + desc + '")').length === 0) {
		var opc = "<option value=" + item + ">" + desc + "</option>"
		$("#Rel03List").append(opc);
	}
}

function CargarTiposDeCuentaDirecta() {
	var data = {};
	PostGenHtml(data, cargarTiposDeCuentaDirectaUrl, function (obj) {
		$("#divLs03").html(obj);
		$("#listaLs03").prop("disabled", true);
	}, function (obj) {
		ControlaMensajeError(obj.message);
	});
}

function analizaEstadoBtnDetalle() {
	var res = $("#divDetalle").hasClass("show");
	if (res === true) {
		selectRegCli(regSelected, Grids.GridCuentaDirecta);
		activarGrilla(Grids.GridCuentaDirecta);
	}
	return true;

}

function buscarCuentasDirectas(pag, esBaja = false) {
	AbrirWaiting();
	var buscar = "";
	var id = "";
	var id2 = "";
	var r01 = [];

	if ($("#chkDescr").is(":checked")) {
		buscar = $("#Buscar").val();
	}
	if ($("#chkDesdeHasta").is(":checked")) {
		id = $("#Id").val();
		id2 = $("#Id2").val();
	}
	if ($("#chkRel03").is(":checked")) {
		$("#Rel03List").children().each(function (i, item) { r01.push($(item).val()) });
	}

	var data1 = {
		id, id2,
		rel01: r01,
		rel02: [],
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
		$("#divFiltro").collapse("hide")
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
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function NuevaCuentaDirecta() {
	var data = {};
	PostGenHtml(data, nuevaCuentaDirectaUrl, function (obj) {
		$("#divDatosCuentaDirecta").html(obj);
		$(".nav-link").prop("disabled", true);
		$(".activable").prop("disabled", false);
		$("#CuentaDirecta_Ctag_Id").prop("disabled", true);
		desactivarGrilla(Grids.GridCuentaDirecta);
		accionBotones(AbmAction.ALTA, Tabs.TabCuentaDirecta);
		$("#divFiltro").collapse("hide");
		$("#divDetalle").collapse("show");
		setTimeout(function () {
			// Inicializar el selector de cuentas
			inicializarSelectorCuentas();
		}, 500);
		$("#CuentaDirecta_Ctag_Denominacion").focus();
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});
}

function selectRegDbl(x, gridId) {
	AbrirWaiting("Espere mientras se busca el elemento seleccionado...");
	$("#" + gridId + " tbody tr").each(function (index) {
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selectedEdit-row");
	regSelected = x;

	switch (gridId) {
		case Grids.GridCuentaDirecta:
			var ctag_id = x[0].cells[0].innerText.trim();
			var tcg_id = x[0].cells[3].innerText.trim();
			if (ctag_id !== "") {
				ctagIdRow = x[0];
				ctagId = ctag_id;
				BuscarCuentaDirecta(ctag_id, tcg_id);
				/*ActualizarTitulo();*/
				activarBotones(true);
				$(".activable").prop("disabled", true);
				$("#btnDetalle").prop("disabled", false);
				$("#divFiltro").collapse("hide");
				$("#divDetalle").collapse("show");
				$("#IdSelected").val(ctagId);
				posicionarRegOnTop(x);
				desactivarGrilla('tbGridCuentaDirecta');
				setTimeout(function () {
					// Inicializar el selector de cuentas
					inicializarSelectorCuentas();
				}, 1000);
			}
			break;
		default:
			break;
	}
}

function BuscarCuentaDirecta(ctagId, tcgId) {
	var data = { ctagId, tcgId };
	AbrirWaiting();
	PostGenHtml(data, buscarCuentaDirectaUrl, function (obj) {
		$("#divDatosCuentaDirecta").html(obj);
		$("#IdSelected").val($("#CuentaDirecta_Ctag_Id").val());
		let ccb_desc = "";
		let id = $("#CuentaDirecta_Ccb_Id").val();
		let nombre = $("#CuentaDirecta_Ccb_Desc").val();
		if (id != undefined && id != "") {
			ccb_desc = `(${id}) ${nombre}`;
			$("#cuentaContable").val(ccb_desc);
			$("#cuentaContableId").val(id);
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