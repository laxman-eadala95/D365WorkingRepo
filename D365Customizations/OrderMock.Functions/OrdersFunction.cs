/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Azure Function HTTP trigger that mocks an external order API for local OrderSync testing.
**     Deserializes CRM plugin execution context JSON, extracts order details, and returns acknowledgment.
*/

using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace OrderMock.Functions
{
    /// <summary>
    /// Mock REST endpoint that accepts CRM order JSON payloads, extracts order details, logs them,
    /// and returns HTTP 200 acknowledgment with the extracted order number.
    /// </summary>
    /// <remarks>
    /// Local run: <c>func start</c> then POST to <c>http://localhost:7071/api/orders</c>.
    /// </remarks>
    public class OrdersFunction
    {
        private readonly ILogger<OrdersFunction> _logger;

        /// <summary>
        /// Initializes a new instance of the OrdersFunction class.
        /// </summary>
        /// <param name="logger">Host-injected logger for request diagnostics.</param>
        public OrdersFunction(ILogger<OrdersFunction> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles POST /api/orders: deserializes CRM payload, extracts order details, returns acknowledgment.
        /// </summary>
        /// <param name="req">Incoming HTTP request (anonymous authorization).</param>
        /// <returns>HTTP 200 response with status and extracted order number.</returns>
        [Function("Orders")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders")] HttpRequestData req)
        {
            string body;
            using (var reader = new StreamReader(req.Body))
            {
                body = await reader.ReadToEndAsync();
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var order = new ParsedOrder
            {
                OrderNumber = "(unknown)",
                PotentialCustomer = "(unknown)",
                TotalAmount = "(unknown)",
                OrderDate = "(unknown)"
            };

            try
            {
                var requestModel = JsonSerializer.Deserialize<CrmRequest>(string.IsNullOrWhiteSpace(body) ? "{}" : body, options);
                var entity = GetTargetEntity(requestModel, options);

                if (entity != null)
                {
                    // Fallback chain: ordernumber -> salesorderid -> entity GUID
                    order.OrderNumber = GetAttributeString(entity, "ordernumber")
                        ?? GetAttributeString(entity, "salesorderid")
                        ?? entity.Id
                        ?? order.OrderNumber;

                    order.PotentialCustomer = GetEntityReferenceName(entity, "customerid")
                        ?? GetEntityReferenceId(entity, "customerid")
                        ?? order.PotentialCustomer;

                    order.TotalAmount = GetMoneyValue(entity, "totalamount")
                        ?? order.TotalAmount;

                    order.OrderDate = GetDateTimeValue(entity, "createdon")
                        ?? order.OrderDate;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning("Could not parse order JSON. Length: {Length}, Error: {Error}", body?.Length ?? 0, ex.Message);
            }

            _logger.LogInformation(
                "Received order: OrderNumber={OrderNumber}, PotentialCustomer={Customer}, OrderDate={Date}",
                order.OrderNumber, order.PotentialCustomer, order.OrderDate);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            var responseBody = JsonSerializer.Serialize(new { status = "received", orderNumber = order.OrderNumber });
            await response.WriteStringAsync(responseBody);
            return response;
        }

        /// <summary>
        /// Locates the Target entity from either the direct Target property or InputParameters array.
        /// </summary>
        private static CrmEntity? GetTargetEntity(CrmRequest? request, JsonSerializerOptions options)
        {
            if (request?.Target != null)
            {
                return request.Target;
            }

            if (request?.InputParameters == null)
            {
                return null;
            }

            foreach (var parameter in request.InputParameters)
            {
                if (string.Equals(parameter.Key, "Target", StringComparison.OrdinalIgnoreCase)
                    && parameter.Value.ValueKind == JsonValueKind.Object)
                {
                    return JsonSerializer.Deserialize<CrmEntity>(parameter.Value.GetRawText(), options);
                }
            }

            return null;
        }

        /// <summary>
        /// Finds an attribute by logical name (case-insensitive) in the entity's Attributes array.
        /// </summary>
        private static CrmAttribute? FindAttribute(CrmEntity entity, string logicalName)
        {
            if (entity.Attributes == null)
            {
                return null;
            }

            foreach (var attribute in entity.Attributes)
            {
                if (string.Equals(attribute.key, logicalName, StringComparison.OrdinalIgnoreCase))
                {
                    return attribute;
                }
            }

            return null;
        }

        /// <summary>
        /// Extracts a string value from the attribute, unwrapping Money.Value and EntityReference as needed.
        /// </summary>
        private static string? GetAttributeString(CrmEntity entity, string logicalName)
        {
            var attribute = FindAttribute(entity, logicalName);
            if (attribute == null)
            {
                return null;
            }

            return attribute.value.ValueKind switch
            {
                JsonValueKind.String => attribute.value.GetString(),
                JsonValueKind.Number => attribute.value.ToString(),
                JsonValueKind.Object when attribute.value.TryGetProperty("Value", out var objectValue) => objectValue.ToString(),
                JsonValueKind.Object when attribute.value.TryGetProperty("Id", out var idValue) => idValue.ToString(),
                JsonValueKind.Object when attribute.value.TryGetProperty("Name", out var nameValue) && nameValue.ValueKind == JsonValueKind.String => nameValue.GetString(),
                _ => attribute.value.ToString()
            };
        }

        /// <summary>
        /// Returns the Name from an EntityReference attribute, falling back to generic string extraction.
        /// </summary>
        private static string? GetEntityReferenceName(CrmEntity entity, string logicalName)
        {
            var attribute = FindAttribute(entity, logicalName);
            if (attribute == null)
            {
                return null;
            }

            if (attribute.value.ValueKind == JsonValueKind.Object
                && attribute.value.TryGetProperty("Name", out var nameValue)
                && nameValue.ValueKind == JsonValueKind.String)
            {
                return nameValue.GetString();
            }

            return GetAttributeString(entity, logicalName);
        }

        /// <summary>
        /// Returns the Id (GUID) from an EntityReference attribute when Name is not available.
        /// </summary>
        private static string? GetEntityReferenceId(CrmEntity entity, string logicalName)
        {
            var attribute = FindAttribute(entity, logicalName);
            if (attribute == null)
            {
                return null;
            }

            if (attribute.value.ValueKind == JsonValueKind.Object
                && attribute.value.TryGetProperty("Id", out var idValue))
            {
                return idValue.ValueKind == JsonValueKind.String ? idValue.GetString() : idValue.ToString();
            }

            return attribute.value.ValueKind == JsonValueKind.String ? attribute.value.GetString() : null;
        }

        /// <summary>
        /// Extracts the numeric value from a CRM Money attribute.
        /// </summary>
        private static string? GetMoneyValue(CrmEntity entity, string logicalName)
        {
            var attribute = FindAttribute(entity, logicalName);
            if (attribute == null)
            {
                return null;
            }

            if (attribute.value.ValueKind == JsonValueKind.Object
                && attribute.value.TryGetProperty("Value", out var moneyValue))
            {
                return moneyValue.ToString();
            }

            return attribute.value.ValueKind == JsonValueKind.Number ? attribute.value.ToString() : attribute.value.GetString();
        }

        /// <summary>
        /// Parses a datetime attribute, handling both /Date()/ and ISO formats, returns ISO 8601.
        /// </summary>
        private static string? GetDateTimeValue(CrmEntity entity, string logicalName)
        {
            var attribute = FindAttribute(entity, logicalName);
            if (attribute == null)
            {
                return null;
            }

            if (attribute.value.ValueKind == JsonValueKind.String)
            {
                var text = attribute.value.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return ParseJsonDateString(text);
                }
            }

            // DateTime can also be wrapped in an object with a Value property
            if (attribute.value.ValueKind == JsonValueKind.Object
                && attribute.value.TryGetProperty("Value", out var valueElement)
                && valueElement.ValueKind == JsonValueKind.String)
            {
                var text = valueElement.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return ParseJsonDateString(text);
                }
            }

            return null;
        }

        /// <summary>
        /// Converts /Date(ms)/ (Microsoft JSON date) or ISO strings to ISO 8601. Returns raw text on failure.
        /// </summary>
        private static string ParseJsonDateString(string text)
        {
            if (text.StartsWith("/Date(") && text.EndsWith(")/"))
            {
                var raw = text.Substring(6, text.Length - 8);
                if (long.TryParse(raw, out var milliseconds))
                {
                    return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).UtcDateTime.ToString("o");
                }
            }

            if (DateTime.TryParse(text, out var parsed))
            {
                return parsed.ToString("o");
            }

            return text;
        }
    }
}

