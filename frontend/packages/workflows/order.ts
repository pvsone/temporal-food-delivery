import { defineSignal, defineQuery } from '@temporalio/workflow'

type OrderState = 'Charging card' | 'Paid' | 'Picked up' | 'Delivered' | 'Refunding'

export interface OrderStatus {
  productId: number
  state: OrderState
  deliveredAt?: Date
}

export const pickedUpSignal = defineSignal('pickedUp')
export const deliveredSignal = defineSignal('delivered')
export const getStatusQuery = defineQuery<OrderStatus>('getStatus')
