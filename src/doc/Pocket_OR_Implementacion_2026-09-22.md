# Pocket OR: implementación de mejoras de TR

Fecha: 22/09/2026. Código: `D:\Sis25\git\GeConnect\src`.
Complementa la auditoría y el plan de esta fecha. No se modificó ningún SP ni se ejecutaron movimientos de inventario.

## Implementado

1. **Datos actuales y permisos:** selección, carga y eliminación reconsultan Lista por comprobante, item, producto y BOX, sin depender del filtro visual. La política ORColeccionReglas se comparte entre controlador y Razor. Originales completos/excedidos permiten operar a quien ya tiene colección propia; reemplazos controlan operador/estado; eliminar exige colección propia. Una selección nueva invalida el contexto de carga anterior, incluidas otras pestañas de la misma sesión.
2. **Carga previa:** diálogo Sobrescribir carga / Acumular / Cancelar. Se envía la cantidad nueva y una fotografía de la colección anterior; el servidor reconsulta, compara y calcula el total. Ante cambio devuelve el renglón actualizado y exige otra confirmación. No suma cantidades de otros operadores ni las del original cuando se inicia un reemplazo.
3. **Cantidades:** admite fracciones positivas menores de 1 para productos no unitarios; mantiene enteros para up_id 07 y verifica presentación/desglose. El exceso no se bloquea localmente: Valida/Carga y control de salida conservan su decisión. Bultos del DTO, acción y JS se manejan como decimal conforme al contrato recibido (hasta un decimal), sin parseInt que trunque silenciosamente. La acumulación exige la misma presentación y un desglose previo coherente.
4. **Seguridad del contexto:** requiere BOX validado, producto consultado correcto y contexto de selección vigente. BOX limita 11 dígitos y utiliza el valor canónico sugerido por el servidor también en el input y la consulta de vencimiento. No habilita BOX completo.
5. **Secuencia SP:** la función común ValidarYCargarOR no llama Carga cuando Valida rechaza. No se agregaron parámetros al SP: siguen siendo los 15 del contrato recibido, incluido item. modoCarga, snapshot y contexto son datos internos de la interacción web.
6. **Refresco:** la vista inicial renderiza su grilla con una sola consulta. Cambiar orden consulta de nuevo Lista. Carga/eliminación regresan al listado con BOX/rubro y orden conservados. Un listado vacío ya no se interpreta como filas de producto por tener una celda informativa.
7. **Grilla:** Pedido, Colectado, Reemplazo y Colectado Otros. En originales, Falta/Sobra se calcula con la suma visible; si coincide no se agrega leyenda. Reemplazos mantienen su significado, muestran referencia abreviada y no duplican la leyenda de diferencias. resultado_msj tiene prioridad incluso con código desconocido; texto local si está vacío y genérico si tampoco se conoce el código.
8. **Móvil:** tarjetas de TR reutilizadas para originales y reemplazos; BOX/rubro destacados, textos de una línea y comprobante/BOX protegidos. Se quitaron CARGAR genérico y título inicial redundante en móvil. Se compactaron itinerario, radios y grillas. Se conservan colores Golden, rutas OR, botón Volver y cabecera compacta existentes. No se agregaron dependencias visuales.
9. **Trazas:** categoría ILogger de OR corregida; etapas LISTA, RENGLON, VALIDA, CARGA, decisión y cambio de colección. Pocket registra TraceId; servicio HTTP registra comprobante, request, estado HTTP y response. Sin tokens/cookies. Buscar `[OR-TRACE]`. No se cambió el contrato SQL para instrumentar logs.
10. **Errores:** sin reenvío automático; el cliente evita doble click durante la carga y ante error de red pide revisar la lista. El estado de reemplazo solo se limpia tras éxito. La eliminación informa el producto del renglón, no un producto previamente consultado.

## Verificación

- Compilación Pocket y API en carpetas temporales fuera de los binarios de Visual Studio: 0 errores; existen advertencias.
- 53 pruebas ejecutables de las reglas C# reales mediante PowerShell.
- 14 pruebas JavaScript con servicios simulados (decisiones, payloads, cancelación, concurrencia, rechazo, éxito y error de red).
- 23 comprobaciones estáticas de contratos/integración: parámetros exactos, guardas, refresco y condiciones Razor.
- Sintaxis JS y git diff --check correctos.
- No hubo conexión a SQL ni operaciones reales. Estas pruebas no equivalen a certificar SP, transacciones, publicación o renderizado en el colector.

Pruebas en `gc.pocket.site/Tests`:

```powershell
./gc.pocket.site/Tests/ORColeccion.Servidor.Tests.ps1 -BuildDirectory '<salida compilada de Pocket>'
./gc.pocket.site/Tests/ORContratos.Tests.ps1
node gc.pocket.site/Tests/ORColeccion.Tests.cjs
```

## Pendientes explícitos

- **Vencimiento:** se conserva el comportamiento anterior de OR. Se solicitó confirmación para activar el control de fecha de TR; no se impuso esa regla sin respuesta.
- **BOX completo:** permanece fuera del flujo habilitado; no se activa desarma=false. Cualquier ampliación necesita el contrato vigente y pruebas específicas.
- **DBA:** validar las versiones instaladas de Lista/Valida/Carga con los ejemplos actuales. Las observaciones SQL históricas siguen como consultas, no se compensan modificando datos en Pocket. La reconsulta no crea una transacción atómica entre Lista, Valida y Carga; esa garantía depende del servidor.
- **Prueba real:** al verificar no había aplicación escuchando en 7176. Falta publicar/levantar API y Pocket, autenticarse y ejecutar el checklist en navegador y CN80. No se certificó visualmente la aplicación viva.

## Prueba manual prioritaria

1. Ingresar nuevamente a OR (no reutilizar una pestaña anterior a la publicación).
2. Producto con propia 200: nueva 300; verificar acumular 500, sobrescribir 300 y cancelar sin movimiento.
3. Producto pedido 30, propia 12, reemplazo 18, otros 0: sin Falta/Sobra, mensaje del SP.
4. Completo solo por otros: sin carga; completo con propia: carga habilitada. Reemplazo ajeno: sin modificación/eliminación.
5. Reemplazar por mismo producto/otro BOX y por producto distinto; revisar los 15 parámetros en el log y las filas devueltas.
6. Pesable 0,5 y exceso sobre pedido: llegan a validación; rechazo del SP no muestra éxito.
7. Cambiar colección desde otra sesión entre abrir y guardar: solicita revisar nuevamente; no acumula silenciosamente sobre datos viejos.
8. Cargar/eliminar y cambiar orden por BOX/rubro/producto: datos frescos, filtro y orden conservados.
9. Colector angosto: BOX/comprobante completos, acciones visibles, textos largos sin segunda línea y sin desbordes.

Referencias: [auditoría](Pocket_TR_OR_Auditoria_2026-09-22.md), [observaciones DBA](Pocket_OR_Contratos_y_Observaciones_DBA.md).
