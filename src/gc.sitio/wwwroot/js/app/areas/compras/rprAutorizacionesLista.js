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
	//Primero vemos si el usuario esta trabajando sobre algun registro seleccionado de la grilla
	var rpSeleccionado = ObtenerRPRAutorizacionPendienteSeleccionadoEnLista();
	//if (rpSeleccionado == "") {
	//	var grid = document.getElementById("tbListaAutorizaciones");
	//	if (grid) {
	//		var rowsBody = grid.getElementsByTagName('tbody')[0];
	//		if (rowsBody && rowsBody.firstElementChild) {
	//			rowsBody.firstElementChild.className = "selected-row"
	//			selectRPRow(rowsBody.firstElementChild);
	//		}
	//	}
	//}
	//else {
	//	$("#tbListaAutorizaciones").find('tr').each(function (i, el) {
	//		var td = $(this).find('td');
	//		if (td.eq(0)[0]) {
	//			if (td.eq(0)[0].children[0].innerText == rpSeleccionado) {
	//				el.firstElementChild.className = "selected-row";
	//				selectRPRow(el);
	//			}
	//		}
	//	});
	//}
}

function ObtenerRPRAutorizacionPendienteSeleccionadoEnLista() {
	datos = {};
	PostGen(datos, ObtenerRPRAutorizacionPendienteSeleccionadoEnListaURL, function (o) {
		if (o.error === true) {
			var grid = document.getElementById("tbListaAutorizaciones");
			if (grid) {
				var rowsBody = grid.getElementsByTagName('tbody')[0];
				if (rowsBody && rowsBody.lastElementChild) {
					rowsBody.lastElementChild.className = "selected-row"
					selectRPRow(rowsBody.lastElementChild);
				}
			}
			return "";
		} else if (o.warn === true) {
			return "";
		} else if (o.codigo === "") {
			return "";
		} else {
			if (o.codigo == "") {
				var grid = document.getElementById("tbListaAutorizaciones");
				if (grid) {
					var rowsBody = grid.getElementsByTagName('tbody')[0];
					if (rowsBody && rowsBody.firstElementChild) {
						rowsBody.firstElementChild.className = "selected-row"
						selectRPRow(rowsBody.firstElementChild);
					}
				}
			}
			else {
				$("#tbListaAutorizaciones").find('tr').each(function (i, el) {
					var td = $(this).find('td');
					if (td.eq(0).length > 0) {
						if (td[0]) {
							if (td[0].innerText == o.codigo) {
								el.className = "selected-row";
								selectRPRow(el);
							}
						}
					}
					
				});
			}
			return o.codigo;
		}
	});
}

function selectRPRow(x) {
	$("#idRPSelected").val(x.cells[9].innerText.trim());
	var link = ModificarAutorizacionRPUrl + "?rp=" + x.cells[9].innerText.trim();
	$("#btnModificarAut").prop("href", link);
	var linkVer = VerAutorizacionRPUrl + "?rp=" + x.cells[9].innerText.trim();
	$("#btnVer").prop("href", linkVer);
};

function ModificarAutorizacion() {

};
