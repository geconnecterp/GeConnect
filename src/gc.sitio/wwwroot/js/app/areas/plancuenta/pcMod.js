$(function () {

    $("#btnFiltro").on("click", function () {
        if ($("#divFiltro").hasClass("show")) {
            $("#divDetalle").collapse("hide");
        }
        /*activarGrilla(Grids.GridUser);*/
    });

    $("#chkTipo").on("click", function () {
        if ($("#chkTipo").is(":checked")) {
            $("#Tipo").prop("disabled", false);
        }
        else {
            $("#Tipo").prop("disabled", true).val("%");
        }
    });

    ///utilizo el evento mousedown para ejecutar el evento antes que el collapse.
    $("#btnDetalle").on("mousedown", function () {
        manejarDetalle();
    });

    $("#btnDetalle").prop("disabled", true);
    $("#btnCancel").on("click", function () {
        window.location.href = homepc;
    });
    $("#pagEstado").on("change", function () {
        var div = $("#divPaginacion");
        presentaPaginacion(div);
    });
    $("#btnBuscar").on("click", function () {

        //es nueva la busqueda no resguardamos la busqueda anterior. es util para paginado
        $("#divpanel01").empty();
        dataBak = "";
        //es una busqueda por filtro. siempre sera pagina 1
        pagina = 1;
        buscarPlanCuenta(pagina);
    });
    //callback para que funcione la paginación
    funcCallBack = buscarPlanCuenta;

    $(".inputEditable").on("keypress", analizaEnterInput);

    $("#BtnLiTab01").on("click", function () {
        tabAbm = 1;
        //activarGrilla(Grids.GridUser);

        if (EntidadSelect !== "" && regSelected !== "") {
            activarBotones(true);
        }
    });

    InicializaPantallaPlanCuenta();
    $("#btnFiltro").trigger("click");
});

function manejarDetalle() {
    // Verifica si el divDetalle está visible (tiene la clase "show")
    if ($("#divDetalle").hasClass("show")) {
        console.log("Colapsando #divDetalle...");
        $("#divDetalle").collapse("hide");
        $("#divFiltro").collapse("show");
    } else {
        console.log("Mostrando #divFiltro...");
        $("#divFiltro").collapse("hide");
    }

    // Limpia el contenido del divCuentaData
    console.log("Limpiando contenido de #divCuentaData...");
    $("#divCuentaData").empty();

    // Deshabilita el botón para evitar múltiples clics
    console.log("Deshabilitando botón #btnDetalle...");
    $("#btnDetalle").prop("disabled", true);
}

function analizaEstadoBtnDetalle() {
    InicializaPantallaPlanCuenta();

    $("#btnDetalle").prop("disabled", true);
    $("#btnFiltro").trigger("click");
}

function InicializaPantallaPlanCuenta() {
    $("#divpanel01").jstree("destroy").empty();
    manejarDetalle();
    // INICIALIZAMOS el id del elemento para verificar lo antes del ABM
    EntidadSelect = "";
    activarBotones(false);

    CerrarWaiting();
    return true;
}

function buscarPlanCuenta(pagina) {
    AbrirWaiting("Espere un momento, mientras se trae el Plan de Cuenta solicitado...");
    //desactivamos los botones de acción
    activarBotones(false);


    var buscar = $("#Buscar").val();
    var id = $("#Id").val();
    var id2 = $("#Id2").val();
    var tipo = $("#Tipo option:selected").val();


    var data = {
        id, id2,
        buscar,
        tipo
    };

    PostGen(data, buscarUrl, function (obj) {
        if (obj.error === true) {
            $("#divpanel01").html(obj.msg);
        }
        else {
            let jsonP = $.parseJSON(obj.arbol);

            // Preprocesamos para asignar clases según tipo y cuenta
            function procesarNodos(nodos) {
                nodos.forEach(nodo => {
                    const tipo = nodo.data?.tipo;
                    const cuenta = nodo.data?.cuenta?.toLowerCase();

                    // Asignamos el tipo del nodo para que tome el ícono
                    nodo.type = cuenta || "default";

                    nodo.a_attr = nodo.a_attr || {};
                    let clases = [];

                    if (tipo === "M") clases.push("tipo-m");
                    if (cuenta) clases.push("cuenta-" + cuenta);

                    nodo.a_attr.class = clases.join(" ");

                    if (nodo.children && nodo.children.length > 0) {
                        procesarNodos(nodo.children);
                    }
                });
            }

            procesarNodos(jsonP);

            // Armamos el árbol
            $("#divpanel01").jstree("destroy").empty();
            $("#divpanel01").jstree({
                "core": {
                    "data": jsonP
                },
                "types": {
                    "activo": {
                        "icon": "bx bx-wallet"
                    },
                    "pasivo": {
                        "icon": "bx bx-trending-down"
                    },
                    "patrimonio": {
                        "icon": "bx bx-building-house"
                    },
                    "ingresos": {
                        "icon": "bx bx-dollar-circle"
                    },
                    "egresos": {
                        "icon": "bx bx-print-dollar"
                    },
                    "default": {
                        "icon": "bx bx-folder"
                    }
                },
                "plugins": ["types"]
            });


            // Adjuntamos el evento select_node.jstree al árbol
            $("#divpanel01").on("select_node.jstree", function (e, data) {
                // Capturamos el ID del nodo seleccionado
                var nodoId = data.node.id;
                
                console.log("Nodo seleccionado con ID:", nodoId);
               
                // Realizamos la llamada AJAX con PostGen
                var requestData = { dato: nodoId };
                
                AbrirWaiting("Buscando datos de la cuenta...");
                PostGen(requestData, buscarCuentaUrl, function (obj) {
                    CerrarWaiting();
                    if (obj.error === true) {
                        $("#divCuentaData").html(obj.msg);
                    }
                    else if (obj.warn === true) {
                        if (obj.auth === true) {
                            AbrirMensaje("ATENCIÓN", obj.msg, function () {
                                window.location.href = login;
                            }, false, ["Aceptar"], "warn!", null);
                        }
                        else {
                            $("#divCuentaData").html(obj.msg);
                        }
                    } else {
                        // resguardamos el id del elemento para verificar lo antes del ABM
                        EntidadSelect = obj.entidad;
                        //activar botones de acción
                        activarBotones(true);
                        $("#btnDetalle").prop("disabled", false);
                        $("#divCuentaData").html(obj.cuenta);
                    }                  
                });
            });

            
            $("#divFiltro").collapse("hide");
            $("#divDetalle").collapse("show");

            
        }
        CerrarWaiting();
    }, function (obj) {
        ControlaMensajeError(obj.message);
        CerrarWaiting();
    });
}

