using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using D365.Integration.OrderSync.Services;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace D365.Integration.OrderSync
{
    /// <summary>
    /// Composition root: Dataverse connection, repository, API client, polling sync.
    /// Connection string name: <c>CrmConnection</c> in App.config (use OAuth secrets via env/Key Vault in production).
    /// </summary>
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
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

            var useMock = bool.TryParse(ConfigurationManager.AppSettings["UseMockApi"], out var mock) && mock;

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
                    apiClient = new MockExternalApiClient(succeed: true);
                    Console.WriteLine("Using MockExternalApiClient (no HTTP).");
                }
                else
                {
                    var http = new HttpClient();
                    apiClient = new ExternalApiClient(http, apiUrl);
                    Console.WriteLine($"Posting orders to: {apiUrl}");
                }

                void Log(string line)
                {
                    Console.WriteLine(line);
                }

                var sync = new OrderSyncService(repository, apiClient, Log);

                var since = DateTime.UtcNow.AddHours(-24);
                if (args != null && args.Length > 0 && DateTime.TryParse(args[0], null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var parsed))
                {
                    since = parsed.ToUniversalTime();
                }

                Console.WriteLine($"Syncing orders created on or after {since:O} (UTC)...");
                await sync.SyncNewOrdersAsync(since).ConfigureAwait(false);
            }
        }
    }
}
