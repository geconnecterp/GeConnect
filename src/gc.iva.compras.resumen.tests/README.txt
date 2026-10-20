IVA Compras Resumen - verificaciones técnicas

Raíz: D:\Sis25\git\GeConnect\src
Pantalla: /Libros/IvaComprasResumen/Index
Permiso: imp_lc_res (el menú y sus perfiles los administra Juan).
Reporte PDF: 105. Los reportes 103 y 104 no se sustituyen.

Contrato: cuatro SP de lectura con @periodo varchar(4), @mes varchar(2):
SPGECO_IVA_C_Conta_BI / SPGECO_IVA_C_R_BI
SPGECO_IVA_C_Conta_OT / SPGECO_IVA_C_R_OT
Sin cambios en SQL. No agrupar por tipo: hay múltiples GR y dos IV.
Únicos cálculos: sumas de importes y Contabilidad menos Libro IVA.
Dif. cero se omite en vista, PDF y Excel. Sin tolerancias ni inversión de NC.

Compilación aislada:
dotnet build gc.iva.compras.resumen.tests/gc.iva.compras.resumen.tests.csproj --artifacts-path .artifacts/iva-compras-resumen-20261019 -p:NuGetAudit=false
dotnet .artifacts/iva-compras-resumen-20261019/bin/gc.iva.compras.resumen.tests/debug/gc.iva.compras.resumen.tests.dll
node gc.iva.compras.resumen.tests/panel-tests.cjs

--serve levanta opcionalmente http://127.0.0.1:17865/qa (sólo localhost).
Es un host de pruebas con datos sintéticos y una cabecera simulada, no ejecuta
el Program de gc.sitio ni se conecta a SQL. Sirve la parcial Razor real, su JS
y los CSS reales de Bootstrap/Core/Golden. Detenerlo al terminar.
La verificación en el sitio completo requiere recompilar/reiniciar los tres
proyectos y autenticar una sesión con imp_lc_res.

Casos: permisos, precisión decimal, ceros de cuenta, filas IV repetidas,
contrato incompleto, cuatro hojas XLSX, Dif. cero/no cero, PDF A4 horizontal,
PDF extenso sin truncar, filtros/Detalle/reset, spinner, error sin datos viejos,
exportación con criterios aplicados y conservación de reportes 103/104.

Diseño: cuatro cuadros completos en 1366x768 con las cantidades de filas del
modelo provisto. A menor ancho se apilan. No hay recortes ni límite artificial
de filas: si el catálogo de cuentas/alícuotas aumenta considerablemente,
la página crece y el PDF continúa en más páginas para no perder información.

Los importes >= 10 billones se escriben como texto exacto en XLSX para no
perder centavos por el límite de 15 cifras significativas de Excel.
El resto son números nativos. Las cuentas siempre son texto.
Las exportaciones vuelven a consultar los cuatro SP; no constituyen una
instantánea transaccional compartida con una consulta anterior.

Pendientes del origen, sin compensar en C#:
- Conta_OT recibido contiene '03''05' en la lista de meses de 31 días.
- Los controles recibidos fijan febrero en 28 días.
- Las descripciones y agregaciones fiscales se respetan según los SP.
La validación fiscal y contable contra datos reales corresponde al tester/DBA.
TXT no incluido.
