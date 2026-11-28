# GYM ELITE

Para la integración de la etapa 4, consulta el [contrato de la API REST](docs/api-fase4.md) y la guía de [logging, monitoreo y despliegue](docs/despliegue-y-monitoreo.md).

Proyecto universitario de gestión de gimnasio para la asignatura Desarrollo de Aplicaciones con Software Propietario, Universidad Don Bosco.

El proyecto partió de la plantilla `GYMELITEGiTHub`. La versión de la etapa 4 incluye autenticación, administración de usuarios por roles y persistencia en SQL Server.

## Demo en Azure

[Abrir GYM ELITE](https://gymelite-udb-demo-2026-hbdbg7bjckgnhkea.westus3-01.azurewebsites.net/)

| Campo | Valor |
| --- | --- |
| Correo | `admin@gymelite-udb-demo.example` |
| Contraseña | Pedila por privado al responsable del despliegue. |
| Rol | Administrador |

Esta cuenta corresponde a la aplicación publicada en Azure. El menú **Usuarios** permite crear accesos de Recepción e Inventario.

La versión publicada corresponde al commit `888a170`. Se verificó que abre el formulario de inicio de sesión y que [el endpoint de estado](https://gymelite-udb-demo-2026-hbdbg7bjckgnhkea.westus3-01.azurewebsites.net/health) confirma la conexión con `GymEliteDb`.

## Tecnología

- ASP.NET Core MVC y C#.
- Proyecto dirigido a .NET 8 (`net8.0`).
- Vistas Razor, Bootstrap, jQuery y recursos en `wwwroot`.

## Estructura

```text
GYMELITEGiTHub.slnx
GYMELITEGiTHub/
├── Controllers/
├── Models/
├── Views/
├── Properties/
├── wwwroot/
├── Program.cs
├── appsettings.json
└── GYMELITEGiTHub.csproj
```

La solución conserva el nombre del proyecto enviado por el compañero. El archivo `.slnx` requiere un IDE o SDK con soporte para ese formato; el proyecto `.csproj` se puede abrir directamente.

## Abrir el proyecto

Abrir `GYMELITEGiTHub/GYMELITEGiTHub.csproj` en Visual Studio con la carga de trabajo de desarrollo web y los componentes de .NET 8 instalados.

Para ejecutar una copia local, configurá `ConnectionStrings:GymEliteDb`, `Jwt:SecretKey`, `BootstrapAdmin:Email` y `BootstrapAdmin:Password` con valores de tu entorno mediante el Administrador de secretos de .NET.

Desde una terminal con el SDK correspondiente:

```sh
dotnet restore GYMELITEGiTHub/GYMELITEGiTHub.csproj
dotnet run --project GYMELITEGiTHub/GYMELITEGiTHub.csproj
```

## Prototipo de interfaz

La demostración navegable se encuentra en [Figma Make — GYM ELITE](https://www.figma.com/make/QcfA8vfOdzahyuOhf92m4W). Utiliza datos simulados y es un entregable separado del proyecto MVC.

## Trabajo en equipo

- Trabajar a partir de esta base y registrar aportes concretos en el historial.
- Mantener separados modelos, vistas y controladores.
- Documentar el alcance real de cada módulo y las funciones pendientes.
- Conservar los archivos locales del IDE y las salidas de compilación fuera del repositorio mediante `.gitignore`.
