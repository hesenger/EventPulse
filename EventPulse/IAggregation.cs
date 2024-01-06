namespace EventPulse;

public interface IAggregation
{
    (object AggregationId, IReadOnlyList<object> Events) GetState();
}
