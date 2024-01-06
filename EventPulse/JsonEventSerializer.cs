using System.Text.Json;

namespace EventPulse;

public class JsonEventSerializer : IEventSerializer
{
    public string Serialize(object evt)
    {
        return JsonSerializer.Serialize(evt);
    }

    public object Deserialize(string eventType, string eventData)
    {
        var types =
            from ass in AppDomain.CurrentDomain.GetAssemblies()
            where ass.GetType(eventType) != null
            select ass.GetType(eventType);

        var type =
            types.FirstOrDefault() ?? throw new NotSupportedException("Event type not found");

        return JsonSerializer.Deserialize(eventData, type)!;
    }
}
