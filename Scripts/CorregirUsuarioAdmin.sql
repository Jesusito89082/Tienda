-- Script de verificación y corrección del usuario admin
USE TiendaRopa;
GO

PRINT '========================================';
PRINT '   VERIFICACIÓN Y CORRECCIÓN ADMIN ';
PRINT '========================================';
PRINT '';

-- 1. Verificar roles existentes
PRINT '1?? ROLES EXISTENTES:';
PRINT '----------------------------------------';
SELECT Id, Name as 'Rol', NormalizedName 
FROM AspNetRoles;
PRINT '';

-- 2. Verificar usuario admin
PRINT '2?? USUARIO ADMIN:';
PRINT '----------------------------------------';
SELECT Id, UserName, Email, EmailConfirmed, LockoutEnabled
FROM AspNetUsers 
WHERE UserName = 'admin';
PRINT '';

-- 3. Verificar roles asignados al admin
PRINT '3?? ROLES DEL USUARIO ADMIN:';
PRINT '----------------------------------------';
SELECT 
    u.UserName as 'Usuario',
    u.Email,
    r.Name as 'Rol'
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.UserName = 'admin';
PRINT '';

-- 4. CORRECCIÓN: Asignar rol ADMINISTRADOR si no lo tiene
PRINT '4?? CORRECCIÓN AUTOMÁTICA:';
PRINT '----------------------------------------';

DECLARE @AdminUserId NVARCHAR(450);
DECLARE @AdminRoleId NVARCHAR(450);
DECLARE @IsAssigned INT;

-- Obtener ID del usuario admin
SELECT @AdminUserId = Id FROM AspNetUsers WHERE UserName = 'admin';

-- Obtener ID del rol ADMINISTRADOR
SELECT @AdminRoleId = Id FROM AspNetRoles WHERE NormalizedName = 'ADMINISTRADOR';

IF @AdminUserId IS NULL
BEGIN
    PRINT '? ERROR: Usuario admin NO existe';
    PRINT '   Solución: Ejecuta la aplicación para que el DataSeeder lo cree';
    PRINT '';
END
ELSE IF @AdminRoleId IS NULL
BEGIN
    PRINT '? ERROR: Rol ADMINISTRADOR NO existe';
    PRINT '   Solución: Ejecuta la aplicación para que el DataSeeder lo cree';
    PRINT '';
END
ELSE
BEGIN
    -- Verificar si ya tiene el rol asignado
    SELECT @IsAssigned = COUNT(*) 
    FROM AspNetUserRoles 
    WHERE UserId = @AdminUserId AND RoleId = @AdminRoleId;

 IF @IsAssigned = 0
    BEGIN
        -- Asignar el rol
        INSERT INTO AspNetUserRoles (UserId, RoleId)
        VALUES (@AdminUserId, @AdminRoleId);
        PRINT '? Rol ADMINISTRADOR asignado correctamente al usuario admin';
    END
    ELSE
    BEGIN
        PRINT '? Usuario admin YA tiene el rol ADMINISTRADOR';
    END
    PRINT '';
END
GO

-- 5. VERIFICACIÓN FINAL
PRINT '5?? VERIFICACIÓN FINAL:';
PRINT '----------------------------------------';
SELECT 
    u.UserName as 'Usuario',
    u.Email,
    u.EmailConfirmed as 'Email OK',
    u.LockoutEnabled as 'Puede Bloquearse',
    STRING_AGG(r.Name, ', ') as 'Roles'
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.UserName = 'admin'
GROUP BY u.UserName, u.Email, u.EmailConfirmed, u.LockoutEnabled;
GO

PRINT '';
PRINT '========================================';
PRINT '        INFORMACIÓN DE ACCESO          ';
PRINT '========================================';
PRINT 'URL: /Auth/Login';
PRINT 'Usuario: admin';
PRINT 'Contraseña: admin123';
PRINT '';
PRINT 'Si el usuario tiene el rol ADMINISTRADOR,';
PRINT 'deberías ver los menús:';
PRINT '  ? Categorías';
PRINT '  ? Ventas';
PRINT '  ? Detalles de Venta';
PRINT '';
PRINT '========================================';
GO
