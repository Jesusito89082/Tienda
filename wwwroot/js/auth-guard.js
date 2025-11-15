// Guard de autenticación basado en JWT
(function() {
    'use strict';

    // Función para decodificar JWT (solo la parte del payload)
    function parseJwt(token) {
        try {
         const base64Url = token.split('.')[1];
  const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
  const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
           return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
     }).join(''));
         return JSON.parse(jsonPayload);
     } catch (e) {
    console.error('Error al decodificar token:', e);
        return null;
     }
    }

    // Función para obtener el token desde cookies
  function getTokenFromCookie() {
        const name = 'JWTToken=';
        const decodedCookie = decodeURIComponent(document.cookie);
        const cookies = decodedCookie.split(';');
        
        for(let i = 0; i < cookies.length; i++) {
       let cookie = cookies[i].trim();
            if (cookie.indexOf(name) === 0) {
       return cookie.substring(name.length, cookie.length);
    }
   }
        return null;
    }

    // Función para verificar si el usuario tiene rol de ADMINISTRADOR
    function hasAdminRole() {
        const token = getTokenFromCookie();
        
  if (!token) {
            return false;
        }

      const payload = parseJwt(token);
        
        if (!payload) {
            return false;
     }

  // El token puede tener el rol en diferentes formatos
        // Verificar en el claim 'role' o en un array de roles
        const roles = payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || [];
      
        if (Array.isArray(roles)) {
         return roles.some(r => r.toUpperCase() === 'ADMINISTRADOR');
      }
        
        return roles.toUpperCase() === 'ADMINISTRADOR';
    }

    // Función para verificar si el token ha expirado
    function isTokenExpired() {
        const token = getTokenFromCookie();
        
      if (!token) {
       return true;
 }

        const payload = parseJwt(token);
        
        if (!payload || !payload.exp) {
return true;
        }

   const currentTime = Math.floor(Date.now() / 1000);
        return payload.exp < currentTime;
    }

    // Función para proteger rutas en el cliente
    function guardRoute() {
    const path = window.location.pathname.toLowerCase();
        
        // Rutas que requieren autenticación de admin
        const adminRoutes = [
            '/categorias',
            '/ventas',
  '/detallesventums',
 '/clientes',
     '/facturas',
    '/admin'
        ];

 // Verificar si la ruta actual requiere admin
 const requiresAdmin = adminRoutes.some(route => path.startsWith(route));

        if (requiresAdmin) {
            if (isTokenExpired()) {
    console.warn('Token expirado, redirigiendo a login');
           window.location.href = '/Auth/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
     return;
       }

            if (!hasAdminRole()) {
       console.warn('Acceso denegado: Sin rol de ADMINISTRADOR');
    window.location.href = '/Auth/AccessDenied';
   return;
            }
    }
    }

 // Función para mostrar/ocultar elementos del menú según el rol
    function updateMenuVisibility() {
        const isAdmin = hasAdminRole() && !isTokenExpired();
   
        // Ocultar/mostrar menús según el rol
     document.querySelectorAll('[data-require-role]').forEach(element => {
            const requiredRole = element.getAttribute('data-require-role');
            
         if (requiredRole === 'ADMINISTRADOR') {
        element.style.display = isAdmin ? '' : 'none';
        }
    });

  // Mostrar menú público si NO es admin
  document.querySelectorAll('[data-public-only]').forEach(element => {
            element.style.display = !isAdmin ? '' : 'none';
        });
    }

    // Ejecutar guard al cargar la página
    document.addEventListener('DOMContentLoaded', function() {
        guardRoute();
        updateMenuVisibility();
        
        // Verificar token cada minuto
        setInterval(function() {
     if (isTokenExpired()) {
                console.warn('Token expirado');
    // Opcional: redirigir automáticamente o mostrar mensaje
     }
        }, 60000); // 60 segundos
    });

    // Interceptar clics en links que requieren autenticación
    document.addEventListener('click', function(e) {
        const link = e.target.closest('a[data-require-role]');
        
        if (link) {
          const requiredRole = link.getAttribute('data-require-role');
          
            if (requiredRole === 'ADMINISTRADOR') {
     if (isTokenExpired()) {
  e.preventDefault();
       window.location.href = '/Auth/Login?returnUrl=' + encodeURIComponent(link.href);
       return;
   }
     
   if (!hasAdminRole()) {
   e.preventDefault();
        window.location.href = '/Auth/AccessDenied';
          return;
     }
        }
        }
    });

    // Exponer funciones globalmente para uso en otras partes
    window.AuthGuard = {
        hasAdminRole: hasAdminRole,
  isTokenExpired: isTokenExpired,
        guardRoute: guardRoute
    };
})();
