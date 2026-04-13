namespace D365.Integration.OrderSync.Models
{
    /// <summary>
    /// Simple result object for API call outcomes.
    /// </summary>
    public class ApiResponse
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
