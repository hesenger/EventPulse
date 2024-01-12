namespace EventPulse;

public interface IStreamPersistor
{
    void Persist(
        string streamName,
        object streamId,
        int revision,
        string eventType,
        string eventData
    );
    IEnumerable<EventResult> GetEvents(string streamName, object streamId);
}
