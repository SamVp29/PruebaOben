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
La FK FK_AuditLogs_User utiliza ON DELETE SET NULL. Al eliminar
físicamente un usuario:
- Los registros de auditoría anteriores se conservan.
- auditLogs.userId pasa a NULL porque la fila relacionada ya no existe.
- auditLogs.entidadId mantiene el ID histórico del usuario.
- El trigger registra id, username y fullname anteriores, sin email ni
  passwordHash.
- cambioRealizado conserva el ID del actor de SESSION_CONTEXT.

La auditoría no se elimina en cascada: debe sobrevivir al usuario para
cumplir su propósito. La ruta de eliminación física está protegida para
el rol Admin.

Para actualizar una base existente, ejecutar
Database/Migrations/20260926_UserPhysicalDeleteAudit.sql antes de usar
la acción de eliminación física.

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
AFTER DELETE. Usa deleted para registrar id, username y fullname
anteriores de cada usuario borrado físicamente. userId del evento queda
NULL y entidadId conserva el ID eliminado.

9. PASSWORDHASH
---------------
passwordHash no se registra como valorAnterior ni valorNuevo.

Razón: el historial no necesita exponer información de credenciales.

10. SESSION_CONTEXT
-------------------
SQL Server no conoce automáticamente el usuario autenticado de la
aplicación.

Flujo:
Usuario inicia sesión
    -> API identifica usuario
    -> API obtiene el claim sub del JWT
    -> C# establece SESSION_CONTEXT('UserId') en la conexión de escritura
    -> INSERT/UPDATE/DELETE
    -> Trigger
    -> auditLogs.cambioRealizado

Cada escritura establece el contexto en la misma conexión, justo antes
del DML. No depende del valor anterior de una conexión reciclada.

11. PRUEBAS REALIZADAS
----------------------
INSERT:
Se validó una inserción de usuario con un actor en SESSION_CONTEXT y el
trigger registró cambioRealizado.

UPDATE:
Se modificó fullname y active desde la API; el trigger registró los
campos modificados y el actor autenticado.

BORRADO LÓGICO:
La API actualizó active/deletedAt y el trigger produjo eventos de
eliminación lógica con el actor autenticado.

RESTAURACIÓN:
Se volvió active a 1 y deletedAt a NULL. Debe registrarse como UPDATE.

DELETE FÍSICO:
La migración se aplicó en la base local. Una prueba por API creó,
actualizó, eliminó lógicamente y borró físicamente un usuario sintético.
La FK preservó el historial, entidadId conservó su ID, el trigger
registró ID/username/fullname y cambioRealizado recibió el actor que
ejecutó cada operación. El endpoint devolvió 403 para rol User.
Los datos sintéticos de prueba se retiraron al finalizar.

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
[OK] FK preparada para conservar auditoría tras DELETE físico.
[OK] Integrar SESSION_CONTEXT desde C# para identificar al actor.
[OK] Aplicar la migración a la base local y verificar
INSERT/UPDATE/DELETE de API con el actor en cambioRealizado.
[OK] Definir comportamiento de DELETE físico y snapshot auditado.
[OK] Retirar filas sintéticas generadas para la prueba.

15. DESPLIEGUE EN OTROS ENTORNOS
--------------------------------
- La base local, sus triggers, SESSION_CONTEXT y la eliminación física
  con auditoría están implementados y probados.
- Antes de habilitar eliminación física en otra base, aplicar allí la
  migración Database/Migrations/20260926_UserPhysicalDeleteAudit.sql.
- La pantalla de auditoría con filtro por acción también está verificada
  contra la base local.

16. CÓDIGO PARA EXPLICAR EN LA SUSTENTACIÓN
-------------------------------------------
Ejemplo principal: cómo SQL Server registra los cambios de un usuario.
Los siguientes son fragmentos seleccionados; para ver las definiciones
completas, revisa los archivos indicados.

16.1 Tabla y restricciones
--------------------------
Archivo: PruebaOben.Database/Tables/user.sql

```sql
[id] INT IDENTITY(1,1) NOT NULL,
[username] NVARCHAR(50) NOT NULL,
[email] NVARCHAR(150) NOT NULL,
[active] BIT NOT NULL
    CONSTRAINT [DF_Users_Active] DEFAULT 1,
[deletedAt] DATETIME2 NULL,
CONSTRAINT [PK_Users] PRIMARY KEY ([id]),
CONSTRAINT [UQ_Users_Username] UNIQUE ([username]),
CONSTRAINT [UQ_Users_Email] UNIQUE ([email])
```

`IDENTITY` genera el ID; `PRIMARY KEY` identifica cada fila; `UNIQUE`
impide usernames y correos duplicados. `active` y `deletedAt` expresan
dos estados distintos: una cuenta puede estar inactiva sin haber sido
eliminada lógicamente.

16.2 Trigger de actualización
-----------------------------
Archivo: PruebaOben.Database/Triggers/TR_Audit_Users_Update.sql

```sql
FROM inserted AS i
INNER JOIN deleted AS d
    ON d.[id] = i.[id]
CROSS APPLY
(
    VALUES
        (N'fullname',
         CONVERT(NVARCHAR(MAX), d.[fullname]),
         CONVERT(NVARCHAR(MAX), i.[fullname])),
        (N'active',
         CONVERT(NVARCHAR(MAX), d.[active]),
         CONVERT(NVARCHAR(MAX), i.[active]))
) AS cambios ([nombreCampo], [valorAnterior], [valorNuevo])
WHERE ISNULL(cambios.[valorAnterior], N'')
   <> ISNULL(cambios.[valorNuevo], N'');
```

En un trigger `UPDATE`, `deleted` contiene los valores previos e
`inserted` los nuevos. El `CROSS APPLY` convierte cada campo auditado en
una fila comparable; el `WHERE` evita guardar campos que no cambiaron.
El trigger real incluye username, fullname, email, rol, active y
deletedAt. También clasifica el paso de `deletedAt` nulo a fecha como
acción `DELETE`, que en este proyecto significa borrado lógico.

Los triggers trabajan con conjuntos de filas: `inserted` y `deleted`
pueden contener más de una fila. Por eso la consulta relaciona ambas
tablas por ID y no supone que una operación afecte solo un usuario.

16.3 Identidad del actor
------------------------
El repositorio C# establece el actor antes del DML:

```sql
EXEC sys.sp_set_session_context
    @key = N'UserId',
    @value = @actorId;
```

El trigger lee ese valor y lo guarda como `cambioRealizado`:

```sql
TRY_CONVERT(INT, SESSION_CONTEXT(N'UserId'))
```

`SESSION_CONTEXT` pertenece a la conexión actual. Por eso el código
ejecuta la asignación y la escritura en el mismo comando/conexión. En
una eliminación física, el trigger usa la tabla lógica `deleted` para
conservar ID, username y nombre completo del usuario eliminado; no
incluye correo ni passwordHash.

Cómo contarlo:
"La estructura y las restricciones están declaradas en el Database
Project. Cuando la API escribe un cambio con SQL parametrizado, primero
establece el ID del actor en la sesión SQL. El trigger compara los
valores anteriores y nuevos y agrega a auditLogs solo los campos que
cambiaron. Así la auditoría se genera en SQL Server y conserva el actor
sin guardar el hash de la contraseña."

Preguntas que podrían hacer:
- ¿Qué representan `inserted` y `deleted`? Tablas lógicas que SQL Server
  expone al trigger con las filas nuevas y anteriores, respectivamente.
- ¿El trigger solo funciona con la API? No; se ejecuta ante cambios de
  la tabla sin importar el punto que ejecutó el DML, aunque el actor
  requiere que la conexión haya establecido `SESSION_CONTEXT`.
- ¿Por qué no guardar el passwordHash? No es necesario para explicar
  cambios y exponerlo aumentaría el riesgo de revelar información de
  credenciales.
- ¿Por qué no borrar auditoría junto con el usuario? La auditoría debe
  conservar la evidencia histórica; la FK usa `ON DELETE SET NULL` para
  el usuario relacionado y la fila auditada conserva el ID histórico.
