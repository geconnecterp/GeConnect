$(function () {
	if (document.getElementById("tbListaAutorizaciones") != null) {
		document.getElementById("tbListaAutorizaciones").addEventListener('click', function (e) {
			if (e.target.nodeName === 'TD') {
				var selectedRow = this.querySelector('.selected-row');
				if (selectedRow) {
					selectedRow.classList.remove('selected-row');
				}
				e.target.closest('tr').classList.add('selected-row');
			}
		});
	}

	$("#btnModificarAut").on("click", ModificarAutorizacion);
	SelecccionarPrimerRegistro("tbListaAutorizaciones");
});

function SelecccionarPrimerRegistro() {
	var grid = document.getElementById("tbListaAutorizaciones");
	if (grid) {
		var rowsBody = grid.getElementsByTagName('tbody')[0];
		if (rowsBody && rowsBody.firstElementChild) {
			rowsBody.firstElementChild.className = "selected-row"
			selectRPRow(rowsBody.firstElementChild);
		}
	}
}

function selectRPRow(x) {
	$("#idRPSelected").val(x.cells[0].innerText.trim());
	var link = ModificarAutorizacionRPUrl + "?rp=" + x.cells[0].innerText.trim();
	$("#btnModificarAut").prop("href", link);
	var linkVer = VerAutorizacionRPUrl + "?rp=" + x.cells[0].innerText.trim();
	$("#btnVer").prop("href", linkVer);
};

function ModificarAutorizacion() {

};
