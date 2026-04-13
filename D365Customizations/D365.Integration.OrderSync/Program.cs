using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using D365.Integration.OrderSync.Services;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace D365.Integration.OrderSync
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            // Read config: prefer environment variables, fall back to App.config
            var connectionString = ConfigurationManager.AppSettings["CrmConnection"]
                ?? Environment.GetEnvironmentVariable("CRM_CONNECTION_STRING");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.Error.WriteLine("Set AppSettings:CrmConnection or CRM_CONNECTION_STRING.");
                Environment.ExitCode = 1;
                return;
            }

            var apiUrl = ConfigurationManager.AppSettings["OrderApiUrl"]
                ?? Environment.GetEnvironmentVariable("ORDER_API_URL")
                ?? "http://localhost:7071/api/orders";

            var useMock = bool.TryParse(ConfigurationManager.AppSettings["UseMockApi"], out var m) && m;

            using (var service = new ServiceClient(connectionString))
            {
                if (!service.IsReady)
                {
                    Console.Error.WriteLine($"Dataverse connection failed: {service.LastError}");
                    Environment.ExitCode = 2;
                    return;
                }

                var repository = new OrderRepository(service);

                IExternalApiClient apiClient;
                if (useMock)
                {
                    apiClient = new MockExternalApiClient();
                    Console.WriteLine("Using in-memory mock (no HTTP calls).");
                }
                else
                {
                    apiClient = new ExternalApiClient(new HttpClient(), apiUrl);
                    Console.WriteLine($"Posting orders to: {apiUrl}");
                }

                var sync = new OrderSyncService(repository, apiClient, Console.WriteLine);

                // Default: last 24 hours; or pass a UTC date as first arg
                var since = DateTime.UtcNow.AddHours(-24);
                if (args?.Length > 0 && DateTime.TryParse(args[0], null,
                        System.Globalization.DateTimeStyles.AdjustToUniversal, out var parsed))
                {
                    since = parsed.ToUniversalTime();
                }

                Console.WriteLine($"Syncing orders since {since:O} ...");
                await sync.SyncNewOrdersAsync(since).ConfigureAwait(false);
            }
        }
    }
}
