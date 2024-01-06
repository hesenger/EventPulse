namespace EventPulse;

public class EventStore
{
    private readonly IEventPersistor _persistor;
    private readonly IEventAggregatorProvider _aggregatorProvider;

    public EventStore(IEventPersistor eventPersistor, IEventAggregatorProvider aggregatorProvider)
    {
        _persistor = eventPersistor;
        _aggregatorProvider = aggregatorProvider;
    }

    public async Task<TAggregateRoot?> Find<TAggregateRoot>(object id)
    {
        var events = await _persistor.GetEvents(id);
        var aggregator = _aggregatorProvider.GetAggregator<TAggregateRoot>();
        var aggregation = events.Aggregate(default(TAggregateRoot?), aggregator.Aggregate);
        return aggregation;
    }

    public async Task Save<TAggregateRoot>(TAggregateRoot aggregateRoot)
        where TAggregateRoot : IAggregation
    {
        await Task.CompletedTask;
        var (id, events) = aggregateRoot.GetState();
        events.ToList().ForEach(evt => _persistor.Persist(id, evt));
    }
}
