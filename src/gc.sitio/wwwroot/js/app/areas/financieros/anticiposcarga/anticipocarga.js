let anticiposSeleccionados = [];
let empleadosSeleccionados = [];
let anticipoHistoricoSeleccionado = null;

$(function () {

	InicializarControles();
	ActualizarEstadoBotonHistorial();
	$(document).on("click", "#btnAgregar", AbrirModalAgregarAnticipo);
	$(document).on("click", "#btnAplicar", AplicarCambiosEnFilas);
	$(document).on("click", "#btnConfirmar", AgregarAnticipo);
	$(document).on("click", "#btnSalir", CerrarModal);
	$(document).on("click", "#btnSalirDeCargaDeHistorial", CerrarModalHistorico);
	$(document).on("click", "#btnConfirmarCargaDeAnticipo", ConfirmarAnticipos);
	$(document).on("click", "#btnCancelar", CancelarAnticipos);
	$(document).on("click", "#btnHistorial", AbrirModalAgregarAnticipoDesdeHistorial);
	$(document).on("click", "#btnBuscarHistoricoDeAnticipo", BuscarHistoricoDeAnticipo);
	$(document).on("click", "#btnConfirmarCargaDesdeHistorial", ConfirmarCargaDesdeHistorial);
	$(document).on("click", "#btnEliminarTodos", EliminarTodosLosAnticipos);

	$(document).on("keydown.autocomplete", "input#Rel01", function () {
		$(this).autocomplete({
			source: function (request, response) {

				data = { prefix: request.term };

				$.ajax({
					url: autoComRel01Url,
					type: "POST",
					dataType: "json",
					data: data,
					success: function (obj) {
						response($.map(obj, function (item) {
							var texto = item.descripcion;
							return { label: texto, value: item.descripcion, id: item.id, prov: item.provId };
						}));
					}
				})
			},
			minLength: 3,
			select: function (event, ui) {
				ctaIdSelected = ui.item.id;
				ctaDescSelected = ui.item.value;


				return true;
			}
		});
	});

	let valorOriginal = null;
	$("#porc_interes").on("focus", function () {
		valorOriginal = $(this).val();
	});
	$("#porc_interes").on("blur", function () {
		const valorActual = $(this).val();
		const filas = $("#tbListaAnticipos tbody tr").length;
		if (valorActual !== valorOriginal && filas > 0) {
			console.log("El valor de % Interés ha cambiado:", valorOriginal, "→", valorActual);
			$(this).trigger("valorInteresModificado", [valorOriginal, valorActual]);
		}
	});
	$("#porc_interes").on("valorInteresModificado", function (e, anterior, nuevo) {
		console.log("Cambio detectado:", anterior, "→", nuevo);
		//valorInteresModificado(nuevo);
	});


	ctaIdSelected = $("#prov_id_selected").val();
	ctaDescSelected = $("#prov_denominacion_selected").val();
	$("#Rel01").val(`${ctaDescSelected} (${ctaIdSelected})`);
});

function EliminarTodosLosAnticipos() {

	const filas = $("#tbListaAnticipos tbody tr").length;

	if (filas <= 0) {
		MostrarError("No existen cuentas cargadas.");
		return;
	}

	AbrirMensaje(
		"ATENCIÓN",
		"¿Está seguro que desea eliminar todas las cuentas cargados?",
		function (e) {

			$("#msjModal").modal("hide");

			switch (e) {
				case "SI":
					handlerEliminarTodosLosAnticipos();
					break;
			}

			return true;
		},
		true,
		["SI", "NO"],
		"question!",
		null
	);
}

function handlerEliminarTodosLosAnticipos() {

	AbrirWaiting();

	PostGen({}, eliminarTodosLosAnticiposUrl, function (obj) {

		CerrarWaiting();

		if (obj.error === true) {
			MostrarError(obj.msg);
		}
		else {
			anticiposSeleccionados = [];
			ActualizarListaDeAnticipos();
		}
	});
}

function valorInteresModificado(nuevo_interes) {
	AbrirWaiting();
	var data = { nuevo_interes };
	PostGen(data, actualizarInteresDeAnticiposUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			setTimeout(() => {
				ActualizarListaDeAnticipos();
			}, 500);
		}
	});
}

function CancelarAnticipos() {
	var filas = $("#tbListaAnticipos tbody tr").length;
	if (filas > 0) {
		AbrirMensaje("ATENCIÓN", "¿Esta seguro que desea cancelar la carga de anticipos? Se eliminarán todos los anticipos cargados.", function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI":
					handlerCancelarAnticipos();
					break;
				case "NO":
					break;
				default: //NO
					break;
			}
			return true;

		}, true, ["Aceptar", "Cancelar"], "question!", null);

	}
}

function handlerCancelarAnticipos() {
	AbrirWaiting();
	PostGen({}, cancelarAnticiposUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			setTimeout(() => {
				$('#modalCargaDeAnticipo').modal('hide');
				$("#listaTipoAnticipo").val("");
				$("#Concepto").val("");
				$("#porc_interes").val("0");
				ActualizarListaDeAnticipos();
			}, 500);
		}
	});
}

function ConfirmarAnticipos() {
	var filas = $("#tbListaAnticipos tbody tr").length;
	if (filas > 0) {
		AbrirMensaje("ATENCIÓN", "¿Esta seguro que desea confirmar la carga de anticipos?", function (e) {
			$("#msjModal").modal("hide");
			switch (e) {
				case "SI":
					handlerConfirmarCargaDeAnticipos();
					break;
				case "NO":
					break;
				default: //NO
					break;
			}
			return true;

		}, true, ["Aceptar", "Cancelar"], "question!", null);

	}
	else {
		AbrirMensaje("ATENCIÓN", "No se han cargado registros de anticipos de empleados.", function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
}

function handlerConfirmarCargaDeAnticipos() {
	AbrirWaiting("Guardando Anticipo...");
	var ant_id = $("#listaTipoAnticipo").val();
	var an_concepto = $("#Concepto").val();
	var an_porc_interes = $("#porc_interes").inputmask('unmaskedvalue');
	var cta_id = ctaIdSelected;
	var data = { ant_id, an_concepto, an_porc_interes, cta_id };
	PostGen(data, confirmarCargaDeAnticipoUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			console.log(obj.id);
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				console.log(obj.id); //Tomar este valor para imprimir.
				ImprimirAnticipo_Generado(obj.id, ctaIdSelected);
				handlerCancelarAnticipos();
				return true;
			}, false, ["Aceptar"], "succ!", null);
		}
	});
}
function ReseteoDeReportes() {
	console.log("Reseto de reportes");
	ReporteResetArre();
}

function ImprimirAnticipo_Generado(id, cta_id) {
	ReseteoDeReportes();
	setTimeout(() => {
		let data = { id, ctaId: cta_id };
		cargarReporteEnArre(39, data, "ANTICIPO DE EMPLEADOS", "", "");
		invocacionGestorDoc({});
	}, 500);
}

//############ COMENTAR AL FINALIZAR ############
// Botón de imprimir
$(document).on("click", ".btnImprimir", function () {
	imprimirOPP();
});

$("#btnImprimirTemp").on("click", function () {
	ImprimirAnticipo_Generado("00-00006214", "C0030000");
});

function imprimirOPP() {
	// Invocar gestor documental
	invocacionGestorDoc({});
}
//############ COMENTAR AL FINALIZAR ############

function ObtenerIdsAnticiposExistentes() {

	return $("#tbListaAnticipos tbody tr")
		.map(function () {
			return $(this).data("cta-id");
		})
		.get()
		.map(String);
}

function AgregarAnticipo() {
	const idsExistentes = ObtenerIdsAnticiposExistentes();
	empleadosSeleccionados = empleadosSeleccionados.filter(
		x => !idsExistentes.includes(String(x.id))
	);

	var cuotas = $("#cuotasModalCarga").inputmask('unmaskedvalue');
	var importe = $("#importeModalCarga").inputmask('unmaskedvalue');
	var intereses = $("#interesModalCarga").inputmask('unmaskedvalue');
	var filasCuentasEmpleadosParaAgregar = $("#tabCuentasEmpleadosParaAgregar tbody tr").length;
	if (filasCuentasEmpleadosParaAgregar <= 0) {
		MostrarError("Debe agregar al menos una Cuenta.");
	}
	else if (intereses < 0) {
		AbrirMensaje("ATENCIÓN", "Intereses no debe ser negativo.", function () {
			$("#msjModal").modal("hide");
			$("#interesModalCarga").trigger('focus');
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting();
		var data = { empleadosSeleccionados, cuotas, importe, intereses };
		PostGen(data, agregarAnticipoUrl, function (obj) {
			if (obj.error === true) {
				CerrarWaiting();
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);

			}
			else {
				setTimeout(() => {
					//$('#modalCargaDeAnticipo').modal('hide');
					CerrarWaiting();
					ActualizarListaDeAnticipos();
					limpiarCamposEnModal();
				}, 300);
			}
		});
	}
}

function CerrarModal() {
	empleadosSeleccionados = [];
	$('#modalCargaDeAnticipo').modal('hide');
}

function CerrarModalHistorico() {
	$('#modalCargaDeAnticipoDesdeHistorial').modal('hide');
	anticipoHistoricoSeleccionado = null;
}

function ActualizarListaDeAnticipos() {
	AbrirWaiting();
	PostGen({}, actualizarListaDeAnticiposUrl, function (obj) {
		$("#divGrillaAnticipos").html(obj);
		RestaurarChecksSeleccionados(anticiposSeleccionados);
		ActualizarEstadoBotonHistorial();
		CerrarWaiting();
	});
}

function ObtenerAnticiposSeleccionados() {
	return $("#tbListaAnticipos .chkItemAnticipo:checked")
		.map(function () {
			const $fila = $(this).closest("tr");

			return {
				id: $fila.data("cta-id"),
				descripcion: $fila.data("cta-denominacion")
			};
		})
		.get();
}

function AplicarCambiosEnFilas() {

	const filas = $("#tbListaAnticipos tbody tr").length;

	if (filas <= 0) {
		MostrarError("No existen registros a los cuales aplicar el proceso.");
		return;
	}

	if (anticiposSeleccionados.length === 0) {
		MostrarError("Debe seleccionar al menos un anticipo.");
		return;
	}

	const aplicarCuotas = $("#chkCuotas").is(":checked");
	const aplicarImporte = $("#chkImporte").is(":checked");
	const aplicarInteres = $("#chkInteres").is(":checked");

	if (!aplicarCuotas && !aplicarImporte && !aplicarInteres) {
		MostrarError("Debe seleccionar al menos un campo para aplicar cambios.");
		return;
	}

	//const cuotas = Number($("#cuotas").inputmask("unmaskedvalue") || 0);
	const cuotas = $("#cuotas").inputmask("unmaskedvalue");
	const importe = $("#importe").inputmask("unmaskedvalue");
	//const interes = Number($("#porc_interes").inputmask("unmaskedvalue") || 0);
	const interes = $("#porc_interes").inputmask("unmaskedvalue");

	if (aplicarCuotas) {

		if (cuotas < 1 || cuotas > 60) {

			AbrirMensaje(
				"ATENCIÓN",
				"La cantidad de cuotas debe estar comprendida entre 1 y 60.",
				function () {
					$("#msjModal").modal("hide");
					$("#cuotas").trigger("focus");
					return true;
				},
				false,
				["Aceptar"],
				"error!",
				null
			);

			return;
		}
	}

	if (aplicarInteres) {

		if (interes < 0) {

			AbrirMensaje(
				"ATENCIÓN",
				"El porcentaje de interés debe ser mayor o igual a 0.",
				function () {
					$("#msjModal").modal("hide");
					$("#porc_interes").trigger("focus");
					return true;
				},
				false,
				["Aceptar"],
				"error!",
				null
			);

			return;
		}
	}

	AbrirWaiting();
	const anticipos = ObtenerAnticiposSeleccionados();
	const data = {
		anticipos,
		cuotas,
		importe,
		interes,
		aplicarCuotas,
		aplicarImporte,
		aplicarInteres
	};

	PostGen(data, aplicarCambiosEnFilasUrl, function (obj) {

		CerrarWaiting();

		if (obj.error === true) {

			AbrirMensaje(
				"ATENCIÓN",
				obj.msg,
				function () {
					$("#msjModal").modal("hide");
					return true;
				},
				false,
				["Aceptar"],
				"error!",
				null
			);
		}
		else {

			setTimeout(() => {
				ActualizarListaDeAnticipos();
			}, 500);
		}
	});
}

function AbrirModalAgregarAnticipoDesdeHistorial() {
	AbrirWaiting();
	let data = {};
	PostGenHtml(data, abrirModalAgregarAnticipoDesdeHistorialUrl, function (obj) {
		$("#divCargaDeAnticipoDesdeHistorial").empty();
		$("#divCargaDeAnticipoDesdeHistorial").html(obj);
		const $modal = $("#modalCargaDeAnticipoDesdeHistorial");

		$modal.modal({
			backdrop: 'static',
		});

		//inicializarCamposEnModal();

		CerrarWaiting();
		$modal.modal('show');

		setTimeout(() => {
			BuscarHistoricoDeAnticipo();
		}, 500);

		return true
	});
}

function BuscarHistoricoDeAnticipo() {
	AbrirWaiting();
	var desde = $("#Desde").val();
	var hasta = $("#Hasta").val();
	var data = { desde, hasta };
	PostGenHtml(data, buscarHistoricoDeAnticipoUrl, function (obj) {
		$("#divGridHistoricoDeAnticipos").empty();
		$("#divGridHistoricoDeAnticipos").html(obj);
		inicializarEventosTablaAnticipoHistorico();
		// Posicionar en el primer registro y cargar detalle
		const $primerRegistro = $("#tbGridAnticipoFinEmp tbody tr:first");

		if ($primerRegistro.length) {
			$primerRegistro.trigger("click");
		}
		CerrarWaiting();
	});
}

function inicializarEventosTablaAnticipoHistorico() {
	let clickTimer = null;

	// CLICK
	$(document)
		.off("click", "#tbGridAnticipoFinEmp tbody tr")
		.on("click", "#tbGridAnticipoFinEmp tbody tr", function () {

			const row = this;
			selectReg(row, "tbGridAnticipoFinEmp");   // CLICK NORMAL
			CargarDetalleDeAnticipo(row);

		});

}

function CargarDetalleDeAnticipo(row) {

	const anCompte = $(row).data("id");

	AbrirWaiting();

	const data = {
		an_compte: anCompte
	};

	PostGenHtml(data, cargarDetalleDeAnticipoUrl, function (obj) {

		$("#divGridHistoricoDeAnticiposDetalle").empty();
		$("#divGridHistoricoDeAnticiposDetalle").html(obj);

		CerrarWaiting();
	});
	anticipoHistoricoSeleccionado = anCompte;
}

function ConfirmarCargaDesdeHistorial() {
	AbrirWaiting("Cargando desde Historial...");
	var filas = $("#tbGridAnticipoFinEmpDetalle tbody tr").length;
	if (filas > 0) {
		var anCompte = anticipoHistoricoSeleccionado;
		var data = { anCompte };
		PostGen(data, confirmarCargaDesdeHistorialUrl, function (obj) {
			CerrarWaiting();
			if (obj.error === true) {
				AbrirMensaje("ATENCIÓN", obj.msg, function () {
					$("#msjModal").modal("hide");
					return true;
				}, false, ["Aceptar"], "error!", null);
			}
			else {
				ActualizarListaDeAnticipos();
			}
		});
	}
	else {
		MostrarError("El anticipo seleccionado no tiene detalle asociado.");
	}
}

function AbrirModalAgregarAnticipo() {
	var intereses = $("#porc_interes").inputmask('unmaskedvalue');
	if (intereses < 0) {
		AbrirMensaje("ATENCIÓN", "Debe establecer un valor para Intereses, mayor o igual a 0.", function () {
			$("#msjModal").modal("hide");
			$("#porc_interes").trigger('focus');
			return true;
		}, false, ["Aceptar"], "error!", null);
	}
	else {
		AbrirWaiting();
		var cta_id = ctaIdSelected;
		var cta_desc = ctaDescSelected;
		var data = { intereses, cta_id, cta_desc };
		PostGenHtml(data, abrirModalAgregarAnticipoUrl, function (obj) {
			$("#divCargaDeAnticipo").empty();
			$("#divCargaDeAnticipo").html(obj);
			const $modal = $("#modalCargaDeAnticipo");

			$modal.modal({
				backdrop: 'static',
			});

			inicializarCamposEnModal();

			CerrarWaiting();
			$modal.modal('show');

			setTimeout(() => {
				const $rel02 = $("#Rel02");
				if ($rel02.length > 0) {
					$rel02.trigger("focus");
					console.log("Foco aplicado a #Rel02");
				} else {
					console.warn("No se encontró el input #Rel02");
				}
			}, 500);

			return true
		});
	}
}

function limpiarCamposEnModal() {
	$("#Rel02").val("");
	$("#Rel02Item").empty();
	$("#cuotas").val("1");
	$("#importe").val("0");
	$("#Rel02").trigger("focus");
	clienteIdSelected = "";
}

function ExisteEmpleadoEnAnticipos(id) {

	return $("#tbListaAnticipos tbody tr").toArray().some(
		tr => String($(tr).data("cta-id")) === String(id)
	);
}

function agregarEmpleadoSeleccionado(id, descripcion) {
	//Tabla izq. en modal
	if (empleadosSeleccionados.some(x => x.id === id)) {
		MostrarError("La cuenta ya existe en Cuentas de Empleados para Agregar.");
		return;
	}
	//Tabla de emp. en Index
	else if (ExisteEmpleadoEnAnticipos(id)) {
		MostrarError("La cuenta ya existe en Cuentas de Empleados.");
		return;
	}
	else {
		empleadosSeleccionados.push({
			id,
			descripcion
		});
		renderizarTablaSeleccionados();
	}
}

function renderizarTablaSeleccionados() {

	const $tbody =
		$("#tabCuentasEmpleadosParaAgregar tbody");

	$tbody.addClass("animando-tabla");

	let html = "";
	let alt = true;

	empleadosSeleccionados.forEach(x => {

		const clase = alt ? "alt" : "";

		html += `
            <tr class="${clase} fila-nueva"
                data-id="${x.id}"
                data-descripcion="${x.descripcion}"
                onclick="selectItemGrillaCuentasEmpleadosParaCargar(this);">

                <td class="text-center" style="width:80px;">
                    ${x.id}
                </td>

                <td class="text-start">
                    ${x.descripcion}
                </td>

            </tr>
        `;

		alt = !alt;
	});

	setTimeout(() => {

		$tbody.html(html);

		requestAnimationFrame(() => {
			$tbody.removeClass("animando-tabla");
		});

	}, 150);
}

function selectItemGrillaCuentasEmpleadosParaCargar(x) {

	$("#tabCuentasEmpleadosParaAgregar tbody tr")
		.removeClass("selected-row");

	$(x).addClass("selected-row");
}

function inicializarCamposEnModal() {
	$("#modalCenterTitle").text("Carga de Anticipo de Empleado");
	getMaskForIntegerMax("#cuotasModalCarga", 1, 60);
	getMaskForMoneyType("#importeModalCarga");
	getMaskForMoneyType("#interesModalCarga");
	$("input#Rel02").on("click", function () {
		$("input#Rel02").val("");
		$("#Rel02Item").val("");
	});
	$(document).on("keydown.autocomplete", "input#Rel02", function () {
		$(this).autocomplete({
			source: function (request, response) {

				data = { prefix: request.term };

				$.ajax({
					url: autoComRel02Url,
					type: "POST",
					dataType: "json",
					data: data,
					success: function (obj) {
						response($.map(obj, function (item) {
							return normalizarClienteAutocomplete(item);
						}));
					}
				})
			},
			minLength: 3,
			select: function (event, ui) {
				clienteIdSelected = ui.item.id;
				clienteDescSelected = ui.item.value;
				agregarEmpleadoSeleccionado(
					ui.item.id,
					ui.item.value
				);
				return true;
			}
		});
		aplicarRenderClienteAutocomplete($(this));
	});

	// 🧠 Lista ordenada manualmente
	const $ordenFoco = [
		$("#Rel02"),
		$("#cuotasModalCarga"),
		$("#importeModalCarga"),
		$("#interesModalCarga"),
		$("#btnConfirmar"),
		$("#btnSalir")
	];

	$("#cuotasModalCarga").off("keydown.enterDirect").on("keydown.enterDirect", function (e) {
		if (e.key === "Enter") {
			e.preventDefault();
			e.stopImmediatePropagation(); // 🔒 Evita salto al btn-close

			// Foco directo al botón Confirmar
			$("#importeModalCarga").trigger('focus');
		}
	});


	$("#importeModalCarga").off("keydown.enterDirect").on("keydown.enterDirect", function (e) {
		if (e.key === "Enter") {
			e.preventDefault();
			e.stopImmediatePropagation(); // 🔒 Evita salto al btn-close

			// Foco directo al botón Confirmar
			$("#interesModalCarga").trigger('focus');
		}
	});

	$("#interesModalCarga").off("keydown.enterDirect").on("keydown.enterDirect", function (e) {
		if (e.key === "Enter") {
			e.preventDefault();
			e.stopImmediatePropagation(); // 🔒 Evita salto al btn-close

			// Foco directo al botón Confirmar
			$("#btnConfirmar").trigger('focus');
		}
	});

	// 🔘 Enter en botón dispara acción
	$("#btnConfirmar").off("keydown.enterClick").on("keydown.enterClick", function (e) {
		if (e.key === "Enter") {
			e.preventDefault();
			$(this).trigger('click');
		}
	});

	$(document).off("dblclick", "#tabCuentasEmpleados tbody tr").on(
		"dblclick",
		"#tabCuentasEmpleados tbody tr",
		function () {

			let id = $(this).data("id");
			let descripcion = $(this).data("descripcion");

			agregarEmpleadoSeleccionado(id, descripcion);
		});

	$(document).off("dblclick", "#tabCuentasEmpleadosParaAgregar tbody tr").on(
		"dblclick",
		"#tabCuentasEmpleadosParaAgregar tbody tr",
		function () {

			let id = $(this).data("id");

			empleadosSeleccionados =
				empleadosSeleccionados.filter(
					x => x.id !== id
				);

			renderizarTablaSeleccionados();
		});

	$(document).off("click", "#btnAgregarTodasLasCuentas").on("click", "#btnAgregarTodasLasCuentas", AgregarTodasLasCuentas);

	$(document).off("click", "#btnQuitarTodasLasCuentas").on("click", "#btnQuitarTodasLasCuentas", QuitarTodasLasCuentas);
}

function AgregarTodasLasCuentas() {

	const idsExistentes = new Set(
		empleadosSeleccionados.map(x => x.id)
	);

	$("#tabCuentasEmpleados tbody tr").each(function () {

		const id = $(this).data("id");

		if (!idsExistentes.has(id)) {

			empleadosSeleccionados.push({
				id: id,
				descripcion: $(this).data("descripcion")
			});

			idsExistentes.add(id);
		}
	});

	renderizarTablaSeleccionados();
	animarGrillaCuentasParaAgregar();
}

function animarGrillaCuentasParaAgregar() {

	const $grid = $("#containerGrillaCuentasParaAgregar");

	$grid.removeClass("destino-actualizado");

	// Fuerza el navegador a recalcular el DOM para reiniciar la animación
	void $grid[0].offsetWidth;

	$grid.addClass("destino-actualizado");
}

function QuitarTodasLasCuentas() {

	if (empleadosSeleccionados.length === 0) {
		return;
	}
	empleadosSeleccionados = [];

	renderizarTablaSeleccionados();
	animarGrillaCuentasParaAgregar();
}

function eliminarItem(id) {
	AbrirMensaje("ATENCIÓN", `¿Esta seguro de eliminar el item Anticipo?`, function (e) {
		$("#msjModal").modal("hide");
		switch (e) {
			case "SI":
				handlerEliminarItemAnticipo(id);
				break;
			case "NO":
				break;
			default: //NO
				break;
		}
		return true;

	}, true, ["Aceptar", "Cancelar"], "question!", null);
}

function handlerEliminarItemAnticipo(id) {
	var data = { id };
	PostGen(data, eliminarItemAnticipoUrl, function (obj) {
		CerrarWaiting();
		if (obj.error === true) {
			AbrirMensaje("ATENCIÓN", obj.msg, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else {
			ActualizarListaDeAnticipos();
		}
	});
}

function selectItemGrillaAnticipo(x) {
	$("#tbListaAnticipos tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
}

function selectItemGrillaCuentasEmpleados(x) {
	$("#tabCuentasEmpleados tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
}

function selectItemGrillaCuentasEmpleadosParaCargar(x) {
	$("#containerGrillaCuentasParaAgregar tbody tr").each(function (index) {
		$(this).removeClass("selected-row");
		$(this).removeClass("selectedEdit-row");
	});
	$(x).addClass("selected-row");
}

function InicializarControles() {
	getMaskForMoneyType("#porc_interes");
	getMaskForIntegerMax("#cuotas", 1, 60);
	getMaskForMoneyType("#importe");
	$("#listaTipoAnticipo").trigger('focus');

	const ordenFoco = [
		"#listaTipoAnticipo",
		"#Concepto",
		"#Rel01",
		"#cuotas",
		"#importe",
		"#porc_interes",
		"#btnAgregar",
		"#btnConfirmarCargaDeAnticipo",
		"#btnCancelar"
	];

	$(document)
		.off("keydown.flujoAnticipos")
		.on(
			"keydown.flujoAnticipos",
			ordenFoco.join(","),
			function (e) {

				if (e.key !== "Enter")
					return;

				// si el autocomplete está abierto,
				// dejamos actuar a jQuery UI
				if ($(this).is("#Rel01") &&
					$(".ui-autocomplete:visible").length > 0) {
					return;
				}

				e.preventDefault();

				const actual = "#" + this.id;
				const indice = ordenFoco.indexOf(actual);

				if (indice >= 0 &&
					indice < ordenFoco.length - 1) {

					$(ordenFoco[indice + 1]).trigger("focus");
				}
			});

	$(document)
		.off("keydown.botonesAnticipos")
		.on(
			"keydown.botonesAnticipos",
			"#btnAgregar,#btnConfirmarCargaDeAnticipo,#btnCancelar",
			function (e) {

				if (e.key === "Enter") {
					e.preventDefault();
					$(this).trigger("click");
				}
			});

	$(document).on("change", "#chkTodos", function () {
		const checked = $(this).prop("checked");
		$(".chkItemAnticipo").prop("checked", checked);
		anticiposSeleccionados = ObtenerIdsSeleccionados();
	});

	$(document).on("change", ".chkItemAnticipo", function () {
		anticiposSeleccionados = ObtenerIdsSeleccionados();
		ActualizarCheckCabecera();
	});

	$("#cuotas,#importe,#porc_interes")
		.addClass("campo-deshabilitado");

	$(".check-aplicar").change(function () {

		const input = $(this)
			.closest(".input-group-text")
			.next(".input-group-text")
			.next("input");

		input.toggleClass("campo-deshabilitado", !this.checked);

	});
}

function ObtenerIdsSeleccionados() {

	return $(".chkItemAnticipo:checked")
		.map(function () {
			return $(this).data("id");
		})
		.get();
}

function RestaurarChecksSeleccionados(idsSeleccionados) {

	$(".chkItemAnticipo").each(function () {

		const id = $(this).data("id").toString();

		if (idsSeleccionados.includes(id)) {
			$(this).prop("checked", true);
		}
	});

	ActualizarCheckCabecera();
}

function ActualizarEstadoBotonHistorial() {
	const tieneFilas = $("#tbListaAnticipos tbody tr").length > 0;

	$("#btnHistorial").prop("disabled", tieneFilas);
}

function ActualizarCheckCabecera() {

	const total = $(".chkItemAnticipo").length;
	const seleccionados = $(".chkItemAnticipo:checked").length;

	$("#chkTodos").prop(
		"checked",
		total > 0 && total === seleccionados
	);
}

function getMaskForMoneyType(selector) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: '.',
		radixPoint: ',',
		digits: 2,
		digitsOptional: false,
		allowMinus: false,
		prefix: '',
		suffix: '',
		rightAlign: true,
		unmaskAsNumber: true
	});
}

function getMaskForIntegerMax24(selector) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: '.',       // separador de miles
		digits: 0,                 // sin decimales
		digitsOptional: false,
		allowMinus: false,
		prefix: '',
		suffix: '',
		rightAlign: true,
		unmaskAsNumber: true,
		min: 1,
		max: 24
	});
}

function getMaskForIntegerMax(selector, min = 1, max = 60) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: '.',       // separador de miles
		digits: 0,                 // sin decimales
		digitsOptional: false,
		allowMinus: false,
		prefix: '',
		suffix: '',
		rightAlign: true,
		unmaskAsNumber: true,
		min: min,
		max: max
	});
}

function getMaskForIntegerMax1000(selector) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: '.',       // separador de miles
		digits: 0,                 // sin decimales
		digitsOptional: false,
		allowMinus: false,
		prefix: '',
		suffix: '',
		rightAlign: true,
		unmaskAsNumber: true,
		min: 0,
		max: 1000
	});
}

function MostrarError(msg) {
	AbrirMensaje("ATENCIÓN", msg, function () {
		$("#msjModal").modal("hide");
		return true;
	},
		false, ["Aceptar"], "error!", null
	);
}