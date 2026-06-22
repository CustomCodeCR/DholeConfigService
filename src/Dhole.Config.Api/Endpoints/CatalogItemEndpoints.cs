using CustomCodeFramework.Api.Responses;
using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Config.Api.Authorization;
using Dhole.Config.Api.Extensions;
using Dhole.Config.Application.CatalogItems.ChangeCatalogItemSortOrder;
using Dhole.Config.Application.CatalogItems.CreateCatalogItem;
using Dhole.Config.Application.CatalogItems.DeleteCatalogItem;
using Dhole.Config.Application.CatalogItems.GetCatalogItemById;
using Dhole.Config.Application.CatalogItems.GetCatalogItems;
using Dhole.Config.Application.CatalogItems.GetCatalogItemsByGroupSlug;
using Dhole.Config.Application.CatalogItems.GetCatalogItemsForSelect;
using Dhole.Config.Application.CatalogItems.SetCatalogItemActive;
using Dhole.Config.Application.CatalogItems.UpdateCatalogItem;
using Dhole.Config.Application.CatalogItems.ValidateCatalogItem;
using Dhole.Config.Contracts.Catalogs;

namespace Dhole.Config.Api.Endpoints;

public static class CatalogItemEndpoints
{
    public static IEndpointRouteBuilder MapCatalogItemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/config/catalog-items")
            .WithTags("Catalog Items")
            .RequireAuthorization();

        group
            .MapGet(
                "/",
                async (
                    int pageNumber,
                    int pageSize,
                    Guid? catalogGroupId,
                    string? search,
                    bool? isActive,
                    IQueryDispatcher dispatcher,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = await dispatcher.DispatchAsync(
                        new GetCatalogItemsQuery(
                            PageRequest.Create(pageNumber, pageSize),
                            catalogGroupId,
                            search,
                            isActive
                        ),
                        cancellationToken
                    );

                    return EndpointResults.FromPaged(result);
                }
            )
            .RequireScope(ConfigScopeNames.CatalogItemsView);

        group
            .MapGet(
                "/select",
                async (
                    Guid? catalogGroupId,
                    string? catalogGroupSlug,
                    string? search,
                    IQueryDispatcher dispatcher,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = await dispatcher.DispatchAsync(
                        new GetCatalogItemsForSelectQuery(catalogGroupId, catalogGroupSlug, search),
                        cancellationToken
                    );

                    return Results.Ok(
                        ApiResponse<IReadOnlyCollection<CatalogItemSelectDto>>.Ok(result)
                    );
                }
            )
            .RequireScope(ConfigScopeNames.CatalogSelectsView);

        group
            .MapGet(
                "/by-group/{catalogGroupSlug}",
                async (
                    string catalogGroupSlug,
                    IQueryDispatcher dispatcher,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = await dispatcher.DispatchAsync(
                        new GetCatalogItemsByGroupSlugQuery(catalogGroupSlug),
                        cancellationToken
                    );

                    return Results.Ok(ApiResponse<IReadOnlyCollection<CatalogItemDto>>.Ok(result));
                }
            )
            .RequireScope(ConfigScopeNames.CatalogItemsView);

        group
            .MapGet(
                "/{catalogItemId:guid}",
                async (
                    Guid catalogItemId,
                    IQueryDispatcher dispatcher,
                    HttpContext httpContext,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = await dispatcher.DispatchAsync(
                        new GetCatalogItemByIdQuery(catalogItemId),
                        cancellationToken
                    );

                    return EndpointResults.FromResult(result, httpContext);
                }
            )
            .RequireScope(ConfigScopeNames.CatalogItemsView);

        group
            .MapGet(
                "/validate",
                async (
                    string catalogGroupSlug,
                    string catalogItemSlug,
                    IQueryDispatcher dispatcher,
                    HttpContext httpContext,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = await dispatcher.DispatchAsync(
                        new ValidateCatalogItemQuery(catalogGroupSlug, catalogItemSlug),
                        cancellationToken
                    );

                    return EndpointResults.FromResult(result, httpContext);
                }
            )
            .RequireScope(ConfigScopeNames.CatalogItemsValidate);

        group
            .MapPost(
                "/",
                async (
                    CreateCatalogItemRequest request,
                    ICommandDispatcher dispatcher,
                    HttpContext httpContext,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = await dispatcher.DispatchAsync(
                        new CreateCatalogItemCommand(
                            request.CatalogGroupId,
                            request.Name,
                            request.Slug,
                            request.Description,
                            request.Value,
                            request.MetadataJson,
                            request.SortOrder,
                            request.IsSystem,
                            httpContext.GetCurrentUserId()
                        ),
                        cancellationToken
                    );

                    return EndpointResults.FromResult(result, httpContext);
                }
            )
            .RequireScope(ConfigScopeNames.CatalogItemsCreate);

        group
            .MapPost(
                "/catalog-groups/{catalogGroupId:guid}",
                async (
                    Guid catalogGroupId,
                    CreateCatalogItemForGroupRequest request,
                    ICommandDispatcher dispatcher,
                    HttpContext httpContext,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = await dispatcher.DispatchAsync(
                        new CreateCatalogItemCommand(
                            catalogGroupId,
                            request.Name,
                            request.Slug,
                            request.Description,
                            request.Value,
                            request.MetadataJson,
                            request.SortOrder,
                            request.IsSystem,
                            httpContext.GetCurrentUserId()
                        ),
                        cancellationToken
                    );

                    return EndpointResults.FromResult(result, httpContext);
                }
            )
            .RequireScope(ConfigScopeNames.CatalogItemsCreate);

        group
            .MapPut(
                "/{catalogItemId:guid}",
                async (
                    Guid catalogItemId,
                    UpdateCatalogItemRequest request,
                    ICommandDispatcher dispatcher,
                    HttpContext httpContext,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = await dispatcher.DispatchAsync(
                        new UpdateCatalogItemCommand(
                            catalogItemId,
                            request.Name,
                            request.Description,
                            request.Value,
                            request.MetadataJson,
                            request.SortOrder,
                            httpContext.GetCurrentUserId()
                        ),
                        cancellationToken
                    );

                    return EndpointResults.FromResult(result, httpContext);
                }
            )
            .RequireScope(ConfigScopeNames.CatalogItemsUpdate);

        group
            .MapPatch(
                "/{catalogItemId:guid}/active",
                async (
                    Guid catalogItemId,
                    SetCatalogItemActiveRequest request,
                    ICommandDispatcher dispatcher,
                    HttpContext httpContext,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = await dispatcher.DispatchAsync(
                        new SetCatalogItemActiveCommand(
                            catalogItemId,
                            request.IsActive,
                            httpContext.GetCurrentUserId()
                        ),
                        cancellationToken
                    );

                    return EndpointResults.FromResult(result, httpContext);
                }
            )
            .RequireScope(ConfigScopeNames.CatalogItemsSetActive);

        group
            .MapPatch(
                "/{catalogItemId:guid}/sort-order",
                async (
                    Guid catalogItemId,
                    ChangeCatalogItemSortOrderRequest request,
                    ICommandDispatcher dispatcher,
                    HttpContext httpContext,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = await dispatcher.DispatchAsync(
                        new ChangeCatalogItemSortOrderCommand(
                            catalogItemId,
                            request.SortOrder,
                            httpContext.GetCurrentUserId()
                        ),
                        cancellationToken
                    );

                    return EndpointResults.FromResult(result, httpContext);
                }
            )
            .RequireScope(ConfigScopeNames.CatalogItemsChangeSortOrder);

        group
            .MapDelete(
                "/{catalogItemId:guid}",
                async (
                    Guid catalogItemId,
                    ICommandDispatcher dispatcher,
                    HttpContext httpContext,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = await dispatcher.DispatchAsync(
                        new DeleteCatalogItemCommand(catalogItemId, httpContext.GetCurrentUserId()),
                        cancellationToken
                    );

                    return EndpointResults.FromResult(result, httpContext);
                }
            )
            .RequireScope(ConfigScopeNames.CatalogItemsDelete);

        return app;
    }

    private sealed record SetCatalogItemActiveRequest(bool IsActive);

    private sealed record CreateCatalogItemForGroupRequest(
        string Name,
        string? Slug,
        string? Description,
        string? Value,
        string? MetadataJson,
        int SortOrder,
        bool IsSystem
    );
}
