# Guía de Contribución - GeConnect

## 📋 Tabla de Contenidos

1. [Principios Fundamentales](#-principios-fundamentales)
2. [Análisis y Modificación de Código](#-análisis-y-modificación-de-código)
3. [Código Simple, Robusto y Escalable](#-código-simple-robusto-y-escalable)
4. [Estándares Técnicos](#-estándares-técnicos)
5. [Presentación de Cambios](#-presentación-de-cambios)
6. [Patrones de Diseño](#-patrones-de-diseño)
7. [Checklist de Calidad](#-checklist-de-calidad)
8. [Mejores Prácticas](#-mejores-prácticas)
9. [Guía de Respuesta](#-guía-de-respuesta)

---

## 🎯 Principios Fundamentales

### Análisis Exhaustivo Obligatorio

**Antes de modificar cualquier código, se debe realizar un análisis completo que incluya:**

#### Pasos del Análisis

1. **Identificación de Componentes**
   - Listar archivos que serán modificados
   - Identificar dependencias directas e indirectas
   - Verificar impacto en módulos relacionados

2. **Análisis de Contexto**
   - Revisar archivos abiertos en el IDE
   - Examinar arquitectura existente
   - Identificar patrones de diseño utilizados
   - Verificar convenciones de nomenclatura

3. **Evaluación de Riesgos**
   - Detectar código periférico que NO debe modificarse
   - Identificar funciones críticas del sistema
   - Evaluar compatibilidad con versiones anteriores

4. **Planificación de Cambios**
   - Definir alcance preciso
   - Establecer orden de implementación
   - Preparar plan de rollback

---

## 🔧 Análisis y Modificación de Código

### Regla de Objetividad en Cambios de Código

**Cuando se analiza un archivo, función o proceso específico:**

#### ✅ **HACER:**
- Analizar **SOLO** el código directamente relacionado con el objetivo
- Modificar **ÚNICAMENTE** las funciones que causan el problema o necesitan la funcionalidad
- Mantener el resto del código sin cambios
- Documentar claramente QUÉ se modificó y POR QUÉ

#### ❌ **NO HACER:**
- Modificar código periférico que funciona correctamente
- Realizar "mejoras" no solicitadas en funciones no relacionadas
- Refactorizar código fuera del alcance del análisis
- Tocar funciones que NO pertenecen al proceso objetivo
- Cambiar estilos de código o formateo en áreas no relacionadas

### Alcance de Modificaciones

#### 1. **Código Objetivo Principal** (✅ Modificar)
- La función específicamente mencionada
- Funciones que llaman directamente a la función objetivo
- Funciones llamadas por la función objetivo

#### 2. **Código Periférico** (❌ NO Modificar)
- Funciones con nombres similares pero funcionalidad diferente
- Código que funciona correctamente en otros módulos
- Utilidades generales que no causan el problema

#### 3. **Código de Soporte** (⚠️ Modificar solo si es necesario)
- DTOs o modelos compartidos (solo si afectan directamente)
- Constantes o configuraciones (solo las relevantes)

### Ejemplo Práctico

**Solicitud:** "Cambiar sistema de mensajes de Lobibox a AbrirMensaje"

#### ✅ **Correcto:**
```javascript
// Modificar SOLO la función abrirMensaje()
function abrirMensaje() {
    // ... cambios específicos aquí
}
```

#### ❌ **Incorrecto:**
```javascript
// NO modificar funciones como:
// - enviarWhatsApp() (no relacionada con mensajes)
// - presentarArchivos() (no relacionada con el sistema de mensajería)
// - invocaGenerarArchivo() (funcionalidad diferente)
```

---

## 🏗️ Código Simple, Robusto y Escalable

### Simplicidad (KISS - Keep It Simple, Stupid)

#### Principios
- ✅ Una función = Una responsabilidad
- ✅ Nombres descriptivos y autoexplicativos
- ✅ Evitar anidamiento excesivo (máximo 3 niveles)
- ✅ Preferir código legible sobre código "inteligente"
- ✅ Funciones pequeñas (<50 líneas idealmente)

#### Ejemplo Correcto
```javascript
// Buena práctica: función pequeña y con nombre descriptivo
function calcularPrecioConIVA(precioSinIVA) {
    const tasaIVA = 0.21;
    return precioSinIVA * (1 + tasaIVA);
}
```

### Robustez

#### Checklist de Robustez
- ✅ Validación de parámetros de entrada
- ✅ Manejo de errores con try-catch
- ✅ Valores por defecto para evitar null/undefined
- ✅ Mensajes de error descriptivos
- ✅ Logging en puntos críticos
- ✅ Timeouts en operaciones asíncronas

#### Template de Función Robusta
```javascript
function nombreFuncion(param1, param2) {
    // ✅ Validación de parámetros
    if (!param1 || !param2) {
        throw new Error('Faltan parámetros obligatorios');
    }

    // ✅ Valores por defecto
    const timeout = 5000;
    let resultado;

    try {
        // ... código de la función

        // ✅ Manejo de errores
    } catch (error) {
        console.error('Error en nombreFuncion:', error);
        throw error; // Volver a lanzar el error después de hacer logging
    }

    // ✅ Logging
    console.log('Resultado de nombreFuncion:', resultado);

    return resultado;
}
```

### Escalabilidad

#### Principios de Escalabilidad
- ✅ Separación de responsabilidades (SRP)
- ✅ Uso de patrones de diseño apropiados
- ✅ API pública bien documentada
- ✅ Configuración externalizada
- ✅ Código preparado para extensión futura (Open/Closed Principle)

#### Ejemplo de Código Escalable
```javascript
// Ejemplo de función que sigue los principios de escalabilidad
function configurarRuta(api) {
    // ✅ Separación de responsabilidades: configuración de ruta en su propia función
    api.get('/ruta', manejarSolicitud);

    // ✅ Uso de patrones de diseño: manejo de solicitudes usando el patrón Strategy
    function manejarSolicitud(req, res) {
        // ... lógica para manejar la solicitud
    }
}

// Configuración externalizada: las credenciales no están en el código
const configuracion = obtenerConfiguracion();
```
---

## 🛠️ Estándares Técnicos

- Seguir las convenciones de codificación del lenguaje utilizado.
- Mantener una estructura de archivos y carpetas ordenada y predecible.
- Escribir pruebas unitarias y de integración para validar los cambios.

---

## 🚀 Presentación de Cambios

- Hacer commit de los cambios en pequeñas cantidades y de forma atómica.
- Escribir mensajes de commit claros y descriptivos.
- Asegurarse de que todos los tests pasen antes de enviar un pull request.

---

## 🎨 Patrones de Diseño

- Aplicar patrones de diseño apropiados según la situación (p. ej., Singleton, Factory, Observer).
- Ser coherente en el uso de patrones a lo largo del código.
- Documentar la decisión de diseño y el patrón utilizado.

---

## ✔️ Checklist de Calidad

Antes de solicitar una revisión de código, asegurarse de que:

- [ ] Se ha realizado un análisis exhaustivo.
- [ ] Solo se modificó el código objetivo.
- [ ] Se documentaron claramente los cambios.
- [ ] Se probaron todas las funcionalidades afectadas.
- [ ] Se siguieron los estándares técnicos y de codificación.

---

## 🌟 Mejores Prácticas

- Mantener el código limpio y bien comentado.
- Refactorizar el código regularmente para mejorar la calidad.
- Aprender y aplicar nuevos conocimientos y tecnologías que beneficien al proyecto.

---

## 📖 Guía de Respuesta

- Ser constructivo y respetuoso en las revisiones de código.
- Hacer preguntas si algo no está claro.
- Sugerir mejoras pero también reconocer el buen trabajo.

---

**Última actualización:** 3 de marzo de 2026

### Sistema de Mensajes Unificado

**Usar SIEMPRE la función `AbrirMensaje` de `siteGen.js`:**

**Tipos de mensajes disponibles:**
- `"succ!"` - Éxito (icono check, color verde)
- `"error!"` - Error (icono hand, color rojo)
- `"warn!"` - Advertencia (icono error, color naranja)
- `"info!"` - Información (icono info-circle, color azul)

### Logging Estandarizado

#### JavaScript

### Formato y Estilo

Seguir las reglas definidas en `.editorconfig` para mantener consistencia: