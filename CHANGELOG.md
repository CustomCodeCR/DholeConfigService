# Changelog

## [1.5.0] - 2026-11-12

- MINOR DH-20261112-CRM-ACTIVITY-TYPES-125: incorpora al catálogo CRM Seguimiento comercial (COMMERCIAL_FOLLOWUP), WhatsApp, Recomendación y Oferta; se conserva CALL con nombre visible «Llamada telefónica» y el seguimiento FOLLOWUP independiente.
- Migración idempotente sin eliminar referencias existentes ni modificar REST, SignalR u Outbox. Config 1.2.1 → 1.5.0; ecosistema 1.46.0. CI y staging pendientes; producción no autorizada.

## [1.2.1] - 2026-11-05

- Actualiza versión SemVer del componente y metadatos del ecosistema Dhole 1.3.1.
- Garantiza despliegues por push en master/develop y preserva las comprobaciones CI.

## [1.2.0] - 2026-11-05

- Incorporates universal Dhole SemVer policy with GitHub Actions checks.
- Synchronizes service assembly metadata with the ecosystem version.
- Adds permanent Codex instructions and coordinated release requirements.
