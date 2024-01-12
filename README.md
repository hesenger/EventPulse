# EventPulse

EventPulse provides a thin layer between your application and the event persistence
for Event Sourcing systems. It's designed to be less intrusive as possible and
avoid totally reflection (for sure we use Json serialization though), which means
the real methods are used to retrieve the aggregations based in the events stored
previously.

## Code

The library was written since the initial commit using TDD, so code coverage is not
a problem, furthermore, you can check the specs written to quickly undestand the
usage and advanced features.

## Persistence support

The events can be stored using the following implementations:

- InMemoryEventPersistor: useful for testing purposes;
- SqlServerEventPersistor: using a single append-only table on SQL Server;
- PostgreSqlEventPersistor: **planned**;

Custom implementations can be easily made by implementing `IEventPersistor`.

## Building it locally

Just clone this repo and run the following commands available the Makefile, or
use them as reference to run the commands manually.

- `make startdb`: start docker instances for database tests;
- `make build`: build the library;
- `make test`: run tests and if success, generate the coverage report;
- `make stopdb`: stop docker instances;

## Usage

For web apps you likely want to register the SessionFactory instance in your
services provider.

```csharp
long id;
using (var session = sessionFactory.CreateSession())
{
    var evt = V1.BookingCreated.Create(
        1L,
        1L,
        DateTime.Today,
        DateTime.Today.AddDays(3),
        300,
        0
    );
    var booking = new Booking(evt);
    booking.RegisterPayment(new V1.BookingPaid(100, DateTime.Today));
    id = evt.BookingId;

    session.Complete();
}

using (var session = CreateSession())
{
    var booking = session.Find<Booking>("Booking", id);
    Assert.NotNull(booking);
    Assert.Equal(200, booking!.PendingAmount);
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
    public BookingStatus Status { get; private set; }

    public decimal PendingAmount => _totalPrice - _amountPaid;

    public Booking(V1.BookingCreated created)
    {
        (_id, _roomId, _guestId, _checkIn, _checkOut, _totalPrice, _amountPaid) = created;
        Status = BookingStatus.Valid;

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

    public void Cancel(V1.BookingCancelled cancelled)
    {
        Status = BookingStatus.Cancelled;
        _events.Append(cancelled);
    }
}
```
