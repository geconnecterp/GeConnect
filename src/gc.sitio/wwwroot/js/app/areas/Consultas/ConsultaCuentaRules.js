$(function () {
    $("#btnImprimirCtaCte").on("click", function () {
        fkey = ModImpresion.ModCtaCte;
        var data = GeneradorParametros(fkey);
        invocacionGestorDoc(data);
    });
    $("#btnImprimirVenc").on("click", function () {
        fkey = ModImpresion.ModVenc;
        var data = GeneradorParametros(fkey);
        invocacionGestorDoc(data);
    });
    $("#btnImprimirCmpte").on("click", function () {
        fkey = ModImpresion.ModCmpte;
        var data = GeneradorParametros(fkey);
        invocacionGestorDoc(data);
    });
    $("#btnImprimirOP").on("click", function () {
        fkey = ModImpresion.ModOrdPagos;
        var data = GeneradorParametros(fkey);
        invocacionGestorDoc(data);
    });
    $("#btnImprimirRecP").on("click", function () {
        fkey = ModImpresion.ModRecProv;
        var data = GeneradorParametros(fkey);
        invocacionGestorDoc(data);
    });

});


//funciones que funcionaran como las variables de sesion para devolver estados de
//objetos.

function GridCtaCte() {
    grid = Grids.GridConsCtaCte;
    return hayRegistrosEnTabla(grid);
}

function GridVencimiento() {
    grid = Grids.GridConsVto;
    return hayRegistrosEnTabla(grid);
}

function GridCtaCte() {
    grid = Grids.GridConsCmpteTot;
    return hayRegistrosEnTabla(grid);
}

function GeneradorParametros(mod) {
    switch (mod) {
        case ModImpresion.ModCtaCte:
            fechaD = $("#fechaD").val();
            var data = {
                modulo: mod, parametros: [ consCta.toString(), fechaD.toString() ]
                //modulo: mod
            };
            return data;

        case ModImpresion.ModVenc:
            fechaD = $("#cvfechaD").val();
            fechaH = $("#cvfechaH").val();
            var data = {
                modulo: mod, parametros: [consCta.toString(), fechaD.toString(), fechaH.toString()]
            };
            return data;
        case ModImpresion.ModCmpte:
            relCuil = false;
            if ($("#relCuil").is(":checked")) {
                relCuil = true;
            };
            meses = $("#inMeses").val();
            var data = {
                modulo: mod, parametros: [consCta.toString(), relCuil.toString(), meses.toString(), fkey.toString()]
            }; //recuperamos el periodo de la variable global
            return data;
        case ModImpresion.ModOrdPagos:
            fechaD = $("#opfechaD").val();
            fechaH = $("#opfechaH").val();
            var data = {
                modulo: mod, parametros: [consCta.toString(), fechaD.toString(), fechaH.toString(), fkey.toString()]
            };
            return data;
        case ModImpresion.ModRecProv:
            fechaD = $("#opfechaD").val();
            fechaH = $("#opfechaH").val();

            var data = {
                modulo: mod, parametros: [consCta.toString(), fechaD.toString(), fechaH.toString(), fkey.toString()]
            };
            return data;
        default:
            return false;
    }
}