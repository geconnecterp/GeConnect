# SEG-006 — Bitácora de CA y HTTPS en IIS

Versión 0.22 — 24/09/2026. Documento vivo: se actualiza con cada comando y resultado confirmado.

## Ubicación vigente del proyecto

**La única raíz de GECO para este trabajo es `D:\Sis25\git\GeConnect\src`.** Confirmada por el usuario y por su `AGENTS.md`. Los comandos de este documento usan esa ubicación. Las instrucciones anteriores que remitían a otra copia quedan reemplazadas por esta versión.

Esta carpeta contiene documentación y componentes públicos del procedimiento. Las claves y certificados operativos se administran fuera del repositorio, en `$env:USERPROFILE\GecoPKI` por defecto. Cambiar la ubicación del proyecto no exige regenerar ni mover la clave de la CA.

## Objetivo y decisiones

- Dominio de la organización: `cafeamerica.com.ar`.
- Primer ensayo: backend IIS accesible por VPN en `172.10.10.12`.
- Nombre definitivo del ensayo: `api-dev.cafeamerica.com.ar`.
- Un sitio IIS con aplicaciones por ruta: `https://api-dev.cafeamerica.com.ar/apigeco`, `https://api-dev.cafeamerica.com.ar/apiclover`, etc. Aplicaciones reales inventariadas en el paso 15; GCServicios probado mediante una ruta funcional. Las direcciones de este punto son ejemplos de diseño.
- Un certificado para ese nombre cubre todas esas rutas. El certificado no contiene los nombres de las carpetas o aplicaciones.
- El nombre provisional `apigeco.cafeamerica.com.ar` fue descartado para este ensayo.
- Autoridad interna para este ensayo. Los accesos públicos desde dispositivos ajenos usarán certificados emitidos por una autoridad públicamente reconocida, en una etapa posterior.
- Por instrucción del usuario del 24/09/2026, la raíz tendrá **200 años calendario**, calculados al emitir. Si se emite en 2026 vencerá en 2226. Se sustituye la propuesta anterior de 3650 días.

Por pedido adicional del usuario, comunicado como requisito de su jefe, el backend también solicita 200 años calendario. Su fecha efectiva se limita al vencimiento de la raíz: 24/09/2226 a las 13:45:52 UTC. Como se emite después de la raíz, el período restante es ligeramente menor que 200 años completos. Cambiar la vigencia solicitada no modifica certificados ya emitidos. Tampoco garantiza 200 años de compatibilidad, seguridad criptográfica o disponibilidad: la rotación de claves, los cambios de algoritmos y la recuperación ante compromiso siguen siendo parte del mantenimiento.

## Estado confirmado

| Paso | Estado y evidencia |
|---|---|
| OpenSSL | Versión local comprobada: 3.6.0; ruta `C:\Program Files\OpenSSL-Win64\bin\openssl.exe` |
| Red del backend | Resultados entregados por el usuario desde el servidor remoto |
| Resolución local del nombre | Usuario confirmó `api-dev.cafeamerica.com.ar` → `172.10.10.12` |
| Clave privada de la CA | Usuario confirmó `Key is valid` el 24/09/2026 |
| Certificado público real de la raíz | Emitido y verificado; usuario confirmó la salida completa el 24/09/2026. Vence el 24/09/2226 a las 13:45:52 UTC |
| Clave y CSR del backend | Usuario confirmó creación y verificaciones para api-dev.cafeamerica.com.ar el 24/09/2026 |
| Certificado del backend | Emitido; cadena, nombre, uso TLS, clave pública y vigencia verificados según salida del usuario. Vence el 24/09/2226 a las 13:45:52 UTC |
| PFX del backend | Creado y verificado por el usuario el 24/09/2026: AES-256-CBC, MAC SHA-256 y 100000 iteraciones |
| Windows del backend | Usuario confirmó Windows Server 2019, 2022 o 2025; versión exacta pendiente |
| Confianza del usuario Windows | Instalada y verificada en Cert:\CurrentUser\Root, según salida confirmada por el usuario el 24/09/2026 |
| Relevamiento IIS | Confirmado: Default Web Site por HTTP y SitiosGC por HTTPS; detalle registrado en paso 10 |
| Copia al servidor | Confirmada por el usuario; hashes SHA-256 de CER y PFX coinciden con los originales |
| Confianza del servidor e importación PFX | Confirmadas: raíz en LocalMachine\Root y backend en LocalMachine\My, HasPrivateKey=True |
| Nuevo enlace HTTPS con SNI | Confirmado por el usuario: host api-dev, certificado 0D6D1A14… y sslFlags=1; enlace anterior conservado |
| Aplicaciones de SitiosGC | Cinco aplicaciones inventariadas; GET de GCServicios/api/administracion confirmado con HTTP 200 y ocho registros |
| Respaldo cifrado de PKI | Creado por el usuario; 27 archivos recuperados y comparados, RAR5 AES-256. Hash del RAR contrastado por Codex; copia externa pendiente |
| Consumo desde 172.10.10.11 | Usuario confirmó CA confiable, entrada hosts hacia api-dev y respuesta correcta de GCServicios por HTTPS |
| Certificado app-dev para 172.10.10.11 | Nombre elegido; emisión y PFX pendientes. Comando único preparado en paso 20 |
| Respaldo previo de IIS | Nombre y resultado aún no comunicados |
| Prueba HTTPS desde cliente VPN | GET autorizado ejecutado por Codex: HTTP 200, JSON válido y ocho registros, sin redirección. Kaspersky presenta un certificado sustituto al cliente. Certificado original IIS confirmado por prueba local en el servidor: SHA-256 coincidente y TLS 1.2 |

La creación de claves y la configuración remota fueron comunicadas por el usuario. Codex ejecutó posteriormente el GET HTTPS autorizado que se registra en el paso 16; no realizó cambios remotos. Las pruebas de los scripts con material descartable no constituyen emisión de la CA operativa.

## Material generado y por generar

Carpeta predeterminada: `$env:USERPROFILE\GecoPKI`, por ejemplo `C:\Users\<usuario>\GecoPKI`.

| Archivo | Función | Tratamiento |
|---|---|---|
| `private\cafeamerica-ca.key.pem` | Clave privada RSA de la CA, cifrada con contraseña | Secreto; no copiar a IIS, repositorios ni al chat |
| `certs\cafeamerica-ca.cert.pem` | Certificado público raíz en PEM | Distribuible para establecer confianza |
| `certs\cafeamerica-ca.cer` | El mismo certificado público en DER | Distribuible para importación en Windows |
| `servidores\api-dev.cafeamerica.com.ar\servidor.iis.pfx` | Certificado y clave privada del servidor, con su cadena | Privado; instalación controlada en IIS |

Guardar una copia de seguridad protegida de la clave y del certificado raíz, y la contraseña en un gestor. El respaldo cifrado y su extracción se confirmaron en el paso 18; el traslado a otro equipo todavía está pendiente. Una clave nueva no reemplaza a la anterior ante los clientes que ya confían en ella.

## Paso 1 — Comprobar OpenSSL

En PowerShell de la computadora del usuario, fuera del Escritorio remoto:

```powershell
Get-Command openssl
& 'C:\Program Files\OpenSSL-Win64\bin\openssl.exe' version
```

Resultado conocido: `OpenSSL 3.6.0 1 Oct 2025`.

## Paso 2 — Consultar red y DNS del backend

En PowerShell dentro del servidor remoto:

```powershell
Get-DnsClientServerAddress -AddressFamily IPv4 |
    Where-Object { $_.ServerAddresses.Count -gt 0 } |
    Format-Table InterfaceAlias, ServerAddresses -AutoSize

Get-NetIPConfiguration |
    Format-List InterfaceAlias, IPv4Address, IPv4DefaultGateway
```

Resultados comunicados:

| Interfaz | IPv4 | Servidores DNS | Puerta de enlace |
|---|---|---|---|
| Ethernet | `172.10.10.12` | `8.8.8.8`, `8.8.4.4` | Presente; la salida no mostró su dirección |
| Ethernet 2 | `169.254.3.42` | `172.16.0.1`, `192.168.1.2`, `8.8.8.8`, `8.8.4.4` | Ausente |

La dirección `169.254.3.42` es de enlace local. No se ha identificado un DNS corporativo operativo. No se modificaron interfaces ni sus DNS. `172.10.10.x` no es espacio privado RFC 1918: se conserva el destino comunicado para el ensayo por VPN, sin inferir que sea accesible desde Internet.

Consulta pública realizada durante el diagnóstico:

```powershell
Resolve-DnsName -Name api-dev.cafeamerica.com.ar -Type A -Server 8.8.8.8 -DnsOnly
```

El usuario informó que el nombre no existía. También consultó el nombre provisional `apigeco.cafeamerica.com.ar`, con `DNS_ERROR_RCODE_NAME_ERROR`. Una consulta al DNS público no descarta registros internos o entradas locales en hosts.

## Paso 3 — Resolución local para el ensayo

En la computadora del usuario, fuera del Escritorio remoto:

1. Abrir Bloc de notas como administrador.
2. Abrir `C:\Windows\System32\drivers\etc\hosts`, seleccionando Todos los archivos.
3. Conservar las entradas existentes y agregar una única entrada para este nombre:

```text
172.10.10.12    api-dev.cafeamerica.com.ar
```

4. Guardar sin extensión `.txt`.
5. Comprobar en PowerShell:

```powershell
[System.Net.Dns]::GetHostAddresses('api-dev.cafeamerica.com.ar') |
    Select-Object -ExpandProperty IPAddressToString
```

Resultado confirmado: `172.10.10.12`.

Esto solo configura el equipo donde se editó hosts. Los demás consumidores necesitarán resolución propia o DNS central. Esta comprobación no verifica todavía HTTPS. Si quedó una entrada provisional de `apigeco.cafeamerica.com.ar`, revisar y retirar únicamente esa entrada del ensayo.

Para deshacer: eliminar solo la entrada agregada; si hace falta, ejecutar `ipconfig /flushdns` en consola elevada. No borrar el archivo hosts. La consulta con `-Server 8.8.8.8 -DnsOnly` no comprueba la entrada local.

## Paso 4 — Clave privada de la CA (completado)

Comandos de reproducción, en PowerShell local sin administrador. La guarda evita sobrescribir una clave existente:

```powershell
$carpetaCA = Join-Path $env:USERPROFILE 'GecoPKI'
New-Item -ItemType Directory -Force -Path "$carpetaCA\private", "$carpetaCA\certs" | Out-Null
Set-Location $carpetaCA

if (Test-Path '.\private\cafeamerica-ca.key.pem') {
    Write-Host 'La clave ya existe. No se vuelve a generar.'
} else {
    & 'C:\Program Files\OpenSSL-Win64\bin\openssl.exe' genpkey -algorithm RSA -aes-256-cbc -pkeyopt rsa_keygen_bits:4096 -out '.\private\cafeamerica-ca.key.pem'
    if ($LASTEXITCODE -ne 0) { throw 'Fallo la generacion; revisar el error.' }
}
```

OpenSSL solicita contraseña y confirmación. No muestra los caracteres al escribir. No incluir contraseñas en comandos, scripts ni esta bitácora.

Verificación repetible sin regenerar:

```powershell
& 'C:\Program Files\OpenSSL-Win64\bin\openssl.exe' pkey -in "$env:USERPROFILE\GecoPKI\private\cafeamerica-ca.key.pem" -check -noout
```

Resultado confirmado el 24/09/2026:

```text
Key is valid
```

### Explicación de lo generado

Es una clave privada RSA de 4096 bits, almacenada en PEM y cifrada con AES-256-CBC. De ella se obtiene la clave pública correspondiente. La clave privada permite firmar certificados y es el material secreto de la autoridad.

`Key is valid` confirma que OpenSSL pudo abrirla con la contraseña y verificar su consistencia matemática. No confirma respaldos, permisos de archivos ni confianza de navegadores.

Este paso no genera un certificado público, un certificado de IIS ni un PFX. No configura HTTPS. La nueva vigencia solicitada no requiere regenerar esta clave.

## Paso 5 — Certificado público de la raíz, con 200 años calendario

Identidad: país `AR`, organización `Cafe America`, unidad `GECO`, nombre `Cafe America GECO Root CA`.

La CA raíz firmará directamente los certificados de servidor en esta implementación. `pathlen:0` impide cadenas con CA subordinadas; una futura jerarquía con intermediarias necesitaría un diseño distinto.

Componentes públicos, ubicados en el proyecto correcto:

- [Script de creación y verificación](Certificados/02-Crear-Certificado-Raiz.ps1).
- [Configuración OpenSSL](Certificados/cafeamerica-root-ca.cnf).

Mantener ambos archivos en la misma carpeta al replicar. El script requiere OpenSSL con soporte de `req -not_before` y `-not_after`, presente en la versión 3.6 comprobada. Lee la clave existente y pide su contraseña mediante OpenSSL; no la guarda, no instala confianza y no modifica IIS.

En PowerShell de la computadora donde está la clave, sin administrador:

```powershell
& 'D:\Sis25\git\GeConnect\src\Documentacion\Certificados\02-Crear-Certificado-Raiz.ps1'
```

El parámetro predeterminado es `-Anios 200`. El script toma una fecha UTC común para inicio y fin, suma 200 años mediante `AddYears(200)` y pasa ambas fechas completas a OpenSSL. Esto incluye los años bisiestos, en lugar de aproximar a 73000 días. Para una fecha inicial del 29 de febrero que no exista en el año de destino, `AddYears` usa el 28 de febrero.

Se detiene si ya existe cualquiera de los certificados de salida. Si se ejecutó la versión anterior de 10 años, no borrar ni reemplazar automáticamente esos certificados: comprobar primero si se distribuyeron o utilizaron para emitir otros. Compartir el mensaje para resolver ese caso conservando la clave.

### Incidencia: ejecución de scripts deshabilitada (24/09/2026)

El usuario informó `PSSecurityException / UnauthorizedAccess`. Ese intento no ejecutó el script ni generó el certificado. El usuario luego confirmó la ejecución correcta y la emisión de la raíz; no aportó el listado de políticas efectivo.

En la misma ventana de PowerShell local, consultar:

```powershell
Get-ExecutionPolicy -List
```

Si `MachinePolicy` y `UserPolicy` muestran `Undefined`, habilitar scripts locales solo para la sesión:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy RemoteSigned
Get-ExecutionPolicy
```

Aceptar la confirmación si aparece. Continuar cuando la política efectiva sea `RemoteSigned`:

```powershell
& 'D:\Sis25\git\GeConnect\src\Documentacion\Certificados\02-Crear-Certificado-Raiz.ps1'
```

No requiere administrador. El ajuste termina al cerrar la ventana. Si hay una política organizacional definida o persiste el bloqueo, compartir la lista y el error para determinar la vía permitida; no eludirla ni modificar políticas globales.

Referencia: [Microsoft — políticas de ejecución](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_execution_policies).

### Configuración utilizada

```ini
[req]
prompt = no
distinguished_name = ca_identity
x509_extensions = root_ca
default_md = sha256

[ca_identity]
C = AR
O = Cafe America
OU = GECO
CN = Cafe America GECO Root CA

[root_ca]
basicConstraints = critical, CA:true, pathlen:0
keyUsage = critical, keyCertSign, cRLSign
subjectKeyIdentifier = hash
authorityKeyIdentifier = keyid:always
```

### Comandos de emisión y verificación

Referencia equivalente a lo que hace el script. No ejecutar después de haber creado el certificado: para emitir se recomienda el script, que detiene el proceso ante errores y evita sobrescrituras.

```powershell
$openssl = 'C:\Program Files\OpenSSL-Win64\bin\openssl.exe'
$carpetaCA = Join-Path $env:USERPROFILE 'GecoPKI'
$clave = Join-Path $carpetaCA 'private\cafeamerica-ca.key.pem'
$certificado = Join-Path $carpetaCA 'certs\cafeamerica-ca.cert.pem'
$certificadoWindows = Join-Path $carpetaCA 'certs\cafeamerica-ca.cer'
$configuracion = 'D:\Sis25\git\GeConnect\src\Documentacion\Certificados\cafeamerica-root-ca.cnf'
$instanteEmision = [DateTime]::UtcNow
$formatoFecha = "yyyyMMddHHmmss'Z'"
$inicio = $instanteEmision.ToString($formatoFecha, [Globalization.CultureInfo]::InvariantCulture)
$vencimiento = $instanteEmision.AddYears(200).ToString($formatoFecha, [Globalization.CultureInfo]::InvariantCulture)

& $openssl req -new -x509 -sha256 -not_before $inicio -not_after $vencimiento -config $configuracion -key $clave -out $certificado
& $openssl verify -x509_strict -check_ss_sig -CAfile $certificado $certificado
& $openssl x509 -in $certificado -outform DER -out $certificadoWindows
& $openssl x509 -in $certificado -noout -subject -issuer -serial -dates -fingerprint -sha256 -ext 'basicConstraints,keyUsage,subjectKeyIdentifier,authorityKeyIdentifier'
```

El script también lee el DER mediante `X509Certificate2` y comprueba que Windows interpreta exactamente las fechas solicitadas, incluido el vencimiento en el siglo XXIII. Esta comprobación local no sustituye las pruebas posteriores en cada consumidor.

Resultados esperados:

1. `cafeamerica-ca.cert.pem: OK` al verificar estructura y firma.
2. `subject` e `issuer` con la misma identidad: raíz autofirmada.
3. `Basic Constraints`: crítica, `CA:TRUE, pathlen:0`.
4. `Key Usage`: crítica, `Certificate Sign, CRL Sign`.
5. `notBefore`: fecha de emisión según el reloj UTC; `notAfter`: 200 años después.
6. Mensaje `Fechas verificadas con Windows: coinciden con las solicitadas.`

Revisar que el reloj del equipo sea correcto antes de emitir. Los datos reales de la raíz ya emitida se registran a continuación, según la salida proporcionada por el usuario.

Consulta posterior de datos públicos, sin emitir nuevamente:

```powershell
& 'C:\Program Files\OpenSSL-Win64\bin\openssl.exe' x509 -in "$env:USERPROFILE\GecoPKI\certs\cafeamerica-ca.cert.pem" -noout -subject -issuer -serial -dates -fingerprint -sha256
```

`OK` usa el certificado como raíz explícita para esa ejecución de OpenSSL; no instala confianza en Windows o navegadores. Los archivos PEM y CER contienen el mismo certificado público, sin la clave privada. Ante errores, conservar el mensaje y revisar archivos parciales antes de reintentar; no regenerar la clave como forma de resolver errores de certificado.

## Resultado confirmado — Certificado raíz emitido

Evidencia: salida completa proporcionada por el usuario el 24/09/2026. No se volvió a emitir la raíz para documentarla.

| Dato | Valor confirmado |
|---|---|
| Identidad (subject e issuer) | `C=AR, O=Cafe America, OU=GECO, CN=Cafe America GECO Root CA` |
| Serie | `4B5D15549C09733DD6BCC0B4C5C440C6B4D32C31` |
| Inicio UTC | `2026-09-24 13:45:52` |
| Vencimiento UTC | `2226-09-24 13:45:52` |
| Huella SHA-256 | `56:DD:C7:40:DD:BE:C1:ED:58:55:24:47:D2:E7:F8:5B:54:EB:6E:C0:6B:7A:9E:2B:27:78:5C:E9:3A:86:B8:7A` |
| Basic Constraints | Crítica: `CA:TRUE, pathlen:0` |
| Key Usage | Crítica: `Certificate Sign, CRL Sign` |
| Subject/Authority Key Identifier | Ambos: `C0:5D:14:40:C5:D9:8A:7F:27:CE:7C:18:6C:90:68:D7:B4:70:9D:D0` |
| Verificación OpenSSL | `cafeamerica-ca.cert.pem: OK` |
| Lectura de fechas en Windows | Coincide con las fechas solicitadas |
| PEM real | `C:\Users\Juanjobe\GecoPKI\certs\cafeamerica-ca.cert.pem` |
| DER real | `C:\Users\Juanjobe\GecoPKI\certs\cafeamerica-ca.cer` |

La CA ya tiene su clave privada y su certificado público. La huella identifica este certificado concreto y permite contrastar una copia antes de instalarla como raíz de confianza. Todavía no se instaló confianza ni se modificó IIS. No volver a generar la raíz para continuar.

## Paso 6 — Clave propia del backend y solicitud CSR (completado)

Una CSR es una solicitud firmada con la clave privada del servidor: contiene su clave pública, identidad y los nombres que pedimos certificar. Todavía no es el certificado final. No tiene el vencimiento del futuro certificado; se definirá al firmarla con la CA.

La clave del backend es diferente de la clave de la CA. Se creó en la computadora del usuario para preparar luego el PFX que se importará en IIS. Elegir una contraseña nueva para esta clave y guardarla; no enviar contraseñas ni claves al chat.

Ejecutar en PowerShell local, en la sesión ya habilitada, sin administrador:

```powershell
& 'D:\Sis25\git\GeConnect\src\Documentacion\Certificados\03-Crear-Solicitud-Servidor.ps1'
```

Si se abrió otra ventana y se repite el bloqueo de scripts, aplicar la consulta y habilitación temporal documentadas en el paso anterior.

[Script del paso](Certificados/03-Crear-Solicitud-Servidor.ps1). Sus valores predeterminados son `-NombreDNS api-dev.cafeamerica.com.ar` y `-CarpetaPKI "$env:USERPROFILE\GecoPKI"`. El nombre se valida antes de utilizarlo en rutas y configuración. Se detiene si encuentra salidas existentes; si ocurre un fallo parcial, conservar archivos y error para resolverlo sin sobrescribir claves.

Archivos previstos bajo `GecoPKI\servidores\api-dev.cafeamerica.com.ar`:

| Archivo | Contenido |
|---|---|
| `private\servidor.key.pem` | Clave RSA de 3072 bits, cifrada con AES-256-CBC; privada |
| `servidor.csr.pem` | Solicitud pública, firmada con la clave del backend |
| `servidor-request.cnf` | Configuración pública de la solicitud |

OpenSSL pedirá elegir una contraseña para la nueva clave y repetirla. No se usa ni se solicita la contraseña de la CA en este paso.

Configuración generada por el script:

```ini
[req]
prompt = no
distinguished_name = server_identity
req_extensions = server_request
default_md = sha256

[server_identity]
C = AR
O = Cafe America
OU = GECO
CN = api-dev.cafeamerica.com.ar

[server_request]
basicConstraints = critical, CA:false
keyUsage = critical, digitalSignature, keyEncipherment
extendedKeyUsage = serverAuth
subjectAltName = @server_names

[server_names]
DNS.1 = api-dev.cafeamerica.com.ar
```

Comandos equivalentes para referencia (la generación se realiza una sola vez; preferir el script con sus guardas):

```powershell
$openssl = 'C:\Program Files\OpenSSL-Win64\bin\openssl.exe'
$carpetaServidor = Join-Path $env:USERPROFILE 'GecoPKI\servidores\api-dev.cafeamerica.com.ar'
$claveServidor = Join-Path $carpetaServidor 'private\servidor.key.pem'
$solicitud = Join-Path $carpetaServidor 'servidor.csr.pem'
$configuracion = Join-Path $carpetaServidor 'servidor-request.cnf'

# El script crea previamente la carpeta y el archivo de configuracion de arriba.
& $openssl req -new -newkey rsa:3072 -cipher aes-256-cbc -sha256 -config $configuracion -keyout $claveServidor -out $solicitud
& $openssl req -in $solicitud -verify -noout -subject
& $openssl req -in $solicitud -noout -text
```

Las dos últimas consultas pueden repetirse sin regenerar. Comprobar:

- `Certificate request self-signature verify OK`.
- Subject con `CN=api-dev.cafeamerica.com.ar`.
- SAN exacto: `DNS:api-dev.cafeamerica.com.ar`, sin IP ni nombres de rutas.
- `CA:FALSE`, uso de autenticación TLS de servidor, y clave pública RSA de 3072 bits.
- Mensaje final del script: clave de servidor y CSR creadas y verificadas.

La firma válida de la solicitud acredita la relación con su clave; no acredita todavía la firma de la CA ni la confianza en Windows. Las extensiones solicitadas deberán comprobarse nuevamente en el certificado final. Tras recibir el resultado, el siguiente paso será emitir el certificado del backend y preparar su PFX.

## Resultado confirmado — Clave y CSR del backend

El usuario devolvió el 24/09/2026:

```text
Certificate request self-signature verify OK
subject=C=AR, O=Cafe America, OU=GECO, CN=api-dev.cafeamerica.com.ar
SAN verificado: DNS:api-dev.cafeamerica.com.ar
Solicitud para servidor: CA:FALSE; autenticacion TLS de servidor.
```

Ubicaciones comunicadas:

- Clave cifrada: `C:\Users\Juanjobe\GecoPKI\servidores\api-dev.cafeamerica.com.ar\private\servidor.key.pem`.
- CSR: `C:\Users\Juanjobe\GecoPKI\servidores\api-dev.cafeamerica.com.ar\servidor.csr.pem`.
- Configuración: `C:\Users\Juanjobe\GecoPKI\servidores\api-dev.cafeamerica.com.ar\servidor-request.cnf`.

No volver a ejecutar el paso 03. El certificado y el PFX ya fueron generados; la confianza del usuario, la importación del servidor y el enlace IIS fueron confirmados en los pasos posteriores. No repetir la generación.

## Paso 7 — Firmar el certificado del backend (completado)

Se usa la CA existente para emitir un certificado de servidor con **200 años calendario solicitados**, por instrucción expresa del usuario. Se calcula la menor fecha entre la emisión más 200 años y el vencimiento de la raíz. Para esta CA, el vencimiento efectivo será **24/09/2226 a las 13:45:52 UTC**. La raíz se conserva. No se amplía ningún certificado ya emitido; los scripts mantienen sus guardas contra sobrescritura.

En PowerShell local, dentro de la sesión habilitada:

```powershell
& 'D:\Sis25\git\GeConnect\src\Documentacion\Certificados\04-Emitir-Certificado-Servidor.ps1'
```

[Script de emisión](Certificados/04-Emitir-Certificado-Servidor.ps1). Pedirá la contraseña de la **clave de la autoridad**, no la del backend. El nombre predeterminado es `api-dev.cafeamerica.com.ar`. Se pueden indicar `-NombreDNS`, `-CarpetaPKI`, `-OpenSSLPath` y `-Anios` (1 a 200, predeterminado 200). Se reemplaza el antiguo parámetro `-Dias`.

El script verifica la CSR, el CN y el SAN, y comprueba la raíz. Fija las extensiones del certificado explícitamente; no copia otras extensiones arbitrarias de la solicitud. Evita sobrescribir certificados y registra la emisión mediante `openssl ca`.

Se inicializa una sola vez `GecoPKI\ca-db`, con `index.txt`, `index.txt.attr`, `serial`, `newcerts` y `openssl-ca.cnf`. Conservar y respaldar toda esta base junto con la CA: no borrar ni reiniciar el índice o la serie entre emisiones. Un bloqueo de archivo evita dos escritores simultáneos que usen este script. No administrar la misma base en paralelo mediante otros comandos.

Configuración de la CA generada en la primera emisión (las rutas se resuelven desde `GecoPKI`):

```ini
[ca]
default_ca = CA_default

[CA_default]
database = ./ca-db/index.txt
new_certs_dir = ./ca-db/newcerts
serial = ./ca-db/serial
certificate = ./certs/cafeamerica-ca.cert.pem
private_key = ./private/cafeamerica-ca.key.pem
default_md = sha256
# La vigencia se pasa explicitamente mediante -startdate y -enddate.
default_crl_days = 30
policy = policy_server
unique_subject = no
copy_extensions = none

[policy_server]
countryName = match
organizationName = match
organizationalUnitName = optional
commonName = supplied
```

La serie inicial es aleatoria (16 bytes) y OpenSSL la incrementa al emitir. Se conserva una copia pública de cada certificado bajo `newcerts`. La base permitirá administrar emisiones y revocaciones; no hay todavía CRL publicada ni comprobación de revocación desplegada en clientes.

Extensiones escritas en `servidores\api-dev.cafeamerica.com.ar\servidor-cert.cnf`:

```ini
[server_certificate]
basicConstraints = critical, CA:false
keyUsage = critical, digitalSignature, keyEncipherment
extendedKeyUsage = serverAuth
subjectKeyIdentifier = hash
authorityKeyIdentifier = keyid:always
subjectAltName = DNS:api-dev.cafeamerica.com.ar
```

Comandos equivalentes, para referencia. Usar el script para conservar sus controles de archivos, errores y bloqueo:

```powershell
$openssl = 'C:\Program Files\OpenSSL-Win64\bin\openssl.exe'
$carpetaPKI = Join-Path $env:USERPROFILE 'GecoPKI'
$carpetaServidor = Join-Path $carpetaPKI 'servidores\api-dev.cafeamerica.com.ar'
$solicitud = Join-Path $carpetaServidor 'servidor.csr.pem'
$certificado = Join-Path $carpetaServidor 'servidor.cert.pem'
$certificadoCA = Join-Path $carpetaPKI 'certs\cafeamerica-ca.cert.pem'
$configuracionCA = Join-Path $carpetaPKI 'ca-db\openssl-ca.cnf'
$extensiones = Join-Path $carpetaServidor 'servidor-cert.cnf'

# Fechas exactas en UTC, sin superar la vigencia de la autoridad.
$finCA = & $openssl x509 -in $certificadoCA -enddate -dateopt iso_8601 -noout
if ($LASTEXITCODE -ne 0 -or $finCA -notmatch '^notAfter=') { throw 'No se pudo leer la fecha de la CA.' }
$cultura = [Globalization.CultureInfo]::InvariantCulture
$estiloUTC = [Globalization.DateTimeStyles]::AssumeUniversal -bor [Globalization.DateTimeStyles]::AdjustToUniversal
$finCAUtc = [DateTime]::ParseExact($finCA.Substring(9), "yyyy-MM-dd HH:mm:ss'Z'", $cultura, $estiloUTC)
$instanteEmision = [DateTime]::UtcNow
$finEfectivo = $instanteEmision.AddYears(200)
if ($finEfectivo -gt $finCAUtc) { $finEfectivo = $finCAUtc }
if ($finEfectivo -le $instanteEmision) { throw 'La CA no permite emitir un certificado vigente.' }
$inicio = $instanteEmision.ToString("yyyyMMddHHmmss'Z'", $cultura)
$vencimiento = $finEfectivo.ToString("yyyyMMddHHmmss'Z'", $cultura)

# Solo con base y configuraciones inicializadas por el script; no repetir la emision.
Push-Location $carpetaPKI
try {
    & $openssl ca -batch -notext -config $configuracionCA -extfile $extensiones -extensions server_certificate -startdate $inicio -enddate $vencimiento -md sha256 -in $solicitud -out $certificado
} finally { Pop-Location }

# Estas verificaciones si pueden repetirse:
& $openssl verify -x509_strict -purpose sslserver -verify_hostname api-dev.cafeamerica.com.ar -CAfile $certificadoCA $certificado
& $openssl req -in $solicitud -pubkey -noout
& $openssl x509 -in $certificado -pubkey -noout
& $openssl x509 -in $certificado -noout -subject -issuer -serial -dates -fingerprint -sha256 -ext 'subjectAltName,basicConstraints,keyUsage,extendedKeyUsage'
```

Resultados esperados: `servidor.cert.pem: OK`, CN y SAN de api-dev, emisor `Cafe America GECO Root CA`, `CA:FALSE`, uso TLS de servidor y coincidencia entre las claves públicas de CSR y certificado. El script compara esas claves automáticamente y verifica que las fechas emitidas coincidan con las calculadas. La serie, huella y fechas reales se registran en el resultado confirmado de este paso. Si una base existente conserva default_days = 365 de una versión anterior, las fechas explícitas -startdate/-enddate tienen prioridad; no reiniciar la base para cambiar ese valor.

Si falla una firma, no regenerar claves ni borrar el índice para reintentar. Conservar el error y los archivos, que pueden incluir una emisión ya registrada.

## Resultado confirmado — Certificado del backend emitido

Evidencia entregada por el usuario el 24/09/2026, después de ejecutar el script 04. Se registran los datos sin reemitir ni modificar el certificado.

| Dato | Valor confirmado |
|---|---|
| Subject | `C=AR, O=Cafe America, OU=GECO, CN=api-dev.cafeamerica.com.ar` |
| Emisor | `C=AR, O=Cafe America, OU=GECO, CN=Cafe America GECO Root CA` |
| Serie | `1D6EB4BED2B132407AEAE2ACB045EFAF` |
| Inicio UTC | `2026-09-24 16:22:50` |
| Vencimiento UTC | `2226-09-24 13:45:52` |
| Huella SHA-256 | `CE:A7:6F:01:B4:59:09:46:FE:81:8C:0D:AE:2C:73:F9:C7:39:A4:45:70:07:05:BE:28:01:38:DC:2B:8F:35:A4` |
| SAN | `DNS:api-dev.cafeamerica.com.ar` |
| Basic Constraints | Crítica: `CA:FALSE` |
| Key Usage | Crítica: `Digital Signature, Key Encipherment` |
| Extended Key Usage | `TLS Web Server Authentication` |
| Cadena, nombre y uso TLS | `servidor.cert.pem: OK` |
| Correspondencia de clave pública | Verificada por el script contra la CSR |
| Fechas | Verificadas; el vencimiento no supera el de la CA |
| Registro de emisión | `Database updated`; una nueva entrada |
| Archivo | `C:\Users\Juanjobe\GecoPKI\servidores\api-dev.cafeamerica.com.ar\servidor.cert.pem` |
| Base de la CA | `C:\Users\Juanjobe\GecoPKI\ca-db` |

No volver a ejecutar el script 04: el certificado ya existe. El PFX con la clave existente del backend fue generado posteriormente mediante el script 05, según el resultado registrado más adelante. La exportación no cambia la fecha de vencimiento. No se ha confirmado todavía confianza instalada ni configuración HTTPS en IIS.

## Paso 8 — Preparar y verificar el PFX para IIS (completado)

El usuario confirmó que el backend utiliza Windows Server 2019, 2022 o 2025; esa familia admite PFX con AES256-SHA256. Versión exacta pendiente. Se mantiene cifrado moderno, sin modo legacy.

Solo si el paso 04 terminó correctamente:

```powershell
& 'D:\Sis25\git\GeConnect\src\Documentacion\Certificados\05-Exportar-PFX-IIS.ps1'
```

[Script de exportación](Certificados/05-Exportar-PFX-IIS.ps1). Pedirá, en este orden:

1. `Enter pass phrase ...servidor.key.pem`: contraseña de la **clave del backend**.
2. `Enter Export Password`: elegir una contraseña nueva para el **PFX**.
3. `Verifying - Enter Export Password`: repetir esa contraseña del PFX.
4. `Enter Import Password`: ingresar otra vez la contraseña del PFX para verificarlo. Esta comprobación no instala nada.

Guardar la contraseña del PFX: se necesitará en IIS. No enviar contraseñas ni archivos PFX al chat.

Contenido: certificado del servidor, su clave privada y certificado **público** de la raíz. La clave privada de la CA no se incluye. La exportación verifica que la clave privada corresponda al certificado.

Comandos equivalentes (el script evita sobrescrituras y solo da el nombre final al archivo después de verificar):

```powershell
$openssl = 'C:\Program Files\OpenSSL-Win64\bin\openssl.exe'
$carpetaPKI = Join-Path $env:USERPROFILE 'GecoPKI'
$carpetaServidor = Join-Path $carpetaPKI 'servidores\api-dev.cafeamerica.com.ar'
$claveServidor = Join-Path $carpetaServidor 'private\servidor.key.pem'
$certificado = Join-Path $carpetaServidor 'servidor.cert.pem'
$certificadoCA = Join-Path $carpetaPKI 'certs\cafeamerica-ca.cert.pem'
$pfx = Join-Path $carpetaServidor 'servidor.iis.pfx'
$pfxPendiente = "$pfx.pending"

& $openssl pkcs12 -export -out $pfxPendiente -inkey $claveServidor -in $certificado -certfile $certificadoCA -name api-dev.cafeamerica.com.ar -keypbe AES-256-CBC -certpbe AES-256-CBC -macalg SHA256 -iter 100000
& $openssl pkcs12 -in $pfxPendiente -info -noout
# El script solo renombra si ambos comandos concluyen con codigo cero.
```

Resultados previstos: MAC SHA-256, cifrado AES-256-CBC y mensaje `PFX creado y verificado`. Destino final:

```text
C:\Users\Juanjobe\GecoPKI\servidores\api-dev.cafeamerica.com.ar\servidor.iis.pfx
```

Si falla la exportación o la contraseña de verificación, se conserva un archivo `.pending`; no reemitir el certificado ni importar ese archivo hasta revisar el error. Todavía no se instala confianza en Windows ni se cambia IIS.

### Validación previa de los componentes 04 y 05

Probados el 24/09/2026 con CA y claves descartables, sin utilizar las claves reales: emisión registrada, cadena/nombre/uso TLS, correspondencia de claves públicas, PFX cifrado y verificación de integridad correctos. Windows pudo leer el PFX en memoria con exactamente dos certificados y una clave privada, la del servidor; no se instaló confianza. Las guardas impidieron sobrescribir y no alteraron el índice en reintentos. El material privado de prueba fue eliminado. La emisión y la exportación reales fueron luego confirmadas por el usuario; la confianza del usuario local fue luego confirmada; confianza del servidor y configuración IIS también fueron confirmadas en los pasos posteriores.

## Resultado confirmado — PFX del backend

El 24/09/2026 el usuario confirmó la ejecución correcta del script 05:

```text
servidor.cert.pem: OK
MAC: sha256, Iteration 100000
MAC length: 32, salt length: 16
PKCS7 Encrypted data: PBES2, PBKDF2, AES-256-CBC, Iteration 100000, PRF hmacWithSHA256
Certificate bag
Certificate bag
PKCS7 Data
Shrouded Keybag: PBES2, PBKDF2, AES-256-CBC, Iteration 100000, PRF hmacWithSHA256
PFX creado y verificado
```

Archivo real: `C:\Users\Juanjobe\GecoPKI\servidores\api-dev.cafeamerica.com.ar\servidor.iis.pfx`.

Contiene la clave privada del backend, su certificado y el certificado público de la CA. No contiene la clave privada de la CA. La integridad y la contraseña de exportación se comprobaron mediante OpenSSL. Esa comprobación no importa el archivo a Windows ni configura IIS. No volver a ejecutar el script de exportación sobre el archivo existente.

## Paso 9 — Confiar en la CA desde el usuario de la computadora (completado)

Objetivo: instalar el certificado público de la CA en `Cert:\CurrentUser\Root`, el almacén de autoridades raíz de confianza del usuario actual. Esta confianza abarca certificados válidos emitidos por esa CA, no solamente un nombre DNS. Se aplicará al usuario de la computadora donde se realiza la prueba; servidores, otros usuarios y cuentas de servicio se configurarán por separado. No se necesita la clave privada de la CA ni el PFX para este paso.

[Script de confianza del usuario](Certificados/06-Confiar-CA-Usuario.ps1). Antes de importar, comprueba vigencia y que la huella SHA-256 del archivo DER coincida exactamente con la raíz registrada. Luego comprueba nuevamente la huella en el almacén. Si ya existe, la verifica sin duplicarla.

En PowerShell de la computadora del usuario, fuera del Escritorio remoto y con su usuario habitual, sin administrador:

```powershell
& 'D:\Sis25\git\GeConnect\src\Documentacion\Certificados\06-Confiar-CA-Usuario.ps1'
```

Usar la sesión habilitada para scripts. Si se abrió otra, consultar las políticas y aplicar el ajuste temporal documentado en el paso 5 cuando corresponda. No ejecutar como otro usuario, porque cambiaría el almacén destinatario.

Si Windows solicita confirmar la incorporación de una raíz, comprobar que sea `Cafe America GECO Root CA` y aceptar para completar este paso. No se pide contraseña: se instala únicamente el certificado público.

Archivo predeterminado: `$env:USERPROFILE\GecoPKI\certs\cafeamerica-ca.cer`.

Huella SHA-256 fijada y contrastada de forma local el 24/09/2026:

```text
56DDC740DDBEC1ED58552447D2E7F85B54EB6EC06B7A9E2B27785CE93A86B87A
```

El script permite `-ArchivoRaiz` para señalar una copia del mismo DER. Para una CA deliberadamente diferente se debe establecer su propia huella verificada mediante `-HuellaEsperada`; no cambiar el valor para silenciar una discrepancia inesperada.

Comandos equivalentes de referencia:

```powershell
$archivoRaiz = Join-Path $env:USERPROFILE 'GecoPKI\certs\cafeamerica-ca.cer'
$esperada = '56DDC740DDBEC1ED58552447D2E7F85B54EB6EC06B7A9E2B27785CE93A86B87A'
if ((Get-FileHash -LiteralPath $archivoRaiz -Algorithm SHA256).Hash -ne $esperada) {
    throw 'La huella no coincide; detenerse sin instalar.'
}
Import-Certificate -FilePath $archivoRaiz -CertStoreLocation 'Cert:\CurrentUser\Root'
Get-Item 'Cert:\CurrentUser\Root\804A4E8101F68B142C50E06F88FF98D2425416C7' |
    Select-Object Subject, Thumbprint, NotAfter
```

La huella de 40 caracteres utilizada en la ruta es el identificador SHA-1 que Windows usa para localizar el certificado. El control de identidad del script se realiza con SHA-256 y no cambia el algoritmo de firma del certificado.

Resultado esperado:

```text
Confianza de la CA instalada y verificada para el usuario actual.
```

Esto no verifica todavía el HTTPS del backend. Después se trasladarán al servidor únicamente los archivos necesarios, se revisarán sitios y enlaces existentes y se preparará la instalación en IIS.

Validación del componente 06: sintaxis comprobada y ejecución con `-WhatIf` usando exclusivamente el certificado público real. Huella y vigencia correctas; no se instaló confianza durante esa prueba. El usuario confirmó posteriormente la instalación real y la verificación de identidad, registradas a continuación.

## Resultado confirmado — Confianza del usuario Windows

El 24/09/2026 el usuario devolvió:

```text
Autoridad verificada: CN=Cafe America GECO Root CA, OU=GECO, O=Cafe America, C=AR
Huella SHA-256: 56DDC740DDBEC1ED58552447D2E7F85B54EB6EC06B7A9E2B27785CE93A86B87A
Destino: Cert:\CurrentUser\Root (solo el usuario actual de Windows).
Vencimiento UTC: 2226-09-24 13:45:52
Confianza de la CA instalada y verificada para el usuario actual.
El servidor IIS y las cuentas de servicio se configuraran por separado.
```

Se da por completada la instalación de la raíz para ese usuario. No acredita confianza para otras cuentas ni para el servidor remoto; tampoco prueba que el backend ya sirva HTTPS. No es necesario repetir el script 06 en esa misma cuenta.

## Paso 10 — Relevar IIS del backend (completado)

Conectarse por Escritorio remoto a `172.10.10.12`. Abrir **Windows PowerShell como administrador dentro de ese servidor**. Usar Windows PowerShell (5.1) para este módulo de administración de IIS.

Ejecutar el bloque completo. Son consultas de configuración: no crean sitios, no cambian enlaces ni importan certificados.

```powershell
Import-Module WebAdministration -ErrorAction Stop

Get-Website |
    Select-Object Name, State, PhysicalPath |
    Format-List

Get-Website | ForEach-Object {
    $sitio = $_
    Get-WebBinding -Name $sitio.Name |
        Select-Object @{Name='Sitio'; Expression={$sitio.Name}},
            protocol, bindingInformation, certificateStoreName,
            certificateHash, sslFlags
} | Format-List
```

Registrar la salida antes de elegir el sitio para las aplicaciones. `bindingInformation` muestra IP, puerto y nombre de host. Los enlaces HTTPS existentes permiten identificar certificados, almacenes y opciones SSL ya utilizados. No asumir que Default Web Site está disponible ni reemplazar enlaces de otros servicios.

Si el módulo no está disponible o la consulta falla, conservar el mensaje y detenerse; no instalar características ni reiniciar IIS como parte de este relevamiento. El resultado fue entregado por el usuario y se registra a continuación.

Después de revisar los sitios se indicará el traslado del PFX y del certificado público raíz, la importación en los almacenes del equipo servidor y el enlace HTTPS correspondiente. La clave privada de la CA permanece en la computadora de administración y nunca se copia al backend.

## Resultado confirmado — IIS del backend

Salida comunicada por el usuario el 24/09/2026:

| Sitio | Estado | Carpeta | Enlace | Almacén | Certificado actual | sslFlags |
|---|---|---|---|---|---|---|
| Default Web Site | Started | `%SystemDrive%\inetpub\wwwroot` | HTTP `*:80:` | Sin certificado | Sin certificado | 0 |
| SitiosGC | Started | `C:\Sitios` | HTTPS `*:443:` | My | `A0864BEEB174C572ACEF84C800C3A006BDE11B6D` | 0 |

El enlace HTTPS de SitiosGC no especifica nombre de host y no usa SNI. Ya tiene otro certificado instalado. Se propone utilizar SitiosGC para las aplicaciones por ruta y agregar un enlace HTTPS con host `api-dev.cafeamerica.com.ar` y SNI, asociado al nuevo certificado. Conservar el enlace `*:443:` y su certificado para los consumidores existentes. La configuración de aplicaciones y la prueba funcional se revisarán después; este inventario no acredita qué aplicaciones están desplegadas.

Todavía no se cambiaron enlaces ni certificados. Antes de modificar enlaces se respaldará la configuración IIS y se verificará nuevamente que no haya conflicto.

## Paso 11 — Copiar al backend el PFX y el certificado público raíz (completado)

Destino: servidor `172.10.10.12`, carpeta de preparación `C:\GecoCertificados`. Mantener el PFX bajo control del administrador durante la instalación; no colocarlo en `C:\Sitios` ni en otra carpeta servida por IIS.

En Windows PowerShell del servidor, como administrador:

```powershell
New-Item -ItemType Directory -Path 'C:\GecoCertificados' -Force | Out-Null
```

Desde el Explorador de la computadora del usuario, copiar mediante la sesión de Escritorio remoto estos dos archivos a esa carpeta del servidor:

| Origen en la computadora | Destino en el servidor |
|---|---|
| `C:\Users\Juanjobe\GecoPKI\certs\cafeamerica-ca.cer` | `C:\GecoCertificados\cafeamerica-ca.cer` |
| `C:\Users\Juanjobe\GecoPKI\servidores\api-dev.cafeamerica.com.ar\servidor.iis.pfx` | `C:\GecoCertificados\servidor.iis.pfx` |

No copiar la carpeta completa GecoPKI ni la clave privada de la CA. El PFX ya contiene la clave privada necesaria para el backend. Si la sesión no permite copiar archivos, informar esa restricción para elegir un canal autorizado.

Después de copiar, consultar en PowerShell del servidor:

```powershell
Get-FileHash -LiteralPath 'C:\GecoCertificados\cafeamerica-ca.cer', 'C:\GecoCertificados\servidor.iis.pfx' -Algorithm SHA256 |
    Format-List Path, Hash
```

Valores de origen comprobados localmente el 24/09/2026, sin abrir ni exportar claves privadas:

| Archivo | SHA-256 del archivo |
|---|---|
| `cafeamerica-ca.cer` | `56DDC740DDBEC1ED58552447D2E7F85B54EB6EC06B7A9E2B27785CE93A86B87A` |
| `servidor.iis.pfx` | `365A16FC8D6144221828013D4CF008A941DB0BD6271EC498CB49C1DD13816ECE` |

Comparar ambos hashes antes de importar. El hash del PFX identifica este archivo concreto, no es la huella del certificado del backend; cambia si se vuelve a exportar el PFX. El usuario confirmó ambos hashes desde el servidor; coinciden exactamente con los de origen. Ningún comando de este paso importa certificados ni modifica IIS.

El paso siguiente instalará el certificado público raíz en el almacén del equipo y el certificado del backend con su clave en Personal (My) del equipo servidor. Después se preparará el nuevo enlace SNI. La contraseña requerida para importar será la contraseña de exportación del PFX.

## Resultado confirmado — Copia al servidor

El usuario informó desde `172.10.10.12` los siguientes hashes, coincidentes con el origen:

```text
C:\GecoCertificados\cafeamerica-ca.cer
56DDC740DDBEC1ED58552447D2E7F85B54EB6EC06B7A9E2B27785CE93A86B87A

C:\GecoCertificados\servidor.iis.pfx
365A16FC8D6144221828013D4CF008A941DB0BD6271EC498CB49C1DD13816ECE
```

La copia y la importación en almacenes del servidor están verificadas. El enlace IIS y la prueba TLS también se confirmaron posteriormente.

## Paso 12 — Importar raíz y PFX en el equipo servidor (completado)

En **Windows PowerShell como administrador dentro del servidor 172.10.10.12**, pegar el bloque completo de abajo. No ejecutarlo en la computadora de administración.

Solicita una vez la contraseña de **exportación del PFX**. Primero la comprueba con `Get-PfxData`, sin instalar. Después importa la raíz pública a `Cert:\LocalMachine\Root` y el PFX a `Cert:\LocalMachine\My`. La clave importada queda no exportable porque no se utiliza `-Exportable`; se conserva el PFX original para recuperación controlada. No se modifican enlaces ni sitios IIS en este paso.

Los archivos deben ser los verificados en el paso 11; si fueron reemplazados, repetir la comprobación de hashes antes de importar.

```powershell
& {
    $ErrorActionPreference = 'Stop'
    $clavePfx = Read-Host 'Contrasena de EXPORTACION del PFX' -AsSecureString
    try {
        Get-PfxData -FilePath 'C:\GecoCertificados\servidor.iis.pfx' -Password $clavePfx | Out-Null

        Import-Certificate -FilePath 'C:\GecoCertificados\cafeamerica-ca.cer' -CertStoreLocation 'Cert:\LocalMachine\Root' | Out-Null
        Import-PfxCertificate -FilePath 'C:\GecoCertificados\servidor.iis.pfx' -CertStoreLocation 'Cert:\LocalMachine\My' -Password $clavePfx | Out-Null

        $raiz = Get-Item 'Cert:\LocalMachine\Root\804A4E8101F68B142C50E06F88FF98D2425416C7'
        $certificado = Get-Item 'Cert:\LocalMachine\My\0D6D1A14F7698FD1C45F0AF9CAB85D31CA427363'
        if (-not $certificado.HasPrivateKey) {
            throw 'El certificado del backend no tiene su clave privada asociada.'
        }
        Write-Host "Raiz instalada en el equipo: $($raiz.Subject)"
        $certificado | Format-List Subject, Thumbprint, NotAfter, HasPrivateKey
    }
    finally {
        $clavePfx.Dispose()
    }
}
```

El bloque se detiene ante errores; la contraseña se mantiene como SecureString y se libera al finalizar. Si hay un fallo, conservar el mensaje y no repetir sin revisar qué importaciones llegaron a completarse.

Datos para verificar la salida:

- Raíz instalada con `CN=Cafe America GECO Root CA` en el almacén del equipo.
- Backend: `CN=api-dev.cafeamerica.com.ar`.
- Thumbprint del backend: `0D6D1A14F7698FD1C45F0AF9CAB85D31CA427363`.
- `HasPrivateKey : True` para el backend.
- Vencimiento en 2226. `NotAfter` se muestra según la zona horaria del servidor; el valor UTC registrado es `2226-09-24 13:45:52`.

El thumbprint SHA-1 utilizado para localizar el backend fue obtenido de su certificado público y contrastado con la huella SHA-256 ya registrada (`CE:A7:6F:01:...:35:A4`). Es un identificador del almacén Windows, no un cambio del algoritmo de firma. El certificado actual de SitiosGC (`A0864BEEB174C572ACEF84C800C3A006BDE11B6D`) permanece asociado a su enlace existente.

Estado: importación real confirmada por el usuario. La raíz está instalada en el equipo y el backend tiene su clave privada asociada. El enlace SNI fue confirmado en el paso 13; la prueba funcional HTTPS y el certificado original se confirmaron en los pasos 16 y 17.

## Resultado confirmado — Importación en el servidor

El usuario informó el siguiente resultado desde el backend:

```text
Raiz instalada en el equipo: CN=Cafe America GECO Root CA, OU=GECO, O=Cafe America, C=AR
Subject       : CN=api-dev.cafeamerica.com.ar, OU=GECO, O=Cafe America, C=AR
Thumbprint    : 0D6D1A14F7698FD1C45F0AF9CAB85D31CA427363
NotAfter      : 24/9/2226 10:45:52
HasPrivateKey : True
```

La hora local 10:45:52 con desplazamiento UTC-3 equivale a 13:45:52 UTC, el vencimiento registrado. HasPrivateKey=True confirma la asociación de la clave privada. El enlace se confirmó posteriormente; falta probar HTTPS desde el cliente. No repetir la importación.

## Paso 13 — Respaldar IIS y agregar el enlace HTTPS con SNI (enlace confirmado; respaldo por registrar)

Ejecutar en **Windows PowerShell como administrador dentro del servidor 172.10.10.12**:

```powershell
& {
    $ErrorActionPreference = 'Stop'
    Import-Module WebAdministration
    $certificado = Get-Item 'Cert:\LocalMachine\My\0D6D1A14F7698FD1C45F0AF9CAB85D31CA427363'
    if (-not $certificado.HasPrivateKey) { throw 'Falta la clave privada del backend.' }
    $existente = @(Get-WebBinding -Protocol https | Where-Object {
        $_.bindingInformation -like '*:443:api-dev.cafeamerica.com.ar'
    })
    if ($existente.Count -gt 0) { throw 'Ya existe un enlace para este host y puerto. Revisar antes de continuar.' }
    $respaldo = 'GECO-Antes-ApiDev-' + (Get-Date -Format 'yyyyMMdd-HHmmss')
    Backup-WebConfiguration -Name $respaldo
    $carpeta = Join-Path $env:windir "System32\inetsrv\backup\$respaldo"
    if (-not (Test-Path -LiteralPath (Join-Path $carpeta 'applicationHost.config'))) {
        throw 'No se encontro el respaldo de applicationHost.config. Detenerse.'
    }
    Write-Host "Respaldo IIS verificado: $carpeta"
}
```

Continuar solamente si aparece `Respaldo IIS verificado`. Conservar el nombre y la ubicación. Este respaldo guarda la configuración IIS; no reemplaza los respaldos de aplicaciones, certificados, claves ni del PFX.

En **Administrador de Internet Information Services (IIS)**:

1. Abrir **Sitios → SitiosGC → Enlaces…**.
2. Pulsar **Agregar…**, dejando intacto el enlace HTTPS existente.
3. Completar:

| Campo | Valor |
|---|---|
| Tipo | https |
| Dirección IP | Todas las no asignadas (All Unassigned) |
| Puerto | 443 |
| Nombre de host | api-dev.cafeamerica.com.ar |
| Requerir indicación de nombre de servidor / Require Server Name Indication | Activado |
| Certificado SSL | Certificado de api-dev.cafeamerica.com.ar |

4. Usar **Ver…** para comprobar el certificado seleccionado: huella `0D6D1A14F7698FD1C45F0AF9CAB85D31CA427363`, emitido por Cafe America GECO Root CA y vencimiento 24/09/2226. La huella puede aparecer en minúsculas o con espacios.
5. Aceptar y cerrar. Si IIS muestra un conflicto, detenerse y conservar el mensaje; no reemplazar otro enlace.
6. No hace falta ejecutar `iisreset` para agregar este enlace.

SNI permite que el cliente indique el nombre durante el inicio de TLS y que IIS seleccione su certificado. El enlace nuevo utiliza el mismo sitio y sus aplicaciones. El enlace anterior sin nombre conserva su certificado.

Verificación posterior en PowerShell del servidor:

```powershell
Get-WebBinding -Name 'SitiosGC' -Protocol https |
    Format-List bindingInformation, certificateStoreName, certificateHash, sslFlags
```

Se esperan estos dos enlaces:

| bindingInformation | certificateStoreName | certificateHash | sslFlags |
|---|---|---|---|
| *:443: | My | A0864BEEB174C572ACEF84C800C3A006BDE11B6D | 0 |
| *:443:api-dev.cafeamerica.com.ar | My | 0D6D1A14F7698FD1C45F0AF9CAB85D31CA427363 | 1 |

El usuario confirmó ambos enlaces con los valores esperados. No repetir el alta. El nombre y el resultado del respaldo previo aún no fueron comunicados y no se dan por confirmados. El siguiente paso es probar HTTPS desde el cliente VPN.

Si hace falta deshacer exclusivamente este paso, retirar desde Enlaces solamente el nuevo enlace `*:443:api-dev.cafeamerica.com.ar`; conservar el anterior. No restaurar automáticamente toda la configuración IIS, porque podría revertir otros cambios posteriores al respaldo.

## Resultado confirmado — Enlaces HTTPS de SitiosGC

El usuario devolvió:

```text
bindingInformation   : *:443:
certificateStoreName : My
certificateHash      : A0864BEEB174C572ACEF84C800C3A006BDE11B6D
sslFlags             : 0

bindingInformation   : *:443:api-dev.cafeamerica.com.ar
certificateStoreName : My
certificateHash      : 0D6D1A14F7698FD1C45F0AF9CAB85D31CA427363
sslFlags             : 1
```

La configuración coincide con lo previsto: el host nuevo usa SNI y el certificado emitido, y el enlace anterior mantiene su certificado. Esto verifica la configuración consultada en IIS; la conexión real desde el cliente todavía debe comprobarse. No se recibió el nombre ni el resultado del respaldo previo.

## Paso 14 — Probar HTTPS desde la computadora conectada a la VPN (respuesta HTTP recibida)

Usar la computadora del usuario donde se configuró el nombre y se instaló la raíz en CurrentUser\Root, con la VPN conectada. No hacer esta prueba desde el Escritorio remoto: buscamos comprobar el recorrido desde el cliente.

1. Abrir en Edge o Chrome `https://api-dev.cafeamerica.com.ar/`.
2. Si aparece una advertencia de certificado, no continuar pasando por alto la advertencia; copiar el código exacto.
3. Si la conexión se acepta, abrir la información del sitio junto a la dirección y consultar el certificado. Comprobar el nombre api-dev.cafeamerica.com.ar, emisor Cafe America GECO Root CA y vencimiento 24/09/2226. Si se muestra la huella SHA-256 debe ser `CEA76F01B4590946FE818C0DAE2C73F9C739A445700705BE280138DC2B8F35A4`. El identificador SHA-1 de Windows es `0D6D1A14F7698FD1C45F0AF9CAB85D31CA427363`.
4. Registrar la dirección final si hay redirección y el mensaje o página que aparece. Un 403.14 o 404 en la raíz puede corresponder a la configuración del sitio; por sí solo no indica un error de certificado. Tampoco demuestra que las API funcionen.

Comprobación reproducible en una ventana nueva de Windows PowerShell de esa misma computadora, sin privilegios de administrador:

```powershell
Invoke-WebRequest -Uri 'https://api-dev.cafeamerica.com.ar/' -UseBasicParsing -TimeoutSec 30 |
    Select-Object StatusCode, StatusDescription
```

Se mantiene la validación TLS predeterminada. UseBasicParsing evita procesar scripts del contenido. Si devuelve error, conservar su texto exacto: distinguir una respuesta HTTP (por ejemplo 403 o 404) de fallos de confianza, nombre, resolución, conexión o tiempo de espera. El comando sigue redirecciones; por eso se registra también la dirección observada en el navegador. No usar mecanismos para omitir la validación del certificado.

Si no conecta, estas consultas ayudan a localizar el problema, sin cambiar configuración:

```powershell
[System.Net.Dns]::GetHostAddresses('api-dev.cafeamerica.com.ar') |
    Select-Object IPAddressToString
Test-NetConnection -ComputerName 'api-dev.cafeamerica.com.ar' -Port 443 |
    Select-Object ComputerName, RemoteAddress, RemotePort, TcpTestSucceeded
```

Se espera la dirección 172.10.10.12 y TcpTestSucceeded=True. Estas consultas prueban resolución y conectividad TCP; no validan el certificado. El usuario informó posteriormente HTTP 403 desde navegador y PowerShell; se registra a continuación. La prueba funcional de GCServicios se confirmó posteriormente en el paso 16.

## Resultado confirmado — HTTP 403 en la raíz del sitio

El usuario informó que el navegador, al abrir https://api-dev.cafeamerica.com.ar/, mostró `403 - Forbidden: Access is denied.`. Invoke-WebRequest devolvió el mismo HTTP 403 y WebCmdletWebResponseException. No se informó error de confianza, nombre o negociación TLS.

Esto acredita una respuesta HTTP tras solicitar la URL HTTPS con el comando indicado. El error de PowerShell corresponde al estado HTTP no exitoso. No confirma que una API funcione ni identifica por sí solo el certificado presentado. El GET posterior del paso 16 confirmó ausencia de redirección y un certificado sustituto de Kaspersky; la prueba local del paso 17 confirmó el certificado original de IIS.

El mensaje genérico no contiene el subestado IIS: no se puede afirmar que sea 403.14 ni atribuirlo a permisos NTFS o credenciales. Una posibilidad es que la raíz de C:\Sitios no tenga documento de inicio y el listado esté deshabilitado; otras restricciones también producen 403. Para el diseño de aplicaciones por ruta, primero se identificarán las aplicaciones publicadas. No se habilitará el listado de directorios ni se cambiarán permisos como respuesta automática a este mensaje.

## Paso 15 — Consultar aplicaciones publicadas en SitiosGC (aplicaciones confirmadas)

Dentro del servidor 172.10.10.12, abrir Windows PowerShell como administrador y ejecutar:

```powershell
Import-Module WebAdministration -ErrorAction Stop

Get-WebApplication -Site 'SitiosGC' |
    Select-Object Path, PhysicalPath, ApplicationPool |
    Format-List

Get-WebVirtualDirectory -Site 'SitiosGC' |
    Select-Object Path, PhysicalPath |
    Format-List
```

Son consultas de configuración. Compartir la salida; si no aparecen resultados, indicarlo. Permiten distinguir aplicaciones configuradas y directorios virtuales para elegir una URL real. Un directorio virtual no equivale necesariamente a una aplicación. La ausencia de aplicaciones secundarias tampoco descarta una aplicación en la raíz del sitio.

No asumir que /apigeco, /apiclover o /swagger estén desplegados por haberlos usado como ejemplos del diseño. Con el inventario se seleccionará la aplicación y una ruta GET conocida para probarla. Si esa ruta también devuelve 403, se revisará el subestado y el código Win32 en el registro IIS antes de modificar autenticación o permisos.

Estado: el usuario informó cinco aplicaciones, detalladas a continuación. No se recibió una salida separada de directorios virtuales; no se infiere su ausencia. No se modificaron permisos, autenticación, documentos predeterminados ni configuración IIS. El nombre del respaldo previo sigue pendiente de informar.

## Resultado confirmado — Aplicaciones de SitiosGC

Inventario comunicado por el usuario:

| Ruta IIS | Carpeta física | Application Pool | Dirección base con el nuevo nombre |
|---|---|---|---|
| /GCServicios | C:\Sitios\GCServicios | GCServicios | https://api-dev.cafeamerica.com.ar/GCServicios/ |
| /ApiClover | C:\Sitios\ApiClover | ApiClover | https://api-dev.cafeamerica.com.ar/ApiClover/ |
| /ApiReport | C:\Sitios\ApiReport | ApiReport | https://api-dev.cafeamerica.com.ar/ApiReport/ |
| /GFidelity | C:\Sitios\GFidelity | GFidelity | https://api-dev.cafeamerica.com.ar/GFidelity/ |
| /ApiGen | C:\Sitios\ApiGenv02 | ApiGenV02Pool | https://api-dev.cafeamerica.com.ar/ApiGen/ |

Las direcciones base se derivan del enlace y las rutas IIS confirmadas; no son pruebas de funcionamiento. Todas usan el mismo certificado para api-dev.cafeamerica.com.ar. La carpeta física y el nombre del pool no determinan la URL: por ejemplo, ApiGenv02 corresponde a la ruta /ApiGen/. No se informó una aplicación /apigeco; era un ejemplo del diseño.

## Paso 16 — Probar una aplicación y confirmar el certificado presentado (GET confirmado; certificado original verificado en paso 17)

En la computadora del usuario conectada a la VPN, abrir https://api-dev.cafeamerica.com.ar/ApiClover/ en el navegador. Registrar la página o error y la dirección final si hay redirección. Consultar desde la información del sitio el certificado presentado: nombre api-dev.cafeamerica.com.ar, emisor Cafe America GECO Root CA, vencimiento 24/09/2226 y huella SHA-256 CEA76F01B4590946FE818C0DAE2C73F9C739A445700705BE280138DC2B8F35A4 (o SHA-1 de Windows 0D6D1A14F7698FD1C45F0AF9CAB85D31CA427363).

Comando reproducible desde la misma computadora:

```powershell
Invoke-WebRequest -Uri 'https://api-dev.cafeamerica.com.ar/ApiClover/' -UseBasicParsing -TimeoutSec 30 |
    Select-Object StatusCode, StatusDescription
```

No se sabe aún si ApiClover ofrece una página o respuesta GET en su raíz. Un 404 puede indicar que falta la ruta de un endpoint. Un 401 puede indicar autenticación requerida; un 403 requiere revisar su causa. Ninguno de estos estados equivale por sí solo a un fallo de certificado. No asumir que Swagger está habilitado.

Si el usuario ya dispone de una URL GET de consulta que funciona con ApiClover, utilizar su ruta completa cambiando solamente el esquema y el servidor a https://api-dev.cafeamerica.com.ar. No incluir contraseñas, tokens ni claves en la bitácora o el chat. Para completar la validación funcional hace falta conocer un endpoint y su respuesta esperada; la raíz de una API no siempre responde con 200.

La prueba propuesta sobre ApiClover fue sustituida por una ruta de GCServicios conocida por el usuario, cuyo resultado exitoso se registra a continuación. Las demás aplicaciones no fueron probadas. La configuración de certificados y enlaces no requiere cambios por existir estas cinco aplicaciones.

## Resultado confirmado — GET autorizado a GCServicios

El usuario informó una respuesta correcta y autorizó expresamente a Codex a invocar y observar:

`https://api-dev.cafeamerica.com.ar/gcservicios/api/administracion`

Consulta GET realizada el 24/09/2026 a las 21:11:33 de Argentina (25/09/2026 00:11:33 UTC), sin token ni credenciales HTTP, con la validación TLS de Windows habilitada y sin seguir redirecciones:

| Comprobación | Resultado |
|---|---|
| Resolución desde el equipo | 172.10.10.12 |
| Estado HTTP | 200 OK |
| URL de respuesta | Igual a la solicitada; sin redirección |
| Content-Type | application/json; charset=utf-8 |
| JSON | Válido, propiedades superiores data y meta |
| Registros en data | 8 |
| meta.totalCount | 8 |
| Paginación | Página 1 de 1, pageSize 200, sin página siguiente ni anterior |
| Longitud del cuerpo | 4380 caracteres |

No se incluye el cuerpo completo en esta bitácora. El contenido contiene campos llamados cx_login, cx_pass y cx_profile. No se determinó si sus valores son credenciales utilizables, datos cifrados u otro formato; no se intentó descifrarlos. Al tratarse de una ruta sin token, conviene revisar qué campos deben exponerse antes de extender su acceso. Este hallazgo procede de la respuesta real y no modifica el alcance de la instalación de certificados.

### Certificado observado y límite de la comprobación

El cliente recibió:

| Campo | Valor |
|---|---|
| Subject | CN=api-dev.cafeamerica.com.ar, OU=GECO, O=Cafe America, C=AR |
| Issuer | CN=Kaspersky Anti-Virus Personal Root Certificate, O=AO Kaspersky Lab |
| SHA-256 | F2B0DA78FF33D32119DF07D7B189280E3C84A941008AB955AE49DB6E8BB4B542 |
| Thumbprint SHA-1 | 01ECA72B6DB6CE0A74966EDF8B6DE8B14524E468 |
| NotAfter UTC | 2027-03-25 23:46:40 |

El emisor observado evidencia sustitución del certificado durante la inspección HTTPS de Kaspersky. Windows aceptó la conexión, pero la huella no corresponde al certificado emitido por Cafe America GECO Root CA. Este resultado no acredita directamente la cadena original IIS→CA ni cambia el vencimiento del certificado instalado en IIS, que sigue registrado en 2226. El certificado de 2027 es el presentado por la inspección al cliente.

El primer intento desde el entorno aislado no accedió a la confianza del usuario y falló con un error del paquete de seguridad. La consulta posterior se ejecutó en Windows PowerShell 5.1 fuera de ese aislamiento, con autorización revisada, sin instalar confianza ni omitir validaciones TLS. El control de huella detectó la diferencia; se inspeccionaron emisor y respuesta para explicar el resultado. No se deshabilitó Kaspersky ni se configuraron excepciones.

Conclusión: la ruta funcional responde correctamente por HTTPS desde este equipo. Los enlaces y la importación IIS ya están confirmados por las consultas previas del usuario. La observación directa del certificado original se completó posteriormente mediante TLS local en el servidor, registrada en el paso 17.

### Repetir la consulta sin mostrar valores de registros

En Windows PowerShell del usuario conectado a la VPN:

```powershell
& {
    $ErrorActionPreference = 'Stop'
    $r = Invoke-WebRequest -Uri 'https://api-dev.cafeamerica.com.ar/gcservicios/api/administracion' -UseBasicParsing -TimeoutSec 30 -MaximumRedirection 0
    $j = $r.Content | ConvertFrom-Json
    [pscustomobject]@{
        StatusCode = [int]$r.StatusCode
        ContentType = $r.Headers['Content-Type']
        Registros = @($j.data).Count
        TotalCount = $j.meta.totalCount
    } | Format-List
}
```

Para repetir además la lectura del certificado, se conserva `Documentacion\Certificados\07-Verificar-Endpoint-HTTPS.ps1`. Ejecutarlo con Windows PowerShell 5.1; consulta la misma URL, compara la huella y muestra solo resumen y metadatos. Una advertencia de huella distinta no se interpreta como coincidencia: el campo CoincideCertificadoEmitido debe revisarse junto con Issuer. El script mantiene la validación TLS de Windows y no altera configuración. Si hay redirección no la sigue.

```powershell
& 'D:\Sis25\git\GeConnect\src\Documentacion\Certificados\07-Verificar-Endpoint-HTTPS.ps1'
```

Se comprobó sintaxis del componente final. La consulta real que dio origen a este componente ya fue ejecutada y sus resultados están registrados arriba; no se repitió el GET por el solo cambio de presentación del resumen.

## Paso 17 — Verificar el certificado original mediante TLS local en IIS (completado)

Ejecutar **dentro del servidor 172.10.10.12**, en Windows PowerShell 5.1. La prueba conecta a 127.0.0.1:443 y solicita el nombre api-dev.cafeamerica.com.ar mediante TLS/SNI. La dirección local solo sirve al diagnóstico: las aplicaciones conservan sus URL por nombre. No requiere modificar hosts o DNS del servidor.

Esta conexión local no pasa por el antivirus de la computadora cliente. Si existe inspección también en el servidor, la comparación de huella detectará un certificado sustituto. No se desactiva el antivirus ni se añaden excepciones. El script no cambia IIS, certificados, permisos ni contenido; tampoco llama a la API.

SslStream usa la validación normal de Windows, sin callbacks que acepten errores. Se comprueban confianza, nombre y vigencia, además de comparar el certificado recibido con la huella SHA-256 emitida. La revocación no se comprueba en esta prueba (último argumento false); la infraestructura CRL/OCSP de esta CA aún no está desplegada. Los protocolos se negocian según la configuración de Windows; no se fuerza un protocolo antiguo.

Pegar el siguiente bloque completo en PowerShell del servidor:

```powershell
& {
    $ErrorActionPreference = 'Stop'
    $nombre = 'api-dev.cafeamerica.com.ar'
    $esperada = 'CEA76F01B4590946FE818C0DAE2C73F9C739A445700705BE280138DC2B8F35A4'
    $tcp = [System.Net.Sockets.TcpClient]::new()
    $tls = $null
    $cert = $null
    $sha = $null
    try {
        $conexion = $tcp.ConnectAsync('127.0.0.1', 443)
        if (-not $conexion.Wait(10000)) { throw 'No se pudo conectar al puerto 443 local en 10 segundos.' }
        [void]$conexion.GetAwaiter().GetResult()

        $tls = [System.Net.Security.SslStream]::new($tcp.GetStream(), $false)
        $autenticacion = $tls.AuthenticateAsClientAsync(
            $nombre, $null, [System.Security.Authentication.SslProtocols]::None, $false)
        if (-not $autenticacion.Wait(15000)) { throw 'La comprobacion TLS excedio 15 segundos.' }
        [void]$autenticacion.GetAwaiter().GetResult()

        $cert = [System.Security.Cryptography.X509Certificates.X509Certificate2]::new($tls.RemoteCertificate)
        $sha = [System.Security.Cryptography.SHA256]::Create()
        $huella = ([BitConverter]::ToString($sha.ComputeHash($cert.RawData))).Replace('-', '')
        $coincide = ($huella -eq $esperada)
        [pscustomobject]@{
            NombreVerificado = $nombre
            Subject = $cert.Subject
            Issuer = $cert.Issuer
            VencimientoUTC = $cert.NotAfter.ToUniversalTime().ToString('yyyy-MM-dd HH:mm:ss')
            SHA256 = $huella
            CoincideCertificadoEmitido = $coincide
            ProtocoloTLS = $tls.SslProtocol
            ConexionCifrada = $tls.IsEncrypted
            IntegridadProtegida = $tls.IsSigned
        } | Format-List

        if (-not $coincide) { throw 'El certificado recibido no coincide con el emitido. Conservar la salida para revisar.' }
        if (-not ($tls.IsEncrypted -and $tls.IsSigned)) { throw 'La conexion no tiene la proteccion TLS esperada.' }
        Write-Host 'Certificado original de IIS verificado: nombre, confianza y huella correctos.'
    }
    finally {
        if ($sha) { $sha.Dispose() }
        if ($cert) { $cert.Dispose() }
        if ($tls) { $tls.Dispose() }
        $tcp.Dispose()
    }
}
``` 

Resultados esperados:

- Issuer: Cafe America GECO Root CA.
- VencimientoUTC: 2226-09-24 13:45:52.
- SHA256: CEA76F01B4590946FE818C0DAE2C73F9C739A445700705BE280138DC2B8F35A4.
- CoincideCertificadoEmitido, ConexionCifrada e IntegridadProtegida: True.
- Mensaje final: Certificado original de IIS verificado: nombre, confianza y huella correctos.

Compartir la salida completa. Si falla, conservar el mensaje y no cambiar confianza o bindings sin revisar el motivo. La llamada espera hasta 10 segundos por TCP y 15 segundos por TLS; los recursos se cierran al terminar.

El componente reproducible es `Documentacion\Certificados\08-Verificar-TLS-Local-IIS.ps1`. Si se copia al servidor, debe ejecutarse allí. No ejecutarlo desde la computadora cliente, ya que 127.0.0.1 siempre identifica el equipo donde se ejecuta el comando.

Estado: ejecución real confirmada por el usuario en el servidor. Huella, nombre, confianza y conexión TLS correctos, según resultado registrado a continuación.

## Resultado confirmado — Certificado original IIS y TLS local

El usuario ejecutó el paso 17 dentro del servidor y comunicó:

```text
NombreVerificado           : api-dev.cafeamerica.com.ar
Subject                    : CN=api-dev.cafeamerica.com.ar, OU=GECO, O=Cafe America, C=AR
Issuer                     : CN=Cafe America GECO Root CA, OU=GECO, O=Cafe America, C=AR
VencimientoUTC             : 2226-09-24 13:45:52
SHA256                     : CEA76F01B4590946FE818C0DAE2C73F9C739A445700705BE280138DC2B8F35A4
CoincideCertificadoEmitido : True
ProtocoloTLS               : Tls12
ConexionCifrada            : True
IntegridadProtegida        : True

Certificado original de IIS verificado: nombre, confianza y huella correctos.
```

La huella recibida coincide exactamente con el certificado emitido por la CA de Cafe America. La autenticación TLS finalizó usando la validación predeterminada de Windows, sin aceptar certificados inválidos mediante callbacks. Esta conexión negoció TLS 1.2 y confirmó cifrado e integridad; no implica que sea el único protocolo habilitado. No se comprobó revocación, conforme al alcance explícito del paso 17.

Las dos líneas System.Threading.Tasks.VoidTaskResult que aparecieron antes del resultado son valores internos devueltos al canal de salida de PowerShell; no son errores. Se ajustaron el bloque documentado y el componente 08 para descartar esos valores con [void], conservando la propagación de excepciones. No hace falta repetir la prueba por ese ajuste.

La primera implementación funcional del backend de desarrollo queda confirmada: CA y certificado emitidos, PFX importado, confianza instalada en las ubicaciones indicadas, enlace SNI correcto, GET de GCServicios con HTTP 200 y ocho registros, y certificado original verificado en IIS. El enlace anterior se conservó según el inventario confirmado.

El alcance es este servidor y el cliente ensayado. El respaldo local cifrado de CA/PFX/base de emisiones y su extracción fueron verificados en el paso 18. Continúan pendientes trasladarlo a otro equipo, registrar el respaldo previo IIS, la distribución de confianza y resolución de nombres a otros consumidores, y las pruebas de las demás aplicaciones. La causa exacta del 403 de la raíz no se investigó porque la ruta funcional respondió correctamente. Los campos de conexión observados en la API requieren una revisión separada antes de ampliar el acceso.

## Paso 18 — Crear un respaldo cifrado y verificar su extracción (completado; traslado externo pendiente)

El usuario indicó que no dispone ahora de una ubicación restringida y que copiará el respaldo a su computadora personal. El archivo cifrado se creó en `C:\Users\Juanjobe\GecoPKI-Respaldos` como ubicación provisional. La copia a otro equipo queda pendiente hasta que el usuario la realice y compruebe. Una copia en el mismo equipo no protege ante la pérdida de ese equipo.

Herramienta comprobada localmente: `C:\Program Files\WinRAR\Rar.exe`, versión 7.23 x64. El componente `09-Respaldar-PKI.ps1` utiliza el formato RAR5 con cifrado AES-256 de datos y nombres. La contraseña del respaldo se ingresa directamente en la consola de RAR; el script no la recibe como argumento, no la guarda ni la muestra. Elegir una contraseña nueva, larga y exclusiva y guardarla por separado en un gestor. Las contraseñas originales de la CA, la clave del backend y el PFX siguen siendo necesarias: el archivo conserva su cifrado original dentro del RAR.

### Contenido

- Toda la carpeta GecoPKI: clave privada cifrada de la CA, certificados públicos, base ca-db con índices/series/newcerts y archivos anteriores, claves cifradas de servidores, CSR, configuraciones, certificados y PFX.
- Bitácora SEG-006 y componentes públicos de Documentacion\Certificados.
- Manifiesto SHA-256 de cada archivo, incluido dentro del archivo cifrado.

Se excluye ca-emision.lock, que es un archivo de coordinación sin datos de recuperación. No se incluyen contraseñas, aplicaciones, bases de datos ni configuración de IIS del servidor. El respaldo previo IIS debe registrarse y conservarse por separado; su nombre todavía no se comunicó.

### Procedimiento

En la computadora donde está `C:\Users\Juanjobe\GecoPKI` (fuera del Escritorio remoto), cerrar otras tareas que emitan certificados o modifiquen esa carpeta. Ejecutar:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "D:\Sis25\git\GeConnect\src\Documentacion\Certificados\09-Respaldar-PKI.ps1"
```

Bypass se limita al proceso que ejecuta este componente; no cambia la política de ejecución persistente del equipo.

RAR solicitará una nueva contraseña del respaldo y su confirmación al crear el archivo en consola interactiva. Más adelante solicitará la misma contraseña para extraer la copia de prueba. No escribir estas contraseñas en el chat. No se necesitan las contraseñas de las claves privadas o del PFX para copiarlos: se conservan cifrados.

El componente realiza estas verificaciones:

1. Comprueba archivos mínimos, base de emisiones y cabeceras de claves PEM cifradas. Rechaza rutas con enlaces/puntos de reanálisis y destinos dentro de la PKI o del proyecto.
2. Crea una carpeta temporal accesible por la cuenta ejecutora, SYSTEM y administradores. Mantiene el bloqueo utilizado por el emisor 04 mientras copia el material y comprueba hashes; no ejecutar otros cambios de PKI durante esta operación.
3. Copia y compara cada archivo con SHA-256, comprobando que el origen no haya cambiado. El original no se elimina ni se reemplaza.
4. Crea un archivo con extensión .pendiente.rar y comprueba que sea RAR5 con cabeceras cifradas.
5. Extrae el archivo a otra carpeta temporal protegida y verifica el manifiesto, el número de archivos y todos los hashes. Solo después publica el nombre final .rar.
6. Guarda el hash del archivo RAR y un resumen en un archivo .rar.verificacion.json junto a él. Este resumen no contiene claves ni contraseñas.
7. Elimina únicamente las carpetas temporales de esa ejecución después de verificar sus rutas. Si hay un fallo, se detiene; un .pendiente.rar no acredita respaldo completo y no debe usarse como tal.

Resultado esperado:

```text
Archivo              : C:\Users\Juanjobe\GecoPKI-Respaldos\GecoPKI-<fecha>-<identificador>.rar
SHA256               : <hash del archivo cifrado>
ArchivosVerificados  : <cantidad real>
Formato              : RAR5 AES-256, datos y nombres cifrados
ExtraccionVerificada : True
Respaldo cifrado creado y extraido para verificacion. Todos los archivos coinciden con el origen.
```

Compartir solo la ruta, SHA256, ArchivosVerificados y ExtraccionVerificada. No compartir el archivo ni sus contraseñas en el chat.

Esta comprobación demuestra la recuperación exacta de los archivos desde el RAR y que la contraseña del respaldo funciona. No prueba el descifrado de cada clave PEM o del PFX con sus contraseñas originales, ni una restauración completa de IIS; esas comprobaciones son distintas.

### Trasladar a la computadora personal

Copiar el archivo .rar final y su .rar.verificacion.json. En el destino, calcular:

```powershell
Get-FileHash -LiteralPath 'RUTA_COMPLETA_DEL_ARCHIVO_COPIADO.rar' -Algorithm SHA256
```

Reemplazar la ruta por la real y comparar con el SHA256 registrado al crear el respaldo. La coincidencia verifica integridad de la copia; guardar el hash registrado por separado permite detectar una sustitución del archivo y su resumen. Mantener la contraseña separada del RAR.

Para recuperar en el futuro, extraer primero en una carpeta vacía protegida, comprobar los hashes del manifiesto y revisar la documentación. No superponer automáticamente esa copia sobre una CA que siguió emitiendo certificados: una base index/serial antigua puede perder emisiones posteriores. Este primer respaldo representa solo el estado de la fecha de creación; hay que repetirlo tras nuevas emisiones o cambios de la CA.

### Validación del componente

Ensayo completo con 13 archivos ficticios y contraseña de prueba: creación RAR5 con cabeceras cifradas, extracción y comparación SHA-256 correctas. Se comprobó que un fallo de extracción impide publicar el nombre final. No se copiaron claves reales durante las pruebas. Se verificó la sintaxis en Windows PowerShell 5.1 y se ajustó la carga del módulo de utilidades para evitar heredar una ruta de módulos incompatible desde PowerShell 7.

Estado operativo: **respaldo real creado y extracción verificada por el usuario: 27 archivos coincidentes**. Codex contrastó posteriormente el hash del RAR y el resumen de verificación. Traslado a la computadora personal y verificación del hash en destino pendientes.

## Incidencia y corrección — Inicio del respaldo

El usuario ejecutó el comando del paso 18 desde C:\ y recibió en el parámetro CarpetaDocumentacion:

```text
Split-Path : No se puede enlazar el argumento con el parámetro 'Path' porque es una cadena vacía.
```

La expresión predeterminada evaluaba Split-Path sobre PSScriptRoot al enlazar parámetros, cuando ese valor estaba vacío en esta ejecución. El fallo ocurre antes del cuerpo del script, de la copia de archivos y de la solicitud de contraseña; ese intento no creó el respaldo.

Corrección del componente 09: el parámetro CarpetaDocumentacion deja de evaluar esa expresión. Cuando se omite, se resuelve dentro del cuerpo usando PSCommandPath, con alternativa MyInvocation.MyCommand.Path, y se comprueba que la ruta esté disponible antes de derivar la carpeta. Una ruta proporcionada explícitamente conserva su comportamiento.

El ensayo anterior indicaba CarpetaDocumentacion explícitamente, por lo que no cubría el caso que falló. La prueba de regresión ejecutó la versión corregida con powershell.exe -NoProfile -ExecutionPolicy Bypass -File, desde C:\ y sin pasar CarpetaDocumentacion, usando exclusivamente PKI ficticia y un destino temporal. Resultado: código 0, RAR cifrado creado y 13 archivos extraídos y comparados correctamente. Se eliminaron los archivos de ensayo; no se usó material operativo.

El usuario reintentó el mismo comando del paso 18 con el componente actualizado y completó el respaldo. No fue necesario cambiar certificados, contraseñas anteriores ni rutas manualmente. El resultado se registra a continuación; no hace falta repetir la creación.

## Resultado confirmado — Respaldo cifrado de PKI

Resultado comunicado por el usuario y contrastado con el archivo y su resumen local por Codex:

| Campo | Resultado |
|---|---|
| Archivo | C:\Users\Juanjobe\GecoPKI-Respaldos\GecoPKI-20260924-214137-c9658025.rar |
| SHA-256 | 573919D3F120A42A54F3B0762D759F04463A0EC450C7F1CB76AEB13624B9C709 |
| Tamaño del RAR | 66974 bytes |
| Archivos verificados | 27 |
| Formato | RAR5 AES-256; datos y nombres cifrados |
| Extracción verificada | True |
| Fecha UTC | 2026-09-25 00:42:26 |
| Hora Argentina (UTC-3) | 2026-09-24 21:42:26 |

El archivo acompañante es `GecoPKI-20260924-214137-c9658025.rar.verificacion.json`. El usuario confirmó que la extracción y comparación con los originales finalizaron correctamente. Codex calculó nuevamente SHA-256 sobre el RAR sin abrirlo ni solicitar la contraseña; coincide exactamente. La comprobación de hash posterior no constituye una segunda extracción.

El RAR conserva la instantánea de los archivos y la documentación al momento de su creación (bitácora v0.20); las anotaciones de confirmación posteriores están en esta bitácora actualizada. No se modifica el archivo ya verificado para incorporarlas. No hace falta regenerarlo por esta actualización documental.

## Paso 19 — Copiar y comprobar el respaldo en la computadora personal (próximo paso)

Copiar estos dos archivos desde la carpeta GecoPKI-Respaldos:

- GecoPKI-20260924-214137-c9658025.rar
- GecoPKI-20260924-214137-c9658025.rar.verificacion.json

Mantener la contraseña del RAR por separado. Después, en PowerShell de la computadora personal, ejecutar:

```powershell
& {
    $ErrorActionPreference = 'Stop'
    $archivo = (Read-Host 'Ruta completa del RAR copiado').Trim().Trim('"')
    $esperada = '573919D3F120A42A54F3B0762D759F04463A0EC450C7F1CB76AEB13624B9C709'
    $hash = Get-FileHash -LiteralPath $archivo -Algorithm SHA256
    $hash | Format-List Path, Hash
    if ($hash.Hash -ne $esperada) { throw 'La copia no coincide con el respaldo original.' }
    Write-Host 'Copia externa verificada: SHA-256 coincide con el respaldo original.'
}
```

La huella esperada está fijada al valor verificado en origen, no se obtiene del archivo JSON acompañante. Compartir el resultado, sin incluir contraseñas. Hasta recibirlo, la copia externa sigue pendiente y no se considera comprobada. Este paso no requiere extraer nuevamente la CA ni instalar certificados en la computadora personal.

## Resultado confirmado — Confianza desde el servidor 172.10.10.11

El usuario informó que instaló cafeamerica-ca.cer en Raíces de confianza del .11 y agregó en hosts:

```text
172.10.10.12 api-dev.cafeamerica.com.ar
```

Probó `https://api-dev.cafeamerica.com.ar/gcservicios/api/administracion` y confirmó respuesta correcta con confianza. El alcance exacto del almacén (CurrentUser o LocalMachine) no fue informado; una prueba desde la sesión del usuario no acredita por sí sola todas las cuentas de servicio. Para aplicaciones que usan el almacén Windows del equipo, la confianza se instala en LocalMachine\Root. Plataformas con almacén propio requieren su configuración correspondiente.

El usuario eligió `app-dev.cafeamerica.com.ar` como nombre HTTPS del IIS alojado en 172.10.10.11. Esto requiere un certificado propio de servidor, firmado por la CA existente. La confianza para consumir api-dev y la identidad de app-dev para recibir conexiones son funciones distintas.

## Paso 20 — Emitir el PFX de un servidor nuevo con un único comando

La CA existente se reutiliza. No regenerar su clave ni crear otra CA en cada servidor. Cada nombre HTTPS nuevo tiene su certificado y clave propia; las aplicaciones que comparten ese nombre por distintas rutas usan el mismo certificado. El componente actual emite para un nombre DNS exacto, sin wildcard ni SAN adicionales.

Ejecutar en la computadora de administración donde reside GecoPKI, no en cada servidor destino:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "D:\Sis25\git\GeConnect\src\Documentacion\Certificados\10-Crear-PFX-Servidor.ps1" -NombreDNS "app-dev.cafeamerica.com.ar"
```

El componente 10 encadena los componentes 03, 04 y 05: clave RSA3072 cifrada y CSR, emisión registrada en ca-db, y PFX AES256-SHA256 con 100000 iteraciones. Comprueba la identidad de la CA por SHA-256 y conserva los controles existentes de SAN, cadena, uso TLS, fechas, correspondencia de clave pública y protección del PFX.

La vigencia solicitada predeterminada es 200 años y se limita a la vigencia restante de la CA. Con nuestra raíz, el límite es **2226-09-24 13:45:52 UTC**. Emitir más adelante no agrega otros 200 años después del vencimiento de la CA. El parámetro -Anios permite pedir menos años si se necesita.

Durante la ejecución OpenSSL solicita:

1. Nueva contraseña para la clave privada de app-dev y su confirmación.
2. Contraseña existente de la CA para firmar.
3. Contraseña de la clave de app-dev para exportar.
4. Nueva contraseña de exportación del PFX y su confirmación; después se repite para verificarlo.

No enviar contraseñas al chat. Se mantiene la interacción local de OpenSSL; el comando único simplifica los pasos, pero no guarda contraseñas ni las convierte en argumentos del proceso.

Salida esperada:

```text
C:\Users\Juanjobe\GecoPKI\servidores\app-dev.cafeamerica.com.ar\servidor.iis.pfx
```

El resumen final muestra NombreDNS, PFX, SHA256PFX, CertificadoServidor y CertificadoPublicoCA. Compartir ese resumen para registrar la emisión. La clave privada de la CA nunca se incluye en el PFX del servidor.

Para el siguiente servidor, usar el mismo comando sustituyendo el valor de -NombreDNS por su nombre real. La IP no es un parámetro de emisión: se configura en DNS o hosts y no se incorpora al SAN de este procedimiento.

El script exige una CA y una base existentes completas. Rechaza una carpeta de servidor que ya exista, incluso si quedó incompleta; no sobrescribe claves ni reemite silenciosamente. Si falla una etapa, conservar sus archivos y el error para identificar cuál de los componentes 03/04/05 debe retomarse. No borrar certificados ni la base para volver a empezar.

Se puede añadir -WhatIf al comando para comprobar precondiciones sin crear clave, CSR, certificado ni PFX. Los parámetros avanzados -CarpetaPKI y -OpenSSLPath permiten indicar rutas distintas; -HuellaCA es la huella pública esperada y solo debe cambiarse deliberadamente para otra autoridad. El valor predeterminado fija nuestra CA de Cafe America.

### Configuración resumida en cada IIS

| Elemento | Para app-dev / 172.10.10.11 |
|---|---|
| Resolución del nombre en consumidores | app-dev.cafeamerica.com.ar → 172.10.10.11 |
| Confianza | cafeamerica-ca.cer en el almacén que utiliza el consumidor; no reinstalarlo si ya está correctamente confiado |
| Identidad HTTPS del .11 | Importar su servidor.iis.pfx en Cert:\LocalMachine\My con clave privada |
| Enlace IIS | En el sitio correcto, HTTPS puerto 443, nombre app-dev.cafeamerica.com.ar, SNI y certificado de app-dev |
| Prueba | Abrir una ruta válida de la aplicación y comprobar el certificado servido |

En hosts, la nueva entrada convive con la de api-dev:

```text
172.10.10.12 api-dev.cafeamerica.com.ar
172.10.10.11 app-dev.cafeamerica.com.ar
```

La entrada debe existir en cada consumidor que no tenga DNS central para ese nombre. Para producción interna y sucursales, un DNS administrado y la distribución central de la CA evitan repetir hosts y confianza manualmente. Si los consumidores públicos no están administrados por la organización, utilizar un certificado de una CA públicamente reconocida para ese acceso, según el diseño ya acordado.

No se ha inspeccionado el sitio ni los enlaces del .11: no asumir que se llama SitiosGC, ni reemplazar enlaces existentes. La importación y el enlace se configuran allí después de obtener el PFX, con inventario y respaldo de su configuración. Este comando no instala certificados ni modifica IIS remoto.

### Respaldo y verificación

Cada emisión nueva modifica ca-db y agrega material. Después de emitir app-dev, ejecutar nuevamente el componente 09 y trasladar el nuevo respaldo. El RAR anterior conserva correctamente el estado anterior, pero no contiene emisiones posteriores. La copia externa del primer RAR continúa sin confirmación.

Ensayo realizado con una CA descartable de dos días y el nombre ensayo.example.test: WhatIf sin cambios, ejecución completa de 03/04/05, validación del PFX, límite de vigencia exactamente igual al de la CA y rechazo de un reintento sin modificar el índice. No se usaron claves de la CA operativa ni se instalaron certificados. Las claves de ensayo se eliminaron. Componente 05 ajustado solo en los textos para decir «este servidor» en lugar de «backend».

Además del ensayo completo, se verificaron las precondiciones contra la CA operativa con -WhatIf para app-dev, sin crear material. El cálculo SHA-256 del componente 10 usa lectura .NET para no quedar afectado por preferencias WhatIf de cmdlets internos. Estado: comando preparado y probado; emisión real para app-dev y configuración IIS del .11 pendientes de ejecución y confirmación del usuario.

## Replicar sin perder la identidad de la CA

- Otro servidor de la misma organización: reutilizar la CA; generar una clave propia del servidor y emitir para su nombre. No crear una CA por servidor.
- Otra aplicación bajo `api-dev.cafeamerica.com.ar`: utiliza el certificado de ese sitio IIS; no requiere otra emisión por ruta.
- Otra CA deliberadamente independiente: otra carpeta y otra identidad en el `.cnf`, con clave y confianza independientes.
- `-CarpetaCA` permite indicar otra ubicación segura de administración; `-OpenSSLPath`, otra instalación de OpenSSL. La clave de la CA no se distribuye a todos los servidores.
- Repetir verificaciones no equivale a regenerar claves o certificados. La raíz, el certificado del backend y el PFX ya fueron generados; la confianza del usuario local también está instalada; la raíz y el PFX ya están importados en el servidor; el enlace IIS ya está confirmado; se recibió HTTP 403 en la raíz por HTTPS; la prueba funcional de GCServicios fue exitosa; el certificado original de IIS quedó confirmado mediante la prueba local del paso 17.

## Cierre de la primera implementación y pendientes operativos

1. Raíz emitida y registrada: conservarla; no volver a ejecutar su script de creación.
2. Clave, CSR y certificado del backend completados: SAN `DNS:api-dev.cafeamerica.com.ar`, `CA:FALSE`, autenticación de servidor y vencimiento confirmado el 24/09/2226.
3. PFX protegido creado y verificado: conservarlo junto con su contraseña de exportación.
4. Copia e importación en el servidor completadas: raíz en LocalMachine\Root y PFX en LocalMachine\My. La confianza de los demás consumidores se configurará después.
5. Enlace SNI agregado y ambos enlaces verificados. Registrar el nombre del respaldo previo pendiente de informar. Cinco aplicaciones inventariadas; GCServicios probado. Certificado original IIS confirmado mediante prueba local del servidor.
6. GET de GCServicios confirmado sin desactivar validación TLS. El cliente recibió un certificado de Kaspersky; el original IIS fue confirmado desde el servidor. Probar las demás API según necesidad.
7. Registrar respaldo, inventario, renovación, revocación, DNS central y emisión pública antes de extender el despliegue.

## Fuentes

- [Microsoft: enlaces IIS, nombres y SNI](https://learn.microsoft.com/en-us/iis/configuration/system.applicationhost/sites/site/bindings/binding).

- [RARLAB: formato RAR5 y cifrado AES-256](https://www.rarlab.com/technote.htm).
- Manual local de RAR 7.23: C:\Program Files\WinRAR\Rar.txt (opciones -hp, -p y -cfg-).
- [Microsoft: creación de carpetas con permisos Windows](https://learn.microsoft.com/en-us/dotnet/api/system.io.directory.createdirectory?view=netframework-4.8.1).

- [Microsoft: SslStream y autenticación TLS](https://learn.microsoft.com/en-us/dotnet/api/system.net.security.sslstream?view=netframework-4.8.1).
- [Microsoft: AuthenticateAsClientAsync](https://learn.microsoft.com/en-us/dotnet/api/system.net.security.sslstream.authenticateasclientasync?view=netframework-4.8.1).

- [Kaspersky: inspección de conexiones cifradas](https://support.kaspersky.com/KESWin/11.4.0/en-US/175451.htm).
- [Kaspersky: certificados sustitutos durante inspección TLS](https://support.kaspersky.com/KWTS/6.1/en-US/189213.htm).

- [Microsoft: estados y subestados HTTP de IIS](https://learn.microsoft.com/en-us/troubleshoot/developer/webapps/iis/health-diagnostic-performance/http-status-code).
- [Microsoft: Get-WebApplication](https://learn.microsoft.com/es-es/powershell/module/webadministration/get-webapplication?view=windowsserver2025-ps).
- [Microsoft: Get-WebVirtualDirectory](https://learn.microsoft.com/en-us/powershell/module/webadministration/get-webvirtualdirectory?view=windowsserver2025-ps).

- [Microsoft: Invoke-WebRequest en Windows PowerShell 5.1](https://learn.microsoft.com/en-us/powershell/module/Microsoft.PowerShell.Utility/Invoke-WebRequest?view=powershell-5.1).

- [Microsoft: Backup-WebConfiguration](https://learn.microsoft.com/en-us/powershell/module/webadministration/backup-webconfiguration?view=windowsserver2025-ps).
- [Microsoft: New-WebBinding y opciones SSL](https://learn.microsoft.com/en-us/powershell/module/webadministration/new-webbinding?view=windowsserver2025-ps).
- [Microsoft: configuración de SNI en IIS](https://learn.microsoft.com/en-us/iis/get-started/whats-new-in-iis-8/iis-80-server-name-indication-sni-ssl-scalability).


- [Microsoft: Import-PfxCertificate](https://learn.microsoft.com/en-us/powershell/module/pki/import-pfxcertificate?view=windowsserver2025-ps).
- [Microsoft: Get-PfxData](https://learn.microsoft.com/en-us/powershell/module/pki/get-pfxdata?view=windowsserver2025-ps).

- [Microsoft: Get-Website](https://learn.microsoft.com/en-us/powershell/module/webadministration/get-website?view=windowsserver2025-ps).
- [Microsoft: Get-WebBinding](https://learn.microsoft.com/en-us/powershell/module/webadministration/get-webbinding?view=windowsserver2025-ps).

- [Microsoft: Import-Certificate y almacenes de usuario/equipo](https://learn.microsoft.com/en-us/powershell/module/pki/import-certificate?view=windowsserver2025-ps).

- [OpenSSL 3.6: administración de CA](https://docs.openssl.org/3.6/man1/openssl-ca/).
- [OpenSSL 3.6: PKCS12/PFX](https://docs.openssl.org/3.6/man1/openssl-pkcs12/).
- [OpenSSL 3.6: certificados X.509](https://docs.openssl.org/3.6/man1/openssl-x509/).
- [Microsoft: compatibilidad de PFX AES256-SHA256](https://learn.microsoft.com/en-us/troubleshoot/windows-server/certificates-and-public-key-infrastructure-pki/cannot-import-aes256-sha256-encrypted-pfx-certificate).

- [OpenSSL 3.6: generación de claves](https://docs.openssl.org/3.6/man1/openssl-genpkey/).
- [OpenSSL 3.6: req y fechas explícitas](https://docs.openssl.org/3.6/man1/openssl-req/).
- [OpenSSL 3.6: extensiones X.509](https://docs.openssl.org/3.6/man5/x509v3_config/).

## Historial

- 24/09/2026, v0.1: documentados el diagnóstico, hosts y `Key is valid`; propuesta inicial de raíz de 3650 días todavía sin emisión confirmada.
- 24/09/2026, v0.2: ubicación corregida a `D:\Sis25\git\GeConnect\src`; vigencia de raíz cambiada a 200 años calendario por pedido del usuario; script y verificaciones regenerados. Se conserva la clave existente.

- 24/09/2026, v0.3: registrado el bloqueo previo a ejecutar; incorporada consulta de politicas y habilitacion temporal RemoteSigned, pendiente de confirmacion del usuario.

- 24/09/2026, v0.4: registrada la raiz real emitida (serie, huella, fechas y verificaciones); preparado el paso de clave y CSR del backend, pendiente de ejecucion del usuario.

Validacion del componente 03 (24/09/2026): ejecucion completa con dominio de prueba ensayo.example.test y clave descartable; cifrado PEM, firma de CSR, SAN y guarda contra sobrescritura correctos. La clave de prueba fue eliminada. No se uso material de la CA real. Posteriormente el usuario confirmo la ejecucion para api-dev, registrada en la version 0.5.

- 24/09/2026, v0.5: clave y CSR del backend confirmadas; Windows Server 2019 o posterior comunicado por el usuario; preparados y probados los scripts de emision (365 dias) y PFX AES256-SHA256. Emision e instalacion reales pendientes.

- 24/09/2026, v0.6: por pedido del usuario y requisito comunicado de su jefe, vigencia del backend cambiada de 365 días a 200 años solicitados, con límite en la vigencia real de la raíz (2226-09-24 13:45:52 UTC). Script 04 actualizado; script 05 y claves existentes conservados. La emisión real sigue pendiente.

Validación v0.6: sintaxis y ejecución con CA y claves descartables correctas. Se comprobó un pedido de 200 años limitado exactamente por la raíz, y un pedido de un año calendario que no necesitó limitarse. Firma, validación TLS, fechas leídas por Windows y registro de ambas emisiones correctos. Claves de prueba eliminadas; no se emitieron certificados operativos.

- 24/09/2026, v0.7: emision real del backend confirmada por el usuario; registrados serie, huella, SAN, usos, fechas y alta en la base de la CA. Exportacion del PFX, confianza e IIS pendientes.

- 24/09/2026, v0.8: PFX real confirmado por el usuario y parametros de proteccion registrados. Preparada la instalacion de confianza para el usuario Windows con huella SHA-256 fijada; comprobacion WhatIf sin modificar almacenes. Confianza e IIS pendientes.

- 24/09/2026, v0.9: confianza de la CA confirmada en CurrentUser\Root, con huella y vencimiento verificados; agregado relevamiento de sitios y enlaces IIS en el backend. Configuracion remota pendiente.

- 24/09/2026, v0.10: registrados sitios y enlaces actuales; seleccionado SitiosGC para el nuevo host mediante SNI conservando el enlace existente. Registrados hashes de origen y procedimiento de copia al servidor; copia e importacion pendientes.

- 24/09/2026, v0.11: copia de CER y PFX al servidor confirmada con hashes coincidentes; documentada importacion en almacenes LocalMachine y verificacion de clave privada. Se mantiene pendiente la ejecucion remota y el enlace SNI.

- 24/09/2026, v0.12: importacion de raiz y PFX confirmada en el servidor; clave privada asociada y vencimiento local coherente con UTC. Preparados respaldo IIS, alta manual del enlace SNI y verificacion de preservacion del enlace anterior. Enlace y prueba HTTPS pendientes.

- 24/09/2026, v0.13: enlace HTTPS con SNI confirmado por el usuario y enlace anterior conservado. Nombre y resultado del respaldo previo pendientes de informar. Documentadas prueba HTTPS desde cliente VPN y consultas de diagnostico; ejecucion pendiente.

- 24/09/2026, v0.14: registrado HTTP 403 en la raiz desde navegador y PowerShell, sin error TLS informado. Causa y subestado sin determinar. Preparada consulta de aplicaciones y directorios virtuales de SitiosGC para elegir una ruta funcional. Identidad del certificado servido y respaldo previo pendientes de informar.

- 24/09/2026, v0.15: registradas cinco aplicaciones de SitiosGC, carpetas, pools y direcciones base. Preparada prueba de ApiClover y confirmacion del certificado presentado. Endpoint funcional y resultados pendientes; no se modifico IIS.

- 24/09/2026, v0.16: GET autorizado a GCServicios ejecutado por Codex: HTTP 200, JSON valido, ocho registros y sin redireccion. Certificado recibido emitido por Kaspersky, distinto del original IIS; verificacion directa de este ultimo pendiente. Agregado comprobador 07 con salida resumida y anotado hallazgo de campos de conexion sin copiar sus valores.

- 24/09/2026, v0.17: preparado paso 17 y componente 08 para verificar TLS/SNI por loopback dentro del servidor, con validacion Windows y comparacion SHA-256. Sintaxis comprobada, ejecucion pendiente. No se modificaron IIS ni antivirus.

- 24/09/2026, v0.18: verificacion TLS local confirmada por el usuario: certificado original con huella SHA-256 exacta, emisor Cafe America, vencimiento 2226 y TLS 1.2 cifrado con integridad. Primera implementacion funcional confirmada. Componente 08 ajustado para suprimir VoidTaskResult; no requiere repetir la prueba. Respaldos y extension a otros consumidores pendientes.

- 24/09/2026, v0.19: inventariado material PKI y WinRAR disponible. Por indicacion del usuario, respaldo provisional local para traslado posterior a computadora personal. Preparado y ensayado componente 09 para RAR cifrado y extraccion verificada por SHA-256. Respaldo real pendiente de contraseña local y ejecucion; copia externa pendiente.

- 24/09/2026, v0.20: corregido fallo de inicio del componente 09 al resolver CarpetaDocumentacion dentro del cuerpo del script. Regresion desde C:\ con Windows PowerShell 5.1 y parametro omitido: 13 archivos ficticios respaldados y recuperados correctamente. Ejecucion real pendiente de reintento del usuario.

- 24/09/2026, v0.21: respaldo RAR real confirmado: 27 archivos extraidos y comparados, AES-256 con nombres cifrados. Codex verifico nuevamente el hash del archivo y su resumen. Registrados nombre, tamaño y fecha; preparado paso 19 para cotejar la copia externa con la huella exacta. Traslado a computadora personal pendiente.

- 24/09/2026, v0.22: confianza y consumo HTTPS desde .11 confirmados por el usuario. Nombre app-dev.cafeamerica.com.ar elegido para IIS del .11. Agregado componente 10 para crear PFX con un comando reutilizando CA; ensayo completo con CA descartable y controles de WhatIf/reintento correctos. Emision real app-dev pendiente; recordatorio de nuevo respaldo tras emitir.
