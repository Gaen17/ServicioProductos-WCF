# Evidencias de verificación - PA3: HTTPS y autenticación básica

**Fecha de verificación:** 05/11/2026  
**Entorno:** IIS local en equipo de desarrollo, Windows 11

---

## 1. Sitio y pool de aplicaciones

```
Sitio  : ServicioProductosPA3
Estado : Started
Ruta   : C:\inetpub\wwwroot\ServicioProductosPA3

Pool   : ServicioProductosPA3
CLR    : v4.0
Pipeline: Integrated
Identidad: ApplicationPoolIdentity
```

## 2. Certificado SSL

```
Subject   : CN=localhost
Thumbprint: 9A8550DE22CAE93FB3B11BD75992B2620714D556
Válido desde: 05/11/2026
Vence       : 05/11/2028
Almacén     : Cert:\LocalMachine\My
```

Certificado autofirmado creado con `scripts/crear_certificado.ps1`.

## 3. Binding HTTPS

```
Protocolo : https
Dirección : *:443
Sin binding HTTP adicional (el servicio no es accesible por HTTP)
```

## 4. Configuración de autenticación en IIS

```
anonymousAuthentication : False
basicAuthentication     : True
```

## 5. Usuario demo

```
Usuario : desava_demo
Tipo    : Usuario local de Windows
Estado  : Habilitado
Creado con: scripts/crear_usuario_demo.ps1
```

> La contraseña no se registra en el repositorio.

## 6. Pruebas de transporte

### Sin credenciales → HTTP 401

```
$ curl.exe -k -s -o NUL -w "%{http_code}" https://localhost/ProductoService.svc?wsdl
401

Cabecera recibida: WWW-Authenticate: Basic realm="localhost"
```

### Con credenciales → HTTP 200

```
$ curl.exe -k -u desava_demo:<contraseña> -s -o NUL -w "%{http_code}" https://localhost/ProductoService.svc?wsdl
200

El cuerpo contiene el WSDL XML del servicio.
```

### Sin binding HTTP → sin respuesta por HTTP 80

El sitio no tiene binding HTTP 80. Una petición a `http://localhost/ProductoService.svc?wsdl`
no obtiene respuesta (connection refused o sin binding).

## 7. Llamada SOAP autenticada

Operación `ListarProductos` invocada con credenciales:

```
Exito   : true
Mensaje : (vacío)
Productos: 8 registros devueltos
```

Resultado: el pool de aplicaciones conectó a la base de datos y ejecutó el procedimiento
almacenado `usp_Producto_Listar` con éxito.

## 8. Resumen del checklist

| # | Punto | Estado |
| --- | --- | --- |
| 3 | Certificado autofirmado creado | VERIFICADO |
| 4 | Certificado en almacén de confianza / asociado al binding | VERIFICADO |
| 5 | Binding HTTPS en IIS | VERIFICADO |
| 6 | Autenticación anónima deshabilitada | VERIFICADO |
| 7 | Autenticación básica habilitada | VERIFICADO |
| 8 | `GET ?wsdl` sin credenciales devuelve 401 | VERIFICADO |
| 9 | `GET ?wsdl` con credenciales devuelve 200 | VERIFICADO |
| 10 | HTTP no devuelve el WSDL | VERIFICADO |
| 11 | Cliente WCF invoca una operación con credenciales | VERIFICADO |
| 12 | Base de datos accesible desde el pool de IIS | VERIFICADO |
