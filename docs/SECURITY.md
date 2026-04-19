# SECURITY - manejo de secrets y claves

Este documento describe prácticas recomendadas para manejar secretos en desarrollo y producción.

1) Desarrollo local (recomendado)
- Use `dotnet user-secrets` para almacenar secretos locales por proyecto. No commitear nunca `appsettings.*.local.json` ni `secrets.json`.
- Ejemplo rápido:
  - cd `PlataformaECommerce.Web`
  - `dotnet user-secrets init`
  - `dotnet user-secrets set "Secrets:Database:PrimaryConnectionString" "Server=...;Database=...;User Id=...;Password=...;"`
  - `dotnet user-secrets set "Secrets:Security:JwtSigningKey" "<clave-segura-de-32+caracteres>"`

2) Variables de entorno
- En CI/CD y entornos de depuración también es aceptable usar variables de entorno.
- Para keys anidadas use `__` (doble underscore) como separador. Ejemplo:
  - `Secrets__Database__PrimaryConnectionString`
  - `Secrets__Security__JwtSigningKey`

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

