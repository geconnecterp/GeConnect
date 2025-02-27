$(function () {
    $("#tipoCuentaId").on("change", cambiarTipoId);
});

function cambiarTipoId() {
    //esta funcionCorresponde a la que se renderiza en el control de cuentas comerciales
    //SE DEBE COLOCAR EL SUFIJO ESPECIFICADO EN LA VISTA QUE SE USA.
    var tipo = $("#tipoCuentaId option:selected").val();
    cargarTipoCuentaIdCons(tipo);
    return true;
}