using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Config.Application.CatalogItems.DeleteCatalogItem;

public sealed record DeleteCatalogItemCommand(Guid Id, Guid? DeletedBy) : ICommand<Result>;
