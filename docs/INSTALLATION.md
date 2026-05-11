# Instalación de la plataforma

## Objetivo
Dejar una instancia monocliente operativa, segura y reproducible de `PlataformaECommerce`.

## Requisitos
- SDK `.NET 10`
- SQL Server accesible desde la aplicación
- MongoDB solo si se habilita auditoría documental (`MongoDb:Enabled=true`)
- Visual Studio 2026 o `dotnet CLI`

## Configuración mínima
No almacenar secretos en `appsettings.json`. Configura secretos y cadenas sensibles por variables de entorno, `User Secrets` o tu sistema de despliegue.

### Secciones relevantes
- `ConnectionStrings`
- `Jwt`
- `Payments:Wompi`
- `Notifications:Smtp`
- `MongoDb`
- `ClientExperience`
- `Observability`

## Base de datos y migraciones EF Core

Después de configurar la cadena de conexión, aplique el esquema con las migraciones del proyecto `PlataformaECommerce.Infrastructure`. El procedimiento detallado (comandos `dotnet ef`, script idempotente, backup y checklist de Production) está en [database-migrations.md](database-migrations.md).

## Branding por cliente
La instancia actual trabaja en modo monocliente configurable. Ajusta la sección `ClientExperience` para personalizar:
- `ClientId`
- `StorefrontName`
- `BackofficeName`
- `StorefrontTagline`
- `HomeHeroBadge`
- `HomeHeroTitle`
- `HomePromoTitle`
- `LegalCompanyName`
- `SupportEmail`
- `SupportPhone`
- `SupportHours`
- `SupportSla`
- `PrimaryColor`
- `AccentColor`
- `AdminSidebarStartColor`
- `AdminSidebarEndColor`
- `LogoGlyph`

## Pasos de instalación
1. Restaurar paquetes.
2. Configurar secretos por ambiente.
3. Aplicar migraciones manualmente (ver [database-migrations.md](database-migrations.md); incluya `--context ECommerceDbContext`).
4. Arrancar la aplicación.
5. Validar health checks.
6. Validar acceso al backoffice.

## Comandos CLI
```powershell
dotnet restore
dotnet build
dotnet ef database update --project PlataformaECommerce.Infrastructure --startup-project PlataformaECommerce.Web --context ECommerceDbContext
dotnet run --project PlataformaECommerce.Web
```

Para lista de migraciones, scripts idempotentes y checklist de Production, use el documento [database-migrations.md](database-migrations.md).

## Package Manager Console
```powershell
Update-Database -Project PlataformaECommerce.Infrastructure -StartupProject PlataformaECommerce.Web -Context ECommerceDbContext
```

## Verificaciones posteriores
- `https://<host>/health/live`
- `https://<host>/health/ready`
- ingreso al storefront con branding configurado
- acceso al dashboard admin
- acceso a `Admin > Operación y soporte`
- acceso a `Admin > Auditoría transversal`

## Criterios de salida
La instalación se considera lista cuando:
- la base está migrada
- el storefront refleja el branding configurado
- el backoffice muestra versión, `ClientId` y correlación
- la auditoría es consultable
- los health checks responden correctamente
