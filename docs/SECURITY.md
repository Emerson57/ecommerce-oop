# SECURITY - manejo de secrets y claves

Este documento describe prácticas recomendadas para manejar secretos en desarrollo y producción.

1) Desarrollo local (recomendado)
- Use `dotnet user-secrets` para almacenar secretos locales por proyecto. No commitear nunca `appsettings.*.local.json` ni `secrets.json`.
- Ejemplo rápido:
  - cd `PlataformaECommerce.Web`
  - `dotnet user-secrets init`
  - `dotnet user-secrets set "Secrets:Database:PrimaryConnectionString" "Server=...;Database=...;User Id=...;Password=...;"`
  - `dotnet user-secrets set "Secrets:Security:JwtSigningKey" "<clave-segura-de-32+caracteres>"`
  - Opcional (host filtering local): `dotnet user-secrets set "Secrets:Hosting:AllowedHosts" "localhost;127.0.0.1;miapp.local"`

2) Variables de entorno
- En CI/CD y entornos de depuración también es aceptable usar variables de entorno.
- Para keys anidadas use `__` (doble underscore) como separador. Ejemplo:
  - `Secrets__Database__PrimaryConnectionString`
  - `Secrets__Hosting__AllowedHosts` (alias hacia `AllowedHosts`)
  - Bootstrap (alias opcionales hacia `Bootstrap:SuperUser`): `AdminBootstrap__Enabled`, `AdminBootstrap__Email`, `AdminBootstrap__Password`

3) Producción
- Use un secret manager del proveedor (Azure Key Vault, AWS Secrets Manager, GCP Secret Manager).
- Preferir Managed Identity / IAM para que las aplicaciones no lleven credenciales de acceso a KeyVault en código.
- Inyecte secretos como variables de entorno o configure el proveedor de Key Vault en `Program.cs`/configuración del host.

4) Docker
- Nunca `COPY` archivos locales con secretos en la imagen final.
- Use docker secrets o montajes en runtime si necesita proveer secretos a contenedores.

5) Prevención automática (CI/Local)
- Añada checks en pipelines que fallen si existen `*.local.json`, `secrets.json` o `*.secrets.json` en el commit o artefacto.
- Proveer un hook de pre-commit de ejemplo en `.githooks/pre-commit.sample` para uso local.

6) Recomendaciones operativas
- Rotar claves periódicamente.
- Usar longitudes seguras para claves simétricas (>=32 chars). Preferir claves generadas por RNGCrypto.
- Registrar accesos a secrets (auditoría) en el secret manager.

7) Recursos
- Azure Key Vault docs: https://learn.microsoft.com/azure/key-vault/
- dotnet user-secrets: https://learn.microsoft.com/aspnet/core/security/app-secrets

8) AllowedHosts (ASP.NET Core host filtering)
- La clave raíz `AllowedHosts` limita los valores de la cabecera `Host` que la aplicación acepta. **No use `*` en Production**: equivale a desactivar el filtro y aumenta el riesgo de Host Header Injection frente a cookies, redirecciones y URLs absolutas.
- **Variable de entorno** (recomendado en contenedores / App Service / Kubernetes), lista separada por `;`:
  - `AllowedHosts=midominio.com;www.midominio.com;127.0.0.1`
- **Alias opcional** (User Secrets / secret manager): `Secrets:Hosting:AllowedHosts` se proyecta a `AllowedHosts` en el arranque.
- **Multi-tenant (host)**: si `SaaS:ResolveTenantFromHost` es `true`, cada hostname público del storefront o backoffice debe aparecer en `AllowedHosts` (configuración operativa). Los hostnames en `SaaS:Tenants:*:Hostnames` no se fusionan automáticamente en `AllowedHosts`; en despliegues reales debe mantenerse la lista alineada con los dominios que enrutan hacia esta instancia (y con registros DNS/TLS).
- **Staging / integración**: el archivo incluye `localhost` para que `WebApplicationFactory` y pruebas con `TestServer` sigan respondiendo con `Host: localhost`. En un staging público, sustituya o amplíe la lista con el hostname real del entorno.
- **Production**: si `AllowedHosts` está vacío o contiene `*`, la aplicación **falla al iniciar** con un mensaje explícito (guardia en `ConfigureWebApplicationHost`).

9) Creación de administradores y bootstrap del super usuario
- **UI de alta (`/Admin/Users/Create`)**: solo está disponible si `Backoffice:Users:EnableAdministratorCreationUi` es `true` y el usuario tiene política `SuperUserOnly`. En **Production** y **Staging** la aplicación **no arranca** si esa opción está en `true` (evita despliegues con flujo interactivo de privilegios en entornos reales).
- **Desarrollo**: `appsettings.Backoffice.Development.json` mantiene `EnableAdministratorCreationUi` en `true` para pruebas locales.
- **Primer super usuario (bootstrap)**:
  - Preferido: ejecutar el proyecto de mantenimiento una vez con el tenant correcto: `dotnet run --project PlataformaECommerce.Maintenance -- bootstrap-superuser` (ver ayuda del ejecutable para flags). El servicio `SuperUserBootstrapService` es **idempotente**: si ya existe un `SuperUsuario`, no crea duplicados.
  - En **Production**, el arranque web con `Bootstrap:SuperUser:Enabled=true` exige además `Bootstrap:SuperUser:AllowInProduction=true` solo durante el primer despliegue; después debe deshabilitarse `Enabled` (el servicio lanza si queda habilitado cuando ya hay super usuario).
- **Variables de entorno (alias hacia `Bootstrap:SuperUser`)** — útiles en pipelines sin escribir en disco:
  - `AdminBootstrap__Enabled` → `Bootstrap:SuperUser:Enabled`
  - `AdminBootstrap__Email` → `Bootstrap:SuperUser:Email`
  - `AdminBootstrap__Password` → `Bootstrap:SuperUser:Password`
  - Sigue existiendo el alias `Secrets:Bootstrap:SuperUserPassword` → `Bootstrap:SuperUser:Password` (User Secrets).
- **Contraseña de bootstrap**: debe cumplir la misma composición mínima que el alta administrativa (longitud, mayúscula, minúscula, dígito y carácter especial). No registrar contraseñas en logs.
