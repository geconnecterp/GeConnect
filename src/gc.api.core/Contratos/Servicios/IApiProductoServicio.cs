using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Almacen.Response;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Almacen.Tr;
using NDeCYPI = gc.infraestructura.Dtos.Almacen.Tr.NDeCYPI;
using gc.infraestructura.Dtos.Almacen.Tr.Transferencia;
using gc.infraestructura.Dtos.Box;
using gc.infraestructura.Dtos.CuentaComercial;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.AjusteDeStock;

namespace gc.api.core.Contratos.Servicios
{
    public interface IApiProductoServicio : IServicio<Producto>
    {
        ProductoBusquedaDto ProductoBuscar(BusquedaBase busqueda);
        List<ProductoBusquedaDto> ProductoBuscarPorIds(BusquedaBase busqueda);

        List<ProductoListaDto> ProductoListaBuscar(BusquedaProducto search);

        List<InfoProdStkD> InfoProductoStkD(string id, string admId);
        List<InfoProdStkBox> InfoProductoStkBoxes(string id, string adm, string depo);
        List<InfoProdStkA> InfoProductoStkA(string id, string admId);
        List<InfoProdMovStk> InfoProductoMovStk(string id, string adm, string depo, string tmov, DateTime desde, DateTime hasta);
        List<InfoProdLP> InfoProductoLP(string id);
        List<NDeCYPI.InfoProdIExMesDto> InfoProdIExMes(string admId, string pId, int meses);
        List<NDeCYPI.InfoProdIExSemanaDto> InfoProdIExSemana(string admId, string pId, int semanas);
        List<ProductoNCPISustitutoDto> InfoProdSustituto(string pId, string tipo, string admId, bool soloProv);
        List<NDeCYPI.InfoProductoDto> InfoProd(string pId);
        List<TipoAjusteDeStockDto> ObtenerTipoDeAjusteDeStock();
        List<AjustePrevioCargadoDto> ObtenerAJPreviosCargados(string admId);
        List<AjusteRevertidoDto> ObtenerAJREVERTIDO(string ajId);

		List<AutorizacionPendienteDto> RPRObtenerAutorizacionPendiente(string adm);
        RegistroResponseDto RPRRegistrarProductos(string json,bool esModificacion);
        List<AutoComptesPendientesDto> RPRObtenerComptesPendientes(string adm);
        List<RespuestaDto> RPRCargar(CargarJsonGenRequest request);
        List<RespuestaDto> RPRElimina(string rp);
        List<RespuestaDto> RPRConfirma(string rp, string adm_id);
        List<RPRxULDto> RPRxUL(string rp);
        List<RPRxULDetalleDto> RPRxULDetalle(string ulId);


		List<JsonDto> RPREObtenerDatosJsonDesdeRP(string rp);
        List<RPRItemVerCompteDto> RPRObtenerDatosVerCompte(string rp);
        List<RPRVerConteoDto> RPRObtenerConteos(string rp);
        RespuestaDto ResguardarProductoCarrito(TiProductoCarritoDto request);
        List<TRPendienteDto> ObtenerTRPendientes(ObtenerTRPendientesRequest request);
        List<TRAutSucursalesDto> ObtenerTRAut_Sucursales(string admId);
        List<TRAutPIDto> ObtenerTRAut_PI(string admId, string admIdLista);
        List<TRAutPIDetalleDto> ObtenerTRAut_PI_Detalle(string piCompte);

		List<TRAutDepoDto> ObtenerTRAut_Depositos(string admId);


        RespuestaDto TRCtrlSalida(string ti, string adm, string usu);
        TIRespuestaDto TRNuevaSinAuto(string tipoIt, string adm, string usu);
        TIRespuestaDto TRValidaPendiente(string usu);
        RespuestaDto TR_Confirma(TIRequestConfirmaDto conf);
        List<TRAutAnalizaDto> TRAutAnaliza(string listaPi, string listaDepo, bool stkExistente, bool sustituto, int palletNro);
        List<TRProductoParaAgregar> TRObtenerSustituto(string pId, string listaDepo, string admIdDes, string tipo);
		List<RespuestaDto> TRConfirmaAutorizaciones(TRConfirmaRequest request);
        RespuestaDto ValidarProductoCarrito(TiProductoCarritoDto request);
        List<ProductoGenDto> TRVerCtrlSalida(string tr, string user);
        RespuestaDto TRCargarCtrlSalida(string json);    
        List<TRVerConteosDto> TRVerConteos(string ti);
        List<RespuestaDto> TRValidarTransferencia(TRValidarTransferenciaRequest request);

        List<ProductoNCPIDto> NCPICargarListaDeProductos(NCPICargarListaDeProductosRequest request);
        List<NCPICargaPedidoResponse> NCPICargaPedido(NCPICargaPedidoRequest request);

        BoxInfoDto ObtenerBoxInfo(string box_id);
        List<BoxInfoStkDto> ObtenerBoxInfoStk(string box_id);
        List<BoxInfoMovStkDto> ObtenerBoxInfoMovStk(string box_id,string sm_tipo,DateTime desde,DateTime hasta);
    }
}
