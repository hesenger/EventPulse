namespace EventPulse;

public interface IEventSerializer
{
    string Serialize(object evt);
    object Deserialize(string eventType, string eventData);
}
