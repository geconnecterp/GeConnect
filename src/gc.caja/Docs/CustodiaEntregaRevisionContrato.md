# Entrega de Valores en Custodia — contrato recibido y revisión SQL

Fecha: 2026-10-30. Raíz: D:/Sis25/git/GeConnect/src.

## Conexión implementada

Acceso: Inicio → Administrador de Caja → Entrega de valores. No requiere apertura del PV.

La aplicación conecta SPGECO_Vtas_PV_CTL_Entregas_Guardar con la firma literal recibida y cotejada por SELECT de metadatos de la base local de Caja:

| Parámetro | Tipo | Origen |
| --- | --- | --- |
| @ent_resp_entrega | varchar(60) | Responsable de la entrega, recortado en extremos |
| @ent_resp_recibe | varchar(60) | En custodia de / quien recibe |
| @ent_compte_retiro | varchar(60) | N.º de comprobante de entrega |
| @usu_id | varchar(10) | Usuario autenticado |
| @adm_id | varchar(10) | Sucursal autenticada, contrastada con configuración del puesto |
| @json_rend | varchar(max) | Arreglo con los 14 campos de CUST-05, reconstruidos desde pendientes |

Los tres datos de cabecera admiten de 4 a 60 caracteres; la aplicación rechaza excedentes sin truncar. @app='CAJA' sigue perteneciendo exclusivamente a SPGECO_Vtas_PV_CTL_Guardar: el nuevo SP de entrega no declara ese parámetro.

La entrega envía todas las filas seleccionadas, modificadas o no, incluyendo PES y DOL en una sola llamada. El SP actualiza rend_importe_ok dentro del mismo registro; no se ejecuta previamente una corrección independiente. Se comprueba vigencia en una transacción exterior antes del envío, se conserva el JSON de arqueo/indicadores y se valida la respuesta antes del COMMIT exterior.

Un rechazo explícito devuelve su mensaje sin convertirlo en éxito. Un OK debe incluir resultado_id no vacío, de hasta 20 caracteres, que conserva sus ceros. Si falta, se revierte la transacción exterior cuando sigue disponible; si no se puede confirmar la reversión, se informa resultado incierto y se bloquea el reenvío automático. El acuse consulta SPGECO_ENT_Datos usando ese ID, sin repetir el registro.

## Observaciones concretas para el DBA

Referencia: script entregado en dd867e1f-e7ca-4bff-ab3f-86d31fb387ee/Texto pegado.txt, con encabezado de siete líneas. Las definiciones recuperadas de la base por SELECT tienen siete líneas menos.

1. **Retorno incondicional, líneas 72–77 del adjunto (65–70 de la definición).** Después de validar la cabecera hay un BEGIN sin IF que fija resultado=2, emite «Debe informar las rendiciones.» y ejecuta RETURN. Toda entrada válida termina aquí; no alcanza el registro. Corresponde al DBA retirar o condicionar ese bloque.
2. **Columna inexistente, líneas 122 y 145 (115 y 138 de la definición).** Las dos expresiones SUM(importe) leen #rend, pero su OPENJSON sólo declara rend_importe_arq y rend_importe_ok. El DBA debe definir el campo monetario correcto; por el contrato, el importe entregado es el OK corregido. No agregar un campo importe al JSON desde Caja para ocultar el problema.
3. **ID de resultado sin asignación, líneas 22–23 y 179–181.** @resultado_id nace vacío y nunca recibe @ent_compte, aunque éste se genera en la llamada a SPGECO_Compte_GECO (línea 97). El éxito debe devolver el identificador generado para consultar/imprimir el acuse. No adivinarlo desde Caja.
4. **Dos monedas, para confirmar con el modelo real.** El detalle agrupa por instrumento, pero fija ent_nro_item=1 para cada grupo (línea 143); verificar su clave real cuando se incluyan PES y DOL. También definir el significado del importe de cabecera si hay dos monedas: la aplicación muestra sus totales separados y no introduce una cotización.
5. **Pendientes anteriores conservados.** El SP de consulta no aplica @adm_id/@ins_id, por lo que la aplicación filtra y limita la lectura a 25 s. SPGECO_ENT_Datos omite filas con importe OK cero: revisar el acuse del faltante total admitido funcionalmente.

Los puntos 1–3 se observaron tanto en el adjunto como en la definición recuperada. El punto 4 requiere confirmación del DBA sobre estructura y uso posterior; no se declara una violación de clave comprobada. No se modificó ni ejecutó ningún SP de escritura.

## Verificación y alcance

65 comprobaciones JS y 102 de servicio/API/contrato/DI/PDF con datos ficticios aprobadas. La firma y los 14 campos se compararon directamente contra el script adjunto. Incluye selección de ambas monedas, filas sin modificar, cero, límites de cabecera, rechazo del SP, identidad, versiones, ID ausente y reenvíos. La prueba no certifica transacciones SQL ni una entrega real. Scripts y evidencia permanecen privados en .artifacts.

Antes de la prueba integrada: recompilar Caja y API; el DBA debe revisar los tres defectos de registro y los filtros de consulta. El diseño del acuse es preliminar hasta recibir el modelo definitivo.

## Revalidación integrada — 2026-11-01

Caja y API recompiladas por el responsable; sesión autenticada de sucursal 0000. Acceso por menú administrativo y regreso sin perder sesión verificados. La consulta devuelve HTTP 503 tras 26,37 s, por cancelación del lector a los 25 s; pantalla informa fallo, habilita Actualizar y mantiene guardado/entrega bloqueados.

Se volvieron a leer los metadatos y definiciones: los puntos 1–3 y los filtros de consulta continúan pendientes. El catálogo sólo contiene PES; DOL no se simula ni se agrega a la base. La tabla cajas_rendiciones tiene aproximadamente 4.764.842 filas totales según metadatos. Eso no es un conteo de pendientes: el SELECT limitado a sucursal también agotó 10 s. El DBA debe revisar filtros y rendimiento/esperas, y disponer de filas adecuadas para la prueba. No se puede concluir que no haya pendientes.

No se modificaron SP ni código funcional y no se registraron correcciones o entregas. Selección real, escritura y acuse continúan pendientes del desbloqueo de la consulta. Las comprobaciones simuladas anteriores no se repitieron ni se presentan como aceptación SQL real.
