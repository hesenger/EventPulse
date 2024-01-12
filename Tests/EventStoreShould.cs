using System.Collections.Concurrent;
using System.Text.Json;
using System.Transactions;
using EventPulse;
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

public class BookingSerializer
{
    public Booking Aggregate(Booking? booking, object evt)
    {
        switch (evt)
        {
            case V1.BookingCreated created:
                return new Booking(created);

            case V1.BookingPaid paid:
                booking!.RegisterPayment(paid);
                return booking;

            default:
                throw new NotSupportedException($"Event {evt.GetType()} not supported");
        }
    }

    public (string eventType, string eventData) Serialize(object evt)
    {
        return evt switch
        {
            V1.BookingCreated created => ("V1.BookingCreated", JsonSerializer.Serialize(created)),
            V1.BookingPaid paid => ("V1.BookingPaid", JsonSerializer.Serialize(paid)),
            _ => throw new NotSupportedException($"Event {evt.GetType()} not supported")
        };
    }

    public object Deserialize(string eventType, string evt)
    {
        return eventType switch
        {
            "V1.BookingCreated" => JsonSerializer.Deserialize<V1.BookingCreated>(evt)!,
            "V1.BookingPaid" => JsonSerializer.Deserialize<V1.BookingPaid>(evt)!,
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
    public record BookingPaid(decimal AmountPaid, DateTime PaidAt);

    public record BookingCreated(
        long BookingId,
        long RoomId,
        long GuestId,
        DateTime CheckIn,
        DateTime CheckOut,
        decimal TotalPrice,
        decimal AmountPaid
    )
    {
        public static BookingCreated Create(
            long roomId,
            long guestId,
            DateTime checkIn,
            DateTime checkOut,
            decimal totalPrice,
            decimal amountPaid
        )
        {
            return new(
                Generator.Next<BookingCreated>(),
                roomId,
                guestId,
                checkIn,
                checkOut,
                totalPrice,
                amountPaid
            );
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
    private decimal _totalPrice;
    private decimal _amountPaid;

    public decimal PendingAmount => _totalPrice - _amountPaid;

    public Booking(V1.BookingCreated created)
    {
        (_id, _roomId, _guestId, _checkIn, _checkOut, _totalPrice, _amountPaid) = created;
        _events = new EventList("Booking", created.BookingId);
        _events.Append(created);
    }

    public void RegisterPayment(V1.BookingPaid payment)
    {
        if (_amountPaid + payment.AmountPaid > _totalPrice)
            throw new InvalidOperationException("Amount paid is greater than total price");

        _amountPaid += payment.AmountPaid;
        _events.Append(payment);
    }
}

public static class Database
{
    public static List<object[]> EventsTable = new();
}
