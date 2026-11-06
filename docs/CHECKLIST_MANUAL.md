# Checklist manual - HTTPS y autenticación básica

Este documento reúne los pasos que requieren IIS, permisos de administrador y un
certificado. En el equipo de desarrollo no hay IIS ni permisos para crear el certificado,
por lo que **estos puntos quedan como NO VERIFICADOS** y deben completarse en el equipo
de entrega.

La versión funcional sin seguridad quedó guardada en el tag `baseline-sin-seguridad`.
Las pruebas de `docs/PRUEBAS.md` aplican a esa línea base.

## 1. Requisitos

- Windows con IIS y los siguientes componentes instalados:
  - WCF Services > HTTP Activation.
  - IIS > World Wide Web Services > Security > Basic Authentication.
  - ASP.NET 4.7 o superior registrado en IIS.
- SQL Server accesible por la identidad del pool de aplicaciones.
- PowerShell ejecutado como administrador.

## 2. Publicación en IIS

1. Publicar el proyecto `ServicioProductos` en una carpeta del servidor o crear el sitio
   directamente desde Visual Studio.
2. Crear un sitio llamado `ServicioProductos` con un pool de aplicaciones:
   - .NET CLR Version: `v4.0`.
   - Managed pipeline mode: `Integrated`.
3. Dar permiso de lectura a la identidad del pool sobre la carpeta publicada.
4. Verificar que el sitio tenga un binding HTTPS en el puerto 443.

### 2.1 Cadena de conexión

La cadena actual apunta a `(localdb)\MSSQLLocalDB`. LocalDB es una instancia por usuario:
la identidad del pool de IIS no ve la instancia del desarrollador. Para IIS hay dos
opciones:

- Usar SQL Server Express o una instancia completa, crear la base `DesavaPA` y dar acceso
  a la identidad del pool (`IIS APPPOOL\ServicioProductos`) con autenticación integrada.
- Usar autenticación SQL con un usuario de aplicación y una cadena con `User ID` y
  `Password`. No subir credenciales reales al repositorio.

Ajustar `connectionStrings` en `Web.config` antes de publicar. Ejecutar los scripts de
`sql/` contra la instancia elegida.

## 3. Certificado y binding HTTPS

Desde la raíz del repositorio, con PowerShell como administrador:

```powershell
.\scripts\crear_certificado.ps1 -NombreSitio "ServicioProductos" -DnsName "localhost" -ConfiarEnEquipo
```

El script:

1. Crea un certificado autofirmado en `Cert:\LocalMachine\My`.
2. Lo agrega al almacén de entidades de confianza si se usa `-ConfiarEnEquipo`.
3. Crea el binding HTTPS del sitio si no existe.
4. Asocia el certificado al binding.

Si el sitio usa otro nombre o puerto:

```powershell
.\scripts\crear_certificado.ps1 -NombreSitio "MiSitio" -DnsName "mi-servidor" -Puerto 8443
```

Para un ambiente real se reemplaza el certificado autofirmado por uno emitido por una
entidad certificadora. El certificado autofirmado es solo para la demostración local.

## 4. Autenticación básica en IIS

1. Abrir IIS Manager.
2. Seleccionar el sitio `ServicioProductos`.
3. Abrir `Authentication`.
4. Deshabilitar `Anonymous Authentication`.
5. Habilitar `Basic Authentication`.

También se puede hacer por línea de comandos:

```powershell
Import-Module WebAdministration

Set-WebConfigurationProperty -Filter "system.webServer/security/authentication/anonymousAuthentication" `
    -Name enabled -Value $false -PSPath "IIS:\Sites\ServicioProductos"

Set-WebConfigurationProperty -Filter "system.webServer/security/authentication/basicAuthentication" `
    -Name enabled -Value $true -PSPath "IIS:\Sites\ServicioProductos"
```

Alternativa con `appcmd`:

```powershell
& "$env:windir\System32\inetsrv\appcmd.exe" set config "ServicioProductos" /section:anonymousAuthentication /enabled:false
& "$env:windir\System32\inetsrv\appcmd.exe" set config "ServicioProductos" /section:basicAuthentication /enabled:true
```

La autenticación básica envía usuario y contraseña codificados en Base64, no cifrados.
Por eso **solo debe usarse sobre HTTPS**.

## 5. Usuario demo

Crear un usuario local de Windows para la prueba:

```powershell
$clave = Read-Host "Contrasena del usuario demo" -AsSecureString
.\scripts\crear_usuario_demo.ps1 -Usuario "desava_demo" -Password $clave
```

El script crea el usuario si no existe o actualiza su contraseña. La cuenta debe poder
iniciar sesión en el equipo; no se le asignan roles de administrador.

Para eliminar el usuario al terminar la demostración:

```powershell
Remove-LocalUser -Name "desava_demo"
```

## 6. Verificación del transporte

Reemplazar `LaClave` por la contraseña elegida.

### 6.1 Sin credenciales debe responder 401

```powershell
curl.exe -k -i "https://localhost/ProductoService.svc?wsdl"
```

Resultado esperado: `HTTP/1.1 401 Unauthorized` y encabezado
`WWW-Authenticate: Basic`.

### 6.2 Con credenciales debe responder 200

```powershell
curl.exe -k -u desava_demo:LaClave -i "https://localhost/ProductoService.svc?wsdl"
```

Resultado esperado: `HTTP/1.1 200 OK` y el WSDL del servicio.

Si el certificado se agregó al almacén de confianza, se puede quitar `-k`.

### 6.3 HTTP no debe exponer el servicio

```powershell
curl.exe -i "http://localhost/ProductoService.svc?wsdl"
```

Resultado esperado: error de conexión, `403` o `404`; nunca `200 OK` con el WSDL.
Si el sitio conserva un binding HTTP, eliminarlo desde IIS o dejar solo el binding HTTPS.

## 7. Cliente WCF con credenciales

El cliente debe usar un binding con seguridad de transporte y credenciales Basic:

```xml
<bindings>
  <basicHttpBinding>
    <binding name="BasicAuthHttps">
      <security mode="Transport">
        <transport clientCredentialType="Basic" />
      </security>
    </binding>
  </basicHttpBinding>
</bindings>
```

Al crear el cliente:

```csharp
var cliente = new ProductoServiceClient(
    "BasicAuthHttps",
    "https://localhost/ProductoService.svc");

cliente.ClientCredentials.UserName.UserName = "desava_demo";
cliente.ClientCredentials.UserName.Password = "LaClave";

var respuesta = cliente.ListarProductos();
```

El nombre `ProductoServiceClient` depende del proxy generado al agregar la referencia de
servicio. Ajustar el nombre si Visual Studio generó otro.

## 8. Checklist

| # | Punto | Estado | Evidencia |
| --- | --- | --- | --- |
| 1 | Compilación con el `Web.config` seguro | VERIFICADO | La solución compila en Debug |
| 2 | Sintaxis de los scripts de PowerShell | VERIFICADO | Parseo sin errores |
| 3 | Certificado autofirmado creado | NO VERIFICADO | Requiere administrador |
| 4 | Certificado agregado a entidades de confianza | NO VERIFICADO | Requiere administrador |
| 5 | Binding HTTPS en IIS | NO VERIFICADO | Requiere IIS |
| 6 | Autenticación anónima deshabilitada | NO VERIFICADO | Requiere IIS |
| 7 | Autenticación básica habilitada | NO VERIFICADO | Requiere IIS |
| 8 | `GET ?wsdl` sin credenciales devuelve 401 | NO VERIFICADO | Requiere IIS y certificado |
| 9 | `GET ?wsdl` con credenciales devuelve 200 | NO VERIFICADO | Requiere IIS y certificado |
| 10 | HTTP no devuelve el WSDL | NO VERIFICADO | Requiere IIS |
| 11 | Cliente WCF invoca una operación con credenciales | NO VERIFICADO | Requiere IIS y certificado |
| 12 | Base de datos accesible desde el pool de IIS | NO VERIFICADO | Requiere ajustar la cadena y permisos |

## 9. Notas de seguridad

- La autenticación básica sin HTTPS expone las credenciales. El binding configurado
  exige `Transport`, por lo que el servicio no acepta Basic sobre HTTP.
- El certificado autofirmado es válido solo para la demostración local.
- El usuario demo debe eliminarse o deshabilitarse después de la entrega.
- Cualquier usuario de Windows válido puede autenticarse; esta entrega implementa
  autenticación, no autorización por roles. Si se requiere autorización, debe agregarse
  en una etapa posterior.
- Los metadatos WSDL quedan disponibles solo por HTTPS y requieren credenciales.
- La navegación de directorios quedó deshabilitada en `Web.config`.

## 10. Volver a la línea base sin seguridad

Para ejecutar las pruebas funcionales de `docs/PRUEBAS.md` sin IIS ni certificado, usar
un worktree sobre el tag:

```powershell
git worktree add ..\ServicioProductos-baseline baseline-sin-seguridad
```

Ese worktree conserva el `Web.config` con HTTP y sin autenticación. No modificar la rama
`pa2-pa3` para volver atrás.
