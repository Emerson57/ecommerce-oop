# Convención arquitectónica y de estilo - Fase 0

## Propósito

Este documento deja explícitas las decisiones mínimas de estabilización vigentes al cierre de `Fase 0`.
Su objetivo es eliminar ambigüedad interna, fijar una única forma oficial de implementar casos de uso y evitar nuevas inconsistencias mientras se ejecuta la consolidación arquitectónica de `Fase 1`.

## Convención de capas vigente

### 1. Dirección obligatoria de dependencias

La solución mantiene la siguiente dirección de dependencias:

`Web -> Application -> Domain`

`Infrastructure` implementa contratos definidos en `Application` y materializa detalles técnicos de persistencia, seguridad y auditoría.

### 2. Regla de acceso para la capa Web

La capa `Web` actúa como composición raíz y superficie de entrada HTTP.
Desde este punto:

- `Pages`, `PageModel` y `Controllers` solo consumen interfaces `I*ApplicationService` expuestas por `Application`.
- No se permite acceso directo desde `Web` a repositorios concretos de `Infrastructure`.
- No se permite acceso directo desde `Web` a `DbContext` ni a servicios concretos de `Infrastructure`.
- No se permite introducir lógica de negocio en `PageModel` ni en controladores.

### 3. Responsabilidad por capa

#### `Domain`
- Contiene entidades, agregados, value objects, reglas e invariantes del negocio.
- No depende de `Application`, `Infrastructure` ni `Web`.

#### `Application`
- Orquesta casos de uso.
- Define contratos, DTOs, validaciones y puertos requeridos por el negocio.
- Es la única capa autorizada para hablar con repositorios mediante interfaces.
- Expone públicamente solo servicios de aplicación; `Commands`, `Queries`, `Validators`, `Mappings` y mecanismos equivalentes se consideran soporte interno.

#### `Infrastructure`
- Implementa repositorios, acceso a base de datos, autenticación, auditoría y demás adaptadores técnicos.
- No expone comportamiento de negocio directamente a `Web`.

#### `Web`
- Resuelve binding, autenticación web, navegación, serialización HTTP y experiencia de usuario.
- No contiene reglas de dominio ni acceso directo a persistencia.

## Decisión arquitectónica oficial de Fase 0

La solución adopta la siguiente decisión obligatoria:

- Las interfaces `I*ApplicationService` y sus implementaciones `*ApplicationService` constituyen la única frontera pública de `Application`.
- La capa `Web` entra exclusivamente por interfaces `I*ApplicationService`.
- `Commands`, `Queries`, `Validators` y mecanismos similares solo pueden existir como modelos o soporte interno de `Application` cuando aportan claridad real al caso de uso.
- No se introduce una segunda arquitectura pública basada en repositorios, servicios concretos de `Infrastructure` ni jerarquías alternativas de ejecución.

## Regla operativa para artefactos internos

Tras el cierre de `Fase 0`, la solución conserva `Commands` y `Queries` solo como modelos internos simples.
Mientras `Fase 1` decide su evolución definitiva, aplica la siguiente regla:

- La capa `Web` entra solo por interfaces `I*ApplicationService` del ensamblado `Application`.
- No se crean nuevos flujos `Web -> Repository`.
- No se crean nuevos puntos de entrada públicos que compitan con `I*ApplicationService` o `*ApplicationService`.
- No se reintroducen abstracciones base para comandos, consultas o handlers sin una decisión arquitectónica posterior explícita.

## Convención de naming

- El lenguaje del negocio se mantiene en español para entidades, casos de uso, excepciones y mensajes funcionales.
- Los sufijos técnicos se mantienen consistentes con el patrón ya existente: `ApplicationService`, `Repository`, `Command` y `Query`.
- No se mezclan sin necesidad nombres equivalentes en español e inglés dentro del mismo agregado o caso de uso.
- Los métodos asíncronos deben terminar en `Async`.

## Convención de estilo

- Los cambios deben ser mínimos, explícitos y coherentes con la capa modificada.
- Las APIs públicas deben conservar documentación XML profesional.
- Las validaciones defensivas deben usar guards claros y excepciones precisas.
- Las pruebas deben nombrarse por comportamiento y mantenerse deterministas.

## Criterio de estabilización de Fase 0

Se considera que `Fase 0` queda técnicamente estable cuando:

- existe una sola respuesta a la pregunta "cómo entra un caso de uso en la solución";
- la capa `Web` consume exclusivamente interfaces `I*ApplicationService`;
- los artefactos internos y heredados quedan explícitamente clasificados y alineados con la convención vigente;
- el `build` compila en verde,
- la suite de pruebas queda en verde,
- no permanecen artefactos heredados obvios en el repositorio,
- y esta convención sirve como referencia operativa hasta la consolidación de `Fase 1`.
