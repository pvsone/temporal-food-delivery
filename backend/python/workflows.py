import asyncio
from datetime import timedelta, datetime

from temporalio import workflow
from temporalio.common import RetryPolicy
from temporalio.exceptions import ApplicationError

from activities import FoodDeliveryActivities

from shared_objects import OrderStatus, OrderStates, Product


@workflow.defn
class OrderWorkflow:
    def __init__(self) -> None:
        self.order_status = OrderStatus()
        self.retry_policy = RetryPolicy(maximum_interval=timedelta(seconds=5))

    @workflow.run
    async def order(self, product_id: int) -> str:

        product = await workflow.execute_activity_method(
            FoodDeliveryActivities.get_product,
            product_id,
            start_to_close_timeout=timedelta(seconds=10),
            retry_policy=self.retry_policy
        )

        self.order_status = OrderStatus(product_id, OrderStates.CHARGING_CARD, None)
        try:
            await workflow.execute_activity_method(
                FoodDeliveryActivities.charge_customer,
                product,
                start_to_close_timeout=timedelta(seconds=10),
                retry_policy=self.retry_policy
            )
        except ApplicationError as e:
            self.order_status.state = OrderStates.FAILED
            message = "Failed to charge customer for " + product.name + ". Error: " + e.message
            await workflow.execute_activity_method(
                FoodDeliveryActivities.send_push_notification,
                message,
                start_to_close_timeout=timedelta(seconds=10),
                retry_policy=self.retry_policy
            )
            raise ApplicationError(message, type=e.type)
        self.order_status.state = OrderStates.PAID

        not_picked_up_in_time = not workflow.wait_condition(
            lambda: self.order_status.state == OrderStates.DELIVERED,
            timeout=timedelta(seconds=30)
        )
        if not_picked_up_in_time:
            self.order_status.state = OrderStates.REFUNDING
            self.refund_and_notify(
                product,
                "⚠️  Your driver was unable to deliver your order. Your payment has been refunded."
            )
            raise ApplicationError("Not delivered in time", type="NotDeliveredInTime")
        await workflow.execute_activity_method(
            FoodDeliveryActivities.send_push_notification,
            "✅  Order delivered!",
            start_to_close_timeout=timedelta(seconds=10),
            retry_policy=self.retry_policy
        )

        await asyncio.sleep(30)
        await workflow.execute_activity_method(
            FoodDeliveryActivities.send_push_notification,
            "✍️  Rate your meal. How was the " + product.name + "?",
            start_to_close_timeout=timedelta(seconds=10),
            retry_policy=self.retry_policy
        )

        return "success"

    @workflow.signal
    def picked_up_signal(self):
        if self.order_status.state == OrderStates.PAID:
            self.order_status.state = OrderStates.PICKED_UP

    @workflow.signal
    def delivered_signal(self):
        if self.order_status.state == OrderStates.PICKED_UP:
            self.order_status.state = OrderStates.DELIVERED
            self.order_status.delivered_at = datetime.now()

    @workflow.query
    def get_status_query(self) -> OrderStatus:
        return self.order_status

    def refund_and_notify(self, product: Product, message: str):
        workflow.execute_activity_method(
            FoodDeliveryActivities.refund_order,
            product,
            start_to_close_timeout=timedelta(seconds=10),
            retry_policy=self.retry_policy
        )
        workflow.execute_activity_method(
            FoodDeliveryActivities.send_push_notification,
            message,
            start_to_close_timeout=timedelta(seconds=10),
            retry_policy=self.retry_policy
        )
