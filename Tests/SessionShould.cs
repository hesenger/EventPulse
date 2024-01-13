using System.Data.Common;
using EventPulse;

namespace Tests;

public class SessionShould
{
    private readonly SessionFactory _sessionFactory;

    public SessionShould()
    {
        var serializerProvider = new SerializerProvider();
        serializerProvider.Register("Booking", new BookingSerializer());

        _sessionFactory = new SessionFactory(serializerProvider, new InMemoryStreamPersistor());
    }

    public Session CreateSession() => _sessionFactory.Create();

    private long CreateBookingWithPartialPayment()
    {
        using var session = CreateSession();
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

        session.Complete();
        return evt.BookingId;
    }

    [Fact]
    public void RetrievePersistedEvents()
    {
        var id = CreateBookingWithPartialPayment();

        using (var session = CreateSession())
        {
            var booking = session.Find<Booking>("Booking", id);
            Assert.NotNull(booking);
            Assert.Equal(200, booking!.PendingAmount);
        }
    }

    [Fact]
    public void AppendEventsExistingStream()
    {
        var id = CreateBookingWithPartialPayment();

        // append events
        using (var session = CreateSession())
        {
            var booking = session.Find<Booking>("Booking", id);
            booking.Cancel(new V1.BookingCancelled(DateTime.Today));

            session.Complete();
        }

        // assert events committed were serialized and permanently added
        // to the session
        using (var session = CreateSession())
        {
            var booking = session.Find<Booking>("Booking", id);
            Assert.Equal(BookingStatus.Cancelled, booking!.Status);
        }
    }

    [Fact]
    public void RollbackChangesWhenNonCompleted()
    {
        var id = CreateBookingWithPartialPayment();

        using (var session = CreateSession())
        {
            var booking = session.Find<Booking>("Booking", id);
            booking.Cancel(new V1.BookingCancelled(DateTime.Today));
        }

        using (var session = CreateSession())
        {
            var booking = session.Find<Booking>("Booking", id);
            Assert.Equal(BookingStatus.Valid, booking!.Status);
        }
    }
}
