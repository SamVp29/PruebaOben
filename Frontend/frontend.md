PROYECTO PRUEBAOBEN - FRONTEND
=============================

Objetivo: construir una sola interfaz de usuario Razor con MudBlazor y
reutilizarla en navegador y en la aplicación nativa .NET MAUI mediante
Blazor Hybrid. Los hosts cambian; las páginas, componentes y estilos
compartidos no se duplican.

1. ARQUITECTURA
---------------
Frontend/
+-- PruebaOben/
|   +-- PruebaOben/          Host nativo MAUI + BlazorWebView.
|   +-- PruebaOben.Shared/   Páginas, componentes, estilos y cliente API.
|   +-- PruebaOben.Web/      Host ASP.NET Core Blazor Web interactivo.

Flujo de la misma interfaz:
Navegador -> PruebaOben.Web -> Razor Components interactivos
Android/Windows -> PruebaOben -> BlazorWebView
                                     |
                                     v
                           PruebaOben.Shared
                                     |
                                     | HTTP/JSON + JWT
                                     v
                            PruebaOben.Api

El Frontend nunca se conecta a SQL Server. Las páginas y los contratos
HTTP comunes viven en PruebaOben.Shared. Cada host registra esa UI y
provee únicamente sus servicios específicos de plataforma.

2. TECNOLOGÍAS Y DECISIONES
---------------------------
- .NET 10, Razor Components y C#.
- MudBlazor 9.10.0 para controles, formularios, tablas, navegación,
  iconos, indicadores y componentes de carga.
- CSS propio compartido para la identidad visual y el comportamiento
  adaptable a móvil.
- HttpClient + System.Net.Http.Json para consumir la API REST.
- Sin Entity Framework, ORM, AutoMapper ni conexiones SQL en el
  Frontend.
- Web usa Interactive Server. Android y MAUI Windows usan BlazorWebView.
- En Web el servidor llama a la API; en MAUI el cliente nativo llama a
  la API.

Sustentación:
"Mantengo las páginas en un solo proyecto Razor compartido. Web las
presenta dentro del host ASP.NET Core y MAUI las presenta dentro de un
BlazorWebView. Así comparto la interfaz y el cliente HTTP, pero cada
host configura la URL de la API y el almacenamiento seguro que le
corresponde."

3. HOSTS
--------
3.1 PruebaOben.Shared
---------------------
Contiene:
- Routes.razor y ProtectedRouteView.razor para resolver rutas y enviar
  a login a quien no tiene una sesión válida en la UI.
- Layout/MainLayout.razor con barra superior, navegación y cierre de
  sesión.
- Pages/Login.razor.
- Pages/Home.razor, el dashboard.
- Pages/Users.razor.
- Pages/Audit.razor.
- Services/ApiClient.cs, modelos HTTP, estado de autenticación y
  almacenamiento de token web.
- Services/AppTheme.cs y wwwroot/app.css para tema y estilos compartidos.

3.2 PruebaOben.Web
------------------
Host ASP.NET Core que activa Interactive Server y añade las rutas del
assembly Shared. La interactividad está configurada sin prerenderizar
las páginas, para que el almacenamiento de sesión del navegador esté
disponible al consultar el estado de autenticación.

La UI usa sessionStorage para conservar el JWT durante la sesión actual
del navegador. El HttpClient se ejecuta en el servidor Web y agrega el
JWT al llamar a la API.

3.3 PruebaOben (MAUI)
---------------------
Host nativo que carga el mismo componente Shared/Routes dentro de
BlazorWebView. El JWT se almacena con SecureStorage de MAUI, no en SQL
ni en archivos planos de la aplicación.

Destinos configurados:
- Android (API mínima 24).
- Windows.
- iOS y MacCatalyst están declarados en el proyecto, pero no se
  compilaron ni probaron en este entorno Windows.

4. FUNCIONALIDADES
------------------
LOGIN
- POST /api/auth/login con correo y contraseña.
- Recibe el JWT y lo guarda en el almacenamiento propio del host.
- El proveedor de autenticación lee los claims y la expiración para
  controlar la navegación visual.
- 401 se presenta como credenciales incorrectas o sesión vencida.
- El servidor de API sigue siendo quien valida firma y expiración; la
  protección de rutas del cliente no reemplaza la seguridad del API.

RESUMEN
- Cuenta los usuarios visibles que entrega GET /api/users.
- Cuenta los activos usando el campo active.
- Muestra el total de eventos desde GET /api/audit y los cinco eventos
  más recientes.
- La actividad reciente conserva una fila de encabezados y filas de datos
  en Web y MAUI; en pantallas estrechas la tabla permite desplazamiento
  horizontal en vez de repetir los nombres de columna en cada registro.
- No hay endpoint de estadísticas: los indicadores se calculan con las
  respuestas existentes.

USUARIOS
- GET /api/users y búsqueda local por nombre, username o email.
- POST /api/users para crear con username, nombre, email, contraseña y
  rol.
- PUT /api/users/{id} para actualizar campos y estado activo.
- DELETE /api/users/{id} para borrado lógico.
- GET /api/users?includeDeleted=true para que Admin consulte usuarios
  eliminados lógicamente y pueda completar su eliminación permanente;
  el API rechaza este parámetro para otros roles.
- Crear y editar se realizan en un diálogo modal; desactivar pide
  confirmación.
- El menú de acciones ofrece activar/desactivar, eliminar lógicamente
  y, solo para el rol Admin, eliminar permanentemente.
- "Desactivar cuenta" usa PUT /api/users/{id} con active=false; la cuenta
  sigue visible como inactiva y deletedAt no se modifica.
- "Eliminar usuario" llama a DELETE /api/users/{id}; el Backend realiza
  el borrado lógico. Para Admin, el usuario permanece en el directorio
  marcado como "Eliminado lógicamente", sin permitir edición ni otro
  borrado lógico, y con la opción de borrado permanente disponible.
- En pantallas estrechas, el directorio se muestra como tarjetas de
  usuario en lugar de una tabla ancha; la búsqueda y las acciones se
  mantienen disponibles en cada tarjeta.
- El listado normal de usuarios sigue excluyendo eliminados lógicos;
  solo el listado extendido administrativo los incluye. Tras eliminar
  permanentemente, el registro sale del directorio.
- "Eliminar permanentemente" requiere rol Admin y una confirmación que
  advierte que solo quedarán en auditoría ID, username y fullname.
- Después de crear, editar o desactivar, la pantalla actualiza solo el
  usuario afectado en memoria; al eliminar, lo retira del listado. No
  vuelve a cargar la lista ni navega para refrescarla.
- No hay acción de restaurar ni cambio de contraseña porque el Backend
  aún no expone esas operaciones.

En Web, Interactive Server mantiene la sesión interactiva abierta y
envía al navegador los cambios de interfaz como diferencias del DOM.
Esto no equivale a recargar toda la página. El DOM sigue siendo la
representación HTML normal del navegador; Blazor aplica las
actualizaciones puntuales cuando cambia el estado de los componentes.

AUDITORÍA
- GET /api/audit?page={page}&pageSize={pageSize}.
- Paginación con 25, 50 o 100 filas.
- Presenta acción, entidad/campo, usuario afectado, actor, valores y
  fecha.
- El actor/usuario se muestra como ID porque auditLogs no guarda una
  copia histórica general de sus nombres. Los eventos de eliminación
  física sí conservan ID, username y fullname. Si cambioRealizado es
  NULL, se indica "No informado"; las filas históricas anteriores no
  tienen actor retroactivo. Para auditoría de un usuario borrado
  físicamente, la interfaz usa entidadId como ID afectado.
- La ruta permite cualquier usuario autenticado, según la decisión
  tomada para el alcance actual.

5. DISEÑO Y ESTILOS
-------------------
Tema claro centralizado en Services/AppTheme.cs:
- Primario azul profundo: #173B57.
- Secundario turquesa: #1D8A8A.
- Fondo gris claro: #F3F6F8.
- Superficies blancas y bordes suaves.
- Verde, ámbar y rojo para estados y acciones.

Los estilos globales se encuentran en wwwroot/app.css dentro del
proyecto Shared. Definen tipografía del sistema, tarjetas, espaciado,
tablas, estados vacíos, formularios y breakpoints para pantallas
pequeñas. Los dos hosts cargan el mismo CSS y los recursos estáticos de
MudBlazor.

El layout usa navegación lateral en escritorio y navegación adaptable
de MudBlazor en tamaños pequeños. Tablas MudTable cambian a presentación
responsive para que sus filas se puedan leer en móvil.

6. CONFIGURACIÓN DE LA API
--------------------------
WEB
La dirección está en `Api:BaseAddress` de
PruebaOben.Web/appsettings.json. Se puede sustituir por configuración
de entorno:

PowerShell:
$env:Api__BaseAddress = "https://localhost:7250/"

MAUI DEBUG
La configuración se lee desde Resources/Raw/api-config.Development.json:
- Android Emulator: `http://10.0.2.2:5083/`.
- Windows: `https://localhost:7250/`.

Para un dispositivo Android físico, `10.0.2.2` no sirve. Cambiar
AndroidBaseAddress a la dirección LAN de la computadora, por ejemplo
`http://192.168.1.20:5083/`, iniciar la API escuchando en esa interfaz y
mantener el dispositivo y el equipo en la misma red. El manifiesto
Android de Debug permite HTTP solo para facilitar desarrollo local.

INTERFAZ EN MÓVIL
- En Android, el app bar, el contenido y el menú lateral se desplazan
  debajo del área segura superior para que no se superpongan con la hora,
  los indicadores ni el recorte de pantalla.
- El directorio y la auditoría cambian a tarjetas compactas en pantallas
  estrechas para evitar columnas cortadas y desplazamiento horizontal.

MAUI RELEASE / APK
Resources/Raw/api-config.json se usa para Release. Reemplazar
`https://api.example.com/` por el host HTTPS real antes de distribuir y
volver a generar el APK. El manifiesto Release no permite HTTP sin
cifrar. No se configuró un servidor de producción para este proyecto.

7. COMPILAR Y EJECUTAR
----------------------
Restaurar y compilar Web:
dotnet restore .\Frontend\PruebaOben\PruebaOben.Web\PruebaOben.Web.csproj
dotnet build .\Frontend\PruebaOben\PruebaOben.Web\PruebaOben.Web.csproj

Ejecutar API y Web desde dos terminales. La API también requiere que
`Jwt__Key` esté configurada como variable de entorno.
dotnet run --project .\Backend\PruebaOben.Api\PruebaOben.Api.csproj
dotnet run --project .\Frontend\PruebaOben\PruebaOben.Web\PruebaOben.Web.csproj

Compilar MAUI Windows:
dotnet build .\Frontend\PruebaOben\PruebaOben\PruebaOben.csproj `
  -f net10.0-windows10.0.19041.0

Compilar MAUI Android Debug:
dotnet build .\Frontend\PruebaOben\PruebaOben\PruebaOben.csproj `
  -f net10.0-android -c Debug

Generar APK Release:
dotnet publish .\Frontend\PruebaOben\PruebaOben\PruebaOben.csproj `
  -f net10.0-android -c Release -p:AndroidPackageFormat=apk

Salida de APK generada:
Frontend/PruebaOben/PruebaOben/bin/Release/net10.0-android/publish/
com.companyname.pruebaoben-Signed.apk

El APK generado permite comprobar empaquetado y compilación. Antes de
distribución comercial se debe configurar un certificado de firma
Release propio y una URL HTTPS real. Para instalar/probar en un
dispositivo se requiere Android SDK Platform Tools (`adb`) y un
emulador o teléfono Android.

8. ESTADO Y VALIDACIÓN
----------------------
[OK] Una sola interfaz Razor en Shared, usada por Web y MAUI.
[OK] MudBlazor y tema/estilos compartidos.
[OK] Login, navegación, dashboard, usuarios y auditoría implementados.
[OK] JWT enviado a las rutas de API desde cada host.
[OK] Cliente HTTP común, con almacenamiento de token por plataforma.
[OK] Compila Web sin advertencias ni errores.
[OK] Compila MAUI Windows Debug sin advertencias ni errores.
[OK] Compila MAUI Android Debug sin advertencias ni errores.
[OK] Genera APK Android Release firmado para prueba.
[OK] Smoke test del navegador con una API simulada: login, dashboard,
     listado de usuarios y auditoría.
[OK] API real probada aparte: GET /api/audit devolvió datos de SQL
     Server con autenticación.
[OK] API real y SQL Server: creación, actualización, borrado lógico y
     eliminación física sintéticos con cambioRealizado por actor.
[OK] Rol User recibe 403 al intentar eliminación física.
[OK] Filas de prueba eliminadas tras verificar la auditoría.
[OK] La migración FK SET NULL y el trigger físico están aplicados en la
     base local PruebaOben.
[ ] No hay emulador ni adb disponibles en este entorno; no se pudo
     instalar/ejecutar el APK ni probar un dispositivo físico.
[ ] Falta probar el flujo completo de login/CRUD del Frontend contra la
     API real con credenciales de demostración.

9. LIMITACIONES CONOCIDAS
-------------------------
- Las rutas se protegen en la experiencia cliente y el Backend exige
  JWT. La validación de autorización real siempre corresponde a API.
- El JWT Web se conserva en sessionStorage del navegador. Mantener la
  aplicación protegida frente a XSS es importante porque JavaScript del
  mismo origen podría acceder a ese almacenamiento.
- La lista de auditoría contiene valores de campos, y el API permite
  leerlos a cualquier usuario autenticado.
- La eliminación física solo se muestra y permite para rol Admin. Las
  demás operaciones conservan el alcance de autorización existente.
- El endpoint de usuarios no devuelve updatedAt correctamente todavía;
  el resumen no depende de ese campo.
- No existe restauración de borrado lógico ni cambio de contraseña en
  los endpoints actuales.

10. SUSTENTACIÓN
----------------
¿Cómo logré una sola interfaz para Web y aplicación móvil?
"Implementé las páginas y el layout en PruebaOben.Shared. El host Web
las ejecuta como Razor Components interactivos y MAUI las presenta con
BlazorWebView. No mantengo una copia de las pantallas por plataforma."

¿Qué hace Blazor Hybrid?
"Usa componentes Razor dentro de una vista web integrada en una
aplicación nativa. La aplicación sigue siendo MAUI y puede usar
capacidades nativas como SecureStorage."

¿Qué aporta MudBlazor?
"Proporciona controles de interfaz listos, como formularios, tablas,
botones, navegación e iconos. Definí un tema MudBlazor y CSS propio
compartido para mantener una identidad visual consistente sin
reimplementar todos los controles."

¿El Frontend se conecta a SQL Server?
"No. Envía solicitudes HTTP/JSON a la API. La API es la única capa que
accede a SQL Server."

¿Cómo adapto el token a Web y MAUI?
"El contrato ITokenStore tiene implementaciones distintas. Web usa
sessionStorage y MAUI usa SecureStorage. El cliente HTTP compartido
adjunta el JWT como Bearer en las llamadas protegidas."

¿La protección de rutas del Frontend es suficiente?
"No. Solo mejora la navegación. La API valida el JWT y protege los
recursos; el cliente no es una frontera de seguridad."

¿Cómo maneja la auditoría los nombres del actor?
"El esquema guarda IDs, no nombres históricos. La interfaz presenta
esos IDs. El API toma el ID del claim sub del JWT, lo establece en
SESSION_CONTEXT en la misma conexión de escritura y el trigger lo guarda
en cambioRealizado. Para una eliminación física también conserva el ID,
username y nombre completo del usuario eliminado, pero no su correo ni
su contraseña."
