namespace Pertamina.Oto.Storage.Abstractions;

/// <summary>Logical identity only. Possession never grants access. Resolve in an authorized local profile.</summary>
public sealed record LocalFileRef
{
    public LocalFileRef(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("Empty reference.", nameof(value));
        Value = value;
    }
    public Guid Value { get; }
}
/// <summary>Managed temporary identity, never a physical locator or assertion of finality.</summary>
public sealed record LocalArtifactRef
{
    public LocalArtifactRef(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("Empty reference.", nameof(value));
        Value = value;
    }
    public Guid Value { get; }
}
/// <summary>Save As selection. Adapter must authorize it again at publication.</summary>
public sealed record LocalTargetRef
{
    public LocalTargetRef(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("Empty reference.", nameof(value));
        Value = value;
    }
    public Guid Value { get; }
}
public enum LocalReferenceStatus { Unknown, Ready, Missing, Moved, AccessRevoked, Changed }
/// <summary>Profile authorization and source-identity check. No path or write authority is returned.</summary>
public interface ILocalFileReferenceResolver
{
    ValueTask<LocalReferenceStatus> RevalidateAsync(LocalFileRef source, CancellationToken cancellationToken);
}
