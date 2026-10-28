
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
    private readonly ServiceBusSender _sender;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public SendOrderFunction(
        ILogger<SendOrderFunction> logger,
        ServiceBusSender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    [Function("SendOrderFunction")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")]
        HttpRequest req)
    {
        if (!req.HasJsonContentType())
        {
            return new BadRequestObjectResult(new
            {
                message = "Content-Type must be application/json."
            });
        }

        CreateOrderRequest? request;

        try
        {
            request = await JsonSerializer.DeserializeAsync<CreateOrderRequest>(
                req.Body,
                JsonOptions,
                req.HttpContext.RequestAborted);
        }
        catch (JsonException)
        {
            return new BadRequestObjectResult(new
            {
                message = "Invalid JSON request body."
            });
        }

        if (request is null)
        {
            return new BadRequestObjectResult(new
            {
                message = "Request body is required."
            });
        }

        // Create the complete order.
        Order order = CreateOrder(request);

        // Serialize the order and publish it to Service Bus.
        try
        {
            string orderJson = JsonSerializer.Serialize(order, JsonOptions);

            var message = new ServiceBusMessage(orderJson)
            {
                ContentType = "application/json",
                MessageId = order.OrderId.ToString()
            };

            await _sender.SendMessageAsync(
                message,
                req.HttpContext.RequestAborted);

            _logger.LogInformation(
                "Order {OrderId} published to Service Bus.",
                order.OrderId);

            return new ObjectResult(new
            {
                message = "Order submitted successfully.",
                data = order
            })
            {
                StatusCode = StatusCodes.Status201Created
            };
        }
        catch (OperationCanceledException)
            when (req.HttpContext.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Order submission was cancelled by the client.");

            throw;
        }
        catch (ServiceBusException ex)
        {
            _logger.LogError(
                ex,
                "Service Bus failed while publishing order {OrderId}.",
                order.OrderId);

            return new ObjectResult(new
            {
                message = "Unable to publish the order to Service Bus."
            })
            {
                StatusCode = StatusCodes.Status503ServiceUnavailable
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while processing order {OrderId}.",
                order.OrderId);

            return new ObjectResult(new
        {
                message = "An unexpected error occurred."
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
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

