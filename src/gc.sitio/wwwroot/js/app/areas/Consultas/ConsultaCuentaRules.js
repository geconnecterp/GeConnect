$(function () {
    $("#btnImprimirCtaCte").on("click", imprimirCtaCte);
    $("#btnImprimirVenc").on("click", imprimirVenc);
    $("#btnImprimirCmpte").on("click", imprimirComp);
    $("#btnImprimirOP").on("click", imprimirOP);
    $("#btnImprimirRecP").on("click", imprimirRecP);

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



function imprimirCtaCte() {

    fkey = ModImpresion.ModCtaCte
    var data = GeneradorParametros(fkey);
    invocacionGestorDoc(data);
}

function imprimirVenc() {
    fkey = ModImpresion.ModVenc;
    var data = GeneradorParametros(fkey);
    invocacionGestorDoc(data);

}

function imprimirComp() {
    fkey = ModImpresion.ModCmpte;
    var data = GeneradorParametros(fkey);
    invocacionGestorDoc(data);

}

function imprimirOP() {
    fkey = ModImpresion.ModOrdPagos;
    var data = GeneradorParametros(fkey);
    invocacionGestorDoc(data);

}

function imprimirRecP() {
    var data = GeneradorParametros(ModImpresion.ModRecProv);
    invocacionGestorDoc(data);

}

function GeneradorParametros(mod) {
    switch (mod) {
        case ModImpresion.ModCtaCte:
            //ctaId = consCta;
            //fechaD = $("#fechaD").val();
            var data = {
                //modulo: mod, parametros: [ consCta.toString(), fechaD.toString() ]
                modulo: mod
            };
            return data;

        case ModImpresion.ModVenc:
            ctaId = consCta;
            fechaD = $("#cvfechaD").val();
            fechaH = $("#cvfechaH").val();
            var data = {
                modulo: mod, parametros: [consCta.toString(), fechaD.toString(), fechaH.toString()]
            };
            return data;
        case ModImpresion.ModCmpte:
            ctaId = consCta;
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