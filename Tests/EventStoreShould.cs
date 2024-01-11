using System.Collections.Concurrent;
using System.Diagnostics.Tracing;
using System.Text.Json;
using System.Transactions;
using EventPulse;
using Microsoft.Identity.Client;
using Microsoft.VisualBasic;
using NSubstitute;

namespace Tests;

public class EventStoreShould
{
    private EventStore CreateDefaultEventStore(IEventPersistor? persistor = null)
    {
        var provider = new ListAggregatorProvider();
        provider.Register(new PersonEventAggregator());

        return new EventStore(persistor ?? new InMemoryEventPersistor(), provider);
    }

    [Fact]
    public async Task RetrieveAggregateRootFullyRestored()
    {
        var eventStore = CreateDefaultEventStore();
        var person = new Person(PersonCreatedEvent.Create("John"));
        person.UpdateName(new PersonNameUpdatedEvent("John Doe"));
        await eventStore.Save(person);

        var restoredPerson = await eventStore.Find<Person>(person.Id);
        Assert.Equal("John Doe", restoredPerson!.Name);
    }

    [Fact]
    public async Task PersistEventsInPersistor()
    {
        var eventPersistor = Substitute.ForPartsOf<InMemoryEventPersistor>();
        var eventStore = CreateDefaultEventStore(eventPersistor);
        var person = new Person(PersonCreatedEvent.Create("John"));
        person.UpdateName(new PersonNameUpdatedEvent("John Doe"));
        await eventStore.Save(person);

        await eventPersistor.Received().Persist(person.Id, Arg.Any<PersonCreatedEvent>());
        await eventPersistor.Received().Persist(person.Id, Arg.Any<PersonNameUpdatedEvent>());
    }

    [Fact]
    public async Task RetrieveNewInstanteOfPerson()
    {
        var eventStore = CreateDefaultEventStore();
        var person = new Person(PersonCreatedEvent.Create("John"));
        person.UpdateName(new PersonNameUpdatedEvent("John Doe"));
        await eventStore.Save(person);

        var restoredPerson = await eventStore.Find<Person>(person.Id);
        Assert.NotNull(restoredPerson);
        Assert.NotSame(person, restoredPerson);
    }

    [Fact]
    public async Task RetrieveNullWhenAggregateRootNotFound()
    {
        var eventStore = CreateDefaultEventStore();
        var restoredPerson = await eventStore.Find<Person>(Guid.NewGuid());
        Assert.Null(restoredPerson);
    }
}

public class SessionShould
{
    [Fact]
    public void PersistEvents()
    {
        long id;
        using (var session = new Session())
        {
            var evt = V1.BookingCreated.Create(1L, 1L, DateTime.Today, DateTime.Today.AddDays(3));
            var booking = new Booking(evt);
            id = evt.BookingId;

            session.Complete();
        }

        using (var session = new Session())
        {
            var booking = session.Find<Booking>(id);
            Assert.NotNull(booking);
        }
    }
}

public class BookingSerializer
{
    public Booking Aggregate(Booking? booking, object evt)
    {
        switch (evt)
        {
            case V1.BookingCreated created:
                return new Booking(created);
            default:
                throw new NotSupportedException($"Event {evt.GetType()} not supported");
        }
    }

    public (string eventType, string eventData) Serialize(object evt)
    {
        return evt switch
        {
            V1.BookingCreated created => ("V1.BookingCreated", JsonSerializer.Serialize(created)),
            _ => throw new NotSupportedException($"Event {evt.GetType()} not supported")
        };
    }

    public object Deserialize(string eventType, string evt)
    {
        return eventType switch
        {
            "BookingCreated" => JsonSerializer.Deserialize<V1.BookingCreated>(evt)!,
            _ => throw new NotSupportedException($"Event {eventType} not supported")
        };
    }
}

public static class Generator
{
    public static ConcurrentDictionary<Type, long> _sequences = new();

    public static long Next<Type>() =>
        _sequences.AddOrUpdate(typeof(Type), 1, (_, current) => current + 1);
}

public static class V1
{
    public record BookingCreated(
        long BookingId,
        long RoomId,
        long GuestId,
        DateTime CheckIn,
        DateTime CheckOut
    )
    {
        public static BookingCreated Create(
            long roomId,
            long guestId,
            DateTime checkIn,
            DateTime checkOut
        )
        {
            return new(Generator.Next<BookingCreated>(), roomId, guestId, checkIn, checkOut);
        }
    }
}

public class Booking
{
    private readonly EventList _events;
    private long _id;
    private long _roomId;
    private long _guestId;
    private DateTime _checkIn;
    private DateTime _checkOut;

    public Booking(V1.BookingCreated evt)
    {
        (_id, _roomId, _guestId, _checkIn, _checkOut) = evt;
        _events = new EventList("Booking", 1);
        _events.Append(evt);
    }
}

public static class Database
{
    public static List<object[]> EventsTable = new();
}

public class Session : IDisposable
{
    private static readonly Dictionary<Transaction, Session> _scopes = new();

    public static Session Current => _scopes[Transaction.Current!];

    public bool IsHydrating { get; private set; }

    private readonly TransactionScope _transactionScope;
    private readonly List<EventEntry> _notPersisted = new();

    public Session()
    {
        _transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew);
        _scopes.Add(Transaction.Current!, this);
    }

    public void Track(EventEntry eventEntry) => _notPersisted.Add(eventEntry);

    public void Complete() => _transactionScope.Complete();

    public void Dispose()
    {
        var serializer = new BookingSerializer();
        Database
            .EventsTable
            .AddRange(
                _notPersisted.Select(
                    evt =>
                        new object[]
                        {
                            evt.StreamName,
                            evt.StreamId,
                            evt.Revision,
                            serializer.Serialize(evt.Event)
                        }
                )
            );

        _transactionScope.Dispose();
    }

    public TStream Find<TStream>(long id)
    {
        IsHydrating = true;
        var serializer = new BookingSerializer();
        var events = Database
            .EventsTable
            .Where(evt => evt[0].Equals(typeof(TStream).Name) && evt[1].Equals(id))
            .OrderBy(evt => evt[2])
            .Select(evt => serializer.Deserialize((string)evt[3]!, (string)evt[4]!))
            .ToList();

        object? stream = null;
        foreach (var evt in events)
            stream = serializer.Aggregate((Booking?)stream, evt);

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
