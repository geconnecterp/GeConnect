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
	
	$("#idTipoCompteDeRPSelected").val(x.cells[0].innerText.trim());
	$("#nroCompteDeRPSelected").val(x.cells[2].innerText.trim());
	document.getElementById('IdTipoCompte').value = x.cells[0].innerText.trim();
	document.getElementById('NroCompte').value = x.cells[2].innerText.trim();
	var depoSelec = $("#listaDeposito").val();
	var notaAuto = $("#txtNota").val();
	var turno = moment($("#dtpFechaTurno").val()).format("X");
	var ponerEnCurso = $("#chkPonerEnCurso")[0].checked;
	console.log("?idTipoCompte=" + x.cells[0].innerText.trim() + "&nroCompte=" + x.cells[2].innerText.trim() + "&depoSelec=" + depoSelec + "&notaAuto=" + notaAuto + "&turno=" + turno + "&ponerEnCurso=" + ponerEnCurso);
	var link = VerDetalleDeCompteDeRPUrl + "?idTipoCompte=" + x.cells[0].innerText.trim() + "&nroCompte=" + x.cells[2].innerText.trim() + "&depoSelec=" + depoSelec + "&notaAuto=" + notaAuto + "&turno=" + turno + "&ponerEnCurso=" + ponerEnCurso;
	$("#VerDetalle").prop("href", link);
}



