# Cuenta del operador en Caja

Implementación en Sis25. Complejidad media: reutiliza el contrato de seguridad de Sitio e integra la sesión propia de Caja.

## Accesos y alcance

- Menú principal: Cambiar contraseña y avatar con iniciales.
- Mi cuenta: nombre, usuario, correo, sucursal y perfil activo de la sesión; cambio de clave y cierre de sesión.
- La identidad y los permisos se consultan; su edición continúa bajo administración. No se añaden permisos ni un cambio de perfil operativo.
- Diseño Golden del inicio de Caja, con columnas que se apilan en pantallas angostas y controles de teclado.

## Integración

1. `gc.caja.core` compila mediante enlaces los dos archivos del adaptador `IConfiguracionSeguridadServicio` / `ConfiguracionSeguridadServicio` de `gc.sitio.core`. No depende de su aplicación web ni mantiene una copia del adaptador.
2. Usa los mismos endpoints de política, cambio normal y cambio forzado. La API y los SP existentes son la autoridad sobre la política vigente y la validez de las credenciales.
3. El controlador exige autenticación, antiforgery, campos obligatorios, límite técnico y confirmación exacta. Nunca recibe un usuario objetivo desde el formulario.
4. La política del servidor gobierna la ayuda y validación del navegador. Espacios y mayúsculas se preservan.
5. Clave temporal o vencida: login y middleware impiden operar hasta completar el cambio. El cambio forzado sólo está habilitado para la sesión correspondiente.
6. Éxito: cierra autenticación, elimina JWT de Caja, limpia sesión y solicita reingreso. Un rechazo conserva la sesión. Un fallo de transporte no se reintenta automáticamente.

## Verificación

- `node gc.caja/Tests/Cuenta.Tests.cjs`: reglas y flujo del formulario con AJAX simulado.
- `pwsh -File gc.caja/Tests/Cuenta.Servidor.Tests.ps1`: controlador compilado, identidad, restricciones, rechazos y cierre de sesión con API simulada.
- Compilar `gc.caja/gc.caja.csproj`.
- Pendiente de comprobación visual en navegador autenticado: escritorio y móvil, abrir avatar, navegar a cuenta y comprobar campos. El navegador integrado no estuvo disponible durante la implementación.
- El cambio de una contraseña real debe realizarlo el operador. Las pruebas automáticas no invocan la API real ni los SP.
