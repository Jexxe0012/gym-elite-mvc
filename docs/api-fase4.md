# Contrato de la API REST

La API de GYM ELITE usa rutas bajo `/api` y JSON con nombres de propiedades en camelCase. Los controladores reutilizan los servicios que usan las pantallas MVC. La configuración de Swagger, la emisión de JWT y la integración con Fetch se coordinan con los responsables de esos frentes.

## Autenticación y permisos

Las solicitudes llevan `Authorization: Bearer <token>`. El esquema que registra el frente de seguridad debe llamarse `Bearer`. La aplicación devuelve `503` mientras ese esquema no esté registrado. Una vez configurado, las peticiones requieren identidad autenticada y el rol correspondiente.

| Política | Roles admitidos | Operaciones |
| --- | --- | --- |
| `ApiRecepcion` | `Administrador`, `Recepcion` | Socios, lectura de planes, membresías, pagos y accesos |
| `ApiInventario` | `Administrador`, `Inventario` | Productos, proveedores, compras y ventas |
| `ApiAdministracion` | `Administrador` | Escritura de planes y consulta de reportes |

Los nombres de roles son la propuesta inicial para integrar con Identity. Alexander puede ajustar la configuración de las políticas según la matriz acordada. Los tokens deben aportar los roles que ASP.NET Core usa para autorizar. César debe enviar el bearer al consumir estas rutas o usar el mecanismo servidor a servidor acordado con Alexander. Las vistas MVC mantienen sus rutas.

## Endpoints

Todos los parámetros de fecha usan `YYYY-MM-DD`. `fechaDesde` y `fechaHasta` son inclusivas. Los identificadores se obtienen de las respuestas, usando `id`.

| Recurso | Métodos y rutas | Entrada y salida |
| --- | --- | --- |
| Socios | `GET /api/socios`, `GET /api/socios/{id}`, `GET /api/socios/{id}/membresias` | `busqueda` opcional en la lista. Devuelve socios, su ficha o sus membresías. |
| Socios | `POST /api/socios`, `PUT /api/socios/{id}`, `DELETE /api/socios/{id}` | Crear y editar datos del socio. PUT requiere `estaHabilitado`. DELETE desactiva la ficha y conserva el historial. |
| Planes | `GET /api/planes`, `GET /api/planes/{id}` | `soloActivos=true` opcional en la lista. |
| Planes | `POST /api/planes`, `PUT /api/planes/{id}`, `DELETE /api/planes/{id}` | Crear requiere `nombre`, `modalidad`, `duracionDias` y `precio`. PUT requiere además `activo`. DELETE desactiva de forma idempotente. |
| Membresías | `GET /api/membresias`, `GET /api/membresias/{id}`, `POST /api/membresias`, `POST /api/membresias/{id}/cancelacion` | Lista con `busqueda` y `estado` opcionales. Alta y renovación comparten solicitud. Cancelación devuelve la membresía y, cuando corresponde, el número de recibo anulado. |
| Pagos | `GET /api/pagos`, `GET /api/pagos/{id}`, `POST /api/pagos`, `POST /api/pagos/{id}/anulacion` | Lista acepta `busqueda`, `estado` (`Registrado`, `Anulado`) y fechas opcionales. Devuelve `pagos`, `pagosHoy` y `montoHoy`. Registrar requiere `membresiaId`. El servidor calcula el monto. |
| Accesos | `GET /api/accesos`, `GET /api/accesos/{id}`, `GET /api/accesos/resumen`, `GET /api/accesos/verificacion`, `POST /api/accesos` | Historial de hasta 200 con búsqueda y fechas opcionales. Resumen con `ingresosHoy`, `sociosHoy` y `recientes`. La verificación exige `busqueda` y devuelve `autorizado`, `motivo` y coincidencias. El alta exige `socioId`. |
| Productos | `GET /api/productos`, `GET /api/productos/{id}`, `POST /api/productos`, `PUT /api/productos/{id}`, `DELETE /api/productos/{id}` | Lista con `busqueda` y `categoria` opcionales. Devuelve `productos`, `productosActivos`, `productosStockBajo` y `unidadesDisponibles`. PUT actualiza los datos editables. DELETE desactiva el producto. |
| Productos | `POST /api/productos/{id}/ajustes` | `tipo` (`Entrada`, `Salida`), `cantidad` y `motivo`. Reconsultar el detalle para mostrar el nuevo saldo. |
| Proveedores | `GET /api/proveedores`, `GET /api/proveedores/{id}`, `POST /api/proveedores`, `PUT /api/proveedores/{id}`, `DELETE /api/proveedores/{id}` | `busqueda` opcional en la lista. Escritura con `nombre`, `telefono` y `correo` opcional. DELETE permite eliminar únicamente proveedores sin compras. |
| Compras | `GET /api/compras`, `GET /api/compras/{id}`, `POST /api/compras`, `POST /api/compras/{id}/anulacion` | Lista con `proveedorId`, `estado` (`Registrada`, `Anulada`) y fechas opcionales. Devuelve `compras` y `resumenMes`. Al registrar, las líneas guardan cantidad y costo unitario. |
| Ventas | `GET /api/ventas`, `GET /api/ventas/{id}`, `POST /api/ventas`, `POST /api/ventas/{id}/anulacion` | Lista con fechas opcionales. Cada línea de alta recibe `productoId` y `cantidad`. El servidor obtiene el precio y calcula el total. |
| Reportes | `GET /api/reportes` | `fechaDesde` y `fechaHasta` se pueden omitir para consultar el mes actual hasta hoy. Rango máximo de tres meses. Devuelve `resumen`, ingresos por día y modalidad, productos más vendidos, movimientos y existencias críticas. |

El filtro de membresías admite `vigentes`, `porvencer`, `pendientes`, `vencidas`, `programadas` y `canceladas`. La verificación de acceso por código, QR, nombre o teléfono devuelve `200` con el resultado de la consulta, aunque no haya cobertura o coincidencias. El campo enumerado `resultado` vale `0` si no hay socio, `1` si hay varias coincidencias y `2` si hay un socio.

Las modalidades de los planes son `Mensualidad` y `Pase diario`. Las categorías de producto disponibles son `Suplementos`, `Nutrición`, `Bebidas` y `Accesorios`. Compras y ventas aceptan de 1 a 100 líneas, cada una con un producto diferente. Los nombres de los estados tienen la capitalización de la tabla.

## Solicitudes de ejemplo

En las escrituras con datos, enviar también `Content-Type: application/json`. Los identificadores y fechas de ejemplo se reemplazan por los del entorno de prueba.

| Solicitud | Cuerpo de ejemplo |
| --- | --- |
| `POST /api/socios` | `{"nombreCompleto":"Socio Demo","telefono":"70000001","correo":"demo@example.com","fechaNacimiento":"2000-01-01"}` |
| `PUT /api/socios/1` | `{"nombreCompleto":"Socio Demo","telefono":"70000002","fechaNacimiento":"2000-01-01","estaHabilitado":true}` |
| `POST /api/planes` | `{"nombre":"Mensual Demo","modalidad":"Mensualidad","duracionDias":30,"precio":25}` |
| `POST /api/membresias` | `{"socioId":1,"tipoMembresiaId":1,"fechaInicio":"2026-11-24","periodosAdquiridos":1,"cobrarAhora":false}` |
| `POST /api/pagos` | `{"membresiaId":1}` |
| `POST /api/accesos` | `{"socioId":1}` |
| `POST /api/productos` | `{"codigo":"PR-901","nombre":"Bebida Demo","categoria":"Bebidas","precioVenta":3,"stockMinimo":2,"existenciaInicial":0}` |
| `PUT /api/productos/1` | `{"nombre":"Bebida Demo","categoria":"Bebidas","precioVenta":3,"stockMinimo":3,"activo":true}` |
| `POST /api/productos/1/ajustes` | `{"tipo":"Entrada","cantidad":5,"motivo":"Conteo de prueba"}` |
| `POST /api/proveedores` | `{"nombre":"Proveedor Demo","telefono":"70000003"}` |
| `POST /api/compras` | `{"proveedorId":1,"lineas":[{"productoId":1,"cantidad":10,"costoUnitario":1.50}]}` |
| `POST /api/ventas` | `{"lineas":[{"productoId":1,"cantidad":2}]}` |

Las rutas de anulación y cancelación no necesitan cuerpo. La cancelación de una membresía con accesos registrados se rechaza. La compra solo se puede anular si las existencias permiten revertir la entrada.

## Respuestas HTTP

| Estado | Significado |
| --- | --- |
| `200 OK` | Consulta o cancelación de membresía exitosa. |
| `201 Created` | Registro persistido. El cuerpo contiene sus datos y `Location` apunta a su detalle. |
| `204 No Content` | Cambio, baja, ajuste o anulación exitosa, sin cuerpo. |
| `400 Bad Request` | Cuerpo, filtros, fechas o datos inválidos. |
| `401 Unauthorized` | La API requiere un token válido. |
| `403 Forbidden` | El usuario autenticado no tiene el permiso requerido. |
| `404 Not Found` | El recurso o una referencia requerida no existe. |
| `409 Conflict` | La operación incumple una regla del estado actual, por ejemplo falta de existencias o un pago ya registrado. |
| `500 Internal Server Error` | Error inesperado. Se entrega un mensaje general y se registra el detalle técnico. |
| `503 Service Unavailable` | Falta configurar la autenticación de las rutas API. |

Los errores usan `application/problem+json`. La validación de entradas devuelve `errors` por propiedad. Las reglas de negocio usan `detail`, por ejemplo:

```json
{
  "title": "Conflict",
  "status": 409,
  "detail": "La membresía ya tiene un pago válido. Puede consultar o reimprimir su recibo."
}
```

## Verificación para la integración

1. Consultar una ruta protegida sin token y con un rol sin permiso. Comprobar `401` y `403` cuando el esquema esté configurado.
2. Crear socio y plan. Registrar membresía pendiente con `cobrarAhora: false`. Consultar la verificación, cobrar el monto calculado y registrar el acceso. Comprobar que un segundo cobro o ingreso inmediato se rechaza.
3. Crear proveedor y producto sin existencias. Comprar 10 unidades y vender 2. Confirmar la existencia de 8, la entrada de 10, la salida de 2 y el ingreso por la venta en reportes.
4. Comprobar validaciones con una lista de líneas vacía, un producto duplicado, un intervalo invertido y una venta superior a las existencias. Revisar que las solicitudes rechazadas no alteren el saldo.
5. Anular la venta y comprobar que vuelva el stock. Anular la compra cuando haya unidades suficientes y consultar el reporte de operaciones válidas.

### Resultado del ensayo del 24 de noviembre de 2026

107 comprobaciones HTTP pasaron en una copia local con SQL Server LocalDB y datos ficticios. Se verificaron las respuestas, los enlaces `Location`, los cambios de inventario y su reflejo en reportes. En el ensayo simultáneo, dos solicitudes intentaron vender la última unidad y solo una se registró. Dos anulaciones simultáneas de esa venta devolvieron la unidad una sola vez.

Para probar los permisos se usaron identidades de prueba en una copia separada. También se verificó el cierre de la API con `503` al faltar el esquema Bearer. La comprobación con JWT reales queda para la integración de Identity.
