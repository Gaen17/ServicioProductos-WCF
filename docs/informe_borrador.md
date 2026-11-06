# Informe de avance - PA2 y PA3

**Estudiante:** [completar]

**Docente:** [completar]

**Asignatura:** Desarrollo de Aplicaciones Avanzadas

**Fecha:** [completar]

**Repositorio:** ServicioProductos

**Rama:** `pa2-pa3`

## 1. Resumen

Se completaron las prácticas PA2 y PA3 sobre el servicio WCF existente. La solución
mantiene .NET Framework y WCF clásico, usa Entity Framework 6 contra una base SQL Server
existente y concentra todo el acceso a datos en procedimientos almacenados.

PA2 incorpora el CRUD de productos con validaciones, mensajes controlados y respuestas
DTO. PA3 incorpora el registro transaccional de devoluciones de venta, con devoluciones
parciales y completas, cálculo del monto devuelto y reingreso opcional de stock. La
seguridad de PA3 se configuró con HTTPS y autenticación básica para IIS.

## 2. Objetivos

- Implementar las operaciones de productos solicitadas en PA2 sin exponer entidades de
  Entity Framework.
- Usar procedimientos almacenados parametrizados para todas las operaciones de datos.
- Implementar la devolución de una venta en PA3 dentro de una transacción.
- Validar los datos de entrada y devolver mensajes claros, sin trazas ni excepciones
  técnicas al cliente.
- Configurar el servicio para HTTPS con autenticación básica.
- Documentar las pruebas y los puntos que requieren un entorno IIS.

## 3. Alcance

### 3.1 Incluido

- Listado, consulta, inserción, actualización y eliminación de productos.
- Validaciones de nombre, categoría, precio, stock, descripción y URL de imagen.
- Devolución total o parcial de una venta.
- Verificación de cantidades pendientes por producto.
- Cálculo del monto devuelto con el precio registrado en la venta.
- Reingreso de stock optativo por ítem.
- Transacción única para cabecera, detalle y stock.
- Configuración de HTTPS y autenticación básica.
- Scripts de base de datos, pruebas y checklist manual.

### 3.2 Fuera de alcance

- Autorización por roles o permisos.
- Interfaz de usuario.
- Migraciones de Entity Framework.
- Facturación, impuestos o actualización del total de la venta.
- Pruebas automatizadas dentro del repositorio.

## 4. Arquitectura

La solución mantiene una separación simple en capas:

```text
Cliente WCF
    |
    v
IProductoService / ProductoService.svc.cs     (contrato y validaciones)
    |
    v
ProductoData / DevolucionData                 (acceso a datos)
    |
    v
DesavaPAContext (EF6) + Database.SqlQuery     (ejecución parametrizada)
    |
    v
Procedimientos almacenados
    |
    v
SQL Server / LocalDB
```

Responsabilidades:

- `ProductoService.svc.cs`: recibe la llamada, valida reglas de negocio, arma la
  respuesta y registra errores técnicos en `Trace`.
- `ProductoData` y `DevolucionData`: ejecutan los procedimientos almacenados y mapean
  resultados a DTO internos.
- `DesavaPAContext`: contexto de EF6 que representa el esquema existente y expone
  `Database.SqlQuery` y `Database.ExecuteSqlCommand`.
- `Models/Producto.cs` y `Models/Respuestas.cs`: contratos públicos del servicio.
- `sql/`: esquema, procedimientos almacenados y datos de prueba.

### 4.1 Decisiones de diseño

- **Sin migraciones:** el esquema se crea y actualiza con scripts SQL. El contexto usa
  `Database.SetInitializer<DesavaPAContext>(null)`.
- **Sin entidades EF en el contrato:** el servicio devuelve `Producto` y clases de
  respuesta con `[DataContract]`; las entidades de `Data/Entidades.cs` son internas.
- **Una respuesta por operación:** cada operación tiene su clase con `Exito`, `Mensaje`
  y los datos que correspondan. No se usa una clase genérica.
- **SQL parametrizado:** todos los procedimientos reciben parámetros; no se concatena
  SQL con datos del usuario.
- **Errores controlados:** las excepciones se registran con `Trace` y el cliente recibe
  un mensaje en español sin detalle técnico.
- **Clave foránea:** el error 547 de SQL Server se traduce a un mensaje de categoría
  inexistente o producto con ventas, según la operación.

## 5. Modelo de datos

El esquema se reconstruyó en `sql/00_esquema_base.sql` a partir del enunciado y del
modelo usado por el servicio. Las tablas son:

| Tabla | Columnas principales |
| --- | --- |
| `Categorias` | `Id`, `Nombre` |
| `Productos` | `Id`, `Nombre`, `Descripcion`, `Precio`, `Stock`, `IdCategoria`, `UrlImagen` |
| `Ventas` | `Id`, `Fecha`, `Total` |
| `DetalleVenta` | `Id`, `IdVenta`, `IdProducto`, `Cantidad`, `Precio` |
| `Devolucion` | `Id`, `IdVenta`, `Fecha`, `MotivoDevolucion`, `TipoDevolucion`, `MontoTotalDevuelto` |
| `DetalleDevolucion` | `Id`, `IdDevolucion`, `IdProducto`, `Cantidad`, `PrecioVenta` |

Relaciones principales:

- `Productos.IdCategoria` referencia `Categorias.Id`.
- `DetalleVenta.IdVenta` referencia `Ventas.Id`.
- `DetalleVenta.IdProducto` referencia `Productos.Id`.
- `Devolucion.IdVenta` referencia `Ventas.Id`.
- `DetalleDevolucion.IdDevolucion` referencia `Devolucion.Id`.
- `DetalleDevolucion.IdProducto` referencia `Productos.Id`.

## 6. Implementación de PA2

### 6.1 Operaciones

| Operación | Respuesta | Descripción |
| --- | --- | --- |
| `ListarProductos()` | `RespuestaListaProductos` | Devuelve todos los productos con el nombre de su categoría |
| `ObtenerProducto(int id)` | `RespuestaProducto` | Devuelve un producto o un error controlado |
| `InsertarProducto(Producto producto)` | `RespuestaProducto` | Valida, inserta y devuelve el producto creado |
| `ActualizarProducto(Producto producto)` | `RespuestaProducto` | Valida, actualiza y devuelve el producto |
| `EliminarProducto(int id)` | `RespuestaOperacion` | Elimina si el producto no tiene ventas |

### 6.2 Validaciones

- Nombre obligatorio y de hasta 100 caracteres.
- Categoría mayor que cero y existente en la base.
- Precio mayor que cero.
- Stock mayor o igual que cero.
- Descripción de hasta 255 caracteres.
- URL de imagen de hasta 500 caracteres.
- Identificador mayor que cero en consultas y eliminación.
- Producto existente antes de actualizar o eliminar.
- Producto sin ventas antes de eliminar.

### 6.3 Procedimientos almacenados

- `usp_Producto_Listar`
- `usp_Producto_Obtener`
- `usp_Producto_Insertar`
- `usp_Producto_Actualizar`
- `usp_Producto_Eliminar`
- `usp_Categoria_Existe`
- `usp_Producto_TieneVentas`

Los procedimientos auxiliares devuelven 1 o 0 y no exponen datos al cliente.

## 7. Implementación de PA3

### 7.1 Operación

```csharp
RespuestaDevolucion RegistrarDevolucionVenta(
    int idVenta,
    string motivo,
    List<ItemDevolucionDto> items)
```

Cada ítem informa:

- `IdProducto`
- `Cantidad`
- `ReingresaInventario`

La respuesta informa:

- `Exito`
- `Mensaje`
- `IdDevolucion`
- `TipoDevolucion` (`Parcial` o `Completa`)
- `MontoTotalDevuelto`

### 7.2 Validaciones

1. La lista de ítems no puede estar vacía.
2. El motivo es obligatorio y de hasta 255 caracteres.
3. La venta debe existir.
4. Cada producto debe pertenecer a la venta.
5. Cada cantidad debe ser mayor que cero.
6. No se permite el mismo producto dos veces en la misma solicitud.
7. La cantidad no puede superar la pendiente de ese producto en la venta.

### 7.3 Cálculo de pendientes

El procedimiento `usp_Venta_ObtenerPendientes` agrupa `DetalleVenta` y
`DetalleDevolucion` por producto:

```text
pendiente = cantidad vendida - cantidad ya devuelta
```

El precio utilizado es el registrado en `DetalleVenta`, no el precio actual del producto.

### 7.4 Tipo de devolución

- `Completa`: después de registrar la devolución no queda ninguna cantidad pendiente en
  toda la venta.
- `Parcial`: todavía queda al menos una unidad pendiente en algún producto.

Esta regla es una decisión documentada porque el enunciado no define el criterio de
manera explícita.

### 7.5 Transacción

`DevolucionData.Registrar` abre una transacción con
`db.Database.BeginTransaction()` y dentro de ella:

1. Inserta la cabecera en `Devolucion`.
2. Inserta cada fila en `DetalleDevolucion`.
3. Suma stock solo cuando `ReingresaInventario` es verdadero.
4. Hace `Commit` si todo salió bien.
5. Hace `Rollback` y relanza la excepción si algo falla.

De este modo no quedan cabeceras sin detalle ni cambios de stock parciales.

### 7.6 Procedimientos almacenados

- `usp_Venta_Existe`
- `usp_Venta_ObtenerPendientes`
- `usp_Devolucion_Insertar`
- `usp_DetalleDevolucion_Insertar`
- `usp_Producto_SumarStock`

## 8. Seguridad

La configuración de seguridad se agregó en `Web.config`:

- Endpoint `basicHttpBinding` con `security mode="Transport"`.
- `clientCredentialType="Basic"`.
- Validación de usuario contra cuentas de Windows.
- Metadatos solo por HTTPS (`httpGetEnabled="false"`).
- Navegación de directorios deshabilitada.

Los pasos de IIS se documentan en `docs/CHECKLIST_MANUAL.md`:

1. Publicar el sitio en IIS.
2. Crear o importar un certificado.
3. Configurar el binding HTTPS.
4. Deshabilitar autenticación anónima.
5. Habilitar autenticación básica.
6. Crear el usuario demo con `scripts/crear_usuario_demo.ps1`.

La autenticación básica solo se usa sobre HTTPS porque el esquema Basic codifica las
credenciales en Base64 y no las cifra.

## 9. Pruebas ejecutadas

Se ejecutó un arnés de integración externo contra la base LocalDB y contra el servicio
self-hosted. El arnés no forma parte del entregable; las pruebas manuales equivalentes
están en `docs/PRUEBAS.md`.

| Bloque | Casos | Resultado |
| --- | --- | --- |
| CRUD de productos | 10 | 10 correctos |
| Devoluciones y validaciones | 12 | 12 correctos |
| Rollback de transacción | 1 | Correcto |
| Contrato WCF self-hosted | 2 | 2 correctos |
| **Total** | **25** | **25 correctos** |

Evidencia principal:

```text
== CRUD de productos ==
[OK] ListarProductos devuelve 8 productos
[OK] ObtenerProducto(1) devuelve Laptop Lenovo con categoria
[OK] ObtenerProducto(999) devuelve error controlado
[OK] InsertarProducto valido
[OK] InsertarProducto con precio 0 falla
[OK] InsertarProducto con categoria inexistente falla
[OK] ActualizarProducto valido
[OK] ActualizarProducto inexistente falla
[OK] EliminarProducto sin ventas
[OK] EliminarProducto con ventas falla con mensaje claro

== RegistrarDevolucionVenta ==
[OK] Lista vacia
[OK] Motivo obligatorio
[OK] Venta inexistente
[OK] Producto que no pertenece a la venta
[OK] Cantidad menor o igual a cero
[OK] Producto duplicado
[OK] Cantidad mayor que la pendiente
[OK] Devolucion parcial exitosa
[OK] Stock: producto 1 reingresa (11), producto 2 no reingresa (20)
[OK] Pendiente se reduce tras devolucion parcial
[OK] Devolucion completa exitosa
[OK] Stock tras devolucion completa (16 y 9)

== Rollback de la transaccion ==
[OK] Rollback por producto inexistente deja la cabecera sin grabar

== Contrato WCF (self-host) ==
[OK] WCF ListarProductos
[OK] WCF ObtenerProducto(1)

RESULTADO: TODAS LAS PRUEBAS PASARON
```

Resultados funcionales destacados:

- Devolución parcial de la venta 1: monto `3620,00`, tipo `Parcial`, stock del producto 1
  de `10` a `11` y stock del producto 2 sin cambios.
- Devolución completa de la venta 2: monto `2050,00`, tipo `Completa`, stock del producto
  3 de `15` a `16` y del producto 4 de `8` a `9`.
- Rollback: una falla durante el detalle no dejó la cabecera registrada.

### 9.1 Compilación

```text
ServicioProductos -> ...\ServicioProductos\bin\ServicioProductos.dll
```

La solución compila en configuración Debug sin errores.

### 9.2 Verificaciones de seguridad

| Punto | Estado |
| --- | --- |
| XML de `Web.config` | Verificado |
| Sintaxis de los scripts de PowerShell | Verificada |
| Compilación con la configuración segura | Verificada |
| Certificado, binding HTTPS y autenticación básica en IIS | NO VERIFICADO |
| Llamada WCF con credenciales sobre HTTPS | NO VERIFICADO |

El equipo de desarrollo no tiene IIS, permisos de administrador ni certificado
configurado. Los pasos pendientes están en `docs/CHECKLIST_MANUAL.md`.

## 10. Pendientes y limitaciones

- **IIS:** no se pudo verificar HTTPS, el binding del certificado ni la autenticación
  básica en un IIS real.
- **Cliente:** `TestCliente` no está incluido en el repositorio. El contrato cambió de
  `Producto` a clases de respuesta, por lo que un cliente externo debe actualizar su
  referencia de servicio.
- **Base para IIS:** LocalDB es una instancia por usuario. Para publicar en IIS hay que
  usar una instancia de SQL Server accesible por el pool o ajustar la cadena.
- **Esquema:** no se encontró el script `pa2_pa3_esquema.sql`; el esquema se reconstruyó
  y se documentó como supuesto.
- **Datos de prueba:** los datos de `sql/reset_datos.sql` son de apoyo para las pruebas,
  no provienen del enunciado.
- **Autorización:** solo se implementó autenticación; no hay roles ni permisos por
  operación.

## 11. Supuestos

1. El esquema de la base se reconstruyó a partir del enunciado y del modelo esperado.
2. `TipoDevolucion` es `Completa` cuando no queda cantidad pendiente en toda la venta;
   en caso contrario es `Parcial`.
3. Los procedimientos auxiliares de existencia y de ventas asociadas son internos y no
   forman parte del contrato del servicio.
4. El precio de la devolución es el precio registrado en `DetalleVenta`.
5. El stock solo se modifica cuando el cliente indica `ReingresaInventario = true`.
6. La base de desarrollo es LocalDB; la base de IIS debe configurarse aparte.
7. El usuario demo de autenticación básica es un usuario local de Windows.
8. El certificado autofirmado es solo para la demostración local.

## 12. Archivos principales

| Archivo | Contenido |
| --- | --- |
| `sql/00_esquema_base.sql` | Creación del esquema |
| `sql/procedimientos.sql` | Procedimientos almacenados |
| `sql/reset_datos.sql` | Datos de prueba y reinicio de identidades |
| `ServicioProductos/IProductoService.cs` | Contrato del servicio |
| `ServicioProductos/ProductoService.svc.cs` | Implementación y validaciones |
| `ServicioProductos/Models/Producto.cs` | DTO de producto |
| `ServicioProductos/Models/Respuestas.cs` | Clases de respuesta |
| `ServicioProductos/Data/DesavaPAContext.cs` | Contexto EF6 |
| `ServicioProductos/Data/Entidades.cs` | Entidades internas |
| `ServicioProductos/Data/ProductoData.cs` | Acceso a datos de productos |
| `ServicioProductos/Data/DevolucionData.cs` | Acceso a datos de devoluciones |
| `ServicioProductos/Web.config` | Configuración de EF y seguridad |
| `docs/PRUEBAS.md` | Plan de pruebas manuales |
| `docs/CHECKLIST_MANUAL.md` | Pasos de IIS, certificado y autenticación |
| `scripts/crear_certificado.ps1` | Certificado y binding HTTPS |
| `scripts/crear_usuario_demo.ps1` | Usuario local para Basic |

## 13. Conclusiones

PA2 y PA3 quedaron implementados sobre la arquitectura existente, sin cambiar de
tecnología y sin exponer entidades de Entity Framework. El acceso a datos está
centralizado en procedimientos almacenados parametrizados y la devolución se registra
de forma atómica.

La funcionalidad de negocio fue verificada con 25 comprobaciones de integración. La
parte de seguridad quedó configurada y documentada, pero no pudo ejecutarse en este
equipo por falta de IIS y permisos de administrador; se entrega como pendiente de
verificación manual.
