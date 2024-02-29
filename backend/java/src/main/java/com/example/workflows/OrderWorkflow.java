package com.example.workflows;

import com.example.model.OrderStatus;
import io.temporal.workflow.QueryMethod;
import io.temporal.workflow.SignalMethod;
import io.temporal.workflow.WorkflowInterface;
import io.temporal.workflow.WorkflowMethod;

@WorkflowInterface
public interface OrderWorkflow {

    @WorkflowMethod(name = "order")
    String order(int productId);

    @SignalMethod(name = "pickedUp")
    void pickedUpSignal();

    @SignalMethod(name = "delivered")
    void deliveredSignal();

    @QueryMethod(name = "getStatus")
    OrderStatus getStatusQuery();
}
