$(function () {
	//$("#listaSucursales").on("change", listaSucursalesChange);
	//$("#txtMeses").on("change", BuscarInfoAdicional);
	//$("#txtSemanas").on("change", BuscarInfoAdicional);
	addTxtMesesKeyUpHandler();
	addTxtSemanasKeyUpHandler();
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacionNC(div);
	});
});

function presentaPaginacionNC(div) {
	div.pagination({
		items: totalRegs,
		itemsOnPage: pagRegs,
		cssStyle: "dark-theme",
		currentPage: pagina,
		onPageClick: function (num) {
			BuscarProductos(num);
		}
	});
	$("#pagEstado").val(false);
	$("#divFiltro").collapse("hide")
	return true;
}

//function listaSucursalesChange() {
//	if (pIdSeleccionado == undefined) {
//		AbrirMensaje("Atención", "Debe seleccionar un producto.", function () {
//			$("#msjModal").modal("hide");
//			return true;
//		}, false, ["Aceptar"], "error!", null);
//	}
//	if (pIdSeleccionado == "") {
//		AbrirMensaje("Atención", "Debe seleccionar un producto.", function () {
//			$("#msjModal").modal("hide");
//			return true;
//		}, false, ["Aceptar"], "error!", null);
//	}
//	BuscarInfoAdicional();
//}


function selectListaInfoProdIExMesRow(x) {
}

function selectListaInfoProdIExSemanaRow(x) {
}

function selectListaProductoSustitutoRow(x) {
}

function selectListaInfoProductoRow() {
}