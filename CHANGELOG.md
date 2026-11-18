## [1.11.0] - 2026-11-18 (SRM · rutas de proveedores)

- MINOR DH-20261118-SRM-ROUTES-225: crea el catálogo de 10 categorías SRM específicas, desactiva categorías genéricas de transporte sin borrar clasificaciones históricas.
- Agrega cobertura de rutas por proveedor (multiorigen POL y multidestino POE/POD) con validación de terminal, modalidad y catálogo; endpoints CRUD con scopes srm.supplier.read/update.
- Migración PostgreSQL indexada, bitácora de cambios por actor y eventos Outbox realtime en transacciones. Endpoint limitado a identidad y cobertura para futuras búsquedas de proveedores por parte de Pricing, con scope pricing.cost.view.
- Config 1.10.1 → 1.11.0; Web 1.80.0 → 1.81.0; ecosistema 1.84.0 → 1.85.0. Requiere migración, pruebas y staging antes de integración a producción.

## [1.10.1] - 2026-11-17 (SRM · reutilizar nombre archivado)

- PATCH DH-20261117-SRM-ARCHIVE-UI-203: crear un proveedor nuevo con el nombre de un proveedor previamente archivado ya no provoca srm duplicate-name; SRM valida solo nombres activos, mientras los otros catálogos mantienen la reserva histórica.
- Migración PostgreSQL parcial del índice único de nombre de CatalogItems (is_deleted = false). Los registros archivados conservan ID, slug, código, auditoría, referencias y datos históricos; los nuevos obtienen IDs/códigos/slugs nuevos. Rollback detecta duplicados antes de restaurar la restricción.
- Config 1.10.0 → 1.10.1; ecosistema 1.72.0 → 1.73.0. Requiere validación PostgreSQL y staging.

## [1.10.0] - 2026-11-17 (SRM · seguimiento y bitácora de proveedores)

- MINOR DH-20261117-SRM-RECORDS-201: bitácora append-only por proveedor con tipo de gestión, asunto, detalle, siguiente gestión opcional, autor y fecha de servidor.
- GET y POST /api/config/srm/suppliers/{supplierId}/followups; scopes SRM de lectura y edición, validación del proveedor, persistencia PostgreSQL, migración y evento Redis Outbox saneado.
- No se agregan documentos al catálogo JSON ni se altera la protección de cuentas bancarias. Config 1.9.1 → 1.10.0; ecosistema 1.70.1 → 1.71.4.

## [1.9.1] - 2026-11-17 (DH-20261117-CONFIG-APT-DEPLOY-191 · producción Config: build Docker resistente)

- PATCH: acota `apt-get update/install` en los dos Dockerfiles a 180/300 s, fuerza IPv4, limita tiempos de conexión y reintenta errores de red sin prompts de tzdata.
- Docker en staging y producción usa la red del host para el build, logs `--progress=plain` y tiempo máximo; los releases nuevos de producción cancelan el build anterior atascado con la misma concurrency key, sin tocar los contenedores existentes hasta construir ambas imágenes.
- API y workers conservan las mismas dependencias; no se cambian contratos REST, datos, OAuth, Redis, SignalR ni lógica de SRM.
- Config 1.9.0 → 1.9.1; ecosistema 1.70.0 → 1.70.1. Validar CI, staging y health tras despliegue.

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
