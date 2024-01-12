namespace EventPulse;

public record EventEntry(string StreamName, object StreamId, int Revision, object Event);
