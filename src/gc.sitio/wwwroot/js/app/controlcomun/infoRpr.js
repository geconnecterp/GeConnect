$(function () {
	//cargo el js para llamar desde esta funcion a la función de busqueda
	//verifico si se hace click en el boton buscar
	$("#btnBusquedaBase").on("click", function () {
		InicializaPantalla();
		buscarProducto();
		return true;
	});

	$("#btnVolverRpr").on("click", function () {
		var tb = $("#divRprGrid #tbProdRPR tbody td");
		if (tb.length > 0) {
			AbrirMensaje("¡¡Atención!!", "Esta volviendo al inicio de esta aplicación. Tenga en cuenta que si cambia de Nro de Autorización Pendiente, perderá toda la información cargada.",
				function (resp) {
					if (resp === "SI") { window.location.href = homeRPRUrl; }
					else {
						$("#msjModal").modal("hide");
						return true;
					}
				}, true, ["Volver", "Quedarse"], "warn!", null);
		} else {
			window.location.href = homeRPRUrl;
		}



	});
	//input del control. Sirve para permitir inicializar pantalla.
	$("input#Busqueda").on("focus", function () {
		InicializaPantalla();
	});

	$("#estadoFuncion").on("change", verificaEstado); //este control debe ser insertado el mismo o similar para cada modulo.
	$("#CantProdRPR").on("change", function () {
		//aca se puede saber si tiene o no productos cargados
		var cant = $("#CantProdRPR").val();
		if (parseInt(cant) > 0) {
			$("#btnContinuarRpr").show("slow");
		}
		else {
			$("#btnContinuarRpr").hide("fast");
		}
	}); //este control sirve para verificar si hay registros o no y asi presentar o no el boton de avanzar


	$(".inputEditable").on("keypress", analizaEnterInput);
	$("#btnCargarProd").on("click", cargarProductos);
	$("#tbProdRPR").on("click", ".btnDelete", EliminarProducto)


	InicializaPantalla();

	const txtB = document.getElementById("txtBtoEnComprobanteRP");
	txtB.addEventListener('input', function (e) {
		if (!isValid(this.value))
			txtB.value = 0;
	});
});

function isValid(value) {
	if (parseInt(value) < 0)
		return false;
	return true;
}

