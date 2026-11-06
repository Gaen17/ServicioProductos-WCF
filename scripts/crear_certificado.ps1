<#
    Crea un certificado autofirmado para el sitio del servicio y configura el
    binding HTTPS en IIS. Requiere PowerShell como administrador y el modulo
    WebAdministration.

    Ejemplo:
        .\scripts\crear_certificado.ps1 -NombreSitio "ServicioProductos" -DnsName "localhost" -ConfiarEnEquipo
#>
[CmdletBinding()]
param(
    [string]$NombreSitio = "ServicioProductos",
    [string]$DnsName = "localhost",
    [int]$Puerto = 443,
    [switch]$ConfiarEnEquipo
)

$ErrorActionPreference = "Stop"

$esAdministrador = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
    [Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $esAdministrador) {
    throw "Ejecute este script como administrador."
}

Import-Module WebAdministration -ErrorAction Stop

$certificado = New-SelfSignedCertificate `
    -DnsName $DnsName `
    -CertStoreLocation "Cert:\LocalMachine\My" `
    -FriendlyName "DesavaPA - $DnsName" `
    -NotAfter (Get-Date).AddYears(2)

Write-Host "Certificado creado. Huella: $($certificado.Thumbprint)"

if ($ConfiarEnEquipo) {
    $rutaCer = Join-Path $env:TEMP "desava-$DnsName.cer"
    Export-Certificate -Cert $certificado -FilePath $rutaCer | Out-Null
    Import-Certificate -FilePath $rutaCer -CertStoreLocation "Cert:\LocalMachine\Root" | Out-Null
    Remove-Item $rutaCer -Force
    Write-Host "Certificado agregado al almacen de entidades de confianza del equipo."
}

$binding = Get-WebBinding -Name $NombreSitio -Protocol "https" -Port $Puerto -ErrorAction SilentlyContinue
if (-not $binding) {
    New-WebBinding -Name $NombreSitio -Protocol "https" -Port $Puerto
    $binding = Get-WebBinding -Name $NombreSitio -Protocol "https" -Port $Puerto
}

$binding.AddSslCertificate($certificado.Thumbprint, "My")

Write-Host "Binding HTTPS configurado en el sitio '$NombreSitio' (puerto $Puerto)."
Write-Host "Verifique que el sitio tenga un binding HTTP activo o elimine el existente si solo se usara HTTPS."
