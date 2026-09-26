PROYECTO PRUEBAOBEN - BACKEND
============================

Documento: 03-Backend.txt

1. ESTRUCTURA
-------------
Backend/
+-- PruebaOben.Domain/
+-- PruebaOben.Application/
+-- PruebaOben.Infrastructure/
+-- PruebaOben.Api/

2. REFERENCIAS
--------------
Application -> Domain
Infrastructure -> Domain
Api -> Application
Api -> Infrastructure

No existe Application -> Infrastructure.

Esto es importante porque Application no debe conocer la implementación
concreta de acceso a datos. Application trabaja mediante interfaces
definidas en Domain.

Sustentación:

"Application depende de abstracciones y no de implementaciones concretas.
Por eso puede trabajar con IUserRepository sin conocer si los datos
vienen de SQL Server, otra base de datos o cualquier otra implementación."

3. DOMAIN
---------
Proyecto: PruebaOben.Domain
Tipo: Class Library

Estructura actual:
Domain/
+-- Entities/
|   +-- User.cs
+-- Interfaces/
    +-- IUserRepository.cs

3.1 User.cs
-----------
Entidad de dominio User con:
- Id
- Username
- FullName
- Email
- PasswordHash
- Rol
- Active
- CreatedAt
- UpdatedAt
- DeletedAt

La entidad representa al usuario dentro del dominio.

PasswordHash existe en la entidad para persistencia/autenticación, pero
no se expone al frontend mediante el DTO de respuesta.

Importante:

Las propiedades de User actualmente utilizan nombres en minúscula
porque esa es la convención que se está utilizando en este proyecto.

No se deben cambiar automáticamente a PascalCase si eso implica alterar
el código que ya está funcionando.

3.2 IUserRepository
-------------------
Contrato de acceso a datos:
- GetAllAsync()
- GetByIdAsync(int id)
- GetByEmailAsync(string email)
- CreateAsync(User user)
- UpdateAsync(User user)
- DeleteAsync(int id)

GetByEmailAsync fue agregado específicamente para autenticación.

¿Para qué sirve?
Define qué operaciones necesita la aplicación sin conocer cómo se
implementan técnicamente.
La interfaz no contiene SQL.
La implementación concreta se encuentra en Infrastructure.

Sustentación:
"Definí el contrato en Domain y la implementación concreta en
Infrastructure. Así Application depende de una abstracción y no de SQL."

4. INFRASTRUCTURE
-----------------
4.1 Microsoft.Data.SqlClient
Paquete instalado: 7.1.0.
Permite comunicación directa entre C# y SQL Server.

Flujo:
C#->Microsoft.Data.SqlClient->SqlConnection->SqlCommand->SQL Server

4.2 SqlConnectionFactory
------------------------
Archivo: Data/SqlConnectionFactory.cs

Responsabilidad: centralizar la creación de SqlConnection utilizando
la connection string.

¿Para qué sirve?
Evita que cada repositorio tenga que construir manualmente una conexión.
El repositorio solicita una conexión a la factory.

Sustentación:
"Centralicé la creación de conexiones para evitar duplicar la lógica
de construcción de SqlConnection en cada repositorio."

4.3 UserRepository
------------------
Archivo: Repositories/UserRepository.cs
Implementa IUserRepository.

Métodos implementados:
- GetAllAsync
- GetByIdAsync
- GetByEmailAsync
- CreateAsync
- UpdateAsync
- DeleteAsync

Utiliza:
- SqlConnection.
- SqlCommand.
- SqlDataReader.
- SQL parametrizado.
- async/await.

GetAllAsync: Obtiene los usuarios que no están eliminados lógicamente.

Utiliza: WHERE deletedAt IS NULL

GetByIdAsync:Obtiene un usuario específico mediante su id.

Utiliza:
WHERE id = @id
AND deletedAt IS NULL

GetByEmailAsync: Obtiene un usuario específico mediante su email.

Utiliza:
WHERE email = @email
AND deletedAt IS NULL
Esto es utilizado principalmente por AuthService durante el login.

CreateAsync:
Inserta un nuevo usuario y utiliza:
OUTPUT INSERTED
para recuperar los datos del registro creado.

UpdateAsync: Actualiza los campos permitidos del usuario y establece UpdatedAt.

DeleteAsync: Realiza borrado lógico. No elimina físicamente el registro.

Establece:
active = 0
deletedAt = SYSDATETIME()
UpdatedAt = SYSDATETIME()

¿Por qué borrado lógico?
Porque permite conservar el registro y su historial de auditoría,
evitando perder información.

SQL parametrizado:
Los valores se envían mediante parámetros como: @id, @email, @username
Esto evita construir SQL concatenando directamente valores recibidos
del usuario.

Sustentación: "Utilizo consultas parametrizadas para separar la instrucción SQL de los
valores recibidos y reducir riesgos de SQL Injection."

async/await: Las operaciones de base de datos se ejecutan de forma asíncrona para
no bloquear innecesariamente el hilo mientras se espera la respuesta
de SQL Server.

NOTA: En GetByIdAsync existe un uso de AddWithValue que será normalizado a
SqlDbType explícito para mantener un estilo consistente.

Ejemplo:
command.Parameters.Add("@id", SqlDbType.Int).Value = id;
Esto permite indicar explícitamente el tipo de dato que espera SQL Server.

5. API
------
Proyecto: PruebaOben.Api
Tipo: ASP.NET Core Web API
Target: .NET 10

Configurado:
- HTTPS.
- OpenAPI.
- Swagger.
- Swagger UI.

Paquete OpenAPI: Microsoft.AspNetCore.OpenApi 10.0.12.

5.1 Connection String
---------------------
Configurada en appsettings.json bajo ConnectionStrings:DefaultConnection.
Servidor: SAMVP\SQLEXPRESS
Base: PruebaOben
Integrated Security: True
TrustServerCertificate: True

5.2 Configuración JWT
------------------
Se eliminó la clave JWT hardcodeada del código fuente.
Se creó la interfaz IJwtSettings en Application para abstraer la configuración necesaria por AuthService.
La implementación JwtSettings se encuentra en API.
La clave se obtiene mediante la variable de entorno:Jwt__Key
key: "prueba_oben_uso_ddd_clean_arquitectura_2026
ASP.NET Core interpreta: Jwt__Key
como: Jwt:Key

Flujo: Variable de entorno-> Program.cs-> JwtSettings-> IJwtSettings-> AuthService-> Firma JWT

AuthService no conoce el origen de la clave.

Sustentación: "La clave JWT no está hardcodeada. Application define mediante IJwtSettings qué configuración necesita, mientras que API se encarga de obtener el secreto desde una variable de entorno y proporcionar esa dependencia mediante Inyección de Dependencias."


5.3 Dependency Injection
-------------------------
Registrado:SqlConnectionFactory como Singleton.
IUserRepository -> UserRepository como Scoped.

Significado:
Cuando una clase solicite IUserRepository, ASP.NET Core proporciona
UserRepository automáticamente.

¿Para qué sirve Dependency Injection?
Permite que las clases reciban sus dependencias desde el contenedor
de ASP.NET Core en lugar de crearlas directamente.

Ejemplo conceptual:
AuthService necesita IUserRepository.
En lugar de hacer: new UserRepository(...)
AuthService recibe: IUserRepository repository y ASP.NET Core resuelve la implementación registrada.

Sustentación:
"Uso Dependency Injection para que las clases reciban sus dependencias
desde el contenedor y no tengan que crearlas directamente. Esto reduce
el acoplamiento y facilita las pruebas y el mantenimiento."

5.4 UsersController
----------------------
Archivo: Api/Controllers/UsersController.cs
Responsabilidad: Exponer mediante HTTP las operaciones de gestión de usuarios.

Endpoints:
GET /api/users
Obtiene todos los usuarios.

GET /api/users/{id}
Obtiene un usuario por ID.

POST /api/users
Crea un nuevo usuario.

PUT /api/users/{id}
Actualiza un usuario.

DELETE /api/users/{id}
Realiza la eliminación lógica de un usuario.

Flujo: HTTP Request-> UsersController-> IUserService-> UserService-> IUserRepository-> UserRepository-> SQL Server

El Controller no contiene consultas SQL ni lógica de acceso a datos.

Respuestas utilizadas:
200 OK: Consulta exitosa.
201 Created: Usuario creado correctamente.
204 No Content: Actualización o eliminación realizada correctamente sin contenido de respuesta.
404 Not Found: El usuario solicitado no existe.
409 Conflict: Se intentó crear un usuario con un correo que ya existe.

Sustentación: "El Controller funciona como punto de entrada HTTP. Recibe las solicitudes, valida el flujo básico de la petición, llama al servicio correspondiente y transforma el resultado en una respuesta HTTP. La lógica de aplicación permanece en UserService y el acceso a datos en UserRepository."

5.5 AuthController
---------------------
Archivo: Api/Controllers/AuthController.cs
Responsabilidad: Exponer mediante HTTP el proceso de autenticación.

Endpoint: POST /api/auth/login

Recibe:LoginDto

Proceso: LoginDto-> AuthController-> IAuthService-> AuthService-> GetByEmailAsync()-> BCrypt.Verify()-> Claims-> JWT-> Token

Si las credenciales son incorrectas: 401 Unauthorized.
Si la autenticación es correcta: 200 OK con el JWT.

El Controller no genera directamente el JWT ni verifica la contraseña. Esa responsabilidad pertenece a AuthService.

Sustentación: "AuthController es el punto de entrada HTTP para la autenticación. Recibe las credenciales y delega el proceso en AuthService. Si la autenticación es correcta devuelve el JWT; si falla devuelve 401 Unauthorized."
Estado:Pendiente de prueba en Swagger.

6. APPLICATION
--------------
Application contiene los casos de uso y objetos de transferencia de
datos.

Estructura actual:
Application/
+-- DTOs/
| +-- UserResponseDto.cs
| +-- CreateUserDto.cs
| +-- UpdateUserDto.cs
| +-- LoginDto.cs
|
+-- Interfaces/
| +-- IAuthService.cs
| +-- IUserService.cs
|
+-- Services/
+-- AuthService.cs
+-- UserService.cs (pendiente)

DTO significa Data Transfer Object.
Sirve para controlar los datos que entran y salen de los casos de uso,
Los DTOs evitan exponer directamente las entidades del dominio.
También permiten definir diferentes estructuras para entrada y salida.

Ejemplo: La entidad User contiene: passwordHash
Pero UserResponseDto no contiene passwordHash.
Esto evita exponer información sensible al cliente.

6.1 UserResponseDto
-------------------
Define lo que la API devuelve al cliente.
Incluye Id, Username, FullName, Email, Rol, Active, CreatedAt y UpdatedAt.
NO incluye PasswordHash.

¿Para qué sirve?
Controlar la información que sale de la aplicación hacia el cliente.

Sustentación: "Utilizo un DTO de respuesta para controlar qué información expongo.
Aunque PasswordHash pertenece a la entidad y es necesario para la
autenticación, no debe viajar al frontend."

6.2 CreateUserDto
-----------------
Define los datos necesarios para crear un usuario: username, fullname, email, password y rol.
Recibe Password, no PasswordHash.

¿Por qué?
El cliente proporciona la contraseña original durante la creación.
Application deberá convertirla en un hash antes de almacenarla.
Nunca se debe almacenar la contraseña original directamente.

El proceso previsto es: Password->BCrypt->PasswordHash->SQL Server

6.3 UpdateUserDto
-----------------
Define los datos permitidos para actualizar: Username, FullName, Email, Rol y Active.
No incluye contraseña. El cambio de contraseña puede ser un caso de uso
separado. Esto evita mezclar diferentes responsabilidades dentro de una misma
operación.

6.4 LoginDto
-------------
Define los datos necesarios para iniciar sesión: email, password
Utiliza validaciones de DataAnnotations: [Required], [EmailAddress]

Ejemplo conceptual: El email es obligatorio y debe tener formato de correo.
La contraseña también es obligatoria.
LoginDto representa los datos que llegan desde el cliente al caso de uso de autenticación.

6.5 IAuthService
------------------
Archivo: Application/Interfaces/IAuthService.cs
Define el contrato del servicio de autenticación.
Método: Task<string?> LoginAsync(LoginDto dto);

¿Para qué sirve?
Permite que el Controller trabaje con una abstracción en lugar de conocer directamente la implementación de AuthService.

Flujo: AuthController->IAuthService->AuthService

Sustentación: "Separé el contrato de autenticación de su implementación para reducir
el acoplamiento y mantener la responsabilidad del Controller enfocada
en HTTP."

6.6 AuthService
------------------
Archivo: Application/Services/AuthService.cs
Responsabilidad: Gestionar la autenticación del usuario.

Actualmente realiza:
Recibe LoginDto.
Busca el usuario mediante GetByEmailAsync.
Verifica la contraseña mediante BCrypt.
Crea los claims.
Crea la clave de firma.
Firma el JWT mediante HMAC-SHA256.
Establece una duración de una hora.
Convierte el JWT a texto.
Devuelve el token.

Flujo:
LoginDto->AuthService->GetByEmailAsync(email)->Usuario->BCrypt.Verify()->Claims->JWT->Token

6.7 UserService
----------------
Archivo: Application/Services/UserService.cs
Responsabilidad: Gestionar los casos de uso relacionados con usuarios.

Actualmente realiza:
* Obtener todos los usuarios.
* Obtener un usuario por ID.
* Crear usuarios.
* Actualizar usuarios.
* Eliminar lógicamente usuarios.
* Validar que el correo no esté registrado al crear.
* Generar el hash de la contraseña mediante BCrypt.
* Convertir User a UserResponseDto.

Flujo de creación: CreateUserDto-> UserService-> Validar email-> Crear User-> BCrypt.HashPassword()-> IUserRepository-> UserRepository-> SQL Server

Flujo de consulta: IUserRepository-> User-> UserService-> UserResponseDto-> Controller-> Cliente

Mapeo:
User -> UserResponseDto
El DTO no incluye passwordHash ni deletedAt.

Sustentación:
"UserService contiene los casos de uso de usuarios y coordina al repositorio. Además, se encarga de transformar las entidades en DTOs para no exponer directamente el modelo interno de la aplicación."
La contraseña se hashea antes de enviarla al Repository y el Repository se mantiene enfocado únicamente en la persistencia.


6.7.2 BCrypt
-----------
BCrypt se utiliza para verificar la contraseña.
Conceptualmente:
BCrypt.Verify(
contraseñaIngresada,
passwordHash
)

No se compara directamente: password == passwordHash
porque el hash no es la contraseña original.

BCrypt permite verificar si la contraseña proporcionada corresponde
al hash almacenado.

Sustentación: "Las contraseñas no se almacenan en texto plano. Utilizo BCrypt para
verificar la contraseña ingresada contra el hash almacenado."

6.7.3 Claims
-------------
Los claims son datos que se incorporan dentro del token para representar
información relacionada con el usuario autenticado.

Actualmente se agregan:
Sub -> id del usuario.
Email -> email del usuario.
Name -> nombre completo.
Role -> rol del usuario.

Ejemplo: new Claim(JwtRegisteredClaimNames.Sub, user.id.ToString())
Esto permite que posteriormente la API pueda conocer información del
usuario autenticado a partir del token.

6.7.4 SymmetricSecurityKey
-------------
Se utiliza una clave simétrica para firmar el JWT.
La clave actualmente se encuentra escrita directamente en el código: "clave-super-secreta-para-oben-123"

Esto funciona para la prueba local, pero NO es la configuración final
recomendada para producción.
Posteriormente la clave debe trasladarse a configuración segura, por ejemplo:
appsettings.json para configuración no sensible de desarrollo.
User Secrets durante desarrollo.
Variables de entorno.
Secret Manager o un servicio de secretos en producción.

6.7.5 HMAC-SHA256
----------
Se utiliza: SecurityAlgorithms.HmacSha256 para firmar el JWT.
La firma permite que la API pueda verificar que el token fue generado
con la clave esperada y que no fue alterado.

Sustentación: "Firmo el JWT utilizando HMAC-SHA256 para que posteriormente el servidor
pueda validar la integridad y autenticidad de la firma."

6.7.6 Expiración
---------------
El token actualmente tiene una duración de una hora: DateTime.UtcNow.AddHours(1)
Se utiliza UTC para evitar depender de la zona horaria local del servidor.
Después de su expiración, el token deja de ser válido y el usuario
deberá autenticarse nuevamente según el mecanismo que se implemente.

6.7.7 WriteToken
------------
Finalmente:
new JwtSecurityTokenHandler().WriteToken(token)
convierte el objeto JwtSecurityToken a su representación textual.
Ese texto es el JWT que será enviado al cliente.

6.8 IUserService
-------------
Archivo: Application/Interfaces/IUserService.cs

Responsabilidad: Definir el contrato para los casos de uso relacionados con la gestión
de usuarios.

Métodos:
GetAllAsync()
GetByIdAsync(int id)
CreateAsync(CreateUserDto dto)
UpdateAsync(int id, UpdateUserDto dto)
DeleteAsync(int id)

Importante:
IUserService y IUserRepository NO son lo mismo.
IUserRepository: Representa operaciones de persistencia.
IUserService: Representa casos de uso de la aplicación.

Ejemplo:
IUserRepository: "Guardar este usuario."
IUserService: "Crear un usuario aplicando las reglas necesarias y luego solicitar
al repositorio que lo persista."

Flujo: Controller->IUserService->UserService->IUserRepository->UserRepository->SQL Server

7. FLUJO BACKEND PREVISTO
-------------------------
HTTP Request -> Controller -> Application Service -> IUserRepository -> UserRepository 
    -> SqlConnection / SqlCommand -> SQL Server -> Trigger -> auditLogs

8. CQRS
-------
No se utilizará CQRS en esta prueba.
Razón: El alcance actual es CRUD + autenticación + auditoría. Separar modelos
de lectura y escritura agregaría complejidad sin una necesidad clara.

9. SEGURIDAD PREVISTA
---------------------
- Consultas SQL parametrizadas.
- No exponer PasswordHash.
- Hash de contraseñas en C#.
- BCrypt para verificación de contraseñas.
- JWT para autenticación.
- Claims para representar información del usuario.
- Validación de entrada.
- SESSION_CONTEXT para identificar al usuario que realiza cambios.
- Borrado lógico.
- No guardar contraseñas en texto plano.

10. MAPEO ENTITY -> DTO
________________________
Pendiente.

El mapeo permitirá transformar: User->UserResponseDto
Esto evita devolver directamente la entidad del dominio desde el API.

Ejemplo conceptual:
User contiene: passwordHash

UserResponseDto NO contiene: passwordHash
Por eso el servicio será responsable de construir el DTO de respuesta
a partir de la entidad.

Sustentación: "El mapeo separa el modelo interno de dominio del contrato que expongo
hacia el cliente. Así puedo controlar exactamente qué datos salen de
la aplicación."MAPEO ENTITY -> DTO

Pendiente.
El mapeo permitirá transformar: User->UserResponseDto
Esto evita devolver directamente la entidad del dominio desde el API.

Ejemplo conceptual:
User contiene: passwordHash

UserResponseDto NO contiene: passwordHash
Por eso el servicio será responsable de construir el DTO de respuesta
a partir de la entidad.

Sustentación: "El mapeo separa el modelo interno de dominio del contrato que expongo
hacia el cliente. Así puedo controlar exactamente qué datos salen de
la aplicación."

11. MANEJO DE ERRORES DE APLICACIÓN
_______________________________
Pendiente.

Se deberá definir cómo manejar:
usuario inexistente
usuario inactivo
email duplicado
username duplicado
datos inválidos
errores de persistencia
errores inesperados

No se debe devolver información sensible de excepciones directamente
al cliente.
El Controller posteriormente traducirá los resultados de Application
a códigos HTTP apropiados.

12. DEPENDENCY INJECTION
------------------------
Archivo: Api/Program.cs
Responsabilidad: Registrar las implementaciones que utilizará ASP.NET Core mediante Inyección de Dependencias.
Se registraron:
IUserRepository -> UserRepository
IUserService -> UserService
IAuthService -> AuthService

Todos utilizan ciclo de vida Scoped.

Flujo:
AuthController->IAuthService->AuthService->IUserRepository->UserRepository
UsersController->IUserService->UserService->IUserRepository->UserRepository

¿Para qué?
Permite que las clases dependan de interfaces en lugar de crear directamente sus implementaciones.

Sustentación:
"Registré las interfaces y sus implementaciones en el contenedor de Inyección de Dependencias de ASP.NET Core. Utilizo Scoped porque quiero que los servicios tengan un ciclo de vida asociado a cada petición HTTP."

13. CHECKLIST BACKEND
---------------------
ESTRUCTURA
[OK] Crear Domain.
[OK] Crear Application.
[OK] Crear Infrastructure.
[OK] Crear Api.
[OK] Configurar referencias.
[OK] Verificar dirección de dependencias.

DOMAIN
[OK] Crear User.
[OK] Crear IUserRepository.

INFRASTRUCTURE
[OK] Instalar Microsoft.Data.SqlClient.
[OK] Crear SqlConnectionFactory.
[OK] Configurar conexión SQL Server.
[OK] Crear UserRepository.
[OK] GetAllAsync.
[OK] GetByIdAsync.
[OK] CreateAsync.
[OK] UpdateAsync.
[OK] DeleteAsync como borrado lógico.
[OK] SQL parametrizado.
[OK] Registrar IUserRepository en DI.

APPLICATION
[OK] UserResponseDto.
[OK] CreateUserDto.
[OK] UpdateUserDto.
[OK] LoginDto
[OK] IAuthService.
[OK] AuthService + JWT
[ ] IUserService.
[OK] UserService.
[OK] Validaciones.
[OK] Mapeo Entity -> DTO.
[OK] Manejo de errores de aplicación.

API
[OK] ASP.NET Core Web API.
[OK] OpenAPI/Swagger.
[OK] Swagger UI.
[ ] UsersController.
[ ] GET /api/users.
[ ] GET /api/users/{id}.
[ ] POST /api/users.
[ ] PUT /api/users/{id}.
[ ] DELETE /api/users/{id}.
[ ] AuthController.
[ ] Login.
[ ] JWT.
[ ] SESSION_CONTEXT.

PRUEBAS
[ ] Probar CRUD desde Swagger.
[ ] Probar validaciones.
[ ] Probar autenticación.
[ ] Probar auditoría desde C#.

11. PRÓXIMO PASO
----------------
Crear IUserService y después UserService.
El Controller NO llamará directamente al Repository.
Controller -> IUserService -> IUserRepository -> UserRepository -> SQL

12. SUSTENTACIÓN
----------------
¿Por qué separar Domain, Application, Infrastructure y API?
"Separé responsabilidades para evitar que toda la lógica quede en una
sola capa. Domain representa conceptos del negocio, Application
coordina los casos de uso, Infrastructure implementa detalles técnicos
como SQL Server y API se encarga de HTTP."

¿Por qué no llamar Repository desde Controller?
"Porque quiero separar la exposición HTTP de los casos de uso. El
Controller recibe la petición y delega en Application."

¿Por qué IUserRepository y UserRepository están separados?
"La interfaz define el contrato y la implementación concreta queda en
Infrastructure. Esto reduce el acoplamiento y aplica Dependency
Inversion."

¿Por qué IUserService y UserService están separados?
"IUserService define el contrato del caso de uso y UserService contiene
la implementación. Esto permite que el Controller dependa de una
abstracción y mantiene separadas las responsabilidades."

¿Por qué AuthService y UserService son servicios diferentes?
"Porque autenticación y gestión de usuarios son responsabilidades
diferentes. AuthService maneja login, contraseña, claims y JWT.
UserService manejará las operaciones de administración de usuarios."

¿Por qué DTOs?
"Para controlar qué datos entran y salen de los casos de uso y evitar
exponer directamente las entidades, especialmente PasswordHash."

¿Por qué no devolver User directamente?
"Porque la entidad contiene información interna que no necesariamente
debe formar parte del contrato de la API. El DTO me permite controlar
exactamente qué expongo."

¿Por qué GetByEmailAsync?
"Porque para autenticación solamente necesito un usuario. Traer todos
los usuarios para buscar uno en memoria sería innecesario. Por eso la
consulta se realiza directamente en SQL Server."

¿Por qué no guardar Password directamente?
"Porque una contraseña no debe almacenarse en texto plano. Se almacena
un hash y durante el login se verifica la contraseña ingresada contra
ese hash."

¿Qué hace BCrypt.Verify?
"Verifica si la contraseña proporcionada corresponde al hash almacenado,
sin necesidad de recuperar la contraseña original."

¿Qué son los Claims?
"Son datos que representan información asociada al usuario autenticado
y que se incorporan al token. En este proyecto incluyo el ID, email,
nombre y rol."

¿Por qué JWT?
"Porque permite que el cliente presente un token en solicitudes
posteriores y la API pueda validar la identidad y autorización del
usuario sin enviar nuevamente las credenciales."

¿Por qué HMAC-SHA256?
"Es el algoritmo utilizado para firmar el JWT. La firma permite validar
que el token fue generado con la clave esperada y que su contenido no
fue alterado."

¿Por qué expira el JWT?
"Para limitar el tiempo durante el cual un token puede utilizarse. En
esta implementación inicial tiene una duración de una hora."

¿Por qué usar async/await con SQL Server?
"Porque las operaciones de base de datos pueden tardar y son operaciones
de I/O. El código asíncrono evita bloquear innecesariamente el hilo
mientras espera la respuesta."

¿Por qué SQL parametrizado?
"Porque separa los valores de la instrucción SQL y ayuda a prevenir
inyecciones SQL."

¿Por qué borrado lógico?
"Porque permite conservar el registro y su historial, en lugar de
eliminarlo físicamente. En este proyecto se utiliza deletedAt junto
con active."

¿Por qué no CQRS?
"Porque el alcance de la prueba no justifica la complejidad adicional
de separar modelos de lectura y escritura."

¿Qué diferencia hay entre Domain y Application?
"Domain contiene las entidades y contratos relacionados con el dominio.
Application coordina los casos de uso utilizando esas abstracciones."

¿Qué diferencia hay entre Application e Infrastructure?
"Application define qué debe hacer la aplicación. Infrastructure
resuelve técnicamente cómo acceder a recursos externos como SQL Server."

¿Qué diferencia hay entre API y Application?
"API se ocupa de HTTP: requests, responses y códigos de estado.
Application se ocupa de los casos de uso."

¿Por qué Application no referencia Infrastructure?
"Para evitar acoplar los casos de uso con una implementación concreta.
Application depende de IUserRepository, no de UserRepository."

¿Qué pasaría si mañana cambiara SQL Server por otra tecnología?
"La implementación concreta podría cambiar en Infrastructure mientras
Application continúa utilizando el contrato IUserRepository."

¿Qué principio SOLID estamos aplicando principalmente?
"Dependency Inversion, porque las capas superiores dependen de
abstracciones y no de implementaciones concretas. También se busca
Single Responsibility al separar Controller, Service y Repository."