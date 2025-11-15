-- Script de verificación del usuario CAJERO
USE TiendaRopa;
GO

PRINT '========================================';
PRINT '    VERIFICACIÓN USUARIO CAJERO    ';
PRINT '========================================';
PRINT '';

-- 1. Verificar que exista el rol CAJERO
PRINT '1?? VERIFICAR ROL CAJERO:';
PRINT '----------------------------------------';
SELECT Id, Name as 'Rol', NormalizedName 
FROM AspNetRoles
WHERE NormalizedName = 'CAJERO';
PRINT '';

-- 2. Verificar usuario cajero
PRINT '2?? VERIFICAR USUARIO CAJERO:';
PRINT '----------------------------------------';
SELECT Id, UserName, Email, EmailConfirmed, LockoutEnabled
FROM AspNetUsers 
WHERE UserName = 'cajero';
PRINT '';

-- 3. Verificar rol asignado al cajero
PRINT '3?? ROLES DEL USUARIO CAJERO:';
PRINT '----------------------------------------';
SELECT 
    u.UserName as 'Usuario',
    u.Email,
    r.Name as 'Rol'
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.UserName = 'cajero';
PRINT '';

-- 4. CORRECCIÓN: Asignar rol CAJERO si no lo tiene
PRINT '4?? CORRECCIÓN AUTOMÁTICA (si es necesario):';
PRINT '----------------------------------------';

DECLARE @CajeroUserId NVARCHAR(450);
DECLARE @CajeroRoleId NVARCHAR(450);
DECLARE @IsAssigned INT;

-- Obtener ID del usuario cajero
SELECT @CajeroUserId = Id FROM AspNetUsers WHERE UserName = 'cajero';

-- Obtener ID del rol CAJERO
SELECT @CajeroRoleId = Id FROM AspNetRoles WHERE NormalizedName = 'CAJERO';

IF @CajeroUserId IS NULL
BEGIN
    PRINT '? ERROR: Usuario cajero NO existe';
    PRINT '   Solución: Ejecuta la aplicación para que el DataSeeder lo cree';
    PRINT '';
END
ELSE IF @CajeroRoleId IS NULL
BEGIN
    PRINT '? ERROR: Rol CAJERO NO existe';
  PRINT '   Solución: Ejecuta la aplicación para que el DataSeeder lo cree';
    PRINT '';
END
ELSE
BEGIN
    -- Verificar si ya tiene el rol asignado
    SELECT @IsAssigned = COUNT(*) 
    FROM AspNetUserRoles 
    WHERE UserId = @CajeroUserId AND RoleId = @CajeroRoleId;

 IF @IsAssigned = 0
    BEGIN
        -- Asignar el rol
        INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES (@CajeroUserId, @CajeroRoleId);
        PRINT '? Rol CAJERO asignado correctamente al usuario cajero';
    END
    ELSE
    BEGIN
    PRINT '? Usuario cajero YA tiene el rol CAJERO';
    END
    PRINT '';
END
GO

-- 5. VERIFICACIÓN FINAL
PRINT '5?? RESUMEN DE USUARIOS Y ROLES:';
PRINT '----------------------------------------';
SELECT 
  u.UserName as 'Usuario',
    u.Email,
    STRING_AGG(r.Name, ', ') as 'Roles'
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.UserName IN ('admin', 'cajero')
GROUP BY u.UserName, u.Email;
GO

PRINT '';
PRINT '========================================';
PRINT '      CREDENCIALES DE ACCESO   ';
PRINT '========================================';
PRINT '';
PRINT '?? ADMINISTRADOR:';
PRINT '   Usuario: admin';
PRINT '   Contraseña: admin123';
PRINT '   Acceso a: Todos los módulos';
PRINT '';
PRINT '?? CAJERO:';
PRINT '   Usuario: cajero';
PRINT '   Contraseña: cajero123';
PRINT '   Acceso a: Ventas, Detalles de Venta, Facturas';
PRINT '';
PRINT '========================================';
GO
