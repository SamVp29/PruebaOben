PROYECTO PRUEBAOBEN - ARQUITECTURA GENERAL
============================================

Documento general del proyecto.
Los detalles de cada área se encuentran en:
- Database/database.md
- Backend/backend.md
- Frontend/frontend.md

1. OBJETIVO
-----------
Desarrollar una aplicación empresarial utilizable desde Web y Android,
con un único Backend REST y una única fuente de datos SQL Server.

Funcionalidades previstas:
- Login.
- Dashboard.
- Gestión de usuarios.
- Historial / Auditoría de cambios.

2. TECNOLOGÍAS
--------------
Frontend:
- Blazor.
- Blazor Hybrid.
- .NET MAUI.
- MudBlazor.

Backend:
- C#.
- ASP.NET Core Web API.
- Microsoft.Data.SqlClient.
- SQL Server.

Persistencia:
- SQL directo.
- SqlConnection.
- SqlCommand.
- Consultas parametrizadas.
- Sin Entity Framework Core.

Arquitectura:
- Arquitectura modular por capas.
- Principios de Clean Architecture.
- Principios de Domain-Driven Design (DDD).
- SOLID.
- API REST.

Herramientas:
- Visual Studio.
- Git.
- Swagger / OpenAPI.
- SQL Server Management Studio (SSMS).

3. ESTRUCTURA DE LA SOLUCIÓN
----------------------------
PruebaOben/
|
+-- Frontend/
|   +-- PruebaOben/
|   +-- PruebaOben.Shared/
|   +-- PruebaOben.Web/
|
+-- Backend/
|   +-- PruebaOben.Domain/
|   +-- PruebaOben.Application/
|   +-- PruebaOben.Infrastructure/
|   +-- PruebaOben.Api/
|
+-- Database/
|   +-- PruebaOben.Database/
|
+-- PruebaOben.sln

4. ARQUITECTURA GENERAL
-----------------------
Se utiliza una arquitectura modular por capas inspirada en DDD y Clean
Architecture. No se utilizan microservicios.

Flujo:
Web / MAUI
    |
    | HTTP / REST
    v
PruebaOben.Api
    |
    v
PruebaOben.Application
    |
    v
PruebaOben.Domain
    ^
    |
PruebaOben.Infrastructure
    |
    v
SQL Server
    |
    v
Triggers
    |
    v
auditLogs

5. RESPONSABILIDADES
--------------------
Domain: núcleo del dominio, entidades, reglas y contratos.

Application: casos de uso, DTOs, servicios y validaciones. No ejecuta
SQL directamente.

Infrastructure: conexiones, SqlConnection, SqlCommand, repositorios,
consultas SQL y demás detalles técnicos externos.

API: entrada HTTP, controllers, endpoints, configuración, autenticación
y Swagger. No debe contener SQL ni lógica de negocio.

Frontend: interfaces Web y MAUI que consumen la API.

Database: definición/versionado de la base de datos mediante SQL Server
Database Project.

6. DIRECCIÓN DE DEPENDENCIAS
----------------------------
Application -> Domain
Infrastructure -> Domain
Api -> Application
Api -> Infrastructure

No se debe crear:
Domain -> Infrastructure
Domain -> API
Domain -> SQL Server
Application -> Infrastructure
Application -> UI

7. PRINCIPIOS
-------------
DDD: se utiliza como inspiración para modelar y separar el dominio.
No se afirma una implementación completa de DDD.

Clean Architecture: separación de responsabilidades y dependencias
orientadas hacia el núcleo.

SOLID:
- SRP: cada componente tiene una responsabilidad principal.
- DIP: Application trabaja con abstracciones como IUserRepository.

DRY: evitar duplicación innecesaria.
KISS: mantener soluciones sencillas para el alcance de la prueba.
YAGNI: no agregar complejidad que no sea necesaria.

8. PERSISTENCIA
---------------
Se utiliza SQL puro mediante Microsoft.Data.SqlClient.

C#
 |
 v
Microsoft.Data.SqlClient
 |
 v
SqlConnection / SqlCommand
 |
 v
SQL Server

No se utilizará Entity Framework Core.

9. API REST PREVISTA
--------------------
GET    /api/users
GET    /api/users/{id}
POST   /api/users
PUT    /api/users/{id}
DELETE /api/users/{id}
POST   /api/auth/login
GET    /api/audit?page=1&pageSize=50

Un endpoint solo se considera implementado cuando se haya creado y
probado.

10. EJECUCIÓN
-------------
Web:
- Seleccionar PruebaOben.Web como inicio.
- Ejecutar.
- Se abre el navegador mediante localhost.

MAUI:
- Seleccionar PruebaOben como inicio.
- Seleccionar Windows, Android Emulator o dispositivo Android.
- Ejecutar.

API:
- Seleccionar PruebaOben.Api.
- Ejecutar.
- Swagger se utiliza para probar endpoints.

11. COMANDOS BÁSICOS
--------------------
GIT
---
git status
Muestra cambios y archivos pendientes.

git add .
Prepara cambios para commit.

git commit -m "mensaje"
Crea un punto de cambio en el historial local.

git log --oneline
Muestra historial resumido.

git branch
Muestra ramas locales.

git switch nombre-rama
Cambia de rama.

git pull
Descarga e integra cambios remotos.

git push
Sube commits al remoto.

Antes de add/commit/push se recomienda ejecutar git status.

.NET
----
dotnet --info
dotnet --list-sdks
dotnet --list-runtimes
dotnet restore
dotnet build
dotnet clean
dotnet run

SOLUCIÓN
--------
dotnet sln list
dotnet sln add <proyecto.csproj>
dotnet add <proyecto.csproj> reference <otro.csproj>

Estos comandos son referencia; no todos implican que ya hayan sido
utilizados en esta etapa.

12. DECISIONES IMPORTANTES
--------------------------
- Un único Backend para Web y Android.
- SQL Server como base de datos.
- SQL puro en lugar de Entity Framework Core.
- Arquitectura modular por capas.
- Separación Frontend / Backend / Database.
- Auditoría mediante triggers.
- Eliminación lógica mediante active + deletedAt.
- PasswordHash no se expone mediante DTO de respuesta.
- CQRS no se utiliza porque el alcance actual no necesita separar
  modelos de lectura y escritura.

13. ESTADO GENERAL
-----------------
[OK] Solución en blanco.
[OK] Estructura Frontend / Backend / Database.
[OK] Proyectos Frontend creados.
[OK] Proyectos Backend creados.
[OK] Database Project creado.
[OK] Base de datos y auditoría preparadas.
[OK] Referencias entre proyectos configuradas.
[OK] Acceso Backend -> SQL Server preparado.
[OK] Swagger funcionando.
[OK] API de gestión de usuarios, autenticación JWT y consulta paginada
de auditoría implementadas.
[OK] Prueba de API/SQL local: CRUD de usuarios, actores de auditoría y
eliminación física auditable; migración aplicada a PruebaOben local.
[EN PROCESO] Prueba de flujo de login/CRUD desde Web con credenciales
de demostración y prueba Android en emulador/dispositivo.
[OK] Frontend compartido implementado para Web y MAUI con MudBlazor.
[OK] Hosts Web, MAUI Windows y Android compilan; APK Android generado.
[EN PROCESO] Configuración de URL HTTPS y firma Release definitivas
antes de distribuir el APK.

El detalle de arquitectura, ejecución, verificación y temas pendientes
del frontend está en Frontend/frontend.md.

14. REGLA DE DOCUMENTACIÓN
--------------------------
Cada tarea importante debe registrar:

QUÉ: qué se implementó.
PARA QUÉ: qué problema resuelve.
POR QUÉ: por qué se eligió.
CÓMO: cómo funciona.
SUSTENTACIÓN: cómo explicarlo en la prueba técnica.

Los checklists detallados NO se duplican aquí; están en cada documento
específico.
