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
