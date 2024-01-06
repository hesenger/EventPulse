namespace EventPulse;

public interface IEventAggregatorProvider
{
    IEventAggregator<TAggregation> GetAggregator<TAggregation>();
}
