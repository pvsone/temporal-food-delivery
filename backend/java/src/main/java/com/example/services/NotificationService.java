package com.example.services;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.concurrent.TimeUnit;

public class NotificationService {

    private static final Logger logger = LoggerFactory.getLogger(NotificationService.class);

    public static String sendNotification(String type, String message) {
        if (Math.random() < CommonService.CHAOS_FACTOR) {
            throw new RuntimeException(String.format("Failed to send %s notification. Unable to reach notification service.", type));
        }

        CommonService.delay(50); // simulate delay

        logger.info("Sent notification {}, {}", type, message);
        return "success";
    }
}
