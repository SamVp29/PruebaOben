PROYECTO PRUEBAOBEN - DATABASE
==============================

Documento: 02-Database.txt

1. TECNOLOGÍA
-------------
- SQL Server.
- SQL Server Management Studio (SSMS).
- Visual Studio SQL Server Database Project.
- SQL directo.
- Microsoft.Data.SqlClient desde C#.

No se utiliza Entity Framework Core.

2. DATABASE PROJECT
-------------------
Proyecto: PruebaOben.Database
Ubicación: Database/PruebaOben.Database/

Estructura:
PruebaOben.Database/
+-- Tables/
|   +-- users.sql
|   +-- auditLogs.sql
+-- Triggers/
    +-- TR_Audit_Users_Insert.sql
    +-- TR_Audit_Users_Update.sql
    +-- TR_Audit_Users_Delete.sql

El Database Project contiene la definición/versionado. La base física
está en SQL Server.

3. PUBLICACIÓN
--------------
Servidor: SAMVP\SQLEXPRESS
Base: PruebaOben
Autenticación: Integrated Security

Configuración relevante:
- Always recreate database: desactivado.
- Block incremental deployment if data loss possible: activado.
- Allow table recreation on publish: desactivado.

El proyecto fue publicado correctamente.

4. TABLA USERS
--------------
Campos:
- id INT IDENTITY PRIMARY KEY
- username NVARCHAR(50) NOT NULL UNIQUE
- fullname NVARCHAR(150) NOT NULL
- email NVARCHAR(150) NOT NULL UNIQUE
- passwordHash NVARCHAR(255) NOT NULL
- rol NVARCHAR(50) NOT NULL
- active BIT NOT NULL DEFAULT 1
- createdAt DATETIME2 NOT NULL DEFAULT SYSDATETIME()
- UpdatedAt DATETIME2 NULL
- deletedAt DATETIME2 NULL

active = estado operativo del usuario.
deletedAt = marca de eliminación lógica.

5. ELIMINACIÓN LÓGICA
---------------------
UPDATE dbo.users
SET
    active = 0,
    deletedAt = SYSDATETIME(),
    UpdatedAt = SYSDATETIME()
WHERE id = @id;

Los listados normales utilizan deletedAt IS NULL.

Restauración:
UPDATE dbo.users
SET
    active = 1,
    deletedAt = NULL,
    UpdatedAt = SYSDATETIME()
WHERE id = @id;

6. TABLA AUDITLOGS
------------------
Campos:
- id BIGINT IDENTITY PRIMARY KEY
- userId INT NULL
- accion NVARCHAR(20)
- entidad NVARCHAR(100)
- entidadId INT NULL
- nombreCampo NVARCHAR(100) NULL
- valorAnterior NVARCHAR(MAX) NULL
- valorNuevo NVARCHAR(MAX) NULL
- cambioRealizado INT NULL
- cambioAt DATETIME2 DEFAULT SYSDATETIME()

userId = usuario afectado.
cambioRealizado = usuario que ejecutó la operación.
entidad = entidad afectada.
entidadId = ID afectado.
nombreCampo = campo modificado.
valorAnterior / valorNuevo = valores antes/después.
accion = INSERT / UPDATE / DELETE.
cambioAt = fecha y hora.

7. FOREIGN KEY Y DELETE FÍSICO
------------------------------
Inicialmente auditLogs.userId tiene FK hacia users.id.

Se comprobó que un DELETE físico puede ser bloqueado por:
FK_AuditLogs_User.

Esto es comportamiento de integridad referencial, no un error del
trigger.

Si el requisito final exige DELETE físico conservando auditoría,
se puede retirar la FK y conservar userId como referencia histórica.

Retirar:
ALTER TABLE dbo.auditLogs
DROP CONSTRAINT FK_AuditLogs_User;

Restaurar:
ALTER TABLE dbo.auditLogs
ADD CONSTRAINT FK_AuditLogs_User
    FOREIGN KEY (userId)
    REFERENCES dbo.users(id);

Antes de restaurarla se deben comprobar posibles registros huérfanos.

8. TRIGGERS
-----------
Tres triggers independientes:
- TR_Audit_Users_Insert
- TR_Audit_Users_Update
- TR_Audit_Users_Delete

INSERT:
AFTER INSERT. Utiliza inserted y registra la creación.

UPDATE:
AFTER UPDATE. Utiliza inserted y deleted, compara los campos:
username, fullname, email, rol, active y deletedAt.
Registra únicamente los campos modificados mediante CROSS APPLY + VALUES.

Si deletedAt pasa de NULL a valor, la acción se registra como DELETE,
porque representa borrado lógico.

DELETE:
AFTER DELETE. Utiliza la pseudo-tabla deleted para obtener las filas
eliminadas físicamente.

9. PASSWORDHASH
---------------
passwordHash no se registra como valorAnterior ni valorNuevo.

Razón: el historial no necesita exponer información de credenciales.

10. SESSION_CONTEXT
-------------------
SQL Server no conoce automáticamente el usuario autenticado de la
aplicación.

Flujo previsto:
Usuario inicia sesión
    -> API identifica usuario
    -> C# establece SESSION_CONTEXT('UserId')
    -> INSERT/UPDATE/DELETE
    -> Trigger
    -> auditLogs.cambioRealizado

Como existe connection pooling, el contexto debe establecerse en la
misma conexión antes de ejecutar la operación.

11. PRUEBAS REALIZADAS
----------------------
INSERT:
Se creó un usuario de prueba y se generó auditoría INSERT.

UPDATE:
Se modificó fullname y se generó auditoría del campo modificado.

BORRADO LÓGICO:
Se modificaron active y deletedAt. Se generaron registros con acción
DELETE para los campos modificados.

RESTAURACIÓN:
Se volvió active a 1 y deletedAt a NULL. Debe registrarse como UPDATE.

DELETE FÍSICO:
Se probó y fue bloqueado por la FK FK_AuditLogs_User.

12. LIMPIAR DATOS DE PRUEBA
---------------------------
Para eliminar filas de auditLogs manteniendo estructura:
DELETE FROM dbo.auditLogs;

DROP TABLE elimina la tabla y su estructura.
TRUNCATE vacía y reinicia identity, pero tiene restricciones con FKs.

13. SUSTENTACIÓN
----------------
¿Por qué triggers?
"Porque determinadas operaciones quedan auditadas en SQL Server
independientemente del punto desde el que se ejecute el SQL."

¿Por qué no auditar PasswordHash?
"Porque el historial no necesita exponer información de credenciales."

¿Por qué active y deletedAt?
"active representa estado operativo y deletedAt representa borrado
lógico. Así puedo distinguir un usuario deshabilitado de uno eliminado
lógicamente."

14. CHECKLIST DATABASE
---------------------
[OK] Crear base de datos PruebaOben.
[OK] Crear SQL Server Database Project.
[OK] Crear tabla users.
[OK] Crear tabla auditLogs.
[OK] PK de users.
[OK] UNIQUE username.
[OK] UNIQUE email.
[OK] active.
[OK] UpdatedAt.
[OK] deletedAt.
[OK] Trigger INSERT.
[OK] Trigger UPDATE.
[OK] Trigger DELETE.
[OK] Publicar Database Project.
[OK] Probar INSERT.
[OK] Probar UPDATE.
[OK] Probar borrado lógico.
[OK] Probar restauración.
[OK] Comprobar DELETE físico con FK.
[ ] Integrar SESSION_CONTEXT desde C# para identificar al actor.
[PARCIAL] La API lee auditLogs mediante GET /api/audit; falta verificar
INSERT/UPDATE/DELETE originados desde Backend y el actor en
cambioRealizado.
[ ] Definir comportamiento definitivo de DELETE físico.
[ ] Limpiar datos de prueba antes de demostración final.

15. PRÓXIMOS PASOS
------------------
- Integrar SESSION_CONTEXT con Backend.
- Probar INSERT/UPDATE/DELETE iniciados por la API y verificar
  cambioRealizado.
- Validar flujo completo usuario + auditoría.
