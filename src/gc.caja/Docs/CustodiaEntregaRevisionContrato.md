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

## Diagnóstico del rechazo actual — 2026-11-02

Base efectiva consultada por el diagnóstico con la configuración de gc.api: DEB1-BKP\\SQLGECODEV / geco_0000. No se imprimen ni copian credenciales.

- SPGECO_Vtas_PV_CTL_Rend_Para_Entrega: modify_date 2026-11-02 09:08:58.9. Los dos filtros ya están incorporados y la aplicación recibe 55 filas PES. La observación histórica de filtros queda resuelta para esta definición.
- SPGECO_Vtas_PV_CTL_Entregas_Guardar: modify_date 2026-10-30 08:39:05.89. Definición idéntica a la recuperada el 2026-11-01. **Los tres puntos de registro siguen presentes en esta base**, aunque el responsable informa correcciones del DBA: verificar si falta desplegarlas en esta instancia/base.

El intento de las 18:07:44, solicitud 7e839d35-cdd7-4bb8-942a-53dc9ce91878, envía una fila en @json_rend, arreglo directo con los 14 campos correctos. La respuesta real es resultado=2, resultado_msj «Debe informar las rendiciones.», resultado_id vacío. El rechazo no indica ausencia de la fila enviada.

Bloque responsable, líneas 65–70 de OBJECT_DEFINITION (líneas 72–77 del adjunto original):

~~~sql
Begin
    Set @resultado = 2
    Set @resultado_msj = 'Debe informar las rendiciones.'
    Select @resultado AS 'resultado', @resultado_msj AS 'resultado_msj', @resultado_id as 'resultado_id'
    Return
End
~~~

Ese bloque no tiene IF. Se ejecuta tras validar la cabecera y retorna antes de BEGIN TRANSACTION y de insertar la entrega. El DBA debe revisar el bloque y desplegar su versión corregida; no corresponde añadir campos al JSON ni tratar el rechazo como éxito desde la aplicación. Siguen pendientes SUM(importe) en líneas 115/138 y asignar resultado_id al comprobante generado antes de la salida.

La corrección que realizó el responsable sí está confirmada: proceso 00-06548, cierre 1, rendición 2, ítem 1, PES; arqueado 3717000.00, OK 4000000.00, rend_entrega=N y ent_compte=NULL en la consulta directa. La UI conserva esa misma selección e importe y muestra el mensaje del SP.

Durante este diagnóstico no se repitieron correcciones ni confirmaciones, no se modificaron SP/código funcional y no se perdió el contexto de pantalla. Request/response completos y comprobaciones permanecen privados en .artifacts/custodia-20261102.

## Logs para el DBA — 2026-11-02

Archivo configurado: D:/Sitios/Logs/GC/gc.api.log. Buscar [CUSTODIA].

- **[SP REQUEST]**: nombre de SP, Llamada, Solicitud cuando corresponda, Fase, endpoint SQL/base y JSON con parametros/tipos_parametros. @json_rend se presenta como arreglo copiable, sin doble escape. Parámetros SQL sin modificaciones.
- **[SP RESPONSE]**: misma Llamada, duración, cantidad y JSON con columnas y filas reales. Se incluyen ins_importe del catálogo y fecha de pendientes aunque no sean propiedades del DTO. Sin hashes/valores agregados por la UI.
- **[SP ERROR]**: excepción completa y errores SQL con número, estado, clase, procedimiento y línea. Las filas alcanzadas antes de una falla se identifican como lectura parcial.
- **[TRANSACCION]**: resultado exterior COMMIT/ROLLBACK/INCIERTO, con Solicitud. Un OK del SP previo a una reversión no se presenta como confirmación definitiva.

El diagnóstico conserva columnas repetidas mediante claves #2/#3 y deja su nombre original y ordinal en columnas. Los escapes legítimos de rutas o textos citados se conservan. No se registra la cadena de conexión ni cabeceras/tokens.

Se necesitan recompilar y reiniciar gc.api y los hosts que consumen la infraestructura compartida. El diagnóstico nuevo no cambia retrospectivamente las entradas antiguas ni requiere repetir cobros o registros para probarlo. Verificación: 34 pruebas de logs y 102 regresiones; lectura real de catálogo/55 rendiciones por el DAL actualizado, sin escrituras. Fuentes y evidencia de pruebas privadas en .artifacts/custodia-logs-20261102.

## Verificación de logs después del reinicio de API — 2026-11-02

Se verificó desde Caja el formato nuevo en gc.api.log: catálogo real de una fila/3 columnas, pendientes de 55 filas/18 columnas y acuse de una entrega histórica existente. Los tres pares request/response son JSON completos y parseables, comparten Llamada y conservan metadatos SQL, valores, duración y columnas no usadas por UI. El acuse respondió HTTP 200; la columna usu_id repetida se conserva como usu_id/usu_id#2 con ordinal/nombre original.

Exportación privada para el DBA: .artifacts/custodia-api-log-real-20261102/Custodia-DBA-20261102.log. Incluye las lecturas nuevas y, en una sección separada y etiquetada, los antecedentes de corrección/entrega que realizó el operador antes de la recompilación. No presenta esos antecedentes como ejecuciones del formato nuevo.

La definición de Entregas_Guardar volvió a leerse: continúa el retorno incondicional. Queda pendiente la confirmación de una entrega solicitada por el responsable; no se ejecutó una nueva escritura ni se modificó SQL/aplicación. La selección del operador y su importe OK se conservaron.
