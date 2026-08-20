/**
 * Comportamiento común para posicionar el selector de cuentas contables
 * sobre la cuenta que ya tiene asignada el módulo invocador.
 */
(function (window, $) {
    "use strict";

    function normalizarCuentaId(valor) {
        return valor == null ? "" : String(valor).trim();
    }

    function expandirRuta(tree, nodo) {
        const padres = [];
        let padreId = nodo.parent;

        while (padreId && padreId !== "#") {
            padres.unshift(padreId);
            const padre = tree.get_node(padreId);
            padreId = padre ? padre.parent : "#";
        }

        padres.forEach(function (id) {
            tree.open_node(id);
        });
    }

    function buscarYPosicionar($tree, cuentaId) {
        const tree = $tree.jstree(true);
        if (!tree || tree.get_json('#', { flat: true }).length === 0) {
            return false;
        }

        tree.clear_search();

        if (!cuentaId) {
            tree.close_all();
            return true;
        }

        tree.search(cuentaId, false, true);

        const nodo = tree.get_node(cuentaId);
        if (nodo) {
            expandirRuta(tree, nodo);

            window.setTimeout(function () {
                const $nodo = tree.get_node(cuentaId, true);
                if ($nodo && $nodo.length) {
                    $nodo[0].scrollIntoView({ block: "center", behavior: "auto" });
                }
            }, 0);
        }

        return true;
    }

    function preparar(opciones) {
        const cuentaId = normalizarCuentaId(opciones && opciones.cuentaId);
        const inputSelector = opciones && opciones.inputSelector;
        const treeSelector = opciones && opciones.treeSelector;

        if (!inputSelector || !treeSelector) {
            return;
        }

        const $input = $(inputSelector);
        const $tree = $(treeSelector);
        const token = Number($tree.data('geco-cuenta-inicial-token') || 0) + 1;
        let intentos = 0;

        $tree.data('geco-cuenta-inicial-token', token);
        $input.val(cuentaId).trigger("focus");

        function intentarPosicionar() {
            if ($tree.data('geco-cuenta-inicial-token') !== token) {
                return;
            }

            if (buscarYPosicionar($tree, cuentaId)) {
                return;
            }

            intentos += 1;
            if (intentos < 100) {
                window.setTimeout(intentarPosicionar, 50);
            }
        }

        intentarPosicionar();
    }

    window.GecoSelectorCuentaContable = {
        preparar: preparar
    };
})(window, jQuery);
