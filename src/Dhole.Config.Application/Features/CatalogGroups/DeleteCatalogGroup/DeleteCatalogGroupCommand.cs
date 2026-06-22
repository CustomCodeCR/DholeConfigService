using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Config.Application.CatalogGroups.DeleteCatalogGroup;

public sealed record DeleteCatalogGroupCommand(Guid Id, Guid? DeletedBy) : ICommand<Result>;
