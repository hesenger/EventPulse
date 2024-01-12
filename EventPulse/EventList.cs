namespace EventPulse;

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
