Pruebas sintéticas de Libro IVA Compras Cierre

No leen conexiones ni abren SQL. No ejecutan SP, cierres ni escrituras de negocio.
Cubren contratos, precisión decimal, nulos, permisos, Central, respuestas, archivos
C/D completos, errores, temporales, cancelación y renderizado de la vista real.
El doble de concurrencia no reemplaza la prueba de applock/PK con SQL real.
Verificado el 20/10/2026: 79 comprobaciones C# correctas. Pruebas JS del panel
nuevo y regresión del panel de Ventas correctas; regresión C# Ventas: 63 checks.

Desde D:\Sis25\git\GeConnect\src:
dotnet build gc.iva.compras.cierre.tests/gc.iva.compras.cierre.tests.csproj --artifacts-path .artifacts/iva-compras-cierre-20261020 -p:NuGetAudit=false -consoleloggerparameters:ErrorsOnly
dotnet .artifacts/iva-compras-cierre-20261020/bin/gc.iva.compras.cierre.tests/debug/gc.iva.compras.cierre.tests.dll
node gc.iva.compras.cierre.tests/panel-tests.cjs

QA visual aislada opcional, con la misma vista parcial/CSS/JS y marco sintético:
agregar --serve al segundo comando y abrir http://127.0.0.1:17866/qa.
Ese host no inicia gc.sitio ni omite su autenticación: es un fixture separado.
El endpoint de cierre del fixture siempre rechaza; jamás alcanza a SQL.
Detener con Ctrl+C al terminar. No desplegar este proyecto como aplicación.

La aceptación real comienza sólo con consultas y descargas de períodos cerrados.
Un cierre real requiere aprobación explícita de base y período por Juan.
