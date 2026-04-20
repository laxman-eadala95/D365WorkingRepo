/*
** Author: Laxman Eadala
** Date: 20-04-2026
** Description: Tests OrdersFunction CRM payload parsing, attribute extraction, and response contract. Refer to following steps
**     1. Full order payload with direct Target resolves all fields correctly
**     2. InputParameters-based Target is resolved when direct Target is absent
**     3. EntityReference extracts Name, falls back to Id when Name missing
**     4. Money attribute unwraps Value property; bare numbers handled
**     5. DateTime handles ISO strings, /Date(ms)/ format, and wrapped Value objects
**     6. Missing attributes default to "(unknown)" placeholders
**     7. Empty/malformed JSON does not throw; defaults preserved
**     8. OrderNumber fallback chain: ordernumber -> salesorderid -> entity Id
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace D365.Customizations.Tests.Integration
{
    /// <summary>
    /// Tests for OrderMock.Functions CRM payload deserialization and field extraction logic.
    /// Validates the JSON contract between OrderSync and the mock function endpoint.
    /// </summary>
    public class OrdersFunctionTests
    {
        private static readonly JsonSerializerOptions CaseInsensitive =
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // ------------------------------------------------------------------
        //  Full payload with direct Target
        // ------------------------------------------------------------------

        /// <summary>Complete CRM payload with direct Target should resolve all order fields.</summary>
        [Fact]
        public void FullPayload_DirectTarget_ResolvesAllFields()
        {
            var json = @"{
                ""Target"": {
                    ""Id"": ""A1B2C3D4-0000-0000-0000-000000000001"",
                    ""Attributes"": [
                        { ""key"": ""ordernumber"", ""value"": ""SO-1001"" },
                        { ""key"": ""customerid"", ""value"": { ""Name"": ""Contoso Ltd"", ""Id"": ""C1D2E3F4-0000-0000-0000-000000000002"" } },
                        { ""key"": ""totalamount"", ""value"": { ""Value"": 1500.75 } },
                        { ""key"": ""createdon"", ""value"": ""2026-04-01T10:30:00Z"" }
                    ]
                }
            }";

            var parsed = ParseOrderFromJson(json);

            Assert.Equal("SO-1001", parsed.OrderNumber);
            Assert.Equal("Contoso Ltd", parsed.PotentialCustomer);
            Assert.Equal("1500.75", parsed.TotalAmount);
            Assert.Contains("2026-04-01", parsed.OrderDate);
        }

        // ------------------------------------------------------------------
        //  Target via InputParameters
        // ------------------------------------------------------------------

        /// <summary>Target inside InputParameters array should be resolved when direct Target is null.</summary>
        [Fact]
        public void InputParameters_ResolvesTarget_WhenDirectTargetAbsent()
        {
            var json = @"{
                ""InputParameters"": [
                    {
                        ""Key"": ""Target"",
                        ""Value"": {
                            ""Id"": ""B2C3D4E5-0000-0000-0000-000000000003"",
                            ""Attributes"": [
                                { ""key"": ""ordernumber"", ""value"": ""SO-2001"" }
                            ]
                        }
                    }
                ]
            }";

            var parsed = ParseOrderFromJson(json);

            Assert.Equal("SO-2001", parsed.OrderNumber);
        }

        /// <summary>InputParameters key match should be case-insensitive.</summary>
        [Fact]
        public void InputParameters_CaseInsensitiveKeyMatch()
        {
            var json = @"{
                ""InputParameters"": [
                    {
                        ""Key"": ""target"",
                        ""Value"": {
                            ""Attributes"": [
                                { ""key"": ""ordernumber"", ""value"": ""SO-LOWER"" }
                            ]
                        }
                    }
                ]
            }";

            var parsed = ParseOrderFromJson(json);

            Assert.Equal("SO-LOWER", parsed.OrderNumber);
        }

        // ------------------------------------------------------------------
        //  OrderNumber fallback chain
        // ------------------------------------------------------------------

        /// <summary>Missing ordernumber should fall back to salesorderid attribute.</summary>
        [Fact]
        public void OrderNumber_FallsBackToSalesOrderId()
        {
            var json = @"{
                ""Target"": {
                    ""Attributes"": [
                        { ""key"": ""salesorderid"", ""value"": ""GUID-FALLBACK-001"" }
                    ]
                }
            }";

            var parsed = ParseOrderFromJson(json);

            Assert.Equal("GUID-FALLBACK-001", parsed.OrderNumber);
        }

        /// <summary>Missing ordernumber and salesorderid should fall back to entity Id.</summary>
        [Fact]
        public void OrderNumber_FallsBackToEntityId()
        {
            var json = @"{
                ""Target"": {
                    ""Id"": ""E5F6A7B8-0000-0000-0000-000000000004"",
                    ""Attributes"": []
                }
            }";

            var parsed = ParseOrderFromJson(json);

            Assert.Equal("E5F6A7B8-0000-0000-0000-000000000004", parsed.OrderNumber);
        }

        /// <summary>No ordernumber, salesorderid, or entity Id should keep the default placeholder.</summary>
        [Fact]
        public void OrderNumber_DefaultsToUnknown_WhenAllFallbacksMissing()
        {
            var json = @"{
                ""Target"": {
                    ""Attributes"": []
                }
            }";

            var parsed = ParseOrderFromJson(json);

            Assert.Equal("(unknown)", parsed.OrderNumber);
        }

        // ------------------------------------------------------------------
        //  EntityReference extraction
        // ------------------------------------------------------------------

        /// <summary>EntityReference with Name should extract the display name.</summary>
        [Fact]
        public void EntityReference_ExtractsName_WhenPresent()
        {
            var json = @"{
                ""Target"": {
                    ""Attributes"": [
                        { ""key"": ""customerid"", ""value"": { ""Name"": ""Adventure Works"", ""Id"": ""00000000-0000-0000-0000-AAAAAAAAAAAA"" } }
                    ]
                }
            }";

            var parsed = ParseOrderFromJson(json);

            Assert.Equal("Adventure Works", parsed.PotentialCustomer);
        }

        /// <summary>EntityReference without Name should fall back to Id GUID.</summary>
        [Fact]
        public void EntityReference_FallsBackToId_WhenNameMissing()
        {
            var json = @"{
                ""Target"": {
                    ""Attributes"": [
                        { ""key"": ""customerid"", ""value"": { ""Id"": ""00000000-0000-0000-0000-BBBBBBBBBBBB"" } }
                    ]
                }
            }";

            var parsed = ParseOrderFromJson(json);

            Assert.Equal("00000000-0000-0000-0000-BBBBBBBBBBBB", parsed.PotentialCustomer);
        }

        /// <summary>Missing customerid attribute should keep the default placeholder.</summary>
        [Fact]
        public void PotentialCustomer_DefaultsToUnknown_WhenMissing()
        {
            var json = @"{
                ""Target"": {
                    ""Attributes"": [
                        { ""key"": ""ordernumber"", ""value"": ""SO-NO-CUST"" }
                    ]
                }
            }";

            var parsed = ParseOrderFromJson(json);

            Assert.Equal("(unknown)", parsed.PotentialCustomer);
        }

        // ------------------------------------------------------------------
        //  Money attribute
        // ------------------------------------------------------------------

        /// <summary>Money object with Value property should extract the numeric value.</summary>
        [Fact]
        public void Money_ExtractsValueProperty()
        {
            var json = @"{
                ""Target"": {
                    ""Attributes"": [
                        { ""key"": ""totalamount"", ""value"": { ""Value"": 999.99 } }
                    ]
                }
            }";

            var parsed = ParseOrderFromJson(json);

            Assert.Equal("999.99", parsed.TotalAmount);
        }

        /// <summary>Bare numeric totalamount (not wrapped in Money object) should still resolve.</summary>
        [Fact]
        public void Money_HandlesBareNumber()
        {
            var json = @"{
                ""Target"": {
                    ""Attributes"": [
                        { ""key"": ""totalamount"", ""value"": 250 }
                    ]
                }
            }";

            var parsed = ParseOrderFromJson(json);

            Assert.Equal("250", parsed.TotalAmount);
        }

        /// <summary>Missing totalamount should keep the default placeholder.</summary>
        [Fact]
        public void TotalAmount_DefaultsToUnknown_WhenMissing()
        {
            var json = @"{
                ""Target"": {
                    ""Attributes"": []
                }
            }";

            var parsed = ParseOrderFromJson(json);

            Assert.Equal("(unknown)", parsed.TotalAmount);
        }

        // ------------------------------------------------------------------
        //  DateTime parsing
        // ------------------------------------------------------------------

        /// <summary>ISO 8601 date string should be parsed and reformatted to ISO 8601 round-trip.</summary>
        [Fact]
        public void DateTime_ParsesIsoString()
        {
            var json = @"{
                ""Target"": {
                    ""Attributes"": [
                        { ""key"": ""createdon"", ""value"": ""2026-03-15T14:00:00Z"" }
                    ]
                }
            }";

            var parsed = ParseOrderFromJson(json);

            Assert.Contains("2026-03-15", parsed.OrderDate);
        }

        /// <summary>Microsoft /Date(ms)/ format should be converted to ISO 8601.</summary>
        [Fact]
        public void DateTime_ParsesMicrosoftDateFormat()
        {
            // 1711900800000 ms = 2024-03-31T16:00:00Z
            var json = @"{
                ""Target"": {
                    ""Attributes"": [
                        { ""key"": ""createdon"", ""value"": ""/Date(1711900800000)/"" }
                    ]
                }
            }";

            var parsed = ParseOrderFromJson(json);

            Assert.Contains("2024-03-31", parsed.OrderDate);
        }

        /// <summary>DateTime wrapped in an object { Value: "..." } should be unwrapped and parsed.</summary>
        [Fact]
        public void DateTime_UnwrapsValueObject()
        {
            var json = @"{
                ""Target"": {
                    ""Attributes"": [
                        { ""key"": ""createdon"", ""value"": { ""Value"": ""2026-06-01T08:00:00Z"" } }
                    ]
                }
            }";

            var parsed = ParseOrderFromJson(json);

            Assert.Contains("2026-06-01", parsed.OrderDate);
        }

        /// <summary>Missing createdon should keep the default placeholder.</summary>
        [Fact]
        public void OrderDate_DefaultsToUnknown_WhenMissing()
        {
            var json = @"{
                ""Target"": {
                    ""Attributes"": []
                }
            }";

            var parsed = ParseOrderFromJson(json);

            Assert.Equal("(unknown)", parsed.OrderDate);
        }

        // ------------------------------------------------------------------
        //  Edge cases: empty and malformed JSON
        // ------------------------------------------------------------------

        /// <summary>Empty JSON object should not throw; all fields default to "(unknown)".</summary>
        [Fact]
        public void EmptyJson_DefaultsAllFields()
        {
            var parsed = ParseOrderFromJson("{}");

            Assert.Equal("(unknown)", parsed.OrderNumber);
            Assert.Equal("(unknown)", parsed.PotentialCustomer);
            Assert.Equal("(unknown)", parsed.TotalAmount);
            Assert.Equal("(unknown)", parsed.OrderDate);
        }

        /// <summary>Null/whitespace body should not throw; all fields default to "(unknown)".</summary>
        [Fact]
        public void NullBody_DefaultsAllFields()
        {
            var parsed = ParseOrderFromJson(null);

            Assert.Equal("(unknown)", parsed.OrderNumber);
            Assert.Equal("(unknown)", parsed.PotentialCustomer);
            Assert.Equal("(unknown)", parsed.TotalAmount);
            Assert.Equal("(unknown)", parsed.OrderDate);
        }

        /// <summary>Malformed JSON should not throw; all fields default to "(unknown)".</summary>
        [Fact]
        public void MalformedJson_DefaultsAllFields()
        {
            var parsed = ParseOrderFromJson("{bad json!!");

            Assert.Equal("(unknown)", parsed.OrderNumber);
            Assert.Equal("(unknown)", parsed.PotentialCustomer);
            Assert.Equal("(unknown)", parsed.TotalAmount);
            Assert.Equal("(unknown)", parsed.OrderDate);
        }

        /// <summary>Attribute lookup should be case-insensitive to match CRM serialization variants.</summary>
        [Fact]
        public void AttributeLookup_CaseInsensitive()
        {
            var json = @"{
                ""Target"": {
                    ""Attributes"": [
                        { ""key"": ""OrderNumber"", ""value"": ""SO-UPPER"" }
                    ]
                }
            }";

            var parsed = ParseOrderFromJson(json);

            Assert.Equal("SO-UPPER", parsed.OrderNumber);
        }

        // ==================================================================
        //  Private helpers — mirror OrdersFunction extraction logic so tests
        //  validate the same JSON contract without a net8.0 project reference.
        // ==================================================================

        /// <summary>Parses a raw JSON body through the same logic path as OrdersFunction.Run.</summary>
        private static ParsedOrderResult ParseOrderFromJson(string body)
        {
            var result = new ParsedOrderResult
            {
                OrderNumber = "(unknown)",
                PotentialCustomer = "(unknown)",
                TotalAmount = "(unknown)",
                OrderDate = "(unknown)"
            };

            try
            {
                var request = JsonSerializer.Deserialize<CrmRequestDto>(
                    string.IsNullOrWhiteSpace(body) ? "{}" : body, CaseInsensitive);

                var entity = GetTargetEntity(request);
                if (entity == null) return result;

                result.OrderNumber = GetAttributeString(entity, "ordernumber")
                    ?? GetAttributeString(entity, "salesorderid")
                    ?? entity.Id
                    ?? result.OrderNumber;

                result.PotentialCustomer = GetEntityReferenceName(entity, "customerid")
                    ?? GetEntityReferenceId(entity, "customerid")
                    ?? result.PotentialCustomer;

                result.TotalAmount = GetMoneyValue(entity, "totalamount")
                    ?? result.TotalAmount;

                result.OrderDate = GetDateTimeValue(entity, "createdon")
                    ?? result.OrderDate;
            }
            catch (JsonException)
            {
                // Mirrors OrdersFunction catch: swallow parse errors, keep defaults
            }

            return result;
        }

        private static CrmEntityDto GetTargetEntity(CrmRequestDto request)
        {
            if (request == null) return null;
            if (request.Target != null) return request.Target;
            if (request.InputParameters == null) return null;

            foreach (var p in request.InputParameters)
            {
                if (string.Equals(p.Key, "Target", StringComparison.OrdinalIgnoreCase)
                    && p.Value.ValueKind == JsonValueKind.Object)
                {
                    return JsonSerializer.Deserialize<CrmEntityDto>(p.Value.GetRawText(), CaseInsensitive);
                }
            }

            return null;
        }

        private static CrmAttributeDto FindAttribute(CrmEntityDto entity, string logicalName)
        {
            if (entity.Attributes == null) return null;

            foreach (var attr in entity.Attributes)
            {
                if (string.Equals(attr.key, logicalName, StringComparison.OrdinalIgnoreCase))
                    return attr;
            }

            return null;
        }

        private static string GetAttributeString(CrmEntityDto entity, string logicalName)
        {
            var attr = FindAttribute(entity, logicalName);
            if (attr == null) return null;

            switch (attr.value.ValueKind)
            {
                case JsonValueKind.String:
                    return attr.value.GetString();
                case JsonValueKind.Number:
                    return attr.value.ToString();
                case JsonValueKind.Object:
                    JsonElement inner;
                    if (attr.value.TryGetProperty("Value", out inner)) return inner.ToString();
                    if (attr.value.TryGetProperty("Id", out inner)) return inner.ToString();
                    if (attr.value.TryGetProperty("Name", out inner) && inner.ValueKind == JsonValueKind.String)
                        return inner.GetString();
                    return attr.value.ToString();
                default:
                    return attr.value.ToString();
            }
        }

        private static string GetEntityReferenceName(CrmEntityDto entity, string logicalName)
        {
            var attr = FindAttribute(entity, logicalName);
            if (attr == null) return null;

            if (attr.value.ValueKind == JsonValueKind.Object)
            {
                JsonElement nameVal;
                if (attr.value.TryGetProperty("Name", out nameVal) && nameVal.ValueKind == JsonValueKind.String)
                    return nameVal.GetString();
            }

            return GetAttributeString(entity, logicalName);
        }

        private static string GetEntityReferenceId(CrmEntityDto entity, string logicalName)
        {
            var attr = FindAttribute(entity, logicalName);
            if (attr == null) return null;

            if (attr.value.ValueKind == JsonValueKind.Object)
            {
                JsonElement idVal;
                if (attr.value.TryGetProperty("Id", out idVal))
                    return idVal.ValueKind == JsonValueKind.String ? idVal.GetString() : idVal.ToString();
            }

            return attr.value.ValueKind == JsonValueKind.String ? attr.value.GetString() : null;
        }

        private static string GetMoneyValue(CrmEntityDto entity, string logicalName)
        {
            var attr = FindAttribute(entity, logicalName);
            if (attr == null) return null;

            if (attr.value.ValueKind == JsonValueKind.Object)
            {
                JsonElement moneyVal;
                if (attr.value.TryGetProperty("Value", out moneyVal))
                    return moneyVal.ToString();
            }

            return attr.value.ValueKind == JsonValueKind.Number ? attr.value.ToString() : attr.value.GetString();
        }

        private static string GetDateTimeValue(CrmEntityDto entity, string logicalName)
        {
            var attr = FindAttribute(entity, logicalName);
            if (attr == null) return null;

            if (attr.value.ValueKind == JsonValueKind.String)
            {
                var text = attr.value.GetString();
                if (!string.IsNullOrWhiteSpace(text)) return ParseJsonDateString(text);
            }

            if (attr.value.ValueKind == JsonValueKind.Object)
            {
                JsonElement valElem;
                if (attr.value.TryGetProperty("Value", out valElem) && valElem.ValueKind == JsonValueKind.String)
                {
                    var text = valElem.GetString();
                    if (!string.IsNullOrWhiteSpace(text)) return ParseJsonDateString(text);
                }
            }

            return null;
        }

        private static string ParseJsonDateString(string text)
        {
            if (text.StartsWith("/Date(") && text.EndsWith(")/"))
            {
                var raw = text.Substring(6, text.Length - 8);
                long milliseconds;
                if (long.TryParse(raw, out milliseconds))
                    return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).UtcDateTime.ToString("o");
            }

            DateTime parsed;
            if (DateTime.TryParse(text, out parsed))
                return parsed.ToString("o");

            return text;
        }

        // ------------------------------------------------------------------
        //  DTOs — mirror OrderMock.Functions.CrmPayloadModels for net462
        // ------------------------------------------------------------------

        /// <summary>Mirrors CrmRequest: wraps Target and InputParameters.</summary>
        private class CrmRequestDto
        {
            public CrmEntityDto Target { get; set; }
            public List<CrmKeyValuePairDto> InputParameters { get; set; }
        }

        /// <summary>Mirrors CrmKeyValuePair: key-value with deferred JsonElement.</summary>
        private class CrmKeyValuePairDto
        {
            public string Key { get; set; }
            public JsonElement Value { get; set; }
        }

        /// <summary>Mirrors CrmEntity: Id and Attributes list.</summary>
        private class CrmEntityDto
        {
            public string Id { get; set; }
            public List<CrmAttributeDto> Attributes { get; set; }
        }

        /// <summary>Mirrors CrmAttribute: logical name key and JsonElement value.</summary>
        private class CrmAttributeDto
        {
            public string key { get; set; }
            public JsonElement value { get; set; }
        }

        /// <summary>Mirrors ParsedOrder: extracted display fields.</summary>
        private class ParsedOrderResult
        {
            public string OrderNumber { get; set; }
            public string PotentialCustomer { get; set; }
            public string TotalAmount { get; set; }
            public string OrderDate { get; set; }
        }
    }
}
