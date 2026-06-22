using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Config.Application.CatalogGroups.SetCatalogGroupActive;

public sealed record SetCatalogGroupActiveCommand(Guid Id, bool IsActive, Guid? UpdatedBy)
    : ICommand<Result>;
