from random import random
from time import sleep
import logging

from temporalio import activity
from temporalio.exceptions import ApplicationError

from shared_objects import Product

logging.basicConfig(level=logging.INFO)
CHAOS_FACTOR = 0.5


class FoodDeliveryActivities:

    @activity.defn
    async def get_product(self, product_id: int) -> Product:
        products = {
            1: Product(1, "Swordfish", 3500),
            2: Product(2, "Burrata", 2000),
            3: Product(3, "Potato", 1500),
            4: Product(4, "Poke", 2000)
        }

        sleep(0.05)

        product = products.get(product_id)
        if product is None:
            raise ApplicationError("Product " + str(id) + " not found", type="ProductNotFound", non_retryable=True)
        return product

    @activity.defn
    async def send_push_notification(self, message: str) -> str:
        if random() < CHAOS_FACTOR:
            raise ApplicationError("Failed to send Push notification. Unable to reach notification service.")

        sleep(0.05)

        activity.logger.info("Sent notification push, %s" % message)
        return "success"

    @activity.defn
    async def refund_order(self, product: Product) -> str:
        info = activity.info()
        idempotency_token = info.workflow_id + "-refund"
        activity.logger.debug("Idempotency Token %s" % idempotency_token)

        if random() < CHAOS_FACTOR:
            raise ApplicationError("Failed to refund. Unable to reach payment service.")

        sleep(0.05)

        activity.logger.info("Refunded %s" % product.cents)
        return "success"

    @activity.defn
    async def charge_customer(self, product: Product) -> str:
        info = activity.info()
        idempotency_token = info.workflow_id + "-charge"
        activity.logger.debug("Idempotency Token %s" % idempotency_token)

        if product.cents >= 3500:
            raise ApplicationError("Card declined: insufficient funds", type="InsufficientFunds", non_retryable=True)

        if random() < CHAOS_FACTOR:
            raise ApplicationError("Failed to charge customer. Unable to reach payment service.")

        sleep(0.05)

        activity.logger.info("Charged %s" % product.cents)
        return "success"
