# Hardening de configuración y secretos

## Fuentes recomendadas por entorno

### Development
Usa una de estas opciones, en este orden de preferencia:
1. variables de entorno
2. `dotnet user-secrets`
3. `PlataformaECommerce.Web/appsettings.Development.local.json` creado a partir de `appsettings.Development.local.example.json`

### Staging / Production
No uses archivos locales versionados para secretos. Configura los valores mediante variables de entorno o un proveedor externo equivalente del entorno.

## Variables de entorno para producción

### Obligatorias
- `ConnectionStrings__DefaultConnection`
- `Jwt__SigningKey`

### Obligatorias cuando MongoDB está habilitado
- `MongoDb__Enabled=true`
- `MongoDb__ConnectionString`
- `MongoDb__DatabaseName`
- `MongoDb__AuditCollectionName`

### Obligatorias cuando Wompi está habilitado
- `Payments__Wompi__Enabled=true`
- `Payments__Wompi__PublicKey`
- `Payments__Wompi__IntegritySecret`
- `Payments__Wompi__CheckoutBaseUrl`
- `Payments__Wompi__TransactionsApiBaseUrl`

### Obligatorias cuando SMTP está habilitado
- `Notifications__Smtp__Enabled=true`
- `Notifications__Smtp__Host`
- `Notifications__Smtp__Port`
- `Notifications__Smtp__EnableSsl`
- `Notifications__Smtp__UserName`
- `Notifications__Smtp__Password`
- `Notifications__Smtp__FromAddress`
- `Notifications__Smtp__FromDisplayName`

### Solo aprovisionamiento inicial controlado
Configura estas variables únicamente para un bootstrap explícito y retíralas después:
- `Bootstrap__SuperUser__Enabled`
- `Bootstrap__SuperUser__AllowInProduction`
- `Bootstrap__SuperUser__TenantId`
- `Bootstrap__SuperUser__Name`
- `Bootstrap__SuperUser__Email`
- `Bootstrap__SuperUser__Password`
- `Bootstrap__SuperUser__Area`

## Notas operativas
- La aplicación vuelve a aplicar variables de entorno y argumentos al final del arranque para que siempre tengan precedencia sobre `appsettings*.json`.
- `appsettings.Development.local.json` debe permanecer solo en la máquina del desarrollador y fuera de Git.
- La plantilla `appsettings.Development.local.example.json` conserva la estructura requerida sin exponer secretos reales.
