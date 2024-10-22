$(function () {
	$("#btnradioPorProveedor").on("click", buscarPorProveedor);
	$("#btnradioSinStk").on("click", buscarSinStk);
	$("#btnradioConPI").on("click", buscarConPI);
	$("#btnradioPorProveedorFamilia").on("click", buscarPorProveedorFamilia);
	$("#btnradioConStkAVencer").on("click", buscarConStkAVencer);
	$("#btnradioConOC").on("click", buscarConOC);
	$("#btnradioPorRubro").on("click", buscarPorRubro);
	$("#btnradioAltaRotacion").on("click", buscarAltaRotacion);
	DisableComboProveedoresFamilia(true);
	DisableComboRubros(true);
	buscarPorProveedor();
});

var tipoBusqueda = "";
const FuncionSobreBusquedaDeProductos = {
	PROVEEDORES: 'PROVEEDORES',
	PROVEEDORESYFAMILIA: 'PROVEEDORESYFAMILIA',
	RUBROS: 'RUBROS',
	SINSTOCK: 'SINSTOCK',
	CONSTOCKAVENCER: 'CONSTOCKAVENCER',
	ALTAROTACION: 'ALTAROTACION',
	CONPI: 'CONPI',
	CONOC: 'CONOC'
}

function buscarPorProveedor() {
	if ($("#listaProveedor").val() == "") {
		$("#listaProveedor").prop("selectedIndex", 1).change();
	}
	DisableComboProveedoresFamilia(true);
	DisableComboRubros(true);
	DisableComboProveedores(false);
	tipoBusqueda = FuncionSobreBusquedaDeProductos.PROVEEDORES;
	BuscarProductos(tipoBusqueda);
}

function buscarSinStk() {
	DisableComboProveedoresFamilia(true);
	DisableComboRubros(true);
	DisableComboProveedores(true);
	tipoBusqueda = FuncionSobreBusquedaDeProductos.SINSTOCK;
	BuscarProductos(tipoBusqueda);
}

function buscarConPI() {
	DisableComboProveedoresFamilia(true);
	DisableComboRubros(true);
	DisableComboProveedores(true);
	tipoBusqueda = FuncionSobreBusquedaDeProductos.CONPI;
	BuscarProductos(tipoBusqueda);
}

function buscarPorProveedorFamilia() {
	if ($("#listaProveedor").val() == "") {
		$("#listaProveedor").prop("selectedIndex", 1).change();
	}
	if ($("#listaFamiliaProveedor").val() == "") {
		$("#listaFamiliaProveedor").prop("selectedIndex", 1).change();
	}
	DisableComboProveedoresFamilia(false);
	DisableComboProveedores(true);
	DisableComboRubros(true);
	tipoBusqueda = FuncionSobreBusquedaDeProductos.PROVEEDORESYFAMILIA;
	BuscarProductos(tipoBusqueda);
}

function buscarConStkAVencer() {
	DisableComboProveedoresFamilia(true);
	DisableComboRubros(true);
	DisableComboProveedores(true);
	tipoBusqueda = FuncionSobreBusquedaDeProductos.CONSTOCKAVENCER;
	BuscarProductos(tipoBusqueda);
}

function buscarConOC() {
	DisableComboProveedoresFamilia(true);
	DisableComboRubros(true);
	DisableComboProveedores(true);
	tipoBusqueda = FuncionSobreBusquedaDeProductos.CONOC;
	BuscarProductos(tipoBusqueda);
}

function buscarPorRubro() {
	if ($("#listaRubro").val() == "") {
		$("#listaRubro").prop("selectedIndex", 1).change();
	}
	DisableComboRubros(false);
	DisableComboProveedores(true);
	DisableComboProveedoresFamilia(true);
	tipoBusqueda = FuncionSobreBusquedaDeProductos.RUBROS;
	BuscarProductos(tipoBusqueda);
}

function buscarAltaRotacion() {
	DisableComboProveedoresFamilia(true);
	DisableComboRubros(true);
	DisableComboProveedores(true);
	tipoBusqueda = FuncionSobreBusquedaDeProductos.ALTAROTACION
	BuscarProductos(tipoBusqueda);
}

function BuscarProductos(tipoBusqueda) {
	AbrirWaiting();
	var filtro = "";
	var id = "";
	var tipo = "OC";

	switch (tipoBusqueda) {
		case FuncionSobreBusquedaDeProductos.PROVEEDORES:
			filtro = "C";
			id = $("#listaProveedor").val();
			break;
		case FuncionSobreBusquedaDeProductos.PROVEEDORESYFAMILIA:
			filtro = "F";
			id = $("#listaProveedor").val() + $("#listaFamiliaProveedor").val();
			break;
		case FuncionSobreBusquedaDeProductos.RUBROS:
			id = $("#listaRubro").val();
			filtro = "R";
			break;
		case FuncionSobreBusquedaDeProductos.SINSTOCK:
			id = "null";
			filtro = "S";
			break;
		case FuncionSobreBusquedaDeProductos.CONSTOCKAVENCER:
			id = "null";
			filtro = "V";
			break;
		case FuncionSobreBusquedaDeProductos.ALTAROTACION:
			id = "null";
			filtro = "A";
			break;
		case FuncionSobreBusquedaDeProductos.CONPI:
			id = "null";
			filtro = "I";
			break;
		case FuncionSobreBusquedaDeProductos.CONOC:
			id = "null";
			filtro = "O";
			break;
		default:
	}
	var datos = { filtro, id, tipo };
	PostGenHtml(datos, BuscarProductosOCPIURL, function (obj) {
		$("#divListaProducto").html(obj);
		CerrarWaiting();
		return true
	});
}

function DisableComboProveedores(value) {
	if (value === true) {
		$('#listaProveedor').prop('disabled', 'disabled');
	}
	else {
		$('#listaProveedor').prop('disabled', false);
	}
}

function DisableComboProveedoresFamilia(value) {
	if (value === true) {
		$('#listaFamiliaProveedor').prop('disabled', 'disabled');
	}
	else {
		$('#listaFamiliaProveedor').prop('disabled', false);
	}
}

function DisableComboRubros(value) {
	if (value === true) {
		$('#listaRubro').prop('disabled', 'disabled');
	}
	else {
		$('#listaRubro').prop('disabled', false);
	}
}

