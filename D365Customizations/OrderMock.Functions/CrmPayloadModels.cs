using System.Collections.Generic;
using System.Text.Json;

namespace OrderMock.Functions
{
    /*
    ** Author: Laxman Eadala
    ** Date: 12-04-2026
    ** Description: Data models for deserializing CRM plugin context JSON payloads. Models match the structure
    **     of orchestration JSON sent by Dynamics 365 plugins and integration endpoints.
    */

    /// <summary>
    /// Wraps the CRM plugin execution context; Target may be direct or inside InputParameters.
    /// </summary>
    internal class CrmRequest
    {
        public CrmEntity? Target { get; set; }
        public List<CrmKeyValuePair>? InputParameters { get; set; }
    }

    /// <summary>
    /// Key-value pair used in the InputParameters array.
    /// </summary>
    internal class CrmKeyValuePair
    {
        public string? Key { get; set; }
        public JsonElement Value { get; set; }
    }

    /// <summary>
    /// CRM entity with an attributes collection; values stay as JsonElement for flexible deserialization.
    /// </summary>
    internal class CrmEntity
    {
        public string? Id { get; set; }
        public List<CrmAttribute>? Attributes { get; set; }
    }

    /// <summary>
    /// Single CRM attribute: logical name key and a deferred JsonElement value.
    /// </summary>
    internal class CrmAttribute
    {
        public string? key { get; set; }
        public JsonElement value { get; set; }
    }

    /// <summary>
    /// Parsed order fields extracted from the CRM entity.
    /// </summary>
    internal class ParsedOrder
    {
        public string? OrderNumber { get; set; }
        public string? PotentialCustomer { get; set; }
        public string? TotalAmount { get; set; }
        public string? OrderDate { get; set; }
    }
}
