## [1.8.0] - 2026-11-16 (SRM: ficha fiscal y banca cifrada · master backport)

- MINOR DH-20261116-SRM-SUPPLIER-FINANCE-176: perfil corporativo y UPPER CASE; integración Hacienda CR; contacto principal de Finanzas; separación bancaria cifrada AES-256-GCM y scopes restringidos. Migración `20261116170000_AddSrmSupplierBankRecords`.
- `DHOLE_SRM_BANK_KEY_BASE64`: 32 bytes Base64 que deben permanecer estables y respaldarse de forma segura; el endpoint bancario no permite guardar registros sin esta llave. Cambios realtime usan envelope SRM sin contenido bancario.
- Backport selectivo sin promover cambios pendientes ajenos de develop.

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
