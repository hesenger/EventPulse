namespace EventPulse;

public class SerializerProvider
{
    private readonly Dictionary<string, IStreamSerializer> _serializers = new();

    public void Register(string streamName, IStreamSerializer serializer)
    {
        _serializers.Add(streamName, serializer);
    }

    public IStreamSerializer GetSerializer(string streamName)
    {
        return (IStreamSerializer)_serializers[streamName];
    }
}
