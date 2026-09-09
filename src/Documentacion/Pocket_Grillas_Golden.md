# Grillas Pocket — aplicación de adaptive-record-styler

Fecha: 2026-09-07. Raíz: `D:\Sis25\git\GeConnect\src`.

## Alcance aplicado

38 tablas Razor incorporadas a `pocket-grids.css`; la tabla TR conserva `pocket-records.css`. Se incluye además la tabla dinámica de búsqueda avanzada en `busquedasV02.js`: 40 tablas identificadas en total. Se conservaron IDs, eventos, rutas, atributos, columnas, condiciones y formatos de cantidad.

- Intercalado blanco/dorado suave; encabezado carbón de alto contraste.
- Selección, edición y avisos de inventario independientes del intercalado.
- Cantidades legibles con numerales tabulares; sin modificar cultura o decimales.
- Botones de al menos 44 px y foco visible.
- Desplazamiento horizontal interno en tablas anchas; registros OR adaptables.
- Sin transformaciones que agranden las filas al pasar el puntero.

## Inventario de vistas

- [x] `gc.pocket.site/Areas/ABMs/Views/AbmProducto/_gridAbmProds.cshtml`
- [x] `gc.pocket.site/Areas/ABMs/Views/AbmProducto/LabMenu.cshtml`
- [x] `gc.pocket.site/Views/Shared/_gridProdsAdv.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/ImprEt/_productosEtiqueta.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/ImprEt/LabMenu.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/OR/_OR_Lista_Rub.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/OR/_gridOrdenReparto.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/OR/_OR_Lista_Box.cshtml`
- [x] `gc.pocket.site/Areas/Gestion/Views/Almacen/_gridRprAutorizados.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/OR/_gridORListaProducto.cshtml`
- [x] `gc.pocket.site/Areas/Gestion/Views/Almacen/_gridProductos.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/ORCtl/_listaControl.cshtml`
- [x] `gc.pocket.site/Areas/Gestion/Views/Inventario/_gridInvProductos.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/InfoProd/_gridBoxInfoMovStk.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/TrInt/_gridTIBox.cshtml`
- [x] `gc.pocket.site/Areas/Gestion/Views/Inventario/_gridInventarios.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/TrInt/_gridTRAutoPendientes.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/TrInt/_gridTIListaProducto.cshtml` — diseño de registros TR conservado
- [x] `gc.pocket.site/Areas/PocketPpal/Views/TrInt/_gridTIRubro.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/RPR/_AutoPendienteGrid.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/InfoProd/_gridBoxLibres.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/InfoProd/_gridBoxInfoStk.cshtml`
- [x] `gc.pocket.site/Areas/Gestion/Views/Inventario/_gridInventarioPlantilla.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/RPR/_detalleULsGrid.cshtml`
- [x] `gc.pocket.site/Areas/Gestion/Views/Inventario/_gridInventarioBox.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/InfoProd/_gridDepoInfoStk.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/RPR/_rprProductosCargardos.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/InfoProd/_gridDepoInfoStkVal.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/InfoProd/_gridListadoUL.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/InfoProd/_infoProdLPGrid.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/Shared/_ctrlproductosCargardos.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/InfoProd/_infoProdMovStkGrid.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/InfoProd/_infoProdStkAGrid.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/InfoProd/_infoProdStkBoxGrid.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/Shared/_productosCargardos.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/InfoProd/_infoProdStkDGrid.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/RTI/_detalleULsGrid.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/RTI/_rtiProductosCargardos.cshtml`
- [x] `gc.pocket.site/Areas/PocketPpal/Views/RTI/_rtiRemitosPendientes.cshtml`

- [x] `gc.pocket.site/wwwroot/js/app/busquedasV02.js` — clases de la tabla dinámica y su contenedor; lógica sin cambios.

## Verificación

- [x] Comparación con contenido previo: las 39 vistas conservan todo su contenido salvo clases visuales agregadas y saltos finales de línea.
- [x] Compilación Pocket: 0 errores; advertencias existentes.
- [x] Sintaxis JavaScript.
- [x] Prueba visual con datos ficticios y CSS real a 1440 y 390 px: sin desborde de página; tabla ancha desplaza internamente y registro OR se adapta. Estados y botones de 44 px comprobados.
- [ ] Verificación funcional/visual con datos reales por módulo y dispositivos Honeywell: pendiente de reiniciar el sitio (localhost:7176 no estaba disponible).

No se ejecutaron operaciones de stock para probar apariencia. Recompilar/reiniciar Pocket para incorporar las clases Razor y recargar sin caché. No requiere cambios en API o SQL por esta intervención.

