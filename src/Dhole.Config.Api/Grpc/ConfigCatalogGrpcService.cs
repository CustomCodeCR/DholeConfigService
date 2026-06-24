using Dhole.Config.Application.Abstractions.Repositories;
using Dhole.Config.Contracts.Catalogs;
using Dhole.Config.Contracts.Grpc;
using Grpc.Core;

namespace Dhole.Config.Api.Grpc;

public sealed class ConfigCatalogGrpcService(ICatalogItemRepository catalogItemRepository)
    : ConfigCatalogGrpc.ConfigCatalogGrpcBase
{
    public override async Task<CatalogItemGrpcResponse> GetCatalogItemById(
        GetCatalogItemByIdGrpcRequest request,
        ServerCallContext context
    )
    {
        if (!Guid.TryParse(request.CatalogItemId, out var catalogItemId))
        {
            return new CatalogItemGrpcResponse { Found = false };
        }

        var item = await catalogItemRepository.GetLookupByIdAsync(
            catalogItemId,
            context.CancellationToken
        );

        return ToResponse(item);
    }

    public override async Task<CatalogItemGrpcResponse> GetCatalogItemBySlug(
        GetCatalogItemBySlugGrpcRequest request,
        ServerCallContext context
    )
    {
        var item = await catalogItemRepository.GetLookupAsync(
            request.CatalogGroupSlug,
            request.CatalogItemSlug,
            context.CancellationToken
        );

        return ToResponse(item);
    }

    public override async Task<CatalogItemGrpcResponse> GetCatalogItemByCode(
        GetCatalogItemByCodeGrpcRequest request,
        ServerCallContext context
    )
    {
        var item = await catalogItemRepository.GetLookupByCodeAsync(
            request.CatalogGroupSlug,
            request.CatalogItemCode,
            context.CancellationToken
        );

        return ToResponse(item);
    }

    public override async Task<CatalogItemGrpcResponse> ResolveCatalogItem(
        ResolveCatalogItemGrpcRequest request,
        ServerCallContext context
    )
    {
        var item = await catalogItemRepository.ResolveLookupAsync(
            request.CatalogGroupSlug,
            request.Value,
            context.CancellationToken
        );

        return ToResponse(item);
    }

    public override async Task<ValidateCatalogItemGrpcResponse> ValidateCatalogItem(
        ValidateCatalogItemGrpcRequest request,
        ServerCallContext context
    )
    {
        var item = await catalogItemRepository.GetLookupAsync(
            request.CatalogGroupSlug,
            request.CatalogItemSlug,
            context.CancellationToken
        );

        return new ValidateCatalogItemGrpcResponse
        {
            IsValid = item?.IsActive == true,
            CatalogGroupSlug = request.CatalogGroupSlug,
            CatalogItemSlug = request.CatalogItemSlug,
            Item = item is null ? null : ToModel(item)
        };
    }

    public override async Task<CatalogItemListGrpcResponse> GetActiveCatalogItemsByGroup(
        GetActiveCatalogItemsByGroupGrpcRequest request,
        ServerCallContext context
    )
    {
        var items = await catalogItemRepository.GetActiveLookupsByGroupSlugAsync(
            request.CatalogGroupSlug,
            context.CancellationToken
        );

        var response = new CatalogItemListGrpcResponse();
        response.Items.AddRange(items.Select(ToModel));

        return response;
    }

    private static CatalogItemGrpcResponse ToResponse(CatalogItemLookupDto? item)
    {
        if (item is null)
        {
            return new CatalogItemGrpcResponse { Found = false };
        }

        return new CatalogItemGrpcResponse
        {
            Found = true,
            Item = ToModel(item)
        };
    }

    private static CatalogItemGrpcModel ToModel(CatalogItemLookupDto item)
    {
        return new CatalogItemGrpcModel
        {
            Id = item.Id.ToString(),
            CatalogGroupId = item.CatalogGroupId.ToString(),
            CatalogGroupCode = item.CatalogGroupCode,
            CatalogGroupSlug = item.CatalogGroupSlug,
            Code = item.Code,
            Slug = item.Slug,
            Name = item.Name,
            Value = item.Value ?? string.Empty,
            MetadataJson = item.MetadataJson ?? string.Empty,
            IsActive = item.IsActive
        };
    }
}
