# Codebase Index

This file provides a fast map of the repository, its main projects, and the most important entry points.

## Solution Overview

- Solution file: `PlataformaECommerce.slnx`
- Architecture baseline: `ARCHITECTURE.md`
- Product/operations guide: `README.md`
- Total tracked files in workspace (excluding `.git`, `bin`, `obj`, `.vs`): **808**

## Top-Level Structure (File Count)

- `PlataformaECommerce.Web` (321)
- `PlataformaECommerce.Application` (217)
- `PlataformaECommerce.Tests` (96)
- `PlataformaECommerce.Infrastructure` (80)
- `PlataformaECommerce.Domain` (50)
- `docs` (10)
- `PlataformaECommerce.Maintenance` (10)
- `scripts` (5)
- `.github` (4)
- `.githooks` (2)
- Root docs/config files and startup logs

## Project Index

### `PlataformaECommerce.Web`

**Role:** Composition root + HTTP presentation (`Razor Pages`, controllers, middleware, startup wiring).

Key areas:

- `Extensions` (startup/pipeline wiring, security, observability)
- `Pages` (storefront and admin UI)
- `wwwroot` (static assets, frontend scripts, uploads)
- `Controllers` (complementary API/controller endpoints)
- `Middlewares` (global request/exception/security behavior)
- `Initialization` (startup tasks and runtime validations)
- `Configuration` + `Security` (validated options and hardening)
- `Services` + `ViewComponents` (UI support services/components)

Primary entry points:

- `PlataformaECommerce.Web/Program.cs`
- `PlataformaECommerce.Web/PlataformaECommerce.Web.csproj`
- `PlataformaECommerce.Web/appsettings.json`
- `PlataformaECommerce.Web/appsettings.Development.json`

### `PlataformaECommerce.Application`

**Role:** Use cases, DTOs, validators, interfaces, and application orchestration.

Key areas:

- `Features` (vertical slices by domain capability)
- `Interfaces` (ports/contracts for infrastructure and services)
- `Common` (shared application-level utilities and base components)
- `DependencyInjection` (registration and composition helpers)

Primary entry points:

- `PlataformaECommerce.Application/PlataformaECommerce.Application.csproj`
- `PlataformaECommerce.Application/DependencyInjection/ApplicationServiceCollectionExtensions.cs`

### `PlataformaECommerce.Domain`

**Role:** Core business model and invariant enforcement.

Key areas:

- `Entities`
- `ValueObjects`
- `Rules`
- `Events`
- `Exceptions`
- `Enums`
- `Common`

Primary entry point:

- `PlataformaECommerce.Domain/PlataformaECommerce.Domain.csproj`

### `PlataformaECommerce.Infrastructure`

**Role:** External integrations and technical adapters (SQL Server/EF Core, MongoDB audit, identity, email, payments).

Key areas:

- `Persistence` (DbContext, EF configurations, persistence concerns)
- `Migrations` (EF Core schema history)
- `Repositories` (infrastructure implementations of data contracts)
- `Services` (SMTP, payment gateway, user/context providers)
- `Mongo` + `Audit` (audit pipeline and Mongo integration)
- `Configurations` (infrastructure-bound options)
- `DependencyInjection` (wiring infrastructure services)

Primary entry points:

- `PlataformaECommerce.Infrastructure/PlataformaECommerce.Infrastructure.csproj`
- `PlataformaECommerce.Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs`

### `PlataformaECommerce.Tests`

**Role:** Automated test suite (`NUnit`) across all layers.

Key areas:

- `Domain`
- `Application`
- `Infrastructure`
- `Web`

Primary entry point:

- `PlataformaECommerce.Tests/PlataformaECommerce.Tests.csproj`

### `PlataformaECommerce.Maintenance`

**Role:** Maintenance/bootstrap operational commands and supporting automation.

Primary entry point:

- `PlataformaECommerce.Maintenance/PlataformaECommerce.Maintenance.csproj`

## Operational and Documentation Index

- `docs/`:
  - architecture/operations support material
  - diagrams/images under `docs/images`
- `scripts/`:
  - bootstrap and local setup scripts
  - secret scan scripts for CI/local checks
- `.github/workflows/`:
  - CI workflows and pipeline automation
- `.githooks/`:
  - local git hooks

## Configuration and Environment

Primary runtime configuration lives under `PlataformaECommerce.Web`:

- `appsettings*.json`
- security/observability/branding/backoffice/SaaS/payments/infrastructure segmented config files
- launch profile in `Properties/launchSettings.json`

Repository-level config:

- `.pre-commit-config.yaml`
- `compose.yml`
- `Dockerfile`
- `Directory.Build.props`
- `.dockerignore`, `.gitignore`, `.env.example`

## Dependency Direction (Architectural)

As documented in `README.md` and `ARCHITECTURE.md`:

- `Web -> Application -> Domain`
- `Infrastructure` implements contracts required by `Application`

## Quick Navigation

- Start here for architecture: `ARCHITECTURE.md`
- Start here for local run/setup: `README.md`
- Runtime startup root: `PlataformaECommerce.Web/Program.cs`
- Domain model: `PlataformaECommerce.Domain/`
- Use-cases and contracts: `PlataformaECommerce.Application/`
- Integrations/persistence: `PlataformaECommerce.Infrastructure/`
- Test coverage: `PlataformaECommerce.Tests/`
