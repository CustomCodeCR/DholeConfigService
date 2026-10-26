using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dhole.Config.Persistence.Migrations;

/// <summary>Idempotent CRM phase 2 catalogs. Existing manual records are never overwritten.</summary>
[DbContext(typeof(ServiceDbContext))]
[Migration("20261014194500_AddCrmPhase2Catalogs")]
public sealed class AddCrmPhase2Catalogs : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
INSERT INTO config."CatalogGroups"
  (id, code, slug, name, description, metadata_json, is_system, is_active,
   created_at_utc, created_by, is_deleted)
VALUES
  (gen_random_uuid(), 'CRM_CUSTOMER_TYPES', 'crm-customer-types', 'Tipos de cliente',
   'Catálogo comercial DholeCRM Fase 2',
   '{"crm":true,"phase":2}'::jsonb, true, true, now(), 'migration', false)
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems"
  (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
   is_system, is_active, created_at_utc, created_by, is_deleted)
SELECT gen_random_uuid(), cg.id, 'CASH', 'cash',
  'Contado', 'Opción de catálogo CRM', 'Contado',
  '{}'::jsonb, 10, true, true, now(), 'migration', false
FROM config."CatalogGroups" cg
WHERE cg.slug = 'crm-customer-types' AND cg.is_deleted = false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems"
  (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
   is_system, is_active, created_at_utc, created_by, is_deleted)
SELECT gen_random_uuid(), cg.id, 'CREDIT', 'credit',
  'Crédito', 'Opción de catálogo CRM', 'Crédito',
  '{}'::jsonb, 20, true, true, now(), 'migration', false
FROM config."CatalogGroups" cg
WHERE cg.slug = 'crm-customer-types' AND cg.is_deleted = false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogGroups"
  (id, code, slug, name, description, metadata_json, is_system, is_active,
   created_at_utc, created_by, is_deleted)
VALUES
  (gen_random_uuid(), 'CRM_ACCOUNT_SOURCES', 'crm-account-sources', 'Fuentes de clientes',
   'Catálogo comercial DholeCRM Fase 2',
   '{"crm":true,"phase":2}'::jsonb, true, true, now(), 'migration', false)
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems"
  (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
   is_system, is_active, created_at_utc, created_by, is_deleted)
SELECT gen_random_uuid(), cg.id, 'WEB', 'web',
  'Sitio web', 'Opción de catálogo CRM', 'Sitio web',
  '{}'::jsonb, 10, true, true, now(), 'migration', false
FROM config."CatalogGroups" cg
WHERE cg.slug = 'crm-account-sources' AND cg.is_deleted = false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems"
  (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
   is_system, is_active, created_at_utc, created_by, is_deleted)
SELECT gen_random_uuid(), cg.id, 'REFERRAL', 'referral',
  'Referido', 'Opción de catálogo CRM', 'Referido',
  '{}'::jsonb, 20, true, true, now(), 'migration', false
FROM config."CatalogGroups" cg
WHERE cg.slug = 'crm-account-sources' AND cg.is_deleted = false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems"
  (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
   is_system, is_active, created_at_utc, created_by, is_deleted)
SELECT gen_random_uuid(), cg.id, 'PHONE', 'phone',
  'Teléfono', 'Opción de catálogo CRM', 'Teléfono',
  '{}'::jsonb, 30, true, true, now(), 'migration', false
FROM config."CatalogGroups" cg
WHERE cg.slug = 'crm-account-sources' AND cg.is_deleted = false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems"
  (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
   is_system, is_active, created_at_utc, created_by, is_deleted)
SELECT gen_random_uuid(), cg.id, 'EMAIL', 'email',
  'Correo', 'Opción de catálogo CRM', 'Correo',
  '{}'::jsonb, 40, true, true, now(), 'migration', false
FROM config."CatalogGroups" cg
WHERE cg.slug = 'crm-account-sources' AND cg.is_deleted = false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems"
  (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
   is_system, is_active, created_at_utc, created_by, is_deleted)
SELECT gen_random_uuid(), cg.id, 'CAMPAIGN', 'campaign',
  'Campaña', 'Opción de catálogo CRM', 'Campaña',
  '{}'::jsonb, 50, true, true, now(), 'migration', false
FROM config."CatalogGroups" cg
WHERE cg.slug = 'crm-account-sources' AND cg.is_deleted = false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems"
  (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
   is_system, is_active, created_at_utc, created_by, is_deleted)
SELECT gen_random_uuid(), cg.id, 'OTHER', 'other',
  'Otro', 'Opción de catálogo CRM', 'Otro',
  '{}'::jsonb, 60, true, true, now(), 'migration', false
FROM config."CatalogGroups" cg
WHERE cg.slug = 'crm-account-sources' AND cg.is_deleted = false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogGroups"
  (id, code, slug, name, description, metadata_json, is_system, is_active,
   created_at_utc, created_by, is_deleted)
VALUES
  (gen_random_uuid(), 'CRM_RELATIONSHIP_TYPES', 'crm-relationship-types', 'Tipos de relación',
   'Catálogo comercial DholeCRM Fase 2',
   '{"crm":true,"phase":2}'::jsonb, true, true, now(), 'migration', false)
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems"
  (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
   is_system, is_active, created_at_utc, created_by, is_deleted)
SELECT gen_random_uuid(), cg.id, 'PROSPECT', 'prospect',
  'Prospecto', 'Opción de catálogo CRM', 'Prospecto',
  '{}'::jsonb, 10, true, true, now(), 'migration', false
FROM config."CatalogGroups" cg
WHERE cg.slug = 'crm-relationship-types' AND cg.is_deleted = false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems"
  (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
   is_system, is_active, created_at_utc, created_by, is_deleted)
SELECT gen_random_uuid(), cg.id, 'CUSTOMER', 'customer',
  'Cliente', 'Opción de catálogo CRM', 'Cliente',
  '{}'::jsonb, 20, true, true, now(), 'migration', false
FROM config."CatalogGroups" cg
WHERE cg.slug = 'crm-relationship-types' AND cg.is_deleted = false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems"
  (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
   is_system, is_active, created_at_utc, created_by, is_deleted)
SELECT gen_random_uuid(), cg.id, 'PARTNER', 'partner',
  'Aliado', 'Opción de catálogo CRM', 'Aliado',
  '{}'::jsonb, 30, true, true, now(), 'migration', false
FROM config."CatalogGroups" cg
WHERE cg.slug = 'crm-relationship-types' AND cg.is_deleted = false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems"
  (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
   is_system, is_active, created_at_utc, created_by, is_deleted)
SELECT gen_random_uuid(), cg.id, 'NVOCC', 'nvocc',
  'NVOCC', 'Opción de catálogo CRM', 'NVOCC',
  '{}'::jsonb, 40, true, true, now(), 'migration', false
FROM config."CatalogGroups" cg
WHERE cg.slug = 'crm-relationship-types' AND cg.is_deleted = false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems"
  (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
   is_system, is_active, created_at_utc, created_by, is_deleted)
SELECT gen_random_uuid(), cg.id, 'OTHER', 'other',
  'Otro', 'Opción de catálogo CRM', 'Otro',
  '{}'::jsonb, 50, true, true, now(), 'migration', false
FROM config."CatalogGroups" cg
WHERE cg.slug = 'crm-relationship-types' AND cg.is_deleted = false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogGroups"
  (id, code, slug, name, description, metadata_json, is_system, is_active,
   created_at_utc, created_by, is_deleted)
VALUES
  (gen_random_uuid(), 'CRM_CUSTOMER_CATEGORIES', 'crm-customer-categories', 'Categorías de clientes',
   'Catálogo comercial DholeCRM Fase 2',
   '{"crm":true,"phase":2}'::jsonb, true, true, now(), 'migration', false)
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems"
  (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
   is_system, is_active, created_at_utc, created_by, is_deleted)
SELECT gen_random_uuid(), cg.id, 'A', 'a',
  'A', 'Opción de catálogo CRM', 'A',
  '{}'::jsonb, 10, true, true, now(), 'migration', false
FROM config."CatalogGroups" cg
WHERE cg.slug = 'crm-customer-categories' AND cg.is_deleted = false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems"
  (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
   is_system, is_active, created_at_utc, created_by, is_deleted)
SELECT gen_random_uuid(), cg.id, 'B', 'b',
  'B', 'Opción de catálogo CRM', 'B',
  '{}'::jsonb, 20, true, true, now(), 'migration', false
FROM config."CatalogGroups" cg
WHERE cg.slug = 'crm-customer-categories' AND cg.is_deleted = false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems"
  (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
   is_system, is_active, created_at_utc, created_by, is_deleted)
SELECT gen_random_uuid(), cg.id, 'C', 'c',
  'C', 'Opción de catálogo CRM', 'C',
  '{}'::jsonb, 30, true, true, now(), 'migration', false
FROM config."CatalogGroups" cg
WHERE cg.slug = 'crm-customer-categories' AND cg.is_deleted = false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems"
  (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
   is_system, is_active, created_at_utc, created_by, is_deleted)
SELECT gen_random_uuid(), cg.id, 'D', 'd',
  'D', 'Opción de catálogo CRM', 'D',
  '{}'::jsonb, 40, true, true, now(), 'migration', false
FROM config."CatalogGroups" cg
WHERE cg.slug = 'crm-customer-categories' AND cg.is_deleted = false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogGroups"
  (id, code, slug, name, description, metadata_json, is_system, is_active,
   created_at_utc, created_by, is_deleted)
VALUES
  (gen_random_uuid(), 'CRM_NVOCC_CATEGORIES', 'crm-nvocc-categories', 'Categorías NVOCC',
   'Catálogo comercial DholeCRM Fase 2',
   '{"crm":true,"phase":2}'::jsonb, true, true, now(), 'migration', false)
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems"
  (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
   is_system, is_active, created_at_utc, created_by, is_deleted)
SELECT gen_random_uuid(), cg.id, 'A', 'a',
  'A', 'Opción de catálogo CRM', 'A',
  '{}'::jsonb, 10, true, true, now(), 'migration', false
FROM config."CatalogGroups" cg
WHERE cg.slug = 'crm-nvocc-categories' AND cg.is_deleted = false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems"
  (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
   is_system, is_active, created_at_utc, created_by, is_deleted)
SELECT gen_random_uuid(), cg.id, 'B', 'b',
  'B', 'Opción de catálogo CRM', 'B',
  '{}'::jsonb, 20, true, true, now(), 'migration', false
FROM config."CatalogGroups" cg
WHERE cg.slug = 'crm-nvocc-categories' AND cg.is_deleted = false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogGroups"
  (id, code, slug, name, description, metadata_json, is_system, is_active,
   created_at_utc, created_by, is_deleted)
VALUES
  (gen_random_uuid(), 'CRM_CONTACT_CHANNELS', 'crm-contact-channels', 'Canales de contacto',
   'Catálogo comercial DholeCRM Fase 2',
   '{"crm":true,"phase":2}'::jsonb, true, true, now(), 'migration', false)
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems"
  (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
   is_system, is_active, created_at_utc, created_by, is_deleted)
SELECT gen_random_uuid(), cg.id, 'EMAIL', 'email',
  'Correo electrónico', 'Opción de catálogo CRM', 'Correo electrónico',
  '{}'::jsonb, 10, true, true, now(), 'migration', false
FROM config."CatalogGroups" cg
WHERE cg.slug = 'crm-contact-channels' AND cg.is_deleted = false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems"
  (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
   is_system, is_active, created_at_utc, created_by, is_deleted)
SELECT gen_random_uuid(), cg.id, 'PHONE', 'phone',
  'Teléfono', 'Opción de catálogo CRM', 'Teléfono',
  '{}'::jsonb, 20, true, true, now(), 'migration', false
FROM config."CatalogGroups" cg
WHERE cg.slug = 'crm-contact-channels' AND cg.is_deleted = false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems"
  (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
   is_system, is_active, created_at_utc, created_by, is_deleted)
SELECT gen_random_uuid(), cg.id, 'WHATSAPP', 'whatsapp',
  'WhatsApp', 'Opción de catálogo CRM', 'WhatsApp',
  '{}'::jsonb, 30, true, true, now(), 'migration', false
FROM config."CatalogGroups" cg
WHERE cg.slug = 'crm-contact-channels' AND cg.is_deleted = false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems"
  (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
   is_system, is_active, created_at_utc, created_by, is_deleted)
SELECT gen_random_uuid(), cg.id, 'VISIT', 'visit',
  'Visita', 'Opción de catálogo CRM', 'Visita',
  '{}'::jsonb, 40, true, true, now(), 'migration', false
FROM config."CatalogGroups" cg
WHERE cg.slug = 'crm-contact-channels' AND cg.is_deleted = false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems"
  (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
   is_system, is_active, created_at_utc, created_by, is_deleted)
SELECT gen_random_uuid(), cg.id, 'OTHER', 'other',
  'Otro', 'Opción de catálogo CRM', 'Otro',
  '{}'::jsonb, 50, true, true, now(), 'migration', false
FROM config."CatalogGroups" cg
WHERE cg.slug = 'crm-contact-channels' AND cg.is_deleted = false
ON CONFLICT DO NOTHING;
""");
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new NotSupportedException("Do not delete Config catalogs: records may be shared by other services.");
    }
}
