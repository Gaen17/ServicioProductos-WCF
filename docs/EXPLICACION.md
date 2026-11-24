# Explicación técnica del avance

Este documento explica cómo está armado el servicio y por qué se tomaron las decisiones
principales. Sirve para estudiar el código y para defender el trabajo.

## 1. Mapa del proyecto

```text
ServicioProductos/
├── IProductoService.cs          Contrato WCF
├── ProductoService.svc.cs       Implementación y validaciones
├── Models/
│   ├── Producto.cs              DTO público de producto
│   └── Respuestas.cs            Respuestas de cada operación
├── Data/
│   ├── DesavaPAContext.cs       Contexto EF6
│   ├── Entidades.cs             Entidades internas de EF
│   ├── SpDtos.cs                Resultados internos de los SP
│   ├── ProductoData.cs          Acceso a datos de productos
│   └── DevolucionData.cs        Acceso a datos de devoluciones
├── Web.config                   EF, endpoint y seguridad
└── sql/
    ├── 00_esquema_base.sql
    ├── procedimientos.sql
    └── reset_datos.sql
```

La regla de oro: **el cliente nunca ve una entidad de Entity Framework**. El contrato
solo expone `Producto`, `ItemDevolucionDto` y las clases de respuesta.

## 2. Recorrido de una llamada

Tomemos `ListarProductos`:

1. El cliente WCF envía la solicitud al endpoint.
2. WCF deserializa el mensaje y llama a `ProductoService.ListarProductos`.
3. El servicio llama a `ProductoData.Listar`.
4. `ProductoData` crea un `DesavaPAContext` y ejecuta:

   ```csharp
   db.Database.SqlQuery<ProductoSp>("EXEC dbo.usp_Producto_Listar").ToList();
   ```

5. EF abre la conexión, ejecuta el procedimiento y mapea las columnas del resultado a
   las propiedades de `ProductoSp`.
6. El servicio convierte cada `ProductoSp` en `Producto` con el método `Mapear`.
7. El servicio arma `RespuestaListaProductos` con `Exito`, `Mensaje` y la lista.
8. WCF serializa la respuesta y la envía al cliente.

En una inserción cambia el paso 4: se ejecuta `usp_Producto_Insertar`, que devuelve el
nuevo `Id`; después se vuelve a consultar el producto para devolver los datos completos.

## 3. Entity Framework 6 sin migraciones

El contexto está en `Data/DesavaPAContext.cs`:

```csharp
static DesavaPAContext()
{
    Database.SetInitializer<DesavaPAContext>(null);
}
```

`SetInitializer(null)` le dice a EF que no cree, no borre y no migre la base. El esquema
es responsabilidad de los scripts SQL.

El contexto tiene `DbSet` para todas las tablas, pero las consultas del servicio no usan
LINQ contra esos `DbSet`. Se usan solamente para que el modelo represente el esquema y
para tener el `Database` disponible.

`Configuration.LazyLoadingEnabled = false` evita que una navegación dispare consultas
inesperadas al serializar o recorrer objetos.

### 3.1 ¿Por qué no se usan migraciones?

Porque la base ya existe y el enunciado pide trabajar sobre ella. Las migraciones de EF
agregan una capa de administración de esquema que no se necesita y que podría intentar
modificar tablas reales. Los scripts SQL son explícitos y reproducibles.

## 4. Acceso a datos con procedimientos almacenados

Todas las operaciones usan `Database.SqlQuery<T>` o `Database.ExecuteSqlCommand` con
parámetros `SqlParameter`.

Ejemplo de consulta:

```csharp
db.Database.SqlQuery<ProductoSp>(
    "EXEC dbo.usp_Producto_Obtener @Id",
    new SqlParameter("@Id", SqlDbType.Int) { Value = id })
    .FirstOrDefault();
```

Ejemplo de escritura:

```csharp
db.Database.ExecuteSqlCommand(
    "EXEC dbo.usp_Producto_SumarStock @IdProducto, @Cantidad",
    new SqlParameter("@IdProducto", SqlDbType.Int) { Value = idProducto },
    new SqlParameter("@Cantidad", SqlDbType.Int) { Value = cantidad });
```

Puntos importantes:

- El nombre del procedimiento es fijo; los valores viajan como parámetros.
- No se concatena SQL con datos del usuario.
- `SqlQuery<T>` mapea las columnas del resultado por nombre a las propiedades del DTO.
- `Single()` se usa cuando el procedimiento garantiza una fila; `FirstOrDefault()` cuando
  el resultado puede ser vacío.

## 5. DTO y clases de respuesta

### 5.1 `Producto`

Es un `[DataContract]` con los campos que el cliente necesita:

- `Id`, `Nombre`, `Descripcion`, `Precio`, `Stock`
- `IdCategoria`, `CategoriaNombre`, `UrlImagen`

No tiene navegaciones ni atributos de EF. Si mañana cambia una tabla, el contrato no
cambia automáticamente.

### 5.2 Respuestas

Cada operación tiene su clase:

- `RespuestaListaProductos`: `Exito`, `Mensaje`, `Productos`.
- `RespuestaProducto`: `Exito`, `Mensaje`, `Producto`.
- `RespuestaOperacion`: `Exito`, `Mensaje`.
- `RespuestaDevolucion`: `Exito`, `Mensaje`, `IdDevolucion`, `TipoDevolucion`,
  `MontoTotalDevuelto`.

No se usa una clase genérica porque cada operación tiene datos distintos y el contrato
queda más claro. Tampoco se devuelven excepciones: el error se representa con
`Exito = false` y un mensaje en español.

## 6. Validaciones de PA2

Las validaciones están en `ProductoService.ValidarProducto`:

1. Nombre obligatorio y hasta 100 caracteres.
2. Categoría mayor que cero.
3. Categoría existente, consultada con `usp_Categoria_Existe`.
4. Precio mayor que cero.
5. Stock mayor o igual que cero.
6. Descripción de hasta 255 caracteres.
7. URL de imagen de hasta 500 caracteres.

Antes de validar, `Normalizar` recorta espacios y convierte cadenas vacías en `null`.

En eliminar se verifica que el producto exista y que no tenga ventas
(`usp_Producto_TieneVentas`). Si a pesar de la validación la base rechaza la operación
por clave foránea, el `catch (SqlException ex) when (ex.Number == 547)` traduce el error
a un mensaje entendible.

## 7. Devoluciones de PA3

### 7.1 Datos que recibe

```csharp
RegistrarDevolucionVenta(
    int idVenta,
    string motivo,
    List<ItemDevolucionDto> items)
```

Cada ítem tiene `IdProducto`, `Cantidad` y `ReingresaInventario`.

### 7.2 Orden de validaciones

1. Lista no vacía.
2. Motivo obligatorio y hasta 255 caracteres.
3. Venta existente.
4. Productos pertenecientes a la venta.
5. Cantidades mayores que cero.
6. Sin productos duplicados.
7. Cantidad menor o igual a la pendiente.

Las validaciones se hacen antes de abrir la transacción. Si alguna falla, no se escribe
nada en la base.

### 7.3 Cálculo de pendientes

`usp_Venta_ObtenerPendientes` usa dos CTE:

```sql
VentasPorProducto:      SUM(Cantidad) por IdProducto desde DetalleVenta
DevolucionesPorProducto: SUM(Cantidad) por IdProducto desde DetalleDevolucion
```

Luego resta:

```text
CantidadPendiente = CantidadVendida - CantidadDevuelta
```

El resultado también incluye `PrecioVenta`, tomado de `DetalleVenta`.

### 7.4 Cálculo del monto

```csharp
montoTotal = items.Sum(i => i.Cantidad * pendiente.PrecioVenta);
```

Se usa el precio de la venta original, no el precio actual del producto. Si el precio
del producto cambió después de la venta, la devolución igual devuelve el precio que se
cobró.

### 7.5 Tipo de devolución

```csharp
var quedaPendiente = pendientes.Any(p =>
    p.CantidadPendiente - devueltoDelProducto > 0);

var tipo = quedaPendiente ? "Parcial" : "Completa";
```

Si después de esta devolución no queda ninguna unidad pendiente en ningún producto de
la venta, es `Completa`. Si queda algo, es `Parcial`.

### 7.6 Transacción

`DevolucionData.Registrar` hace todo dentro de una transacción:

```csharp
using (var db = new DesavaPAContext())
using (var transaccion = db.Database.BeginTransaction())
{
    try
    {
        // 1. Insertar cabecera
        // 2. Insertar detalles
        // 3. Sumar stock si corresponde
        transaccion.Commit();
        return idDevolucion;
    }
    catch
    {
        transaccion.Rollback();
        throw;
    }
}
```

Todos los comandos usan el mismo contexto y la misma conexión, por eso participan de la
misma transacción. Si falla el detalle o la suma de stock, la cabecera también se
deshace.

### 7.7 Stock

El stock se suma solamente cuando el ítem tiene `ReingresaInventario = true`. Esto
permite representar mercadería devuelta que no vuelve a la venta porque está dañada.

## 8. Seguridad

### 8.1 HTTPS

El endpoint usa `basicHttpBinding` con:

```xml
<security mode="Transport">
  <transport clientCredentialType="Basic" />
</security>
```

`Transport` obliga a que la comunicación viaje por HTTPS. Sin HTTPS, el endpoint no
acepta la configuración.

### 8.2 Autenticación básica

IIS recibe el encabezado `Authorization: Basic ...`, valida el usuario y la contraseña
contra una cuenta de Windows y deja pasar la solicitud. WCF usa
`userNamePasswordValidationMode="Windows"`.

La autenticación básica codifica las credenciales en Base64, que es reversible. Por eso
solo se usa sobre HTTPS: el canal cifrado protege las credenciales.

### 8.3 Certificado

`scripts/crear_certificado.ps1`:

1. Crea un certificado autofirmado para `localhost`.
2. Lo agrega al almacén de confianza si se usa `-ConfiarEnEquipo`.
3. Crea el binding HTTPS del sitio.
4. Asocia el certificado al binding.

En un ambiente real se reemplaza por un certificado emitido por una entidad certificadora.

### 8.4 Usuario demo

`scripts/crear_usuario_demo.ps1` crea un usuario local de Windows. IIS lo valida con
autenticación básica. El script no guarda la contraseña; la recibe como parámetro
seguro.

## 9. Preguntas de defensa

**¿Por qué usás Entity Framework si todo va por procedimientos almacenados?**

Porque el enunciado lo exige y porque EF aporta la conexión, el mapeo de resultados y el
manejo de la transacción. No se usa EF como ORM de consultas: no hay LINQ a `DbSet` en
la lógica del servicio.

**¿Por qué no hay migraciones?**

Porque la base ya existe y el esquema lo controlan los scripts SQL. `SetInitializer(null)`
evita que EF intente crear o modificar la base.

**¿Por qué no devolvés las entidades de EF?**

Porque acoplan el contrato al esquema, pueden exponer navegaciones y ciclos, y filtran
columnas que el cliente no necesita. Los DTO son estables y explícitos.

**¿Por qué una clase de respuesta por operación?**

Porque cada operación devuelve datos distintos. Una clase genérica obliga a usar `object`
o a adivinar el contenido. Las clases concretas documentan el contrato.

**¿Cómo evitás la inyección SQL?**

Todos los valores viajan como `SqlParameter`; nunca se concatenan en el texto del
comando. Los procedimientos reciben parámetros tipados.

**¿Qué pasa si el cliente manda una categoría que no existe?**

La validación la rechaza antes de llegar a la base con
`La categoria indicada no existe.` Si la base igual devuelve el error 547, el `catch`
lo traduce al mismo mensaje.

**¿Cómo se calcula la cantidad pendiente?**

El SP agrupa las cantidades vendidas y las ya devueltas por producto, y resta. También
devuelve el precio de venta para calcular el monto.

**¿Cuándo una devolución es completa?**

Cuando después de registrarla no queda ninguna cantidad pendiente en toda la venta.

**¿Cómo asegurás que no queden datos a medias?**

La cabecera, los detalles y el stock se escriben dentro de una transacción. Si algo
falla, se hace rollback y no queda ninguna fila parcial.

**¿Por qué el stock no siempre se suma?**

Porque el cliente indica si el producto reingresa al inventario. Un producto dañado
puede devolverse sin volver al stock.

**¿Por qué Basic y no Windows?**

El enunciado pide autenticación básica. Basic permite enviar usuario y contraseña de
forma explícita; se compensa el riesgo con HTTPS.

**¿Qué queda pendiente?**

Verificar en IIS el certificado, el binding HTTPS, la autenticación básica y una llamada
WCF con credenciales. En el equipo de desarrollo no hay IIS ni permisos de administrador.
También queda pendiente actualizar un cliente externo si existía `TestCliente`, porque
el contrato cambió.

**¿Por qué LocalDB no sirve para IIS?**

LocalDB es una instancia por usuario. El pool de aplicaciones de IIS corre con otra
identidad y no ve la instancia del desarrollador. Para IIS hay que usar SQL Server
Express o una instancia completa, y ajustar la cadena de conexión.

**¿Qué pasa si se intenta devolver dos veces la misma venta?**

La segunda vez la cantidad pendiente es cero y la validación rechaza la operación con un
mensaje que indica que se superó la cantidad pendiente.

**¿Por qué el monto usa el precio de `DetalleVenta` y no el precio actual?**

Porque el precio actual puede haber cambiado. La devolución debe devolver el precio que
realmente se cobró en la venta.

## 10. Limitaciones y supuestos

- El esquema se reconstruyó porque no se entregó el script `pa2_pa3_esquema.sql`.
- La regla de `Completa`/`Parcial` es una decisión documentada.
- Los datos de `reset_datos.sql` son de prueba.
- La seguridad de IIS no se pudo ejecutar en este equipo.
- No hay autorización por roles; cualquier usuario de Windows válido puede llamar al
  servicio.
- `TestCliente` no está en el repositorio; un cliente existente debe actualizar su
  referencia.
