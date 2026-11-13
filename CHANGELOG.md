# Changelog

## [1.5.0] - 2026-11-12

- MINOR DH-20261112-CRM-ACTIVITY-TYPES-125: incorpora al catálogo CRM Seguimiento comercial (COMMERCIAL_FOLLOWUP), WhatsApp, Recomendación y Oferta; se conserva CALL con nombre visible «Llamada telefónica» y el seguimiento FOLLOWUP independiente.
- Migración idempotente sin eliminar referencias existentes ni modificar REST, SignalR u Outbox. Config 1.4.1 → 1.5.0; ecosistema 1.46.0. CI y staging pendientes; producción no autorizada.

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
