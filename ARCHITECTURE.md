# Arquitectura oficial de la solución

## Estado
Aprobada.

## Propósito
Congelar formalmente la arquitectura real de la solución en esta etapa del proyecto para evitar desviaciones estructurales y preservar una implementación profesional alineada con Clean Architecture.

## Decisión arquitectónica
La capa de presentación oficial de la solución es `PlataformaECommerce.Web`.

Esta decisión se basa en la estructura real del repositorio y en la composición actualmente implementada:
- hospeda Razor Pages como interfaz principal de usuario;
- contiene la composición raíz en `Program.cs`;
- centraliza autenticación, autorización, middleware y bootstrap de arranque;
- expone controladores HTTP complementarios sin convertir la solución en una API-first.

En consecuencia, durante esta etapa no se creará un proyecto separado `WebApi`.

## Estructura oficial de capas
La estructura oficial vigente de la solución queda definida así:

1. `PlataformaECommerce.Domain`
   - Núcleo del negocio.
   - Entidades, value objects, enums, reglas y excepciones de dominio.

2. `PlataformaECommerce.Application`
   - Casos de uso, contratos, validaciones de aplicación, DTOs y orquestación.
   - Define interfaces consumidas por la infraestructura.

3. `PlataformaECommerce.Infrastructure`
   - Persistencia con EF Core sobre SQL Server.
   - Implementación de repositorios, unidad de trabajo y servicios técnicos.

4. `PlataformaECommerce.Web`
   - Capa de presentación y composición de la solución.
   - Razor Pages, controladores, autenticación, autorización, middleware y arranque.

5. `PlataformaECommerce.Tests`
   - Pruebas automatizadas por capa y por comportamiento.

## Restricciones de esta fase
- No se separará un nuevo proyecto `WebApi` en esta etapa.
- No se moverán responsabilidades ya correctamente ubicadas entre `Domain`, `Application`, `Infrastructure`, `Web` y `Tests`.
- No se introducirá una reorganización artificial distinta de la estructura real del repositorio.

## Criterio de composición
`PlataformaECommerce.Web` seguirá siendo el único punto de entrada de la aplicación mientras la solución mantenga como experiencia principal una interfaz web basada en Razor Pages.

Si en una fase futura aparece una necesidad real de integración externa, consumo por terceros o exposición pública desacoplada, podrá evaluarse una API separada. Esa decisión no forma parte de la línea base actual.

## Consecuencias prácticas
- Toda recomendación futura debe respetar `Domain`, `Application`, `Infrastructure`, `Web` y `Tests` como estructura oficial.
- Las decisiones de UI y composición deben aterrizar en `PlataformaECommerce.Web`.
- Los casos de uso deben seguir viviendo en `Application`.
- La persistencia de usuarios en SQL Server mediante EF Core debe mantenerse en `Infrastructure`.
- Las pruebas deben continuar en `PlataformaECommerce.Tests`.

## Alcance de la fase 1
Esta fase deja congelada la arquitectura real de la solución. No modifica comportamiento funcional, seguridad de ejecución ni contratos públicos; únicamente establece la línea base oficial sobre la cual deben ejecutarse las siguientes fases.

## Fase 2. Endurecimiento del modelo de identidad

### Estado
Confirmada.

### Objetivo
Dejar fijado el modelo de identidad oficial de la solución para evitar ambigüedad en roles, duplicidad de modelos de usuario y desviaciones en los flujos de autenticación.

### Decisiones oficiales

1. `RolUsuario` es la única fuente oficial de roles del sistema.
   - Todo rol funcional del negocio debe definirse y mantenerse en `PlataformaECommerce.Domain.Enums.RolUsuario`.
   - No se introducirán catálogos paralelos de roles en `Web`, `Infrastructure`, configuración o base de datos fuera del valor controlado persistido.
   - Los claims, políticas y validaciones de autorización deben derivarse de este enumerador y de sus extensiones oficiales.

2. `Cliente`, `Administrador` y `SuperUsuario` permanecen dentro del mismo modelo de usuarios.
   - El agregado raíz del modelo es `Usuario`.
   - `Cliente` y `Administrador` son especializaciones del mismo núcleo de identidad.
   - `SuperUsuario` no constituye una entidad distinta ni una tabla distinta; es una cuenta administrativa representada por `Administrador` con `RolUsuario.SuperUsuario`.
   - La persistencia debe seguir centralizada en el mismo conjunto transaccional de usuarios sobre SQL Server.

3. El acceso al sistema se realiza por correo electrónico en todos los flujos de autenticación.
   - El correo electrónico es el identificador funcional de inicio de sesión para clientes y cuentas administrativas.
   - La UI de acceso, registro y recuperación debe seguir expresando el correo como credencial de entrada.
   - No se introducirán flujos alternos basados en nombre de usuario mientras esta arquitectura siga vigente.

### Implicaciones arquitectónicas
- `RolUsuario` gobierna la semántica de autorización en todo el sistema.
- El modelo de persistencia de usuarios se mantiene unificado.
- `SuperUsuario` se trata como una capacidad privilegiada del modelo administrativo, no como un módulo aparte.
- Los servicios de autenticación y autorización deben resolver identidad a partir del correo electrónico.

### Restricciones de esta fase
- No se crearán tablas, entidades o catálogos separados para `SuperUsuario`.
- No se implementarán múltiples fuentes de verdad para roles.
- No se habilitará inicio de sesión por nombre de usuario, alias o identificadores alternos.

### Consecuencias prácticas
- Cualquier validación de rol debe apoyarse en `RolUsuario` y sus extensiones.
- Cualquier creación de cuenta debe respetar el modelo único de usuarios.
- Cualquier flujo de login, recuperación o aprovisionamiento debe seguir tomando el correo electrónico como identificador principal.

### Alcance de la fase 2
Esta fase consolida el contrato arquitectónico del modelo de identidad. No introduce aún nuevos comportamientos; fija la base obligatoria para las fases de autorización, aprovisionamiento de administradores y endurecimiento de seguridad.

## Fase 3. Blindaje de la regla de negocio crítica

### Estado
Confirmada.

### Objetivo
Fijar como regla arquitectónica obligatoria que el máximo privilegio del sistema se aprovisiona una sola vez, bajo control estricto, y que el flujo administrativo estándar nunca puede utilizarse para crear nuevas cuentas con privilegio de `SuperUsuario`.

### Reglas oficiales

1. Solo existe un `SuperUsuario` raíz inicial.
   - La plataforma reconoce una única cuenta raíz de gobierno.
   - Esa cuenta representa el punto de control inicial del backoffice.
   - No se admite, por diseño, el alta funcional de múltiples cuentas raíz desde los flujos normales del sistema.

2. El bootstrap controlado se conserva únicamente para esa cuenta.
   - El mecanismo de bootstrap existe exclusivamente para aprovisionar el primer `SuperUsuario`.
   - Una vez completado ese aprovisionamiento inicial, el bootstrap deja de ser un mecanismo funcional de creación ordinaria.
   - El bootstrap no reemplaza el flujo administrativo estándar; solo resuelve la creación controlada de la cuenta raíz inicial.

3. La creación normal de `SuperUsuario` queda prohibida por diseño.
   - Ninguna UI administrativa debe ofrecer la creación de cuentas con rol `SuperUsuario`.
   - Ningún caso de uso estándar del backoffice debe aceptar la creación funcional de un `SuperUsuario`.
   - El flujo administrativo estándar solo puede aprovisionar cuentas con rol `Administrador`.

4. Desde UI solo se permite la creación de `Administrador`.
   - La pantalla administrativa de alta de usuarios privilegiados debe estar limitada a cuentas con rol `Administrador`.
   - La UI no debe exponer selectores, parámetros ni rutas para crear `SuperUsuario`.
   - Toda definición funcional entregada a la capa de presentación debe dejar explícito que el rol permitido en el flujo normal es `Administrador`.

### Implicaciones arquitectónicas
- El `SuperUsuario` es una cuenta raíz de gobierno, no un rol de alta operativa ordinaria.
- El flujo estándar del backoffice queda acotado a la creación de cuentas `Administrador`.
- La separación entre bootstrap y aprovisionamiento normal pasa a ser una restricción obligatoria del diseño.
- La autorización y la validación del caso de uso deben reforzar esta distinción aunque la UI ya la restrinja.

### Restricciones de esta fase
- No se habilitarán formularios, endpoints ni comandos estándar para crear `SuperUsuario`.
- No se incorporarán opciones de selección de rol privilegiado en la UI de administración.
- No se reinterpretará el bootstrap como mecanismo reutilizable para crear nuevas cuentas raíz.

### Consecuencias prácticas
- El alta administrativa normal debe seguir produciendo únicamente cuentas `Administrador`.
- El bootstrap debe permanecer como capacidad técnica excepcional y controlada.
- Las validaciones de aplicación y la autorización del caso de uso deben rechazar cualquier intento de crear `SuperUsuario` fuera del flujo de bootstrap inicial.
- La documentación funcional y técnica debe tratar al `SuperUsuario` como cuenta raíz única.

### Alcance de la fase 3
Esta fase blinda la regla crítica de gobierno del sistema. Su finalidad es impedir escalamiento privilegiado desde flujos ordinarios y consolidar la separación entre la cuenta raíz inicial y la administración estándar del backoffice.

## Fase 4. Consolidación del caso de uso en `Application`

### Estado
Implementada.

### Objetivo
Concentrar el alta administrativa en un único caso de uso de `Application`, asegurando que la validación, la autorización, la unicidad de correo, la persistencia transaccional y la auditoría obligatoria se resuelvan desde la misma frontera de aplicación.

### Decisiones oficiales

1. El alta administrativa queda centralizada en `RegisterAdminAsync(RegisterAdminCommand, CancellationToken)`.
   - Tanto la UI administrativa como el bootstrap controlado deben entrar por este mismo caso de uso.
   - No se permite lógica paralela de creación de administradores fuera de `Application`.

2. El caso de uso es responsable de validar y autorizar la operación completa.
   - La validación estructural del comando se ejecuta antes de cualquier efecto en persistencia.
   - La autorización del actor se resuelve dentro del caso de uso.
   - La unicidad de correo se confirma antes de intentar registrar la cuenta.

3. La persistencia del alta administrativa debe ejecutarse bajo `UnitOfWork`.
   - El registro del nuevo administrador se ejecuta dentro de una frontera transaccional explícita.
   - La confirmación de cambios del almacenamiento transaccional no debe depender de la UI ni de infraestructura de presentación.

4. La auditoría del alta es obligatoria para considerar exitoso el flujo.
   - El caso de uso no debe considerar completada el alta si no logra registrar la traza requerida.
   - La auditoría forma parte del cierre obligatorio del caso de uso.

### Implicaciones arquitectónicas
- `Application` concentra la orquestación completa del alta administrativa.
- `Web` solo captura la intención del usuario y delega en el caso de uso.
- `Infrastructure` implementa persistencia, transacción y auditoría sin apropiarse de la lógica funcional.
- El flujo de bootstrap y el flujo de backoffice quedan alineados sobre la misma ruta de aplicación.

### Consecuencias prácticas
- La creación de administradores queda consolidada y trazable desde un solo punto.
- La autorización sensible no depende únicamente de Razor Pages o políticas HTTP.
- La solución mantiene separación limpia entre presentación, aplicación e infraestructura.

### Alcance de la fase 4
Esta fase consolida técnicamente el caso de uso de alta administrativa en `Application` y deja establecida la línea base para los siguientes endurecimientos de persistencia, seguridad operativa y pruebas integrales.

## Fase 5. Consolidación de persistencia en `Infrastructure`

### Estado
Confirmada.

### Objetivo
Dejar fijada la estrategia oficial de persistencia del módulo de usuarios en `Infrastructure`, manteniendo una única implementación de repositorio, restricciones relacionales explícitas y almacenamiento transaccional sobre SQL Server con EF Core.

### Decisiones oficiales

1. `UserRepository` se mantiene como implementación única de usuarios.
   - La persistencia del agregado `Usuario` y de sus especializaciones debe resolverse exclusivamente a través de `PlataformaECommerce.Infrastructure.Repositories.Users.UserRepository`.
   - No se introducirán implementaciones paralelas para usuarios en otros módulos o capas sin una necesidad arquitectónica nueva y explícita.

2. La unicidad por correo y las restricciones por rol son obligatorias a nivel persistente.
   - El correo electrónico de usuario debe permanecer protegido por índice único en el modelo relacional.
   - Las restricciones de rol deben seguir controladas mediante configuración de EF Core y restricciones del esquema.
   - Las cuentas administrativas y de cliente deben seguir respetando las reglas persistentes ya definidas para `Rol` y `Area`.

3. SQL Server con EF Core se mantiene como almacenamiento transaccional oficial de usuarios.
   - `ECommerceDbContext` sigue siendo la frontera transaccional relacional del sistema.
   - `Infrastructure` mantiene la responsabilidad de mapear entre el modelo persistente y el modelo de dominio.
   - No se sustituirá esta base transaccional por otro almacenamiento para usuarios en esta etapa.

4. Todo cambio de esquema debe versionarse mediante migraciones.
   - Cualquier modificación de la persistencia de usuarios debe expresarse en migraciones de EF Core.
   - No se aceptan cambios manuales de esquema fuera del proceso versionado del proyecto.
   - La evolución del modelo persistente debe conservar trazabilidad técnica y repetibilidad de despliegue.

### Implicaciones arquitectónicas
- `Infrastructure` conserva una única estrategia de persistencia para usuarios.
- Las reglas críticas de unicidad y consistencia no dependen únicamente de validaciones en `Application`.
- La solución mantiene consistencia entre repositorio, configuración EF Core, `DbContext` y migraciones.

### Consecuencias prácticas
- La creación y actualización de usuarios debe seguir entrando por `UserRepository`.
- El correo electrónico continúa siendo un identificador funcional único también en base de datos.
- La integridad por roles y por datos administrativos se conserva en el esquema relacional.
- Cualquier ajuste futuro en persistencia deberá venir acompañado por su migración correspondiente.

### Alcance de la fase 5
Esta fase confirma la línea base de persistencia profesional del módulo de usuarios en `Infrastructure`. No introduce cambios funcionales nuevos; formaliza que la solución actual ya cumple con repositorio único, restricciones relacionales, almacenamiento transaccional y versionado mediante migraciones.

## Fase 6. Aseguramiento de la capa `Web`

### Estado
Implementada.

### Objetivo
Reforzar que la capa `Web` se comporte exclusivamente como entrada HTTP y experiencia de usuario, manteniendo protección de la UI sensible, acceso por correo electrónico y `PageModel` orientados a traducción de entrada hacia comandos y consultas de `Application`.

### Decisiones oficiales

1. La UI de creación de administradores permanece protegida con `SuperUserOnly`.
   - La protección se mantiene tanto por convención de Razor Pages como por atributo explícito en la página sensible.
   - La capa `Web` no relaja ni reemplaza la autorización funcional de `Application`.

2. El inicio de sesión se mantiene por correo electrónico.
   - La experiencia de acceso de la UI debe tratar el correo electrónico como identificador principal.
   - Los mensajes, títulos, ayudas visuales y navegación deben reflejar esa semántica.

3. Los `PageModel` solo traducen entrada a comandos y consultas.
   - La capa `Web` captura datos, arma solicitudes hacia `Application` y traduce resultados a navegación o mensajes.
   - No debe contener reglas de negocio, autorización funcional ni persistencia.

4. La UX debe ser coherente con el modelo de identidad oficial.
   - El acceso, registro y recuperación deben hablar de correo electrónico como credencial.
   - La navegación entre páginas públicas y administrativas debe mantener ese mismo lenguaje funcional.

### Implicaciones arquitectónicas
- `Web` permanece desacoplada de la lógica de negocio.
- La seguridad HTTP y la experiencia de usuario quedan alineadas con el modelo de identidad definido en fases previas.
- La solución mantiene coherencia entre UI, autenticación y casos de uso.

### Consecuencias prácticas
- Las páginas de autenticación y alta administrativa siguen delegando en `Application`.
- La UI administrativa sensible continúa restringida a `SuperUsuario`.
- El usuario final recibe mensajes consistentes con acceso por correo electrónico.

### Alcance de la fase 6
Esta fase endurece la capa `Web` en seguridad de acceso, claridad de UX y separación de responsabilidades. Consolida la presentación basada en Razor Pages como fachada limpia sobre los casos de uso de `Application`.

## Fase 7. Unificación de seguridad técnica

### Estado
Implementada.

### Objetivo
Definir una configuración de autenticación técnicamente coherente para la solución, dejando las cookies como mecanismo principal del sitio basado en Razor Pages, separando el registro de JWT para uso explícito en API y reforzando la protección de sesión del backoffice y del sitio público.

### Decisiones oficiales

1. Las cookies son la autenticación principal del sitio Razor Pages.
   - La navegación interactiva del sitio y del backoffice se resuelve con cookies diferenciadas para administración y clientes.
   - Los esquemas de cookie continúan siendo la base de autenticación para Razor Pages.

2. JWT queda separado como esquema explícito para API.
   - El registro del esquema bearer no debe imponerse como autenticación predeterminada del sitio.
   - JWT queda disponible para endpoints API que lo requieran de forma explícita, sin interferir con la autenticación principal basada en cookies.

3. Las cookies deben mantenerse endurecidas a nivel técnico.
   - `HttpOnly`, `Secure` y `SameSite=Strict` siguen siendo obligatorios.
   - La expiración y la renovación deslizante continúan controladas por la configuración del esquema y por la validación de sesión.
   - Las cookies del sitio se consideran esenciales para la operación autenticada.

4. La protección de sesión debe incluir validación y revocación activa.
   - Cada solicitud autenticada por cookie debe validarse contra persistencia y consistencia de claims.
   - Si la sesión deja de ser válida, el principal se rechaza y la cookie se revoca.

5. Las rutas API no deben recibir redirecciones HTML de autenticación.
   - Las solicitudes a `/api` protegidas por cookies deben responder con `401` o `403` según corresponda.
   - La capa `Web` no debe mezclar semántica de navegación HTML con respuesta técnica de API.

### Implicaciones arquitectónicas
- Razor Pages mantiene una experiencia autenticada coherente y segura basada en cookies.
- Los endpoints API quedan desacoplados de redirecciones de login propias de la UI.
- JWT se conserva disponible sin contaminar la autenticación principal del sitio.
- La protección de sesión queda respaldada por validación técnica y revocación temprana.

### Consecuencias prácticas
- La solución evita ambigüedad entre cookies y bearer.
- Las APIs protegidas responden con códigos HTTP apropiados.
- La autenticación del backoffice y del sitio público conserva controles de seguridad empresariales sobre expiración, consistencia y revocación.

### Alcance de la fase 7
Esta fase consolida la estrategia técnica de autenticación y protección de sesión de la solución. Deja a Razor Pages sobre cookies como mecanismo principal, mantiene JWT disponible para uso explícito en API y endurece el comportamiento de seguridad de las rutas protegidas.

## Fase 8. Endurecimiento operativo

### Estado
Implementada.

### Objetivo
Elevar el endurecimiento operativo de la solución trasladando secretos fuera del repositorio, controlando con mayor rigor el bootstrap del `SuperUsuario` en producción, reforzando el logging estructurado de eventos sensibles y dejando configurada la base para monitoreo de accesos privilegiados.

### Decisiones oficiales

1. Los secretos operativos no deben permanecer en archivos versionados.
   - La clave de firma JWT deja de residir en `appsettings.json`.
   - La solución queda preparada para consumir secretos desde `User Secrets`, variables de entorno o proveedores seguros equivalentes.
   - El proyecto `Web` mantiene un identificador de `User Secrets` para desarrollo seguro.

2. El bootstrap del `SuperUsuario` queda bloqueado en producción salvo habilitación explícita.
   - En producción, el aprovisionamiento inicial solo puede ejecutarse si la configuración lo habilita de forma explícita.
   - Si la aplicación detecta que el bootstrap sigue habilitado después del primer aprovisionamiento, el arranque falla para evitar una configuración insegura persistente.

3. Los eventos sensibles deben registrarse con logging estructurado.
   - Intentos de autenticación rechazados, accesos privilegiados exitosos, revocación de sesiones administrativas y ejecución del bootstrap se registran con contexto útil para operación.
   - Los logs deben conservar información de actor, correo, rol, dirección remota y resultado, sin exponer credenciales ni secretos.

4. El monitoreo de accesos privilegiados se apoya en categorías operativas específicas.
   - La configuración de logging del sitio eleva visibilidad sobre bootstrap, autenticación y sesiones administrativas.
   - La solución queda preparada para integrarse con plataformas externas de observabilidad a partir de estos eventos estructurados.

### Implicaciones arquitectónicas
- La configuración sensible se desplaza fuera del repositorio sin romper la composición de `Web` e `Infrastructure`.
- El bootstrap del `SuperUsuario` deja de ser una capacidad operativa abierta en producción.
- La seguridad operativa incorpora trazabilidad útil para detección temprana de accesos anómalos y sesiones inválidas.

### Consecuencias prácticas
- La ejecución local y por entorno debe proveer secretos seguros antes del arranque.
- Producción exige una postura explícita y temporal para el aprovisionamiento inicial del `SuperUsuario`.
- Los equipos operativos pueden monitorear accesos privilegiados y revocaciones de sesión con mayor contexto.

### Alcance de la fase 8
Esta fase endurece la operación real de la solución más allá del código funcional. Formaliza la salida de secretos del repositorio, refuerza el control del bootstrap en producción y mejora la trazabilidad de eventos sensibles para observabilidad y gobierno operativo.

## Fase 9. Pruebas

### Estado
Implementada.

### Objetivo
Consolidar una cobertura de pruebas alineada con los riesgos principales del módulo de identidad, administración y persistencia de usuarios, asegurando validación de dominio, casos de uso críticos, autorización web y restricciones relacionales reales del almacenamiento EF Core.

### Decisiones oficiales

1. El dominio de usuarios y roles debe permanecer cubierto por pruebas unitarias.
   - Las reglas de `Usuario`, `Administrador` y `RolUsuario` deben validarse en pruebas de dominio.
   - Los roles efectivos y la semántica administrativa deben seguir protegidos por pruebas explícitas.

2. El caso de uso de alta administrativa debe probarse por escenarios críticos.
   - Se cubren creación válida por `SuperUsuario`, rechazo a `Administrador`, rechazo a usuario no autenticado, conflicto por correo duplicado y control de bootstrap único.

3. Razor Pages y políticas de autorización deben mantenerse bajo pruebas web.
   - La capa `Web` conserva cobertura sobre login, alta administrativa y políticas de acceso sensibles.
   - La autorización del backoffice y la traducción de `PageModel` hacia `Application` permanecen verificadas por pruebas dedicadas.

4. Las restricciones persistentes deben validarse con proveedor relacional real.
   - La cobertura de infraestructura no se limita al proveedor en memoria.
   - Las reglas de unicidad de correo y restricciones persistentes por rol se validan sobre SQLite en memoria para reflejar comportamiento relacional real.

### Implicaciones arquitectónicas
- La solución mantiene una base de pruebas equilibrada entre dominio, aplicación, web e infraestructura.
- Las restricciones críticas dejan de depender únicamente de validación en código y quedan verificadas también a nivel persistente.
- Los escenarios de identidad y privilegios quedan protegidos ante regresiones funcionales.

### Consecuencias prácticas
- Cambios futuros en identidad, alta administrativa, autorización o persistencia deberán conservar esta cobertura.
- El repositorio de usuarios queda validado tanto por rehidratación como por integridad relacional.
- La plataforma reduce riesgo de regresiones en los puntos más sensibles de seguridad y gobierno.

### Alcance de la fase 9
Esta fase consolida la línea base de pruebas del módulo de usuarios y administración. Formaliza la cobertura de dominio, casos de uso críticos, Razor Pages sensibles y restricciones persistentes reales como parte obligatoria de la calidad de la solución.

## Fase 10. Producción

### Estado
Implementada.

### Objetivo
Formalizar la secuencia de liberación a producción del módulo de identidad y backoffice, asegurando migraciones controladas, aprovisionamiento seguro del `SuperUsuario` inicial, desactivación posterior del bootstrap y validación operativa de auditoría, autorización y acceso restringido.

### Decisiones oficiales

1. Las migraciones deben aplicarse de forma controlada.
   - La actualización del esquema relacional debe ejecutarse mediante el flujo oficial de migraciones EF Core.
   - No se permiten cambios manuales ad hoc sobre el esquema productivo fuera del proceso versionado.

2. El `SuperUsuario` inicial se aprovisiona una sola vez.
   - El aprovisionamiento inicial se ejecuta bajo control operativo explícito.
   - La habilitación de bootstrap en producción es temporal, auditable y exclusiva para el primer alta raíz.

3. El bootstrap debe deshabilitarse inmediatamente después del aprovisionamiento.
   - La configuración de producción no debe conservar el bootstrap habilitado tras la creación inicial del `SuperUsuario`.
   - Si queda habilitado, la solución debe considerarse en estado operativo inseguro.

4. La validación posterior al despliegue es obligatoria.
   - La liberación no se considera cerrada sin verificar auditoría, autorización y restricciones efectivas de acceso.
   - El módulo de alta administrativa debe quedar accesible únicamente para `SuperUsuario`.

5. La liberación del módulo administrativo debe alinearse con gobierno de privilegios.
   - El backoffice de alta administrativa no se habilita para uso general hasta confirmar el aprovisionamiento raíz y la restricción efectiva por política.

### Implicaciones arquitectónicas
- Producción deja de ser solo un acto de despliegue técnico y pasa a ser una secuencia controlada de seguridad y gobierno.
- La arquitectura de identidad se completa con un procedimiento operativo obligatorio para el primer arranque productivo.
- Las capacidades sensibles del backoffice quedan subordinadas al control del `SuperUsuario` raíz.

### Consecuencias prácticas
- Todo despliegue productivo debe acompañarse por un runbook operativo.
- Los secretos, la habilitación temporal de bootstrap y la verificación posterior al despliegue pasan a ser parte del proceso estándar.
- La plataforma solo queda liberada cuando el acceso privilegiado y la auditoría fueron confirmados en entorno real.

### Alcance de la fase 10
Esta fase cierra la implementación profesional de la solución llevando la arquitectura a un estado realmente operable en producción. Formaliza el procedimiento de migración, aprovisionamiento inicial, desactivación de bootstrap y verificación final del backoffice administrativo.

## Fase 11. Estandarización del composition root y startup modular

### Estado
Implementada.

### Objetivo
Formalizar una convención estable para la composition root de `PlataformaECommerce.Web`, organizando el arranque por dominios operativos y reduciendo el acoplamiento entre configuración, registro de servicios, pipeline HTTP y mapeo de endpoints.

### Decisiones oficiales

1. La carpeta `Extensions/Startup` se organiza por dominio.
   - Los artefactos de startup se distribuyen en `Platform`, `Security`, `Observability`, `Presentation` y `Operations`.
   - La ubicación física de cada archivo debe reflejar su responsabilidad dominante dentro del arranque web.

2. La convención de nombres del startup queda fijada.
   - `Configure*` se reserva para la preparación del `WebApplicationBuilder`.
   - `Add*Module` se reserva para el registro de servicios por dominio.
   - `Use*Module` se reserva para fases explícitas del pipeline HTTP.
   - `Map*Endpoints` se reserva para superficies HTTP agrupadas por dominio.

3. `Program.cs` permanece mínimo y orientado al ciclo de vida.
   - El punto de entrada solo crea el builder, delega la configuración del host, construye la app, ejecuta bootstrap y arranca el proceso.
   - La composición detallada debe vivir fuera de `Program.cs`.

4. El pipeline HTTP se orquesta desde un coordinador delgado.
   - `PipelineExtensions` conserva únicamente la secuencia de alto nivel.
   - La implementación concreta de cada fase se distribuye en extensiones por dominio para preservar SRP y facilitar revisiones operativas.

5. El mapeo de endpoints se expresa por capacidades del host.
   - Operaciones, seguridad, presentación y plataforma se mapean por separado.
   - Esta agrupación debe mantenerse como estándar para nuevas superficies HTTP.

### Estructura oficial de startup en `Web`

- `Extensions/Startup/Platform`
  - host builder
  - configuración base del host
  - bootstrap de aplicación
  - inicialización runtime
  - activación runtime
  - composición de módulos
  - fuentes de configuración
- `Extensions/Startup/Security`
  - autenticación y autorización
  - antiforgery
  - forwarded headers
  - rate limiting
  - activación runtime de forwarded headers
  - activación runtime de rate limiting
  - mapeo especializado de antiforgery
  - fases de pipeline de seguridad
- `Extensions/Startup/Observability`
  - problem details
  - correlación
  - logging estructurado
  - activaciones runtime de correlación, excepciones y request logging
  - fases de pipeline de observabilidad
- `Extensions/Startup/Presentation`
  - Razor Pages y MVC
  - branding y backoffice
  - static files controlados
  - activaciones runtime de localización, headers, static files y routing
  - mapeo especializado de activos y páginas
  - fases de pipeline de presentación
- `Extensions/Startup/Operations`
  - validación de configuración de startup
  - verificación de infraestructura
  - bootstrap único e idempotente
  - warmup no destructivo
  - mantenimiento explícito exclusivo de desarrollo
  - health checks
  - OpenAPI
  - activación runtime de OpenAPI
  - mapeo específico de health/readiness
  - pipeline coordinator
  - endpoint mapping

### Implicaciones arquitectónicas
- La composition root pasa a ser una arquitectura explícita y navegable, no una colección plana de extensiones.
- El arranque de `Web` puede evolucionar por dominios sin perder claridad operativa.
- Las revisiones de seguridad, observabilidad y operación se facilitan al existir fronteras claras dentro del startup.

### Consecuencias prácticas
- Todo archivo nuevo de startup debe ubicarse en el dominio correspondiente.
- Nuevas capacidades de arranque deben respetar la convención `Configure*`, `Add*Module`, `Use*Module` y `Map*Endpoints`.
- `Program.cs` no debe volver a absorber lógica de composición detallada.
- Dentro de `Platform`, los coordinadores deben permanecer delgados y delegar configuración base, composición de módulos, inicialización runtime y activación runtime a archivos específicos.
- Dentro de `Security`, los coordinadores deben delegar antiforgery, forwarded headers y rate limiting a piezas runtime o de endpoint especializadas.
- Dentro de `Observability`, los coordinadores deben delegar correlación, manejo de excepciones y request logging a activaciones runtime específicas.
- Dentro de `Presentation`, los coordinadores deben delegar localización, headers defensivos, static files, routing y mapping de activos o páginas a piezas runtime y de endpoint especializadas.
- Dentro de `Operations`, los coordinadores deben delegar activación OpenAPI y mapeos operativos especializados a extensiones runtime específicas.
- Las tareas de startup deben separarse entre validación de configuración, verificación de infraestructura, bootstrap único, mantenimiento explícito de desarrollo y warmup no destructivo.
- Ninguna tarea peligrosa de desarrollo debe ejecutarse automáticamente en cada arranque del host web.
- Las tareas correctivas de desarrollo deben ejecutarse desde un proceso separado de mantenimiento y no desde `PlataformaECommerce.Web`.

### Alcance de la fase 11
Esta fase formaliza el startup enterprise de la solución y lo deja preparado para crecer como SaaS comercial, con una convención estable, auditable y mantenible para composition root, pipeline y endpoint mapping.

## Runbook técnico breve del pipeline HTTP

### Orden operativo oficial
El pipeline HTTP de `PlataformaECommerce.Web` debe activarse en este orden:

1. `ForwardedHeaders`
2. `Correlation ID`
3. `Exception handling`
4. `Request logging`
5. `HSTS`
6. `HTTPS redirection`
7. `OpenAPI runtime` en `Development`
8. `Localization`
9. `Security headers`
10. `Static files`
11. `Routing`
12. `Authentication`
13. `Rate limiting`
14. `Authorization`
15. `Endpoint mapping`

### Motivo de cada posición

1. `ForwardedHeaders`
   - Debe ejecutarse primero para corregir esquema, host e IP real cuando la aplicación opera detrás de proxy o balanceador.
   - Todo middleware posterior debe trabajar sobre el contexto HTTP ya normalizado.

2. `Correlation ID`
   - Debe ejecutarse al inicio para que el resto del pipeline comparta el mismo identificador de trazabilidad.

3. `Exception handling`
   - Debe envolver el resto del pipeline funcional para transformar fallos en respuestas seguras y observables.
   - Debe ejecutarse después de la correlación para emitir errores con el mismo identificador de seguimiento.

4. `Request logging`
   - Debe ocurrir temprano para registrar duración, resultado y contexto enriquecido de toda la solicitud.

5. `HSTS`
   - En entornos no locales debe emitirse antes de servir contenido para endurecer la política de transporte seguro del navegador.

6. `HTTPS redirection`
   - Debe ejecutarse antes de UI, archivos o endpoints funcionales para forzar el canal seguro cuanto antes.

7. `OpenAPI runtime`
   - Se expone solo en `Development` y después del endurecimiento básico de transporte, sin afectar la ruta productiva.

8. `Localization`
   - Debe ejecutarse antes de la UI y de respuestas dependientes de cultura para fijar idioma y formato efectivos.

9. `Security headers`
   - Debe ejecutarse antes de entregar contenido para proteger páginas y respuestas HTTP con headers defensivos.

10. `Static files`
    - Debe ir antes de `Routing` para resolver activos rápidamente sin pasar por el pipeline completo de endpoints.

11. `Routing`
    - Debe ejecutarse antes de autenticación, rate limiting y autorización para que el sistema conozca la superficie HTTP seleccionada.

12. `Authentication`
    - Debe ejecutarse antes de `Rate limiting` y `Authorization` para disponer del usuario autenticado en políticas y particiones.

13. `Rate limiting`
    - Debe ejecutarse después de autenticación para poder particionar por actor autenticado cuando aplique.

14. `Authorization`
    - Debe ejecutarse después de autenticación y limitación de tráfico para aplicar políticas sobre identidad ya resuelta.

15. `Endpoint mapping`
    - Debe permanecer al final del bootstrap HTTP como cierre natural de la composición del host.

### Restricción operativa
- No alterar este orden sin justificar impacto en trazabilidad, seguridad de transporte, autenticación o comportamiento detrás de proxy.
- Cualquier cambio futuro del pipeline debe preservar esta secuencia o documentar explícitamente la nueva razón operativa.

## Runbook técnico breve de mantenimiento explícito

### Proceso separado oficial
La normalización de datos legacy por tenant y cualquier bootstrap funcional SaaS dejan de ejecutarse desde el arranque de `PlataformaECommerce.Web` y pasan a ejecutarse únicamente desde `PlataformaECommerce.Maintenance`.

### Uso previsto
- Ejecutar solo en `Development`.
- Usar cuando existan filas históricas sin `TenantId` después de migrar una base local antigua al modelo SaaS actual.
- No usar como parte del arranque ordinario del sitio web.

### Garantías operativas
- El host web ya no ejecuta esta corrección en cada inicio.
- El operador debe invocar el proceso de mantenimiento bajo intención explícita.
- El proceso puede fijar un tenant concreto con `--tenant=<tenantId>` cuando la corrección requiera alcance controlado.
- Los comandos mutantes de bootstrap o seed se ejecutan bajo lock exclusivo sobre SQL Server para evitar carreras entre varias instancias o ejecuciones simultáneas.

### Comandos oficiales
- `inspect-legacy-tenant-data [--tenant=<tenantId>]`: inspecciona de forma no destructiva si existen filas legacy pendientes.
- `normalize-legacy-tenant-data [--tenant=<tenantId>]`: ejecuta la corrección explícita de filas legacy sin tenant.
- `readiness/bootstrap-status [--tenant=<tenantId>]`: inspecciona por tenant si ya existen sync, seed y bootstrap persistidos sin mutar datos.
- `sync-saas-catalog [--tenant=<tenantId>]`: sincroniza el catálogo SaaS persistente desde configuración.
- `seed-configured-tenants [--tenant=<tenantId>]`: ejecuta la siembra funcional configurada para tenants.
- `bootstrap-superuser [--tenant=<tenantId>]`: ejecuta el bootstrap explícito del superusuario inicial.
- `run-saas-bootstrap [--tenant=<tenantId>]`: ejecuta sync, seed funcional y bootstrap en una sola operación protegida.
- `help`: muestra la lista actual de comandos soportados por `PlataformaECommerce.Maintenance`.

### Estructura recomendada del proceso de mantenimiento
- `Program.cs` debe permanecer como composition root mínima.
- El parseo de argumentos debe vivir en `MaintenanceCommandRequest`.
- El enrutamiento y alcance operativo debe vivir en `MaintenanceCommandDispatcher`.
- La implementación concreta de comandos legacy debe vivir en `LegacyTenantMaintenanceCommands`.
- La implementación concreta de bootstrap SaaS debe vivir en `SaaSBootstrapMaintenanceCommands`.

### Restricción operativa
- Cualquier nueva tarea correctiva o destructiva debe seguir este mismo patrón: proceso separado, intención explícita y fuera del bootstrap HTTP del host web.
- En despliegues multi-instancia, el host web solo puede ejecutar validación, verificación técnica y warmup; sync, seed y bootstrap funcional deben ejecutarse como comandos separados bajo exclusión mutua.
