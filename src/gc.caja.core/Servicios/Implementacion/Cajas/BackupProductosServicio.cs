using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Cajas.Request;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.caja.core.Servicios.Implementacion.Cajas
{
    /// <summary>
    /// ✅ ACTUALIZADO v2.1: Servicio de backup de productos en archivo plano
    /// Usa Newtonsoft.Json para serialización/deserialización
    /// Usa la ruta configurada en AppSettings.RutaFileCaja
    /// Permite recuperar productos en caso de cortes de comunicación
    /// </summary>
    public class BackupProductosServicio : IBackupProductosServicio
    {
        private readonly ILogger<BackupProductosServicio> _logger;
        private readonly string _backupBasePath;
        private const string BACKUP_FOLDER = "BackupProductos";
        private const string BACKUP_EXTENSION = ".json";

        // ✅ Configuración de serialización Newtonsoft.Json
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        // ✅ AGREGAR DESPUÉS DEL MÉTODO InicializarBackup

        /// <summary>
        /// ✅ NUEVO v2.2: Reinicia el backup eliminando archivos previos y creando uno nuevo
        /// Se invoca cuando se carga el primer producto (item = 1) de una nueva sesión
        /// CRÍTICO: Garantiza que cada sesión tenga su propio backup limpio
        /// </summary>
        public async Task<bool> ReiniciarBackup(string cajaId, string usuarioId)
        {
            try
            {
                _logger.LogInformation("═══════════════════════════════════════════════════");
                _logger.LogInformation("🔄 REINICIANDO BACKUP DE PRODUCTOS v2.2");
                _logger.LogInformation("   ⚠️ NUEVA SESIÓN DETECTADA (item = 1)");
                _logger.LogInformation($"   Caja: {cajaId}");
                _logger.LogInformation($"   Usuario: {usuarioId}");
                _logger.LogInformation("═══════════════════════════════════════════════════");

                // ❶ PASO 1: Eliminar TODOS los backups anteriores de esta caja/usuario
                _logger.LogInformation("📝 PASO 1: Eliminando backups previos...");

                bool backupsEliminados = await LimpiarBackup(cajaId, usuarioId);

                if (backupsEliminados)
                {
                    _logger.LogInformation("   ✅ Backups previos eliminados correctamente");
                }
                else
                {
                    _logger.LogInformation("   ℹ️ No había backups previos (primera sesión o ya limpiados)");
                }

                // ❷ PASO 2: Crear un nuevo archivo de backup limpio
                _logger.LogInformation("📝 PASO 2: Creando nuevo archivo de backup...");

                bool backupCreado = await InicializarBackup(cajaId, usuarioId);

                if (!backupCreado)
                {
                    _logger.LogError("   ❌ Error al crear nuevo archivo de backup");
                    return false;
                }

                _logger.LogInformation("   ✅ Nuevo archivo de backup creado");

                // ❸ RESULTADO
                _logger.LogInformation("═══════════════════════════════════════════════════");
                _logger.LogInformation("✅ BACKUP REINICIADO EXITOSAMENTE");
                _logger.LogInformation("   → Archivos previos eliminados");
                _logger.LogInformation("   → Nuevo archivo creado y listo para usar");
                _logger.LogInformation("═══════════════════════════════════════════════════");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al reiniciar backup");
                return false;
            }
        }

        /// <summary>
        /// ✅ ACTUALIZADO v2.1: Constructor con inyección de AppSettings
        /// </summary>
        public BackupProductosServicio(
            ILogger<BackupProductosServicio> logger,
            IOptions<AppSettings> appSettings)
        {
            _logger = logger;

            // ✅ CRÍTICO: Usar la ruta configurada en AppSettings.RutaFileCaja
            string rutaFileCaja = appSettings.Value.RutaFileCaja;

            if (string.IsNullOrWhiteSpace(rutaFileCaja))
            {
                _logger.LogError("═══════════════════════════════════════════════════");
                _logger.LogError("❌ ERROR CRÍTICO: RutaFileCaja no está configurada en appsettings.json");
                _logger.LogError("═══════════════════════════════════════════════════");
                throw new InvalidOperationException(
                    "La ruta de configuración de caja (RutaFileCaja) no está definida en appsettings.json. " +
                    "Por favor, configure 'AppSettings:RutaFileCaja' con la ruta del directorio de configuración."
                );
            }

            // ✅ Obtener directorio base desde la ruta del archivo cajasettings.json
            string directorioBase = Path.GetDirectoryName(rutaFileCaja) ?? string.Empty;

            if (string.IsNullOrWhiteSpace(directorioBase))
            {
                _logger.LogError("═══════════════════════════════════════════════════");
                _logger.LogError($"❌ ERROR: No se pudo obtener directorio desde RutaFileCaja: {rutaFileCaja}");
                _logger.LogError("═══════════════════════════════════════════════════");
                throw new InvalidOperationException(
                    $"No se pudo determinar el directorio base desde RutaFileCaja: {rutaFileCaja}"
                );
            }

            // ✅ Crear ruta de backup: mismo directorio que cajasettings.json + subcarpeta BackupProductos
            _backupBasePath = Path.Combine(directorioBase, BACKUP_FOLDER);

            _logger.LogInformation("═══════════════════════════════════════════════════");
            _logger.LogInformation("📂 INICIALIZANDO SERVICIO DE BACKUP v2.1");
            _logger.LogInformation("   ✅ Usando Newtonsoft.Json para serialización");
            _logger.LogInformation($"   RutaFileCaja configurada: {rutaFileCaja}");
            _logger.LogInformation($"   Directorio base: {directorioBase}");
            _logger.LogInformation($"   Ruta de backup: {_backupBasePath}");
            _logger.LogInformation("═══════════════════════════════════════════════════");

            // ✅ Crear directorio de backup si no existe
            try
            {
                if (!Directory.Exists(_backupBasePath))
                {
                    Directory.CreateDirectory(_backupBasePath);
                    _logger.LogInformation($"✅ Directorio de backup creado: {_backupBasePath}");
                }
                else
                {
                    _logger.LogInformation($"✅ Directorio de backup existente: {_backupBasePath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ ERROR al crear directorio de backup: {_backupBasePath}");
                throw new InvalidOperationException(
                    $"No se pudo crear el directorio de backup: {_backupBasePath}. " +
                    $"Verifique los permisos del sistema.", ex
                );
            }
        }

        /// <summary>
        /// Genera el nombre del archivo de backup según caja y usuario
        /// Formato: backup_CAJA001_USUARIO_20260515_143022.json
        /// </summary>
        private string ObtenerRutaArchivo(string cajaId, string usuarioId)
        {
            // ✅ Normalizar identificadores
            string cajaNormalizada = cajaId.Replace(" ", "_").Replace("\\", "_").Replace("/", "_");
            string usuarioNormalizado = usuarioId.Replace(" ", "_").Replace("\\", "_").Replace("/", "_");

            // ✅ Agregar timestamp para identificación única por sesión
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            string nombreArchivo = $"backup_{cajaNormalizada}_{usuarioNormalizado}_{timestamp}{BACKUP_EXTENSION}";
            return Path.Combine(_backupBasePath, nombreArchivo);
        }

        /// <summary>
        /// ✅ ACTUALIZADO v2.1: Busca el archivo de backup más reciente para la caja/usuario
        /// </summary>
        private string? ObtenerRutaArchivoMasReciente(string cajaId, string usuarioId)
        {
            try
            {
                string cajaNormalizada = cajaId.Replace(" ", "_").Replace("\\", "_").Replace("/", "_");
                string usuarioNormalizado = usuarioId.Replace(" ", "_").Replace("\\", "_").Replace("/", "_");

                string patron = $"backup_{cajaNormalizada}_{usuarioNormalizado}_*{BACKUP_EXTENSION}";

                var archivos = Directory.GetFiles(_backupBasePath, patron);

                if (archivos.Length == 0)
                {
                    return null;
                }

                // ✅ Ordenar por fecha de creación descendente (más reciente primero)
                var archivoMasReciente = archivos
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(fi => fi.CreationTime)
                    .FirstOrDefault();

                return archivoMasReciente?.FullName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al buscar archivo de backup más reciente");
                return null;
            }
        }

        /// <summary>
        /// ✅ ACTUALIZADO v2.1: Inicializa un nuevo archivo de backup (vacío)
        /// Usa Newtonsoft.Json para serialización
        /// </summary>
        public async Task<bool> InicializarBackup(string cajaId, string usuarioId)
        {
            try
            {
                _logger.LogInformation("═══════════════════════════════════════════════════");
                _logger.LogInformation("🔄 INICIALIZANDO BACKUP DE PRODUCTOS v2.1");
                _logger.LogInformation($"   Caja: {cajaId}");
                _logger.LogInformation($"   Usuario: {usuarioId}");
                _logger.LogInformation($"   Ruta base: {_backupBasePath}");
                _logger.LogInformation("═══════════════════════════════════════════════════");

                string rutaArchivo = ObtenerRutaArchivo(cajaId, usuarioId);

                // ✅ Crear estructura inicial vacía
                var backupData = new BackupDataDto
                {
                    CajaId = cajaId,
                    UsuarioId = usuarioId,
                    FechaInicio = DateTime.Now,
                    FechaUltimaActualizacion = DateTime.Now,
                    Productos = new List<ProductoDatosResponseDto>()
                };

                // ✅ Serializar con Newtonsoft.Json
                string jsonContent = JsonConvert.SerializeObject(backupData, _jsonSettings);

                await File.WriteAllTextAsync(rutaArchivo, jsonContent);

                _logger.LogInformation($"✅ Archivo de backup inicializado: {rutaArchivo}");
                _logger.LogInformation($"   Serializado con Newtonsoft.Json (Formatting.Indented)");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al inicializar backup");
                return false;
            }
        }

        /// <summary>
        /// ✅ ACTUALIZADO v2.1: Guarda un producto en el archivo de backup
        /// Usa Newtonsoft.Json para serialización/deserialización
        /// </summary>
        public async Task<bool> GuardarProducto(ProductoDatosResponseDto producto, string cajaId, string usuarioId)
        {
            try
            {
                _logger.LogInformation("═══════════════════════════════════════════════════");
                _logger.LogInformation("💾 GUARDANDO PRODUCTO EN BACKUP v2.1");
                _logger.LogInformation($"   Producto: {producto.p_id} - {producto.p_desc}");
                _logger.LogInformation($"   Cantidad: {producto.cantidad_tot}");
                _logger.LogInformation("═══════════════════════════════════════════════════");

                // ❶ Buscar archivo más reciente
                string? rutaArchivo = ObtenerRutaArchivoMasReciente(cajaId, usuarioId);

                // ❷ Si no existe archivo, inicializar uno nuevo
                if (rutaArchivo == null || !File.Exists(rutaArchivo))
                {
                    _logger.LogWarning("⚠️ Archivo de backup no existe, inicializando...");
                    await InicializarBackup(cajaId, usuarioId);
                    rutaArchivo = ObtenerRutaArchivoMasReciente(cajaId, usuarioId);

                    if (rutaArchivo == null)
                    {
                        _logger.LogError("❌ No se pudo crear archivo de backup");
                        return false;
                    }
                }

                // ❸ Leer contenido actual
                string jsonActual = await File.ReadAllTextAsync(rutaArchivo);
                
                // ✅ Deserializar con Newtonsoft.Json
                var backupData = JsonConvert.DeserializeObject<BackupDataDto>(jsonActual);

                if (backupData == null)
                {
                    _logger.LogError("❌ Error al deserializar backup existente con Newtonsoft.Json");
                    return false;
                }

                // ❹ Agregar nuevo producto
                backupData.Productos.Add(producto);
                backupData.FechaUltimaActualizacion = DateTime.Now;
                backupData.CantidadProductos = backupData.Productos.Count;

                // ❺ Escribir archivo actualizado
                // ✅ Serializar con Newtonsoft.Json
                string jsonActualizado = JsonConvert.SerializeObject(backupData, _jsonSettings);

                await File.WriteAllTextAsync(rutaArchivo, jsonActualizado);

                _logger.LogInformation($"✅ Producto guardado. Total productos: {backupData.CantidadProductos}");
                _logger.LogInformation($"   Archivo: {Path.GetFileName(rutaArchivo)}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al guardar producto en backup");
                return false;
            }
        }

        /// <summary>
        /// ✅ ACTUALIZADO v2.1: Recupera todos los productos del backup más reciente
        /// Usa Newtonsoft.Json para deserialización
        /// </summary>
        public async Task<List<ProductoDatosResponseDto>> RecuperarBackup(string cajaId, string usuarioId)
        {
            try
            {
                _logger.LogInformation("═══════════════════════════════════════════════════");
                _logger.LogInformation("📂 RECUPERANDO BACKUP DE PRODUCTOS v2.1");
                _logger.LogInformation($"   Caja: {cajaId}");
                _logger.LogInformation($"   Usuario: {usuarioId}");
                _logger.LogInformation("═══════════════════════════════════════════════════");

                string? rutaArchivo = ObtenerRutaArchivoMasReciente(cajaId, usuarioId);

                if (rutaArchivo == null || !File.Exists(rutaArchivo))
                {
                    _logger.LogWarning("⚠️ No existe archivo de backup");
                    return new List<ProductoDatosResponseDto>();
                }

                string jsonContent = await File.ReadAllTextAsync(rutaArchivo);
                
                // ✅ Deserializar con Newtonsoft.Json
                var backupData = JsonConvert.DeserializeObject<BackupDataDto>(jsonContent);

                if (backupData == null || backupData.Productos == null)
                {
                    _logger.LogWarning("⚠️ Backup vacío o corrupto (deserialización con Newtonsoft.Json falló)");
                    return new List<ProductoDatosResponseDto>();
                }

                _logger.LogInformation($"✅ Backup recuperado: {backupData.Productos.Count} productos");
                _logger.LogInformation($"   Archivo: {Path.GetFileName(rutaArchivo)}");
                _logger.LogInformation($"   Fecha inicio: {backupData.FechaInicio}");
                _logger.LogInformation($"   Última actualización: {backupData.FechaUltimaActualizacion}");
                _logger.LogInformation($"   Deserializado con Newtonsoft.Json");

                return backupData.Productos;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "❌ Error de deserialización JSON (Newtonsoft.Json) al recuperar backup");
                return new List<ProductoDatosResponseDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al recuperar backup");
                return new List<ProductoDatosResponseDto>();
            }
        }

        /// <summary>
        /// ✅ ACTUALIZADO v2.1: Limpia todos los archivos de backup de la caja/usuario
        /// </summary>
        public async Task<bool> LimpiarBackup(string cajaId, string usuarioId)
        {
            try
            {
                _logger.LogInformation("═══════════════════════════════════════════════════");
                _logger.LogInformation("🗑️ LIMPIANDO BACKUPS DE PRODUCTOS v2.1");
                _logger.LogInformation($"   Caja: {cajaId}");
                _logger.LogInformation($"   Usuario: {usuarioId}");
                _logger.LogInformation("═══════════════════════════════════════════════════");

                string cajaNormalizada = cajaId.Replace(" ", "_").Replace("\\", "_").Replace("/", "_");
                string usuarioNormalizado = usuarioId.Replace(" ", "_").Replace("\\", "_").Replace("/", "_");

                string patron = $"backup_{cajaNormalizada}_{usuarioNormalizado}_*{BACKUP_EXTENSION}";

                var archivos = Directory.GetFiles(_backupBasePath, patron);

                if (archivos.Length == 0)
                {
                    _logger.LogWarning("⚠️ No había archivos de backup para eliminar");
                    return true;
                }

                int archivosEliminados = 0;
                foreach (var archivo in archivos)
                {
                    try
                    {
                        File.Delete(archivo);
                        archivosEliminados++;
                        _logger.LogInformation($"   ✅ Eliminado: {Path.GetFileName(archivo)}");
                    }
                    catch (Exception exFile)
                    {
                        _logger.LogWarning(exFile, $"   ⚠️ No se pudo eliminar: {Path.GetFileName(archivo)}");
                    }
                }

                _logger.LogInformation($"✅ Archivos de backup eliminados: {archivosEliminados}/{archivos.Length}");
                return archivosEliminados > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al limpiar backups");
                return false;
            }
        }

        /// <summary>
        /// ✅ ACTUALIZADO v2.1: Verifica si existe un backup pendiente
        /// </summary>
        public async Task<bool> ExisteBackup(string cajaId, string usuarioId)
        {
            try
            {
                string? rutaArchivo = ObtenerRutaArchivoMasReciente(cajaId, usuarioId);
                bool existe = rutaArchivo != null && File.Exists(rutaArchivo);

                if (existe)
                {
                    _logger.LogInformation($"✅ Backup encontrado: {Path.GetFileName(rutaArchivo)}");
                }

                return existe;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al verificar existencia de backup");
                return false;
            }
        }

        /// <summary>
        /// ✅ NUEVO v1.1: Elimina un producto específico del backup
        /// </summary>
        public async Task<bool> EliminarProducto(int item, string cajaId, string usuarioId)
        {
            try
            {
                _logger.LogInformation("═══════════════════════════════════════════════════");
                _logger.LogInformation($"🗑️ ELIMINANDO PRODUCTO DEL BACKUP - Item: {item}");
                _logger.LogInformation($"   Caja: {cajaId}, Usuario: {usuarioId}");
                _logger.LogInformation("═══════════════════════════════════════════════════");

                // ❶ Validar parámetros
                if (string.IsNullOrWhiteSpace(cajaId) || string.IsNullOrWhiteSpace(usuarioId))
                {
                    _logger.LogWarning("❌ Parámetros inválidos: cajaId o usuarioId vacíos");
                    return false;
                }

                if (item <= 0)
                {
                    _logger.LogWarning($"❌ Item inválido: {item} (debe ser mayor a 0)");
                    return false;
                }

                // �② Construir ruta del archivo
                var nombreArchivo = ConstruirNombreArchivo(cajaId, usuarioId);
                var rutaArchivo = Path.Combine(_backupBasePath, nombreArchivo);

                // ❸ Validar que el archivo exista
                if (!File.Exists(rutaArchivo))
                {
                    _logger.LogWarning($"⚠️ No existe backup en: {rutaArchivo}");
                    return false; // ✅ No es error, simplemente no hay nada que eliminar
                }

                // ❹ Leer productos actuales
                var jsonContenido = await File.ReadAllTextAsync(rutaArchivo);

                if (string.IsNullOrWhiteSpace(jsonContenido))
                {
                    _logger.LogWarning("⚠️ Archivo de backup vacío");
                    return false;
                }

                var productos = JsonConvert.DeserializeObject<List<ProductoDatosResponseDto>>(jsonContenido);

                if (productos == null || productos.Count == 0)
                {
                    _logger.LogWarning("⚠️ No hay productos en el backup");
                    return false;
                }

                _logger.LogInformation($"📋 Total productos antes de eliminar: {productos.Count}");

                // ❺ Buscar el producto por item
                var productoAEliminar = productos.FirstOrDefault(p => p.item == item);

                if (productoAEliminar == null)
                {
                    _logger.LogWarning($"⚠️ No se encontró producto con item={item}");

                    // 🔍 Mostrar items disponibles para debugging
                    var itemsDisponibles = string.Join(", ", productos.Select(p => p.item));
                    _logger.LogInformation($"   Items disponibles: [{itemsDisponibles}]");

                    return false;
                }

                _logger.LogInformation($"🔍 Producto encontrado:");
                _logger.LogInformation($"   - Item: {productoAEliminar.item}");
                _logger.LogInformation($"   - Código: {productoAEliminar.p_id}");
                _logger.LogInformation($"   - Descripción: {productoAEliminar.p_desc}");

                // ❻ Eliminar el producto de la lista
                productos.Remove(productoAEliminar);

                _logger.LogInformation($"✅ Producto eliminado de la lista");
                _logger.LogInformation($"📋 Total productos después de eliminar: {productos.Count}");

                // ❼ Guardar lista actualizada
                if (productos.Count == 0)
                {
                    // Si no quedan productos, eliminar el archivo completo
                    _logger.LogInformation("🧹 No quedan productos, eliminando archivo de backup...");

                    File.Delete(rutaArchivo);

                    _logger.LogInformation("✅ Archivo de backup eliminado completamente");
                }
                else
                {
                    // Si quedan productos, guardar lista actualizada
                    _logger.LogInformation($"💾 Guardando lista actualizada ({productos.Count} productos)...");

                    var jsonActualizado = JsonConvert.SerializeObject(productos, Formatting.Indented);
                    await File.WriteAllTextAsync(rutaArchivo, jsonActualizado);

                    _logger.LogInformation("✅ Archivo de backup actualizado");
                }

                _logger.LogInformation("═══════════════════════════════════════════════════");
                _logger.LogInformation($"✅ PRODUCTO ELIMINADO EXITOSAMENTE");
                _logger.LogInformation($"   Item: {item}");
                _logger.LogInformation($"   Productos restantes: {productos.Count}");
                _logger.LogInformation("═══════════════════════════════════════════════════");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ ERROR al eliminar producto del backup (item={item})");
                _logger.LogError($"   Mensaje: {ex.Message}");
                _logger.LogError($"   StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// ✅ HELPER: Construye el nombre del archivo de backup
        /// Formato: backup_{cajaId}_{usuarioId}.json
        /// </summary>
        private string ConstruirNombreArchivo(string cajaId, string usuarioId)
        {
            // Limpiar caracteres no válidos para nombres de archivo
            var cajaLimpia = LimpiarNombreArchivo(cajaId);
            var usuarioLimpio = LimpiarNombreArchivo(usuarioId);

            return $"backup_{cajaLimpia}_{usuarioLimpio}.json";
        }

        /// <summary>
        /// ✅ HELPER: Limpia caracteres no válidos de un nombre de archivo
        /// </summary>
        private string LimpiarNombreArchivo(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return "default";

            // Remover caracteres inválidos
            var caracteresInvalidos = Path.GetInvalidFileNameChars();
            var nombreLimpio = string.Join("_", nombre.Split(caracteresInvalidos, StringSplitOptions.RemoveEmptyEntries));

            return nombreLimpio.Trim();
        }

    }


}