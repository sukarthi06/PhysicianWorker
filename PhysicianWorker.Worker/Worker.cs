using PhysicianWorker.Application.UseCases;

namespace PhysicianWorker.Worker
{
    public class Worker(
        IMessageConsumer messageConsumer,
        IPhysicianNoteService physicianNoteService,
        ILogger<Worker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            messageConsumer.MessageReceived += async consumed =>
            {
                try
                {
                    logger.LogInformation("Started processing message for RecordingId: {RecordingId}", consumed.Payload.RecordingId);
                    var result = await physicianNoteService.GenerateNotesAsync(consumed.Payload.RecordingId, stoppingToken);
                    if (result)
                    {
                        logger.LogInformation("Physician note generated for RecordingId: {RecordingId}",
                            consumed.Payload.RecordingId);
                        await messageConsumer.AcknowledgeAsync(consumed.DeliveryTag, stoppingToken);
                    }                        
                    else
                    {
                        logger.LogWarning("Physician note generation failed for RecordingId: {RecordingId}",
                            consumed.Payload.RecordingId);
                        await messageConsumer.RejectAsync(consumed.DeliveryTag, stoppingToken);
                    }                        
                }
                catch
                {
                    await messageConsumer.RejectAsync(consumed.DeliveryTag, stoppingToken);
                    logger.LogError("Failed to process message with RecordingId: {RecordingId} delivery tag {DeliveryTag}",
                        consumed.Payload.RecordingId, consumed.DeliveryTag);
                }
            };
            await messageConsumer.StartConsumingAsync(stoppingToken);
            //var recordingID = Guid.Parse("6f4bb276-73b4-4572-9f05-5edb28960ae5");
            //var result = await physicianNoteService.GenerateNotesAsync(recordingID, stoppingToken);
        }
    }
}
