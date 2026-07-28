using PhysicianWorker.Infra;
using PhysicianWorker.Worker;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

#region Serilog
builder.Services.AddSerilog((services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});
#endregion

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
