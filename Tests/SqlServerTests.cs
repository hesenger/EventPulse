using EventPulse;

namespace Tests;

[Collection("SqlServerCollection")]
public class SqlServerTests
{
    private readonly SessionFactory _sessionFactory;

    public SqlServerTests(SqlServerFixture fixture)
    {
        var serializerProvider = new SerializerProvider();
        serializerProvider.Register("Booking", new BookingSerializer());

        _sessionFactory = new SessionFactory(
            serializerProvider,
            new SqlServerStreamPersistor(fixture.ConnectionProvider)
        );
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

        using var session = CreateSession();
        var booking = session.Find<Booking>("Booking", id);
        Assert.NotNull(booking);
        Assert.Equal(200, booking!.PendingAmount);
    }

    [Fact]
    public async Task PerformAsyncCallsInsideSession()
    {
        var id = CreateBookingWithPartialPayment();

        using (var session = CreateSession())
        {
            var booking = session.Find<Booking>("Booking", id);
            await RegisterCancellationAsync(booking);

            session.Complete();
        }

        using (var session = CreateSession())
        {
            var booking = session.Find<Booking>("Booking", id);
            Assert.Equal(BookingStatus.Cancelled, booking.Status);
        }
    }

    private async Task RegisterCancellationAsync(Booking booking)
    {
        await Task.Delay(100);
        booking.Cancel(new V1.BookingCancelled(DateTime.Today));
    }
}
