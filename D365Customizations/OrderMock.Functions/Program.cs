/*
** Author: Laxman Eadala
** Date: 12-04-2026
** Description: Host bootstrap for the Azure Functions isolated worker that runs OrderMock.Functions. Refer to following steps
**     1. Configure Functions worker defaults and run the host process
*/

using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .Build();

host.Run();
