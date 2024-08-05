import asyncio
from datetime import timedelta, datetime

from temporalio import workflow
from temporalio.common import RetryPolicy
from temporalio.exceptions import ApplicationError

from activities import FoodDeliveryActivities

from shared_objects import OrderStatus, OrderStates, Product


@workflow.defn(name="order")
class OrderWorkflow:
    def __init__(self) -> None:
        self.order_status = None
        self.retry_policy = RetryPolicy(maximum_interval=timedelta(seconds=5))

    @workflow.run
    async def order(self, product_id: int) -> str:

        # lookup the product
        product = await workflow.execute_activity_method(
            FoodDeliveryActivities.get_product,
            product_id,
            start_to_close_timeout=timedelta(seconds=10),
            retry_policy=self.retry_policy
        )

        # charge the customer
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
            await self.send_push_notification(message)
            raise ApplicationError(message, type=e.type)
        self.order_status.state = OrderStates.PAID

        # wait for the order to be picked up, or timeout and refund
        try:
            await workflow.wait_condition(
                lambda: self.order_status.state == OrderStates.PICKED_UP,
                timeout=timedelta(seconds=30)
            )
        except asyncio.TimeoutError:
            self.order_status.state = OrderStates.REFUNDING
            await self.refund_and_notify(
                product,
                "⚠️  No drivers were available to pick up your order. Your payment has been refunded."
            )
            raise ApplicationError("Not picked up in time", type="NotPickedUpInTime")
        await self.send_push_notification("🚗  Order picked up")

        # wait for the order to be delivered, or timeout and refund
        try:
            await workflow.wait_condition(
                lambda: self.order_status.state == OrderStates.DELIVERED,
                timeout=timedelta(seconds=30)
            )
        except asyncio.TimeoutError:
            self.order_status.state = OrderStates.REFUNDING
            await self.refund_and_notify(
                product,
                "⚠️  Your driver was unable to deliver your order. Your payment has been refunded."
            )
            raise ApplicationError("Not delivered in time", type="NotDeliveredInTime")
        await self.send_push_notification("✅  Order delivered!")

        # wait for the customer to eat, then ask to rate the meal
        await asyncio.sleep(30)
        await self.send_push_notification("✍️  Rate your meal. How was the " + product.name + "?")

        return "success"

    @workflow.signal(name="pickedUp")
    def picked_up_signal(self):
        if self.order_status.state == OrderStates.PAID:
            self.order_status.state = OrderStates.PICKED_UP

    @workflow.signal(name="delivered")
    def delivered_signal(self):
        if self.order_status.state == OrderStates.PICKED_UP:
            self.order_status.state = OrderStates.DELIVERED
            self.order_status.delivered_at = workflow.now()

    @workflow.query(name="getStatus")
    def get_status_query(self) -> OrderStatus:
        return self.order_status

    async def refund_and_notify(self, product: Product, message: str):
        await workflow.execute_activity_method(
            FoodDeliveryActivities.refund_order,
            product,
            start_to_close_timeout=timedelta(seconds=10),
            retry_policy=self.retry_policy
        )
        await workflow.execute_activity_method(
            FoodDeliveryActivities.send_push_notification,
            message,
            start_to_close_timeout=timedelta(seconds=10),
            retry_policy=self.retry_policy
        )

    async def send_push_notification(self, message: str):
        await workflow.execute_activity_method(
            FoodDeliveryActivities.send_push_notification,
            message,
            start_to_close_timeout=timedelta(seconds=10),
            retry_policy=self.retry_policy
        )
