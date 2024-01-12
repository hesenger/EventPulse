using System.Transactions;

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

public record EventResult(
    string StreamName,
    object StreamId,
    int Revision,
    string EventType,
    string EventData
);

public interface IStreamSerializer
{
    (string eventType, string eventData) Serialize(object @event);
    object Deserialize(string eventType, string eventData);
    object Aggregate(object? stream, object evt);
}

public class SerializerProvider
{
    private readonly Dictionary<string, IStreamSerializer> _serializers = new();

    public void Register(string streamName, IStreamSerializer serializer)
    {
        _serializers.Add(streamName, serializer);
    }

    public IStreamSerializer GetSerializer(string streamName)
    {
        return (IStreamSerializer)_serializers[streamName];
    }
}

public class SessionFactory(SerializerProvider serializerProvider, IStreamPersistor persistor)
{
    public Session Create() => new(serializerProvider, persistor);
}

public class Session : IDisposable
{
    private static readonly Dictionary<Transaction, Session> _scopes = new();

    public static Session Current => _scopes[Transaction.Current!];

    public bool IsHydrating { get; private set; }

    private readonly SerializerProvider _serializerProvider;
    private readonly IStreamPersistor _persistor;
    private readonly TransactionScope _transactionScope;
    private readonly List<EventEntry> _notPersisted = new();

    public Session(SerializerProvider serializerProvider, IStreamPersistor persistor)
    {
        _serializerProvider = serializerProvider;
        _persistor = persistor;
        _transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew);
        _scopes.Add(Transaction.Current!, this);
    }

    public void Track(EventEntry eventEntry) => _notPersisted.Add(eventEntry);

    public void Complete() => _transactionScope.Complete();

    public void Dispose()
    {
        _notPersisted.ForEach(evt =>
        {
            var serializer = _serializerProvider.GetSerializer(evt.StreamName);
            var (eventType, eventData) = serializer.Serialize(evt.Event);
            _persistor.Persist(evt.StreamName, evt.StreamId, evt.Revision, eventType, eventData);
        });

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

public record EventEntry(string StreamName, object StreamId, int Revision, object Event);

public class EventList
{
    private readonly List<object> _events = new();
    public string StreamName { get; }
    public object StreamId { get; }
    private bool _hydrated = true;
    private int _revision;

    public EventList(string streamName, object streamId)
    {
        StreamName = streamName;
        StreamId = streamId;
    }

    public void Append(object evt)
    {
        _events.Add(evt);
        if (Session.Current.IsHydrating)
        {
            _hydrated = false;
            return;
        }

        if (!_hydrated)
        {
            _hydrated = true;
            _revision = _events.Count;
        }

        _revision++;
        Session.Current.Track(new EventEntry(StreamName, StreamId, _revision, evt));
    }
}
