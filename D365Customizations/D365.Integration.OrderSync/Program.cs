/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Console app entry that syncs Sales Orders from Dataverse to an external API or mock. Refer to following steps
**     1. Read Dataverse connection and API URL from App.config or environment variables
**     2. Connect with ServiceClient; choose OrderRepository and real or mock IExternalApiClient
**     3. Parse optional UTC since argument; run OrderSyncService.SyncNewOrdersAsync
*/

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
        /// <summary>
        /// Application entry: connects to Dataverse, builds repository and API client, runs sync for orders since a cutoff time.
        /// </summary>
        /// <param name="args">Optional first argument: UTC date/time parsed as sync lower bound (default: last 24 hours).</param>
        private static async Task Main(string[] args)
        {
            // Prefer App.config; environment variables override for CI/containers.
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

                // Default window: last 24 hours; optional CLI arg overrides with a UTC instant.
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
