/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Azure Function HTTP trigger that mocks an external order API for local OrderSync testing. Refer to following steps
**     1. Accept POST /api/orders and read JSON body
**     2. Parse CustomerName and OrderTotal for logging; respond 200 with JSON acknowledgment
*/

using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace OrderMock.Functions
{
    /// <summary>
    /// Mock REST endpoint that accepts order JSON payloads and logs key fields.
    /// </summary>
    /// <remarks>
    /// Local run: <c>func start</c> then POST to <c>http://localhost:7071/api/orders</c>.
    /// </remarks>
    public class OrdersFunction
    {
        private readonly ILogger<OrdersFunction> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrdersFunction"/> class.
        /// </summary>
        /// <param name="logger">Host-injected logger for request diagnostics.</param>
        public OrdersFunction(ILogger<OrdersFunction> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles POST <c>/api/orders</c>: reads body, best-effort parses JSON for logging, returns JSON acknowledgment.
        /// </summary>
        /// <param name="req">Incoming HTTP request (anonymous authorization).</param>
        [Function("Orders")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders")] HttpRequestData req)
        {
            string body;
            using (var reader = new StreamReader(req.Body))
            {
                body = await reader.ReadToEndAsync();
            }

            // Best-effort parse so malformed JSON still yields a 200 for simple connectivity tests.
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
