# Order integration: triggers and configuration

## What the console app does

`D365.Integration.OrderSync` connects to Dataverse with `ServiceClient`, queries **salesorder** rows created on or after a **UTC** timestamp, maps fields to JSON, and **POST**s to a REST URL (or uses the in-process mock).

## Trigger option A — Polling (implemented)

1. Deploy the console executable (or run from build output).
2. Schedule it (Windows Task Scheduler, Azure WebJob, container cron) every N minutes.
3. Pass the **since** timestamp as the first argument, or rely on the default (last 24 hours) in `Program.cs`.
4. Set `UseMockApi` to `false` in `App.config` and set `OrderApiUrl` to your **Azure Function** URL. Locally, run the function app from **`OrderMock.Functions`** (`func start` or F5 in Visual Studio); default base URL is typically `http://localhost:7071`, so use **`http://localhost:7071/api/orders`**. After deployment, use `https://<your-function-app>.azurewebsites.net/api/orders` (append `?code=<function-key>` if not using anonymous auth).

## Trigger option B — Real-time webhook (documented)

1. Expose an HTTPS endpoint (e.g. Azure Function) that accepts the **Remote Execution Context** posted by Dataverse.
2. In **Plugin Registration Tool**, register a **Service Endpoint** pointing to that URL.
3. Register a plugin step on **salesorder** **Create**, **PostOperation**, **Asynchronous**, with the service endpoint as the destination.
4. The function extracts order details from the context and forwards them to the same REST API shape as the console app.

For this assessment, the **console + polling** path is the reference implementation; webhook is the production-style alternative.

## Configuration keys

| Key | Purpose |
|-----|--------|
| `CrmConnection` | Dataverse connection string (OAuth). Prefer environment variable `CRM_CONNECTION_STRING` in CI. |
| `OrderApiUrl` | Full POST URL for orders. |
| `UseMockApi` | `true` uses `MockExternalApiClient` (no HTTP). |

See `SECURITY_RECOMMENDATIONS.md` for secret handling.
