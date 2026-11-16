## [1.7.0] - 2026-11-16 (Inventario HTTP protegido en producción)

- MINOR DH-20261116-API-ROUTES-DEFAULTS-165: porta el catálogo runtime desde develop a master, protegido por JWT y integrations.thirdparty.manage, sin exposición de operaciones internas ni cambio de permisos REST.
- Se alinea con Gateway 1.10.0 y el manifiesto Dhole 1.63.0.

# Changelog

## [1.5.0] - 2026-11-12

- MINOR DH-20261112-CRM-ACTIVITY-TYPES-125: incorpora al catálogo CRM Seguimiento comercial (COMMERCIAL_FOLLOWUP), WhatsApp, Recomendación y Oferta; se conserva CALL con nombre visible «Llamada telefónica» y el seguimiento FOLLOWUP independiente.
- Migración idempotente sin eliminar referencias existentes ni modificar REST, SignalR u Outbox.
- Fija Microsoft.AspNetCore.WebUtilities 2.3.14 (GHSA-6527-9fp8-r4x6) en Infrastructure para resolver la dependencia transitiva vulnerable y mantener activo el control NU1902. Config 1.2.1 → 1.5.0; ecosistema 1.46.0. CI y staging pendientes; producción no autorizada.

## [1.2.1] - 2026-11-05

- Actualiza versión SemVer del componente y metadatos del ecosistema Dhole 1.3.1.
- Garantiza despliegues por push en master/develop y preserva las comprobaciones CI.

## [1.2.0] - 2026-11-05

- Incorporates universal Dhole SemVer policy with GitHub Actions checks.
- Synchronizes service assembly metadata with the ecosystem version.
- Adds permanent Codex instructions and coordinated release requirements.
