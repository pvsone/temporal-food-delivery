package com.example.services;

import io.temporal.activity.Activity;
import io.temporal.activity.ActivityInfo;
import io.temporal.failure.ApplicationFailure;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class PaymentService {
    private static final Logger logger = LoggerFactory.getLogger(PaymentService.class);

    public static String charge(int cents) {
        // In a real app, we would pass an idempotency token to the downstream service
        ActivityInfo info = Activity.getExecutionContext().getInfo();
        String idempotencyToken = info.getWorkflowId() + "-charge";
        logger.debug("Idempotency Token {}", idempotencyToken);

        if (cents >= 3500) {
            throw ApplicationFailure.newNonRetryableFailure("Card declined: insufficient funds", "InsufficientFunds");
        }

        if (Math.random() < ServiceConstants.CHAOS_FACTOR) {
            throw new RuntimeException("Failed to charge. Unable to reach payment service.");
        }

        logger.info("Charged {}", cents);
        return "success";
    }


    public static String refund(int cents) {
        // In a real app, we would pass an idempotency token to the downstream service
        ActivityInfo info = Activity.getExecutionContext().getInfo();
        String idempotencyToken = info.getWorkflowId() + "-refund";
        logger.debug("Idempotency Token {}", idempotencyToken);

        if (Math.random() < ServiceConstants.CHAOS_FACTOR) {
            throw new RuntimeException("Failed to refund. Unable to reach payment service.");
        }

        logger.info("Refunded {}", cents);
        return "success";
    }
}
