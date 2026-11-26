# Explicación del cliente MVC (TestCliente)

Cliente ASP.NET MVC 5 (.NET Framework 4.7.2) que consume el servicio WCF `ProductoService`
por HTTPS con autenticación Basic. Nunca accede a la base de datos: todo pasa por el servicio.

## Inicio (`/`)

- **Qué hace:** página de bienvenida con accesos a Devoluciones y a la prueba de conexión.
- **Cómo funciona:** solo muestra la vista; no llama al servicio.
- **Por qué:** sirve como punto de entrada simple usando el layout común con Bootstrap.

## Prueba de conexión (`/Prueba`)

- **Qué hace:** comprueba que el cliente puede hablar con el servicio y lista los productos.
- **Cómo funciona:** llama a `ListarProductos` con las credenciales de `appSettings` y muestra la tabla o el error.
- **Validaciones:** distingue credenciales rechazadas, HTTPS fallido, servicio caído y tiempo agotado.
- **Por qué:** es una pantalla de diagnóstico rápida antes de probar las demás.

## Listado de devoluciones (`/Devoluciones`)

- **Qué hace:** muestra las devoluciones registradas con fecha, venta, motivo, tipo y monto.
- **Cómo funciona:** llama a `ListarDevoluciones`; cada fila tiene "Ver detalle" y arriba está "Nueva devolución".
- **Mensajes:** muestra el éxito de una devolución recién registrada (TempData) o el error del servicio.
- **Por qué:** es la pantalla principal del módulo y el destino tras confirmar una devolución.

## Detalle de devolución (`/Devoluciones/Detalle/{id}`)

- **Qué hace:** muestra los productos devueltos en una devolución: cantidad, precio y subtotal.
- **Cómo funciona:** llama a `VerDevolucionDetalle(idDevolucion)` con el número de la fila elegida.
- **Validaciones:** si el número no es válido, avisa sin llamar al servicio.
- **Por qué:** separa el resumen (listado) del detalle para que el listado sea fácil de leer.

## Nueva devolución (`/Devoluciones/Nueva`)

- **Qué hace:** registra la devolución de productos de una venta.
- **Flujo:** 1) escribir la venta y "Buscar productos" (`ObtenerProductosVenta`); 2) marcar productos, cantidad y "Agregar al detalle"; 3) escribir el motivo y "Confirmar devolución" (`RegistrarDevolucionVenta`).
- **Validaciones del cliente:** venta vacía o no numérica, motivo vacío, ningún producto, cantidad no numérica o ≤ 0, y cantidad mayor que la pendiente.
- **Validación final:** la hace el servicio; si rechaza la devolución, su mensaje se muestra tal cual.
- **Por qué:** todo ocurre en el servidor (sin JavaScript) y el detalle viaja en campos ocultos, así no se usa sesión.

## Decisiones comunes

- **ViewModels propios:** las vistas no usan las clases del proxy; si el servicio cambia, solo cambia la capa `Servicios`.
- **`IDevolucionesServicio`:** el controlador depende de esta interfaz, que traduce las respuestas del proxy a ViewModels; si el servicio responde `Exito=false`, su mensaje se muestra tal cual.
- **Errores:** los fallos de conexión se muestran con mensajes claros, sin detalles técnicos ni credenciales.
