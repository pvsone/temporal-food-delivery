package com.example.services;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.concurrent.TimeUnit;

public class NotificationService {

    private static final Logger logger = LoggerFactory.getLogger(NotificationService.class);

    public static String sendNotification(String type, String message) {
        if (Math.random() < ServiceConstants.CHAOS_FACTOR) {
            throw new RuntimeException(String.format("Failed to send %s notification. Unable to reach notification service.", type));
        }

        try {
            TimeUnit.MILLISECONDS.sleep(100); // simulate delay
        } catch (InterruptedException e) {
            logger.error(e.getMessage());
        }

        logger.info("Sent notification {}, {}", type, message);
        return "success";
    }
}
