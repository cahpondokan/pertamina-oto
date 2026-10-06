namespace Pertamina.Oto.Desktop.App;

// Session identity is deliberately not part of the worker protocol.
internal sealed record OrganizerPage(long Id, int Source, int Page, int Rotation = 0)
{
    private static long nextId;
    internal static OrganizerPage Create(int source, int page, int rotation = 0) => new(Interlocked.Increment(ref nextId), source, page, rotation);
    internal OrganizerPage Duplicate() => Create(Source, Page, Rotation);
}

// A drag preview never mutates the committed list. Escape simply discards this transaction.
internal sealed class OrganizerDragOrder(IEnumerable<OrganizerPage> pages, long movingId)
{
    internal OrganizerPage[] Original { get; } = pages.ToArray();
    internal long MovingId { get; } = movingId;
    internal int Target { get; private set; } = Array.FindIndex(pages.ToArray(), p => p.Id == movingId);
    internal OrganizerPage[] Preview(int target)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(target);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(target, Original.Length);
        var moving = Original.Single(p => p.Id == MovingId);
        Target = target;
        var result = Original.Where(p => p.Id != MovingId).ToList();
        result.Insert(target, moving);
        return result.ToArray();
    }
}

internal static class OrganizerOrder
{
    internal static int Destination(int source, int insertion, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
        if (source < 0 || source >= count) throw new ArgumentOutOfRangeException(nameof(source));
        if (insertion < 0 || insertion > count) throw new ArgumentOutOfRangeException(nameof(insertion));
        return insertion > source ? insertion - 1 : insertion;
    }
}
