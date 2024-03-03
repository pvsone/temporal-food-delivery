using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoodDelivery;

#pragma warning disable SA1602 // Enumeration items should be documented
public enum OrderStates
{
    [Description("Charging card")]
    CHARGING_CARD,
    [Description("Paid")]
    PAID,
    [Description("Picked up")]
    PICKED_UP,
    [Description("Delivered")]
    DELIVERED,
    [Description("Refunding")]
    REFUNDING,
}

public class OrderStatesConverter : JsonConverter<OrderStates>
{
    public override OrderStates Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected a string but got {reader.TokenType}.");
        }

        var enumValue = reader.GetString();

        foreach (var enumItem in Enum.GetValues(typeToConvert))
        {
            if (enumItem is OrderStates myEnum && GetDescription(myEnum) == enumValue)
            {
                return myEnum;
            }
        }

        throw new JsonException($"Invalid value for {typeToConvert.Name}: {enumValue}");
    }

    public override void Write(Utf8JsonWriter writer, OrderStates value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(GetDescription(value));
    }

    private string GetDescription(OrderStates value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field != null ? (DescriptionAttribute?)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) : null;

        return attribute == null ? value.ToString() : attribute.Description;
    }
}
