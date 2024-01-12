using EventPulse;

namespace Tests;

public class InMemoryStreamPersistor : IStreamPersistor
{
    private readonly List<EventResult> _events = new();

    public IEnumerable<EventResult> GetEvents(string streamName, object streamId)
    {
        return _events
            .Where(evt => evt.StreamName.Equals(streamName) && evt.StreamId.Equals(streamId))
            .OrderBy(evt => evt.Revision);
    }

    public void Persist(
        string streamName,
        object streamId,
        int revision,
        string eventType,
        string eventData
    )
    {
        _events.Add(new EventResult(streamName, streamId, revision, eventType, eventData));
    }
}
