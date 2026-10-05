# Administrador de Caja

Fecha: 2026-10-05. Reglas canónicas: [Bitácora única, ADM-01 a ADM-10](BitacoraUnicaReglasCaja.md).

## Alcance implementado

Ingreso autenticado sin apertura del puesto, desde el inicio (Administrador de Caja) o desde el menú operativo. El inicio permite elegir Operar Caja antes de validar/abrir el puesto. Usuario y sucursal se derivan de la sesión, sin selector de otra administración. La API utilizada es AppSettings:RutaBase de esa instalación; requiere conectividad y certificado válido cuando utiliza HTTPS.

Dos acciones: habilitación general con número de proceso y cierre general precedido por consulta de puestos abiertos. Grilla con cierre, PV y cajero; consulta fallida bloquea cierre. Confirmación con estilo existente, protección antiforgery, bloqueo durante envío, exclusión por sucursal y repetición del mismo identificador exitoso/incierto suprimida por 24 horas en la instancia. Una operación rechazada puede corregirse y volver a intentarse; un resultado incierto no se reintenta automáticamente.

No se modifican gc.sitio, gc.api ni los SP. La reconsulta previa al cierre no protege por sí sola la ventana entre consulta y cierre frente a otro proceso. Los reinicios/múltiples servidores requieren la garantía del SP y, si se exige, idempotencia persistente futura.

## Contraste con el original y observaciones para su responsable

Fuente: “202604 Habilitación y Cierre General de Cajas SISTEMA GECO - MARCELO.docx”, cuaderno Sistema GECO, leído directamente. El documento exige desactivar Cierre General si existen puestos abiertos y mostrar proceso al habilitar.

| Observación en el original | Tratamiento en Caja |
| --- | --- |
| VentasCajasCierre/Index.cshtml muestra btnCierreGeneralCaja sin condición disabled por filas abiertas; cajac.js tampoco aplica ese control | Cierre deshabilitado hasta consulta válida sin puestos |
| VentasCajasCierreController.CerrarCajas invoca cierre sin reconsultar puestos | Reconsulta en servicio antes del POST; validar atomicidad SQL con DBA |
| gc.sitio.core CajaServicio.ObtenerPVAbiertos devuelve lista vacía ante HTTP fallido o cuerpo vacío | Error explícito y cierre bloqueado |
| Servicios originales pueden devolver Ok=false sin EsError/EsWarn; JS decide éxito por esos indicadores | Sólo resultado explícito 0 permite éxito; falta de proceso en habilitación es incierta |
| Contrato de mensajes alterna msg/mensaje en distintas ramas | Respuesta homogénea ok/mensaje/incierto/proceso |
| Sin protección explícita contra doble envío en el flujo revisado | Bloqueo UI, exclusión por sucursal y memoria temporal de solicitudes en Caja |

Estas observaciones describen el código local revisado, no una auditoría del SP desplegado. No fueron enviadas al autor ni modificadas en Central.

## Pendientes de coordinación

- DBA: confirmar cierre atómico respecto de apertura concurrente de puestos y rechazo de repetición de habilitación/cierre en estado incompatible. No editar SP desde esta tarea.
- Despliegue: verificar que la API/base configurada corresponda a la sucursal; no se inventa nuevo mecanismo de distribución.
- Retiro posterior de ítems centrales vta_cajas_grl_hab y vta_cajas_grl_cierr mediante sus registros, después de aceptación de Caja. No se tocaron registros.
- Clover, MP y CAEA: alcance futuro; centralización/distribución por definir.

## Pruebas reproducibles

Desde D:/Sis25/git/GeConnect/src:

```powershell
./gc.caja/Tests/AdministradorCaja.Tests.ps1
```

31 verificaciones de servicio y controlador con HTTP simulado: rechazo de listas inválidas, errores y HTML; bloqueo con puestos; reconsulta antes de cierre; parámetros/usuario/sucursal; resultado/proceso; errores de SP; resultado incierto; acceso sin CajaActual; duplicado; solicitud inválida; atributos antiforgery; rechazo sin sesión. Compila gc.caja como dependencia.

Host visual aislado (sólo pruebas, no desplegar):

```powershell
dotnet .artifacts/administrador-caja-tests/bin/Debug/net8.0/AdministradorCaja.Tests.dll --browser
```

Se enlaza exclusivamente a 127.0.0.1:7289, inyecta identidad ficticia y transporta todo a FakeApi, nunca a la API real. Escenarios /escenario/abiertos, /escenario/error, /escenario/vacio, /escenario/incierto. Renderiza controlador, Razor, antiforgery, JavaScript y estilos reales. Se comprobó grilla, bloqueo, confirmación y proceso ficticio 00-77777. Prueba de integración real de habilitación/cierre pendiente con operador autorizado; no ejecutar operaciones reales para una prueba visual.
