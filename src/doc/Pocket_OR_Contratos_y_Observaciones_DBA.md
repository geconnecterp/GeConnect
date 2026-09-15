# Pocket OR: contratos recibidos y observaciones para DBA

12/09/2026. Fuentes: SP Valida pegado en la conversación y adjuntos Carga/Lista del usuario. Lectura estática; no se ejecutó ni modificó SQL.

## Contrato confirmado

Valida y Carga conservan los nombres SPGECO_OR_Carrito_Valida y SPGECO_OR_Carrito_Carga. Ambos reciben:

| Parámetro | Tipo SQL |
|---|---|
| @or_compte | varchar(20) |
| @adm_id | varchar(4) |
| @usu_id | varchar(10) |
| @item | smallint |
| @box_id | varchar(20) |
| @desarma_box | bit |
| @p_id | varchar(10) |
| @unidad_pres | smallint |
| @bulto | decimal(15,1) |
| @us | decimal(15,3) |
| @cantidad | decimal(15,3) |
| @fv | varchar(20) |
| @remplazar | bit |
| @remplazar_box_id | varchar(20) |
| @remplazar_p_id | varchar(10) |

Valida declara @remplazar=1 por defecto y Carga=0: la aplicación debe enviarlo explícitamente en ambos, sin depender de esos defaults. Ambas respuestas son resultado y resultado_msj; 0 es éxito. Carga solamente debe ejecutarse si Valida devuelve éxito funcional.

Lista recibe @or_compte, @adm_id, @usu_id, @box_id y @rub_id. Devuelve or_compte (no ti), item, producto/BOX/rubro, pedido, remplazo y referencias, colectado, bulto, us, unidad_pres, colectado_otros, colectado_x_box, colectado_remplazo, colectado_x_p, resultado y resultado_msj, entre los campos de contexto.

La integración ya incorpora ítem, alias or_compte y campos nuevos. Se preservan los SP. La maqueta y resto del plan OR aún no se completaron.

## Consultas al DBA (no son cambios aplicados)

1. **Lista: origen del total de control.** El bloque `#ctl_tot` consulta `From ti_d Where ti = @or_compte`. ¿Es intencional cruzar una OR con transferencias, o quedó de la adaptación de TR? Podría impedir o alterar los estados 03/04 si no existe una TI coincidente.
2. **Lista: código 03 reutilizado.** Se asigna 03 tanto a `OK Colectado con Remp` como, posteriormente, a `OK Cumple PI`; el comentario menciona 02 para el primero. ¿Cuál es el contrato definitivo? Pocket prioriza resultado_msj y no modifica el mensaje recibido.
3. **Lista: condición de exceso propio.** El estado 20 usa `pedido < colectado_otros + colectado_remplazo` y exige colectado > 0, sin sumar colectado propio en esa comparación. ¿Es correcto? Un exceso exclusivamente propio podría llegar a la regla 21 (`Colectado Demás x Otros`). La leyenda numérica de Pocket no debe cambiar el código SP para esconder esa discrepancia.
4. **Carga: comprobante de pedido cliente.** En `Insert pedidos_clientes_d (pc_compte, ...) Select @or_compte, ...` se utiliza el comprobante OR como pc_compte. ¿Es correcto frente a a.pc_compte y @remplazar_pc_compte? Confirmar también el alcance/cantidad cuando la OR contiene varios pedidos del mismo producto.
5. **Carga: granularidad de conteos.** Las operaciones de or_conteos se identifican por OR/usuario/BOX/producto, sin item; las modificaciones de or_d sí usan item. ¿Cómo debe manejar Pocket dos renglones con la misma combinación de producto/BOX? Enviar item evita selección ambigua en la aplicación, pero no aísla por sí solo conteos que el SP comparte.
6. **Carga: eliminación sin stock.** La validación de stock está antes de la rama cantidad=0. Si el stock es cero, una eliminación podría rechazarse antes de llegar al Delete. Confirmar el comportamiento esperado.
7. **Carga: transacción de BOX completo.** Hay Return en la rama desarma_box=0, después de Begin Transaction, sin Commit/Rollback local en esos rechazos. Confirmar el cierre de la transacción en ese flujo antes de habilitar/pruebar BOX completo.
8. **Valida: cantidades de otros.** La consulta de @cantidad_otro filtra estado P, usuario distinto y BOX, pero no p_id ni el comprobante actual; la OR activa se valida en estado O. ¿Se pretende sumar todas esas reservas o la colección del producto/OR? No se cambiará desde Pocket.

Estas observaciones necesitan confirmación del DBA; no constituyen pruebas de un incidente ya reproducido. Las pruebas de stock, reemplazo y eliminación deben realizarse con datos preparados y supervisión del usuario/tester.
