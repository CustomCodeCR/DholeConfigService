using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Config.Application.CatalogItems.ChangeCatalogItemSortOrder;

public sealed record ChangeCatalogItemSortOrderCommand(Guid Id, int SortOrder, Guid? UpdatedBy)
    : ICommand<Result>;
