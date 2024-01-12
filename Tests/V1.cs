namespace Tests;

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
