package com.example.services;

import com.example.model.Product;
import io.temporal.failure.ApplicationFailure;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.Map;
import java.util.concurrent.TimeUnit;

public class ProductService {
    private static final Logger logger = LoggerFactory.getLogger(ProductService.class);

    private static final Map<Integer, Product> products = Map.of(
            1, new Product(1, "Swordfish", 3500),
            2, new Product(2, "Burrata", 2000),
            3, new Product(3, "Potato", 1500),
            4, new Product(4, "Poke", 2000)
    );

    public static Product getProductById(int id) {
        Product product = products.get(id);
        if (product == null) {
            throw ApplicationFailure.newNonRetryableFailure(String.format("Product %s not found", id), "ProductNotFound");
        }

        CommonService.delay(50); // simulate delay

        return product;
    }
}
