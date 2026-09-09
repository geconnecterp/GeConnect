(function ($) {
    "use strict";

    function escaparHtml(valor) {
        return $("<div>").text(valor ?? "").html();
    }

    function confirmarReemplazo($boton) {
        var url = $boton.attr("href");
        var productoId = escaparHtml($boton.attr("data-producto-id"));
        var productoDesc = escaparHtml($boton.attr("data-producto-desc"));
        var boxId = escaparHtml($boton.attr("data-box-id"));
        var pedido = escaparHtml($boton.attr("data-pedido"));
        var colectado = escaparHtml($boton.attr("data-colectado"));

        var mensaje = `
            <div class="text-start">
                <p>Está por iniciar el reemplazo del producto solicitado.</p>
                <p class="mb-1"><strong>Producto original:</strong> ${productoId} - ${productoDesc}</p>
                <p class="mb-1"><strong>BOX original:</strong> ${boxId}</p>
                <p class="mb-2"><strong>Pedido:</strong> ${pedido} | <strong>Colectado:</strong> ${colectado}</p>
                <p class="mb-0">El producto que se colecte posteriormente será registrado como reemplazo del producto original. Verifique la información antes de continuar.</p>
            </div>`;

        AbrirMensaje(
            "Confirmar reemplazo",
            mensaje,
            function (respuesta) {
                $("#msjModal").modal("hide");
                if (respuesta === "SI") {
                    window.location.href = url;
                }
            },
            true,
            ["Iniciar reemplazo", "Cancelar"],
            "warn!",
            null,
            "cancelar"
        );
    }

    $(document)
        .off("click.reemplazoProducto", ".btnReemplazarProducto")
        .on("click.reemplazoProducto", ".btnReemplazarProducto", function (evento) {
            evento.preventDefault();
            confirmarReemplazo($(this));
        });
})(jQuery);
