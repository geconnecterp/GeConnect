$(function () {
	InicializarControles();
});



function InicializarControles() {
	getMaskForMoneyType("#Importe");
}

function eliminarItem(orden) {
}

function selectItemGrillaProyeccion(x) {

}

function getMaskForMoneyType(selector) {
	$(selector).inputmask({
		alias: 'numeric',
		groupSeparator: '.',
		radixPoint: ',',
		digits: 2,
		digitsOptional: false,
		allowMinus: false,
		prefix: '',
		suffix: '',
		rightAlign: true,
		unmaskAsNumber: true
	});
}