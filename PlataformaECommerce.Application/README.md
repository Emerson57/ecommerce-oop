# Application - Convención vigente de Fase 0

## Propósito

Este documento deja explícita la convención operativa vigente dentro del proyecto `PlataformaECommerce.Application` mientras se ejecuta la consolidación arquitectónica posterior.

## Frontera pública vigente

La única frontera pública autorizada de `Application` está compuesta por:

- interfaces `I*ApplicationService`
- implementaciones `*ApplicationService`
- DTOs y resultados consumidos por `Web`

La capa `Web` debe entrar a los casos de uso solo mediante esas interfaces y sus implementaciones asociadas.

## Artefactos internos

Los siguientes elementos se consideran soporte interno de `Application`:

- `Commands`
- `Queries`
- `Validators`
- `Mappings`
- `Common\Execution`

Estos artefactos pueden permanecer cuando aportan claridad semántica o simplifican validación, orquestación y proyección interna del caso de uso.
No constituyen una frontera pública alternativa.

## Artefactos retirados en Fase 0

Las abstracciones heredadas de la jerarquía CQRS previa:

- `ICommand`
- `IQuery`
- `ICommandHandler`
- `IQueryHandler`

fueron retiradas en `Fase 0` para evitar una segunda arquitectura pública dentro de `Application`; cualquier archivo residual asociado a esa jerarquía también se considera eliminado del diseño vigente.

### Regla operativa

- `Commands` y `Queries` permanecen como modelos internos simples.
- No deben ser consumidos directamente desde `Web` fuera de las firmas expuestas por `I*ApplicationService`.
- No debe reintroducirse una jerarquía base para comandos o consultas sin una decisión arquitectónica posterior explícita.
- `I*ApplicationService` y `*ApplicationService` siguen siendo el único mecanismo oficial de entrada a casos de uso.

## Regla de evolución

Cualquier nuevo caso de uso debe diseñarse alrededor de `I*ApplicationService`, `*ApplicationService`, contratos explícitos de `Application` y modelos internos simples para `Commands` y `Queries` cuando aporten claridad semántica.

## Referencia

La convención arquitectónica general de la solución se documenta en `docs/arquitectura-convenciones-fase-0.md`.

# PlataformaECommerce.Application

## Orden recomendado de ejecución del módulo administrativo de usuarios

Esta guía consolida el orden técnico que debe respetarse para extender o evolucionar el módulo `Admin/Users` sin romper seguridad, consistencia entre capas ni trazabilidad operativa.

### Secuencia obligatoria

1. **Asegurar prerrequisitos arquitectónicos**
   - Mantener separación clara entre `Domain`, `Application`, `Infrastructure` y `Web`.
   - Confirmar que la UI solo orquesta interacción y no incorpora lógica de negocio ni seguridad crítica.

2. **Definir `SuperUsuario` en dominio y autorización**
   - `RolUsuario` debe incluir `SuperUsuario` antes de exponer capacidades privilegiadas.
   - Las extensiones de rol, claims y políticas deben soportar su semántica administrativa efectiva.

3. **Ajustar persistencia y repositorios**
   - La tabla única `Users` debe soportar `Administrador` y `SuperUsuario` antes de usar el rol en login, bootstrap o UI.
   - `IUserRepository` debe rehidratar correctamente ambos roles y mantener unicidad por correo.

4. **Bootstrapear el `SuperUsuario` inicial**
   - El seed controlado debe existir antes de exigir acceso privilegiado interactivo.
   - El bootstrap solo puede ejecutarse si aún no existe un usuario con rol `SuperUsuario`.

5. **Endurecer `RegisterAdminAsync`**
   - La validación estructural, autorización, unicidad, persistencia, auditoría y manejo de errores deben quedar resueltos en `Application` antes de exponer el alta en `Web`.

6. **Consolidar claims y políticas**
   - `PrimaryRole`, `AdminArea` e `IsSuperUser` deben quedar consistentes entre autenticación, cookies, JWT y políticas.
   - No se debe confiar únicamente en claims sin revalidación en persistencia para sesiones administrativas.

7. **Construir el módulo `Admin/Users`**
   - Las rutas `/Admin/Users` y `/Admin/Users/Create` solo deben exponerse después de existir `SuperUserOnly` y el endurecimiento del caso de uso.
   - La UI debe depender de `Application`, nunca de repositorios o persistencia directa.

8. **Conectar formulario y flujos al backend**
   - El formulario debe enviar `RequestedByUserId`, `IpAddress` y `Source` para trazabilidad.
   - El flujo debe usar Post-Redirect-Get y prevención de doble submit antes de considerarse cerrado.

9. **Validar persistencia y auditoría**
   - El alta debe confirmarse por `IUserRepository` + `IUnitOfWork` y registrar auditoría sin contraseñas, hashes ni datos sensibles.

10. **Completar pruebas automatizadas**
    - Deben cubrirse escenarios felices, negativos, de seguridad, bootstrap, autenticación, persistencia, claims, políticas y Razor Pages.

11. **Cerrar con revisión final de seguridad y UX**
    - El hardening final ocurre al final del ciclo: manejo de errores, revocación de sesiones inconsistentes, exposición mínima y experiencia operativa del backoffice.

### Dependencias críticas que no deben romperse

- No construir la UI antes de definir `SuperUsuario` y `SuperUserOnly`.
- No exponer el formulario antes de endurecer `RegisterAdminAsync`.
- No dar por cerrado el módulo sin validar bootstrap, login administrativo y revalidación de sesión.
- No asumir seguridad por cookie o claims sin validación adicional en `Application` y persistencia.
- No cerrar el flujo sin verificar persistencia, auditoría y pruebas automatizadas end-to-end.

## Checklist previo a futuras evoluciones

- Confirmar que la estrategia final de rol sigue siendo `SuperUsuario` como privilegio explícito y `Administrador` como rol operativo.
- Confirmar que el bootstrap inicial permanece deshabilitado por defecto y solo se habilita de forma controlada.
- Confirmar que las rutas oficiales del módulo siguen siendo `/Admin/Users` y `/Admin/Users/Create`.
- Confirmar que la política mínima de contraseña permanece centralizada en `AdminRegistrationPolicies` y validada en `Application`.
- Confirmar consistencia de nombres, namespaces, claims y contratos (`RolUsuario`, `SecurityClaimTypes`, DTOs y queries).
- Confirmar que cualquier nueva capacidad administrativa se integra primero en `Domain` y `Application`, luego en `Infrastructure` y finalmente en `Web`.
