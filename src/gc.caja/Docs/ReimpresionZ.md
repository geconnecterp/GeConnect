# Reimpresión Z — implementación y pendientes de integración

Fecha: 29/09/2026. Fuente funcional: cuaderno NotebookLM «Arquitectura de Configuración y Acceso al Sistema de Caja», documento «20260928 REIMPRECIÓN Z.docx». Contrato técnico: definición de SPGECO_CAJA_Z proporcionada por el usuario. Ningún procedimiento SQL fue modificado ni ejecutado durante las pruebas.

## Alcance implementado

- Pantalla `/Facturacion/ReimpresionZ` con identidad Golden de Home/Cuenta, búsqueda por número o fecha, controles Desde/Hasta, teclado virtual numérico existente, confirmación y resultado.
- Botón «Reimpresión Z» en el menú, deshabilitado por defecto. Solo se habilita al verificar en servidor `ctrl_id = 50` y pertenencia de la caja a la sucursal de la sesión. Es el único controlador que el SP proporcionado ejecuta actualmente.
- Disponible con acceso completo, parcial y solo cierre. No se modificó la disponibilidad del botón de cierre operativo de caja.
- Para una caja compatible, al obtener resultado de integridad 0, se ofrece continuar con apertura o acceder a Reimpresión Z sin abrir caja. El resultado 3 mantiene las opciones existentes y agrega acceso directo a Z. Al cambiar de PV se recarga el inicio para consultar la configuración del nuevo punto.
- El módulo consulta ObtenerDatosCF directamente: no invoca Valida_PV, apertura/cierre, rendiciones, stock ni confirmación de ventas.
- Confirmación con rango visible, campos y navegación bloqueados mientras se solicita la impresión, y bloqueo tras un resultado exitoso o incierto hasta preparar explícitamente otra solicitud.
- Sin reintentos automáticos. Un error de transporte/timeout informa resultado incierto: se requiere verificar el controlador antes de preparar otra solicitud.
- API serializa las solicitudes concurrentes por caja dentro de la instancia mediante un bloqueo no bloqueante: rechaza la segunda mientras la primera está en curso.

## Reglas y contrato

- Por número: enteros de 1 a 99999, Desde <= Hasta, Hasta − Desde <= 5. 5000–5005 es válido (seis números incluidos). No se agregan ceros a la izquierda al enviar a SQL.
- Por fecha: Desde <= Hasta, diferencia <= 7 días, Hasta <= hoy y Hasta >= hoy menos cinco años. El límite de cinco años se aplica a Hasta, tal como indica el documento. Hoy se obtiene en el servidor en zona Argentina.
- Interfaz/API intercambian fechas ISO yyyy-MM-dd; el servicio SQL convierte a yyMMdd.
- Interfaz y servidores validan los rangos. El POST de Caja exige antiforgery. Caja obtiene caja/usuario/sucursal de la sesión; la API toma usuario/sucursal de los claims del token y vuelve a verificar la configuración fiscal y sucursal de la caja.
- Ejecución parametrizada de SPGECO_CAJA_Z: @caja_id varchar(4), @usu_id varchar(10), @adm_id varchar(10), @xfecha bit (1 fechas / 0 números), @desde y @hasta varchar(10).
- La fila resultado/resultado_msj se interpreta según el contrato existente. Una ausencia de respuesta o excepción durante ejecución se informa como incierta.
- Con resultado 0 la UI indica que la solicitud finalizó sin errores informados y pide verificar el reporte en el controlador. No afirma haber comprobado físicamente la impresión.

## Observaciones para el DBA — no se modificó el SP

1. **OK sin ejecución para otros controladores.** Solo ctrl_id 50 ejecuta los procedimientos de reportes, pero cualquier otro ctrl_id termina en 0/OK. ¿Se confirmará soporte exclusivo Hasar 2G o habrá otras ramas? La aplicación bloquea actualmente el resto.
2. **Respuesta del controlador ignorada.** @h2g_z contiene impresora/Fiscal, pero no se examina. La verificación de respuesta vacía está comentada. ¿Qué valores indican éxito, papel agotado, controlador ocupado o falta de comunicación? Solicitar ejemplos de éxito/error y definir qué debe devolver resultado/resultado_msj. No basta con que termine sin excepción para garantizar impresión física.
3. **Dependencia FE sin uso posterior.** Consulta GS64.geco_fe.dbo.ambiente para asignar @ambiente, pero ese valor no participa en ninguna llamada posterior. ¿Es necesaria esa dependencia para una reimpresión fiscal? Un fallo allí impediría llegar a los SP de reportes.
4. **Dirección del controlador.** ISNULL cubre caja_ip NULL, pero no vacío/espacios. ¿Debe validarse IP/URL configurada? Confirmar que el fallback http://localhost:5000 apunte al servicio correcto desde el entorno donde se ejecutan los procedimientos anidados.
5. **Contrato de los procedimientos anidados.** Confirmar que reciben fechas AAMMDD, límites inclusivos y números sin relleno. El usuario confirmó que el mínimo de número Z es 1: la aplicación rechaza 0, incluso con ceros de relleno. Este punto ya no requiere consulta al DBA.
6. **Controlador con documento abierto.** ¿SP_ReportarZetasPorFecha / SP_ReportarZetasPorNumeroZeta rechazan correctamente esa condición y la propagan? La aplicación deliberadamente no ejecuta Valida_PV, que puede cancelar documentos y configurar el equipo.
7. **Validaciones en SQL.** El SP proporcionado no valida formatos, orden ni amplitud de rangos. La aplicación sí lo hace; evaluar en DBA si otros consumidores del SP requieren también defensa dentro del procedimiento.

## Verificación realizada

- Compilación gc.caja: 0 errores; 99 advertencias preexistentes en las dependencias compiladas.
- gc.api compilada como dependencia de las pruebas de servidor, sin errores.
- 28 casos de rangos JavaScript (números, límites, fechas inexistentes, bisiesto, antigüedad, formatos inválidos).
- 13 pruebas de interacción con DOM/transporte simulados: cancelación, doble envío, doble aceptación, antiforgery, bloqueo final, fallo incierto y preparación de nueva solicitud.
- 10 pruebas de menú: acceso completo/parcial/solo cierre/ninguno/desconocido, con y sin controlador compatible.
- 53 comprobaciones de servidor con repositorio simulado: mismos rangos, conversión de fechas, seis parámetros SQL tipados, caja cerrada, rechazo de caja ajena/FE, errores, identidad desde token y concurrencia.
- Total: 104 comprobaciones. No hubo conexión SQL ni impresión física en estas pruebas.
- Navegador integrado: intento de apertura de localhost:7257/Facturacion/ReimpresionZ terminó en ERR_CONNECTION_REFUSED. No se pudo verificar la pantalla real ni sus tamaños escritorio/móvil.

Ejecución de pruebas desde la raíz autorizada:

```powershell
node gc.caja/Tests/ReimpresionZ.Tests.cjs
./gc.caja/Tests/ReimpresionZ.Servidor.Tests.ps1
```

## Prueba pendiente en entorno real

Recompilar y levantar **gc.api y gc.caja**, iniciar sesión y actualizar el navegador. Verificar botón deshabilitado en FE; habilitado en Hasar 2G con caja abierta/cerrada y modo solo cierre. Entrar por la opción sin apertura y comprobar que no abre ni cierra caja. Revisar escritorio/móvil y teclado numérico. En un controlador de prueba, enviar primero un rango conocido y contrastar la impresión física, mensajes y manejo de desconexión con el DBA.

Limitaciones: el bloqueo de concurrencia reside en una instancia de API, no es una garantía de idempotencia persistente entre instancias o reinicios. Si la API tiene varias réplicas o se necesita deduplicación duradera, habrá que coordinar ese contrato con backend/DBA. La confirmación física sigue dependiendo de la respuesta que el SP hoy no expone.

> Referencia funcional común: [Bitácora única de reglas de Caja](BitacoraUnicaReglasCaja.md). Este documento conserva el detalle técnico y las consultas al DBA.
