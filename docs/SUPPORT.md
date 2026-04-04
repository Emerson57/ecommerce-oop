# Soporte técnico y funcional

## Objetivo
Reducir el tiempo de diagnóstico usando trazabilidad, correlación y contexto mínimo obligatorio por incidente.

## Datos mínimos para abrir un caso
- `ClientId`
- versión desplegada
- ambiente
- correo del usuario afectado, si aplica
- hora aproximada del incidente en UTC
- módulo afectado (`Auth`, `Users`, `Products`, `Orders`, `Payments`, `Admin`)
- `Correlation Id`, si está disponible
- evidencia visual o mensaje exacto

## Dónde obtener la información
### Desde backoffice
- `Admin > Operación y soporte`
  - muestra `ClientId`
  - muestra versión
  - muestra ambiente
  - muestra `Correlation Id` actual
  - muestra header de correlación configurado

### Desde auditoría
- `Admin > Auditoría transversal`
  - filtra por actor
  - filtra por módulo
  - filtra por acción
  - filtra por `Correlation Id`
  - filtra por rango UTC

## Procedimiento de diagnóstico
1. Confirmar `ClientId` y versión.
2. Buscar el incidente por `Correlation Id` en auditoría.
3. Si no hay correlación, filtrar por actor, módulo y ventana UTC.
4. Confirmar si el problema es funcional, de configuración o de disponibilidad.
5. Revisar `health/live` y `health/ready`.
6. Documentar causa, impacto, mitigación y siguiente acción.

## Clasificación rápida
- **P1**: caída del storefront, del backoffice o de `health/ready`
- **P2**: operaciones críticas degradadas (login, checkout, pedidos, pagos)
- **P3**: fallas parciales de backoffice, branding o documentación operativa
- **P4**: mejoras de UX, ajustes menores o aclaraciones documentales

## Cierre de incidente
Un caso se considera cerrado cuando:
- existe diagnóstico registrado
- existe acción aplicada o workaround documentado
- se confirmó la versión afectada
- se verificó el estado final
- el changelog fue actualizado si hubo release
