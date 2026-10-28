
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using BenchDemoOrderFunction.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

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

        var connectionString = Environment.GetEnvironmentVariable("ServiceBusConnection");

        var client = new ServiceBusClient(connectionString,
            new ServiceBusClientOptions { TransportType = ServiceBusTransportType.AmqpWebSockets});

        var sender = client.CreateSender("orders-queue");

        var orderJson = "";
        try
        {
            using var reader = new StreamReader(req.Body);
            orderJson = await reader.ReadToEndAsync();

            var request = JsonSerializer.Deserialize<CreateOrderRequest>(orderJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (request == null)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }


            Order order = CreateOrder(request);

            string m = JsonSerializer.Serialize(order);
            var messageBody = new ServiceBusMessage(m) { ContentType = "application/json" };
            orderJson = m;

            await sender.SendMessageAsync(messageBody);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        var response = new
        {
            message = "success",
            status = 200,
            data = new
            {
                messageBody = orderJson
            }
        };

        return new OkObjectResult(response);
    }

    public static Order CreateOrder(CreateOrderRequest request)
    {
        return new Order
        {
            OrderId = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            Status = request.Status,
            OrderDate = DateTime.UtcNow,
            EmailAddress = request.EmailAddress,
            ProductPrice = request.ProductPrice
        };
    }

}

