using CustomCodeFramework.Api.Responses;
using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Config.Api.Authorization;
using Dhole.Config.Api.Extensions;
using Dhole.Config.Application.CatalogGroups.CreateCatalogGroup;
using Dhole.Config.Application.CatalogGroups.DeleteCatalogGroup;
using Dhole.Config.Application.CatalogGroups.GetCatalogGroupById;
using Dhole.Config.Application.CatalogGroups.GetCatalogGroups;
using Dhole.Config.Application.CatalogGroups.GetCatalogGroupsForSelect;
using Dhole.Config.Application.CatalogGroups.SetCatalogGroupActive;
using Dhole.Config.Application.CatalogGroups.UpdateCatalogGroup;
using Dhole.Config.Contracts.Catalogs;

namespace Dhole.Config.Api.Endpoints;

public static class CatalogGroupEndpoints
{
    public static IEndpointRouteBuilder MapCatalogGroupEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/config/catalog-groups")
            .WithTags("Catalog Groups")
            .RequireAuthorization();

        group
            .MapGet(
                "/",
                async (
                    int pageNumber,
                    int pageSize,
                    string? search,
                    bool? isActive,
                    IQueryDispatcher dispatcher,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = await dispatcher.DispatchAsync(
                        new GetCatalogGroupsQuery(
                            PageRequest.Create(pageNumber, pageSize),
                            search,
                            isActive
                        ),
                        cancellationToken
                    );

                    return EndpointResults.FromPaged(result);
                }
            )
            .RequireScope(ConfigScopeNames.CatalogGroupsView);

        group
            .MapGet(
                "/select",
                async (
                    string? search,
                    IQueryDispatcher dispatcher,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = await dispatcher.DispatchAsync(
                        new GetCatalogGroupsForSelectQuery(search),
                        cancellationToken
                    );

                    return Results.Ok(
                        ApiResponse<IReadOnlyCollection<CatalogGroupSelectDto>>.Ok(result)
                    );
                }
            )
            .RequireScope(ConfigScopeNames.CatalogSelectsView);

        group
            .MapGet(
                "/{catalogGroupId:guid}",
                async (
                    Guid catalogGroupId,
                    IQueryDispatcher dispatcher,
                    HttpContext httpContext,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = await dispatcher.DispatchAsync(
                        new GetCatalogGroupByIdQuery(catalogGroupId),
                        cancellationToken
                    );

                    return EndpointResults.FromResult(result, httpContext);
                }
            )
            .RequireScope(ConfigScopeNames.CatalogGroupsView);

        group
            .MapPost(
                "/",
                async (
                    CreateCatalogGroupRequest request,
                    ICommandDispatcher dispatcher,
                    HttpContext httpContext,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = await dispatcher.DispatchAsync(
                        new CreateCatalogGroupCommand(
                            request.Name,
                            request.Slug,
                            request.Description,
                            request.MetadataJson,
                            request.IsSystem,
                            httpContext.GetCurrentUserId()
                        ),
                        cancellationToken
                    );

                    return EndpointResults.FromResult(result, httpContext);
                }
            )
            .RequireScope(ConfigScopeNames.CatalogGroupsCreate);

        group
            .MapPut(
                "/{catalogGroupId:guid}",
                async (
                    Guid catalogGroupId,
                    UpdateCatalogGroupRequest request,
                    ICommandDispatcher dispatcher,
                    HttpContext httpContext,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = await dispatcher.DispatchAsync(
                        new UpdateCatalogGroupCommand(
                            catalogGroupId,
                            request.Name,
                            request.Description,
                            request.MetadataJson,
                            httpContext.GetCurrentUserId()
                        ),
                        cancellationToken
                    );

                    return EndpointResults.FromResult(result, httpContext);
                }
            )
            .RequireScope(ConfigScopeNames.CatalogGroupsUpdate);

        group
            .MapPatch(
                "/{catalogGroupId:guid}/active",
                async (
                    Guid catalogGroupId,
                    SetCatalogGroupActiveRequest request,
                    ICommandDispatcher dispatcher,
                    HttpContext httpContext,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = await dispatcher.DispatchAsync(
                        new SetCatalogGroupActiveCommand(
                            catalogGroupId,
                            request.IsActive,
                            httpContext.GetCurrentUserId()
                        ),
                        cancellationToken
                    );

                    return EndpointResults.FromResult(result, httpContext);
                }
            )
            .RequireScope(ConfigScopeNames.CatalogGroupsSetActive);

        group
            .MapDelete(
                "/{catalogGroupId:guid}",
                async (
                    Guid catalogGroupId,
                    ICommandDispatcher dispatcher,
                    HttpContext httpContext,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = await dispatcher.DispatchAsync(
                        new DeleteCatalogGroupCommand(
                            catalogGroupId,
                            httpContext.GetCurrentUserId()
                        ),
                        cancellationToken
                    );

                    return EndpointResults.FromResult(result, httpContext);
                }
            )
            .RequireScope(ConfigScopeNames.CatalogGroupsDelete);

        return app;
    }

    private sealed record SetCatalogGroupActiveRequest(bool IsActive);
}
