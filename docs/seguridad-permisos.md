# Seguridad: roles y matriz de permisos (Etapa 4)

Roles de Identity: `Administrador`, `Recepcion`, `Inventario`. El rol lo asigna el Administrador desde Usuarios (no se elige al iniciar sesión).

Regla general: Recepción e Inventario ven los mismos módulos operativos. Solo **Reportes** y **Usuarios** son exclusivos del Administrador.

## Pantallas MVC (cookie de Identity)

| Módulo | Administrador | Recepción | Inventario |
| --- | :---: | :---: | :---: |
| Inicio | Sí | Sí | Sí |
| Socios | Sí | Sí | Sí |
| Membresías (incluye planes) | Sí | Sí | Sí |
| Pagos | Sí | Sí | Sí |
| Accesos | Sí | Sí | Sí |
| Productos | Sí | Sí | Sí |
| Proveedores | Sí | Sí | Sí |
| Compras | Sí | Sí | Sí |
| Ventas | Sí | Sí | Sí |
| Reportes | Sí | No | No |
| Usuarios (crear, cambiar rol, habilitar/deshabilitar, restablecer contraseña) | Sí | No | No |

## API REST (JWT Bearer)

| Política | Roles | Recursos |
| --- | --- | --- |
| `ApiRecepcion` | Administrador, Recepcion, Inventario | Socios, membresías, pagos, accesos, planes |
| `ApiInventario` | Administrador, Recepcion, Inventario | Productos, proveedores, compras, ventas |
| `ApiAdministracion` | Administrador | Reportes |

La matriz de la API coincide con la de MVC. Las dos políticas operativas aceptan los mismos roles; se conservan separadas por si el equipo decide volver a diferenciarlas.

## Comportamiento esperado

- Sin sesión (MVC): redirige a `/Account/Login`.
- Con sesión pero sin permiso (MVC): `/Account/AccessDenied` (por ejemplo, Recepción en `/Reportes` o `/Usuarios`).
- Sin token (API): `401`. Token de un rol sin permiso (por ejemplo, Recepción en `/api/reportes`): `403`. Token vencido: `401`.
- Bloqueo de cuenta: 5 intentos fallidos, 5 minutos.
- La sesión de cookie dura 8 horas (renovación deslizante).

## Reglas de gestión de usuarios

- Solo el Administrador accede a `/Usuarios`.
- No se puede deshabilitar la propia cuenta ni quitarse el propio rol de administrador.
- Siempre debe quedar al menos un administrador habilitado.
- Un usuario deshabilitado no puede iniciar sesión (bloqueo de Identity sin fecha de fin).
- Toda acción de gestión queda en el log con el id del usuario afectado y del administrador.
