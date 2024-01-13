using System.Transactions;

namespace EventPulse;

public class Session : IDisposable
{
    private static readonly Dictionary<Transaction, Session> _scopes = new();

    public static Session Current => _scopes[Transaction.Current!];

    public bool IsHydrating { get; private set; }

    private readonly SerializerProvider _serializerProvider;
    private readonly IStreamPersistor _persistor;
    private readonly TransactionScope _transactionScope;
    private readonly List<EventEntry> _notPersisted = new();
    private bool _complete = false;

    public Session(SerializerProvider serializerProvider, IStreamPersistor persistor)
    {
        _serializerProvider = serializerProvider;
        _persistor = persistor;
        _transactionScope = CreateScope();
        _scopes.Add(Transaction.Current!, this);
    }

    private TransactionScope CreateScope()
    {
        return new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions
            {
                Timeout = TimeSpan.FromSeconds(3),
                IsolationLevel = IsolationLevel.ReadCommitted
            },
            TransactionScopeAsyncFlowOption.Enabled
        );
    }

    public void Track(EventEntry eventEntry) => _notPersisted.Add(eventEntry);

    public void Complete() => _complete = true;

    public void Dispose()
    {
        if (_complete)
        {
            _notPersisted.ForEach(evt =>
            {
                var serializer = _serializerProvider.GetSerializer(evt.StreamName);
                var (eventType, eventData) = serializer.Serialize(evt.Event);
                _persistor.Persist(
                    evt.StreamName,
                    evt.StreamId,
                    evt.Revision,
                    eventType,
                    eventData
                );
            });

            _transactionScope.Complete();
        }

        _transactionScope.Dispose();
    }

    public TStream Find<TStream>(string streamName, long id)
    {
        IsHydrating = true;
        var serializer = _serializerProvider.GetSerializer(streamName);
        var events = _persistor
            .GetEvents(streamName, id)
            .Select(evt => serializer.Deserialize(evt.EventType, evt.EventData));

        object? stream = null;
        foreach (var evt in events)
            stream = serializer.Aggregate(stream, evt);

        IsHydrating = false;
        return (TStream)stream!;
    }
}
