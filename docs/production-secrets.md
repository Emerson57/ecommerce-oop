# Secretos en producción

Esta guía resume cómo suministrar secretos **sin** versionarlos en Git. El detalle de alias `Secrets:*` y variables está en [operations/configuration-secrets.md](operations/configuration-secrets.md) y las prácticas generales en [SECURITY.md](SECURITY.md).

## Principios

1. **Nunca** commitear `appsettings.*.local.json`, `.env` con valores reales, ni claves en código.
2. En **Production**, use variables de entorno, Key Vault, o el mecanismo equivalente del proveedor de nube.
3. El arranque en Production ejecuta `ProductionSecretsConfigurationGuard`: exige cadena de conexión, `Jwt:SigningKey` (≥32 caracteres), y si Wompi o SMTP están habilitados, los campos obligatorios de esas secciones. Con Wompi habilitado **no** se acepta llave pública con prefijo `pub_test_`.
4. Las variables de entorno y la línea de comandos se aplican **después** de los alias `Secrets:*`, de modo que un valor explícito en `Jwt__SigningKey` prevalece sobre un alias si ambos existen.

## Variables mínimas (Production)

| Variable (entorno) | Rol |
|--------------------|-----|
| `ConnectionStrings__DefaultConnection` | SQL Server (o alias `Secrets__Database__PrimaryConnectionString`) |
| `Jwt__SigningKey` | Firma JWT (≥32 caracteres; o `Secrets__Security__JwtSigningKey`) |
| `AllowedHosts` | Host header permitido (sin `*` en producción) |

Si **Wompi** está habilitado (`Payments__Wompi__Enabled=true`):

- `Payments__Wompi__PublicKey`, `Payments__Wompi__IntegritySecret`, URLs HTTPS de checkout y API.

Si **SMTP** está habilitado (`Notifications__Smtp__Enabled=true`):

- `Notifications__Smtp__Host`, `Notifications__Smtp__Password`, `Notifications__Smtp__FromAddress`, etc.

## Windows (PowerShell, sesión actual)

```powershell
$env:ConnectionStrings__DefaultConnection = '<cadena-desde-secret-manager>'
$env:Jwt__SigningKey = '<clave-de-al-menos-32-caracteres>'
dotnet run --project PlataformaECommerce.Web
```

Para persistir en el perfil de usuario (solo su máquina), use `setx` con cuidado (queda en texto plano en el registro) o preferiblemente **User Secrets** en desarrollo.

## Visual Studio

1. Clic derecho en `PlataformaECommerce.Web` → **Manage User Secrets** (solo Development recomendado).
2. En depuración de un perfil que use **Production**, configure variables en **Project → Properties → Debug → General → Environment variables** (no commitear el perfil con secretos; use perfil local no versionado si aplica).

## Azure App Service

**Configuration → Application settings** (marcar como **Deployment slot setting** si aplica):

- `ConnectionStrings__DefaultConnection`
- `Jwt__SigningKey`
- Resto según [configuration-secrets.md](operations/configuration-secrets.md)

Opcional: referencias a **Key Vault** con identidad administrada en lugar de pegar secretos en la UI.

## Docker / Compose

No incruste secretos en el `Dockerfile`. En `compose.yml` ya se usan sustituciones `${MSSQL_SA_PASSWORD}`, etc. Defina un archivo `.env` **local** (ignorado por Git) o variables en el orquestador:

```bash
export MSSQL_SA_PASSWORD='<secreto>'
export JWT_SIGNING_KEY='<clave-32+>'
docker compose up --build
```

Plantilla sin valores reales: [.env.example](../.env.example) en la raíz del repositorio.

## Plantilla appsettings (sin secretos)

Puede copiarse [appsettings.Production.template.json](../PlataformaECommerce.Web/appsettings.Production.template.json) como referencia de claves; los valores sensibles deben quedar vacíos en el repositorio y rellenarse solo en el entorno de despliegue.

## Rotación e historial Git

Si alguna vez se subió un secreto real al repositorio, además de eliminarlo del árbol debe **rotar** ese secreto en el proveedor (SQL, Wompi, SMTP, JWT, etc.) porque puede permanecer en el historial. Esta guía no sustituye un barrido con herramientas tipo `gitleaks` o `git-secrets` en CI.
