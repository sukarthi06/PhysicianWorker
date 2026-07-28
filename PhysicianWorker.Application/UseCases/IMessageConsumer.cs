using PhysicianWorker.Domain.ValueObjects;

namespace PhysicianWorker.Application.UseCases;

public interface IMessageConsumer
{
    event Func<ConsumedMessage, Task>? MessageReceived;
    Task StartConsumingAsync(CancellationToken ct);
    Task AcknowledgeAsync(ulong deliveryTag, CancellationToken ct);
    Task RejectAsync(ulong deliveryTag, CancellationToken ct);
}
