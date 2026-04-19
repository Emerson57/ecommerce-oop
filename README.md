# PlataformaECommerce

PlataformaECommerce es una solución profesional de comercio electrónico construida sobre `.NET 10`, diseñada para evolucionar como producto comercializable y preparada para operar como base de una oferta `SaaS`.

La solución ya no se documenta como proyecto académico. El foco actual es entregar una plataforma mantenible, segura, observable y desplegable, con storefront web, backoffice administrativo, APIs complementarias y una arquitectura lista para crecer hacia escenarios multi-tenant más avanzados.

## Posicionamiento actual del producto

La plataforma se encuentra en una etapa de consolidación profesional con las siguientes características activas:

- storefront basado en `Razor Pages`;
- backoffice administrativo con dashboard, operaciones y auditoría;
- autenticación y autorización endurecidas para clientes, administradores y `SuperUsuario`;
- persistencia transaccional sobre `SQL Server` con `EF Core`;
- auditoría sobre `MongoDB` para trazabilidad operativa;
- configuración modular por dominio (`Security`, `Observability`, `Branding`, `Backoffice`, `SaaS`, `Payments`, `Infrastructure`);
- composition root limpia en `Program.cs` y arranque organizado por extensiones;
- preparación SaaS mediante catálogo de tenants, features, planes y aprovisionamiento controlado.

## Alcance funcional vigente

### Storefront

- home comercial con branding configurable;
- catálogo de productos y detalle de producto;
- carrito de compras;
- checkout y creación de pedidos;
- historial y detalle de pedidos;
- registro de clientes;
- login por correo electrónico;
- confirmación de correo;
- recuperación y restablecimiento de contraseña;
- cuenta del usuario autenticado.

### Backoffice

- dashboard administrativo;
- centro de operación y soporte;
- auditoría transversal;
- gestión de productos;
- gestión de categorías;
- gestión de usuarios administrativos;
- carga y exposición controlada de imágenes de producto;
- exportación de plantillas de apoyo para operación administrativa.

### APIs complementarias

- API pública de consulta de catálogo;
- API administrativa para operaciones protegidas;
- health checks para liveness y readiness;
- endpoint autenticado para obtención de token antiforgery same-origin;
- Swagger disponible en `Development`.

## Estado SaaS

La plataforma ya incorpora estructuras de configuración orientadas a SaaS:

- `tenant catalog`;
- catálogo de `features`;
- catálogo de `plans`;
- resolución de tenant por host o header;
- aprovisionamiento inicial por tenant;
- configuración comercial por tenant;
- restricciones de seguridad para bootstrap privilegiado.

El runtime actual sigue estando orientado a una experiencia web principal sobre una instancia configurada de forma controlada. Es decir, la solución está **preparada para SaaS**, pero no debe presentarse todavía como una plataforma multi-tenant totalmente desacoplada a nivel de operación y aislamiento extremo en todos los escenarios.

## Arquitectura

La solución sigue `Clean Architecture` con una separación estricta de responsabilidades:

- `PlataformaECommerce.Domain`
  - núcleo del negocio;
  - entidades, value objects, enums, reglas y excepciones de dominio.

- `PlataformaECommerce.Application`
  - casos de uso;
  - contratos;
  - DTOs;
  - validaciones;
  - orquestación funcional.

- `PlataformaECommerce.Infrastructure`
  - `EF Core` y `SQL Server`;
  - persistencia de `Data Protection Keys`;
  - autenticación `JWT` para escenarios API explícitos;
  - auditoría `MongoDB`;
  - correo `SMTP`;
  - pagos `Wompi`;
  - repositorios y adaptadores técnicos.

- `PlataformaECommerce.Web`
  - composition root;
  - `Razor Pages` como experiencia principal;
  - controladores HTTP complementarios;
  - middlewares;
  - seguridad web;
  - endpoints y pipeline.

- `PlataformaECommerce.Tests`
  - pruebas automatizadas por capa y comportamiento con `NUnit`.

### Dirección oficial de dependencias

`Web -> Application -> Domain`

`Infrastructure` implementa contratos y capacidades técnicas requeridas por `Application`, sin convertirse en una capa funcional consumida directamente como atajo desde la UI.

### Documento arquitectónico oficial

La línea base arquitectónica y sus decisiones oficiales se documentan en `ARCHITECTURE.md`.

## Principales capacidades técnicas

### Seguridad

- cookies endurecidas para el sitio web interactivo;
- `JWT` registrado para uso explícito en APIs que lo requieran;
- antiforgery consistente para formularios y endpoints protegidos por cookies;
- `Content-Security-Policy` endurecida;
- `security headers` centralizados;
- rate limiting para autenticación, APIs públicas, administración y endpoints sensibles;
- autorización diferenciada para cliente, administrador y `SuperUsuario`;
- control estricto del bootstrap del usuario raíz;
- uploads servidos de forma encapsulada y con MIME restringidos.

### Observabilidad y operación

- logging estructurado con `Serilog`;
- correlación de solicitudes HTTP;
- health checks en:
  - `/health/live`
  - `/health/ready`
- manejo global de excepciones con middleware propio y `ProblemDetails`;
- trazabilidad transversal de eventos vía `MongoDB`;
- soporte para forwarded headers con validación de confianza.

### Configuración profesional

- configuración modular cargada por dominio funcional;
- validación de opciones críticas al arranque;
- soporte para `User Secrets`, variables de entorno y archivos locales no versionados;
- separación entre configuración base, configuración por ambiente y configuración sensible.

## Estructura de la solución

```text
PlataformaECommerce.Domain/
PlataformaECommerce.Application/
PlataformaECommerce.Infrastructure/
PlataformaECommerce.Web/
PlataformaECommerce.Tests/
ARCHITECTURE.md
README.md
```

## Stack tecnológico

- `.NET 10`
- `ASP.NET Core Razor Pages`
- controladores HTTP complementarios
- `Entity Framework Core 10`
- `SQL Server`
- `MongoDB`
- `Serilog`
- `Swagger / OpenAPI`
- `NUnit`

## Desarrollo local

### Prerrequisitos

- `.NET SDK 10`;
- `SQL Server` accesible desde el entorno local;
- `MongoDB` si se desea auditoría local completa;
- Visual Studio 2026 o `dotnet CLI`.

### Configuración local recomendada

La solución **no** debe almacenar secretos en archivos versionados.

Configura los valores sensibles mediante `User Secrets`, variables de entorno o archivos locales no versionados. Como mínimo, un entorno funcional necesita resolver:

- `ConnectionStrings:DefaultConnection`
- `Jwt:SigningKey`

Dependiendo del escenario, también pueden requerirse:

- `MongoDb:ConnectionString`
- `Notifications:Smtp:Host`
- `Notifications:Smtp:UserName`
- `Notifications:Smtp:Password`
- `Notifications:Smtp:FromAddress`
- `Payments:Wompi:PublicKey`
- `Payments:Wompi:IntegritySecret`

### Restaurar dependencias

```powershell
dotnet restore
```

### Aplicar migraciones

```powershell
dotnet ef database update --project PlataformaECommerce.Infrastructure --startup-project PlataformaECommerce.Web
```

### Ejecutar la aplicación

```powershell
dotnet run --project PlataformaECommerce.Web
```

### Ejecutar pruebas

```powershell
dotnet test
```

## Configuración por dominios

La aplicación carga configuración modular desde archivos especializados cuando existen. Entre ellos:

- `appsettings.Security.json`
- `appsettings.Observability.json`
- `appsettings.Branding.json`
- `appsettings.Backoffice.json`
- `appsettings.SaaS.json`
- `appsettings.Payments.json`
- `appsettings.Infrastructure.json`

Y sus variantes por ambiente, por ejemplo:

- `appsettings.Security.Development.json`
- `appsettings.Branding.Development.json`
- `appsettings.SaaS.Development.json`

Las secciones funcionales activas incluyen:

- `ClientExperience`
- `Backoffice`
- `SecurityHeaders`
- `Antiforgery`
- `ForwardedHeadersSecurity`
- `Observability`
- `RateLimiting`
- `SaaS`
- `Payments:Wompi`
- `Notifications:Smtp`
- `Jwt`
- `DataProtection`
- `MongoDb`

## Endpoints técnicos relevantes

### Salud y operación

- `GET /health/live`
- `GET /health/ready`

### Seguridad web

- `GET /security/antiforgery/token`

### APIs

- `GET /api/products`
- `GET /api/products/{id}`
- `POST /api/admin/products/...`

### Documentación interactiva

En `Development`, la aplicación expone Swagger UI para las APIs pública y administrativa.

## Calidad y pruebas

La solución cuenta con pruebas automatizadas sobre:

- dominio;
- servicios de aplicación;
- repositorios e infraestructura;
- seguridad web;
- uploads y serving de archivos;
- integración HTTP;
- startup y validación de opciones.

Las pruebas se implementan en `PlataformaECommerce.Tests` usando `NUnit` y `Microsoft.AspNetCore.Mvc.Testing` para escenarios web e integración.

## Principios de producto aplicados

Este repositorio se trabaja con mentalidad de producto real:

- seguridad antes que rapidez;
- configuración lista para ambientes reales;
- observabilidad operativa;
- mantenibilidad y separación estricta de responsabilidades;
- documentación alineada con el estado vigente de la solución;
- preparación para comercialización como base SaaS.

## Qué no representa este repositorio

Este repositorio **no** debe seguir interpretándose como una colección de entregas académicas independientes.

La solución actual representa una base profesional de e-commerce con orientación comercial y SaaS, y su documentación debe leerse bajo ese contexto.

## Próximas líneas de evolución razonables

Algunas direcciones naturales para la evolución del producto son:

- aislamiento SaaS más estricto por tenant;
- automatización de despliegue y runbooks operativos;
- observabilidad ampliada con métricas y trazas distribuidas;
- flujos comerciales adicionales de promociones, pagos y fulfillment;
- capacidades de administración multi-tenant de nivel plataforma.

## Resumen ejecutivo

PlataformaECommerce es hoy una base profesional para un producto de comercio electrónico vendible, con:

- storefront y backoffice reales;
- arquitectura limpia;
- seguridad web endurecida;
- observabilidad y trazabilidad;
- configuración modular;
- preparación SaaS;
- pruebas automatizadas;
- composition root y startup organizados de forma mantenible.

## CI Secret Scan

This repository includes a minimal secret-scanner to prevent accidental inclusion of secrets in CI/publish artifacts.

Files added:
- `scripts/secret-scan.ps1` - PowerShell scanner for Windows CI.
- `scripts/secret-scan.sh` - Bash scanner for Linux CI.
- `.github/workflows/secret-scan.yml` - example GitHub Actions workflow that runs the scanner.

How it works:
- Scans repository files for heuristic patterns: JWT signing keys, connection strings, "Password=" entries, user ids, server/database strings and files ending with `.local.json`.
- Fails the CI job if likely secrets are detected.

Usage locally:
- Windows: `pwsh -NoProfile -NonInteractive -ExecutionPolicy Bypass .\\scripts\\secret-scan.ps1 -Path .`
- Linux/macOS: `./scripts/secret-scan.sh .`

Limitations:
- Heuristic-based: may produce false positives or false negatives. Review findings manually.
- Not a replacement for secret scanning tools like TruffleHog, GitLeaks or commercial scanners.
- Does not remove secrets from git history.
- Should be combined with repository policies (branch protection) and secrets management best practices.

Add the scanner as a step in your own pipelines (Azure Pipelines, GitLab CI, etc) by invoking the relevant script before build/publish steps.
