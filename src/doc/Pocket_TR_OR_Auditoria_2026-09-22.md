# Pocket: auditoría de paridad TR → OR y plan de cierre

> Avance posterior del mismo día: [implementación, pruebas y pendientes de certificación](Pocket_OR_Implementacion_2026-09-22.md). La matriz siguiente conserva el estado previo a la implementación para trazabilidad.

Fecha: 22/09/2026. Base examinada: commit `fb5064d2`, árbol limpio al iniciar.
Raíz: `D:\Sis25\git\GeConnect\src`.

## Conclusión y alcance

**No se trasladaron todas las mejoras.** OR tiene la integración inicial de reemplazo, identidad por ítem, contratos y cabecera compartida; faltan reglas operativas, concurrencia, presentación del response y parte de la adaptación móvil.

Esta es una revisión estática del código actual, no una certificación de la aplicación publicada ni de los SP instalados. No se compiló, no se ejecutaron movimientos ni se modificó código funcional o SQL en esta revisión. Los estados «aplicado» indican presencia en código, no prueba funcional aprobada.

Se compara TR entre sucursales con el flujo de colecta de órdenes de reparto OR. Se conservan la finalidad de OR, sus comprobantes, rutas, SPGECO_OR_* y control de salida. No se convierte OR en una transferencia ni se copian restricciones exclusivas de otros tipos de TR.

Este informe es el diagnóstico vigente. El documento del 12/09 conserva la matriz inicial y sus avances como historial.

## Matriz de estado actual

| Mejora de TR | Estado en OR | Evidencia / trabajo pendiente |
|---|---|---|
| Botón naranja de reemplazo y confirmación estándar `warn!` con producto/BOX original y cancelar | Aplicado | `_gridORListaProducto.cshtml` incluye `btnReemplazarProducto`; `reemplazoProducto.js` compartido usa `AbrirMensaje`. |
| Terminología reemplazo | Aplicado en ciclo revisado | Sin coincidencias de «sustitut» en controlador, vistas y JS OR inspeccionados. Los textos del SP siguen siendo responsabilidad del DBA. |
| Banner de modo reemplazo y contexto original separado del BOX cargado | Aplicado | `ORValidaProducto`, `ORSessionDto` y vista de validación conservan original y `BoxCargaId`. |
| Mismo producto en otro BOX o producto distinto en reemplazo | Aplicado localmente | `ValidarProductoIngresado` permite el producto en modo reemplazo; SP conserva la decisión de aceptación. |
| Identidad comprobante + item + producto + BOX | Aplicado, con límites | Enlaces, DTO, sesión, carga y eliminación incluyen item; selección todavía usa datos de sesión. |
| Item y los tres parámetros de reemplazo en Valida/Carga | Aplicado | `OrdenRepartoServicio` envía los 15 parámetros en ambas llamadas; mantiene SP OR. |
| Valida antes de Carga y éxito funcional, no solo HTTP 200 | Aplicado | `ORServicio` usa `resultado == 0`; el controlador no continúa si Valida rechaza. |
| Nuevos campos del listado y alias `or_compte` | Aplicado al DTO | `ORProductoDto` contiene estados, referencias, otros, reemplazo y totales; la grilla aún no los presenta. |
| BOX máximo 11 dígitos y foco en búsqueda tras validar | Aplicado | Vista con maxlength/inputmode, JS limpia no dígitos y limita a 11; foco posterior en `#Busqueda`. |
| Formato entero para up_id 07 y decimales para otros | Parcial | Formato y consistencia básica existen, pero el controlador rechaza cualquier cantidad menor de 1. |
| Acumular / Sobrescribir carga / Cancelar ante colección propia previa | Pendiente | TR tiene modal y `modoCarga`; OR envía directamente la cantidad y no dispone de esa decisión. |
| Fracciones positivas menores de 1 | Pendiente | `ORController`, línea 1184: `cantidad < 1`; trasladar criterio `cantidad <= 0` para colecta individual y validar unidad. |
| Flexibilidad para superar el pedido | Pendiente | `ORController`, líneas 1198–1200: bloqueo local para up_id 07. JS conserva una condición heredada con `sinAU`, propiedad ausente en ORProductoDto. |
| Original completo/excedido: operar solo si hay colección propia | Pendiente | Botones de carga/reemplazo siempre renderizados; no hay regla equivalente a `ObtenerRenglonSucursal` en OR. |
| Reemplazos: propiedad del operador, estados permitidos y eliminación propia | Pendiente | La grilla no distingue renglones originales/reemplazos; el servidor no hace comprobación equivalente de `remplazo_usu_id`. No basta ocultar botones. |
| Reconsultar renglón antes de seleccionar/guardar/eliminar | Pendiente | OR busca en `ORListaProductosActual`; TR consulta Lista y aplica permisos actualizados. |
| Refresco posterior a carga/eliminación | Parcial, no ausente | JS vuelve a `ORCargaCarrito`, que consulta API/SP y recupera filtros de sesión. El orden se reinicia a BOX. |
| Refresco al cambiar orden de grilla | Pendiente | `BuscaORListaProductos`, línea 751, solo ordena sesión; TR consulta servidor en su acción equivalente. |
| Pedido, Colectado, Reemplazo y Colectado Otros, incluidos ceros | Pendiente en grilla | OR muestra pedido y solo colección positiva. Los datos adicionales ya existen en DTO. |
| Falta/Sobra = Pedido − (Colectado + Reemplazo + Colectado Otros); cero sin leyenda | Pendiente | Sin cálculo equivalente en grilla OR. Aplicar a originales; no alterar significado de renglones de reemplazo. |
| Mensaje SP prioritario, respaldo local y texto genérico | Pendiente | OR no presenta `resultado`/`resultado_msj` en la tarjeta. Mensaje no vacío debe mostrarse aun con código desconocido. |
| Tarjeta compacta idéntica para originales y reemplazos | Pendiente | TR usa `pocket-record--collection`; OR conserva fila flex con badges, sin estado/referencia. |
| Referencia abreviada `Rem. producto BOX - (usuario)` | Pendiente | DTO disponible, no se renderiza. |
| BOX/rubro en negrita | Aplicado | Ya se destacan en OR. Conservar en nueva tarjeta. |
| Una línea con elipsis y BOX/comprobante completos | Parcial | Falta composición equivalente en tarjeta y protección explícita del comprobante en `_NroComprobante`. Verificar anchos reales. |
| Cabecera compacta, Volver móvil y título redundante del layout oculto | Aplicado en código compartido | `_headerApp` aplica clase a TrInt y OR; `site.css` contiene cabecera móvil de 52 px nominales y controles de 44 px. No medido en esta revisión. |
| Compactación interior de itinerario, radios y grillas | Parcial | OR ya usa `pocket-grid` y encabezados compactos; mantiene `gap-4 my-3`, títulos interiores y estructura distinta a TR. No eliminar columnas útiles por copia mecánica. |
| Quitar CARGAR genérico en listado de productos | Pendiente | `ORCargaCarrito.cshtml` mantiene `#btnCargar`; sin manejador localizado en JS del módulo. No confundir con `#btnCargarProd` ni Continuar. |
| Logs por etapas request/response | Parcial | Hay logs OR de selección/listado/errores, pero no el seguimiento de Valida/Carga/Lista equivalente a TR. El controlador usa además `ILogger<TrIntController>`. |
| Versionado de recursos | Parcial | JS de carga, validación y reemplazo versionados; `orCoreCommon.js` y `busquedas.js` no lo están en las vistas revisadas. |

## Riesgos adicionales a resolver sin copiar errores

1. **Producto en carga normal:** la búsqueda valida el producto, pero el POST final toma `p_id` recibido sin verificar que coincida con el original cuando no se reemplaza. TR sí repite esa comprobación. Incorporarla junto con comprobante, ítem y BOX del contexto.
2. **Bultos:** request OR declara decimal, pero el response DTO y la acción usan int y JS usa parseInt. El contrato SQL recibido declara decimal(15,1). Confirmar el dominio admitido; evitar truncar o redondear si el SP devuelve fracciones. No ampliar silenciosamente la regla de ingreso de bultos.
3. **Vencimiento:** TR controla la fecha cuando el producto lo requiere; en OR esa validación está comentada. Confirmar equivalencia antes de activarla y conservar formato contractual de fecha.
4. **BOX completo:** OR tiene rama JS desarma=false, pero la vista mantiene el switch deshabilitado y marcado; la acción no recibe desarma y fija true. No habilitar esa rama como efecto colateral del traslado de colecta individual.
5. **Concurrencia:** reconsultar reduce datos obsoletos, pero no garantiza atomicidad entre consulta, Valida y Carga. Acumulación simultánea necesita la definición del DBA; no inventar parámetros ni garantizar que no haya actualizaciones perdidas.
6. **TR no es una especificación infalible:** su acción de lista ordena la copia en sesión, pero pasa `regs` a construir la grilla. OR debe conservar el orden seleccionado efectivamente, sin copiar esa divergencia. El estado/permiso debe distinguir código 40 y marca de reemplazo consistentemente.
7. **SQL histórico:** las observaciones del 12/09 (referencia a ti_d, código 03 repetido, cálculo de exceso, conteos por producto/BOX frente a ítem, etc.) no demuestran el estado actual del servidor. Pedir confirmación de versión al DBA, sin editar SP ni compensar anomalías con datos inventados.

## Plan ordenado de implementación

### Etapa 1 — Fijar contrato y pruebas de referencia

- Conservar SPGECO_OR_* y sus firmas; registrar ejemplos anonimizados de Lista/Valida/Carga vigentes, con item, originales, reemplazos propios/ajenos y estados conocidos/desconocidos.
- Confirmar significados de cantidades (colección propia/otros/reemplazo y colectado_x_p), granularidad por item y tipos/nulos. Contrastar bultos y vencimiento.
- Mantener flujo individual; BOX completo queda separado si se decide habilitarlo.
- Crear pruebas de contrato y fixtures con las firmas ya implementadas: no rehacer el transporte de item.

**Cierre:** pruebas del request de 15 parámetros y del response; ninguna llamada sin item ni cambio de nombres SP. Acuerdos pendientes DBA explícitos, sin frenar ajustes independientes de UI.

### Etapa 2 — Datos actuales, permisos y trazabilidad (prioridad alta)

- Crear consulta de renglón OR actual por comprobante/item/producto/BOX, independiente del filtro visual, tomando TR como referencia.
- Reutilizar una política de permisos OR en servidor y presentación: originales completos/excedidos editables para quien ya colectó; reemplazos según operador/estado; eliminar solo colección propia.
- Validar también el producto del POST normal; invalidar contexto si el ítem desapareció o cambió.
- Incorporar trazas OR correlacionadas de selección, validación, carga y lista, con parámetros/resultados necesarios, sin tokens, cookies ni secretos; corregir categoría de logger. No volcar datos de clientes innecesarios.

**Archivos:** ORController, ORSessionDto, ORServicio, API/servicio OR si se requiere registrar la frontera SP.
**Cierre:** petición directa o pantalla obsoleta no omiten permisos; Valida rechazado nunca dispara Carga; trazabilidad de una operación completa.

### Etapa 3 — Carga y reemplazo (prioridad alta)

- Agregar consulta Sobrescribir carga / Acumular / Cancelar cuando haya colección propia previa; mostrar total resultante y diferenciarlo de reemplazar un producto.
- Acumular solo colección propia con desglose coherente y nueva lectura; si cambió mientras se decidía, volver a confirmar. `modoCarga` es contexto de aplicación, no un parámetro SQL nuevo.
- Aceptar cantidades positivas fraccionarias para unidades que lo permiten. Mantener enteros en up_id 07 y coherencia bultos/unidades.
- Quitar bloqueo local por exceso para esta colecta; seguir respetando Valida/Carga y decisión final de control de salida.
- Mantener original y producto cargado separados; no impedir mismo producto en otro BOX. Limpiar modo solo al éxito, no ante rechazo.
- Corregir el texto de eliminación usando el renglón operado, no un ProductoBase eventualmente anterior.

**Archivos:** ORController, orvalidaProducto.js, ORValidaProducto.cshtml; DTO si los tipos confirmados lo exigen.
**Cierre:** sobrescribir, acumular, cancelar, fracciones y los tres escenarios de reemplazo pasan pruebas; errores no muestran éxito.

### Etapa 4 — Refresco y presentación del estado

- Hacer que BuscaORListaProductos consulte Lista, conservando filtros y orden. Evitar dos consultas idénticas al entrar a la vista: definir un único responsable de refresco inicial.
- Volver de carga/eliminación con filtros y orden conservados. No ocultar mediante estado local un reemplazo devuelto por el SP ni inventar filas si queda fuera del filtro.
- Mostrar cuatro cantidades en originales y datos adecuados en reemplazos, sin totales redundantes. Falta/Sobra por suma visible; si cero no agregar leyenda.
- Mostrar resultado_msj no vacío primero, luego respaldo por código; desconocido sin mensaje: «Estado informado por el servidor». Colores/permisos no dependen del texto.
- Mostrar original y operador de reemplazo con referencia abreviada.

**Archivos:** ORController, ORSessionDto, orCoreCarrito.js, _gridORListaProducto.cshtml.
**Cierre:** ejemplo 30 pedidos, 12 propios, 18 reemplazo, 0 otros no muestra «Falta»; recarga y orden comprobados después de cada operación.

### Etapa 5 — Adaptación móvil y limpieza visual

- Reutilizar estilos de tarjeta de TR para ambos tipos de renglón, manteniendo identidad visual y rutas OR; cantidades a la izquierda, acciones/estado a la derecha.
- Conservar negritas BOX/rubro, proteger BOX y comprobante completos, elipsis en textos extensos y ayuda con texto íntegro.
- Quitar CARGAR genérico del listado, no los botones por producto. Compactar radios, títulos redundantes e itinerario, manteniendo columnas propias útiles de OR.
- Reutilizar cabecera compacta existente, no reconstruirla. Versionar recursos intervenidos.
- Verificar 320/360/390/480 px CSS y escritorio; medir viewport real del CN80. La resolución física 480×854 no determina por sí sola el viewport CSS.

**Cierre:** capturas de original y reemplazo, descripciones largas, tres acciones y estados; sin cortes de BOX/comprobante ni acciones inaccesibles. Prueba física del tester.

### Etapa 6 — Certificación y publicación

- Compilar API y Pocket, pruebas de contratos/reglas y sintaxis JS; revisión de cambios acotados a OR y recursos compartidos necesarios.
- Probar datos preparados con usuario/tester y repetir regresión TR. No ejecutar movimientos reales para verificar solamente apariencia.
- Publicar API y Pocket compatibles; renovar sesión/listado OR y caché de recursos. Registrar versión y evidencias de aceptación.

**Cierre:** checklist crítico aprobado; diferencias DBA abiertas identificadas por caso. No declarar paridad total solo por compilar.

## Checklist mínimo de aceptación

| Grupo | Casos |
|---|---|
| Cantidades | Pedido 500: propia 500, 400 y 600; otros 200 + propia 300; otros 200 + reemplazo 300. |
| Carga previa | Propia 200 + nueva 300: acumular 500, sobrescribir 300, cancelar sin Carga; cambio concurrente mientras se confirma. |
| Reemplazo | Mismo producto/otro BOX; distinto producto/mismo BOX; distinto producto/otro BOX; cancelar modo y rechazo del SP. |
| Permisos | Completo/excedido con colección propia y solo ajena; reemplazo propio/ajeno; eliminar sin colección propia; intento directo por HTTP. |
| Identidad | Producto repetido en varios ítems/BOX; item ausente, desaparecido o cambiado; producto distinto en POST normal. |
| Unidades | up_id 07 entero, no 07 con 0,5; cero/negativos; desglose incoherente; bultos según contrato acordado. |
| Respuestas | Valida resultado 2 sin Carga; Carga resultado 2 sin éxito; error HTTP/nulo; código desconocido con mensaje y sin mensaje. |
| Vista | 30 = 12 + 18 + 0 sin Falta/Sobra; falta y exceso; fila reemplazo sin doble cómputo; BOX/comprobante completos. |
| Refresco | Cargar/eliminar/cambiar orden; filtros BOX/rubro; reemplazo en otro BOX; otro usuario modifica desde otra sesión. |
| Regresión | TR entre sucursales sin cambios; OR conserva navegación/continuar/control de salida; colector real y escritorio. |

## Evidencia principal para navegar el código

- `gc.pocket.site/Areas/PocketPpal/Controllers/ORController.cs`: 699 consulta inicial; 751 orden en sesión; 841 selección; 1099 eliminación; 1163 carga; 1184 fracciones; 1198 exceso.
- `gc.pocket.site/Areas/PocketPpal/Controllers/TrIntController.cs`: BuscaTIListaProductos, ObtenerRenglonSucursal y ResguardarProductoCarrito.
- `gc.pocket.site/wwwroot/js/app/ti/tivalida.js`: decisión de colección previa, desde línea 168. Contraste con `wwwroot/js/app/or/orvalidaProducto.js`, cargarCarritoOR.
- `gc.pocket.site/Areas/PocketPpal/Views/TrInt/_gridTIListaProducto.cshtml` frente a `Views/OR/_gridORListaProducto.cshtml`.
- `gc.infraestructura/Dtos/OrdenReparto/ORProductoDto.cs`, ORCargaCarritoRequest.cs y ORSessionDto.cs.
- `gc.api.core/Servicios/OrdenRepartoServicio.cs`: Valida/Carga desde líneas 167/206.
- `gc.sitio.core/Servicios/Implementacion/ORServicio.cs`: interpretación de resultado en líneas 324/369.
- `gc.pocket.site/Views/Shared/_headerApp.cshtml` y `wwwroot/css/site.css`, cabecera compacta desde línea 1292.

Referencias históricas: [plan del 12/09](Pocket_TR_a_OR_Plan_Implementacion.md) y [consultas al DBA](Pocket_OR_Contratos_y_Observaciones_DBA.md).
