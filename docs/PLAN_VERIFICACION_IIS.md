# Plan de verificación IIS - PA3

## Objetivo

Completar la verificación pendiente de HTTPS y autenticación básica sobre IIS, y probar
una llamada WCF real con credenciales.

## Restricciones

- No modificar ni eliminar `C:\inetpub\wwwroot\ServicioProductos` (publicación vieja).
- No modificar `Default Web Site` ni sus bindings.
- Crear un sitio y un pool nuevos: `ServicioProductosPA3`.
- Publicar en una carpeta nueva: `C:\inetpub\wwwroot\ServicioProductosPA3`.
- No guardar contraseñas en el repositorio.
- No cambiar la configuración global de IIS más allá de habilitar el módulo de
  autenticación básica.
- Todos los cambios deben poder revertirse: eliminar el sitio, el pool, la carpeta, el
  certificado y los usuarios creados para la demo.

## Fase 0 - Estado actual

- IIS completo instalado; `W3SVC` en ejecución.
- `IIS-BasicAuthentication` deshabilitado.
- `WCF-HTTP-Activation45` y `IIS-ASPNET45` habilitados.
- Sin sitio `ServicioProductos` en IIS; solo `Default Web Site`.
- Puerto 443 libre.
- Solo LocalDB, sin servicio SQL Server.

## Fase 1 - Publicación

1. Compilar en Release.
2. Crear `C:\inetpub\wwwroot\ServicioProductosPA3`.
3. Copiar `ProductoService.svc`, `Web.config`, `ServicioProductos.dll`,
   `EntityFramework.dll` y `EntityFramework.SqlServer.dll`.
4. Ajustar solo la copia publicada de `Web.config` para usar la instancia compartida de
   LocalDB: `(localdb)\.\DesavaPA_Compartida`.

## Fase 2 - IIS

1. Crear el pool `ServicioProductosPA3` (.NET v4.0, Integrated, ApplicationPoolIdentity,
   `loadUserProfile=true`).
2. Crear el sitio `ServicioProductosPA3` apuntando a la carpeta nueva.
3. Crear un certificado autofirmado para `localhost` y enlazarlo al binding HTTPS 443.
4. Eliminar el binding HTTP temporal del sitio nuevo.
5. Habilitar `IIS-BasicAuthentication`.
6. En el sitio nuevo: deshabilitar anónimo y habilitar básico.

## Fase 3 - Base de datos

1. Compartir la instancia LocalDB como `DesavaPA_Compartida`.
2. Crear un login SQL para `IIS APPPOOL\ServicioProductosPA3`.
3. Crear el usuario en `DesavaPA` y darle lectura, escritura y ejecución de SP.
4. Verificar que el pool puede abrir la base.

Si la instancia compartida no acepta la identidad del pool, el plan alternativo es crear
un usuario local `desava_app`, ejecutar los scripts SQL con ese usuario y configurar el
pool con esa identidad.

## Fase 4 - Autenticación

1. Crear el usuario local `desava_demo` con contraseña segura.
2. Verificar `401` sin credenciales.
3. Verificar `200` con credenciales sobre WSDL.
4. Verificar que HTTP no devuelve el WSDL.

## Fase 5 - Prueba funcional

1. Compilar un cliente WCF temporal con `BasicHttpBinding`, `Transport` y credenciales
   Basic.
2. Invocar `ListarProductos` contra `https://localhost/ProductoService.svc`.
3. Verificar que devuelve 8 productos.
4. Invocar `ObtenerProducto(1)` y verificar Laptop Lenovo.

## Fase 6 - Evidencia y entrega

1. Guardar salidas de `curl` y del cliente en `docs/entrega/evidencias/`.
2. Actualizar `docs/CHECKLIST_MANUAL.md` con estados `VERIFICADO`.
3. Actualizar `docs/informe_borrador.md` con los resultados reales.
4. Generar la carpeta `docs/entrega/` con el informe final y los anexos.
5. No incluir contraseñas ni rutas absolutas en los documentos de entrega.

## Rollback

```powershell
Remove-Website -Name "ServicioProductosPA3" -ErrorAction SilentlyContinue
Remove-WebAppPool -Name "ServicioProductosPA3" -ErrorAction SilentlyContinue
Remove-Item "C:\inetpub\wwwroot\ServicioProductosPA3" -Recurse -Force -ErrorAction SilentlyContinue
Remove-LocalUser -Name "desava_demo" -ErrorAction SilentlyContinue
sqllocaldb unshare MSSQLLocalDB -ErrorAction SilentlyContinue
```

El certificado autofirmado se puede eliminar desde `Cert:\LocalMachine\My` y
`Cert:\LocalMachine\Root` por su huella.

## Criterios de aceptación

- `https://localhost/ProductoService.svc?wsdl` devuelve `401` sin credenciales.
- La misma URL devuelve `200` con `desava_demo`.
- `http://localhost/ProductoService.svc?wsdl` no devuelve `200`.
- El cliente WCF con credenciales obtiene 8 productos.
- La documentación queda actualizada y sin secretos.
