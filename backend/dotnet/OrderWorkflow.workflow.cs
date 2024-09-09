using Temporalio.Exceptions;
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
            TaskQueue = "durable-delivery-other-activity",
            StartToCloseTimeout = TimeSpan.FromSeconds(10),
            RetryPolicy = new() { MaximumInterval = TimeSpan.FromSeconds(5) },
        };

        var chargeOptions = new ActivityOptions()
        {
            TaskQueue = "durable-delivery-charge-activity",
            StartToCloseTimeout = TimeSpan.FromSeconds(10),
            RetryPolicy = new() { MaximumInterval = TimeSpan.FromSeconds(5) },
        };

        // lookup the product
        var product = await Workflow.ExecuteActivityAsync(
            () => FoodDeliveryActivities.GetProductAsync(productId), options);

        // charge the customer
        orderStatus = new OrderStatus(productId, OrderStates.CHARGING_CARD, null);
        try
        {
            await Workflow.ExecuteActivityAsync(
                () => FoodDeliveryActivities.ChargeCustomerAsync(product), chargeOptions);
        }
        catch (Exception e)
        {
            orderStatus.State = OrderStates.FAILED;
            string message = string.Format("Failed to charge customer for {0}. Error: {1}", product.Name, e.Message);
            await SendPushNotificationAsync(message, options);
            throw new ApplicationFailureException(message);
        }
        orderStatus.State = OrderStates.PAID;

        // wait for the order to be picked up, or timeout and refund
        bool pickedUpInTime = await Workflow.WaitConditionAsync(() => orderStatus.State == OrderStates.PICKED_UP, TimeSpan.FromSeconds(30));
        if (!pickedUpInTime)
        {
            orderStatus.State = OrderStates.REFUNDING;
            await RefundAndNotifyAsync(
                product,
                "⚠️  No drivers were available to pick up your order. Your payment has been refunded.",
                options);
            throw new ApplicationFailureException("Not picked up in time");
        }
        await SendPushNotificationAsync("🚗  Order picked up", options);

        // wait for the order to be delivered, or timeout and refund
        bool deliveredInTime = await Workflow.WaitConditionAsync(() => orderStatus.State == OrderStates.DELIVERED, TimeSpan.FromSeconds(30));
        if (!deliveredInTime)
        {
            orderStatus.State = OrderStates.REFUNDING;
            await RefundAndNotifyAsync(
                product,
                "⚠️  Your driver was unable to deliver your order. Your payment has been refunded.",
                options);
            throw new ApplicationFailureException("Not delivered in time");
        }
        await SendPushNotificationAsync("✅  Order delivered!", options);

        // wait for the customer to eat, then ask to rate the meal
        await Workflow.DelayAsync(TimeSpan.FromSeconds(30));
        await SendPushNotificationAsync(string.Format("✍️  Rate your meal. How was the {0}?", product.Name), options);

        return "success";
    }

    [WorkflowQuery("getStatus")]
    public OrderStatus? GetStatusQuery()
    {
        return orderStatus;
    }

    [WorkflowSignal("pickedUp")]
    public async Task PickedUpSignalAsync()
    {
        if (orderStatus?.State == OrderStates.PAID)
        {
            orderStatus.State = OrderStates.PICKED_UP;
        }
    }

    [WorkflowSignal("delivered")]
    public async Task DeliveredSignalAsync()
    {
        if (orderStatus?.State == OrderStates.PICKED_UP)
        {
            orderStatus.State = OrderStates.DELIVERED;
            orderStatus.DeliveredAt = DateTime.Now;
        }
    }

    private async Task<string> SendPushNotificationAsync(string message, ActivityOptions options)
    {
        return await Workflow.ExecuteActivityAsync(
            () => FoodDeliveryActivities.SendPushNotificationAsync(message), options);
    }

    private async Task<string> RefundAndNotifyAsync(Product product, string message, ActivityOptions options)
    {
        await Workflow.ExecuteActivityAsync(
            () => FoodDeliveryActivities.RefundOrderAsync(product), options);
        return await SendPushNotificationAsync(message, options);
    }
}
