using Pertamina.Oto.Contracts.Identifiers;
using Pertamina.Oto.Modules.Pdf.Contracts;
namespace Pertamina.Oto.Modules.Pdf.Domain;

public sealed class MergePlan
{
    private MergePlan(InputOccurrence[] orderedInputs, Guid targetId, CorrelationId correlationId)
    {
        OrderedInputs = Array.AsReadOnly(orderedInputs);
        TargetId = targetId;
        CorrelationId = correlationId;
    }
    public IReadOnlyList<InputOccurrence> OrderedInputs { get; }
    public Guid TargetId { get; }
    public CorrelationId CorrelationId { get; }

    // FileId may repeat intentionally; each occurrence has its own identity.
    // The order is the selected working set, so removing an input never deletes its source.
    public static MergePlan? TryCreate(MergeCommand command, CorrelationId correlationId)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.TargetId == Guid.Empty || !correlationId.IsValid ||
            command.OrderedOccurrenceIds.Count < 2) return null;
        var inputs = new Dictionary<Guid, InputOccurrence>();
        foreach (var input in command.Inputs)
        {
            if (input is null || input.OccurrenceId == Guid.Empty || input.FileId == Guid.Empty ||
                !inputs.TryAdd(input.OccurrenceId, input)) return null;
        }
        var seen = new HashSet<Guid>();
        var ordered = new List<InputOccurrence>();
        foreach (var id in command.OrderedOccurrenceIds)
        {
            if (!seen.Add(id) || !inputs.TryGetValue(id, out var input)) return null;
            ordered.Add(input);
        }
        if (command.Inputs.Any(input => input.FileId == command.TargetId)) return null;
        return new(ordered.ToArray(), command.TargetId, correlationId);
    }
}
