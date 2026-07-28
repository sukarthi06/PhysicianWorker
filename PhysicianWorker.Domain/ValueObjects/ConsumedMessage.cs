namespace PhysicianWorker.Domain.ValueObjects;

public record ConsumedMessage(TranscriptReadyMessage Payload, ulong DeliveryTag);
