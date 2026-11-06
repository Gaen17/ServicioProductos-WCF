# Índice de entrega - PA2 y PA3

**Estudiante:** Ernesto Antonio Macario Veramendi / Matias Aaron Flores Beraun  
**Asignatura:** Desarrollo de Aplicaciones Avanzadas  
**Docente:** JENNIFER LESLY HUALLANCA PINTO  
**Fecha:** 5/11/2026  
**Repositorio:** ServicioProductos | Rama: `pa2-pa3`

---

## Documentos de esta carpeta

| Archivo | Contenido |
| --- | --- |
| `EVIDENCIAS.md` | Evidencias de verificación de HTTPS y autenticación básica en IIS |

## Documentos en `docs/`

| Archivo | Contenido |
| --- | --- |
| `informe_borrador.md` | Informe completo de PA2 y PA3 |
| `PRUEBAS.md` | Plan de pruebas manuales funcionales |
| `EXPLICACION.md` | Explicación técnica del código |
| `CHECKLIST_MANUAL.md` | Pasos de IIS y checklist de verificación |

## Estado final

| Hito | Estado |
| --- | --- |
| H1: Procedimientos almacenados | Completo |
| H2: CRUD de productos (PA2) | Completo |
| H3: Validaciones y DTO | Completo |
| H4: Devolución de venta (PA3) | Completo |
| H5: HTTPS y autenticación básica | Completo y **verificado** |
| H6: Documentos finales | Completo |

## Cómo reproducir la verificación

1. Instalar IIS con `Basic Authentication` y `HTTP Activation`.
2. Publicar el proyecto en `C:\inetpub\wwwroot\ServicioProductosPA3`.
3. Ejecutar `scripts\crear_certificado.ps1` como administrador.
4. Ejecutar `scripts\crear_usuario_demo.ps1` como administrador.
5. Ajustar la cadena de conexión en `Web.config` si no se usa LocalDB compartida.
6. Ejecutar las verificaciones de `docs\CHECKLIST_MANUAL.md`.
