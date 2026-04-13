namespace D365.Integration.OrderSync.Models
{
    /// <summary>
    /// Normalized HTTP outcome for logging and branching.
    /// </summary>
    public sealed class ApiResponse
    {
        public bool IsSuccess { get; set; }

        public int StatusCode { get; set; }

        public string ErrorMessage { get; set; }
    }
}
