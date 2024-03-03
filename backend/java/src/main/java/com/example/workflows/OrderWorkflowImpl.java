package com.example.workflows;

import com.example.activities.FoodDeliveryActivities;
import com.example.model.OrderStates;
import com.example.model.OrderStatus;
import com.example.model.Product;
import io.temporal.activity.ActivityOptions;
import io.temporal.common.RetryOptions;
import io.temporal.failure.ApplicationFailure;
import io.temporal.workflow.Workflow;
import org.slf4j.Logger;

import java.time.Duration;
import java.util.Date;

public class OrderWorkflowImpl implements OrderWorkflow {

    public static final Logger logger = Workflow.getLogger(OrderWorkflowImpl.class);
    ActivityOptions activityOptions = ActivityOptions.newBuilder()
            .setStartToCloseTimeout(Duration.ofSeconds(10))
            .setRetryOptions(RetryOptions.newBuilder()
                    .setMaximumInterval(Duration.ofSeconds(5))
                    .build())
            .build();

    private final FoodDeliveryActivities activities =
            Workflow.newActivityStub(FoodDeliveryActivities.class, activityOptions);

    private OrderStatus orderStatus;

    @Override
    public String order(int productId) {
        // lookup the product
        Product product = activities.getProduct(productId);

        // charge the customers
        orderStatus = new OrderStatus(productId, OrderStates.CHARGING_CARD, null);
        try {
            activities.chargeCustomer(product);
        } catch (Exception e) {
            orderStatus.setState(OrderStates.FAILED);
            String message = String.format("Failed to charge customer for %s. Error: %s}", product.getName(), e.getMessage());
            activities.sendPushNotification(message);
            throw ApplicationFailure.newFailureWithCause(message, e.getClass().getName(), e.getCause());
        }
        orderStatus.setState(OrderStates.PAID);

        // wait for the order to be picked up, or timeout and refund
        boolean notPickedUpInTime = !Workflow.await(Duration.ofSeconds(30), () -> orderStatus.getState() == OrderStates.PICKED_UP);
        if (notPickedUpInTime) {
            orderStatus.setState(OrderStates.REFUNDING);
            refundAndNotify(
                    product,
                    "⚠️  No drivers were available to pick up your order. Your payment has been refunded."
            );
            throw ApplicationFailure.newFailure("Not picked up in time", "NotPickedUpInTime");
        }
        activities.sendPushNotification("🚗  Order picked up");

        // wait for the order to be delivered, or timeout and refund
        boolean notDeliveredInTime = !Workflow.await(Duration.ofSeconds(30), () -> orderStatus.getState() == OrderStates.DELIVERED);
        if (notDeliveredInTime) {
            orderStatus.setState(OrderStates.REFUNDING);
            refundAndNotify(
                    product,
                    "⚠️  Your driver was unable to deliver your order. Your payment has been refunded."
            );
            throw ApplicationFailure.newFailure("Not delivered in time", "NotDeliveredInTime");
        }
        activities.sendPushNotification("✅  Order delivered!");

        Workflow.sleep(Duration.ofSeconds(30)); // this could also be hours or even months
        activities.sendPushNotification(String.format("✍️  Rate your meal. How was the %s?", product.getName()));

        return "success";
    }

    @Override
    public void pickedUpSignal() {
        if (orderStatus.getState() == OrderStates.PAID) {
            orderStatus.setState(OrderStates.PICKED_UP);
        }
    }

    @Override
    public void deliveredSignal() {
        if (orderStatus.getState() == OrderStates.PICKED_UP) {
            orderStatus.setState(OrderStates.DELIVERED);
            orderStatus.setDeliveredAt(new Date());
        }
    }

    @Override
    public OrderStatus getStatusQuery() {
        return orderStatus;
    }

    private void refundAndNotify(Product product, String message) {
        activities.refundOrder(product);
        activities.sendPushNotification(message);
    }
}
