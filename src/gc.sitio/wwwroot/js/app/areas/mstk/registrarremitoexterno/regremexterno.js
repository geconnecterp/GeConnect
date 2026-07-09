let cta_seleccionada = false;
let cta_id_seleccionada = "";

$(function () {
	InicializarEventos();
});

function InicializarEventos() {
	$(document).on("click", "#Rel03", function () { $(this).val(""); cta_seleccionada = false; cta_id_seleccionada = ""; });
	$(document).on("click", "#btnCargar", btnCargarClick);
	$(document).on("click", "#btnConfirmar", btnConfirmarClick);
	$(document).on("click", "#btnCancelar", btnCancelarClick);
	$(document).on("change", "#listaDeposito", listaDepositoChange);
	$(document).on("keyup", "#PtoVta", ControlaKeyUpComptePtoVta);
	$(document).on("focusout", "#PtoVta", ControlaFocusOutComptePtoVta);
	$(document).on("keyup", "#NroComprobante", ControlaKeyUpCompteNro);
	$(document).on("focusout", "#NroComprobante", ControlaFocusOutCompteNro);
	$("#PtoVta").inputmask("9999");
	$("#NroComprobante").inputmask("99999999");
	$(document).ready(function () {
		// Selecciona el primer radio
		$("#DesdeFactura").prop("checked", true);

		// Dispara el evento para aplicar habilitación + limpieza
		$("input[name='TipoRelacion']:checked").trigger("change");
	});
	$(document).on("change", "input[name='TipoRelacion']", function () {

		const tipo = $(this).val();

		// Controles Factura
		const ddlTipo = $("#listaTipoComprobante");
		const txtPtoVta = $("#PtoVta");
		const txtNroComprobante = $("#NroComprobante");

		// Controles Cotización
		const txtNroCotizacion = $("#NroCotizacion");

		// Autocompletar Sin Relación
		const txtAutocompletar = $("#Rel03");
		const hiddenAutocompletar = $("#Rel03Item");

		// Función auxiliar para limpiar
		function limpiarFactura() {
			ddlTipo.val("");              // limpia selección
			txtPtoVta.val("");            // limpia texto
			txtNroComprobante.val("");    // limpia texto
		}

		function limpiarCotizacion() {
			txtNroCotizacion.val("");
		}

		function limpiarSinRelacion() {
			txtAutocompletar.val("");
			hiddenAutocompletar.val("");
		}

		// ============================
		// Estado según radio seleccionado
		// ============================
		if (tipo === "Factura") {

			// Habilitar Factura
			ddlTipo.prop("disabled", false);
			txtPtoVta.prop("disabled", false);
			txtNroComprobante.prop("disabled", false);

			// Deshabilitar Cotización + limpiar
			txtNroCotizacion.prop("disabled", true);
			limpiarCotizacion();

			// Deshabilitar Autocompletar + limpiar
			txtAutocompletar.prop("disabled", true);
			limpiarSinRelacion();
		}

		else if (tipo === "Cotizacion") {

			// Deshabilitar Factura + limpiar
			ddlTipo.prop("disabled", true);
			txtPtoVta.prop("disabled", true);
			txtNroComprobante.prop("disabled", true);
			limpiarFactura();

			// Habilitar Cotización
			txtNroCotizacion.prop("disabled", false);

			// Deshabilitar Autocompletar + limpiar
			txtAutocompletar.prop("disabled", true);
			limpiarSinRelacion();
		}

		else if (tipo === "SinRelacion") {

			// Deshabilitar Factura + limpiar
			ddlTipo.prop("disabled", true);
			txtPtoVta.prop("disabled", true);
			txtNroComprobante.prop("disabled", true);
			limpiarFactura();

			// Deshabilitar Cotización + limpiar
			txtNroCotizacion.prop("disabled", true);
			limpiarCotizacion();

			// Habilitar Autocompletar
			txtAutocompletar.prop("disabled", false);
		}
	});
	CancelarRemito();
}

function btnCargarClick() {
	let v = ValidarFiltrosParaCarga();

	if (!v.ok) {
		AbrirMensaje("Atención", v.msg, function () {
			$("#msjModal").modal("hide");
			return true;
		}, false, ["Aceptar"], "warn!", null);
		return;
	}

	// Si todo está OK → habilitar carga
	HabilitarCargaDeProductos();
	ValidarExistenciaDeProductos();
}

function LimpiarTablaDeProductos() {
	PostGen({}, limpiarProductosCargadosURL, function (obj) {
		CerrarWaiting();
		if (obj.esError === true) {
			AbrirMensaje("ATENCIÓN", obj.mensaje, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else if (obj.esWarn === true) {
			AbrirMensaje("ATENCIÓN", obj.mensaje, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "warn!", null);
		}
		else {
			// ✔ Limpiar tabla
			const tbody = $("#tbGridProductos tbody");
			tbody.empty();
			tbody.append(`
				<tr class="fila-vacia">
					<td colspan="8" class="text-center text-muted py-4">
						<i class="bx bx-info-circle me-2"></i>
						No hay items para mostrar.
					</td>
				</tr>
			`);
		}
	});
}

function ValidarExistenciaDeProductos() {
	var data = ArmarRequestParaBuscarProductos();
	AbrirWaiting();
	PostGen(data, validarExistenciaDeProdsURL, function (obj) {
		CerrarWaiting();
		if (obj.esError === true) {
			AbrirMensaje("ATENCIÓN", obj.mensaje, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		else if (obj.esWarn === true) {
			AbrirMensaje("ATENCIÓN", obj.mensaje, function () {
				$("#msjModal").modal("hide");
				return true;
			}, false, ["Aceptar"], "error!", null);
		}
		///IMPORTANTE!!! Descomentar este codigo en la version final, esta solo para propositos de prueba con comprobantes que no cumplen las condiciones y sirven para pruebas
		// else if (obj.permite === false) {
		// 	AbrirMensaje("ATENCIÓN", obj.mensaje, function () {
		// 		$("#msjModal").modal("hide");
		// 		return true;
		// 	}, false, ["Aceptar"], "error!", null);
		// }
		else {
			AbrirMensaje("ATENCIÓN", "¿El Comprobante ingresado posee productos, desea agregarlos al remito?", function (e) {
				$("#msjModal").modal("hide");
				switch (e) {
					case "SI": //Confirmar
						BuscarProductosDelComprobante();
						break;
					case "NO":
						break;
					default: //NO
						break;
				}
				return true;

			}, true, ["Aceptar", "Cancelar"], "question!", null);
		}
	});
}

function BuscarProductosDelComprobante() {
	AbrirWaiting();
	PostGenHtml({}, cargarProductosDesdeComprobanteURL, function (obj) {
		$("#divProductos").html(obj);
		// Mover el acordeón al contenedor superior
		var info = $("#divProductos").find("#infoComprobanteRendered");

		if (info.length) {
			$("#infoComprobanteContainer").html(info);
		}
		CerrarWaiting();
	}, function (obj) {
		ControlaMensajeError(obj.message);
		CerrarWaiting();
	});

}

function btnConfirmarClick() {
}

function btnCancelarClick() {
	CancelarRemito();
}

function CancelarRemito() {

	// Habilitar sección superior
	$(".row-block input, .row-block select").prop("disabled", false);
	$("input[name='TipoRelacion']").prop("disabled", false);

	// Deshabilitar sección inferior
	$("#ProdID, #ProdUP, #ProdBto, #ProdUnid, #Busqueda").prop("disabled", true);
	$("#btnAgregarProducto, #btnQuitarProducto, #btnBusquedaBase").prop("disabled", true);

	// Botones
	$("#btnCargar").prop("disabled", false);
	$("#btnConfirmar").prop("disabled", true);
	$("#btnCancelar").prop("disabled", true);

	LimpiarTablaDeProductos();

	console.log("CancelarRemito");
}


function HabilitarCargaDeProductos() {

	// Bloquear sección superior
	$(".row-block input, .row-block select").prop("disabled", true);
	$("input[name='TipoRelacion']").prop("disabled", true);

	// Habilitar sección inferior
	$("#ProdID, #ProdUP, #ProdBto, #ProdUnid, #Busqueda").prop("disabled", false);
	$("#btnAgregarProducto, #btnQuitarProducto, #btnBusquedaBase").prop("disabled", false);

	// Botones
	$("#btnCargar").prop("disabled", true);
	$("#btnConfirmar").prop("disabled", false);
	$("#btnCancelar").prop("disabled", false);

	console.log("HabilitarCargaDeProductos");
}

function ArmarRequestParaBuscarProductos() {
	let tipo = $("input[name='TipoRelacion']:checked").val();
	if (tipo === "Factura") {
		return {
			tipo: tipo,
			tco_id: $("#listaTipoComprobante").val(),
			cm_compte: $("#PtoVta").inputmask('unmaskedvalue').padStart(4, '0') + "-" + $("#NroComprobante").inputmask('unmaskedvalue').padStart(8, '0'),
			pre_id: "",
			box_id: $("#listaBoxes").val(),
			depo_id: $("#listaDeposito").val()
		};
	}

	if (tipo === "Cotizacion") {
		return {
			tipo: tipo,
			tco_id: "",
			cm_compte: "",
			pre_id: $("#NroCotizacion").val(),
			box_id: $("#listaBoxes").val(),
			depo_id: $("#listaDeposito").val()
		};
	}

	if (tipo === "SinRelacion") {
		return {
			tipo: tipo,
			tco_id: "",
			cm_compte: "",
			pre_id: "",
			box_id: $("#listaBoxes").val(),
			depo_id: $("#listaDeposito").val()
		};
	}
}


function ValidarFiltrosParaCarga() {

	let tipo = $("input[name='TipoRelacion']:checked").val();

	// Validaciones según radio
	if (tipo === "Factura") {
		if ($("#PtoVta").val().trim() === "" || $("#NroComprobante").val().trim() === "") {
			return { ok: false, msg: "Debe completar Punto de Venta y Número de Comprobante." };
		}
	}

	if (tipo === "Cotizacion") {
		if ($("#NroCotizacion").val().trim() === "") {
			return { ok: false, msg: "Debe completar el Número de Cotización." };
		}
	}

	if (tipo === "SinRelacion") {
		if ($("#Rel03").val().trim() === "") {
			return { ok: false, msg: "Debe completar el campo de búsqueda de relación." };
		}
	}

	// Validaciones comunes
	if ($("#listaDeposito").val().trim() === "") {
		return { ok: false, msg: "Debe seleccionar un Depósito." };
	}

	if ($("#listaBoxes").val().trim() === "") {
		return { ok: false, msg: "Debe seleccionar un BOX." };
	}

	if ($("#Obs").val().trim() === "") {
		return { ok: false, msg: "Debe completar la Observación." };
	}

	return { ok: true };
}


function ControlaFocusOutComptePtoVta() {
	var ptv = $("#PtoVta").inputmask('unmaskedvalue');
	if (ptv != "") {
		var aux = $("#PtoVta").inputmask('unmaskedvalue').padStart(4, '0');
		$("#PtoVta").val(aux);
		$("#NroComprobante").trigger("focus");
	}
}

function ControlaKeyUpComptePtoVta(e) {
	if (e.which == 13 || e.which == 109) {
		var aux = $("#PtoVta").inputmask('unmaskedvalue').padStart(4, '0');
		$("#PtoVta").val(aux);
		$("#NroComprobante").trigger("focus");
	}
}

function ControlaFocusOutCompteNro() {
	var nro = $("#NroComprobante").inputmask('unmaskedvalue');
	if (nro != "") {
		var aux = $("#NroComprobante").inputmask('unmaskedvalue').padStart(8, '0');
		$("#NroComprobante").val(aux);
		$("#listaDeposito").trigger("focus");
	}
}

function ControlaKeyUpCompteNro(e) {
	if (e.which == 13 || e.which == 109) {
		var aux = $("#NroComprobante").inputmask('unmaskedvalue').padStart(8, '0');
		$("#NroComprobante").val(aux);
		$("#listaDeposito").trigger("focus");
	}
}

function listaDepositoChange() {
	if ($("#listaDeposito").val() == "") {
		BlanquearComboBoxes();
		return false;
	}
	if ($("#listaDeposito").val() == "0") {
		BlanquearComboBoxes();
		return false;
	}
	BuscarBoxDesdeDeposito();
}

function BlanquearComboBoxes() {
	var depoId = "0";
	var datos = { depoId };
	PostGenHtml(datos, BuscarBoxesDesdeDepositoURL, function (obj) {
		$("#divComboBoxes").html(obj);
		$("#listaBoxes").on("change", listaBoxesChange);
		CerrarWaiting();
		return true
	});
}

function listaBoxesChange() {
}

function BuscarBoxDesdeDeposito() {
	AbrirWaiting();
	var depoId = $("#listaDeposito").val();
	var datos = { depoId };
	PostGenHtml(datos, BuscarBoxesDesdeDepositoURL, function (obj) {
		$("#divComboBoxes").html(obj);
		$("#listaBoxes").on("change", listaBoxesChange);
		CerrarWaiting();
		return true
	});
}

$("#Rel03").autocomplete({
	source: function (request, response) {

		data = { prefix: request.term }; Rel03

		$.ajax({
			url: autoComRel03Url,
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
		cta_seleccionada = true;
		cta_id_seleccionada = ui.item.id;
		return true;
	}
});

function VerificarExistenciaDeProductosDesdeComprobantes(datos) {


}

function cargarProductos() {
}

function EliminarProducto(id) {
}

function InicializaPantalla() {
}

async function verificaEstado(e) {
	FunctionCallback = null; //inicializo funcion por si tiene alguna funcionalidad asignada.
	var res = $("#estadoFuncion").val();
	CerrarWaiting();
	if (res === "true") {
		//traigo la variable productoBase e hidrato componentes
		var prod = productoBase;

		let tipo = $("input[name='TipoRelacion']:checked").val();
		if (tipo === "Factura" || tipo === "Cotizacion") {
			const ok = await ValidarExistenciaDeProducto(prod.p_id);
			if (!ok) return;   // corta el flujo
			// seguir con la carga
			ProcesarCargaProducto(prod);
		}
		ProcesarCargaProducto(prod);
	}
	return true;
}

function ProcesarCargaProducto(prod) {
	$("#ProdID").val(prod.p_id);
	$("#ProdNombre").text(prod.p_desc);
	$("#estadoFuncion").val(false);
	$("#UpID").val(prod.up_id);
	$("#BarradoID").val(prod.p_id_barrado);
	$("#ProvID").val(prod.p_id_prov);
	$("#txtUP").mask("000.000.000.000", { reverse: true });
	$("txtBto").mask('#,##0', {
		reverse: true,
		translation: {
			'#': {
				pattern: /-|\d/,
				recursive: true
			}
		},
		onChange: function (value, e) {
			e.target.value = value.replace(/(?!^)-/g, '').replace(/^,/, '').replace(/^-,/, '-');
		}
	});

	$("#ProdUP").val(prod.p_unidad_pres).prop("disabled", false);
	$("#ProdBto").val(prod.bulto).prop("disabled", false);
	$("#ProdUnid").mask("000.000.000.000", { reverse: true });

	if (prod.up_id !== "07") {  //unidades enteras
		// $("#box").mask("000.000.000.000,00", { reverse: true });
		$("#ProdUnid").mask("000.000.000.000,00", { reverse: true });
		$("#ProdUnid").val(0).prop("disabled", false);
	}
	else { //unidades decimales
		//$("#txtUnid").val(0).prop("disabled", true);
	}
	$("#Busqueda").val("");
	if (prod.p_con_vto !== "N") {
	} else {
	}
	$("#ProdUP").focus();
}

///Funcion para validar la existencia del prudcto agregado en el comprobante 
function ValidarExistenciaDeProducto(p_id) {
	return new Promise(function (resolve) {
		AbrirWaiting("Validando existencia de producto en comprobante...");
		PostGen({ pId: p_id }, validarExistenciaDeProductoUrl, function (o) {
			CerrarWaiting();
			if (o.esError === true) {
				AbrirMensaje("Atención", o.mensaje, function () {
					$("#msjModal").modal("hide");
					resolve(false);
				}, false, ["Aceptar"], "error!", null);
				return;
			}
			if (o.esWarn === true) {
				AbrirMensaje("Atención", o.mensaje, function () {
					$("#msjModal").modal("hide");
					resolve(false);
				}, false, ["Aceptar"], "warn!", null);
				return;
			}
			if (o.permite === false) {
				AbrirMensaje("ATENCIÓN", o.mensaje, function () {
					$("#msjModal").modal("hide");
					resolve(false);
				}, false, ["Aceptar"], "error!", null);
				return;
			}
			resolve(true);
		});
	});
}
