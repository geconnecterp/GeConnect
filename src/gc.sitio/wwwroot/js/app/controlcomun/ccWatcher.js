// ccWatcher.js
const ccWatcher = (function () {
	let cuentaInput, razonSocialInput, buscarBtn;

	function init() {
		cuentaInput = document.getElementById('Cuenta');
		razonSocialInput = document.getElementById('razonsocial');
		buscarBtn = document.getElementById('btnBuscarCC');
		//buscarProdBtn = document.getElementById('btnBusquedaBase');

		if (!cuentaInput || !razonSocialInput || !buscarBtn) {
			console.warn('ccWatcher: No se encontraron los elementos esperados');
			return;
		}

		// Interceptar cambios manuales en el input Cuenta
		cuentaInput.addEventListener('input', onCuentaChanged);

		// Interceptar clic en el botón Buscar
		buscarBtn.addEventListener('click', onBuscarClicked);

		// Observar cambios en razonSocial (por AJAX u otros)
		const observer = new MutationObserver(onRazonSocialUpdated);
		observer.observe(razonSocialInput, { attributes: true, childList: true, subtree: true });
	}

	function onCuentaChanged(e) {
		console.log('Cuenta modificada:', e.target.value);
		if (e.target.value == undefined || e.target.value == "") {
			console.log('Actualizar razon social: ', e.target.value);
			document.getElementById('razonsocial').value = "";
		}
		// Podés disparar eventos custom, actualizar otros campos, etc.
	}

	function onBuscarClicked() {
		console.log('Botón Buscar CC presionado');
		// Podés lanzar lógica de búsqueda, mostrar modal, etc.
	}

	function onRazonSocialUpdated(mutations) {
		console.log('Razón social actualizada:', razonSocialInput.value);
		// Podés validar, mostrar íconos, etc.
	}

	return {
		init
	};
})();
