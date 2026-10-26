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
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Received your order! Thank you for shopping with us.");
    }
}