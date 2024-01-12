namespace EventPulse;

public class SessionFactory(SerializerProvider serializerProvider, IStreamPersistor persistor)
{
    public Session Create() => new(serializerProvider, persistor);
}
