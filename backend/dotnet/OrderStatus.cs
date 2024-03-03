using System.Text.Json.Serialization;

namespace FoodDelivery;

public class OrderStatus
{
    [JsonPropertyName("productId")]
    public int ProductId { get; set; }

    [JsonPropertyName("state")]
    [JsonConverter(typeof(OrderStatesConverter))]
    public OrderStates State { get; set; }

    [JsonPropertyName("deliveredAt")]
    public DateTime? DeliveredAt { get; set; }

    public OrderStatus(int productId, OrderStates state, DateTime? deliveredAt)
    {
        ProductId = productId;
        State = state;
        DeliveredAt = deliveredAt;
    }
}