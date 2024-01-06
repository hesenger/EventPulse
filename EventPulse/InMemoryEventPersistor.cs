namespace EventPulse;

public class InMemoryEventPersistor : IEventPersistor
{
    private readonly Dictionary<object, List<object>> _events = new();

    public Task Persist(object stateHolderId, object evt)
    {
        if (!_events.ContainsKey(stateHolderId))
            _events.Add(stateHolderId, new List<object>());

        _events[stateHolderId].Add(evt);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<object>> GetEvents(object stateHolderId)
    {
        if (!_events.ContainsKey(stateHolderId))
            return Task.FromResult(Enumerable.Empty<object>());

        return Task.FromResult(_events[stateHolderId].AsEnumerable());
    }
}
