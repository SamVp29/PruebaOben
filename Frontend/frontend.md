PROYECTO PRUEBAOBEN - FRONTEND
=============================

Documento: 04-Frontend.txt

1. ESTRUCTURA
-------------
Frontend/
+-- PruebaOben/
+-- PruebaOben.Shared/
+-- PruebaOben.Web/

2. TECNOLOGÍAS
--------------
- .NET MAUI.
- Blazor Hybrid.
- Blazor Web.
- Razor Components.
- MudBlazor.

3. PruebaOben
-------------
Proyecto .NET MAUI.

Responsabilidad:
Proporcionar la aplicación nativa.

Destinos previstos:
- Windows.
- Android.
- APK.

MAUI utiliza Blazor mediante BlazorWebView para la experiencia híbrida.

4. PruebaOben.Shared
--------------------
Contendrá componentes Razor reutilizables entre Web y MAUI.

Objetivo:
Evitar duplicación de componentes visuales.

Componentes previstos:
- Login.
- Dashboard.
- Usuarios.
- Auditoría.

5. PruebaOben.Web
-----------------
Proyecto encargado de la aplicación Web.

Se ejecuta mediante ASP.NET Core y se accede desde navegador mediante
localhost.

6. ESTADO ACTUAL
----------------
[OK] Crear proyecto PruebaOben.
[OK] Crear proyecto PruebaOben.Shared.
[OK] Crear proyecto PruebaOben.Web.
[OK] Configurar HTTPS.
[OK] Ejecutar aplicación Web correctamente.
[OK] Comprobación inicial del proyecto MAUI.

La implementación funcional del Frontend todavía no ha comenzado.

7. MUD BLAZOR
------------
MudBlazor está previsto para:
- Formularios.
- Tablas.
- Botones.
- Diálogos.
- Navegación.
- Dashboard.

Todavía no se marca como implementado hasta realizar la configuración.

8. FUNCIONALIDADES PREVISTAS
----------------------------
LOGIN:
- Usuario/contraseña.
- Consumo del endpoint de login.
- Manejo del token.
- Protección de rutas.

DASHBOARD:
- Resumen de información.
- Indicadores.
- Navegación.

GESTIÓN DE USUARIOS:
- Listado.
- Crear.
- Editar.
- Desactivar.
- Restaurar cuando corresponda.

AUDITORÍA:
- Listado de cambios.
- Usuario afectado.
- Usuario que realizó el cambio.
- Acción.
- Campo.
- Valor anterior.
- Valor nuevo.
- Fecha.

9. CONSUMO DE API
-----------------
Web y MAUI utilizarán HTTP/JSON para comunicarse con PruebaOben.Api.

No se conectarán directamente a SQL Server.

El Backend ya expone `GET /api/audit?page=1&pageSize=50`, protegido por
JWT. La API devuelve los IDs del usuario afectado y del actor junto con
los valores auditados; no incluye nombres históricos. La integración de
esta ruta con la pantalla de auditoría sigue pendiente en el Frontend.

Flujo:
Frontend
   |
   | HTTP / JSON
   v
API REST
   |
   v
Application
   |
   v
Infrastructure
   |
   v
SQL Server

10. ANDROID
-----------
Objetivo final:
Generar una aplicación Android y un APK.

Destinos de prueba:
- Android Emulator.
- Dispositivo Android.

La generación final del APK se realizará después de completar la
funcionalidad principal.

11. CHECKLIST FRONTEND
----------------------
ESTRUCTURA
[OK] Crear proyecto PruebaOben.
[OK] Crear proyecto Shared.
[OK] Crear proyecto Web.
[OK] HTTPS.
[OK] Ejecutar Web.
[OK] Comprobar inicialmente MAUI.

UI
[ ] Instalar/configurar MudBlazor.
[ ] Crear layout.
[ ] Crear navegación.
[ ] Crear Login.
[ ] Crear Dashboard.
[ ] Crear gestión de usuarios.
[ ] Crear pantalla de auditoría.

API
[ ] Crear cliente HTTP.
[ ] Configurar URL de API.
[ ] Consumir Login.
[ ] Consumir GET users.
[ ] Consumir GET user por ID.
[ ] Consumir POST users.
[ ] Consumir PUT users.
[ ] Consumir DELETE lógico.
[ ] Consumir GET /api/audit con paginación.
[ ] Manejar errores HTTP.
[ ] Manejar token/JWT.

WEB
[ ] Integrar componentes Shared.
[ ] Probar flujo Web -> API.

MAUI
[ ] Integrar componentes Shared.
[ ] Probar flujo MAUI -> API.
[ ] Probar Android Emulator.
[ ] Probar dispositivo Android.
[ ] Generar APK.

12. SUSTENTACIÓN
----------------
¿Por qué Web y MAUI usan el mismo Backend?
"Para evitar duplicar lógica de negocio y mantener las mismas reglas
para todos los clientes. Son diferentes interfaces, pero consumen el
mismo contrato HTTP."

¿Por qué Shared?
"Para reutilizar componentes Razor entre Web y MAUI y evitar duplicar
la interfaz."

¿El Frontend se conecta directamente a SQL Server?
"No. Consume la API REST. El acceso a SQL Server está encapsulado en
Infrastructure del Backend."

13. PRÓXIMO PASO
----------------
El Frontend funcional se comenzará después de completar la base del
Backend necesaria para consumir la API.
