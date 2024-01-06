namespace EventPulse;

public class ListAggregatorProvider : IEventAggregatorProvider
{
    private readonly Dictionary<Type, object> _aggregators = new();

    public void Register<TAggregation>(IEventAggregator<TAggregation> aggregator)
    {
        _aggregators.Add(typeof(TAggregation), aggregator);
    }

    public IEventAggregator<TAggregation> GetAggregator<TAggregation>()
    {
        return (IEventAggregator<TAggregation>)_aggregators[typeof(TAggregation)];
    }
}
