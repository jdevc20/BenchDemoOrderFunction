using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Xml.Serialization;
using Azure.Messaging.ServiceBus;
using Azure.Identity;

namespace BenchDemoOrderFunction;

public class SendOrderFunction
{
    private readonly ILogger<SendOrderFunction> _logger;

    public SendOrderFunction(ILogger<SendOrderFunction> logger)
    {
        _logger = logger;
    }

    [Function("SendOrderFunction")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        //_logger.LogInformation("C# HTTP trigger function processed a request.");
        var namespaceName = Environment.GetEnvironmentVariable("ServiceBusNamespace");

        if (string.IsNullOrEmpty(namespaceName))
        {
            _logger.LogError("ServiceBusNamespace environment variable is not set.");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        var connectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");

        var client = new ServiceBusClient(connectionString,
            new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets});

        var sender = client.CreateSender("orders-queue");

        try
        {
            using var reader = new StreamReader(req.Body);
            var orderJson = await reader.ReadToEndAsync();
            var message = new ServiceBusMessage(orderJson);
            await sender.SendMessageAsync(message);
        }
        catch(Exception e)
        {
            _logger.LogError(e.Message);
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        return new OkResult();
    }
}