namespace PhysicianWorker.Domain.ValueObjects;

public record TranscriptReadyMessage(
    Guid RecordingId,
    DateTimeOffset CompletedAt);
