namespace EventPulse;

public record EventResult(
    string StreamName,
    object StreamId,
    int Revision,
    string EventType,
    string EventData
);
