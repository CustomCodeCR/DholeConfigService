using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Config.Application.CatalogItems.SetCatalogItemActive;

public sealed record SetCatalogItemActiveCommand(Guid Id, bool IsActive, Guid? UpdatedBy)
    : ICommand<Result>;
