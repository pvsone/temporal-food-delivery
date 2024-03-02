using FoodDelivery;
using Microsoft.Extensions.Logging;
using Temporalio.Client;
using Temporalio.Worker;

var client = await TemporalClient.ConnectAsync(new()
{
    TargetHost = "localhost:7233",
    Namespace = "default",
    LoggerFactory = LoggerFactory.Create(builder =>
        builder.AddSimpleConsole(options => options.TimestampFormat = "[HH:mm:ss] ").SetMinimumLevel(LogLevel.Information)),
});

// Run worker until cancelled
Console.WriteLine("Running worker");

using var worker = new TemporalWorker(
    client,
    new TemporalWorkerOptions("durable-delivery")
        .AddAllActivities(typeof(FoodDeliveryActivities), null)
        .AddWorkflow<OrderWorkflow>());

// Run worker until ctrl+c
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    cts.Cancel();
    eventArgs.Cancel = true;
};

try
{
    await worker.ExecuteAsync(cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Worker cancelled");
}
