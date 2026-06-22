using System.Text.Json;
using CustomCodeFramework.Messaging.Outbox;
using Dhole.Config.Application.Abstractions.Messaging;
using Dhole.Config.Persistence.DbContexts;

namespace Dhole.Config.Persistence.Messaging;

public sealed class IntegrationEventOutboxWriter(ServiceDbContext dbContext)
    : IIntegrationEventOutboxWriter
{
    public async Task WriteAsync(
        string eventType,
        string eventName,
        object payload,
        string? correlationId = null,
        CancellationToken cancellationToken = default
    )
    {
        var message = new OutboxMessage
        {
            EventId = Guid.NewGuid(),
            EventType = eventType,
            EventName = eventName,
            SourceService = "DholeConfigService",
            PayloadJson = JsonSerializer.Serialize(payload),
            HeadersJson = null,
            CorrelationId = correlationId,
            Status = OutboxMessageStatus.Pending,
            RetryCount = 0,
        };

        await dbContext.OutboxMessages.AddAsync(message, cancellationToken);
    }
}
