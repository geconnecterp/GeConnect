$(function () {
    $("#btnImprimirCtaCte").on("click", function () {
        fkey = ModImpresion.ModCtaCte;
        var data = prepararContextoGestorCuenta(GeneradorParametros(fkey), [1]);
        invocacionGestorDoc(data);
    });
    $("#btnImprimirVenc").on("click", function () {
        fkey = ModImpresion.ModVenc;
        var data = prepararContextoGestorCuenta(GeneradorParametros(fkey), [2]);
        invocacionGestorDoc(data);
    });
    $("#btnImprimirComp").on("click", function () {
        fkey = ModImpresion.ModCmpte;
        var data = prepararContextoGestorCuenta(GeneradorParametros(fkey), [3]);
        invocacionGestorDoc(data);
    });
    $("#btnImprimirOP").on("click", function () {
        fkey = ModImpresion.ModOrdPagos;
        var data = prepararContextoGestorCuenta(GeneradorParametros(fkey), [5]);
        invocacionGestorDoc(data);
    });
    $("#btnImprimirRecP").on("click", function () {
        fkey = ModImpresion.ModRecProv;
        var data = prepararContextoGestorCuenta(GeneradorParametros(fkey), [7]);
        invocacionGestorDoc(data);
    });

});

function prepararContextoGestorCuenta(data, reportesPreseleccionados) {
    if (!data) {
        return data;
    }

    data.moduloGestor = "CCUENTAS";
    data.reportesPreseleccionados = reportesPreseleccionados;
    return data;
}


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
            fechaD = $("#rpfechaD").val(); 
            fechaH = $("#rpfechaH").val();

            var data = {
                modulo: mod, parametros: [consCta.toString(), fechaD.toString(), fechaH.toString(), fkey.toString()]
            };
            return data;
        default:
            return false;
    }
}
