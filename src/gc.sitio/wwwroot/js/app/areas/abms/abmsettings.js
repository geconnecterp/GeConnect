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
	PROVEEDORES_FAMILIA: 'proveedores_familia', 
	SECTORES: 'sectores',
	SUB_SECTORES: 'sub_sectores',
	RUBROS: 'rubros',
	MEDIO_DE_PAGO: 'mediospagos',
	OPCIONES_CUOTA: 'mediospagos_cuotas',
	CUENTA_FIN_CONTABLE: 'mediospagos_ctaf',
	POS: 'mediospagos_pos',
	BANCOS: 'bancos',
	CUENTAS_DIRECTAS: 'gastos'
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
	GridPrUsers: 'tbGridPerfilUsers',
	GridSector: 'tbGridSector',
	GridSubSector: 'tbSubSectorEnTab',
	GridRubro: 'tbRubroEnTab',
	GridMedioDePago: 'tbGridMedioDePago',
	GridOpcionesCuotas: 'tbOpcionesCuotas',
	GridCuentaFinYConta: 'tbCuentaFinYContable',
	GridPos: 'tbPos',
	GridUser: 'tbGridUsers',
	GridProductos: 'tbGridProd',
	GridBarrado: 'tbGridBarr',
	GridLimite: 'tbGridLim',
	GridConsCtaCte: 'tbGridCtaCte',
	GridConsVto: 'tbGridVto',
	GridConsCmpteTot: 'tbGridCmpteTot',
	GridConsCmpteDet: 'tbGridCmpteDet',
	GridVendedor: 'tbGridVendedor',
	GridRepartidor: 'tbGridRepartidor',
	GridZona: 'tbGridZona',
	GridBanco: 'tbGridBanco',
	GridCuentaDirecta: 'tbGridCuentaDirecta',
	
}

const Tabs = {
	TabCliente: 'btnTabCliente',
	TabProveedor: 'btnTabProveedor',
	TabFormasDePago: 'btnTabFormasDePago',
	TabOtrosContactos: 'btnTabOtrosContactos',
	TabNotas: 'btnTabNotas',
	TabObservaciones: 'btnTabObservaciones',
	TabFamilias: 'btnTabFliaProv',
	TabSector: 'btnTabSector',
	TabSubSector: 'btnTabSubSector',
	TabRubro: 'btnTabRubro',
	TabMedioDePago: 'btnTabMedioDePago',
	TabOpcionesCuota: 'btnTabOpcionesCuotas',
	TabCuentaFinYContable: 'btnTabCuentaFinContable',
	TabPos: 'btnTabPos',
	TabBanco: 'btnTabBanco',
	TabCuentaDirecta: 'btnTabCuentaDirecta',
}

const ModImpresion = {
    ModCtaCte: 1, //Consulta de Cuenta Corriente
	ModVenc: 2, //Consulta de Vencimientos
	ModCmpte: 3, //Consulta de Comprobantes"
	ModOrdPagos: 4, //Consulta de Ordenes de Pagos
    ModRecProv: 5 //Consulta de Recepcion de Proveedores"

}

//variables globales
var regSelected = "";
var funcCallBack = "";
var dataBak = {};
var totalRegs = 0;
var pagRegs = 0;
var pags = 0;
var pagina = 1;
var tabAbm = 1;
var accion = "";//para tab01
var accion02 = ""; //para tab02
var accion03 = ""; //para tab03
var accion04 = ""; //para tab04
var init = true;
var EntidadSelect = "";
var EntidadEstado = "";
var sizeMinGrid1 = "150px";
var fkey = ""; //para resguardar la clave de la entidad seleccionada. Ej: Se seleccionar registro de Orden de Pago, y se devuelve el nro de orden de pago.