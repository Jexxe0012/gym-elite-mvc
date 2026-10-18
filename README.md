# GYM ELITE — Base MVC

Proyecto universitario de gestión de gimnasio para la asignatura Desarrollo de Aplicaciones con Software Propietario, Universidad Don Bosco.

Este repositorio comienza desde la plantilla `GYMELITEGiTHub` entregada por un integrante del equipo. El código de la plantilla se conserva como punto de partida; se agregan este README y un `.gitignore` para el trabajo compartido.

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

Desde una terminal con el SDK correspondiente:

```sh
dotnet restore GYMELITEGiTHub/GYMELITEGiTHub.csproj
dotnet run --project GYMELITEGiTHub/GYMELITEGiTHub.csproj
```

## Estado de esta entrega inicial

La plantilla contiene controladores para inicio, cuenta, socios, membresías, pagos, productos, compras, ventas y reportes, junto con carpetas de vistas y un layout compartido.

Es una base de arquitectura. Las vistas de varios módulos son marcadores de posición y contienen texto de clases que deberá convertirse en contenido Razor/HTML. Existe una vista de accesos, pero aún falta su controlador. El formulario de inicio de sesión es visual: no implementa autenticación ni permisos y requiere ajustar su envío antes de usar credenciales.

Los modelos de negocio, ViewModels, DTOs, validaciones, persistencia y operaciones CRUD se integrarán en los siguientes aportes del equipo. El único modelo presente en la plantilla inicial es `ErrorViewModel`.

No se ha ejecutado la compilación ni la aplicación durante la preparación de este repositorio.

## Prototipo de interfaz

La demostración navegable se encuentra en [Figma Make — GYM ELITE](https://www.figma.com/make/QcfA8vfOdzahyuOhf92m4W). Utiliza datos simulados y es un entregable separado del proyecto MVC.

## Trabajo en equipo

- Trabajar a partir de esta base y registrar aportes concretos en el historial.
- Mantener separados modelos, vistas y controladores.
- Documentar el alcance real de cada módulo y las funciones pendientes.
- Conservar los archivos locales del IDE y las salidas de compilación fuera del repositorio mediante `.gitignore`.
