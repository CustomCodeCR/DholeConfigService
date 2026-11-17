## [1.9.0] - 2026-11-17 (SRM: eliminación lógica de proveedores)

- MINOR DH-20261117-SRM-SUPPLIER-DELETE-190: DELETE /api/config/srm/suppliers/{supplierId} con scope srm.supplier.archive; impide borrar proveedores del sistema, inexistentes o con contratos y evaluaciones vinculados.
- Conserva la banca cifrada, genera auditoría y Outbox realtime de invalidación SRM sin datos sensibles; añade prueba de referencias exactas.
- Config 1.8.1 → 1.9.0; ecosistema 1.69.2 → 1.70.0. Backport selectivo a master autorizado por el usuario; CI y validación post-despliegue obligatorias.

## [1.8.1] - 2026-11-16 (SRM: IBAN seguro por país)

- PATCH DH-20261116-SRM-IBAN-181: endpoint bancario rechaza IBAN sin prefijo, longitud exacta o dígitos de control MOD-97 válidos según país registrado en Config; acepta números nacionales sin prefijo IBAN ficticio.
- Se conserva cifrado AES-256-GCM, scopes financieros, endpoint REST y evento de invalidación de realtime sin publicar datos bancarios. Pruebas de IBAN CR, DE, GB y de cuentas nacionales.
- Config 1.8.0 → 1.8.1; ecosistema 1.66.1 → 1.66.2; CI/staging pendientes y producción no autorizada.

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
