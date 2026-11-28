# Matriz de pruebas de la etapa 4

Usar una instalación de prueba con HTTPS y una base que admita datos ficticios. Las solicitudes para la API y el permiso de cada rol están en el [contrato de la API](api-fase4.md). Guardar para cada caso la fecha, la versión, el ambiente y el resultado observado. Las evidencias para los manuales se toman en la versión que se vaya a presentar.

## Casos de prueba

| Caso | Cómo probarlo | Resultado esperado |
| --- | --- | --- |
| Swagger | Abrir `/swagger` en Development, autorizar con un JWT de prueba y ejecutar `GET /api/productos`. | La UI envía el Bearer y la consulta devuelve 200. |
| Documento OpenAPI | Revisar los esquemas y métodos de `/swagger/v1/swagger.json`. | Las operaciones están agrupadas y los esquemas y estados HTTP coinciden con la API. El login es público y los recursos indican Bearer. |
| Swagger en producción | Consultar `/swagger/v1/swagger.json` en Production. | Devuelve 404. |
| Inicio de sesión | Probar credenciales válidas e incorrectas en `POST /api/auth/login`. | 200 con JWT y vencimiento para la cuenta válida. 401 para el intento inválido. |
| JWT y permisos | Consultar productos sin token y luego con un JWT válido de Recepción o Inventario. Intentar consultar reportes con ese rol. | 401 sin token, 200 en productos con token y 403 en reportes con rol operativo. Administrador sí consulta reportes. |
| Vencimiento | Reutilizar el JWT después de `expiresAt` más el margen de validación del servidor. | La API devuelve 401. |
| Alta, detalle, edición y baja | Crear socio, producto y proveedor en una base de prueba. Consultar el `Location`, editar y dar de baja. | 201 al crear, 200 al consultar y 204 al editar o dar de baja. Socios y productos conservan su historial. |
| Validaciones de la API | Enviar un cuerpo incompleto, buscar un recurso inexistente y solicitar una venta superior al stock. | 400, 404 y 409 respectivamente. Las peticiones rechazadas no cambian los datos. |
| Membresía, cobro y acceso | Crear socio con membresía pendiente. Intentar ingresar, cobrar y volver a registrar el ingreso. | Se rechaza el acceso sin pago. Se genera el recibo y queda registrado el acceso después del cobro. |
| Compra, venta y reportes | Crear producto sin existencias. Comprar 10 unidades, vender 2 y consultar el reporte del día. | Quedan 8 unidades. Reportes refleja el ingreso válido por la venta y el cobro del mismo día. |
| Anulaciones | Anular venta y luego compra, consultando el stock y los reportes. | El saldo regresa a su valor inicial y el ingreso anulado queda fuera del reporte. |
| Interfaz asíncrona | Filtrar productos y reportes y guardar una venta en el navegador. Probar una cantidad superior a la disponible. | Las pantallas se actualizan sin recargar. Se rechaza la cantidad inválida y una venta válida genera comprobante. |
| Protección de formulario MVC | Guardar con sesión en un formulario legítimo, primero sin token antifalsificación y luego con el token de ese formulario. | ASP.NET Core rechaza la primera solicitud y permite la segunda si los datos son válidos. |
| Logging y monitoreo | En el entorno publicado, solicitar `/health` y correlacionar el `traceId` de la demostración controlada con el archivo de registro. | `/health` responde 200 cuando la base está disponible y el log contiene el código mostrado al usuario. |
| Persistencia en el despliegue | Consultar el entorno IIS o Azure, guardar un registro de prueba y volver a consultarlo tras el reinicio. | El dato continúa disponible en la misma instalación. Con las claves de protección persistidas, la sesión sigue siendo válida. |

## Primera ejecución local

Versión probada: rama `josue-fase4-validacion` basada en `main` `47f2993`, con Swagger, HTTPS local y una base LocalDB separada.

- **Swagger.** Ocho comprobaciones correctas. El documento contiene 45 operaciones REST. La UI autorizó una consulta de productos con un JWT real y devolvió 200. El documento respondió 404 en el entorno Production.
- **API, permisos y formularios.** 42 solicitudes verificadas. Incluyen login válido e inválido, 401 sin JWT, 403 por rol, antifalsificación en formularios MVC y los flujos de membresía, cobro, acceso, compra y venta. El reporte del día reflejó 31 dólares, 25 del cobro y 6 de la venta.
- **Por completar en la validación final.** Vencimiento del JWT, pruebas de interfaz sobre la versión final, revisión de errores y monitoreo tras integrar el PR de despliegue, y verificación en el entorno IIS o Azure.

## Evidencias y entregables

- **Pruebas de la API.** Registrar el estado HTTP obtenido, el dato persistido y el resultado de operaciones inválidas o sin permiso. Usar datos ficticios y omitir credenciales y tokens de las capturas.
- **Manual técnico.** Reunir instalación, configuración por ambiente, migraciones, matriz de roles, API, Swagger, despliegue y diagnóstico. Usar como base el contrato, la matriz de seguridad y la guía de Nicolás una vez integrada.
- **Manual de usuario.** Incluir inicio de sesión y tareas por rol con las pantallas de socios, accesos, pagos, productos, compras, ventas, reportes y usuarios.
- **Presentación.** Mostrar la versión integrada. Cubrir los dos flujos de negocio, una negativa por permiso, una interacción asíncrona y la evidencia del sitio publicado. Dejar constancia de las mejoras propuestas.
