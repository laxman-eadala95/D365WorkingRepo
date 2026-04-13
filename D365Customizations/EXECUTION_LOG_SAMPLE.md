# Sample integration execution logs

These lines illustrate the format produced by `OrderSyncService` (console) and can be used as evidence for successful vs failed API calls.

## Success (mock API / Azure Function)

```
[2026-04-13T12:00:00.0000000Z] SUCCESS order=3fa85f64-5717-4562-b3fc-2c963f66afa6 status=200 customerName=SO-1001
```

## Failure (HTTP 500 or network)

```
[2026-04-13T12:00:01.0000000Z] FAILURE order=3fa85f64-5717-4562-b3fc-2c963f66afa6 status=500 error=Internal Server Error
```

## No work

```
[2026-04-13T12:00:02.0000000Z] No orders found since 2026-04-12T12:00:00.0000000Z.
```

Run `dotnet test` on `D365.Customizations.Tests` to capture automated success/failure assertions for CI.
