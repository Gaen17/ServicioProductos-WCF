# Pruebas del servicio

Este documento describe cómo preparar el entorno y ejecutar las pruebas funcionales de
PA2 (CRUD de productos) y PA3 (registro de devoluciones de venta).

## 1. Alcance

- PA2: listado, consulta, inserción, actualización y eliminación de productos con
  validaciones y mensajes controlados.
- PA3: registro transaccional de devoluciones totales y parciales, con reingreso
  opcional de stock.
- Fuera de alcance: seguridad HTTPS y autenticación básica (se documentan en
  `docs/CHECKLIST_MANUAL.md`).

## 2. Requisitos previos

- Windows con Visual Studio 2022 y herramientas de .NET Framework 4.7.2.
- SQL Server Express LocalDB (instancia `MSSQLLocalDB`).
- `nuget.exe` y `sqlcmd` disponibles en la línea de comandos.
- Cliente WCF Test Client
  (`C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\WcfTestClient.exe`).

## 3. Preparación

### 3.1 Restaurar paquetes

```powershell
nuget install EntityFramework -Version 6.4.4 `
  -Source https://api.nuget.org/v3/index.json `
  -OutputDirectory packages
```

### 3.2 Crear la base y los procedimientos almacenados

Ejecutar los scripts en este orden:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -f 65001 -b -i sql\00_esquema_base.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -f 65001 -b -i sql\procedimientos.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -f 65001 -b -i sql\reset_datos.sql
```

El último comando imprime `Datos de prueba restablecidos correctamente.`

### 3.3 Compilar

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" `
  "ServicioProductos.sln" /p:Configuration=Debug /p:Platform="Any CPU"
```

### 3.4 Ejecutar el servicio

Abrir `ServicioProductos.sln` en Visual Studio, establecer `ServicioProductos` como
proyecto de inicio y presionar F5 (IIS Express).

- URL del servicio: `http://localhost:60035/ProductoService.svc`
- WSDL: `http://localhost:60035/ProductoService.svc?wsdl`

Abrir WCF Test Client, agregar la URL del WSDL y desplegar `IProductoService`.
El contrato expone seis operaciones:

1. `ListarProductos`
2. `ObtenerProducto`
3. `InsertarProducto`
4. `ActualizarProducto`
5. `EliminarProducto`
6. `RegistrarDevolucionVenta`

## 4. Datos de prueba

`sql/reset_datos.sql` deja estos datos:

| Categoría | Productos |
| --- | --- |
| 1 - Electronicos | 1 Laptop Lenovo (3500,00 / stock 10), 2 Mouse Logitech (120,00 / stock 20), 3 Teclado Redragon (250,00 / stock 15), 4 Monitor Samsung (1800,00 / stock 8), 5 Auriculares Sony (450,00 / stock 12) |
| 2 - Ropa de hombre | 6 Polo basico (80,00 / stock 30) |
| 3 - Ropa de mujer | 7 Blusa floral (95,00 / stock 25) |
| 4 - Hogar | 8 Lampara de mesa (60,00 / stock 18) |

| Venta | Fecha | Detalle | Total |
| --- | --- | --- | --- |
| 1 | 2026-10-01 | producto 1 x 2, producto 2 x 3 | 7360,00 |
| 2 | 2026-10-02 | producto 3 x 1, producto 4 x 1 | 2050,00 |
| 3 | 2026-10-03 | producto 2 x 5, producto 5 x 2 | 1500,00 |

## 5. Pruebas de PA2 - CRUD de productos

Ejecutar con la base recién restablecida. Marcar la columna de resultado.

| ID | Caso | Entrada | Resultado esperado | OK |
| --- | --- | --- | --- | --- |
| P2-01 | Listar productos | sin argumentos | `Exito = true`, 8 productos, mensaje de lista obtenida | |
| P2-02 | Obtener producto existente | `id = 1` | `Exito = true`, Laptop Lenovo, categoría Electrónicos, precio 3500, stock 10 | |
| P2-03 | Obtener producto inexistente | `id = 999` | `Exito = false`, `No se encontro un producto con el Id 999.` | |
| P2-04 | Obtener con Id inválido | `id = 0` | `Exito = false`, indica que el identificador debe ser mayor que cero | |
| P2-05 | Insertar producto válido | Nombre `Producto de prueba`, Precio `10`, Stock `5`, IdCategoria `1` | `Exito = true`, producto con Id mayor que cero; aparece al listar | |
| P2-06 | Insertar sin nombre | Nombre vacío o solo espacios | `Exito = false`, `El nombre del producto es obligatorio.` | |
| P2-07 | Insertar con precio cero | Precio `0` | `Exito = false`, `El precio debe ser mayor que cero.` | |
| P2-08 | Insertar con stock negativo | Stock `-1` | `Exito = false`, `El stock debe ser mayor o igual que cero.` | |
| P2-09 | Insertar con categoría inexistente | IdCategoria `99` | `Exito = false`, `La categoria indicada no existe.` | |
| P2-10 | Insertar nombre demasiado largo | Nombre de 101 caracteres | `Exito = false`, indica máximo de 100 caracteres | |
| P2-11 | Insertar descripción demasiado larga | Descripción de 256 caracteres | `Exito = false`, indica máximo de 255 caracteres | |
| P2-12 | Insertar URL demasiado larga | UrlImagen de 501 caracteres | `Exito = false`, indica máximo de 500 caracteres | |
| P2-13 | Actualizar producto válido | Id `8`, Nombre `Lampara actualizada`, Precio `65`, Stock `20`, IdCategoria `4` | `Exito = true`; los cambios se ven al volver a obtener | |
| P2-14 | Actualizar producto inexistente | Id `999` con datos válidos | `Exito = false`, `No se encontro un producto con el Id 999.` | |
| P2-15 | Actualizar con categoría inexistente | Id `8`, IdCategoria `99` | `Exito = false`, `La categoria indicada no existe.`; el producto no cambia | |
| P2-16 | Eliminar producto sin ventas | Id `8` | `Exito = true`, `Producto eliminado correctamente.`; ya no aparece al listar | |
| P2-17 | Eliminar producto con ventas | Id `1` | `Exito = false`, `No se puede eliminar el producto porque tiene ventas asociadas.`; el producto sigue existiendo | |

## 6. Pruebas de PA3 - Devoluciones de venta

Restablecer la base antes de este bloque:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -f 65001 -b -i sql\reset_datos.sql
```

`RegistrarDevolucionVenta` recibe `idVenta`, `motivo` y una lista de items con
`IdProducto`, `Cantidad` y `ReingresaInventario`.

| ID | Caso | Entrada | Resultado esperado | OK |
| --- | --- | --- | --- | --- |
| P3-01 | Lista vacía | `items` vacío | `Exito = false`, `Debe indicar al menos un producto para devolver.` | |
| P3-02 | Motivo obligatorio | `motivo` vacío | `Exito = false`, `El motivo de la devolucion es obligatorio.` | |
| P3-03 | Motivo demasiado largo | `motivo` de 256 caracteres | `Exito = false`, indica máximo de 255 caracteres | |
| P3-04 | Venta inexistente | `idVenta = 999` | `Exito = false`, `La venta indicada no existe.` | |
| P3-05 | Producto ajeno a la venta | venta `1`, producto `3`, cantidad `1` | `Exito = false`, indica que el producto no pertenece a la venta | |
| P3-06 | Cantidad inválida | venta `1`, producto `1`, cantidad `0` | `Exito = false`, la cantidad debe ser mayor que cero | |
| P3-07 | Producto duplicado | venta `1`, dos items del producto `1` | `Exito = false`, indica que el producto está duplicado | |
| P3-08 | Cantidad mayor que la pendiente | venta `1`, producto `1`, cantidad `3` (pendiente 2) | `Exito = false`, `... supera la cantidad pendiente (2).` | |
| P3-09 | Devolución parcial | venta `1`, motivo `Producto defectuoso`, producto `1` x `1` con reingreso, producto `2` x `1` sin reingreso | `Exito = true`, `TipoDevolucion = Parcial`, `MontoTotalDevuelto = 3620.00` | |
| P3-10 | Stock tras devolución parcial | consultar productos 1 y 2 | producto 1 stock `11`; producto 2 stock `20` (no reingresó) | |
| P3-11 | Pendiente tras devolución parcial | repetir el cálculo de pendientes de la venta 1 | producto 1 pendiente `1`; producto 2 pendiente `2` | |
| P3-12 | Devolución completa de la venta 2 | venta `2`, motivo `Devolucion total`, producto `3` x `1` con reingreso, producto `4` x `1` con reingreso | `Exito = true`, `TipoDevolucion = Completa`, `MontoTotalDevuelto = 2050.00` | |
| P3-13 | Stock tras devolución completa | consultar productos 3 y 4 | producto 3 stock `16`; producto 4 stock `9` | |
| P3-14 | Devolver una venta ya devuelta | repetir la devolución de la venta 2 | `Exito = false`, la cantidad supera la pendiente (0) | |
| P3-15 | Segunda devolución parcial de la venta 1 | venta `1`, producto `1` x `1` con reingreso y producto `2` x `2` sin reingreso | `Exito = true`, `TipoDevolucion = Completa`, `MontoTotalDevuelto = 3740.00` | |

### 6.1 Rollback de la transacción

La capa `DevolucionData` inserta la cabecera, los detalles y el stock dentro de una
misma transacción. Si falla una inserción, no debe quedar ninguna fila parcial.

La prueba de rollback se ejecutó con un arnés automatizado externo que fuerza un fallo
en el detalle después de insertar la cabecera; el resultado fue: cabecera no persistida,
sin detalles y sin cambios de stock. No es reproducible desde WCF Test Client sin
alterar la base, por lo que se deja documentada como verificación automatizada.

## 7. Pruebas de contrato y mensajes

| ID | Caso | Resultado esperado | OK |
| --- | --- | --- | --- |
| PC-01 | Operaciones publicadas | El WSDL muestra las seis operaciones del contrato | |
| PC-02 | Forma de las respuestas | Cada operación devuelve `Exito`, `Mensaje` y los datos propios de la operación | |
| PC-03 | Sin entidades EF | Las respuestas no exponen clases del modelo de Entity Framework | |
| PC-04 | Sin stack traces | Ante un error de base, la respuesta trae un mensaje genérico en español y no una excepción | |

Para PC-04 se puede detener la instancia de LocalDB, invocar una operación y comprobar
que la respuesta no contiene nombres de tablas, SQL ni trazas; luego volver a iniciarla.

## 8. Restablecer los datos

Cada escenario debe partir de los datos iniciales:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -f 65001 -b -i sql\reset_datos.sql
```

El script elimina devoluciones, ventas y productos, vuelve a cargar los datos de prueba
y reinicia las identidades. Es seguro ejecutarlo varias veces.

## 9. Criterios de aceptación

- Todas las pruebas P2 y P3 devuelven los mensajes y valores indicados.
- Ninguna operación devuelve una excepción sin controlar ni datos de una entidad EF.
- La devolución parcial y la completa calculan el monto con el precio de `DetalleVenta`.
- El stock solo aumenta cuando el item tiene `ReingresaInventario = true`.
- La base queda sin filas parciales ante un fallo durante el registro de la devolución.
