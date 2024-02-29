package com.example.activities;

import com.example.model.Product;
import io.temporal.activity.ActivityInterface;
import io.temporal.activity.ActivityMethod;

@ActivityInterface
public interface FoodDeliveryActivities {

    @ActivityMethod
    Product getProduct(int productId);

    @ActivityMethod
    String sendPushNotification(String message);

    @ActivityMethod
    String refundOrder(Product product);

    @ActivityMethod
    String chargeCustomer(Product product);
}
