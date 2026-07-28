using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhysicianWorker.Application.UseCases;
using PhysicianWorker.Domain.Configs;
using PhysicianWorker.Infra.Services;

namespace PhysicianWorker.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        #region RabbitMq

        services.AddSingleton<IMessageConsumer>(sp =>
            RabbitMqConsumer.CreateAsync(
                sp.GetRequiredService<IConfiguration>(),
                sp.GetRequiredService<ILogger<RabbitMqConsumer>>()
            ).GetAwaiter().GetResult());

        #endregion

        #region HttpClients

        services.Configure<PhysicianNoteServiceOption>(
            configuration.GetSection(PhysicianNoteServiceOption.SectionName));

        services.AddHttpClient<IPhysicianNoteService, PhysicianNoteService>((sp, client) =>
        {
            var options = sp
                .GetRequiredService<IOptions<PhysicianNoteServiceOption>>()
                .Value;

            client.BaseAddress = new Uri(options.Address!);            
        });

        #endregion

            return services;
    }
}
