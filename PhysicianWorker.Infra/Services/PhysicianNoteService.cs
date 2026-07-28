using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhysicianWorker.Application.UseCases;
using PhysicianWorker.Domain.Configs;
using System.Net.Http.Json;

namespace PhysicianWorker.Infra.Services;

internal class PhysicianNoteService(
    HttpClient httpClient,
    IOptions<PhysicianNoteServiceOption> options,
    ILogger<PhysicianNoteService> logger) : IPhysicianNoteService
{
    public async Task<bool> GenerateNotesAsync(Guid recordingId, CancellationToken ct)
    {
        //var payload = new { recordingId = recordingId };
        var content = JsonContent.Create(recordingId);
        try
        {
            var response = await httpClient.PostAsync(
            requestUri: options.Value.Address + "/api/physiciannoteservice/physician-note",
            content: content,
            cancellationToken: ct);
            logger.LogInformation("Physician Note Service Status: {StatusCode}", response.StatusCode);
            response.EnsureSuccessStatusCode();

            bool result = await response.Content.ReadFromJsonAsync<bool>();
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Physician note generation failed for RecordingId: {RecordingId}", recordingId);
            return false;
        }
    }
}
