/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Normalized outcome of an external API call for OrderSync logging and branching. Refer to following steps
**     1. IsSuccess and StatusCode reflect HTTP result; ErrorMessage holds body or exception text on failure
*/

namespace D365.Integration.OrderSync.Models
{
    /// <summary>
    /// Represents success or failure of posting an order, with optional error text from the response body.
    /// </summary>
    public class ApiResponse
    {
        /// <summary>True when the call completed with a success status code (typically 2xx).</summary>
        public bool IsSuccess { get; set; }

        /// <summary>HTTP status code, or 0 when the request failed before a response (e.g. network error).</summary>
        public int StatusCode { get; set; }

        /// <summary>Response body or exception message when IsSuccess is false.</summary>
        public string ErrorMessage { get; set; }
    }
}
