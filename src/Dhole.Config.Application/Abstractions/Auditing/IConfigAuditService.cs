namespace Dhole.Config.Application.Abstractions.Auditing;

public interface IConfigAuditService
{
    Task PublishAsync(ConfigAuditEvent auditEvent, CancellationToken cancellationToken = default);
}
