
using Azure.Messaging.ServiceBus;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

if (!string.IsNullOrEmpty(
    Environment.GetEnvironmentVariable(
        "APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

var connectionString =
    Environment.GetEnvironmentVariable("ServiceBusConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ServiceBusConnection is not configured.");
}

builder.Services.AddSingleton(_ =>
    new ServiceBusClient(
        connectionString,
        new ServiceBusClientOptions
        {
            TransportType = ServiceBusTransportType.AmqpWebSockets
        }));

// Register the sender for your existing orders-queue.
builder.Services.AddSingleton(serviceProvider =>
    serviceProvider
        .GetRequiredService<ServiceBusClient>()
        .CreateSender("orders-queue"));

builder.Build().Run();
