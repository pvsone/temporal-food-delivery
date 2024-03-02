namespace FoodDelivery;

using Microsoft.Extensions.Logging;
using Temporalio.Activities;
using Temporalio.Exceptions;

public record Product(int Id, string Name, int Cents);

public class FoodDeliveryActivities
{
    [Activity]
    public static async Task<Product> GetProductAsync(int productId)
    {
        if (Products.TryGetValue(productId, out var product))
        {
            await Task.Delay(100);
            return product;
        }
        else
        {
            throw new ApplicationFailureException(string.Format("Product {0} not found", productId), nonRetryable: true);
        }
    }

    [Activity]
    public static async Task<string> SendPushNotificationAsync(string message)
    {
        Random random = new Random();
        if (random.NextDouble() < 0.5)
        {
            throw new Exception("Failed to send push notification. Unable to reach notification service.");
        }

        await Task.Delay(100);

        ActivityExecutionContext.Current.Logger.LogInformation("Sent notification {Type}, {Message}", "push", message);
        return "success";
    }

    [Activity]
    public static async Task<string> ChargeCustomerAsync(Product product)
    {
        var ctx = ActivityExecutionContext.Current;

        // In a real app, we would pass an idempotency token to the downstream service
        var idempotencyToken = $"{ctx.Info.WorkflowId}-charge";
        ctx.Logger.LogDebug("Idempotency Token {IdempotencyToken}", idempotencyToken);

        if (product.Cents >= 3500)
        {
            throw new ApplicationFailureException("Card declined: insufficient funds", nonRetryable: true);
        }

        Random random = new Random();
        if (random.NextDouble() < 0.5)
        {
            throw new Exception("Failed to charge. Unable to reach payment service.");
        }

        await Task.Delay(100);

        ctx.Logger.LogInformation("Charged {Cents}", product.Cents);
        return "success";
    }

    [Activity]
    public static async Task<string> RefundOrderAsync(Product product)
    {
        var ctx = ActivityExecutionContext.Current;

        // In a real app, we would pass an idempotency token to the downstream service
        var idempotencyToken = $"{ctx.Info.WorkflowId}-charge";
        ctx.Logger.LogDebug("Idempotency Token {IdempotencyToken}", idempotencyToken);

        Random random = new Random();
        if (random.NextDouble() < 0.5)
        {
            throw new Exception("Failed to refund. Unable to reach payment service.");
        }

        await Task.Delay(100);

        ctx.Logger.LogInformation("Refunded {Cents}", product.Cents);
        return "success";
    }

    private static readonly Dictionary<int, Product> Products = new Dictionary<int, Product>
    {
        { 1, new Product(1, "Swordfish", 3500) },
        { 2, new Product(2, "Burrata", 2000) },
        { 3, new Product(3, "Potato", 1500) },
        { 4, new Product(4, "Poke", 2000) },
    };
}
