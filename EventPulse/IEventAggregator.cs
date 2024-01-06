namespace EventPulse;

public interface IEventAggregator<TAggregation>
{
    TAggregation Aggregate(TAggregation? aggregation, object evt);
}
