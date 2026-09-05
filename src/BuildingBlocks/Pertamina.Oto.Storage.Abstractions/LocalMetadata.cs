using Pertamina.Oto.Contracts.Identifiers;

namespace Pertamina.Oto.Storage.Abstractions;

public enum LocalHistoryStatus { Unknown, Completed, Cancelled, Failed }
/// <summary>Safe metadata projection. Display names/locations require separately authorized local resolution.
/// No document content, passwords, raw paths, or provider text fields.</summary>
public sealed record LocalHistoryEntry(Guid EntryId, Guid WorkspaceId, Guid CorrelationId, FeatureId Feature,
    LocalFileRef? Result, LocalHistoryStatus Status, DateTimeOffset RecordedAt, long SizeBytes, TimeSpan Duration);
/// <summary>Draft is not final output. Revision/cursor/snapshot commit atomically.</summary>
public sealed record LocalWorkspaceSnapshot(Guid WorkspaceId, long Revision,
    LocalArtifactRef Draft, long OperationCursor, DateTimeOffset SavedAt);
public sealed record LocalHistoryPage(IReadOnlyList<LocalHistoryEntry> Entries, Guid? NextCursor);
public static class LocalMetadataPolicy
{
    public static void ValidatePageSize(int pageSize)
    {
        if (pageSize is < 1 or > 50) throw new ArgumentOutOfRangeException(nameof(pageSize));
    }
}
/// <summary>Authorized local profile persistence only; never implicit sync/upload.
/// History removal only removes metadata, never result/source files. Reads must enforce 1..50 page size.
/// Snapshot compare-and-swap returns false on conflict, preserving prior validated draft.
/// Retention/cleanup is separate; no SQL schema or provider is prescribed by this port.</summary>
public interface ILocalWorkspaceMetadata
{
    ValueTask<bool> TrySaveSnapshotAsync(LocalWorkspaceSnapshot snapshot, long expectedRevision, CancellationToken cancellationToken);
    ValueTask AppendHistoryAsync(LocalHistoryEntry entry, CancellationToken cancellationToken);
    ValueTask<LocalHistoryPage> ReadHistoryAsync(int pageSize, Guid? cursor, CancellationToken cancellationToken);
    ValueTask RemoveHistoryEntryAsync(Guid entryId, CancellationToken cancellationToken);
}
