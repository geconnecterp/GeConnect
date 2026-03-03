using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

// ═══════════════════════════════════════════════════════════
// VARIABLE GLOBAL PARA LOGGER ANTES DE INICIALIZAR LA APP
// ═══════════════════════════════════════════════════════════
ILogger<Program>? globalLogger = null;

try
{
    Console.WriteLine("🚀 Iniciando FileStoreService...");
    
    var builder = WebApplication.CreateBuilder(args);

    Console.WriteLine("✅ WebApplicationBuilder creado");

    // Configurar Log4Net
    try
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();
        builder.Logging.AddLog4Net("log4net.config", watch: true);
        Console.WriteLine("✅ Log4Net configurado");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Error al configurar Log4Net: {ex.Message}");
        // Continuar sin Log4Net si falla
    }

    // Configurar AutoMapper
    try
    {
        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        Console.WriteLine("✅ AutoMapper configurado");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Error al configurar AutoMapper: {ex.Message}");
        // Continuar sin AutoMapper si falla
    }

    // Configurar límite de tamaño de archivos (100MB)
    builder.Services.Configure<FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 104857600; // 100MB
    });
    Console.WriteLine("✅ FormOptions configurado (100MB)");

    builder.Services.AddEndpointsApiExplorer();
    Console.WriteLine("✅ EndpointsApiExplorer agregado");

    builder.Services.AddSwaggerGen();
    Console.WriteLine("✅ Swagger configurado");

    // ✅ SOLUCIÓN: Configurar HTTPS Redirection con opciones
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
        options.HttpsPort = 443; // Puerto HTTPS por defecto
    });
    Console.WriteLine("✅ HTTPS Redirection configurado con puerto 443");

    // Configurar CORS para permitir desde tu app principal
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowGeConnect", policy =>
        {
            policy.WithOrigins("https://localhost:7078", "https://172.10.10.11", "http://localhost:7078")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });
    Console.WriteLine("✅ CORS configurado");

    var app = builder.Build();
    Console.WriteLine("✅ Aplicación construida");

    // ═══════════════════════════════════════════════════════════
    // CREAR LOGGER GLOBAL DESPUÉS DE CONSTRUIR LA APP
    // ═══════════════════════════════════════════════════════════
    globalLogger = app.Services.GetRequiredService<ILogger<Program>>();
    LogAndWrite(globalLogger, "🚀 Iniciando FileStoreService...");
    LogAndWrite(globalLogger, "✅ Logger global inicializado correctamente");

    // Configurar middleware de logging personalizado
    app.Use(async (context, next) =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("📥 Request: {Method} {Path} desde {RemoteIp}", 
            context.Request.Method, 
            context.Request.Path,
            context.Connection.RemoteIpAddress);
        try
        {
            await next();
            logger.LogInformation("✅ Response: {StatusCode}", context.Response.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error procesando request: {Method} {Path}", context.Request.Method, context.Request.Path);
            throw;
        }
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        LogAndWrite(globalLogger, "✅ Swagger UI habilitado (Development)");
    }

    // ✅ SOLUCIÓN: Solo usar HTTPS Redirection si NO estamos detrás de IIS
    // IIS ya maneja la redirección HTTPS
    if (!app.Environment.IsProduction())
    {
        app.UseHttpsRedirection();
        LogAndWrite(globalLogger, "✅ HTTPS Redirection habilitado (Development)");
    }
    else
    {
        LogAndWrite(globalLogger, "ℹ️ HTTPS Redirection deshabilitado (Production/IIS)");
    }

    app.UseCors("AllowGeConnect");
    LogAndWrite(globalLogger, "✅ CORS middleware habilitado");

    // ✅ Leer ruta desde configuración
    var fileStorePath = app.Configuration["FileStoreSettings:PhysicalPath"] ?? @"C:\Sitios\FileStore";
    app.Logger.LogInformation("📁 Usando directorio FileStore: {Path}", fileStorePath);
    LogAndWrite(globalLogger, $"📁 Ruta FileStore: {fileStorePath}");

    // Validar que el directorio exista
    if (!Directory.Exists(fileStorePath))
    {
        app.Logger.LogWarning("⚠️ El directorio no existe, creándolo: {Path}", fileStorePath);
        LogAndWrite(globalLogger, $"⚠️ Creando directorio: {fileStorePath}", LogLevel.Warning);
        try
        {
            Directory.CreateDirectory(fileStorePath);
            LogAndWrite(globalLogger, "✅ Directorio creado exitosamente");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "❌ Error al crear directorio");
            LogAndWrite(globalLogger, $"❌ Error al crear directorio: {ex.Message}", LogLevel.Error);
            throw;
        }
    }
    else
    {
        LogAndWrite(globalLogger, "✅ Directorio FileStore existe");
    }

    // Servir archivos estáticos desde /fileStore
    try
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(fileStorePath),
            RequestPath = "/fileStore",
            ServeUnknownFileTypes = true, // Permitir servir cualquier tipo de archivo
            DefaultContentType = "application/octet-stream"
        });
        LogAndWrite(globalLogger, "✅ Middleware de archivos estáticos configurado");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "❌ Error al configurar archivos estáticos");
        LogAndWrite(globalLogger, $"❌ Error en UseStaticFiles: {ex.Message}", LogLevel.Error);
        throw;
    }

    // ══════════════════════════════════════════════════════════
    // ENDPOINT PRINCIPAL: Upload de archivos
    // ══════════════════════════════════════════════════════════
    app.MapPost("/api/upload", async (HttpRequest request, ILogger<Program> logger) =>
    {
        try
        {
            logger.LogInformation("📤 Recibiendo solicitud de upload desde {RemoteIp}", request.HttpContext.Connection.RemoteIpAddress);

            if (!request.HasFormContentType)
            {
                logger.LogWarning("⚠️ Request no tiene FormContentType. ContentType: {ContentType}", request.ContentType);
                return Results.BadRequest(new { success = false, message = "Content-Type debe ser multipart/form-data" });
            }

            if (request.Form.Files.Count == 0)
            {
                logger.LogWarning("⚠️ No se recibieron archivos en el request");
                return Results.BadRequest(new { success = false, message = "No se recibieron archivos" });
            }

            var file = request.Form.Files[0];
            logger.LogInformation("📄 Archivo recibido: {FileName} ({Size} KB)", file.FileName, file.Length / 1024);

            if (file.Length == 0)
            {
                logger.LogWarning("⚠️ El archivo está vacío");
                return Results.BadRequest(new { success = false, message = "El archivo está vacío" });
            }

            // Validar tamaño (100MB)
            if (file.Length > 104857600)
            {
                logger.LogWarning("⚠️ Archivo excede límite: {Size} MB", file.Length / 1024 / 1024);
                return Results.BadRequest(new { success = false, message = "El archivo excede el límite de 100MB" });
            }

            // Guardar archivo con nombre sanitizado
            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(fileStorePath, fileName);

            logger.LogInformation("💾 Guardando archivo en: {Path}", filePath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            logger.LogInformation("✅ Archivo guardado exitosamente: {Size} KB", new FileInfo(filePath).Length / 1024);

            // Construir URL pública (usar siempre HTTPS en producción)
            var scheme = app.Environment.IsProduction() ? "https" : request.Scheme;
            var baseUrl = $"{scheme}://{request.Host}";
            var publicUrl = $"{baseUrl}/fileStore/{fileName}";

            logger.LogInformation("🔗 URL pública generada: {Url}", publicUrl);

            return Results.Ok(new
            {
                success = true,
                url = publicUrl,
                fileName = fileName,
                size = file.Length,
                message = "Archivo guardado exitosamente"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error al guardar archivo");
            return Results.Problem(
                detail: ex.Message,
                statusCode: 500,
                title: "Error al guardar archivo"
            );
        }
    }).DisableAntiforgery(); // Deshabilitar antiforgery para este endpoint

    LogAndWrite(globalLogger, "✅ Endpoint /api/upload mapeado");

    // Endpoint de health check
    app.MapGet("/api/health", (ILogger<Program> logger, HttpContext context) =>
    {
        logger.LogInformation("💚 Health check solicitado desde {RemoteIp}", context.Connection.RemoteIpAddress);
        
        var directoryInfo = new DirectoryInfo(fileStorePath);
        var filesCount = directoryInfo.Exists ? directoryInfo.GetFiles().Length : 0;
        var totalSize = directoryInfo.Exists 
            ? directoryInfo.GetFiles().Sum(f => f.Length) 
            : 0;

        return Results.Ok(new
        {
            status = "healthy",
            service = "FileStore",
            version = "1.0.0",
            path = fileStorePath,
            filesCount = filesCount,
            totalSizeMB = totalSize / 1024 / 1024,
            timestamp = DateTime.UtcNow,
            environment = app.Environment.EnvironmentName
        });
    });

    LogAndWrite(globalLogger, "✅ Endpoint /api/health mapeado");

    // Endpoint raíz para verificar que la app está corriendo
    app.MapGet("/", () => Results.Ok(new
    {
        service = "FileStore Service",
        version = "1.0.0",
        status = "running",
        endpoints = new[]
        {
            "GET /api/health - Verificar estado del servicio",
            "POST /api/upload - Subir archivos (multipart/form-data)",
            "GET /fileStore/{filename} - Descargar archivo"
        }
    }));

    LogAndWrite(globalLogger, "✅ Endpoint raíz (/) mapeado");

    LogAndWrite(globalLogger, "🎉 Configuración completada. Iniciando servidor...");
    app.Logger.LogInformation("🚀 FileStoreService iniciado correctamente en {Environment}", app.Environment.EnvironmentName);

    // Información de debug
    LogAndWrite(globalLogger, "═══════════════════════════════════════════════════════");
    LogAndWrite(globalLogger, "🔍 CONFIGURACIÓN DEBUG:");
    LogAndWrite(globalLogger, $"   Environment: {app.Environment.EnvironmentName}");
    LogAndWrite(globalLogger, $"   ContentRootPath: {app.Environment.ContentRootPath}");
    LogAndWrite(globalLogger, $"   WebRootPath: {app.Environment.WebRootPath}");
    LogAndWrite(globalLogger, $"   FileStorePath: {fileStorePath}");
    LogAndWrite(globalLogger, $"   Directorio existe: {Directory.Exists(fileStorePath)}");

    if (Directory.Exists(fileStorePath))
    {
        var dirInfo = new DirectoryInfo(fileStorePath);
        LogAndWrite(globalLogger, $"   Permisos: {dirInfo.Attributes}");
        LogAndWrite(globalLogger, $"   Archivos actuales: {dirInfo.GetFiles().Length}");
    }

    LogAndWrite(globalLogger, "═══════════════════════════════════════════════════════");

    app.Run();
}
catch (Exception ex)
{
    var errorMsg = $"""
        ═══════════════════════════════════════════════════════
        ❌❌❌ ERROR CRÍTICO AL INICIAR LA APLICACIÓN ❌❌❌
        ═══════════════════════════════════════════════════════
        Tipo: {ex.GetType().Name}
        Mensaje: {ex.Message}
        StackTrace:
        {ex.StackTrace}
        """;

    Console.WriteLine(errorMsg);
    
    if (globalLogger != null)
    {
        globalLogger.LogCritical(ex, "❌❌❌ ERROR CRÍTICO AL INICIAR LA APLICACIÓN");
    }

    if (ex.InnerException != null)
    {
        var innerErrorMsg = $"""
            
            --- Inner Exception ---
            Tipo: {ex.InnerException.GetType().Name}
            Mensaje: {ex.InnerException.Message}
            StackTrace:
            {ex.InnerException.StackTrace}
            """;
        
        Console.WriteLine(innerErrorMsg);
        
        if (globalLogger != null)
        {
            globalLogger.LogCritical(ex.InnerException, "--- Inner Exception ---");
        }
    }

    Console.WriteLine("═══════════════════════════════════════════════════════");

    // Esperar para que se puedan leer los logs antes de que se cierre
    Console.WriteLine("\nPresiona cualquier tecla para salir...");
    Console.ReadKey();

    throw;
}

// ═══════════════════════════════════════════════════════════
// MÉTODO HELPER PARA LOGGING DUAL (CONSOLA + LOG4NET)
// ═══════════════════════════════════════════════════════════
static void LogAndWrite(ILogger? logger, string message, LogLevel level = LogLevel.Information)
{
    // Siempre escribir en consola
    Console.WriteLine(message);
    
    // Si el logger está disponible, también escribir en Log4Net
    if (logger != null)
    {
        switch (level)
        {
            case LogLevel.Trace:
                logger.LogTrace(message);
                break;
            case LogLevel.Debug:
                logger.LogDebug(message);
                break;
            case LogLevel.Information:
                logger.LogInformation(message);
                break;
            case LogLevel.Warning:
                logger.LogWarning(message);
                break;
            case LogLevel.Error:
                logger.LogError(message);
                break;
            case LogLevel.Critical:
                logger.LogCritical(message);
                break;
        }
    }
}

