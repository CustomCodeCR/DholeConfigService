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

- MINOR DH-20261117-SRM-SUPPLIER-DELETE-190: DELETE /api/config/srm/suppliers/{supplierId} protegido por srm.supplier.archive; verifica pertenencia a SRM y bloquea proveedores del sistema o con contratos/evaluaciones vinculados (409).
- Reutiliza DeleteCatalogItemCommand: eliminación lógica auditada, caché invalidada y eventos existentes mediante Outbox/SignalR; no borra datos bancarios cifrados ni referencias históricas.
- Config 1.8.1 → 1.9.0; ecosistema 1.69.1 → 1.70.0. CI/staging pendientes; sin autorización de producción.

## [1.8.1] - 2026-11-16 (SRM: IBAN seguro por país)

- PATCH DH-20261116-SRM-IBAN-181: endpoint bancario rechaza IBAN sin prefijo, longitud exacta o dígitos de control MOD-97 válidos según país registrado en Config; acepta números nacionales sin prefijo IBAN ficticio.
- Se conserva cifrado AES-256-GCM, scopes financieros, endpoint REST y evento de invalidación de realtime sin publicar datos bancarios. Pruebas de IBAN CR, DE, GB y de cuentas nacionales.
- Config 1.8.0 → 1.8.1; ecosistema 1.66.1 → 1.66.2; CI/staging pendientes y producción no autorizada.

## [1.8.0] - 2026-11-16
### Added
- Datos corporativos SRM y validación de contacto financiero, normalización UPPER CASE en API.
- Consulta de contribuyentes de Costa Rica mediante Hacienda con caché 12 horas y control de concurrencia.
- Almacenamiento bancario separado y cifrado AES-256-GCM con scopes específicos; cambios notifican por Outbox sin transmitir datos bancarios.
- Migración config.srm_supplier_bank_records. Requiere llave DHOLE_SRM_BANK_KEY_BASE64 de 32 bytes para habilitar lectura y guardado.
- Release DH-20261116-SRM-SUPPLIER-FINANCE-176 (ecosistema 1.64.0); CI, migración y staging pendientes.

## [1.7.0] - 2026-11-16 (Inventario protegido para API Manager)

- MINOR DH-20261116-API-ROUTES-DEFAULTS-165: publica GET /api/internal/third-party-route-catalog con métodos y rutas runtime, accesible solo con JWT y scope integrations.thirdparty.manage; no necesita Swagger de desarrollo.
- Oculta rutas internas, credenciales y sondas; no modifica seguridad de ninguna operación.

## [1.6.0] - 2026-11-14 (API Manager discovery)

- MINOR: catálogo interno seguro de rutas REST configurables para el administrador, sin publicar datos ni endpoints externos automáticamente.
- Release global Dhole 1.56.0; conserva API y catálogos Config.

# Changelog

## [1.5.0] - 2026-11-12

- MINOR DH-20261112-CRM-ACTIVITY-TYPES-125: incorpora al catálogo CRM Seguimiento comercial (COMMERCIAL_FOLLOWUP), WhatsApp, Recomendación y Oferta; se conserva CALL con nombre visible «Llamada telefónica» y el seguimiento FOLLOWUP independiente.
- Migración idempotente sin eliminar referencias existentes ni modificar REST, SignalR u Outbox.
- Fija Microsoft.AspNetCore.WebUtilities 2.3.14 (GHSA-6527-9fp8-r4x6) en Infrastructure para resolver la dependencia transitiva vulnerable y mantener activo el control NU1902. Config 1.4.1 → 1.5.0; ecosistema 1.46.0. CI y staging pendientes; producción no autorizada.

## [1.4.1] - 2026-11-09

- PATCH: transactional metadata-only config.catalog-list invalidations on all catalog-item and group mutations so newly created ports and dropdown options can refresh through NotificationsHub.
- Authenticated no-content owner ACL endpoint and staging DholeSystem/develop infrastructure. Ecosystem 1.15.2; production unchanged.


## [1.4.0] - 2026-11-09 (NotificationsHub universal realtime pilot; candidate)

- MINOR: retain existing domain Outbox/REST realtime producer under NotificationsHub-based universal delivery; no shared framework changes.
- Ecosystem candidate 1.14.0 / 16 Dhole repositories, release `rt-20261107-01` in isolated branch `feature/rt-notificationshub-20261109-01`.
- Staging deployment, operational audit and final integration remain pending. Production flags OFF.


## [1.3.6] - 2026-11-08 (Dhole realtime coordinated hardening)

- PATCH 1.13.15 ecosystem release alignment with F2/F6 AOF read-only checks and F10 branch-preservation fix. Service candidate unchanged; realtime flags OFF and gates F6–F10 not formally approved.



## [1.3.5] - 2026-11-08 (F9 review candidate)

- PATCH SemVer global 1.13.14 para excepción F3 en revisión de F9. Mantiene gates formales, flags OFF y prohibición de despliegue hasta verificar staging y durabilidad.



## [1.3.4] - 2026-11-08 (candidato F3)

- Sincronización PATCH de la versión global 1.13.13 correspondiente al ensayo WSS/Traefik de DholeApiGateway; sin nuevas modificaciones funcionales ni despliegue.



## [1.3.3] - 2026-11-08 (F6/F9 staging-audit candidate)

- Sincroniza Dhole Realtime `rt-20261107-01` a versión global `1.13.12` tras incorporar auditoría real de persistencia en staging. PATCH 1.3.2 → 1.3.3; no modifica rutas, eventos, datos, contratos REST ni flags. No se fusiona hasta completar gates formales.



## [1.3.2] - 2026-11-08 (candidato F9)

- Corrige el CI principal de integración MSTest: comparaciones de versiones convertidas a `Assert.IsGreaterThan` y elimina una aserción tautológica de la constante de stream; no cambia productores, eventos, contratos ni datos. Sincroniza la versión global candidata `1.13.11` con el próximo manifiesto de batch.



## [1.3.1] - 2026-11-08 (candidato F9, sin publicación)

- Sincroniza metadatos de versión global del batch Dhole Realtime `rt-20261107-01` a `1.13.10`; actualización PATCH independiente de DholeConfigService `1.3.0` → `1.3.1`. No cambia contratos REST, datos, lógica de negocio ni flags; requiere gates F0–F8 y CI de SHA final antes de merge.



## [1.3.0] - 2026-11-08 (candidato F6.05, no liberado)

- SRM: nuevo evento `srm.supplier.changed.v1` separado de `config.catalog-item`, accesible únicamente con `srm.supplier.read` y verificación de grupo real `srm-suppliers`; evita conceder ámbitos de administración Config a operadores SRM. Compilación y 4 pruebas CI PASS #42501309773.

- Config/SRM: invalidaciones mínimas para grupos/items de catálogo desde SaveChanges con el Outbox durable existente; cubre mutaciones directas de SRM persistidas en catálogo.
- Stream independiente `dhole.config.realtime.v1`, payload sin datos del proveedor/catálogo y flags de batch apagados. El consumidor deberá consultar REST autorizado, sin confiar en hints de versión de distintas instancias.
- Pruebas de esquema, flags, aislamiento y versiones locales; SemVer 1.2.1 → 1.3.0, versión global candidata 1.13.0. Sin deploy.


## [1.2.1] - 2026-11-05

- Despliegues push master/develop sin cancelaciones ni aprobaciones manuales.
- Metadatos nativos y validación SemVer actualizados para Dhole.


### Notas conservadas de master


- Actualiza versión SemVer del componente y metadatos del ecosistema Dhole 1.3.1.
- Garantiza despliegues por push en master/develop y preserva las comprobaciones CI.


## [1.2.0] - 2026-11-05

- Incorporates universal Dhole SemVer policy with GitHub Actions checks.
- Synchronizes service assembly metadata with the ecosystem version.
- Adds permanent Codex instructions and coordinated release requirements.
