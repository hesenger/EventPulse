using System.Text.Json;
using EventPulse;

namespace Tests;

public class BookingSerializer : IStreamSerializer
{
    public object Aggregate(object? stream, object evt)
    {
        var booking = (Booking?)stream;
        switch (evt)
        {
            case V1.BookingCreated created:
                return new Booking(created);

            case V1.BookingPaid paid:
                booking!.RegisterPayment(paid);
                return booking;

            case V1.BookingCancelled cancelled:
                booking!.Cancel(cancelled);
                return booking;

            default:
                throw new NotSupportedException($"Event {evt.GetType()} not supported");
        }
    }

    public (string eventType, string eventData) Serialize(object evt)
    {
        return evt switch
        {
            V1.BookingCreated created => ("V1.BookingCreated", JsonSerializer.Serialize(created)),
            V1.BookingPaid paid => ("V1.BookingPaid", JsonSerializer.Serialize(paid)),
            V1.BookingCancelled cancelled
                => ("V1.BookingCancelled", JsonSerializer.Serialize(cancelled)),
            _ => throw new NotSupportedException($"Event {evt.GetType()} not supported")
        };
    }

    public object Deserialize(string eventType, string evt)
    {
        return eventType switch
        {
            "V1.BookingCreated" => JsonSerializer.Deserialize<V1.BookingCreated>(evt)!,
            "V1.BookingPaid" => JsonSerializer.Deserialize<V1.BookingPaid>(evt)!,
            "V1.BookingCancelled" => JsonSerializer.Deserialize<V1.BookingCancelled>(evt)!,
            _ => throw new NotSupportedException($"Event {eventType} not supported")
        };
    }
}
