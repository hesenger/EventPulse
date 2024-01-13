namespace EventPulse;

public class EventStream
{
    private readonly List<object> _events = new();
    private readonly string _streamName;
    private readonly object _streamId;

    public EventStream(string streamName, object streamId)
    {
        _streamName = streamName;
        _streamId = streamId;
    }

    public void Append(object evt)
    {
        _events.Add(evt);
        if (Session.Current.IsHydrating)
        {
            return;
        }

        Session.Current.Track(new EventEntry(_streamName, _streamId, _events.Count, evt));
    }
}
