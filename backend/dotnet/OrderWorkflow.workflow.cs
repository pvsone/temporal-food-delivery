using Temporalio.Workflows;

namespace FoodDelivery;

[Workflow("order")]
public class OrderWorkflow
{
    private OrderStatus? orderStatus;

    [WorkflowRun]
    public async Task<string> RunAsync(int productId)
    {
        var logger = Workflow.Logger;

        var options = new ActivityOptions()
        {
            StartToCloseTimeout = TimeSpan.FromSeconds(10),
            RetryPolicy = new() { MaximumInterval = TimeSpan.FromSeconds(5) },
        };

        // business logic
        var product = await Workflow.ExecuteActivityAsync(
            () => FoodDeliveryActivities.GetProductAsync(productId), options);

        orderStatus = new OrderStatus(productId, OrderStates.CHARGING_CARD, null);

        await Workflow.ExecuteActivityAsync(
            () => FoodDeliveryActivities.ChargeCustomerAsync(product), options);

        orderStatus.State = OrderStates.PAID;

        await Workflow.ExecuteActivityAsync(
            () => FoodDeliveryActivities.SendPushNotificationAsync("🚗  Order picked up"), options);

        await Workflow.DelayAsync(TimeSpan.FromSeconds(10));
        orderStatus.State = OrderStates.PICKED_UP;

        await Workflow.ExecuteActivityAsync(
            () => FoodDeliveryActivities.SendPushNotificationAsync("✅  Order delivered!"), options);

        await Workflow.DelayAsync(TimeSpan.FromSeconds(10));
        orderStatus.State = OrderStates.DELIVERED;

        await Workflow.ExecuteActivityAsync(
            () => FoodDeliveryActivities.SendPushNotificationAsync(string.Format("✍️  Rate your meal. How was the {0}?", product.Name)), options);

        return "success";
    }

    [WorkflowQuery("getStatus")]
    public OrderStatus? GetStatus()
    {
        return orderStatus;
    }
}
