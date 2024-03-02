namespace FoodDelivery;

using Temporalio.Workflows;

[Workflow("order")]
public class OrderWorkflow
{
    [WorkflowRun]
    public async Task<string> RunAsync(int productId)
    {
        var logger = Workflow.Logger;

        var options = new ActivityOptions()
        {
            StartToCloseTimeout = TimeSpan.FromSeconds(10),
            RetryPolicy = new() { MaximumInterval = TimeSpan.FromSeconds(5) },
        };

        var product = await Workflow.ExecuteActivityAsync(
            () => FoodDeliveryActivities.GetProductAsync(productId), options);

        await Workflow.DelayAsync(TimeSpan.FromSeconds(1));

        await Workflow.ExecuteActivityAsync(
            () => FoodDeliveryActivities.ChargeCustomerAsync(product), options);

        await Workflow.DelayAsync(TimeSpan.FromSeconds(1));

        await Workflow.ExecuteActivityAsync(
            () => FoodDeliveryActivities.SendPushNotificationAsync("🚗  Order picked up"), options);

        await Workflow.DelayAsync(TimeSpan.FromSeconds(1));

        await Workflow.ExecuteActivityAsync(
            () => FoodDeliveryActivities.SendPushNotificationAsync("✅  Order delivered!"), options);

        await Workflow.DelayAsync(TimeSpan.FromSeconds(1));

        await Workflow.ExecuteActivityAsync(
            () => FoodDeliveryActivities.SendPushNotificationAsync(string.Format("✍️  Rate your meal. How was the {0}?", product.Name)), options);

        return "success";
    }
}
