$(function () {
	document.getElementById("tbComptesDeRP").addEventListener('click', function (e) {
		if (e.target.nodeName === 'TD') {
			var selectedRow = this.querySelector('.selected-row');
			if (selectedRow) {
				selectedRow.classList.remove('selected-row');
			}
			e.target.closest('tr').classList.add('selected-row');
		}
	});

});

function selectCompteDeRPRow(x) {
	$("#txtNroCompte").val(x.cells[2].innerText.trim());
	$("#txtMonto").val(x.cells[4].innerText.trim());
	$("#tco_id").val(x.cells[0].innerText.trim());
	$("#idTipoCompteDeRPSelected").val(x.cells[0].innerText.trim());
	$("#nroCompteDeRPSelected").val(x.cells[2].innerText.trim());
	var depoSelec = $("#listaDeposito").val();
	var notaAuto = $("#txtNota").val();
	var turno = moment($("#dtpFechaTurno").val()).format("X");
	var ponerEnCurso = $("#chkPonerEnCurso")[0].checked;
	var link = VerDetalleDeCompteDeRPUrl + "?idTipoCompte=" + $("#idTipoCompteDeRPSelected").val() + "&nroCompte=" + $("#nroCompteDeRPSelected").val() + "&depoSelec=" + depoSelec + "&notaAuto=" + notaAuto + "&turno=" + turno + "&ponerEnCurso=" + ponerEnCurso;
	$("#VerDetalle").prop("href", link);
}



