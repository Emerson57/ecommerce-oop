# Changelog

Todas las modificaciones relevantes de producto, operación y soporte deben registrarse en este archivo.

## [1.5.0] - 2026-04-04
### Added
- Configuración monocliente `ClientExperience` para branding, soporte y personalización visual del storefront y del backoffice.
- Centro de `Operación y soporte` dentro del backoffice con `ClientId`, versión, ambiente, correlación y guías operativas.
- Versionado centralizado de la solución mediante `Directory.Build.props`.
- Documentación formal de instalación, operación y soporte en `docs/`.

### Changed
- Dashboard administrativo con métricas comerciales básicas más visibles, acceso directo al centro operativo y trazabilidad ampliada en la actividad reciente.
- Footer comercial y layouts públicos/administrativos alineados al branding configurable.
- Soporte técnico mejorado mediante exposición de correlación, versión y contexto del cliente activo.

### Fixed
- El backoffice ahora expone información mínima para diagnóstico sin depender de inspección manual del entorno de despliegue.
