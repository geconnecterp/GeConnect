$(function () {
	addTxtMesesKeyUpHandler();
	addTxtSemanasKeyUpHandler();
	$("#pagEstado").on("change", function () {
		var div = $("#divPaginacion");
		presentaPaginacionPI(div);
	});
});

function presentaPaginacionPI(div) {
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