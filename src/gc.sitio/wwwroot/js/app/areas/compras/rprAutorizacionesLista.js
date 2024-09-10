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
});

function selectRPRow(x) {
	$("#idRPSelected").val(x.cells[0].innerText.trim());
	var link = ModificarAutorizacionRPUrl + "?rp=" + x.cells[0].innerText.trim();
	$("#btnModificarAut").prop("href", link);
	var linkVer = VerAutorizacionRPUrl + "?rp=" + x.cells[0].innerText.trim();
	$("#btnVer").prop("href", linkVer);
};

function ModificarAutorizacion() {

};
