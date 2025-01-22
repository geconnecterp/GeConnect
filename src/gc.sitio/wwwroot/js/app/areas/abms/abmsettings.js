const AbmAction = {
	ALTA: 'A',
	BAJA: 'B',
	MODIFICACION: 'M',
	SUBMIT: 'S',
	CANCEL: 'C'
}

const EstadoColor = {
	Activo: '#34dc22', //≈ Lima
	NoActivo: '#f74146', //≈ Sunset Orange
	Discontinuo: '#4180f7' //≈ Dodger Blue
}

const AbmObject = {
	CLIENTES: 'clientes', //ABM principal clientes
	PROVEEDORES: 'proveedores', //ABM principal proveedores
	CLIENTES_CONDICIONES_VTA: 'cuentas_fp', //ABM relacionado clientes formas de pago
	CUENTAS_CONTACTOS: 'cuentas_contactos', //ABM relacionado contactos
	CUENTAS_NOTAS: 'cuentas_notas', //ABM relacionado notas de clientes
	CUENTAS_OBSERVACIONES: 'cuentas_obs', //ABM relacionado observaciones de clientes
	PROVEEDORES_FAMILIA: 'proveedores_familia' //TODO: revisar con Carlos si esta bien el nombre
}

const Grids = {
	GridCliente: 'tbGridCliente',
	GridProveedor: 'tbGridProveedor',
	GridFP: 'tbClienteFormaPagoEnTab',
	GridOC: 'tbClienteOtroContacto',
	GridNota: 'tbClienteNotas',
	GridObs: 'tbClienteObservaciones',
	GridFlias: 'tbProveedorFliaProv',
	GridPerfil: 'tbGridPerfil',
	GridPrUsers: 'tbGridPerfilUsers'
}

const Tabs = {
	TabCliente: 'btnTabCliente',
	TabProveedor: 'btnTabProveedor',
	TabFormasDePago: 'btnTabFormasDePago',
	TabOtrosContactos: 'btnTabOtrosContactos',
	TabNotas: 'btnTabNotas',
	TabObservaciones: 'btnTabObservaciones',
	TabFamilias: 'btnTabFliaProv'
}

//variables globales
var regSelected = "";
var funcCallBack = "";
var dataBak = {};
var totalRegs = 0;
var pagRegs = 0;
var pags = 0;
var pagina = 1;
