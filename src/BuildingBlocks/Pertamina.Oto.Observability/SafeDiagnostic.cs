using System.Collections.ObjectModel;
using Pertamina.Oto.Contracts.Identifiers;

namespace Pertamina.Oto.Observability;

// Internal diagnostics, not the enterprise Audit Event Catalog.
public enum DiagnosticCode { OperationAccepted = 1, OperationCompleted = 2, OperationCancelled = 3, OperationFailed = 4, AccessDenied = 5 }

/// <summary>
/// Closed schema with no free text, exception, provider, path, content or arbitrary metadata.
/// Sinks must preserve this boundary. Version tokens are not accepted as diagnostic input.
/// </summary>
public sealed class SafeDiagnostic
{
    public SafeDiagnostic(CorrelationId correlationId, DiagnosticCode code)
    {
        if (!correlationId.IsValid)
        {
            throw new ArgumentException("A valid correlation identifier is required.", nameof(correlationId));
        }

        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(nameof(code));
        }

        CorrelationId = correlationId;
        Code = code;
    }

    public CorrelationId CorrelationId { get; }
    public DiagnosticCode Code { get; }
    public IReadOnlyDictionary<string, string> ToFields() => new ReadOnlyDictionary<string, string>(
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["schemaVersion"] = "1",
            ["correlationId"] = CorrelationId.ToString(),
            ["code"] = Code.ToString(),
        });
}

public interface ISafeDiagnosticSink
{
    void Write(SafeDiagnostic diagnostic);
}
