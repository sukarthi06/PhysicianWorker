using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
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
        })
        .AddStandardResilienceHandler(options =>
        {
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(60);
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(120);
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(120); // must be ≥ 2× AttemptTimeout
        });

        #endregion

        return services;
    }

    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var otlpEndpoint = configuration["Otlp:Endpoint"] ?? "http://localhost:4317";
        var otlpProtocol = configuration["Otlp:Protocol"] ?? "grpc";
        var otlpHeaders = configuration["Otlp:Headers"];
        var serviceName = configuration["Serilog:Properties:Application"] ?? "PhysicianWorker";
        var exportProtocol = otlpProtocol.Equals("http/protobuf", StringComparison.OrdinalIgnoreCase)
            ? OtlpExportProtocol.HttpProtobuf
            : OtlpExportProtocol.Grpc;

        var tracesEndpoint = new Uri(configuration["Otlp:TracesEndpoint"] ?? otlpEndpoint);
        var metricsEndpoint = new Uri(configuration["Otlp:MetricsEndpoint"] ?? otlpEndpoint);

        void ConfigureTraceExporter(OtlpExporterOptions otlp)
        {
            otlp.Endpoint = tracesEndpoint;
            otlp.Protocol = exportProtocol;
            if (!string.IsNullOrEmpty(otlpHeaders))
                otlp.Headers = otlpHeaders;
        }

        void ConfigureMetricExporter(OtlpExporterOptions otlp)
        {
            otlp.Endpoint = metricsEndpoint;
            otlp.Protocol = exportProtocol;
            if (!string.IsNullOrEmpty(otlpHeaders))
                otlp.Headers = otlpHeaders;
        }

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: serviceName))
            .WithTracing(tracing => tracing
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(ConfigureTraceExporter))
            .WithMetrics(metrics => metrics
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(ConfigureMetricExporter));

        return services;
    }
}
