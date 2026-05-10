# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY PlataformaECommerce.Application/PlataformaECommerce.Application.csproj PlataformaECommerce.Application/
COPY PlataformaECommerce.Domain/PlataformaECommerce.Domain.csproj PlataformaECommerce.Domain/
COPY PlataformaECommerce.Infrastructure/PlataformaECommerce.Infrastructure.csproj PlataformaECommerce.Infrastructure/
COPY PlataformaECommerce.Web/PlataformaECommerce.Web.csproj PlataformaECommerce.Web/
RUN dotnet restore PlataformaECommerce.Web/PlataformaECommerce.Web.csproj --nologo

COPY . .
RUN dotnet publish PlataformaECommerce.Web/PlataformaECommerce.Web.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && useradd --create-home --shell /usr/sbin/nologin appuser \
    && rm -rf /var/lib/apt/lists/*

# AllowedHosts: definir en orquestación (no usar '*'). Ejemplo:
#   AllowedHosts=midominio.com;www.midominio.com;127.0.0.1
# Incluya dominios de tenants (SaaS:Tenants:*:Hostnames) y 127.0.0.1 si el healthcheck usa loopback. Ver docs/SECURITY.md.
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0

EXPOSE 8080

COPY --from=build /app/publish .

RUN chown -R appuser:appuser /app

USER appuser

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 CMD curl --fail http://127.0.0.1:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "PlataformaECommerce.Web.dll"]
