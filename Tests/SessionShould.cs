namespace Tests;

public class SessionShould
{
    [Fact]
    public void RetrievePersistedEvents()
    {
        long id;
        using (var session = new Session())
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

        using (var session = new Session())
        {
            var booking = session.Find<Booking>("Booking", id);
            Assert.NotNull(booking);
            Assert.Equal(200, booking!.PendingAmount);
        }
    }
}
