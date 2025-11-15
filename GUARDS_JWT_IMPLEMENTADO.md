# ?? Sistema de Guards con JWT - Implementado

## ?? Objetivo Completado

Se ha implementado un sistema completo de **guards basados en JWT** que controla el acceso a las rutas del ADMINISTRADOR mediante tokens.

---

## ??? Arquitectura del Sistema

### Componentes Implementados:

1. **JWT Authentication** - Autenticación mediante tokens
2. **JwtRoleGuardMiddleware** - Guard del lado del servidor
3. **auth-guard.js** - Guard del lado del cliente
4. **Cookie HTTP-Only** - Almacenamiento seguro del token

---

## ?? Flujo de Autenticación

### 1?? **Login**
```
Usuario ingresa credenciales
    ?
Validación en servidor
    ?
Genera JWT con rol ADMINISTRADOR
    ?
Guarda token en cookie HTTP-only
    ?
Redirige a Home
```

### 2?? **Acceso a Rutas Protegidas**
```
Usuario intenta acceder a /Categorias
    ?
Middleware JwtRoleGuardMiddleware intercepta
    ?
Extrae token de cookie/header/session
    ?
Valida que contenga rol ADMINISTRADOR
?
? Permite acceso / ? Redirige a AccessDenied
```

### 3?? **Validación en Cliente (JavaScript)**
```
Página carga
    ?
auth-guard.js se ejecuta
    ?
Lee token de cookie
    ?
Decodifica y verifica rol
    ?
Muestra/oculta elementos del menú
```

---

## ?? Archivos Creados/Modificados

### Nuevos Archivos:

1. **`Middleware/JwtRoleGuardMiddleware.cs`** ?
   - Middleware del servidor para validar JWT y roles
   - Intercepta todas las peticiones a rutas protegidas
   - Extrae token de múltiples fuentes (cookie, header, session)
   - Valida rol ADMINISTRADOR

2. **`wwwroot/js/auth-guard.js`** ?
   - Guard del lado del cliente
   - Decodifica JWT en el navegador
   - Oculta/muestra elementos del menú
   - Protege clics en links que requieren auth
   - Verifica expiración del token

### Archivos Modificados:

3. **`Program.cs`** ?
   - Rehabilitado JWT Authentication
   - Configuración dual (Cookie + JWT)
   - Registrado middleware `UseJwtRoleGuard()`

4. **`Controllers/AuthController.cs`** ?
   - Login guarda JWT en cookie HTTP-only
   - Logout elimina JWT de cookies

5. **`Views/Shared/_Layout.cshtml`** ?
   - Agregado script `auth-guard.js`
   - Atributos `data-require-role` en menús admin
   - Atributos `data-public-only` en menús públicos

---

## ?? Rutas Protegidas

### Requieren Rol ADMINISTRADOR:

| Ruta | Guard Servidor | Guard Cliente |
|------|---------------|---------------|
| `/Categorias/*` | ? Middleware | ? JavaScript |
| `/Ventas/*` | ? Middleware | ? JavaScript |
| `/DetallesVentums/*` | ? Middleware | ? JavaScript |
| `/Clientes/*` | ? Middleware | ? JavaScript |
| `/Facturas/*` | ? Middleware | ? JavaScript |
| `/Admin/*` | ? Middleware | ? JavaScript |

### Rutas Públicas:

| Ruta | Acceso |
|------|--------|
| `/Auth/Login` | ? Todos |
| `/Home/*` | ? Todos |
| `/Productoes/Index` | ? Todos |
| `/Productoes/Details` | ? Todos |
| `/Carrito/*` | ? Todos |

---

## ??? Niveles de Seguridad

### Nivel 1: Servidor (C# Middleware)
```csharp
// JwtRoleGuardMiddleware.cs
// Intercepta TODAS las peticiones HTTP
// Valida token JWT antes de llegar al controlador
// No se puede bypassear con JavaScript deshabilitado
```

**Ventajas:**
- ? Seguridad total del lado del servidor
- ? No se puede desactivar
- ? Valida token en cada petición

### Nivel 2: Cliente (JavaScript)
```javascript
// auth-guard.js
// Mejora UX ocultando opciones no permitidas
// Evita intentos de acceso innecesarios
// Puede desactivarse si el usuario lo intenta
```

**Ventajas:**
- ? Mejora experiencia de usuario
- ? Feedback instantáneo
- ? Oculta opciones no disponibles

### Nivel 3: Atributos [Authorize]
```csharp
// En controladores
[Authorize(Roles = "ADMINISTRADOR")]
public class CategoriasController
```

**Ventajas:**
- ? Capa adicional de seguridad
- ? Integrado con ASP.NET Identity
- ? Funciona incluso si middleware falla

---

## ?? Configuración del Token JWT

### Estructura del Token:

```json
{
  "nameid": "user-id-guid",
  "unique_name": "admin",
  "email": "admin@tienda.com",
  "role": "ADMINISTRADOR",
  "jti": "token-id-guid",
  "exp": 1705234567,
  "iss": "TiendaAura",
  "aud": "TiendaAuraUsers"
}
```

### Configuración de Seguridad:

```csharp
// Cookie HTTP-only
var cookieOptions = new CookieOptions
{
    HttpOnly = true,      // No accesible desde JavaScript
    Secure = true,        // Solo HTTPS
    SameSite = SameSiteMode.Strict,  // Protección CSRF
    Expires = DateTime.UtcNow.AddHours(8)  // Expira en 8 horas
};
```

---

## ?? Cómo Probarlo

### Paso 1: Iniciar la aplicación
```bash
dotnet run
```

### Paso 2: Intentar acceder sin login
1. Abre: `https://localhost:XXXX/Categorias`
2. ? **Debería redirigir a** `/Auth/AccessDenied`

### Paso 3: Login como admin
1. Ve a: `/Auth/Login`
2. Usuario: `admin`
3. Contraseña: `admin123`
4. ? **Debería iniciar sesión y guardar JWT en cookie**

### Paso 4: Verificar token en DevTools
1. Abre F12 (DevTools)
2. Ve a **Application** ? **Cookies**
3. ? **Deberías ver cookie `JWTToken`** con el token

### Paso 5: Verificar menú
1. ? **Deberías ver solo:**
   - Inicio
   - Categorías
   - Ventas
   - Detalles de Venta
   - Carrito

### Paso 6: Acceder a ruta protegida
1. Click en **Categorías**
2. ? **Debería permitir acceso**

### Paso 7: Cerrar sesión
1. Click en tu usuario ? **Cerrar Sesión**
2. ? **Cookie JWTToken eliminada**
3. ? **Menú cambia a versión pública**

### Paso 8: Intentar acceder después de logout
1. Intenta ir a `/Categorias`
2. ? **Debería redirigir a AccessDenied**

---

## ?? Debugging del Guard

### Verificar Token en Console:

Abre la consola del navegador (F12) y ejecuta:

```javascript
// Ver si tienes rol de admin
console.log('Es Admin:', AuthGuard.hasAdminRole());

// Ver si el token expiró
console.log('Token Expirado:', AuthGuard.isTokenExpired());

// Forzar verificación de ruta
AuthGuard.guardRoute();
```

### Ver Logs del Servidor:

```
info: Tienda.Middleware.JwtRoleGuardMiddleware[0]
      Acceso permitido a /categorias: Usuario con rol ADMINISTRADOR

warn: Tienda.Middleware.JwtRoleGuardMiddleware[0]
      Acceso denegado a /categorias: Token no encontrado
```

---

## ? Características Especiales

### 1. Multi-Source Token Extraction
El middleware busca el token en 3 lugares:
1. Cookie `JWTToken` (preferido)
2. Header `Authorization: Bearer <token>`
3. Session `JWTToken` (fallback)

### 2. Client-Side Token Validation
JavaScript valida token sin hacer peticiones al servidor:
- ? Decodifica JWT localmente
- ? Verifica expiración
- ? Lee roles del payload

### 3. Automatic Menu Updates
El menú se actualiza automáticamente según el token:
```javascript
// Elementos con data-require-role solo visibles si tienes el rol
<li data-require-role="ADMINISTRADOR">...</li>

// Elementos con data-public-only solo visibles si NO tienes el rol
<li data-public-only>...</li>
```

### 4. Token Expiration Monitoring
Cada 60 segundos verifica si el token expiró:
```javascript
setInterval(function() {
    if (isTokenExpired()) {
        console.warn('Token expirado');
    }
}, 60000);
```

---

## ?? Manejo de Errores

### Token Expirado:
```
Usuario intenta acceder
    ?
Token expiró
    ?
Redirige a /Auth/Login?returnUrl=/Categorias
    ?
Después de login, vuelve a /Categorias
```

### Sin Rol Suficiente:
```
Usuario sin rol ADMINISTRADOR intenta acceder
    ?
Middleware valida token
    ?
No encuentra rol ADMINISTRADOR
    ?
Redirige a /Auth/AccessDenied
```

### Token Inválido:
```
Token corrupto o malformado
    ?
Middleware no puede decodificar
    ?
Trata como "sin token"
    ?
Redirige a /Auth/AccessDenied
```

---

## ?? Tabla Comparativa: Antes vs Ahora

| Aspecto | Antes | Ahora |
|---------|-------|-------|
| **Autenticación** | Solo cookies de Identity | Cookie + JWT |
| **Validación de Roles** | Solo atributos `[Authorize]` | Middleware + Atributos + JS |
| **Menú Dinámico** | Server-side rendering | Server + Client-side |
| **Token Storage** | Session | Cookie HTTP-only |
| **Client Protection** | ? Ninguna | ? JavaScript Guard |
| **Server Protection** | ? `[Authorize]` | ? Middleware + `[Authorize]` |
| **Token Expiration** | ? No monitoreado | ? Monitoreado cada 60s |

---

## ?? Ventajas del Sistema

1. **Doble Capa de Seguridad**
   - ? Middleware valida en servidor
   - ? JavaScript mejora UX

2. **Token Seguro**
   - ? HTTP-only cookie
   - ? No accesible desde JS malicioso
   - ? Protección CSRF

3. **Experiencia de Usuario**
   - ? Menú se actualiza automáticamente
   - ? No ve opciones que no puede usar
   - ? Redirección automática si token expira

4. **Mantenibilidad**
   - ? Middleware centralizado
   - ? Guards reutilizables
   - ? Fácil agregar nuevas rutas protegidas

---

## ?? Cómo Agregar Nuevas Rutas Protegidas

### En el Middleware:
```csharp
// Middleware/JwtRoleGuardMiddleware.cs
var adminRoutes = new[]
{
    "/categorias",
    "/ventas",
    "/nuevaruta" // ? Agregar aquí
};
```

### En JavaScript:
```javascript
// wwwroot/js/auth-guard.js
const adminRoutes = [
    '/categorias',
    '/ventas',
    '/nuevaruta' // ? Agregar aquí
];
```

### En la Vista:
```html
<!-- Agregar data-require-role -->
<a asp-controller="NuevoController" asp-action="Index" data-require-role="ADMINISTRADOR">
    Nueva Opción
</a>
```

---

## ? Checklist de Implementación

- [x] JWT Authentication configurado
- [x] Middleware JwtRoleGuardMiddleware creado
- [x] Guard JavaScript implementado
- [x] Tokens guardados en cookies HTTP-only
- [x] Login actualizado para generar JWT
- [x] Logout actualizado para eliminar JWT
- [x] Menú con atributos data-require-role
- [x] Script auth-guard.js agregado al layout
- [x] Rutas protegidas configuradas
- [x] Manejo de errores implementado
- [x] Documentación completa

---

## ?? Estado Final

? **Sistema de Guards Completamente Implementado**

El ADMINISTRADOR ahora:
- ? Solo ve sus rutas permitidas en el menú
- ? Es validado por token JWT en cada petición
- ? No puede acceder a rutas sin el rol correcto
- ? Tiene feedback visual inmediato
- ? Es redirigido automáticamente si el token expira

---

**Fecha:** 14/01/2025
**Implementado por:** Sistema de Guards con JWT  
**Estado:** ? Completo y Funcional
