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

// Run worker until cancelled
Console.WriteLine("Running worker");

using var worker = new TemporalWorker(
    client,
    new TemporalWorkerOptions(taskQueue: "durable-delivery")
        .AddActivity(FoodDeliveryActivities.ChargeCustomerAsync)
        .AddActivity(FoodDeliveryActivities.GetProductAsync)
        .AddActivity(FoodDeliveryActivities.RefundOrderAsync)
        .AddActivity(FoodDeliveryActivities.SendPushNotificationAsync)
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

static string GetEnv(string key, string fallback)
{
    string? value = Environment.GetEnvironmentVariable(key);
    if (string.IsNullOrEmpty(value))
    {
        return fallback;
    }
    return value;
}
