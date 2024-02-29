package com.example.activities;

import com.example.model.Product;
import com.example.services.NotificationService;
import com.example.services.PaymentService;
import com.example.services.ProductService;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class FoodDeliveryActivitiesImpl implements FoodDeliveryActivities {

    private static final Logger logger = LoggerFactory.getLogger(FoodDeliveryActivitiesImpl.class);

    @Override
    public Product getProduct(int productId) {
        return ProductService.getProductById(productId);
    }

    @Override
    public String sendPushNotification(String message) {
        return NotificationService.sendNotification("push", message);
    }


    @Override
    public String refundOrder(Product product) {
        return PaymentService.refund(product.getCents());
    }


    @Override
    public String chargeCustomer(Product product) {
        return PaymentService.charge(product.getCents());
    }

}
