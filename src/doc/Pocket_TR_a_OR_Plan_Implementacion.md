# Pocket: inventario TR y plan de traslado a OR

Fecha: 12/09/2026. Raíz: `D:\Sis25\git\GeConnect\src`.

## Alcance y estado

El usuario confirmó equivalencia funcional de TR y OR, salvo su finalidad, y equivalencia de sus SP/parámetros. También confirmó conservar SPGECO_OR_* con contratos equivalentes a TR, sin redirigir OR a SPGECO_TR_*. Se conservan comprobantes, navegación y control de salida propios de OR. No se modifican SP ni se ejecutan movimientos de inventario para verificar apariencia.

Esta revisión compara código y, posteriormente, los SP adjuntados por el usuario; no certifica su instalación ni ejecución en base de datos. El traslado funcional completo a OR sigue en curso.

### Avance posterior: contratos OR y limpieza TR

- El usuario confirmó que Valida y Carga requieren `@item` en TR y OR. Los tres SP OR compartidos corroboran el contrato de OR.
- Se eliminó `ApiProductoServicio.ValidaProductoCarrito`, sin referencias ni declaración en interfaz. Se conserva `ValidarProductoCarrito` con `@item`, su interfaz y el endpoint HTTP `ValidaProductoCarrito`.
- OR incorpora `item` nullable para detectar su ausencia, identidad de sesión por comprobante/ítem/producto/BOX, enlaces y payloads de carga/eliminación. Sin ítem no se invoca el SP.
- Validación y carga OR envían los 15 parámetros confirmados, incluidos ítem y los tres de reemplazo.
- Se corrigió el alias de listado `or_compte` (antes el DTO esperaba `ti`) y se incorporaron cantidades/estados/referencias al DTO; su presentación nueva sigue pendiente.
- El servicio HTTP de OR interpreta `resultado == 0` como éxito; HTTP 200 con resultado de rechazo ya no permite continuar a Carga ni simula éxito.
- API y Pocket compilan sin errores; JavaScript modificado valida sintácticamente. Comprobación estática de las firmas de 15 parámetros correcta. No se ejecutaron SP ni movimientos.
- Después de publicar API y Pocket, reabrir el listado OR para renovar datos de sesión y enlaces con ítem.

La tabla siguiente conserva el diagnóstico inicial para trazabilidad; los puntos 8/9/10/20 quedan parcialmente resueltos por este avance. La reconsulta en tiempo real, permisos completos, acumulación y maqueta OR siguen pendientes.

Ya implementado en esta revisión: cabecera móvil compacta para PocketPpal/TrInt y PocketPpal/OR. Diseño de bloque principal de 52 px, inicio y volver de 44×44 px, sucursal/usuario en dos líneas. Escritorio y otros controladores sin cambios. Compilación: 0 errores. Verificación visual pendiente de publicación; el navegador integrado no respondió.

## Inventario detallado

| Nº | Mejora/corrección de TR | Estado de OR y acción |
|---|---|---|
| 1 | Botón naranja para iniciar reemplazo desde el renglón. | Ya existe; completar permisos y estado del renglón. |
| 2 | Modal estándar AbrirMensaje, warn!, datos originales y cancelar sin navegar. | Ya usa reemplazoProducto.js compartido; verificar regresión. |
| 3 | Terminología reemplazo, sin sustituto. | Presente en las rutas principales inspeccionadas; verificar mensajes del ciclo. |
| 4 | Modo reemplazo visible y contexto original en sesión. | Existe parcialmente; completar identidad del renglón. |
| 5 | Permitir mismo producto de otro BOX o producto distinto en reemplazo. | Validación local ya permite; completar recorrido hasta SP. |
| 6 | Carga normal exige producto/BOX esperado. | Existe; corregir selección ambigua de productos repetidos. |
| 7 | BOX de máximo 11 dígitos, teclado numérico, validación y foco en búsqueda. | Ya implementado en vista/JS; verificar en colector y respuesta sugerida del servidor. |
| 8 | Pasar remplazar, remplazar_box_id y remplazar_p_id por validación y carga. | DTO/carga los tienen; servicio Valida NO los envía actualmente. |
| 9 | Identidad por ítem, producto y BOX. | OR no incorpora ítem en sus DTO actuales; carga/eliminación buscan solo por producto. |
| 10 | Valida antes de Carga; no cargar ante rechazo. | Secuencia existente; comprobar respuesta funcional y firma exacta. |
| 11 | Recarga del listado desde API/SP y preservación de filtros/orden. | Entrar a ORCargaCarrito consulta API; BuscaORListaProductos solo ordena sesión. Actualizar este último recorrido. |
| 12 | Reconsulta del renglón antes de modificar/eliminar ante cambios de otros usuarios. | Pendiente. No garantiza atomicidad: decisión final del SP. |
| 13 | Colección propia previa: Sobrescribir carga / Acumular / Cancelar, mostrando resultados. | Pendiente. No confundir sobrescribir cantidades con reemplazar producto. |
| 14 | Acumular cantidades, bultos y unidades propios, no cantidades de otros. | Pendiente; verificar unidad de presentación y concurrencia. |
| 15 | up_id 07 entero; otros decimales; ingreso normalizado y desglose coherente. | Ya tiene formato y validaciones compartidas; verificar regresión. |
| 16 | Admitir fracciones positivas menores de 1 para productos decimales. | OR todavía rechaza cantidad < 1. Adecuar a cantidad <= 0 donde corresponde. |
| 17 | No bloquear por exceso localmente en colección objetivo; SP/control de salida decide. | OR aún bloquea cantidad mayor al pedido en controlador. |
| 18 | Original completo/excedido: puede operar quien tiene colección propia; no quien no colectó y otros ya completaron. | Pendiente; aplicar en servidor y grilla. |
| 19 | Reemplazos sujetos a propiedad del operador y estados; eliminar solo colección propia. | Faltan datos de propiedad/estado. No permitir actuar sobre terceros. |
| 20 | Response enriquecido con ítem, otros, reemplazo, totales, estados y referencia original. | ORProductoDto todavía no contiene esos campos. Mapear response real. |
| 21 | Diferencia = Pedido − (Colectado + Reemplazo + Colectado Otros). | Pendiente: positivo Falta colectar, negativo Sobra, cero sin leyenda adicional. |
| 22 | Diferencia anterior solo para originales; filas de reemplazo mantienen significado propio. | No duplicar cantidades sumando filas cuyo total ya incluye el SP. |
| 23 | Etiquetas Pedido, Colectado, Reemplazo y Colectado Otros; sin totales de BOX/producto redundantes. | OR solo muestra pedido y colección, esta última si > 0. Incluir ceros y campos nuevos. |
| 24 | Resultado_msj tiene prioridad aunque código desconocido; respaldo local si vacío; genérico si ambos desconocidos/vacíos. | Pendiente. Permisos y colores no se deciden por el texto. |
| 25 | Originales/reemplazos con tarjeta móvil compacta: cantidades izquierda, acciones/estado derecha. | Migrar grilla antigua OR preservando rutas, eventos y data attributes. |
| 26 | Código/descripcion, BOX/rubro; valor BOX y código rubro en negrita. | Valores destacados ya existen; unificar composición. |
| 27 | Textos de una línea con puntos suspensivos; BOX/comprobante completos. | Aplicar a OR conservando texto completo en DOM/ayuda cuando corresponda. |
| 28 | Referencia Rem. productoOriginal boxOriginal - (operador). | Pendiente por datos y maqueta. BOX completo y destacado. |
| 29 | Carga verde, reemplazo naranja, eliminar rojo; área táctil y foco visibles. | Mantener semántica e integrar al patrón compacto. |
| 30 | Cabecera menor, títulos redundantes fuera de móvil, menos márgenes y espacio entre radios/grilla. | Cabecera TR/OR implementada ahora; espacios interiores de OR pendientes. |
| 31 | Nota quitada en autorizaciones TR; itinerario sin celdas vacías; título/comprobante centrado azul; menor altura de grillas. | Trasladar criterio a equivalentes OR, sin borrar columnas con datos útiles solo por analogía. |
| 32 | Ocultar CARGA genérico cuando la carga solo se inicia por renglón. | OR conserva btnCargar; revisar su evento y ocultarlo en esa instancia, sin afectar continuar/control de salida. |
| 33 | Logs por etapa: contexto, BOX, producto, Valida/Carga/Lista request-response, errores y cierre de modo. | Logs parciales; agregar trazabilidad OR correlacionada sin tokens, cookies ni claves. |
| 34 | Recursos versionados y verificación tras compilar/publicar. | Algunos scripts OR no tienen asp-append-version. Agregarlo a los modificados. |

## Precauciones del contrato

1. **Nombres confirmados:** OR conserva SPGECO_OR_Carrito_Valida, SPGECO_OR_Carrito_Carga y SPGECO_OR_LISTA_PRODUCTOS. Sus contratos son equivalentes a TR, pero no se reemplazan los nombres por SPGECO_TR_*.
2. **Item en Valida, resuelto:** el usuario confirmó la firma vigente con @item. Se eliminó el método TR sin @item, ajeno a la interfaz y sin referencias; OR se adaptó a los SP suministrados. No confundirlo con el endpoint HTTP ValidaProductoCarrito, que continúa siendo utilizado y se conserva.
3. **Estados:** nuevos mensajes 02/03/04 no describen exactamente lo mismo que ciertos respaldos históricos. El mensaje SP tiene prioridad. No trasladar interpretaciones de texto a permisos.
4. **Total visual y permisos:** TR calcula la leyenda con cantidades visibles, pero habilita originales con colectado_x_p y colección propia. Conservar esta distinción hasta validar los significados equivalentes del response OR.
5. **Reglas particulares:** revisar vencimiento y BOX completo. OR tiene rama JS desarma=false, pero el controlador de carga fija desarma_box=true; no certificar esa rama por analogía.
6. **Campos ausentes:** agregar propiedades al DTO no acredita que el SP las devuelva. Verificar nombres y tipos, sin presentar ceros o estados completados ficticios por valores predeterminados.

## Secuencia objetivo (6 pasos)

1. Seleccionar OR, BOX/rubro y renglón; consultar estado actualizado y guardar identidad/original.
2. Carga normal o confirmación del reemplazo; validar BOX, buscar producto, obtener presentación/vencimiento.
3. Ingresar cantidades; resolver sobrescribir/acumular/cancelar cuando corresponda a colección propia previa.
4. Enviar Valida con contexto correcto y exclusivamente los parámetros de su firma.
5. Solo si es aceptado, ejecutar Carga; interpretar resultado funcional y cerrar modo reemplazo tras éxito.
6. Recargar Lista desde servidor, preservando filtros/orden, y presentar estado y originales/reemplazos devueltos.

## Plan de implementación

| Etapa | Trabajo | Criterio de cierre |
|---|---|---|
| 0. Cabecera común | TR/OR móvil compacto, controles de 44 px; ya codificado. | Compilación correcta; resta publicar y validar en colector. |
| 1. Contratos y datos | Confirmar nombres/firmas/response; actualizar ORProductoDto, ORCargaCarritoRequest y ORSessionDto; separar original y producto/BOX cargado. | Matriz de parámetros, tipos, nulos, ítem y mensajes verificada; ningún SQL editado. |
| 2. Operación y logs | Propagar reemplazo; identidad/propiedad; reconsulta; sobrescribir/acumular; fracciones y excesos; revisar vencimiento/desarma; logs de fronteras. | Casos normales/reemplazos/errores correctos sin cargas indebidas ni parámetros sobrantes. |
| 3. Lista y permisos | Reconsulta real tras carga/eliminación; filtros/orden; totales, leyendas, mensajes SP y referencias; acciones por usuario. | Mismos resultados funcionales que TR con response equivalente; ningún renglón equivocado. |
| 4. Presentación OR | Estilo opt-in de tarjetas; compactar títulos, radios y grillas; quitar controles sin función; proteger BOX/comprobantes; versionar recursos. | Escritorio, 360/390/480 px y colector real, sin desbordes. No confundir resolución física con viewport CSS. |
| 5. Certificación | Compilar Pocket/API afectados; checklist con datos de prueba autorizados; publicar y repetir críticos. | Evidencias y aprobación del tester. Control de salida conserva su decisión final. |

No se introduce la migración completa en un único cambio: cada etapa debe dejar una porción verificable y conservar TR.

## Checklist base OR

1. Pedido 500, propia 500: sin diferencia; mensaje SP.
2. Pedido 500, propia 400: falta 100.
3. Pedido 500, propia 600: sobra 100, sujeto a validación del servidor.
4. Otros 200 + propia 300: sin diferencia; propiedad correcta.
5. Otros 200 + reemplazo 300: sin doble conteo.
6. Propia previa 200 + nueva 300, acumular: propia final 500.
7. Propia previa 200 + nueva 300, sobrescribir: propia final 300.
8. Cancelar consulta de carga previa: no ejecutar Carga.
9. Reemplazar por mismo producto en otro BOX.
10. Reemplazar por otro producto en el mismo BOX.
11. Reemplazar por otro producto en otro BOX.
12. Cancelar inicio de reemplazo: no cambiar modo ni navegar.
13. Completo/excedido con colección propia: habilitación según regla.
14. Completo/excedido solo por otros: no habilitar nueva carga propia.
15. Modificar/eliminar reemplazo propio; impedir el de otro operador.
16. Producto repetido: actuar sobre ítem y BOX correctos.
17. BOX 10/11/12 dígitos, letras, pegado y foco posterior.
18. Producto/BOX incorrectos en carga normal: rechazar.
19. Enteros, decimales, fracción positiva menor de 1, cero, negativos y desglose inconsistente.
20. Rechazo en Valida: no ejecutar Carga; error de Carga: no mostrar éxito.
21. Código conocido/desconocido con mensaje; mensaje nulo/vacío/espacios: prioridad y respaldos correctos.
22. Recarga tras reemplazo en otro BOX con filtros por BOX y rubro.
23. Otro operador cambia colección entre listar/confirmar: reconsulta y decisión del servidor.
24. Dispositivo angosto, texto largo, tres acciones; BOX/comprobante completos.
25. Regresión TR, navegación y cabeceras de módulos no incluidos.

## Evidencia principal del código

- Pocket: Controllers/TrIntController.cs y ORController.cs; vistas TrInt y OR dentro de Areas/PocketPpal.
- JS: app/ti/tivalida.js; app/or/orvalidaProducto.js y orCoreCarrito.js; app/reemplazoProducto.js.
- DTO: Almacen/Tr/TiListaProductoDto.cs, TiProductoCarritoDto.cs; OrdenReparto/ORProductoDto.cs, ORCargaCarritoRequest.cs y ORSessionDto.cs.
- API: ApiProductoServicio.cs, OrdenRepartoServicio.cs, ApiProductoController.cs y ApiORController.cs.
- Visual: _headerApp.cshtml, _LayoutGestion.cshtml, site.css, golden-overlay.css, pocket-records.css y pocket-grids.css. Se conserva la identidad Golden existente, sin agregar dependencias ni otra paleta.
