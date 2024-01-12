namespace EventPulse;

public interface IStreamSerializer
{
    (string eventType, string eventData) Serialize(object @event);
    object Deserialize(string eventType, string eventData);
    object Aggregate(object? stream, object evt);
}
