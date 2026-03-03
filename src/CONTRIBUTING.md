# Guía de Contribución - GeConnect

## ?? Estándares de Desarrollo

### ?? Principios de Análisis y Modificación de Código

#### **Regla de Objetividad en Cambios de Código**

**Cuando se analiza un archivo, función o proceso específico:**

? **HACER:**
- Analizar **SOLO** el código directamente relacionado con el objetivo del análisis
- Modificar **ÚNICAMENTE** las funciones o bloques de código que están causando el problema o necesitan la funcionalidad solicitada
- Mantener el resto del código sin cambios, incluso si existen oportunidades de mejora
- Documentar claramente QUÉ se modificó y POR QUÉ

? **NO HACER:**
- Modificar código periférico que funciona correctamente
- Realizar "mejoras" no solicitadas en funciones no relacionadas
- Refactorizar código que no está dentro del alcance del análisis
- Tocar funciones que NO pertenecen al proceso objetivo
- Cambiar estilos de código o formateo en áreas no relacionadas

#### **Ejemplo Práctico**

**Solicitud:** "Corregir el envío de emails en Outlook Web para que los enlaces sean HTML"

? **Correcto:**
```javascript
// Modificar SOLO la función procesarArchivosParaEmail()
// y las funciones directamente relacionadas con formateo de enlaces
function procesarArchivosParaEmail() {
    // ... cambios específicos aquí
}
```

? **Incorrecto:**
```javascript
// NO modificar funciones como:
// - enviarWhatsApp() (no relacionada con emails)
// - presentarArchivos() (no relacionada con envío)
// - invocaGenerarArchivo() (funcionalidad diferente)
```

#### **Alcance de Modificaciones**

Al realizar cambios, considerar **SOLO** estas categorías:

1. **Código Objetivo Principal** (Modificar)
   - La función específicamente mencionada
   - Funciones que llaman directamente a la función objetivo
   - Funciones llamadas por la función objetivo

2. **Código Periférico** (NO Modificar)
   - Funciones con nombres similares pero funcionalidad diferente
   - Código que funciona correctamente en otros módulos
   - Utilidades generales que no causan el problema

3. **Código de Soporte** (Modificar solo si es necesario)
   - DTOs o modelos compartidos (solo si afectan directamente)
   - Constantes o configuraciones (solo las relevantes)

---

## ?? Prácticas de Código

### Formato y Estilo

Seguir las reglas definidas en `.editorconfig` para mantener consistencia en el código.

### Logging

Usar logging estructurado con emojis para facilitar el debugging:
```csharp
_logger?.LogInformation("? Operación exitosa: {Detalle}", detalle);
_logger?.LogWarning("?? Advertencia: {Mensaje}", mensaje);
_logger?.LogError("? Error: {Error}", error);
```

### Nomenclatura

- **C#**: PascalCase para clases, métodos y propiedades
- **JavaScript**: camelCase para funciones y variables
- **Constantes**: UPPER_SNAKE_CASE

---

## ?? Documentación de Cambios

Cada modificación debe incluir:

1. **Qué** se cambió (función/archivo específico)
2. **Por qué** se cambió (problema que resuelve)
3. **Cómo** se probó (verificación del cambio)
4. **Alcance** (qué NO se tocó intencionalmente)

---

## ? Checklist de Revisión

Antes de finalizar cualquier cambio, verificar:

- [ ] ¿Solo modifiqué el código objetivo?
- [ ] ¿Dejé intacto el código periférico que funciona?
- [ ] ¿Documenté claramente los cambios?
- [ ] ¿Expliqué por qué NO toqué otras funciones?
- [ ] ¿El cambio es mínimo y enfocado?

---

**Última actualización:** 3 de marzo de 2026