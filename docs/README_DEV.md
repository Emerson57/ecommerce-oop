# Desarrollo - configuración de secretos locales

Este archivo describe cómo preparar tu entorno de desarrollo local para ejecutar la aplicación sin exponer secretos en el repositorio.

1) dotnet user-secrets
- Abrir terminal en `PlataformaECommerce.Web`
- Inicializar:
  - `dotnet user-secrets init`
- Establecer secretos de ejemplo:
  - `dotnet user-secrets set "Secrets:Database:PrimaryConnectionString" "Server=.\\SQLEXPRESS;Database=PlataformaECommerceDb;Trusted_Connection=True;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=True;"`
  - `dotnet user-secrets set "Secrets:Security:JwtSigningKey" "<clave-segura-de-32+caracteres>"`

2) Variables de entorno (alternativa)
- PowerShell (sesión):
  - `$env:Secrets__Database__PrimaryConnectionString = 'Server=...;Database=...;User Id=...;Password=...;'`
- Persistente (User):
  - `[System.Environment]::SetEnvironmentVariable('Secrets__Database__PrimaryConnectionString','Server=...;Database=...;User Id=...;Password=...','User')`

3) Visual Studio Debug
- Edita `Properties\launchSettings.json` y añade las variables en `environmentVariables` para el perfil de IIS Express o tu perfil de ejecución.

4) Notas
- No comitees archivos `appsettings.*.local.json` ni `secrets.json`.
- Use `appsettings.Development.local.example.json` como guía para los valores que necesita completar.

