# Operación de la plataforma

## Objetivo
Operar el storefront y el backoffice con foco en estabilidad, trazabilidad y soporte comercial.

## Backoffice disponible
- `Admin > Dashboard`
- `Admin > Operación y soporte`
- `Admin > Auditoría transversal`
- `Admin > Productos`
- `Admin > Categorías`
- `Admin > Usuarios` (solo superusuario)

## Dashboard administrativo
El dashboard resume métricas básicas para operación diaria:
- catálogo total, activos, destacados y bajo stock
- pedidos activos, entregados y cancelados
- usuarios totales, clientes, administradores y accesos recientes
- facturación total y facturación de ventana
- ticket promedio general y de ventana
- carritos activos y eventos auditados recientes
- usuarios pendientes de confirmar correo
- pedidos que requieren atención activa

## Centro de operación y soporte
La pantalla `Admin > Operación y soporte` centraliza:
- `ClientId`
- nombre comercial activo
- versión desplegada
- ambiente actual
- canal de soporte configurado
- header de correlación
- `Correlation Id` de la solicitud actual
- ubicación de manuales operativos

## Health checks
- `GET /health/live`
- `GET /health/ready`

## Trazabilidad operativa
La plataforma expone correlación mediante el header configurado en `Observability:CorrelationHeaderName`.

Regla operativa:
1. Cada incidente debe registrarse con `ClientId`.
2. Cada incidente debe incluir `Correlation Id` si fue observado desde UI o API.
3. Toda investigación debe intentar correlación cruzada con `Admin > Auditoría transversal`.
4. Toda corrección productiva debe reflejarse en `CHANGELOG.md`.

## Rutina diaria sugerida
1. Verificar `health/live` y `health/ready`.
2. Revisar backlog operativo en el dashboard.
3. Revisar productos con bajo stock o agotados.
4. Revisar pedidos con atención activa.
5. Revisar eventos recientes de auditoría.
6. Confirmar si el branding, soporte y `ClientId` corresponden al cliente desplegado.

## Señales de escalamiento
Escala a soporte técnico o desarrollo cuando:
- `health/ready` falle
- existan errores de autenticación repetidos con la misma correlación
- el dashboard no cargue métricas
- la auditoría no retorne eventos recientes esperados
- el branding desplegado no corresponda al cliente configurado
