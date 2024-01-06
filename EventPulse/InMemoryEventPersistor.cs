namespace EventPulse;

public class InMemoryEventPersistor : IEventPersistor
{
    private readonly Dictionary<object, List<object>> _events = new();

    public Task Persist(object aggregationId, object evt)
    {
        if (!_events.ContainsKey(aggregationId))
            _events.Add(aggregationId, new List<object>());

        _events[aggregationId].Add(evt);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<object>> GetEvents(object aggregationId)
    {
        if (!_events.ContainsKey(aggregationId))
            return Task.FromResult(Enumerable.Empty<object>());

        return Task.FromResult(_events[aggregationId].AsEnumerable());
    }
}
