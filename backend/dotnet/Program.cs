using FoodDelivery;
using Microsoft.Extensions.Logging;
using Temporalio.Client;
using Temporalio.Worker;

var address = GetEnv("TEMPORAL_ADDRESS", "127.0.0.1:7233");
var temporalNamespace = GetEnv("TEMPORAL_NAMESPACE", "default");
var tlsCertPath = GetEnv("TEMPORAL_TLS_CERT", string.Empty);
var tlsKeyPath = GetEnv("TEMPORAL_TLS_KEY", string.Empty);

TlsOptions? tls = null;
if (!string.IsNullOrEmpty(tlsCertPath) && !string.IsNullOrEmpty(tlsKeyPath))
{
    tls = new()
    {
        ClientCert = await File.ReadAllBytesAsync(tlsCertPath),
        ClientPrivateKey = await File.ReadAllBytesAsync(tlsKeyPath),
    };
}

var client = await TemporalClient.ConnectAsync(new()
{
    TargetHost = address,
    Namespace = temporalNamespace,
    Tls = tls,
    LoggerFactory = LoggerFactory.Create(builder =>
        builder.AddSimpleConsole(options => options.TimestampFormat = "[HH:mm:ss] ").SetMinimumLevel(LogLevel.Information)),
});

TemporalWorker worker;
switch (args.ElementAtOrDefault(0))
{
    case "workflow-worker":
        worker = new TemporalWorker(
            client, 
            new TemporalWorkerOptions(taskQueue: "durable-delivery")
                .AddWorkflow<OrderWorkflow>());
        break;
    case "activity-worker":
        worker = new TemporalWorker(
            client,
            new TemporalWorkerOptions(taskQueue: "durable-delivery-other-activity")
                .AddActivity(FoodDeliveryActivities.GetProductAsync)
                .AddActivity(FoodDeliveryActivities.RefundOrderAsync)
                .AddActivity(FoodDeliveryActivities.SendPushNotificationAsync));
        break;
    case "charge-worker":
        worker = new TemporalWorker(
            client,
            new TemporalWorkerOptions(taskQueue: "durable-delivery-charge-activity")
                .AddActivity(FoodDeliveryActivities.ChargeCustomerAsync));
        break;
    default:
        throw new ArgumentException("Must pass 'workflow-worker', 'activity-worker', or 'charge-worker' as the single argument");
}

// Run worker until cancelled
Console.WriteLine("Running worker");

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

static string GetEnv(string key, string fallback)
{
    string? value = Environment.GetEnvironmentVariable(key);
    if (string.IsNullOrEmpty(value))
    {
        return fallback;
    }
    return value;
}
