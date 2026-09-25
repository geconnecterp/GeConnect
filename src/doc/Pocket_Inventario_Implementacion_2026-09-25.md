# Inventario de Pocket — implementación del 25/09/2026

## Alcance

Código en `D:\Sis25\git\GeConnect\src`. Se implementan las observaciones del tester y el plan acordado para el conteo de Inventario de Pocket. No se modificaron SP, tablas ni funciones administrativas del GECO principal. Los registros en la tabla `a` son diagnóstico del DBA y no forman parte del flujo funcional.

## Circuito implementado

1. Seleccionar inventario y BOX o planilla. Para planilla nueva se mantiene `tipo_id = "0"`.
2. Validar el contexto con `SPGECO_INV_Conteos_Valida`. La API toma usuario/sucursal del token y comprueba que el inventario esté disponible en la lista activa del usuario.
3. Recuperar el snapshot mediante `SPGECO_INV_Conteos`. Un BOX con conteo previo ofrece recuperar, comenzar desde cero o cancelar. Comenzar desde cero no borra la base.
4. Buscar el producto. En Inventario se permiten activos (`S`) y discontinuados (`D`); no se habilitan otros estados ni se cambia el comportamiento de TR/OR.
5. Antes de habilitar la carga, invocar `SPGECO_INV_Carrito_Valida` con inventario, producto y usuario autenticado. Presentar el rechazo del SP, si corresponde.
6. Cargar Bultos, U. presentación y US. `up_tipo = P` utiliza US con hasta tres decimales, bultos cero y presentación uno; los demás tipos utilizan unidades enteras y total = Bultos × U. presentación + US. Si falta `up_tipo`, no se supone un tipo.
7. Para un producto repetido, mostrar Acumular / Reemplazar / Cancelar carga. Acumular conserva el total; si cambia la presentación, lo normaliza a la nueva presentación y lo informa en el modal. Reemplazar modifica cantidades del mismo producto, no el producto.
8. Confirmar la grilla completa. La API vuelve a validar contexto, pertenencia y cantidades, consulta los metadatos actuales del producto y reconstruye el JSON.
9. Invocar `SPGECO_INV_Conteos_Confirma` con los cinco parámetros existentes. Informar éxito solo ante respuesta válida; conservar la grilla ante un rechazo.

## Contratos

| SP | Parámetros |
|---|---|
| `SPGECO_INV_Carrito_Valida` | `@inv_nro`, `@p_id`, `@usu_id` |
| `SPGECO_INV_Conteos_Valida` | `@inv_nro`, `@tipo`, `@tipo_id`, `@usu_id` |
| `SPGECO_INV_Conteos` | `@inv_nro`, `@tipo`, `@tipo_id`, `@usu_id`, `@p_id` |
| `SPGECO_INV_Conteos_Confirma` | `@inv_nro`, `@tipo`, `@tipo_id`, `@usu_id`, `@json_p` |

Cada renglón del JSON de confirmación contiene únicamente `p_id`, `p_desc`, `up_id`, `invd_unidad_pres`, `invd_bulto`, `invd_unidad_suelta` e `invd_cantidad`. El JSON recibido del navegador no se transmite ciegamente al SP. BOX, planilla y usuario son determinados por los parámetros generales del procedimiento.

La respuesta de conteos incorpora `up_tipo`, `up_desc`, `usu_apellidoynombre` e `inv_grupo`. Los valores numéricos se conservan en atributos sin formato para no releer cantidades desde textos con separadores de presentación.

## Salvaguardas

- No se permite confirmar sin productos, en pantalla, MVC, API y servicio de persistencia. El snapshot guardado permanece intacto.
- Confirmar reemplaza TODO el conteo del usuario en ese BOX/planilla; quitar un producto del snapshot lo elimina al confirmar el resto.
- Se rechazan cantidades incoherentes, productos repetidos en el request, usuarios comodín y modalidades fuera de BOX/planilla.
- Se impide el doble envío. Ante un resultado incierto se conserva la grilla pero no se habilita el reintento: primero se deben revisar los conteos guardados, particularmente para una planilla nueva.
- No se inventó una validación del producto contra existencias del BOX. La regla sigue pendiente de definición; los SP entregados validan otros aspectos del BOX.
- No se modificaron los contratos de TR/OR ni las funciones compartidas de entrada de cantidades. Inventario adapta su configuración según `up_tipo`.

## Archivos principales

- `gc.pocket.site/Areas/Gestion/Controllers/InventarioController.cs`: acceso y validación de producto; resultado de confirmación.
- `gc.pocket.site/Areas/Gestion/Controllers/ProductoController.cs`: excepción de discontinuados exclusiva de Inventario.
- `gc.pocket.site/Areas/Gestion/Views/Inventario/CargaConteo.cshtml` y `_gridInvProductos.cshtml`: campos y metadatos separados.
- `gc.pocket.site/wwwroot/js/app/gestion/inv-conteo.js` e `inv-conteo-reglas.js`: interacción, cantidades, snapshot y confirmación.
- `gc.api/Controllers/Almacen/ApiInventarioController.cs`: controles del request y contexto autenticado.
- `gc.api.core/Servicios/InventarioServicio.cs`: parámetros exactos y JSON del SP.
- `gc.sitio.core/Servicios/Implementacion/InventarioServicio.cs`: comunicación y tratamiento estricto de respuesta.
- `gc.infraestructura/Dtos/Inventario/InventarioConteoReglas.cs`: reglas de cantidades y contexto comprobables sin base de datos.
- Interfaces de Inventario, constantes y DTO de conteos: nuevo método y campos del contrato.

## Verificación automatizada

- `gc.pocket.site/Tests/InventarioConteo.Tests.cjs`: 33 casos; cantidades, normalización, duplicados, validación, recuperación, snapshot, doble envío y errores. DOM y API simulados.
- `gc.pocket.site/Tests/InventarioConteo.Servidor.Tests.ps1`: 18 casos sobre las reglas C# compiladas, sin base de datos.
- `gc.pocket.site/Tests/ORColeccion.Tests.cjs`: 14 pruebas existentes de regresión, con servicios simulados.
- Compilación de Pocket y API con salida separada en sus carpetas `bin/inventario-validation/`, sin iniciar procesos web.

Comandos desde la raíz autorizada:

```powershell
node gc.pocket.site/Tests/InventarioConteo.Tests.cjs
node gc.pocket.site/Tests/ORColeccion.Tests.cjs
./gc.pocket.site/Tests/InventarioConteo.Servidor.Tests.ps1 -BuildDirectory ./gc.pocket.site/bin/inventario-validation
```

## Pendiente de certificación con el tester

No se ejecutaron confirmaciones reales ni se certificó la visualización en el colector. Es necesario recompilar/actualizar tanto Pocket como API y realizar:

- [ ] Producto permitido/rechazado por rubro y proveedor; discontinuado permitido e inactivo rechazado.
- [ ] BOX inexistente, ajeno al depósito y contado por otro usuario.
- [ ] Planilla nueva y recuperación de una existente.
- [ ] Bultos, solo US, combinación y pesable; presentación diferente al acumular.
- [ ] Acumular, reemplazar y cancelar; comprobar persistencia recuperando el conteo.
- [ ] Eliminar un renglón conservando los demás; bloquear eliminación total mediante confirmación vacía.
- [ ] Recuperar BOX y comenzar desde cero; cancelar sin alterar lo guardado.
- [ ] Prueba de pantalla angosta, teclado físico, escáner y foco.
- [ ] Mensajes reales de rechazo de los SP y prueba controlada de desconexión.

Observaciones para el DBA, sin cambios SQL de nuestra parte: condición `inve_id`/`invt_id` en Carrito_Valida, nombres de columnas del CATCH de confirmación, concurrencia de `MAX + 1` y de conteos simultáneos. Las validaciones previas de la aplicación no sustituyen un control transaccional de concurrencia en base de datos.

## Recuperación de despliegue

Publicar Pocket y API como conjunto compatible. Si la prueba revela una incompatibilidad con los SP instalados, detener la prueba de carga y restaurar la versión anterior de ambas aplicaciones mediante el procedimiento habitual de despliegue. No aplicar cambios ni reversiones SQL como parte de este trabajo.
