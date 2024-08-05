import asyncio
import concurrent.futures

from temporalio.client import Client
from temporalio.worker import Worker

from activities import FoodDeliveryActivities
from workflows import OrderWorkflow

TASK_QUEUE = "durable-delivery"


async def main():
    client = await Client.connect("localhost:7233")

    activities = FoodDeliveryActivities()

    worker = Worker(
        client,
        task_queue=TASK_QUEUE,
        workflows=[OrderWorkflow],
        activities=[
            activities.get_product,
            activities.charge_customer,
            activities.refund_order,
            activities.send_push_notification
        ],
    )
    await worker.run()


if __name__ == "__main__":
    asyncio.run(main())
