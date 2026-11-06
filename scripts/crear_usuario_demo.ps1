<#
    Crea o actualiza un usuario local de Windows para probar la autenticacion
    basica del servicio. Requiere PowerShell como administrador.

    La contrasena se recibe por parametro para no dejarla escrita en el script.

    Ejemplo:
        $clave = Read-Host "Contrasena del usuario demo" -AsSecureString
        .\scripts\crear_usuario_demo.ps1 -Usuario "desava_demo" -Password $clave
#>
[CmdletBinding()]
param(
    [string]$Usuario = "desava_demo",
    [Parameter(Mandatory = $true)]
    [Security.SecureString]$Password
)

$ErrorActionPreference = "Stop"

$esAdministrador = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
    [Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $esAdministrador) {
    throw "Ejecute este script como administrador."
}

if (Get-LocalUser -Name $Usuario -ErrorAction SilentlyContinue) {
    Set-LocalUser -Name $Usuario -Password $Password
    Write-Host "Se actualizo la contrasena del usuario local '$Usuario'."
}
else {
    New-LocalUser `
        -Name $Usuario `
        -Password $Password `
        -FullName "Usuario demo WCF" `
        -Description "Acceso de prueba para autenticacion basica" `
        -AccountNeverExpires `
        -PasswordNeverExpires | Out-Null

    Write-Host "Usuario local '$Usuario' creado."
}

Write-Host "Habilite la autenticacion basica en IIS y deshabilite la autenticacion anonima para el sitio."
