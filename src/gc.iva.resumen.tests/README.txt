LIBRO IVA VENTAS - CONSULTA RESUMEN
Implementación: 15/10/2026. Raíz: D:\Sis25\git\GeConnect\src

ALCANCE Y DECISIONES
- Área Libros; controlador IvaVentasResumen; acción Index.
- Ruta de menú: /Libros/IvaVentasResumen/Index. Permiso: imp_lv_res.
- Juan configura exclusivamente el ítem y sus permisos en la base de datos.
- No se ejecutó ni modificó ningún SP. Cierres sigue en standby.
- SP: dbo.SPGECO_IVA_V_R, @periodo varchar(4), @mes varchar(2).
- Fuente: versión corregida suministrada el 15/10/2026, attachment 721acf90.
- Hallazgo pendiente por decisión de Juan: @ali residual en percepciones.
  No se corrigen ni reinterpretan las descripciones o alícuotas desde C#.
  La aceptación fiscal del cálculo y de los signos corresponde al tester/DBA.
  Las pruebas técnicas no certifican esos cálculos fiscales.

FUNCIONAMIENTO
1. Entrar con perfil habilitado para imp_lv_res.
2. Seleccionar año (actual y cinco anteriores) y mes; ambos inician en el actual.
3. Ver muestra la consulta completa, organizada por sucursal y condición fiscal.
   Mientras consulta se bloquean los controles y aparece un spinner explicativo.
4. Filtro alterna entre formulario y resultado sin borrar la consulta.
5. Detalle cierra la consulta, limpia su resultado y conserva año/mes.
6. Inicializar restituye los valores iniciales del formulario y limpia el resultado.
7. Cambiar un filtro invalida el resultado anterior; es necesario volver a Ver.
8. Imprimir y Exportar Excel completo se habilitan sólo con filas consultadas,
   permanecen accesibles con el filtro cerrado y se deshabilitan durante cargas.
9. Imprimir utiliza el gestor de documentos existente, reporte 103, PDF A4 apaisado.
10. Excel usa una descarga XLSX propia a través de gc.api.report; una hoja por
    sucursal y otra Total Libro. No depende de la parte visible de la grilla.
11. Al emitir PDF o Excel se realiza una nueva consulta del período aplicado.
    No es una instantánea congelada: cambios concurrentes en SQL pueden reflejarse.
12. Sin filas no se habilitan salidas. Si el SP devuelve conceptos con ceros,
    se conservan y se informa esa situación sin asumir que no hubo comprobantes.

PRESENTACIÓN Y PRECISIÓN
- Cinco importes originales: MT, CF, RI, EX, otros. Total de fila = suma decimal.
- Totales por grupo = suma de sus filas, conservando signos originales.
- 9999 es el consolidado suministrado por el SP, mostrado al final. Nunca se
  suma el consolidado nuevamente con las sucursales para fabricar otro total.
- Orden visual: sucursal, operación V/C, orden, alícuota, código y descripción.
  Las alícuotas no están codificadas como una lista fija. No se ocultan filas cero.
- Números N2 es-AR, alineados a derecha; desplazamiento horizontal dentro de
  la grilla cuando no cabe. Encabezado y concepto fijos, sin abreviar importes.
- PDF calcula anchos con los valores y totales reales, repite encabezados y
  número de página; rechaza anchos imposibles en vez de emitir cifras cortadas.
- Excel conserva valores numéricos hasta el límite seguro. Importes absolutos
  >= 10.000.000.000.000 se escriben como texto decimal exacto para no perder
  centavos por el límite de precisión de Excel. La hoja incluye esa aclaración.

INTEGRACIÓN Y SEGURIDAD
- gc.api: GET api/iva-ventas-resumen/acceso y POST .../consultar.
- gc.api.report: POST api/iva-ventas-resumen/excel.
- PDF: api/Reportes/generate con Reporte=103 y Parametros Periodo, Mes, Perfil,
  Menu. Adaptador async exclusivo del reporte 103 en ReportesController;
  conserva el contrato anterior de los otros reportes.
- Tanto consulta como salidas validan autenticación, perfil perteneciente al
  token y menú imp_lv_res. Cambio de contraseña obligatorio impide el acceso.
- Sitio: antiforgery en los POST, URLs generadas por MVC y unión de URLs API
  que conserva la subaplicación IIS. Serialización propia con Newtonsoft.
- Configuración del gestor aditiva mediante PostConfigure<DocsManager>:
  IVA_VENTAS_RESUMEN, reporte 103, Print=true; sin mensajería ni enlaces públicos.
  No es necesario modificar arrays de appsettings por ambiente para registrarlo.
- Se reutilizan AppSettings.RutaBase, AppSettings.MenuId, DocsManager.ApiReporteUrl,
  EmpresaGeco y las configuraciones de conexión/autenticación existentes.
- Se consulta todo el SP sin OFFSET ni límite local: es un resumen, no el libro
  de detalle. Se rechazan contratos incompletos y no se devuelven archivos parciales.
- Consulta SQL async parametrizada, cancelable, timeout 600 s; cliente HTTP 10 min.
  El gestor conserva su timeout y límites configurables propios.
- API de reportes admite hasta dos PDF y dos XLSX concurrentes por proceso.

VERIFICACIONES REALIZADAS
- Compilan gc.sitio, gc.api y gc.api.report con salida aislada, cero errores.
  Persisten advertencias del árbol existente; no se cambiaron paquetes NuGet.
- 24 comprobaciones C# sintéticas: filtros, parámetros, consolidado, signos,
  centavos, Excel, páginas PDF, permisos y registro del gestor.
- panel-tests.cjs: Filtro/Detalle, spinner, doble envío, contexto del reporte,
  exportación, respuesta vacía, error 503, cambio de filtros e Inicializar.
- 96 pruebas existentes de gc.iva.tests aprobadas (regresión de Libro IVA Detalle).
- PDF sintético: 120 conceptos en cinco páginas, incluyendo el último concepto;
  importes de 14.278.505.813,70 y total de 1.713.420.697.644,00 completos.
  Revisados visualmente comienzo y cierre del PDF en A4 horizontal.
- Estas pruebas no acceden a SQL ni sustituyen la prueba del circuito real.

REPRODUCIR PRUEBAS DESDE ESTA RAÍZ
dotnet run --project gc.iva.resumen.tests
node gc.iva.resumen.tests/panel-tests.cjs
dotnet run --project gc.iva.tests
Para guardar muestras sintéticas: pasar una carpeta de salida como único
argumento de gc.iva.resumen.tests. No usar esos archivos como reportes fiscales.

PENDIENTE ANTES DE ACEPTACIÓN
- Recompilar/reiniciar gc.api, gc.api.report y gc.sitio; autenticar usuario.
- Verificar menú configurado por Juan, consultas reales, tiempos, errores,
  navegación, tamaños de ventana de escritorio y gestor de impresión completo.
- Con el tester, contrastar cada importe y total contra SP/comprobantes del período.
- Revisar nombres/logotipo reales de empresa en el PDF y hojas del Excel real.
- Hallazgo @ali: standby. TXT fuera del alcance; no se implementó.

MEJORA VISUAL DEL GESTOR COMPARTIDO - 15/10/2026
- docmanager.js: la ventana de espera de impresión unificada ahora tiene fondo
  marfil, tarjeta Golden, spinner CSS, estado accesible y movimiento reducido.
- Toma la identidad del login/home de GECO. No carga recursos externos.
- Se reemplazó exclusivamente el HTML de document.write; no cambió la apertura
  inmediata, petición del paquete, manejo de errores, onload ni navegación al PDF.
- Afecta a todos los módulos que usan esa ventana del gestor, no sólo Libro IVA.
- Prueba: node gc.iva.resumen.tests/print-wait-tests.cjs. Verifica sintaxis y la
  huella del código anterior excluyendo el bloque visual. Actualizar la huella
  sólo tras revisar futuros cambios legítimos del gestor compartido.
- Verificado en la aplicación autenticada con noviembre 2025: espera Golden
  visible a 1920 x 855, animación activa y sin overflow horizontal.
- Chrome bloqueó la automatización al iniciar la prueba de ventana angosta;
  queda pendiente esa comprobación y observar el final del PDF en ese intento.
- No requiere recompilar C#: recargar el módulo para tomar el JS versionado.
