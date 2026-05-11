# Migraciones de base de datos (EF Core + SQL Server)

Este proyecto usa **un único** `DbContext`: `PlataformaECommerce.Infrastructure.Persistence.Context.ECommerceDbContext`. Las migraciones viven en `PlataformaECommerce.Infrastructure/Migrations/`.

## Estado actual del runtime

- La aplicación web **no** ejecuta `Database.Migrate()`, `EnsureCreated()` ni `EnsureDeleted()` al iniciar.
- En el arranque solo se comprueba **conectividad** con `CanConnectAsync` (`InfrastructureVerificationStartupTask`).
- El esquema debe estar **al día antes** de publicar tráfico, aplicando migraciones en un paso explícito (local, pipeline o DBA).
- En **Production**, si se activara `DatabaseOperations:ApplyEfMigrationsOnStartup` (no recomendado), el arranque **falla** por `EntityFrameworkProductionGuard`.
- En **Production**, ningún tenant habilitado puede tener `SaaS:Tenants:*:Provisioning:SeedDemoCatalog` en `true` (`SaaSPlatformProductionGuard`).
- La siembra de catálogo (categorías base / demo) se hace de forma **idempotente** mediante el proyecto **`PlataformaECommerce.Maintenance`** (comandos documentados con `--help`), no como efecto colateral silencioso del publicado web.

## Requisitos

- SDK .NET 10 y herramientas EF (ya referenciadas en `PlataformaECommerce.Infrastructure.csproj`).
- Cadena de conexión disponible para el proceso que ejecuta la CLI (variable `ConnectionStrings__DefaultConnection` o User Secrets en desarrollo).

## Comandos (desde la raíz de la solución)

### Listar migraciones pendientes / historial

```powershell
dotnet ef migrations list `
  --project PlataformaECommerce.Infrastructure `
  --startup-project PlataformaECommerce.Web `
  --context ECommerceDbContext `
  --configuration Release
```

### Aplicar migraciones a la base de datos (desarrollo o entorno controlado)

```powershell
dotnet ef database update `
  --project PlataformaECommerce.Infrastructure `
  --startup-project PlataformaECommerce.Web `
  --context ECommerceDbContext `
  --configuration Release
```

Para aplicar **hasta una migración concreta**:

```powershell
dotnet ef database update NombreExactoDeLaMigracion `
  --project PlataformaECommerce.Infrastructure `
  --startup-project PlataformaECommerce.Web `
  --context ECommerceDbContext `
  --configuration Release
```

### Crear una nueva migración (desarrollo)

Tras cambiar entidades o configuraciones Fluent:

```powershell
dotnet ef migrations add DescripcionCortaDelCambio `
  --project PlataformaECommerce.Infrastructure `
  --startup-project PlataformaECommerce.Web `
  --context ECommerceDbContext `
  --configuration Release
```

Revise el archivo generado en `Infrastructure/Migrations/` (operaciones destructivas, índices, datos sensibles).

### Script SQL idempotente (recomendado para Production)

Genera un script que puede ejecutarse varias veces de forma segura en ventana de mantenimiento:

```powershell
New-Item -ItemType Directory -Force -Path artifacts/sql | Out-Null
dotnet ef migrations script `
  --idempotent `
  --project PlataformaECommerce.Infrastructure `
  --startup-project PlataformaECommerce.Web `
  --context ECommerceDbContext `
  --configuration Release `
  -o artifacts/sql/migrations-idempotent.sql
```

Revise el SQL, pruébelo en **Staging** y aplíquelo en Production solo tras **backup** (ver checklist).

Los archivos `artifacts/sql/*.sql` generados localmente no deben versionarse con secretos; puede ignorarlos en Git si solo son artefactos locales.

## Por entorno

| Entorno | Práctica recomendada |
|---------|----------------------|
| **Development** | `dotnet ef database update` contra su instancia local; User Secrets para la cadena. |
| **Staging** | Mismo flujo que Production: backup de staging, script idempotente o `database update` en job controlado, smoke test. |
| **Production** | **Backup completo** → revisión del script o del plan de migración → aplicación en ventana → verificación (`dotnet test` / pruebas de humo / health). Sin migración automática al iniciar la web. |

## Datos iniciales (seed)

- **No** hay `HasData` masivo en migraciones para productos/usuarios demo de negocio.
- Categorías base y catálogo demo opcional se controlan por configuración SaaS del tenant (`SeedBaseCategories`, `SeedDemoCatalog`) y se ejecutan vía **Maintenance**; en Production `SeedDemoCatalog` debe estar **false** (validado al iniciar).
- Bootstrap de superusuario: flujo documentado en `docs/SECURITY.md` (no es migración EF).

## Checklist antes de migrar en Production

1. **Backup** de la base (nativo SQL Server, Azure SQL export, o política corporativa equivalente).
2. Revisión del **diff** de la migración (tablas eliminadas, columnas NOT NULL sin default, renombres destructivos).
3. Probar el mismo artefacto (script o migración) en **Staging** con volumen representativo si aplica.
4. Ventana de mantenimiento y plan de **rollback** (restaurar backup o migración hacia atrás solo si la migración lo permite; evitar `Down` en caliente sin prueba).
5. Confirmar `ConnectionStrings__DefaultConnection` apunta al **servidor correcto** (evitar aplicar a producción por error).
6. Tras aplicar: arranque de la app, `/health/ready`, flujo crítico (login admin, checkout de prueba en sandbox de pagos).

## Azure SQL

- Compatible con la cadena estándar de SQL Server (`Encrypt=True` según política).
- Use **siempre** secretos/Key Vault para la cadena; ver `docs/production-secrets.md`.

## Riesgos a vigilar

- Migraciones que **eliminan columnas** o tablas con datos: exigen decisión explícita del negocio.
- **Multi-tenant**: el esquema es compartido; los datos siguen filtrados por `TenantId` en runtime; las migraciones de esquema afectan a **todos** los tenants de esa base.

## Nota para desarrolladores (CLI + pruebas)

Si en la misma sesión de terminal definió `ConnectionStrings__DefaultConnection` para `dotnet ef`, recuerde limpiarla antes de `dotnet test` cuando las pruebas de integración esperan otra cadena (p. ej. `Remove-Item Env:ConnectionStrings__DefaultConnection` en PowerShell). Una cadena apuntando a un servidor inaccesible hará fallar `InfrastructureVerificationStartupTask` al levantar el host de pruebas.
