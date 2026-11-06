# ServicioProductos WCF - PA2 y PA3

Servicio WCF en .NET Framework para la gestión de productos y el registro de
devoluciones de venta. Usa Entity Framework 6 sobre una base SQL Server existente y
concentra todo el acceso a datos en procedimientos almacenados parametrizados.

- Rama de entrega: `pa2-pa3`
- Línea base sin seguridad: tag `baseline-sin-seguridad`
- Documentación: `docs/PRUEBAS.md`, `docs/CHECKLIST_MANUAL.md`,
  `docs/informe_borrador.md`, `docs/EXPLICACION.md`

## Tecnologías

- .NET Framework 4.7.2
- WCF clásico (`basicHttpBinding`)
- Entity Framework 6.4.4
- SQL Server 2019 / LocalDB para desarrollo
- Entity Framework sin migraciones (`Database.SetInitializer<T>(null)`)

## Operaciones

| Operación | Respuesta |
| --- | --- |
| `ListarProductos()` | `RespuestaListaProductos` |
| `ObtenerProducto(int id)` | `RespuestaProducto` |
| `InsertarProducto(Producto producto)` | `RespuestaProducto` |
| `ActualizarProducto(Producto producto)` | `RespuestaProducto` |
| `EliminarProducto(int id)` | `RespuestaOperacion` |
| `RegistrarDevolucionVenta(int idVenta, string motivo, List<ItemDevolucionDto> items)` | `RespuestaDevolucion` |

Todas las respuestas incluyen `Exito` y `Mensaje`. Los errores de negocio se devuelven
con `Exito = false`, sin trazas ni excepciones técnicas.

## Estructura

```text
ServicioProductos/
├── IProductoService.cs          Contrato WCF
├── ProductoService.svc.cs       Implementación y validaciones
├── Models/                      DTO públicos
├── Data/                        Contexto EF, entidades internas y acceso a datos
├── Web.config                   EF, endpoint y seguridad
├── sql/                         Esquema, procedimientos y datos de prueba
├── docs/                        Pruebas, checklist, informe y explicación
└── scripts/                     Certificado y usuario demo para IIS
```

## Preparación

### 1. Restaurar Entity Framework

```powershell
nuget install EntityFramework -Version 6.4.4 `
  -Source https://api.nuget.org/v3/index.json `
  -OutputDirectory packages
```

### 2. Crear la base

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -f 65001 -b -i sql\00_esquema_base.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -f 65001 -b -i sql\procedimientos.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -f 65001 -b -i sql\reset_datos.sql
```

### 3. Compilar

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" `
  "ServicioProductos.sln" /p:Configuration=Debug /p:Platform="Any CPU"
```

## Ejecución

### Versión segura (rama actual)

`Web.config` exige HTTPS y autenticación básica. El servicio debe publicarse en IIS con
un certificado y autenticación básica habilitada. Los pasos están en
`docs/CHECKLIST_MANUAL.md`.

- URL de ejemplo: `https://localhost/ProductoService.svc`
- WSDL: `https://localhost/ProductoService.svc?wsdl`
- Credenciales: usuario local creado con `scripts/crear_usuario_demo.ps1`

### Versión sin seguridad (para pruebas funcionales)

Para ejecutar las pruebas de `docs/PRUEBAS.md` sin IIS ni certificado, usar un worktree
sobre el tag:

```powershell
git worktree add ..\ServicioProductos-baseline baseline-sin-seguridad
```

Ese worktree conserva el `Web.config` con HTTP y sin autenticación:

- URL: `http://localhost:60035/ProductoService.svc`
- WSDL: `http://localhost:60035/ProductoService.svc?wsdl`

## Datos de prueba

`sql/reset_datos.sql` carga:

- 4 categorías y 8 productos.
- 3 ventas con su detalle.
- Stock inicial y precios coherentes con las ventas.

El script se puede ejecutar varias veces; elimina devoluciones y ventas, vuelve a cargar
los datos y reinicia las identidades.

## Pruebas

El plan manual está en `docs/PRUEBAS.md`. Cubre:

- 17 casos de CRUD y validaciones de PA2.
- 15 casos de devoluciones de PA3.
- 4 verificaciones de contrato y mensajes.
- Rollback de la transacción documentado como prueba automatizada externa.

Resultado de la última ejecución de integración: 25 de 25 comprobaciones correctas
(10 de CRUD, 12 de devoluciones, 1 de rollback y 2 de contrato WCF self-hosted).

## Supuestos y limitaciones

- El esquema se reconstruyó desde el enunciado; no se entregó
  `pa2_pa3_esquema.sql`.
- `TipoDevolucion` es `Completa` cuando no queda cantidad pendiente en toda la venta;
  en caso contrario es `Parcial`.
- Los datos de prueba son de apoyo, no provienen del enunciado.
- `TestCliente` no está en el repositorio; un cliente externo debe actualizar su
  referencia de servicio porque el contrato cambió a clases de respuesta.
- LocalDB es una instancia por usuario: para IIS hay que usar otra instancia o ajustar
  la cadena de conexión.
- HTTPS y autenticación básica se verificaron el 05/11/2026 sobre IIS local. Estado:
  **VERIFICADO**.
- No se implementó autorización por roles; solo autenticación.

## Seguridad

- `basicHttpBinding` con `security mode="Transport"` y `clientCredentialType="Basic"`.
- Metadatos solo por HTTPS.
- Navegación de directorios deshabilitada.
- Certificado autofirmado solo para demostración local.
- La autenticación básica nunca debe usarse sin HTTPS.
