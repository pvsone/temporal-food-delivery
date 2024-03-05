package com.example.services;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.concurrent.TimeUnit;

public class CommonService {
    public static final double CHAOS_FACTOR = 0.5;

    private static final Logger logger = LoggerFactory.getLogger(CommonService.class);

    public static void delay(int ms) {
        try {
            TimeUnit.MILLISECONDS.sleep(ms);
        } catch (InterruptedException e) {
            logger.error(e.getMessage());
        }
    }
}
