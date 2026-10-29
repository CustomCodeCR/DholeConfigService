# Transición de navieras y agentes de Config a SRM

## Objetivo y restricción operativa

El módulo SRM debe ser propietario de la identidad del proveedor, sus contactos, sedes, contrato, frecuencia, SLA, vigencia, moneda, condiciones de pago y acuerdos comerciales. Config conserva catálogos corporativos reutilizables (**países, POL/POE/POD, moneda, equipo, incoterms**). Pricing continúa siendo propietario de tarifas, costos, reglas de cálculo y revisiones QUO.

**No eliminar ni desactivar aún** `carriers` y `agents` de Config: Pricing, catálogos de importación y cotizaciones anteriores almacenan referencias/códigos. Borrarlos rompe históricos y selección de nuevas tarifas. No convertir códigos Config a UUID SRM sin conciliación.

## Fase A — Catálogo SRM existente (implementado por este PR)

- `GET /api/config/srm/legacy-partners` devuelve navieras y agentes legados con estado de incorporación.
- `POST /api/config/srm/legacy-partners/{carriers|agents}/{legacyId}/import` incorpora una fila al directorio SRM manteniendo `legacyConfigId`, `legacyConfigGroup`, `legacyCode`, y conserva fuente intacta.
- En coincidencia exacta por referencia, responder idempotente; si coincide el nombre y no identidad, registrar conflicto para revisión, nunca fusionar silenciosamente.
- SRM permite guardar país Config, identificación, sitio, contactos y condiciones de contrato (días, divisa, referencia de acuerdo y servicios incluidos).
- El almacén **sigue siendo Config** mediante la fachada SRM; no afirmar que exista un microservicio SRM separado o que sea ya fuente definitiva de condiciones.
- Conservar scopes `srm.*` y no otorgar `config.catalog-*` a vendedores SRM.

## Fase B — Servicio SRM como fuente de verdad (pendiente)

1. Implementar esquema y API SRM con tablas/versiones `srm_suppliers`, `srm_contacts`, `srm_supplier_locations`, `srm_supplier_contracts`, `srm_service_conditions`, `srm_supplier_legacy_aliases`.
2. Contrato gRPC autenticado `ResolveSupplierByLegacyReference`, `ListActiveSuppliers`, `GetEffectiveSupplierTerms` con deadline/caché con TTL e invalidez por evento.
3. Importar desde fachada SRM actual y desde Config sin duplicación de identidad; guardar mapa reversible `(catalog_group_slug, catalog_item_id, catalog_item_code) → srm_supplier_id`.
4. Representar relaciones comerciales con fecha de vigencia, ruta POL/POE/POD, tipo de servicio, modalidad, país, moneda, pago, prioridad y aprobación; **no** guardar un tarifario completo en JSON de Config.
5. Proteger condiciones sensibles por scope comercial y auditar versiones; los vencimientos y cambios alimentan Notifications.

## Fase C — Integrar Pricing, DataExtraction, CRM y Web (pendiente)

- Buscar **todas** las referencias a `carriers` y `agents`: wizard Pricing, FCL/LCL/LTL/FTL/aéreo, importador, tarifarios, DataExtraction, Config gRPC, consultas/reportes y snapshots históricos.
- Cambiar el origen de la selección de proveedor a SRM vía API/Gateway; aceptar alias Config para lectura de tarifas previas. Siempre mostrar nombre de proveedor real y sus códigos/aliases existentes.
- Pricing calcula el costo/venta con su motor; SRM responde proveedor y condiciones aprobadas, nunca modifica líneas de tarifa sin revisión.
- Resolver cualquier preferencia **de cliente** mediante una política aprobada de Pricing ligada a `crm_account_id`, modalidad, ruta, moneda, vigencia y versión QUO. No aplicar descuentos o premios silenciosos.
- Conservar compatibilidad en APIs y mensajes de cola/gRPC durante ventana de transición.
- Probar cotizaciones nuevas y revisiones históricas, detección de duplicados y rollback por feature flag.

## Fase D — Corte, validación y retiro de edición legacy (pendiente)

- Dry-run produce conteos de `legacy_carriers`, `legacy_agents`, vinculados, ambiguos, sin referencia y fallos.
- Conciliar en staging: 100% referencias activas de tarifas y solicitudes, y casos históricos críticos, sin falsear identidades.
- Habilitar **dual-read** temporal SRM+Config con métricas de fallback; las escrituras a proveedor pasan exclusivamente a SRM cuando se confirme la integridad.
- Marcar catálogos legacy como **solo lectura** durante periodo de reversión. Conservarlos como aliases de históricos; no hacer `DELETE` destructivo por defecto.
- Monitorizar disponibilidad, latencia, retornos 404 y discrepancia de proveedor. Rollback a lecturas Config usando flags sin revertir filas nuevas SRM.

## Comprobaciones pendientes que impiden declarar la migración terminada

- PR Config y Web aprobados, compilaciones y pruebas E2E.
- Implementación y despliegue real del servicio SRM dedicado y contratos gRPC.
- Adaptación del cálculo/selección en Pricing con tests para todas las modalidades.
- Backfill con reporte de conciliación firmado.
- UI de condiciones negociadas y preferenciales aplicadas a cotizaciones de forma auditable.
- Verificación de países, POL/POE/POD y monedas de Config en los entornos objetivo; no asumir cobertura global sin inventario.
