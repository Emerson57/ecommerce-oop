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

