# Logging, manejo de errores, monitoreo y despliegue (Etapa 4)

Responsable: Nicolás. Cubre los apartados 6 (Logging y manejo de excepciones) y 7 (Despliegue de la solución), además de la configuración segura de producción del apartado 3. Este documento es la sección correspondiente del manual técnico.

## 1. Registro de eventos (logging)

La aplicación usa Serilog sobre el sistema de logging de ASP.NET Core. El código está en `Infraestructura/RegistroEventos.cs`.

| Qué se registra | Dónde |
| --- | --- |
| Una línea por solicitud: método, ruta, código de estado y duración | `UsarRegistroSolicitudes` |
| Eventos de negocio y seguridad ya existentes (inicio de sesión, bloqueo de cuenta, gestión de usuarios, errores de compras, pagos, accesos, reportes) | Controladores MVC y API |
| Excepciones no controladas con su detalle técnico | Línea de la solicitud y `ApiExceptionFilter` |
| Arranque y detención de la aplicación, fallos de arranque | `Program.cs` |

Cada línea incluye el **id de seguimiento** y el **usuario autenticado** (o `anonimo`). El id de seguimiento es el mismo que ve el usuario en la página de error y el mismo `traceId` que devuelve la API en los errores, así que permite pasar de una queja del usuario a la línea exacta del registro.

Formato de una línea:

```text
2026-11-27 10:15:02.481 -06:00 [ERR] [00-4bf9...-01] [admin@gymelite.com] Serilog.AspNetCore.RequestLoggingMiddleware: HTTP GET /Home/PruebaError respondió 500 en 12.4 ms
System.InvalidOperationException: Excepción de prueba generada desde /Home/PruebaError.
   at ...
```

Niveles: `INF` para solicitudes correctas, `WRN` para respuestas 4xx (datos inválidos, permisos, registros inexistentes), `ERR` para 5xx y excepciones, `FTL` si la aplicación no puede iniciar.

Lo que **no** se registra: contraseñas, tokens JWT, cookies, cuerpos de solicitud, cadenas de conexión ni la consulta (query string) de las URL. EF Core se limita a advertencias para no escribir SQL en el registro.

### Destinos

- Consola (Visual Studio, `dotnet run`).
- Archivo diario `gymelite-AAAAMMDD.log`. Carpeta por defecto: `Logs` dentro de la carpeta de la aplicación; se cambia con `Registro:Carpeta`. Se conservan 14 días en desarrollo y 30 en producción, con un máximo de 10 MB por archivo.

## 2. Manejo de excepciones

Código en `Infraestructura/ManejoExcepciones.cs`.

| Situación | Respuesta al usuario | Registro |
| --- | --- | --- |
| Error de regla de negocio o validación en la API | 400, 404 o 409 con `application/problem+json` (ya existente en `ApiExceptionFilter`) | `WRN` |
| Excepción inesperada en un controlador API | 500 con mensaje general y `traceId` | `ERR` con detalle |
| Excepción inesperada fuera de los controladores API (middleware, rutas `/api` sin controlador) | 500 `problem+json` con `traceId` | `ERR` con detalle |
| Excepción inesperada en una pantalla MVC | Página "No pudimos completar la operación" con el código de seguimiento | `ERR` con detalle |
| Página o registro inexistente en MVC (404, 405...) | Página amigable `/Home/Estado/{codigo}` | `WRN` |

En ningún entorno se muestra al usuario la pila de llamadas ni el mensaje técnico.

### Prueba controlada (para la demostración)

Con `Diagnostico:HabilitarPruebaError = true` (activado en desarrollo, desactivado en producción) y sesión de Administrador:

- MVC: abrir `/Home/PruebaError`. Debe mostrarse la página de error con un código de seguimiento.
- API: `GET /api/diagnostico/error` con token de Administrador. Debe responder 500 con `traceId`.

Luego buscar ese código en el archivo de registro. Con el valor en `false` ambas rutas responden 404.

## 3. Monitoreo de disponibilidad

Código en `Infraestructura/Monitoreo.cs`. Rutas públicas, sin datos sensibles:

| Ruta | Comprueba | Respuesta |
| --- | --- | --- |
| `GET /health/vivo` | Que la aplicación responde | 200 `{"estado":"Healthy",...}` |
| `GET /health` | Aplicación y conexión a la base de datos | 200 si todo está bien, 503 `Unhealthy` si la base no responde |

Ejemplo:

```json
{"estado":"Healthy","duracionMs":18.3,"fechaUtc":"2026-11-27T16:20:00Z","comprobaciones":[{"nombre":"base_de_datos","estado":"Healthy","duracionMs":17.9}]}
```

En Azure, `/health` se puede registrar en *App Service > Supervisión > Comprobación de estado* para que la plataforma avise si la aplicación deja de responder.

### Qué hacer ante un problema

1. Abrir `/health`. Si la base está `Unhealthy`, revisar la cadena de conexión, el firewall de la base o que el servicio SQL esté en marcha.
2. Pedir al usuario el código de seguimiento que muestra la pantalla de error.
3. Buscarlo en el archivo de registro del día (Bloc de notas, `Select-String` en PowerShell, o la consola Kudu en Azure).
4. Si la aplicación no inicia y no hay archivo de registro, activar temporalmente `stdoutLogEnabled="true"` en el `web.config` publicado (IIS) o revisar *Log stream* (Azure). Desactivarlo al terminar.

## 4. Configuración por entorno

| Archivo | Uso |
| --- | --- |
| `appsettings.json` | Valores comunes sin secretos |
| `appsettings.Development.json` | Desarrollo local; habilita la prueba de error |
| `appsettings.Production.json` | Producción; aplica migraciones al iniciar y deshabilita la prueba de error |
| User Secrets | Secretos en desarrollo (`Jwt:SecretKey`, `BootstrapAdmin`) |
| Variables de entorno o configuración de Azure | Secretos y valores del servidor publicado |

Ningún secreto se guarda en el repositorio. En el servidor se definen estas variables (el doble guion bajo equivale a `:`):

| Variable | Valor |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__GymEliteDb` | Cadena de conexión del servidor SQL de producción |
| `Jwt__SecretKey` | Clave en Base64 de al menos 32 bytes, distinta a la de desarrollo |
| `BootstrapAdmin__Email` y `BootstrapAdmin__Password` | Administrador inicial; la aplicación no arranca sin ellos |
| `Registro__Carpeta` | Opcional. Carpeta de registros con permiso de escritura |
| `DataProtection__RutaClaves` | Solo IIS. Carpeta para las claves que protegen cookies y formularios |
| `Diagnostico__HabilitarPruebaError` | Opcional. `true` solo mientras se graba la demostración |

Generar una clave JWT en PowerShell:

```powershell
$bytes = New-Object byte[] 48
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
[Convert]::ToBase64String($bytes)
```

Notas importantes:

- La cookie de sesión exige HTTPS (`SecurePolicy = Always`). Sobre `http://` el inicio de sesión no se completa. Azure ya da HTTPS; en IIS hay que crear un enlace HTTPS.
- LocalDB no sirve en IIS porque pertenece a un usuario de Windows. En IIS usar SQL Server Express o Developer.
- `AplicarMigracionesAlIniciar = true` crea o actualiza la base al arrancar. El usuario de la cadena de conexión debe tener permisos para crear tablas. Alternativa: generar un script con `dotnet ef migrations script --idempotent -o gymelite.sql` y ejecutarlo en el servidor.

## 5. Publicación

Primero, en cualquier caso:

```powershell
dotnet restore
dotnet build -c Release
dotnet publish GYMELITEGiTHub/GYMELITEGiTHub.csproj -c Release -o .\publicacion
```

### Opción A: Azure App Service + Azure SQL

1. En el portal de Azure (cuenta Azure for Students), crear un grupo de recursos `gymelite-rg`.
2. Crear una **Azure SQL Database** (oferta gratuita o nivel Basic) con su servidor. En *Redes*, permitir servicios de Azure y agregar la IP propia.
3. Crear una **App Web**: pila .NET 8, Windows, plan F1 o B1.
4. En la App Web, *Configuración > Variables de entorno*: agregar las variables de la sección 4 y la cadena de conexión `GymEliteDb` (tipo SQLAzure). Para los registros: `Registro__Carpeta = D:\home\LogFiles\GymElite`.
5. Publicar desde Visual Studio (clic derecho en el proyecto > Publicar > Azure > Azure App Service (Windows)) o subir la carpeta `publicacion` con el comando `az webapp deploy`.
6. Abrir `https://<nombre>.azurewebsites.net/health` y luego iniciar sesión.

### Opción B: IIS local

1. Activar IIS en *Características de Windows* e instalar el **ASP.NET Core 8 Hosting Bundle**. Reiniciar IIS (`iisreset`).
2. Tener SQL Server Express o Developer y ajustar la cadena de conexión (por ejemplo `Server=.\SQLEXPRESS;Database=GymEliteDb;Trusted_Connection=True;TrustServerCertificate=True`).
3. Crear el grupo de aplicaciones `GymElite` con "Sin código administrado".
4. Dar acceso a la base al usuario `IIS APPPOOL\GymElite` (inicio de sesión en SQL Server con rol `db_owner` sobre `GymEliteDb`, o al menos permisos para crear tablas si se aplican migraciones al iniciar).
5. Copiar `publicacion` a `C:\inetpub\gymelite`. Crear las carpetas `C:\inetpub\gymelite\Logs` y `C:\inetpub\gymelite-claves` y dar a `IIS APPPOOL\GymElite` permiso de Modificar.
6. Crear el sitio con esa carpeta y ese grupo, con enlace **https** (certificado de desarrollo de IIS Express o uno autofirmado).
7. Definir las variables de la sección 4 en *Editor de configuración > system.webServer/aspNetCore > environmentVariables*, o en el `web.config` publicado. `DataProtection__RutaClaves = C:\inetpub\gymelite-claves`.
8. Abrir `https://localhost/health` y luego iniciar sesión.

## 6. Validación del despliegue

| # | Prueba | Resultado esperado |
| --- | --- | --- |
| 1 | `GET /health` | 200, base de datos `Healthy` |
| 2 | Abrir la URL sin sesión | Redirige a inicio de sesión |
| 3 | Iniciar sesión con el administrador | Entra al inicio |
| 4 | `POST /api/auth/login` y luego `GET /api/socios` con el token | 200 con datos |
| 5 | Crear un socio y registrar una venta | Se guardan y aparecen en los listados |
| 6 | Reiniciar la aplicación (Azure: Reiniciar; IIS: reciclar grupo) | Los datos siguen; la sesión no obliga a volver a iniciar sesión (IIS con claves persistentes) |
| 7 | Abrir una URL inexistente | Página 404 amigable |
| 8 | Prueba controlada de error (variable activada temporalmente) | Página de error con código; el código aparece en el registro |
| 9 | Revisar el archivo de registro | Líneas de solicitudes con usuario e id de seguimiento, sin contraseñas ni tokens |

Evidencias a guardar para el manual y el video: URL pública, captura de `/health`, captura de las variables de entorno (sin mostrar valores secretos), captura del inicio de sesión en el entorno publicado, captura del registro con el id de seguimiento y captura después del reinicio.
