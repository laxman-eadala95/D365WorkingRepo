using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace OrderMock.Functions
{
    /// <summary>
    /// Mock REST endpoint that accepts order payloads and logs them.
    /// Locally: func start -> POST http://localhost:7071/api/orders
    /// </summary>
    public class OrdersFunction
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
                body = await reader.ReadToEndAsync();
            }

            // Try to parse and log the order details
            try
            {
                using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);
                var root = doc.RootElement;

                var customer = root.TryGetProperty("CustomerName", out var cn) ? cn.ToString() : "(unknown)";
                var total = root.TryGetProperty("OrderTotal", out var ot) ? ot.ToString() : "(unknown)";

                _logger.LogInformation("Received order: CustomerName={Customer}, OrderTotal={Total}", customer, total);
            }
            catch (JsonException)
            {
                _logger.LogWarning("Could not parse order JSON. Raw length: {Length}", body?.Length ?? 0);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync("{\"status\":\"received\"}");
            return response;
        }
    }
}
