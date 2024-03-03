package com.example.model;

import com.fasterxml.jackson.annotation.JsonValue;

public enum OrderStates {
    CHARGING_CARD("Charging card"),
    PAID("Paid"),
    PICKED_UP("Picked up"),
    DELIVERED("Delivered"),
    REFUNDING("Refunding"),
    FAILED("Failed");

    private final String displayValue;

    OrderStates(String displayValue) {
        this.displayValue = displayValue;
    }

    @JsonValue
    public String getDisplayValue() {
        return displayValue;
    }

    @Override
    public String toString() {
        return displayValue;
    }
}
