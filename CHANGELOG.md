# Changelog

## [1.3.0] - 2026-11-08 (candidato F6.05, no liberado)

- SRM: nuevo evento `srm.supplier.changed.v1` separado de `config.catalog-item`, accesible únicamente con `srm.supplier.read` y verificación de grupo real `srm-suppliers`; evita conceder ámbitos de administración Config a operadores SRM. Compilación y 4 pruebas CI PASS #42501309773.

- Config/SRM: invalidaciones mínimas para grupos/items de catálogo desde SaveChanges con el Outbox durable existente; cubre mutaciones directas de SRM persistidas en catálogo.
- Stream independiente `dhole.config.realtime.v1`, payload sin datos del proveedor/catálogo y flags de batch apagados. El consumidor deberá consultar REST autorizado, sin confiar en hints de versión de distintas instancias.
- Pruebas de esquema, flags, aislamiento y versiones locales; SemVer 1.2.1 → 1.3.0, versión global candidata 1.13.0. Sin deploy.

## [1.2.1] - 2026-11-05

- Despliegues push master/develop sin cancelaciones ni aprobaciones manuales.
- Metadatos nativos y validación SemVer actualizados para Dhole.

## [1.2.0] - 2026-11-05

- Incorporates universal Dhole SemVer policy with GitHub Actions checks.
- Synchronizes service assembly metadata with the ecosystem version.
- Adds permanent Codex instructions and coordinated release requirements.
