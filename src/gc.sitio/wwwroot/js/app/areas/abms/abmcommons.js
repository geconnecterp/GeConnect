﻿/**
 * permite activar los botones dependiendo de la operacion a ejecutar
 * @param {any} activar
 * @param {any} dejarCancelar: en algunos casos sera necesario dejar el boton cancelar para borrar una consulta visual.
 */
function activarBotones(activar, dejarCancelar = false) {
    if (activar === true) {
        //el activarlos es activar BM
        $("#btnAbmNuevo").prop("disabled", false);
        $("#btnAbmModif").prop("disabled", false);
        $("#btnAbmElimi").prop("disabled", false);


        $("#btnAbmAceptar").prop("disabled", true);
        $("#btnAbmAceptar").hide();

        $("#btnAbmCancelar").prop("disabled", true);
        $("#btnAbmCancelar").hide();

    }
    else {
        $("#btnAbmNuevo").prop("disabled", false);
        $("#btnAbmModif").prop("disabled", true);
        $("#btnAbmElimi").prop("disabled", true);

        $("#btnAbmAceptar").prop("disabled", true);
       
        $("#btnAbmAceptar").hide();
    }

    if (!dejarCancelar) {
        $("#btnAbmCancelar").prop("disabled", true);
        $("#btnAbmCancelar").hide();
    }
    else {
        $("#btnAbmCancelar").prop("disabled", false);
        $("#btnAbmCancelar").show();
    }
}

function activarBotones2(activar) {
    if (activar === true) {
        //el activarlos es activar BM
        $("#btnAbmModif").prop("disabled", false);
        if (tabAbm === 1) {
            $("#btnAbmNuevo").prop("disabled", false);
            $("#btnAbmElimi").prop("disabled", false);
        }
        else {
            $("#btnAbmNuevo").prop("disabled", true);
            $("#btnAbmElimi").prop("disabled", true);
        }

        $("#btnAbmAceptar").prop("disabled", true);
        $("#btnAbmCancelar").prop("disabled", true);
        $("#btnAbmAceptar").hide();
        $("#btnAbmCancelar").hide();
    }
    else {
        $("#btnAbmNuevo").prop("disabled", false);
        $("#btnAbmModif").prop("disabled", true);
        $("#btnAbmElimi").prop("disabled", true);

        $("#btnAbmAceptar").prop("disabled", true);
        $("#btnAbmCancelar").prop("disabled", true);
        $("#btnAbmAceptar").hide();
        $("#btnAbmCancelar").hide();
    }
}


function ejecutaDblClickGrid(x, grid) {
    AbrirWaiting("Espere mientras se busca el elementos seleccionado...");
    selectRegDbl(x, grid);
}

function BuscarFormaDePagoTabClick() {
    if ($(".nav-link").prop("disabled")) {
        return false;
    }
    BuscarFormaDePago();
}

//function posicionarRegOnTop(x) {
//	rowOffset = 0;
//	posActScrollTop = 0;
//	newPosScrollTop = 0

//	posTabla = $(".table-wrapper");
//	//calculamos la posicion del offset del registro seleccionado
//	rowOffset = x.position().top;
//	//posición actual del scroll
//	posActScrollTop = posTabla.scrollTop();
//	//calculamos la nueva posición del scroll
//	newPosScrollTop = rowOffset + posActScrollTop - posTabla.position().top;
//	posTabla.animate({
//		scrollTop: newPosScrollTop
//	}, 500);
//}

function BuscarFormaDePago() {
    if (ctaId != "") {
        var data = { ctaId };
        AbrirWaiting();
        PostGenHtml(data, buscarFormaDePagoUrl, function (obj) {
            $("#divFormasDePago").html(obj);
            AgregarHandlerSelectedRow("tbClienteFormaPagoEnTab");
            $(".activable").prop("disabled", true);
            $("#IdSelected").val("");
            CerrarWaiting();
        }, function (obj) {
            ControlaMensajeError(obj.message);
            CerrarWaiting();
        });
    }
}

function BuscarObservacionesTabClick() {
    if ($(".nav-link").prop("disabled")) {
        return false;
    }
    BuscarObservaciones();
}

function BuscarObservaciones() {
    if (ctaId != "") {
        var data = { ctaId };
        AbrirWaiting();
        PostGenHtml(data, buscarObservacionesUrl, function (obj) {
            $("#divObservaciones").html(obj);
            AgregarHandlerSelectedRow("tbClienteObservaciones");
            $(".activable").prop("disabled", true);
            $("#IdSelected").val("");
            CerrarWaiting();
        }, function (obj) {
            ControlaMensajeError(obj.message);
            CerrarWaiting();
        });
    }
}

function BuscarNotasTabClick() {
    if ($(".nav-link").prop("disabled")) {
        return false;
    }
    BuscarNotas();
}

function BuscarNotas() {
    if (ctaId != "") {
        var data = { ctaId };
        AbrirWaiting();
        PostGenHtml(data, buscarNotasUrl, function (obj) {
            $("#divNotas").html(obj);
            AgregarHandlerSelectedRow("tbClienteNotas");
            $(".activable").prop("disabled", true);
            $("#IdSelected").val("");
            CerrarWaiting();
        }, function (obj) {
            ControlaMensajeError(obj.message);
            CerrarWaiting();
        });
    }
}

function BuscarOtrosContactosTabClick() {
    if ($(".nav-link").prop("disabled")) {
        return false;
    }
    BuscarOtrosContactos();
}

function BuscarOtrosContactos() {
    if (ctaId != "") {
        var data = { ctaId };
        AbrirWaiting();
        PostGenHtml(data, buscarOtrosContactosUrl, function (obj) {
            $("#divOtrosContactos").html(obj);
            AgregarHandlerSelectedRow("tbClienteOtroContacto");
            $(".activable").prop("disabled", true);
            $("#IdSelected").val("");
            CerrarWaiting();
        }, function (obj) {
            ControlaMensajeError(obj.message);
            CerrarWaiting();
        });
    }
}

function LimpiarCampos(tabAct) {
    var data = {};
    switch (tabAct) {
        case Tabs.TabCliente:

            break;
        case Tabs.TabProveedor:
            break;
        case Tabs.TabSector:
            break;
        case Tabs.TabMedioDePago:
            break;
        case Tabs.TabFormasDePago:
            PostGenHtml(data, nuevaFormaDePagoUrl, function (obj) {
                $("#divDatosDeFPSelected").html(obj);
                $(".activable").prop("disabled", true);
            }, function (obj) {
                ControlaMensajeError(obj.message);
            });
            break;
        case Tabs.TabNotas:
            PostGenHtml(data, nuevaNotaUrl, function (obj) {
                $("#divDatosDeNotaSelected").html(obj);
                $(".activable").prop("disabled", true);
            }, function (obj) {
                ControlaMensajeError(obj.message);
            });
            break;
        case Tabs.TabObservaciones:
            PostGenHtml(data, nuevaObservacionUrl, function (obj) {
                $("#divDatosDeObsSelected").html(obj);
                $(".activable").prop("disabled", true);
            }, function (obj) {
                ControlaMensajeError(obj.message);
            });
            break;
        case Tabs.TabOtrosContactos:
            PostGenHtml(data, nuevoContactoUrl, function (obj) {
                $("#divDatosDeOCSelected").html(obj);
                $(".activable").prop("disabled", true);
            }, function (obj) {
                ControlaMensajeError(obj.message);
            });
            break;
        case Tabs.TabFamilias:
            PostGenHtml(data, nuevaFamiliaUrl, function (obj) {
                $("#divDatosDeFamiliaSelected").html(obj);
                $(".activable").prop("disabled", true);
            }, function (obj) {
                ControlaMensajeError(obj.message);
            });
            break;
        case Tabs.TabSubSector:
            PostGenHtml(data, nuevoSubSectorUrl, function (obj) {
                $("#divDatosDeFamiliaSelected").html(obj);
                $(".activable").prop("disabled", true);
            }, function (obj) {
                ControlaMensajeError(obj.message);
            });
            break;
        case Tabs.TabRubro:
            PostGenHtml(data, nuevoRubroUrl, function (obj) {
                $("#divDatosDeFamiliaSelected").html(obj);
                $(".activable").prop("disabled", true);
            }, function (obj) {
                ControlaMensajeError(obj.message);
            });
            break;
        case Tabs.TabOpcionesCuota:
            insId = $("#MedioDePago_Ins_Id").val();
            if (insId === "") {
                insId = $("#IdSelected").val();
            }
            var data = { insId };
            PostGenHtml(data, nuevaOpcionCuotaUrl, function (obj) {
                $("#divOpcionesCuotasSelected").html(obj);
                $(".activable").prop("disabled", true);
            }, function (obj) {
                ControlaMensajeError(obj.message);
            });
            break;
        case Tabs.TabCuentaFinYContable:
            PostGenHtml(data, nuevaCuentaFinYContableUrl, function (obj) {
                $("#divCuentaFinYContableSelected").html(obj);
                $(".activable").prop("disabled", true);
            }, function (obj) {
                ControlaMensajeError(obj.message);
            });
            break;
        case Tabs.TabPos:
            PostGenHtml(data, nuevaPosUrl, function (obj) {
                $("#divPos").html(obj);
                $(".activable").prop("disabled", true);
            }, function (obj) {
                ControlaMensajeError(obj.message);
            });
            break;
        default:
            break;
    }
}

function NuevaFormaDePago() {
    var tabActiva = $('.nav-tabs .active')[0].id;
    var mensaje = PuedoAgregar(tabActiva);
    if (mensaje !== "") {
        AbrirMensaje("ATENCIÓN", mensaje, function () {
            $("#msjModal").modal("hide");
            return true;
        }, false, ["Aceptar"], "error!", null);
    }
    else {
        var data = {};
        PostGenHtml(data, nuevaFormaDePagoUrl, function (obj) {
            $("#divDatosDeFPSelected").html(obj);
            $(".nav-link").prop("disabled", true);
            $(".activable").prop("disabled", false);
            desactivarGrilla(Grids.GridFP);
            accionBotones(AbmAction.ALTA, tabActiva);
            $("#listaFP").trigger("focus");
            CerrarWaiting();
        }, function (obj) {
            ControlaMensajeError(obj.message);
            CerrarWaiting();
        });
    }
}

function NuevoContacto() {
    var tabActiva = $('.nav-tabs .active')[0].id;
    var mensaje = PuedoAgregar(tabActiva);
    if (mensaje !== "") {
        AbrirMensaje("ATENCIÓN", mensaje, function () {
            $("#msjModal").modal("hide");
            return true;
        }, false, ["Aceptar"], "error!", null);
    }
    else {
        var data = {};
        PostGenHtml(data, nuevoContactoUrl, function (obj) {
            $("#divDatosDeOCSelected").html(obj);
            $(".nav-link").prop("disabled", true);
            $(".activable").prop("disabled", false);
            desactivarGrilla(Grids.GridOC);
            accionBotones(AbmAction.ALTA, tabActiva);
            $("#OtroContacto_Cta_Nombre").trigger("focus");
            CerrarWaiting();
        }, function (obj) {
            ControlaMensajeError(obj.message);
            CerrarWaiting();
        });
    }
}

function NuevaNota() {
    var tabActiva = $('.nav-tabs .active')[0].id;
    var mensaje = PuedoAgregar(tabActiva);
    if (mensaje !== "") {
        AbrirMensaje("ATENCIÓN", mensaje, function () {
            $("#msjModal").modal("hide");
            return true;
        }, false, ["Aceptar"], "error!", null);
    }
    else {
        var data = {};
        PostGenHtml(data, nuevaNotaUrl, function (obj) {
            $("#divDatosDeNotaSelected").html(obj);
            $(".nav-link").prop("disabled", true);
            $(".activable").prop("disabled", false);
            desactivarGrilla(Grids.GridNota);
            accionBotones(AbmAction.ALTA, tabActiva);
            $("#Nota_Usu_Id").trigger("focus");
            CerrarWaiting();
        }, function (obj) {
            ControlaMensajeError(obj.message);
            CerrarWaiting();
        });
    }
}

function NuevaObservacion() {
    var tabActiva = $('.nav-tabs .active')[0].id;
    var mensaje = PuedoAgregar(tabActiva);
    if (mensaje !== "") {
        AbrirMensaje("ATENCIÓN", mensaje, function () {
            $("#msjModal").modal("hide");
            return true;
        }, false, ["Aceptar"], "error!", null);
    }
    else {
        var data = {};
        PostGenHtml(data, nuevaObservacionUrl, function (obj) {
            $("#divDatosDeObsSelected").html(obj);
            $(".nav-link").prop("disabled", true);
            $(".activable").prop("disabled", false);
            desactivarGrilla(Grids.GridObs);
            accionBotones(AbmAction.ALTA, tabActiva);
            $("#listaTipoObs").trigger("focus");
            CerrarWaiting();
        }, function (obj) {
            ControlaMensajeError(obj.message);
            CerrarWaiting();
        });
    }
}

function PuedoAgregar(tabAct) {
    var mensaje = "";
    switch (tabAct) {
        case Tabs.TabCliente:
            break;
        case Tabs.TabProveedor:
            break;
        case Tabs.TabSector:
            break;
        case Tabs.TabMedioDePago:
            break;
        case Tabs.TabFormasDePago:
            if (!$("#chkCtaActiva").is(":checked"))
                mensaje = "Solo se pueden agregar formas de pago para cuentas activas.";
            break;
        case Tabs.TabNotas:
            if (!$("#chkCtaActiva").is(":checked"))
                mensaje = "Solo se pueden agregar notas para cuentas activas.";
            break;
        case Tabs.TabObservaciones:
            if (!$("#chkCtaActiva").is(":checked"))
                mensaje = "Solo se pueden agregar observaciones para cuentas activas.";
            break;
        case Tabs.TabOtrosContactos:
            if (!$("#chkCtaActiva").is(":checked"))
                mensaje = "Solo se pueden agregar contactos para cuentas activas.";
            break;
        case Tabs.TabFamilias:
            break;
        case Tabs.TabSubSector:
            break;
        case Tabs.TabRubro:
            break;
        case Tabs.TabOpcionesCuota:
            if (!$("#chkInsActivo").is(":checked"))
                mensaje = "Solo se pueden agregar opciones de cuotas para medios de pago activos.";
            break;
        case Tabs.TabCuentaFinYContable:
            if (!$("#chkInsActivo").is(":checked"))
                mensaje = "Solo se pueden agregar cuentas financieras para medios de pago activos.";
            break;
        case Tabs.TabPos:
            if (!$("#chkInsActivo").is(":checked"))
                mensaje = "Solo se pueden agregar pos para medios de pago activos.";
            break;
        default:
            break;
    }
    return mensaje;
}

/**
 * determina segun la accion como se presentaran los botones.
 * @param {any} btn
 * @param {any} tabActiva
 * @param {any} dejarCancelar Para algunos casos es necesario que el boton siempre este visible. para esos casos accionBotones se invocará con (accion, tab si fuera necesario y true)
 */
function accionBotones(btn, tabActiva, dejarCancelar = false) {
    if (btn === AbmAction.ALTA || btn === AbmAction.MODIFICACION || btn === AbmAction.BAJA) {
        $("#btnFiltro").prop("disabled", true);
        $("#btnDetalle").prop("disabled", true);

        $("#btnAbmNuevo").prop("disabled", true);
        $("#btnAbmModif").prop("disabled", true);
        $("#btnAbmElimi").prop("disabled", true);

        $("#btnAbmAceptar").prop("disabled", false);
        $("#btnAbmCancelar").prop("disabled", false);
        $("#btnAbmAceptar").show();
        $("#btnAbmCancelar").show();
    } else if (btn === AbmAction.SUBMIT || btn === AbmAction.CANCEL) {   // (S)uccess - (C)ancel
        $("#btnFiltro").prop("disabled", false);
        $("#btnDetalle").prop("disabled", false);

        if (btn === AbmAction.ALTA) {

        }
        else if (btn === AbmAction.CANCEL) {
            activarBotones(false, dejarCancelar);
            if (tabActiva === Tabs.TabCliente) {
                $("#btnDetalle").prop("disabled", true);
                activarGrilla(Tabs.TabCliente);
            }
            if (tabActiva === Tabs.TabProveedor) {
                $("#btnDetalle").prop("disabled", true);
                activarGrilla(Tabs.TabProveedor);
            }
            if (tabActiva === Tabs.TabSector) {
                $("#btnDetalle").prop("disabled", true);
                activarGrilla(Tabs.TabSector);
            }
            if (tabActiva === Tabs.TabMedioDePago) {
                $("#btnDetalle").prop("disabled", true);
                activarGrilla(Tabs.TabMedioDePago);
            }
        }
    }
}

function AgregarHandlerSelectedRow(grilla) {
    document.getElementById(grilla).addEventListener('click', function (e) {
        if (e.target.nodeName === 'TD') {
            var selectedRow = this.querySelector('.selected-row');
            if (selectedRow) {
                selectedRow.classList.remove('selected-row');
            }
            e.target.closest('tr').classList.add('selected-row');
        }
    });
}

function controlaValorProvi() {
    var provId = $("#listaProvi option:selected").val()
    if (provId !== "") {
        var data = { provId }
        AbrirWaiting();
        PostGenHtml(data, obtenerDepartamentosUrl, function (obj) {
            $("#divLocalidad").html(obj);
            CerrarWaiting();
        }, function (obj) {
            ControlaMensajeError(obj.message);
            CerrarWaiting();
        });
    }
}

function controlaValorAfip() {
    if ($("#listaAfip option:selected").val() !== "05" && $("#listaAfip option:selected").val() !== "02") {
        $("#listaIB").prop("disabled", false);
        $("#Cliente_Cta_Ib_Nro").prop("disabled", false);
        $("#chkPivaCertActiva").prop("disabled", false);
        $("#chkIbCertActiva").prop("disabled", false);
    }
    else {
        $("#listaIB").prop("disabled", true);
        $("#Cliente_Cta_Ib_Nro").prop("disabled", true);
        $("#chkPivaCertActiva").prop("disabled", true);
        $("#chkIbCertActiva").prop("disabled", true);
    }
}

function ModificaFormaDePago(tabAct, mainGrid) {
    var mensaje = PuedoModificar(tabAct);
    if (mensaje !== "") {
        AbrirMensaje("ATENCIÓN", mensaje, function () {
            $("#msjModal").modal("hide");
            return true;
        }, false, ["Aceptar"], "error!", null);
    }
    else {
        accionBotones(AbmAction.MODIFICACION, tabAct);
        tipoDeOperacion = AbmAction.MODIFICACION;
        SetearDestinoDeOperacion(tabAct);
        $(".nav-link").prop("disabled", true);
        $(".activable").prop("disabled", false);
        $("#listaFP").prop("disabled", true);
        desactivarGrilla(Grids.GridFP);
        desactivarGrilla(mainGrid);
        $("#FormaDePago_Fp_Dias").trigger("focus");
    }
}

function ModificaContacto(tabAct, mainGrid) {
    var mensaje = PuedoModificar(tabAct);
    if (mensaje !== "") {
        activarBotones(false);
        AbrirMensaje("ATENCIÓN", mensaje, function () {
            $("#msjModal").modal("hide");
            return true;
        }, false, ["Aceptar"], "error!", null);
    }
    else {
        accionBotones(AbmAction.MODIFICACION, tabAct);
        tipoDeOperacion = AbmAction.MODIFICACION;
        SetearDestinoDeOperacion(tabAct);
        $(".nav-link").prop("disabled", true);
        $(".activable").prop("disabled", false);
        desactivarGrilla(Grids.GridOC);
        desactivarGrilla(mainGrid);
        $("#OtroContacto_Cta_Nombre").prop("disabled", true);
        $("#OtroContacto_Cta_Celu").trigger("focus");
    }
}

function ModificaNota(tabAct, mainGrid) {
    var mensaje = PuedoModificar(tabAct);
    if (mensaje !== "") {
        AbrirMensaje("ATENCIÓN", mensaje, function () {
            $("#msjModal").modal("hide");
            return true;
        }, false, ["Aceptar"], "error!", null);
    }
    else {
        accionBotones(AbmAction.MODIFICACION, tabAct);
        tipoDeOperacion = AbmAction.MODIFICACION;
        SetearDestinoDeOperacion(tabAct);
        $(".nav-link").prop("disabled", true);
        $(".activable").prop("disabled", false);
        $("#Nota_Usu_Id").prop("disabled", true);
        desactivarGrilla(Grids.GridNota);
        desactivarGrilla(mainGrid);
        $("#Nota_Nota").trigger("focus");
    }
}

function ModificaObservacion(tabAct, mainGrid) {
    var mensaje = PuedoModificar(tabAct);
    if (mensaje !== "") {
        AbrirMensaje("ATENCIÓN", mensaje, function () {
            $("#msjModal").modal("hide");
            return true;
        }, false, ["Aceptar"], "error!", null);
    }
    else {
        accionBotones(AbmAction.MODIFICACION, tabAct);
        tipoDeOperacion = AbmAction.MODIFICACION;
        SetearDestinoDeOperacion(tabAct);
        $(".nav-link").prop("disabled", true);
        $(".activable").prop("disabled", false);
        $("#listaTipoObs").prop("disabled", true);
        desactivarGrilla(Grids.GridObs);
        desactivarGrilla(mainGrid);
        $("#Observacion_Cta_Obs").trigger("focus");
    }
}

function PuedoModificar(tabAct) {
    var mensaje = "";
    switch (tabAct) {
        case Tabs.TabCliente:
            break;
        case Tabs.TabProveedor:
            break;
        case Tabs.TabSector:
            break;
        case Tabs.TabMedioDePago:
            break;
        case Tabs.TabFormasDePago:
            if (!$("#chkCtaActiva").is(":checked"))
                mensaje = "Solo se pueden modificar formas de pago para cuentas activas.";
            if ($("#IdSelected").val() == "")
                mensaje = "Debe seleccionar una forma de pago para modificar.";
            break;
        case Tabs.TabNotas:
            if (!$("#chkCtaActiva").is(":checked"))
                mensaje = "Solo se pueden modificar notas para cuentas activas.";
            if ($("#IdSelected").val() == "")
                mensaje = "Debe seleccionar una nota para modificar.";
            break;
        case Tabs.TabObservaciones:
            if (!$("#chkCtaActiva").is(":checked"))
                mensaje = "Solo se pueden modificar observaciones para cuentas activas.";
            if ($("#IdSelected").val() == "")
                mensaje = "Debe seleccionar una observación para modificar.";
            break;
        case Tabs.TabOtrosContactos:
            if (!$("#chkCtaActiva").is(":checked"))
                mensaje = "Solo se pueden modificar contactos para cuentas activas.";
            if ($("#IdSelected").val() == "")
                mensaje = "Debe seleccionar un contacto para modificar.";
            break;
        case Tabs.TabFamilias:
            if (!$("#chkCtaActiva").is(":checked"))
                mensaje = "Solo se pueden modificar familias para cuentas activas.";
            if ($("#IdSelected").val() == "")
                mensaje = "Debe seleccionar una familia para modificar.";
            break;
        case Tabs.TabSubSector:
            if ($("#IdSelected").val() == "")
                mensaje = "Debe seleccionar un Sub Sector para modificar.";
            break;
        case Tabs.TabRubro:
            if ($("#IdSelected").val() == "")
                mensaje = "Debe seleccionar un Rubro para modificar.";
            break;
        case Tabs.TabOpcionesCuota:
            if ($("#IdSelected").val() == "")
                mensaje = "Debe seleccionar una Opción de Cuota para modificar.";
            break;
        case Tabs.TabCuentaFinYContable:
            if ($("#IdSelected").val() == "")
                mensaje = "Debe seleccionar una Cuenta Fin. para modificar.";
            break;
        case Tabs.TabPos:
            if ($("#MedioDePago_Ins_Id").val() == "")
                mensaje = "Debe seleccionar un Medio de Pago antes de modificar Pos.";
            break;
        case Tabs.TabBanco:
            break;
        default:
            break;
    }
    return mensaje;
}

function GetMensajeParaBaja(tabActiva) {
    var mensaje = "";
    switch (tabActiva) {
        case Tabs.TabCliente:
            mensaje = "Para eliminar primero debe seleccionar un Cliente."
            break;
        case Tabs.TabProveedor:
            mensaje = "Para eliminar primero debe seleccionar un Proveedor."
            break;
        case Tabs.TabSector:
            mensaje = "Para eliminar primero debe seleccionar un Sector."
            break;
        case Tabs.TabMedioDePago:
            mensaje = "Para eliminar primero debe seleccionar un Medio de Pago."
            break;
        case Tabs.TabFormasDePago:
            mensaje = "Para eliminar primero debe seleccionar una Forma de Pago."
            break;
        case Tabs.TabNotas:
            mensaje = "Para eliminar primero debe seleccionar una Nota."
            break;
        case Tabs.TabObservaciones:
            mensaje = "Para eliminar primero debe seleccionar una Observación."
            break;
        case Tabs.TabOtrosContactos:
            mensaje = "Para eliminar primero debe seleccionar un Contacto."
            break;
        case Tabs.TabFamilias:
            mensaje = "Para eliminar primero debe seleccionar una Familia."
            break;
        case Tabs.TabSubSector:
            mensaje = "Para eliminar primero debe seleccionar un Sub Sector."
            break;
        case Tabs.TabRubro:
            mensaje = "Para eliminar primero debe seleccionar un Rubro."
            break;
        case Tabs.TabOpcionesCuota:
            mensaje = "Para eliminar primero debe seleccionar una Opcion de Cuota."
            break;
        case Tabs.TabCuentaFinYContable:
            mensaje = "Para eliminar primero debe seleccionar una Cuenta Financiera y Contable."
            break;
        case Tabs.TabPos:
            mensaje = "Para eliminar primero debe seleccionar un Pos."
            break;
        case Tabs.TabBanco:
            mensaje = "Para eliminar primero debe seleccionar un Banco."
            break;
        case Tabs.TabCuentaDirecta:
            mensaje = "Para eliminar primero debe seleccionar una Cuenta."
            break;
        default:
            mensaje = "";
            break;
    }
    return mensaje;
}

function SetearDestinoDeOperacion(tabActiva) {
    switch (tabActiva) {
        case Tabs.TabCliente:
            destinoDeOperacion = AbmObject.CLIENTES;
            break;
        case Tabs.TabProveedor:
            destinoDeOperacion = AbmObject.PROVEEDORES;
            break;
        case Tabs.TabSector:
            destinoDeOperacion = AbmObject.SECTORES;
            break;
        case Tabs.TabMedioDePago:
            destinoDeOperacion = AbmObject.MEDIO_DE_PAGO;
            break;
        case Tabs.TabFormasDePago:
            destinoDeOperacion = AbmObject.CLIENTES_CONDICIONES_VTA;
            break;
        case Tabs.TabNotas:
            destinoDeOperacion = AbmObject.CUENTAS_NOTAS;
            break;
        case Tabs.TabObservaciones:
            destinoDeOperacion = AbmObject.CUENTAS_OBSERVACIONES;
            break;
        case Tabs.TabOtrosContactos:
            destinoDeOperacion = AbmObject.CUENTAS_CONTACTOS;
            break;
        case Tabs.TabFamilias:
            destinoDeOperacion = AbmObject.PROVEEDORES_FAMILIA;
            break;
        case Tabs.TabSubSector:
            destinoDeOperacion = AbmObject.SUB_SECTORES;
            break;
        case Tabs.TabRubro:
            destinoDeOperacion = AbmObject.RUBROS;
            break;
        case Tabs.TabOpcionesCuota:
            destinoDeOperacion = AbmObject.OPCIONES_CUOTA;
            break;
        case Tabs.TabCuentaFinYContable:
            destinoDeOperacion = AbmObject.CUENTA_FIN_CONTABLE;
            break;
        case Tabs.TabPos:
            destinoDeOperacion = AbmObject.POS;
            break;
        case Tabs.TabBanco:
            destinoDeOperacion = AbmObject.BANCOS;
            break;
        case Tabs.TabCuentaDirecta:
            destinoDeOperacion = AbmObject.CUENTAS_DIRECTAS;
            break;
        default:
            destinoDeOperacion = "";
            break;
    }
}

function btnNuevoClick() {
    tipoDeOperacion = AbmAction.ALTA;
    var tabActiva = $('.nav-tabs .active')[0].id;
    SetearDestinoDeOperacion(tabActiva);
    switch (tabActiva) {
        case Tabs.TabCliente:
            NuevoCliente();
            break;
        case Tabs.TabProveedor:
            NuevoProveedor();
            break;
        case Tabs.TabSector:
            NuevoSector();
            break;
        case Tabs.TabMedioDePago:
            NuevoMedioDePago();
            break;
        case Tabs.TabFormasDePago:
            NuevaFormaDePago();
            break;
        case Tabs.TabNotas:
            NuevaNota();
            break;
        case Tabs.TabObservaciones:
            NuevaObservacion();
            break;
        case Tabs.TabOtrosContactos:
            NuevoContacto();
            break;
        case Tabs.TabFamilias:
            NuevaFamilia();
            break;
        case Tabs.TabSubSector:
            NuevoSubSector();
            break;
        case Tabs.TabRubro:
            NuevoRubro();
            break;
        case Tabs.TabOpcionesCuota:
            NuevaOpcionCuota();
            break;
        case Tabs.TabCuentaFinYContable:
            NuevaCuentaFinYContable();
            break;
        case Tabs.TabPos:
            NuevaPos();
            break;
        case Tabs.TabBanco:
            NuevoBanco();
            break;
        case Tabs.TabCuentaDirecta:
            NuevaCuentaDirecta();
            break;
        default:
            break;
    }
}

function btnModiClick() {
    var tabActiva = $('.nav-tabs .active')[0].id;
    switch (tabActiva) {
        case Tabs.TabCliente:
            ModificaCliente(tabActiva);
            break;
        case Tabs.TabProveedor:
            ModificaProveedor(tabActiva);
            break;
        case Tabs.TabSector:
            ModificaSector(tabActiva);
            break;
        case Tabs.TabMedioDePago:
            ModificaMedioDePago(tabActiva);
            break;
        case Tabs.TabFormasDePago:
            ModificaFormaDePago(tabActiva, Grids.GridProveedor);
            break;
        case Tabs.TabNotas:
            ModificaNota(tabActiva, Grids.GridProveedor);
            break;
        case Tabs.TabObservaciones:
            ModificaObservacion(tabActiva, Grids.GridProveedor);
            break;
        case Tabs.TabOtrosContactos:
            ModificaContacto(tabActiva, Grids.GridProveedor);
            break;
        case Tabs.TabFamilias:
            ModificaFamilia(tabActiva, Grids.GridProveedor);
            break;
        case Tabs.TabSubSector:
            ModificaSubSector(tabActiva, Grids.GridSector);
            break;
        case Tabs.TabRubro:
            ModificaRubro(tabActiva, Grids.GridSector);
            break;
        case Tabs.TabOpcionesCuota:
            ModificaOpcionesCuota(tabActiva, Grids.GridMedioDePago);
            break;
        case Tabs.TabCuentaFinYContable:
            ModificaCuentaFinYContable(tabActiva, Grids.GridMedioDePago);
            break;
        case Tabs.TabPos:
            ModificaPos(tabActiva, Grids.GridMedioDePago);
            break;
        case Tabs.TabBanco:
            ModificaBanco(tabActiva, Grids.GridBanco);
            break;
        case Tabs.TabCuentaDirecta:
            ModificaCuentaDirecta(tabActiva);
            break;
        default:
            break;
    }
}

function btnBajaClick() {
    var tabActiva = $('.nav-tabs .active')[0].id;
    var idSelected = "";
    if (tabActiva == Tabs.TabCliente) {
        idSelected = $("#Cliente_Cta_id").val();
    }
    else if (tabActiva == Tabs.TabProveedor) {
        idSelected = $("#Proveedor_Cta_Id").val();
    }
    else if (tabActiva == Tabs.TabSector) {
        idSelected = $("#Sector_Sec_Id").val();
    }
    else if (tabActiva == Tabs.TabMedioDePago) {
        idSelected = $("#MedioDePago_Ins_Id").val();
    }
    else if (tabActiva == Tabs.TabBanco) {
        idSelected = $("#Banco_Ctaf_Id").val();
    }
    else if (tabActiva == Tabs.TabCuentaDirecta) {
        idSelected = $("#CuentaDirecta_Ctag_Id").val();
    }
    else {
        idSelected = $("#IdSelected").val();
    }
    if (idSelected === "") {
        AbrirMensaje("ATENCIÓN", GetMensajeParaBaja(tabActiva), function () {
            $("#msjModal").modal("hide");
            return false;
        }, false, ["Aceptar"], "error!", null);

    }
    else {
        var tabActiva = $('.nav-tabs .active')[0].id;
        accionBotones(AbmAction.BAJA, tabActiva);
        tipoDeOperacion = AbmAction.BAJA;
        $(".activable").prop("disabled", true);
        SetearDestinoDeOperacion(tabActiva);
        $(".nav-link").prop("disabled", true);
        switch (tabActiva) {
            case Tabs.TabCliente:
                desactivarGrilla(Grids.GridCliente);
                break;
            case Tabs.TabProveedor:
                desactivarGrilla(Grids.GridProveedor);
                break;
            case Tabs.TabSector:
                desactivarGrilla(Grids.GridSector);
                break;
            case Tabs.TabMedioDePago:
                desactivarGrilla(Grids.GridMedioDePago);
                break;
            case Tabs.TabFormasDePago:
                desactivarGrilla(Grids.GridProveedor);
                desactivarGrilla(Grids.GridFP);
                break;
            case Tabs.TabNotas:
                desactivarGrilla(Grids.GridProveedor);
                desactivarGrilla(Grids.GridNota);
                break;
            case Tabs.TabObservaciones:
                desactivarGrilla(Grids.GridProveedor);
                desactivarGrilla(Grids.GridObs);
                break;
            case Tabs.TabOtrosContactos:
                desactivarGrilla(Grids.GridProveedor);
                desactivarGrilla(Grids.GridOC);
                break;
            case Tabs.TabFamilias:
                desactivarGrilla(Grids.GridProveedor);
                desactivarGrilla(Grids.GridFlias);
                break;
            case Tabs.TabSubSector:
                desactivarGrilla(Grids.GridSector);
                desactivarGrilla(Grids.GridSubSector);
                break;
            case Tabs.TabRubro:
                desactivarGrilla(Grids.GridSector);
                desactivarGrilla(Grids.GridRubro);
                break;
            case Tabs.TabOpcionesCuota:
                desactivarGrilla(Grids.GridMedioDePago);
                desactivarGrilla(Grids.GridOpcionesCuotas);
                break;
            case Tabs.TabCuentaFinYContable:
                desactivarGrilla(Grids.GridMedioDePago);
                desactivarGrilla(Grids.GridCuentaFinYConta);
                break;
            case Tabs.TabPos:
                desactivarGrilla(Grids.GridMedioDePago);
                desactivarGrilla(Grids.GridPos);
                break;
            case Tabs.TabBanco:
                desactivarGrilla(Grids.GridBanco);
                break;
            case Tabs.TabCuentaDirecta:
                desactivarGrilla(Grids.GridCuentaDirecta);
                break;
            default:
                break;
        }
    }
}

function btnCancelClick() {
    tipoDeOperacion = AbmAction.CANCEL;
    $(".nav-link").prop("disabled", false);
    $(".activable").prop("disabled", true);
    var tabActiva = $('.nav-tabs .active')[0].id;
    accionBotones(AbmAction.CANCEL, tabActiva);
    switch (tabActiva) {
        case Tabs.TabCliente:
            if ($("#divDetalle").is(":visible")) {
                $("#divDetalle").collapse("hide");
            }
            tb = $("#" + Grids.GridCliente + " tbody tr");
            if (tb.length === 0) {
                $("#divFiltro").collapse("show");
            }
            activarGrilla(Grids.GridCliente);
            QuitarElementoSeleccionado(Grids.GridCliente);
            break;
        case Tabs.TabProveedor:
            if ($("#divDetalle").is(":visible")) {
                $("#divDetalle").collapse("hide");
            }
            tb = $("#" + Grids.GridProveedor + " tbody tr");
            if (tb.length === 0) {
                $("#divFiltro").collapse("show");
            }
            activarGrilla(Grids.GridProveedor);
            QuitarElementoSeleccionado(Grids.GridProveedor);
            break;
        case Tabs.TabSector:
            if ($("#divDetalle").is(":visible")) {
                $("#divDetalle").collapse("hide");
            }
            tb = $("#" + Grids.GridSector + " tbody tr");
            if (tb.length === 0) {
                $("#divFiltro").collapse("show");
            }
            activarGrilla(Grids.GridSector);
            QuitarElementoSeleccionado(Grids.GridSector);
            break;
        case Tabs.TabMedioDePago:
            if ($("#divDetalle").is(":visible")) {
                $("#divDetalle").collapse("hide");
            }
            tb = $("#" + Grids.GridMedioDePago + " tbody tr");
            if (tb.length === 0) {
                $("#divFiltro").collapse("show");
            }
            activarGrilla(Grids.GridMedioDePago);
            QuitarElementoSeleccionado(Grids.GridMedioDePago);
            break;
        case Tabs.TabFormasDePago:
            LimpiarCampos(tabActiva);
            $("#tbClienteFormaPagoEnTab").prop("disabled", false);
            activarGrilla(Grids.GridFP);
            activarGrilla(Grids.GridCliente);
            selectReg(regSelected, Grids.GridFP);
            break;
        case Tabs.TabNotas:
            LimpiarCampos(tabActiva);
            $("#Nota_Usu_Apellidoynombre").prop("disabled", false);
            activarGrilla(Grids.GridNota);
            activarGrilla(Grids.GridCliente);
            selectReg(regSelected, Grids.GridNota);
            break;
        case Tabs.TabObservaciones:
            LimpiarCampos(tabActiva);
            activarGrilla(Grids.GridObs);
            activarGrilla(Grids.GridCliente);
            selectReg(regSelected, Grids.GridObs);
            break;
        case Tabs.TabOtrosContactos:
            LimpiarCampos(tabActiva);
            activarGrilla(Grids.GridOC);
            activarGrilla(Grids.GridCliente);
            selectReg(regSelected, Grids.GridOC);
            $("#OtroContacto_Cta_Nombre").prop("disabled", false);
            $("#listaTC").prop("disabled", false);
            break;
        case Tabs.TabFamilias:
            LimpiarCampos(tabActiva);
            activarGrilla(Grids.GridFlias);
            activarGrilla(Grids.GridProveedor);
            selectReg(regSelected, Grids.GridFlias);
            break;
        case Tabs.TabSubSector:
            LimpiarCampos(tabActiva);
            activarGrilla(Grids.GridSubSector);
            activarGrilla(Grids.GridSector);
            selectReg(regSelected, Grids.GridSubSector);
            break;
        case Tabs.TabRubro:
            LimpiarCampos(tabActiva);
            activarGrilla(Grids.GridRubro);
            activarGrilla(Grids.GridSector);
            selectReg(regSelected, Grids.GridRubro);
            break;
        case Tabs.TabOpcionesCuota:
            LimpiarCampos(tabActiva);
            activarGrilla(Grids.GridOpcionesCuotas);
            activarGrilla(Grids.GridMedioDePago);
            selectReg(regSelected, Grids.GridOpcionesCuotas);
            break;
        case Tabs.TabCuentaFinYContable:
            LimpiarCampos(tabActiva);
            activarGrilla(Grids.GridCuentaFinYConta);
            activarGrilla(Grids.GridMedioDePago);
            selectReg(regSelected, Grids.GridCuentaFinYConta);
            break;
        case Tabs.TabPos:
            LimpiarCampos(tabActiva);
            activarGrilla(Grids.GridMedioDePago);
            break;
        case Tabs.TabBanco:
            if ($("#divDetalle").is(":visible")) {
                $("#divDetalle").collapse("hide");
            }
            tb = $("#" + Grids.GridBanco + " tbody tr");
            if (tb.length === 0) {
                $("#divFiltro").collapse("show");
            }
            activarGrilla(Grids.GridBanco);
            QuitarElementoSeleccionado(Grids.GridBanco);
            break;
        case Tabs.TabCuentaDirecta:
            if ($("#divDetalle").is(":visible")) {
                $("#divDetalle").collapse("hide");
            }
            tb = $("#" + Grids.GridCuentaDirecta + " tbody tr");
            if (tb.length === 0) {
                $("#divFiltro").collapse("show");
            }
            activarGrilla(Grids.GridCuentaDirecta);
            QuitarElementoSeleccionado(Grids.GridCuentaDirecta);
            break;
        default:
            break;
    }
}

function QuitarElementoSeleccionado(grilla) {
    $("#" + grilla + " tbody tr").each(function (index) {
        $(this).removeClass("selectedEdit-row");
    });
    regSelected = null;
}

function selectRegCli(e, gridId) {
    selectReg(e, gridId);
    $("#IdSelected").val("");
    switch (gridId) {
        case Grids.GridProveedor:
            if ($("#divDetalle").is(":visible")) {
                $("#divDetalle").collapse("hide");
            }
            $("#btnDetalle").prop("disabled", true);
            activarGrilla(Grids.GridCliente);
            activarBotones(false);
            break;
        case Grids.GridCliente:
            if ($("#divDetalle").is(":visible")) {
                $("#divDetalle").collapse("hide");
            }
            $("#btnDetalle").prop("disabled", true);
            activarGrilla(Grids.GridCliente);
            activarBotones(false);
            break;
        case Grids.GridSector:
            if ($("#divDetalle").is(":visible")) {
                $("#divDetalle").collapse("hide");
            }
            $("#btnDetalle").prop("disabled", true);
            activarGrilla(Grids.GridSector);
            activarBotones(false);
            break;
        case Grids.GridFP:
            break;
        case Grids.GridOC:
            break;
        case Grids.GridNota:
            break;
        case Grids.GridObs:
            break;
        case Grids.GridFlias:
            break;
        case Grids.GridSubSector:
            break;
        case Grids.GridRubro:
            break;
        case Grids.GridBanco:
            if ($("#divDetalle").is(":visible")) {
                $("#divDetalle").collapse("hide");
            }
            $("#btnDetalle").prop("disabled", true);
            activarGrilla(Grids.GridBanco);
            activarBotones(false);
            break;
        case Grids.GridCuentaDirecta:
            if ($("#divDetalle").is(":visible")) {
                $("#divDetalle").collapse("hide");
            }
            $("#btnDetalle").prop("disabled", true);
            activarGrilla(Grids.GridCuentaDirecta);
            activarBotones(false);
            break;
        default:
    }
}

function PuedoGuardar(tabAct) {
    var mensaje = "";
    switch (tabAct) {
        case Tabs.TabCliente:
            break;
        case Tabs.TabProveedor:
            break;
        case Tabs.TabSector:
            break;
        case Tabs.TabMedioDePago:
            break;
        case Tabs.TabFormasDePago:
            if ($("#FormaDePago_Fp_Dias").val() < 0)
                mensaje = "El valor de Plazo debe ser igual o mayor a 0.";
            break;
        case Tabs.TabNotas:
            break;
        case Tabs.TabObservaciones:
            break;
        case Tabs.TabOtrosContactos:
            break;
        case Tabs.TabFamilias:
            break;
        case Tabs.TabSubSector:
            break;
        case Tabs.TabRubro:
            break;
        case Tabs.TabOpcionesCuota:
            break;
        case Tabs.TabCuentaFinYContable:
            break;
        case Tabs.TabPos:
            break;
        case Tabs.TabBanco:
            break;
        case Tabs.TabCuentaDirecta:
            break;
        default:
            break;
    }
    return mensaje;
    return true;
}

function validarCampos() {
    var tabActiva = $('.nav-tabs .active')[0].id;
    switch (tabActiva) {
        case Tabs.TabCliente:
            if ($("#listaAfip option:selected").val() !== "05" && $("#listaAfip option:selected").val() !== "02") {
                if ($("#Cliente_Ctac_Ptos_Vtas").val() <= 0) {
                    var textoSelected = $("#listaAfip option:selected").text();
                    AbrirMensaje("ATENCIÓN", "La Cantidad de PV debe ser mayor a 0 cuando ha seleccionado " + textoSelected + " como Condición AFIP.", function () {
                        $("#msjModal").modal("hide");
                        return false;
                    }, false, ["Aceptar"], "error!", null);
                    return false;
                }
            }
            break;
        case Tabs.TabProveedor:
            break;
        case Tabs.TabSector:
            break;
        case Tabs.TabMedioDePago:
            break;
        case Tabs.TabFormasDePago:
            break;
        case Tabs.TabNotas:
            break;
        case Tabs.TabObservaciones:
            break;
        case Tabs.TabOtrosContactos:
            break;
        case Tabs.TabFamilias:
            break;
        case Tabs.TabSubSector:
            break;
        case Tabs.TabRubro:
            break;
        case Tabs.TabOpcionesCuota:
            break;
        case Tabs.TabCuentaFinYContable:
            break;
        case Tabs.TabPos:
            break;
        case Tabs.TabBanco:
            break;
        case Tabs.TabCuentaDirecta:
            break;
        default:
            break;
    }

    return true;
}

function ActualizarRegistroEnGrilla(datos, grid) {
    if (grid === Grids.GridCliente || grid === Grids.GridProveedor) {
        $("#" + grid + " tbody tr").each(function (index) {
            let aux = $(this)[0].cells[0].innerText;
            if (aux == datos.cta_id) {
                $(this)[0].cells[1].innerText = datos.cta_denominacion;
                $(this)[0].cells[2].innerText = datos.tdoc_desc;
                $(this)[0].cells[3].innerText = datos.cta_documento;
                $(this)[0].cells[4].innerText = datos.cta_domicilio;
            }
        });
    }
    if (grid === Grids.GridSector) {
        $("#" + grid + " tbody tr").each(function (index) {
            let aux = $(this)[0].cells[0].innerText;
            if (aux == datos.sec_id) {
                $(this)[0].cells[1].innerText = datos.sec_desc;
            }
        });
    }
    if (grid === Grids.GridMedioDePago) {
        $("#" + grid + " tbody tr").each(function (index) {
            let aux = $(this)[0].cells[0].innerText;
            if (aux == datos.ins_id) {
                $(this)[0].cells[1].innerText = datos.ins_desc;
                $(this)[0].cells[3].innerText = datos.ins_vigente;
            }
        });
    }
    if (grid === Grids.GridBanco) {
        $("#" + grid + " tbody tr").each(function (index) {
            let aux = $(this)[0].cells[0].innerText;
            if (aux == datos.ctaf_id) {
                $(this)[0].cells[1].innerText = datos.ban_razon_social.toUpperCase();
                $(this)[0].cells[2].innerText = datos.tcb_desc.toUpperCase();
                $(this)[0].cells[3].innerText = datos.ban_cuenta_nro;
            }
        });
    }
    if (grid === Grids.GridCuentaDirecta) {
        $("#" + grid + " tbody tr").each(function (index) {
            let aux = $(this)[0].cells[0].innerText;
            if (aux == datos.ctaf_id) {
                $(this)[0].cells[1].innerText = datos.ctag_denominacion.toUpperCase();
                $(this)[0].cells[2].innerText = datos.tcg_desc.toUpperCase();
            }
        });
    }
}

function btnSubmitClick() {
    var tabActiva = $('.nav-tabs .active')[0].id;
    var mensaje = PuedoGuardar(tabActiva);
    if (mensaje === "") {
        Guardar();
    }
}

function Guardar() {
    if (validarCampos()) {
        var url = "";
        var gridParaActualizar = "";
        switch (destinoDeOperacion) {
            case AbmObject.CLIENTES:
                activarGrilla(Grids.GridCliente);
                gridParaActualizar = Grids.GridCliente;
                url = dataOpsClienteUrl;
                break;
            case AbmObject.PROVEEDORES:
                activarGrilla(Grids.GridProveedor);
                gridParaActualizar = Grids.GridProveedor;
                url = dataOpsProveedorUrl;
                break;
            case AbmObject.SECTORES:
                activarGrilla(Grids.GridSector);
                gridParaActualizar = Grids.GridSector;
                url = dataOpsSectorUrl;
                break;
            case AbmObject.MEDIO_DE_PAGO:
                activarGrilla(Grids.GridMedioDePago);
                gridParaActualizar = Grids.GridMedioDePago;
                url = dataOpsMedioDePagoUrl;
                break;
            case AbmObject.CLIENTES_CONDICIONES_VTA:
                activarGrilla(Grids.GridFP);
                url = dataOpsFormaDePagoUrl;
                break;
            case AbmObject.CUENTAS_CONTACTOS:
                activarGrilla(Grids.GridOC);
                $("#OtroContacto_Cta_Nombre").prop("disabled", false);
                $("#listaTC").prop("disabled", false);
                url = dataOpsCuentaContactoUrl;
                break;
            case AbmObject.CUENTAS_NOTAS:
                $("#Nota_Usu_Apellidoynombre").prop("disabled", false);
                activarGrilla(Grids.GridNota);
                url = dataOpsCuentaNotaUrl;
                break;
            case AbmObject.CUENTAS_OBSERVACIONES:
                activarGrilla(Grids.GridObs);
                url = dataOpsObservacionesUrl;
                break;
            case AbmObject.PROVEEDORES_FAMILIA:
                activarGrilla(Grids.GridFlias);
                url = dataOpsFamiliaUrl;
                break;
            case AbmObject.SUB_SECTORES:
                activarGrilla(Grids.GridSubSector);
                url = dataOpsSubSectorUrl;
                break;
            case AbmObject.RUBROS:
                activarGrilla(Grids.GridRubro);
                url = dataOpsRubroUrl;
                break;
            case AbmObject.OPCIONES_CUOTA:
                activarGrilla(Grids.GridOpcionesCuotas);
                url = dataOpsOpcionesCuotaUrl;
                break;
            case AbmObject.CUENTA_FIN_CONTABLE:
                activarGrilla(Grids.GridCuentaFinYConta);
                url = dataOpsCuentaFinYContableUrl;
                break;
            case AbmObject.POS:
                url = dataOpsPosUrl;
                break;
            case AbmObject.BANCOS:
                activarGrilla(Grids.GridBanco);
                gridParaActualizar = Grids.GridBanco;
                url = dataOpsBancoUrl;
                break;
            case AbmObject.CUENTAS_DIRECTAS:
                activarGrilla(Grids.GridCuentaDirecta);
                gridParaActualizar = Grids.GridCuentaDirecta;
                url = dataOpsCuentaDirectaUrl;
                break;
            default:
        }
        var data = ObtenerDatosParaJson(destinoDeOperacion, tipoDeOperacion);
        PostGen(data, url, function (obj) {
            if (obj.error === true) {
                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    $("#msjModal").modal("hide");
                    $(".nav-link").prop("disabled", false);

                    if (obj.setFocus && obj.setFocus != "") {
                        var contenedor = ObtenerModuloEjecutado(destinoDeOperacion);
                        var objeto = contenedor + obj.setFocus;
                        var value = $("[name='" + objeto + "']");
                        value.trigger("focus");
                    }
                    return true;
                }, false, ["Aceptar"], "error!", null);
            }
            else {
                AbrirMensaje("ATENCIÓN", obj.msg, function () {
                    $("#msjModal").modal("hide");
                    $(".nav-link").prop("disabled", false);

                    return true;
                }, false, ["Aceptar"], "succ!", null);
                activarBotones(false);
                ActualizarDatosEnGrilla(destinoDeOperacion);
                if (gridParaActualizar != "" && tipoDeOperacion == AbmAction.MODIFICACION) { //Si estoy modificando, actualizo el registro en la grilla.
                    ActualizarRegistroEnGrilla(data, gridParaActualizar);
                }
                if (tipoDeOperacion == AbmAction.ALTA && (destinoDeOperacion === AbmObject.CLIENTES || destinoDeOperacion === AbmObject.PROVEEDORES)) { //Si estoy agregando, busco el registro que acabo de agregar con el id que me devuelve el response (obj.id)
                    var controlCtaId = ObtenerModuloEjecutado(destinoDeOperacion) + 'Cta_Id';
                    var control = $("[name='" + controlCtaId + "']");
                    control.val(obj.id);
                    if (destinoDeOperacion === AbmObject.CLIENTES) {
                        BuscarElementoInsertado(obj.id, AbmObject.CLIENTES);
                    }
                    else if (destinoDeOperacion === AbmObject.PROVEEDORES) {
                        BuscarElementoInsertado(obj.id, AbmObject.PROVEEDORES);
                    }
                }
                if (tipoDeOperacion == AbmAction.ALTA && (destinoDeOperacion === AbmObject.SECTORES)) { //Si estoy agregando, busco el registro que acabo de agregar con el id que me devuelve el response (obj.id)
                    var controlId = ObtenerModuloEjecutado(destinoDeOperacion) + 'Sec_Id';
                    var control = $("[name='" + controlId + "']");
                    control.val(obj.id);
                    BuscarElementoInsertadoSector(obj.id);
                }
                if (tipoDeOperacion == AbmAction.ALTA && (destinoDeOperacion === AbmObject.MEDIO_DE_PAGO)) { //Si estoy agregando, busco el registro que acabo de agregar con el id que me devuelve el response (obj.id)
                    var controlId = ObtenerModuloEjecutado(destinoDeOperacion) + 'Ins_Id';
                    var control = $("[name='" + controlId + "']");
                    control.val(obj.id);
                    BuscarElementoInsertadoMedioDePago(obj.id);
                }
                if (tipoDeOperacion == AbmAction.ALTA && (destinoDeOperacion === AbmObject.BANCOS)) { //Si estoy agregando, busco el registro que acabo de agregar con el id que me devuelve el response (obj.id)
                    var controlId = ObtenerModuloEjecutado(destinoDeOperacion) + 'Ctaf_Id';
                    var control = $("[name='" + controlId + "']");
                    control.val(obj.id);
                    BuscarElementoInsertadoBanco(obj.id);
                }
                if (tipoDeOperacion == AbmAction.ALTA && (destinoDeOperacion === AbmObject.CUENTAS_DIRECTAS)) { //Si estoy agregando, busco el registro que acabo de agregar con el id que me devuelve el response (obj.id)
                    var controlId = ObtenerModuloEjecutado(destinoDeOperacion) + 'Ctag_Id';
                    var control = $("[name='" + controlId + "']");
                    control.val(obj.id);
                    BuscarElementoInsertadoCuentaDirecta(obj.id);
                }
                if (tipoDeOperacion == AbmAction.MODIFICACION) {
                    btnCancelClick();
                }
                if (tipoDeOperacion == AbmAction.BAJA) {
                    btnCancelClick();
                    switch (destinoDeOperacion) {
                        case AbmObject.CLIENTES:
                            buscarClientes(1, true);
                            break;
                        case AbmObject.PROVEEDORES:
                            buscarProveedores(1, true);
                            break;
                        case AbmObject.MEDIO_DE_PAGO:
                            buscarMediosDePago(1, true);
                            break;
                        case AbmObject.BANCOS:
                            buscarBancos(1, true);
                            break;
                        case AbmObject.CUENTAS_DIRECTAS:
                            buscarCuentasDirectas(1, true);
                            break;
                        default:
                    }
                }
            }
        });
    }
}

function BuscarElementoInsertadoBanco(ctafId) {
    var data = { ctafId };
    var url = buscarBancoCargadoUrl;
    PostGen(data, url, function (obj) {
        if (obj.error === true) {
            AbrirMensaje("ATENCIÓN", "Se produjo un error al intentar obtener la entidad recientemente cargada.", function () {
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else {
            if (obj.data) {
                $("#chkDescr").prop('checked', true);
                $("#chkDescr").trigger("change");
                $("#Buscar").val(obj.data);
                $("#chkDesdeHasta").prop('checked', false);
                $("#chkDesdeHasta").trigger("change");
                $("#chkRel01").prop('checked', false);
                $("#chkRel01").trigger("change");
                $("#chkRel02").prop('checked', false);
                $("#chkRel02").trigger("change");
                buscarBancos(1);
            }
        }
    });
}

function BuscarElementoInsertadoMedioDePago(insId) {
    var data = { insId };
    var url = buscarMedioDePagoCargadoUrl;
    PostGen(data, url, function (obj) {
        if (obj.error === true) {
            AbrirMensaje("ATENCIÓN", "Se produjo un error al intentar obtener la entidad recientemente cargada.", function () {
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else {
            if (obj.data) {
                $("#chkDescr").prop('checked', true);
                $("#chkDescr").trigger("change");
                $("#Buscar").val(obj.data);
                $("#chkDesdeHasta").prop('checked', false);
                $("#chkDesdeHasta").trigger("change");
                $("#chkRel01").prop('checked', false);
                $("#chkRel01").trigger("change");
                $("#chkRel02").prop('checked', false);
                $("#chkRel02").trigger("change");
                buscarMediosDePago(1);
            }
        }
    });
}

function BuscarElementoInsertadoSector(secId) {
    var data = { secId };
    var url = buscarSectorCargadoUrl;
    PostGen(data, url, function (obj) {
        if (obj.error === true) {
            AbrirMensaje("ATENCIÓN", "Se produjo un error al intentar obtener la entidad recientemente cargada.", function () {
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else {
            if (obj.data) {
                $("#chkDescr").prop('checked', true);
                $("#chkDescr").trigger("change");
                $("#Buscar").val(obj.data);
                $("#chkDesdeHasta").prop('checked', false);
                $("#chkDesdeHasta").trigger("change");
                $("#chkRel01").prop('checked', false);
                $("#chkRel01").trigger("change");
                $("#chkRel02").prop('checked', false);
                $("#chkRel02").trigger("change");
                buscarSectores(1);
            }
        }
    });
}

function BuscarElementoInsertado(ctaId, origen) {
    var data = { ctaId };
    var url = "";
    switch (origen) {
        case AbmObject.CLIENTES:
            url = buscarClienteCargadoUrl;
            break;
        case AbmObject.PROVEEDORES:
            url = buscarProveedorCargadoUrl;
            break;
        default:
    }
    PostGen(data, url, function (obj) {
        if (obj.error === true) {
            AbrirMensaje("ATENCIÓN", "Se produjo un error al intentar obtener la entidad recientemente cargada.", function () {
                return true;
            }, false, ["Aceptar"], "error!", null);
        }
        else {
            if (obj.data) {
                $("#chkDescr").prop('checked', true);
                $("#chkDescr").trigger("change");
                $("#Buscar").val(obj.data);
                $("#chkDesdeHasta").prop('checked', false);
                $("#chkDesdeHasta").trigger("change");
                $("#chkRel01").prop('checked', false);
                $("#chkRel01").trigger("change");
                $("#chkRel02").prop('checked', false);
                $("#chkRel02").trigger("change");
                switch (origen) {
                    case AbmObject.CLIENTES:
                        buscarClientes(1);
                        break;
                    case AbmObject.PROVEEDORES:
                        buscarProveedores(1);
                        break;
                    default:
                }

            }
        }
    });
}

function ObtenerModuloEjecutado(destinoDeOperacion) {
    switch (destinoDeOperacion) {
        case AbmObject.CLIENTES:
            return "Cliente."
            break;
        case AbmObject.PROVEEDORES:
            return "Proveedor."
            break;
        case AbmObject.SECTORES:
            return "Sector."
            break;
        case AbmObject.MEDIO_DE_PAGO:
            return "MedioDePago."
            break;
        case AbmObject.CLIENTES_CONDICIONES_VTA:
            return "FormaDePago."
            break;
        case AbmObject.CUENTAS_CONTACTOS:
            return "OtroContacto."
            break;
        case AbmObject.CUENTAS_NOTAS:
            return "Nota."
            break;
        case AbmObject.CUENTAS_OBSERVACIONES:
            return "Observacion."
            break;
        case AbmObject.PROVEEDORES_FAMILIA:
            return "ProveedorGrupo."
            break;
        case AbmObject.SUB_SECTORES:
            return "SubSector."
            break;
        case AbmObject.RUBROS:
            return "Rubro."
            break;
        case AbmObject.OPCIONES_CUOTA:
            return "OpcionCuota."
            break;
        case AbmObject.CUENTA_FIN_CONTABLE:
            return "CuentaFin."
            break;
        case AbmObject.POS:
            return ""
            break;
        case AbmObject.BANCOS:
            return "Banco."
            break;
        case AbmObject.CUENTAS_DIRECTAS:
            return "CuentaDirecta."
            break;
        default:
    }
}

function ActualizarDatosEnGrilla(destinoDeOperacion) {
    switch (destinoDeOperacion) {
        case AbmObject.CLIENTES:

            break;
        case AbmObject.PROVEEDORES:

            break;
        case AbmObject.SECTORES:

            break;
        case AbmObject.MEDIO_DE_PAGO:

            break;
        case AbmObject.CLIENTES_CONDICIONES_VTA:
            BuscarFormaDePago();
            break;
        case AbmObject.CUENTAS_CONTACTOS:
            BuscarOtrosContactos();
            break;
        case AbmObject.CUENTAS_NOTAS:
            BuscarNotas();
            break;
        case AbmObject.CUENTAS_OBSERVACIONES:
            BuscarObservaciones();
            break;
        case AbmObject.PROVEEDORES_FAMILIA:
            BuscarFamilias();
            break;
        case AbmObject.SUB_SECTORES:
            BuscarSubSector();
            break;
        case AbmObject.RUBROS:
            BuscarRubro();
            break;
        case AbmObject.OPCIONES_CUOTA:
            BuscarOpcionesCuotas();
            break;
        case AbmObject.CUENTA_FIN_CONTABLE:
            BuscarCuentaFinContable();
            break;
        case AbmObject.BANCOS:

            break;
        case AbmObject.CUENTAS_DIRECTAS:

            break;
        default:
    }
}

function ObtenerDatosParaJson(destinoDeOperacion, tipoDeOperacion) {
    var json = "";
    switch (destinoDeOperacion) {
        case AbmObject.CLIENTES:
            json = ObtenerDatosDeClienteParaJson(destinoDeOperacion, tipoDeOperacion);
            break;
        case AbmObject.PROVEEDORES:
            json = ObtenerDatosDeProveedorParaJson(destinoDeOperacion, tipoDeOperacion);
            break;
        case AbmObject.SECTORES:
            json = ObtenerDatosDeSectorParaJson(destinoDeOperacion, tipoDeOperacion);
            break;
        case AbmObject.MEDIO_DE_PAGO:
            json = ObtenerDatosDeMedioDePagoParaJson(destinoDeOperacion, tipoDeOperacion);
            break;
        case AbmObject.CLIENTES_CONDICIONES_VTA:
            json = ObtenerDatosDeFormaDePagoPParaJson(destinoDeOperacion, tipoDeOperacion);
            break;
        case AbmObject.CUENTAS_CONTACTOS:
            json = ObtenerDatosDeOtrosContactosParaJson(destinoDeOperacion, tipoDeOperacion);
            break;
        case AbmObject.CUENTAS_NOTAS:
            json = ObtenerDatosDeNotasParaJson(destinoDeOperacion, tipoDeOperacion);
            break;
        case AbmObject.CUENTAS_OBSERVACIONES:
            json = ObtenerDatosDeObsParaJson(destinoDeOperacion, tipoDeOperacion);
            break;
        case AbmObject.PROVEEDORES_FAMILIA:
            json = ObtenerDatosDeProveedorFamiliaParaJson(destinoDeOperacion, tipoDeOperacion);
            break;
        case AbmObject.SUB_SECTORES:
            json = ObtenerDatosDeSubSectorParaJson(destinoDeOperacion, tipoDeOperacion);
            break;
        case AbmObject.RUBROS:
            json = ObtenerDatosDeRubroParaJson(destinoDeOperacion, tipoDeOperacion);
            break;
        case AbmObject.OPCIONES_CUOTA:
            json = ObtenerDatosDeOpcCuotaParaJson(destinoDeOperacion, tipoDeOperacion);
            break;
        case AbmObject.CUENTA_FIN_CONTABLE:
            json = ObtenerDatosDeCuentaFinContaParaJson(destinoDeOperacion, tipoDeOperacion);
            break;
        case AbmObject.POS:
            json = ObtenerDatosDePosParaJson(destinoDeOperacion, tipoDeOperacion);
            break;
        case AbmObject.BANCOS:
            json = ObtenerDatosDeBancoParaJson(destinoDeOperacion, tipoDeOperacion);
            break;
        case AbmObject.CUENTAS_DIRECTAS:
            json = ObtenerDatosDeCuentaDirectaParaJson(destinoDeOperacion, tipoDeOperacion);
            break;
        default:
    }
    return json;
}

function ObtenerDatosDeObsParaJson(destinoDeOperacion, tipoDeOperacion) {
    var cta_id = $("#Cliente_Cta_Id").val();
    if (cta_id == undefined || cta_id == null)
        cta_id = $("#Proveedor_Cta_Id").val();
    var to_id = $("#listaTipoObs").val();
    var to_desc = $("#listaTipoObs option:selected").text();
    var to_lista = $("#listaTipoObs option:selected").text() + "(" + $("#listaTipoObs").val() + ")";
    var cta_obs = $("#Observacion_Cta_Obs").val();
    var data = { cta_id, to_id, to_desc, to_lista, cta_obs, destinoDeOperacion, tipoDeOperacion };
    return data;
}

function ObtenerDatosDeNotasParaJson(destinoDeOperacion, tipoDeOperacion) {
    var cta_id = $("#Cliente_Cta_Id").val();
    if (cta_id == undefined || cta_id == null)
        cta_id = $("#Proveedor_Cta_Id").val();
    var usu_id = $("#Nota_Usu_Id").val();
    var usu_apellidoynombre = $("#Nota_usu_id").val();
    var usu_lista = $("#Nota_Usu_Id").val();
    var fecha = null;
    var nota = $("#Nota_Nota").val();
    var data = { cta_id, usu_id, usu_apellidoynombre, usu_lista, fecha, nota, destinoDeOperacion, tipoDeOperacion };
    return data;
}

function ObtenerDatosDeOtrosContactosParaJson(destinoDeOperacion, tipoDeOperacion) {
    var cta_id = $("#Cliente_Cta_Id").val();
    if (cta_id == undefined || cta_id == null)
        cta_id = $("#Proveedor_Cta_Id").val();
    var tc_id = $("#listaTC").val();
    var tc_desc = $("#listaTC option:selected").text();
    var tc_lista = $("#listaTC option:selected").text() + "(" + $("#listaTC").val() + ")";
    var cta_celu = $("#OtroContacto_Cta_Celu").val();
    var cta_te = $("#OtroContacto_Cta_Te").val();
    var cta_email = $("#OtroContacto_Cta_Email").val();
    var cta_nombre = $("#OtroContacto_Cta_Nombre").val();
    var data = { cta_id, tc_id, tc_desc, tc_lista, cta_celu, cta_te, cta_email, cta_nombre, destinoDeOperacion, tipoDeOperacion };
    return data;
}

function ObtenerDatosDeFormaDePagoPParaJson(destinoDeOperacion, tipoDeOperacion) {
    var cta_id = $("#Cliente_Cta_Id").val();
    if (cta_id == undefined || cta_id == null)
        cta_id = $("#Proveedor_Cta_Id").val();
    var fp_id = $("#listaFP").val();
    var fp_desc = $("#listaFP option:selected").text();
    var fp_lista = $("#listaFP option:selected").text() + "(" + $("#listaFP").val() + ")";
    var fp_dias = $("#FormaDePago_Fp_Dias").val();
    var tcb_id = $("#listaTipoCueBco").val();
    var tcb_desc = $("#listaTipoCueBco option:selected").val();
    var tcb_lista = $("#listaTipoCueBco option:selected").text() + "(" + $("#listaTipoCueBco").val() + ")";
    var cta_bco_cuenta_nro = $("#FormaDePago_Cta_Bco_Cuenta_Nro").val();
    var cta_bco_cuenta_cbu = $("#FormaDePago_Cta_Bco_Cuenta_Cbu").val();
    var cta_valores_a_nombre = $("#FormaDePago_Cta_Valores_A_Nombre").val();
    var cta_obs = $("#FormaDePago_Cta_Obs").val();
    var fp_default = "N";
    var data = { cta_id, fp_id, fp_desc, fp_lista, fp_dias, tcb_id, tcb_desc, tcb_lista, cta_bco_cuenta_nro, cta_bco_cuenta_cbu, cta_valores_a_nombre, cta_obs, fp_default, destinoDeOperacion, tipoDeOperacion };
    return data;
}

function activarArbol(div, node, activar, cancelar = false) {
    var nodo = $(div).jstree().get_node(node);
    if (activar === true) {
        $(div).jstree(true).enable_node(nodo);
        nodo.children.forEach(function (child_id) {
            activarArbol(div, child_id, activar);
        });
    }
    else {
        $(div).jstree(true).disable_node(nodo);
        //al cancelar se debe restituir los valores por defecto
        if (cancelar === true && nodo.id !== "00" && nodo.id !== "#") {
            if (nodo.data.asignado === true) {
                $(div).jstree(true).select_node(nodo);
            }
            else {
                $(div).jstree(true).deselect_node(node);
            }
        }
        nodo.children.forEach(function (child_id) {
            activarArbol(div, child_id, activar, cancelar);
        });
    }
}