using System.Collections.Concurrent;

namespace Tests;

/// <summary>
/// An in memory sequnce generator for testing purposes, in production consider
/// using NSequence for integer sequences or a GUID generator.
/// </summary>
public static class Generator
{
    public static ConcurrentDictionary<Type, long> _sequences = new();

    public static long Next<Type>() =>
        _sequences.AddOrUpdate(typeof(Type), 1, (_, current) => current + 1);
}
