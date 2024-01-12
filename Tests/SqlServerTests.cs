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
            new SqlServerStreamPersistor(fixture.DatabaseProvider)
        );
    }

    public Session CreateSession() => _sessionFactory.Create();

    [Fact]
    public void RetrievePersistedEvents()
    {
        long id;
        using (var session = CreateSession())
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
    }
}
