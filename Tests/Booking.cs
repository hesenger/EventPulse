using EventPulse;

namespace Tests;

public class Booking
{
    private readonly EventStream _events;
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

        _events = new EventStream("Booking", created.BookingId);
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
