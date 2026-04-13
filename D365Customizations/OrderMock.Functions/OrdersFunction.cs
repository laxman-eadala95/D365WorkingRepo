using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace OrderMock.Functions;

/// <summary>
/// HTTP mock for D365 order sync: accepts the same JSON shape as <c>D365.Integration.OrderSync</c> POSTs.
/// Deploy to Azure Functions; locally: <c>func start</c> → typically <c>http://localhost:7071/api/orders</c>.
/// </summary>
public sealed class OrdersFunction
{
    private readonly ILogger<OrdersFunction> _logger;

    public OrdersFunction(ILogger<OrdersFunction> logger)
    {
        _logger = logger;
    }

    [Function("Orders")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders")] HttpRequestData req)
    {
        string body;
        using (var reader = new StreamReader(req.Body))
        {
            body = await reader.ReadToEndAsync().ConfigureAwait(false);
        }

        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);
            var root = doc.RootElement;
            var customer = root.TryGetProperty("CustomerName", out var cn) ? cn.ToString() : null;
            var total = root.TryGetProperty("OrderTotal", out var ot) ? ot.ToString() : null;
            _logger.LogInformation(
                "Mock order API received payload. CustomerName={CustomerName} OrderTotal={OrderTotal}",
                customer,
                total);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Could not parse JSON body; logging raw length {Length}", body?.Length ?? 0);
        }

        var ok = req.CreateResponse(HttpStatusCode.OK);
        ok.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await using (var writer = new StreamWriter(ok.Body, leaveOpen: true))
        {
            await writer.WriteAsync("{\"status\":\"received\"}").ConfigureAwait(false);
        }

        return ok;
    }
}
