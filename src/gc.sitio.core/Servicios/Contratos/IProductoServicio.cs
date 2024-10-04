﻿using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.CuentaComercial;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Almacen.Tr;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.Info;
using gc.infraestructura.Dtos.Almacen.Tr.Transferencia;
using gc.infraestructura.Dtos.General;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IProductoServicio : IServicio<ProductoDto>
    {
        Task<ProductoBusquedaDto> BusquedaBaseProductos(BusquedaBase busqueda, string token);
        Task<List<ProductoBusquedaDto>> BusquedaBaseProductosPorIds(BusquedaBase busqueda, string token);

		Task<List<ProductoListaDto>> BusquedaListaProductos(BusquedaProducto search, string tokenCookie);
        Task<List<InfoProdStkD>> InfoProductoStkD(string id, string admId, string token);
        Task<List<InfoProdStkBox>> InfoProductoStkBoxes(string id, string adm, string depo, string token);
        Task<List<InfoProdStkA>> InfoProductoStkA(string id, string admId, string token);
        Task<List<InfoProdMovStk>> InfoProductoMovStk(string id, string adm, string depo, string tmov, DateTime desde, DateTime hasta, string token);
        Task<List<InfoProdLP>> InfoProductoLP(string id, string token);
        Task<List<RPRAutorizacionPendienteDto>> RPRObtenerAutorizacionPendiente(string adm, string token);
        Task<RPRRegistroResponseDto> RPRRegistrarProductos(List<RPRProcuctoDto> json,string admId, string ul, string token);
        Task<List<RPRAutoComptesPendientesDto>> RPRObtenerComptesPendiente(string adm_id, string token);

        Task<RprResponseDto> ValidarUL(string ul,string adm,string token);
        Task<RprResponseDto> ValidarBox(string box,string adm,string token);
        Task<RprResponseDto> ConfirmaBoxUl(string box, string ul, string adm, string token);

        Task<List<RespuestaDto>> RPRCargarCompte(string json_str, string token);
        Task<List<RespuestaDto>> RPREliminarCompte(string rp, string token);
        Task<List<JsonDto>> RPObtenerJsonDesdeRP(string rp, string token);
        Task<List<RPRItemVerCompteDto>> RPRObtenerItemVerCompte(string rp, string token);
        Task<List<RPRVerConteoDto>> RPRObtenerItemVerConteos(string rp, string token);
	


        Task<List<AutorizacionTIDto>> TRObtenerAutorizacionesPendientes(string admId, string usuId, string titId,string token);
        Task<List<TRPendienteDto>> TRObtenerPendientes(string admId, string usuId, string titId, string token);

		Task<List<BoxRubProductoDto>> PresentarBoxDeProductos(string tr, string admId, string usuId, string token);
        Task<List<BoxRubProductoDto>> PresentarRubrosDeProductos(string tr, string admId, string usuId, string token);
        Task<List<TiListaProductoDto>> BuscaTIListaProductos(string tr, string admId, string usuId, string? boxid, string? rubId, string token);
        Task<List<RespuestaDto>> RPRConfirmarRPR(string rp, string adm_id, string token);

        Task<List<TipoMotivoDto>> ObtenerTiposMotivo(string token);
        Task<RespuestaGenerica<RespuestaDto>> ResguardarProductoCarrito(TiProductoCarritoDto request,string token);
        Task<List<TRAutSucursalesDto>> TRObtenerAutSucursales(string admId, string token);
        Task<List<TRAutPIDto>> TRObtenerAutPI(string admId, string admIdLista, string token);
        Task<List<TRAutPIDetalleDto>> TRObtenerAutPIDetalle(string piCompte, string token);

		Task<List<TRAutDepoDto>> TRObtenerAutDepositos(string admId, string token);

        Task<RespuestaGenerica<RespuestaDto>> ControlSalidaTI(string ti,string adm,string usu, string token);
        Task<RespuestaGenerica<TIRespuestaDto>> TIValidaPendiente(string usu, string token);
        Task<RespuestaGenerica<RespuestaDto>> TIConfirma(TIRequestConfirmaDto confirma,string token);
        Task<RespuestaGenerica<TIRespuestaDto>> TINueva_SinAu(string tipo, string adm, string usu, string token);
		Task<List<TRAutAnalizaDto>> TRAutAnaliza(string listaPi, string listaDepo, bool stkExistente, bool sustituto, int palletNro, string token);
        Task<RespuestaGenerica<ProductoDepositoDto>> BuscarFechaVto(string pId, string bId, string tokenCookie);
        Task<List<TRProductoParaAgregar>> TRObtenerSustituto(string pId, string listaDepo, string admIdDes, string tipo, string token);
        Task<List<RespuestaDto>> TRConfirmaAutorizaciones(string json, string admId, string usuId, string token);
	}

}
