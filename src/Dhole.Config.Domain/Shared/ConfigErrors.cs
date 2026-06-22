using CustomCodeFramework.Core.Results;

namespace Dhole.Config.Domain.Shared;

public static class ConfigErrors
{
    public static readonly Error CatalogGroupNameRequired = new(
        "Config.CatalogGroupNameRequired",
        "El nombre del catálogo es obligatorio."
    );

    public static readonly Error CatalogGroupCodeRequired = new(
        "Config.CatalogGroupCodeRequired",
        "El código del catálogo es obligatorio."
    );

    public static readonly Error CatalogGroupSlugRequired = new(
        "Config.CatalogGroupSlugRequired",
        "El slug del catálogo es obligatorio."
    );

    public static readonly Error CatalogGroupNotFound = new(
        "Config.CatalogGroupNotFound",
        "No se encontró el catálogo solicitado."
    );

    public static readonly Error CatalogGroupInactive = new(
        "Config.CatalogGroupInactive",
        "El catálogo se encuentra inactivo."
    );

    public static readonly Error CatalogGroupCodeAlreadyExists = new(
        "Config.CatalogGroupCodeAlreadyExists",
        "Ya existe un catálogo con el mismo código."
    );

    public static readonly Error CatalogGroupSlugAlreadyExists = new(
        "Config.CatalogGroupSlugAlreadyExists",
        "Ya existe un catálogo con el mismo slug."
    );

    public static readonly Error CatalogGroupNameAlreadyExists = new(
        "Config.CatalogGroupNameAlreadyExists",
        "Ya existe un catálogo con el mismo nombre."
    );

    public static readonly Error SystemCatalogGroupCannotBeDeleted = new(
        "Config.SystemCatalogGroupCannotBeDeleted",
        "Los catálogos del sistema no pueden eliminarse."
    );

    public static readonly Error CatalogGroupHasActiveItems = new(
        "Config.CatalogGroupHasActiveItems",
        "No es posible eliminar el catálogo porque tiene items activos."
    );

    public static readonly Error CatalogItemNameRequired = new(
        "Config.CatalogItemNameRequired",
        "El nombre del item es obligatorio."
    );

    public static readonly Error CatalogItemCodeRequired = new(
        "Config.CatalogItemCodeRequired",
        "El código del item es obligatorio."
    );

    public static readonly Error CatalogItemSlugRequired = new(
        "Config.CatalogItemSlugRequired",
        "El slug del item es obligatorio."
    );

    public static readonly Error CatalogItemNotFound = new(
        "Config.CatalogItemNotFound",
        "No se encontró el item solicitado."
    );

    public static readonly Error CatalogItemInactive = new(
        "Config.CatalogItemInactive",
        "El item del catálogo se encuentra inactivo."
    );

    public static readonly Error CatalogItemCodeAlreadyExists = new(
        "Config.CatalogItemCodeAlreadyExists",
        "Ya existe un item con el mismo código dentro del catálogo."
    );

    public static readonly Error CatalogItemSlugAlreadyExists = new(
        "Config.CatalogItemSlugAlreadyExists",
        "Ya existe un item con el mismo slug dentro del catálogo."
    );

    public static readonly Error CatalogItemNameAlreadyExists = new(
        "Config.CatalogItemNameAlreadyExists",
        "Ya existe un item con el mismo nombre dentro del catálogo."
    );

    public static readonly Error SystemCatalogItemCannotBeDeleted = new(
        "Config.SystemCatalogItemCannotBeDeleted",
        "Los items del sistema no pueden eliminarse."
    );

    public static readonly Error InvalidCatalogGroupSlug = new(
        "Config.InvalidCatalogGroupSlug",
        "El slug del catálogo no es válido."
    );

    public static readonly Error InvalidCatalogItemSlug = new(
        "Config.InvalidCatalogItemSlug",
        "El slug del item no es válido."
    );

    public static readonly Error InvalidMetadataJson = new(
        "Config.InvalidMetadataJson",
        "La metadata debe tener formato JSON válido."
    );

    public static readonly Error InvalidSortOrder = new(
        "Config.InvalidSortOrder",
        "El orden del item no es válido."
    );

    public static readonly Error CatalogItemDoesNotBelongToCatalogGroup = new(
        "Config.CatalogItemDoesNotBelongToCatalogGroup",
        "El item no pertenece al catálogo indicado."
    );

    public static readonly Error CatalogItemValidationFailed = new(
        "Config.CatalogItemValidationFailed",
        "El valor seleccionado no existe o no está activo en el catálogo indicado."
    );
}
