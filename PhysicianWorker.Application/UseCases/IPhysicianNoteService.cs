namespace PhysicianWorker.Application.UseCases;

public interface IPhysicianNoteService
{
    Task<bool> GenerateNotesAsync(Guid RecordingId, CancellationToken ct);
}
